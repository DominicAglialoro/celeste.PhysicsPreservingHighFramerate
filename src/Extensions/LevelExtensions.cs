using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public static class LevelExtensions {
    public static void Load() {
        On.Celeste.Level.Update += Level_Update;
        IL.Celeste.Level.Update += Level_Update_IL;
        On.Celeste.Level.LoadLevel += Level_LoadLevel;
    }

    public static void Unload() {
        On.Celeste.Level.Update -= Level_Update;
        IL.Celeste.Level.Update -= Level_Update_IL;
    }

    public static void SmoothUpdate(this Level level) {
        var dynamicData = DynamicData.For(level);

        if (dynamicData.Get<float>("unpauseTimer") <= 0f && level.Overlay == null && !level.SkippingCutscene) {
            foreach (var component in level.Tracker.GetComponents<Interpolation>())
                ((Interpolation) component).Interpolate();

            if (dynamicData.Get<bool?>("doCameraInterpolate") is true) {
                var camera = level.Camera;

                camera.Position = Vector2.Lerp(
                    dynamicData.Get<Vector2?>("startCameraPosition") ?? camera.Position,
                    dynamicData.Get<Vector2?>("endCameraPosition") ?? camera.Position,
                    EngineExtensions.TimeInterp);
            }

            if (dynamicData.Get<bool>("updateHair")) {
                foreach (var component in level.Tracker.GetComponents<PlayerHair>()) {
                    if (component.Active && component.Entity.Active)
                        ((PlayerHair) component).AfterUpdate();
                }

                if (level.FrozenOrPaused)
                    dynamicData.Set("updateHair", false);
            }
            else if (!level.FrozenOrPaused)
                dynamicData.Set("updateHair", true);
        }
        
        if (!level.FrozenOrPaused)
            DynamicData.For(level.RendererList).Invoke("Update");
    }

    private static void BeforeInterpolationUpdate(this Level level) {
        var dynamicData = DynamicData.For(level);
        
        if (!dynamicData.Get<bool?>("doCameraInterpolate") is not true)
            return;
        
        var camera = level.Camera;

        camera.Position = dynamicData.Get<Vector2?>("endCameraPosition") ?? camera.Position;
        dynamicData.Set("startCameraPosition", camera.Position);
    }

    private static void AfterInterpolationUpdate(this Level level) {
        var dynamicData = DynamicData.For(level);
        var camera = level.Camera;
        
        if (!dynamicData.Get<bool?>("doCameraInterpolate") is not true)
            dynamicData.Set("startCameraPosition", camera.Position);
        
        dynamicData.Set("endCameraPosition", camera.Position);
        dynamicData.Set("doCameraInterpolate", true);
    }

    private static void ResetInterpolation(this Level level) => DynamicData.For(level).Set("doCameraInterpolate", false);

    private static void UpdateRenderersIfModDisabled(RendererList rendererList) {
        if (!PhysicsPreservingHighFramerateModule.Settings.Enabled)
            DynamicData.For(rendererList).Invoke("Update");
    }

    private static void OverrideBaseUpdate(Level level) {
        if (level.Paused)
            return;
        
        DynamicData.For(level.Entities).Invoke("Update");

        if (!PhysicsPreservingHighFramerateModule.Settings.Enabled)
            DynamicData.For(level.RendererList).Invoke("Update");
    }

    private static void Level_Update(On.Celeste.Level.orig_Update update, Level level) {
        var dynamicData = DynamicData.For(level);

        if (!PhysicsPreservingHighFramerateModule.Settings.Enabled) {
            foreach (var component in level.Tracker.GetComponents<Interpolation>())
                ((Interpolation) component).Reset();

            level.ResetInterpolation();
            update(level);

            return;
        }

        foreach (var component in level.Tracker.GetComponents<Interpolation>())
            ((Interpolation) component).BeforeUpdate();

        level.BeforeInterpolationUpdate();

        bool updateHair = dynamicData.Get<bool>("updateHair");

        dynamicData.Set("updateHair", false);
        update(level);
        dynamicData.Set("updateHair", updateHair);

        foreach (var component in level.Tracker.GetComponents<Interpolation>()) {
            var entity = component.Entity;
            
            if (entity.Active && entity.Visible && (!level.FrozenOrPaused && !level.Transitioning
                || level.FrozenOrPaused && entity.TagCheck(Tags.PauseUpdate)
                || level.Frozen && entity.TagCheck(Tags.FrozenUpdate)
                || level.Transitioning && entity.TagCheck(Tags.TransitionUpdate)))
                ((Interpolation) component).AfterUpdate();
            else
                ((Interpolation) component).Reset();
        }
        
        level.AfterInterpolationUpdate();
    }

    private static void Level_Update_IL(ILContext il) {
        var cursor = new ILCursor(il);

        while (cursor.TryGotoNext(instr => instr.MatchCallvirt<RendererList>("Update"))) {
            cursor.Remove();
            cursor.Emit(OpCodes.Call, typeof(LevelExtensions).GetMethodUnconstrained(nameof(UpdateRenderersIfModDisabled)));
        }

        cursor.Index = 0;
        cursor.GotoNext(instr => instr.MatchCall<Scene>("Update"));

        cursor.Remove();
        cursor.Emit(OpCodes.Call, typeof(LevelExtensions).GetMethodUnconstrained(nameof(OverrideBaseUpdate)));
    }

    private static void Level_LoadLevel(On.Celeste.Level.orig_LoadLevel loadLevel, Level level, Player.IntroTypes playerintro, bool isfromloader) {
        level.ResetInterpolation();
        loadLevel(level, playerintro, isfromloader);
    }
}