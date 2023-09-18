using System.Collections.Generic;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public static class EntityListExtensions {
    public static void SmoothUpdate(this EntityList entityList) {
        foreach (var entity in DynamicData.For(entityList).Get<List<Entity>>("entities")) {
            if (entity.Active)
                entity.SmoothUpdate();
        }
    }
}