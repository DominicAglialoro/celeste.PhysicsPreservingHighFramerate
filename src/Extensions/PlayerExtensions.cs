using System;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public static class PlayerExtensions {
    public static void Load() {
        typeof(Player).AddEntityMethods(new PlayerEntityMethods());
    }

    public static void Unload() {
        typeof(Player).RemoveEntityMethods();
    }

    private class PlayerEntityMethods : EntityMethods {
        public override Vector2 GetRenderPosition(Entity entity) {
            var player = (Player) entity;

            return player.Position + player.Speed * EngineExtensions.TimeDifference;
        }
    }
}