using System;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public static class PlayerExtensions {
    public static void Load() => On.Celeste.Player.ctor += Player_ctor;

    public static void Unload() => On.Celeste.Player.ctor -= Player_ctor;

    private static void Player_ctor(On.Celeste.Player.orig_ctor ctor, Player player, Vector2 position, PlayerSpriteMode spritemode) {
        ctor(player, position, spritemode);
        player.Add(new PlayerInterpolation());
    }

    private class PlayerInterpolation : Interpolation {
        private Player player;
        private Vector2 position;

        public override void Added(Entity entity) {
            base.Added(entity);
            player = (Player) entity;
        }

        protected override void DoStore() => position = player.Position;

        protected override void DoRestore() => player.Position = position;

        protected override void DoSmoothUpdate() {
            var dynamicData = DynamicData.For(player);

            if (player.StateMachine.State != 22) {
                player.Position = player.Extrapolate(position, player.Speed, EngineExtensions.TimeDifference);

                return;
            }
            
            var difference = dynamicData.Get<Vector2>("attractTo") - player.ExactPosition;
            var speed = 200f * difference.SafeNormalize();

            player.Position = player.Extrapolate(position, speed, EngineExtensions.TimeDifference, difference.Length());
        }
    }
}