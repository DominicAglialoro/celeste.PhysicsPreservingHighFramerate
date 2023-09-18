using System;
using System.Collections.Generic;
using Monocle;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public static class EntityExtensions {
    private static Dictionary<Type, IUpdateOverride> updateOverrides = new();
    
    public static void AddUpdateOverride(this Type type, IUpdateOverride updateOverride)
        => updateOverrides.Add(type, updateOverride);

    public static void RemoveUpdateOverride(this Type type) => updateOverrides.Remove(type);

    public static bool TryGetUpdateOverride(this Type type, out IUpdateOverride updateOverride)
        => updateOverrides.TryGetValue(type, out updateOverride);
    
    public static void OverrideUpdate(this Entity entity) {
        if (PhysicsPreservingHighFramerateModule.Settings.Enabled
            && entity.GetType().TryGetUpdateOverride(out var updateOverride))
            updateOverride.Update(entity);
        else
            entity.Update();
    }
    
    public static void FixedUpdate(this Entity entity) {
        if (entity.GetType().TryGetUpdateOverride(out var updateOverride))
            updateOverride.FixedUpdate(entity);
    }
}