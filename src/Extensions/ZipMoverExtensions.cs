using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public static class ZipMoverExtensions {
    public static void Load() => On.Celeste.ZipMover.Added += ZipMover_Added;

    public static void Unload() => On.Celeste.ZipMover.Added -= ZipMover_Added;

    private static void ZipMover_Added(On.Celeste.ZipMover.orig_Added added, ZipMover zipMover, Scene scene) {
        added(zipMover, scene);
        zipMover.Add(new ZipMoverInterpolation());
    }

    private class ZipMoverInterpolation : Interpolation {
        private Vector2 position;
        private float percent;
        private DynamicData zipMoverDynamicData;
        private DynamicData enumeratorDynamicData;

        public override void Added(Entity entity) {
            base.Added(entity);
            zipMoverDynamicData = DynamicData.For(entity);
            enumeratorDynamicData = DynamicData.For(entity.Get<Coroutine>().Current);
        }

        protected override void DoStore() {
            position = Entity.Position;
            percent = zipMoverDynamicData.Get<float>("percent");
        }

        protected override void DoRestore() {
            Entity.Position = position;
            zipMoverDynamicData.Set("percent", percent);
        }

        protected override void DoSmoothUpdate() {
            int state = enumeratorDynamicData.Get<int>("<>1__state");

            if (state != 3 && state != 5) {
                zipMoverDynamicData.Set("percent", percent);
                Entity.Position = position;
                
                return;
            }
        
            var start = enumeratorDynamicData.Get<Vector2>("<start>5__2");
            var target = zipMoverDynamicData.Get<Vector2>("target");
            float at = enumeratorDynamicData.Get<float>("<at>5__3");
            float newPercent;

            if (state == 3)
                newPercent = Ease.SineIn(Calc.Approach(at, 1f, 2f * EngineExtensions.TimeDifference));
            else
                newPercent = 1f - Ease.SineIn(Calc.Approach(at, 1f, 0.5f * EngineExtensions.TimeDifference));

            zipMoverDynamicData.Set("percent", newPercent);
            Entity.Position = Vector2.Lerp(start, target, newPercent);
        }
    }
}