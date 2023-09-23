using Microsoft.Xna.Framework;
using MonoMod.Utils;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public static class LevelExtensions {
    public static void Load() => On.Celeste.Level.Update += Level_Update;

    public static void Unload() => On.Celeste.Level.Update -= Level_Update;

    public static void SmoothUpdate(this Level level) {
        var dynamicData = DynamicData.For(level);
        
        if (dynamicData.Get<float>("unpauseTimer") > 0f || level.Overlay != null || level.SkippingCutscene)
            return;

        foreach (var component in level.Tracker.GetComponents<Interpolation>()) {
            var entity = component.Entity;
            
            if (!level.FrozenOrPaused && !level.Transitioning
                || level.FrozenOrPaused && entity.TagCheck(Tags.PauseUpdate)
                || level.Frozen && entity.TagCheck(Tags.FrozenUpdate)
                || level.Transitioning && entity.TagCheck(Tags.TransitionUpdate))
                ((Interpolation) component).SmoothUpdate();
        }

        if (!level.FrozenOrPaused) {
            var camera = level.Camera;
            var previousCameraPosition = dynamicData.Get<Vector2?>("previousCameraPosition") ?? level.Camera.Position;
            var cameraPosition = dynamicData.Get<Vector2?>("cameraPosition") ?? level.Camera.Position;

            camera.Position = Vector2.Lerp(previousCameraPosition, cameraPosition, EngineExtensions.TimeInterp);
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

    private static void Level_Update(On.Celeste.Level.orig_Update update, Level level) {
        var dynamicData = DynamicData.For(level);
        
        if (PhysicsPreservingHighFramerateModule.Settings.Enabled) {
            foreach (var component in level.Tracker.GetComponents<Interpolation>())
                ((Interpolation) component).BeforeUpdate();
            
            level.Camera.Position = dynamicData.Get<Vector2?>("cameraPosition") ?? level.Camera.Position;
            dynamicData.Set("previousCameraPosition", level.Camera.Position);

            bool updateHair = dynamicData.Get<bool>("updateHair");

            dynamicData.Set("updateHair", false);
            update(level);
            dynamicData.Set("updateHair", updateHair);
        }
        else
            update(level);
        
        dynamicData.Set("cameraPosition", level.Camera.Position);

        foreach (var component in level.Tracker.GetComponents<Interpolation>())
            ((Interpolation) component).AfterUpdate();
    }
}