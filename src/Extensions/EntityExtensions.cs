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
}