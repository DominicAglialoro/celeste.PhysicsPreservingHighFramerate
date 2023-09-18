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

        if (level.FrozenOrPaused) {
            if (!level.Paused) {
                foreach (var entity in level[Tags.FrozenUpdate]) {
                    if (entity.Active)
                        entity.SmoothUpdate();
                }
            }

            foreach (var entity in level[Tags.PauseUpdate]) {
                if (entity.Active)
                    entity.SmoothUpdate();
            }
        }
        else if (level.Transitioning) {
            foreach (var entity in level[Tags.TransitionUpdate])
                entity.SmoothUpdate();

            var previousCameraPosition = dynamicData.Get<Vector2?>("previousCameraPosition") ?? level.Camera.Position;
            var cameraPosition = dynamicData.Get<Vector2?>("cameraPosition") ?? level.Camera.Position;

            level.Camera.Position = Vector2.Lerp(previousCameraPosition, cameraPosition, EngineExtensions.TimeInterp);
        }
        else if (level.RetryPlayerCorpse != null) {
            foreach (var entity in level[Tags.PauseUpdate]) {
                if (entity.Active)
                    entity.SmoothUpdate();
            }
        }
        else
            level.Entities.SmoothUpdate();

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

    public static bool InMotion(this Level level)
        => !level.FrozenOrPaused && !level.Transitioning && !level.SkippingCutscene && level.Overlay == null 
           && DynamicData.For(level).Get<float>("unpauseTimer") <= 0f;

    private static void Level_Update(On.Celeste.Level.orig_Update update, Level level) {
        if (!PhysicsPreservingHighFramerateModule.Settings.Enabled) {
            update(level);
            
            return;
        }
        
        var dynamicData = DynamicData.For(level);
        bool updateHair = dynamicData.Get<bool>("updateHair");
        
        dynamicData.Set("updateHair", false);
        dynamicData.Set("previousCameraPosition", level.Camera.Position);
        update(level);
        dynamicData.Set("updateHair", updateHair);
        dynamicData.Set("cameraPosition", level.Camera.Position);
    }
}