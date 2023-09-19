using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public static class EntityExtensions {
    public static Vector2 Extrapolate(this Entity entity, Vector2 position, Vector2 speed, float time, float maxDistance = float.MaxValue) {
        var delta = speed * time;

        if (delta.Length() > maxDistance)
            delta = maxDistance * delta.SafeNormalize();
        
        int deltaX = (int) delta.X;
        int deltaY = (int) delta.Y;
        int signDeltaX = Math.Sign(deltaX);
        int signDeltaY = Math.Sign(deltaY);

        while (deltaX != 0) {
            var newPosition = position + signDeltaX * Vector2.UnitX;

            if (entity.CollideCheck<Solid>(newPosition))
                break;

            position = newPosition;
            deltaX -= signDeltaX;
        }
        
        while (deltaY != 0) {
            var newPosition = position + signDeltaY * Vector2.UnitY;

            if (entity.CollideCheck<Solid>(newPosition))
                break;

            position = newPosition;
            deltaY -= signDeltaY;
        }

        return position;
    }
}