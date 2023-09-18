using System;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public static class EntityListExtensions {
    public static void Load() => IL.Monocle.EntityList.Update += EntityList_Update_IL;

    public static void Unload() => IL.Monocle.EntityList.Update -= EntityList_Update_IL;

    public static void FixedUpdate(this EntityList entityList) {
        foreach (var entity in DynamicData.For(entityList).Get<List<Entity>>("entities"))
            entity.FixedUpdate();
    }

    private static void EntityList_Update_IL(ILContext il) {
        var cursor = new ILCursor(il);

        cursor.GotoNext(instr => instr.MatchCallvirt<Entity>("Update"));

        cursor.Remove();
        cursor.Emit(OpCodes.Call, typeof(EntityExtensions).GetMethodUnconstrained(nameof(EntityExtensions.OverrideUpdate)));
    }
}