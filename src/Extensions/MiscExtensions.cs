using System;
using System.Collections.Generic;
using System.Reflection;
using Monocle;
using MonoMod.RuntimeDetour.HookGen;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public class MiscExtensions {
    private static readonly List<KeyValuePair<MethodBase, Delegate>> PATCHES = new(); 
    
    public static void Load() {
        
    }

    public static void Unload() {
        foreach (var pair in PATCHES)
            HookEndpointManager.Remove(pair.Key, pair.Value);
        
        PATCHES.Clear();
    }

    private static void PatchAdded<T>(SimplePositionInterpolation.GetSmoothPosition getSmoothPosition) where T : Entity {
        var methodInfo = typeof(T).GetMethodUnconstrained("Added");
        var addedDelegate = (T_Added<T>) ((added, entity, scene) => {
            added(entity, scene);
            entity.Add(new SimplePositionInterpolation(getSmoothPosition));
        });
        
        HookEndpointManager.Add<T_Added<T>>(methodInfo, addedDelegate);
        PATCHES.Add(new KeyValuePair<MethodBase, Delegate>(methodInfo, addedDelegate));
    }

    private delegate void T_Added<T>(Action<T, Scene> orig, T entity, Scene scene);
}