using System;
using System.Collections.Generic;
using Celeste.Mod.PhysicsPreservingHighFramerate.Other;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public static class EntityListExtensions {
    private static Dictionary<Type, Func<Entity, IUpdateOverride>> updateOverrideGenerators = new();

    public static void Load() {
        On.Monocle.EntityList.UpdateLists += EntityList_UpdateLists;
        On.Monocle.EntityList.Update += EntityList_Update;
    }

    public static void Unload() {
        On.Monocle.EntityList.UpdateLists -= EntityList_UpdateLists;
        On.Monocle.EntityList.Update -= EntityList_Update;
    }

    public static void AddUpdateOverride(this Type type, Func<Entity, IUpdateOverride> generator)
        => updateOverrideGenerators.Add(type, generator);

    public static void RemoveUpdateOverride(this Type type) => updateOverrideGenerators.Remove(type);

    public static void SmoothUpdate(this EntityList entityList) {
        foreach (var updateOverride in entityList.GetUpdateOverrides()) {
            if (updateOverride.Entity.Active)
                updateOverride.SmoothUpdate();
        }
    }

    private static List<IUpdateOverride> GetUpdateOverrides(this EntityList entityList) {
        var dynamicData = DynamicData.For(entityList);

        if (dynamicData.TryGet("updateOverrides", out List<IUpdateOverride> updateOverrides))
            return updateOverrides;
        
        updateOverrides = new List<IUpdateOverride>();
        dynamicData.Set("updateOverrides", updateOverrides);

        return updateOverrides;
    }

    private static void EntityList_UpdateLists(On.Monocle.EntityList.orig_UpdateLists updateLists, EntityList entityList) {
        updateLists(entityList);

        var updateOverrides = entityList.GetUpdateOverrides();
        
        updateOverrides.Clear();

        foreach (var entity in DynamicData.For(entityList).Get<List<Entity>>("entities")) {
            if (updateOverrideGenerators.TryGetValue(entity.GetType(), out var generator))
                updateOverrides.Add(generator(entity));
            else
                updateOverrides.Add(new DefaultUpdateOverride(entity));
        }
    }
    
    private static void EntityList_Update(On.Monocle.EntityList.orig_Update update, EntityList entityList) {
        if (!PhysicsPreservingHighFramerateModule.Settings.Enabled) {
            update(entityList);
            
            return;
        }

        foreach (var updateOverride in entityList.GetUpdateOverrides()) {
            var entity = updateOverride.Entity;
            var dynamicData = DynamicData.For(entity);

            dynamicData.Invoke("_PreUpdate");
            
            if (entity.Active)
                updateOverride.FixedUpdate();
            
            dynamicData.Invoke("_PostUpdate");
        }
    }
}