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

            if (!level.FrozenOrPaused) {
                var camera = level.Camera;
                var startCameraPosition = dynamicData.Get<Vector2?>("startCameraPosition") ?? level.Camera.Position;
                var endCameraPosition = dynamicData.Get<Vector2?>("endCameraPosition") ?? level.Camera.Position;

                camera.Position = Vector2.Lerp(startCameraPosition, endCameraPosition, EngineExtensions.TimeInterp);
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

    private static void Level_Update(On.Celeste.Level.orig_Update update, Level level) {
        var dynamicData = DynamicData.For(level);

        if (!PhysicsPreservingHighFramerateModule.Settings.Enabled) {
            foreach (var component in level.Tracker.GetComponents<Interpolation>())
                ((Interpolation) component).Reset();

            update(level);

            return;
        }

        foreach (var component in level.Tracker.GetComponents<Interpolation>())
            ((Interpolation) component).BeforeUpdate();

        level.Camera.Position = dynamicData.Get<Vector2?>("endCameraPosition") ?? level.Camera.Position;
        dynamicData.Set("startCameraPosition", level.Camera.Position);

        bool updateHair = dynamicData.Get<bool>("updateHair");

        dynamicData.Set("updateHair", false);
        update(level);
        dynamicData.Set("updateHair", updateHair);
        dynamicData.Set("endCameraPosition", level.Camera.Position);

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
    }

    private static void Level_Update_IL(ILContext il) {
        var cursor = new ILCursor(il);

        while (cursor.TryGotoNext(instr => instr.MatchCallvirt<RendererList>("Update"))) {
            cursor.Remove();
            cursor.Emit(OpCodes.Pop);
        }
    }
}