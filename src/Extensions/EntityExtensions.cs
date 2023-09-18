using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public static class EntityExtensions {
    private static Dictionary<Type, EntityMethods> entityMethodsByType = new();
    
    public static void AddEntityMethods(this Type type, EntityMethods entityMethods)
        => entityMethodsByType.Add(type, entityMethods);

    public static void RemoveEntityMethods(this Type type) => entityMethodsByType.Remove(type);

    public static void SmoothUpdate(this Entity entity) {
        if (entityMethodsByType.TryGetValue(entity.GetType(), out var entityMethods))
            entityMethods.SmoothUpdate(entity);
    }
    
    public static Vector2 GetRenderPosition(this Entity entity)
        => entityMethodsByType.TryGetValue(entity.GetType(), out var entityMethods) ? entityMethods.GetRenderPosition(entity) : entity.Position;

    public static Vector2 Project(this Entity entity, Vector2 speed, float time) {
        var delta = speed * time;
        int deltaX = (int) delta.X;
        int deltaY = (int) delta.Y;
        int signDeltaX = Math.Sign(deltaX);
        int signDeltaY = Math.Sign(deltaY);
        var position = entity.Position;

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