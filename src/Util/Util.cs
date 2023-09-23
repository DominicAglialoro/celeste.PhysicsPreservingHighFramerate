using System;
using System.Reflection;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public static class Util {
    public static MethodInfo GetMethodUnconstrained(this Type type, string name) => type.GetMethod(name,
        BindingFlags.Instance |
        BindingFlags.Static |
        BindingFlags.Public |
        BindingFlags.NonPublic);
    
    public static PropertyInfo GetPropertyUnconstrained(this Type type, string name) => type.GetProperty(name,
        BindingFlags.Instance |
        BindingFlags.Static |
        BindingFlags.Public |
        BindingFlags.NonPublic);
}