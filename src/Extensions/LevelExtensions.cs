using MonoMod.Utils;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public static class LevelExtensions {
    public static void SmoothUpdate(this Level level) {
        if (DynamicData.For(level).Get<float>("unpauseTimer") > 0f || level.Overlay != null || level.SkippingCutscene)
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
        }
        else if (level.RetryPlayerCorpse != null) {
            foreach (var entity in level[Tags.PauseUpdate]) {
                if (entity.Active)
                    entity.SmoothUpdate();
            }
        }
        else
            level.Entities.SmoothUpdate();
    }
}