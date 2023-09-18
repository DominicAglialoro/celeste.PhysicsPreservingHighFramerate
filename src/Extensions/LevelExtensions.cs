using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public static class LevelExtensions {
    public static void Load() {
        IL.Celeste.Level.Update += Level_Update_IL;
    }

    public static void Unload() {
        IL.Celeste.Level.Update -= Level_Update_IL;
    }

    public static void FixedUpdate(this Level level) {
        if (DynamicData.For(level).Get<float>("unpauseTimer") > 0f || level.Overlay != null || level.SkippingCutscene)
            return;

        if (level.FrozenOrPaused) {
            if (!level.Paused) {
                foreach (var entity in level[Tags.FrozenUpdate]) {
                    if (entity.Active)
                        entity.FixedUpdate();
                }
            }

            foreach (var entity in level[Tags.PauseUpdate]) {
                if (entity.Active)
                    entity.FixedUpdate();
            }
        }
        else if (level.Transitioning) {
            foreach (var entity in level[Tags.TransitionUpdate])
                entity.FixedUpdate();
        }
        else if (level.RetryPlayerCorpse != null) {
            foreach (var entity in level[Tags.PauseUpdate]) {
                if (entity.Active)
                    entity.FixedUpdate();
            }
        }
        else
            level.Entities.FixedUpdate();
    }

    private static void Level_Update_IL(ILContext il) {
        var cursor = new ILCursor(il);

        while (cursor.TryGotoNext(MoveType.After,
                   instr => instr.MatchLdfld<Entity>("Active"),
                   instr => instr.MatchBrfalse(out _),
                   instr => instr.MatchLdloc(out _),
                   instr => instr.MatchCallvirt<Entity>("Update"))) {
            cursor.Index--;
            cursor.Remove();
            cursor.Emit(OpCodes.Call, typeof(EntityExtensions).GetMethodUnconstrained(nameof(EntityExtensions.OverrideUpdate)));
        }

        cursor.Index = -1;
        cursor.GotoPrev(instr => instr.MatchCallvirt<Entity>("Update"));
        
        cursor.Remove();
        cursor.Emit(OpCodes.Call, typeof(EntityExtensions).GetMethodUnconstrained(nameof(EntityExtensions.OverrideUpdate)));
    }
}