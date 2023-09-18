using System;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public static class PlayerExtensions {
    public static void Load() {
        typeof(Player).AddUpdateOverride(new PlayerUpdateOverride());
    }

    public static void Unload() {
        typeof(Player).RemoveUpdateOverride();
    }

    private class PlayerUpdateOverride : IUpdateOverride {
        public void Update(Entity entity) {
            var player = (Player) entity;
            var dynamicData = DynamicData.For(player);
            var renderPosition = player.Position + player.Speed * EngineExtensions.TimeDifference;
            
            dynamicData.Set("renderPosition", renderPosition);

            if (!player.InControl && !player.ForceCameraUpdate)
                return;
            
            var camera = ((Level) player.Scene).Camera;

            if (player.StateMachine.State == 18)
                camera.Position = player.CameraTarget;
            else {
                float factor = player.StateMachine.State == 20 ? 8f : 1f;

                camera.Position += (player.CameraTarget - camera.Position) * (1f - (float) Math.Pow(0.01f / factor, Engine.DeltaTime));
            }
        }

        public void FixedUpdate(Entity entity) {
            var player = (Player) entity;
            var level = (Level) player.Scene;
            var cameraPosition = level.Camera.Position;
            
            player.Update();
            
            if (player.StateMachine.State != 21)
                level.Camera.Position = cameraPosition;
        }
    }
}