using System;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public static class PlayerExtensions {
    public static void Load() {
        typeof(Player).AddEntityMethods(new PlayerEntityMethods());
        On.Celeste.Player.Update += Player_Update;
    }

    public static void Unload() {
        typeof(Player).RemoveEntityMethods();
        On.Celeste.Player.Update -= Player_Update;
    }

    private static void Player_Update(On.Celeste.Player.orig_Update update, Player player) {
        if (!PhysicsPreservingHighFramerateModule.Settings.Enabled) {
            update(player);
            
            return;
        }
        
        var camera = ((Level) player.Scene).Camera;
        var cameraPosition = camera.Position;

        update(player);
        camera.Position = cameraPosition;
    }

    private class PlayerEntityMethods : EntityMethods {
        public override void SmoothUpdate(Entity entity) {
            var player = (Player) entity;

            DynamicData.For(player).Set("renderPosition", player.Project(player.Speed, EngineExtensions.TimeDifference));

            if (!player.InControl && !player.ForceCameraUpdate)
                return;

            var camera = ((Level) player.Scene).Camera;

            if (player.StateMachine.State == 18)
                camera.Position = player.CameraTarget;
            else {
                var cameraPosition = camera.Position;
                float factor = player.StateMachine.State == 20 ? 8f : 1f;

                camera.Position += (player.CameraTarget - cameraPosition) * (1f - (float) Math.Pow(0.01f / factor, Engine.DeltaTime));
            }
        }

        public override Vector2 GetRenderPosition(Entity entity) {
            if (!((Level) entity.Scene).InMotion())
                return entity.Position;
            
            return DynamicData.For(entity).Get<Vector2?>("renderPosition") ?? entity.Position;
        }
    }
}