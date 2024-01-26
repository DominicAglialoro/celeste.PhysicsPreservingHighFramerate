using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public static class LevelExtensions {
    private const float ONE_OVER_SIXTY = (float) (166667L * 1E-07);
    
    public static void Load() => IL.Celeste.Level.Update += Level_Update_Il;

    public static void Unload() => IL.Celeste.Level.Update -= Level_Update_Il;

    private static bool ModEnabled() => PhysicsPreservingHighFramerateModule.Settings.Enabled;

    private static void OverrideBaseUpdate(Level level) {
        var levelData = DynamicData.For(level);
        var engineData = DynamicData.For(Engine.Instance);
        float timeRate = PhysicsPreservingHighFramerateModule.Settings.GetGameSpeed() / 10f;
        float rawDeltaTime = Engine.RawDeltaTime;
        float deltaTime = timeRate * Engine.DeltaTime;
        
        engineData.Set("DeltaTime", deltaTime);

        foreach (var entity in level.Entities) {
            if ((entity.Tag & (Tags.FrozenUpdate | Tags.PauseUpdate | Tags.TransitionUpdate | Tags.HUD)) != 0)
                entity.Update();
        }

        float timeAccumulator = levelData.Get<float?>("timeAccumulator") ?? 0f;

        timeAccumulator += timeRate * rawDeltaTime;

        if (timeAccumulator > ONE_OVER_SIXTY) {
            engineData.Set("RawDeltaTime", ONE_OVER_SIXTY);
            engineData.Set("DeltaTime", ONE_OVER_SIXTY * Engine.TimeRate * Engine.TimeRateB * engineData.Invoke<float>("GetTimeRateComponentMultiplier", level));
            
            while (timeAccumulator > ONE_OVER_SIXTY) {
                foreach (var entity in level.Entities) {
                    if ((entity.Tag & (Tags.FrozenUpdate | Tags.PauseUpdate | Tags.TransitionUpdate | Tags.HUD)) == 0)
                        entity.Update();
                }
                
                timeAccumulator -= ONE_OVER_SIXTY;
            }
            
            engineData.Set("RawDeltaTime", rawDeltaTime);
            engineData.Set("DeltaTime", deltaTime);
        }
        
        levelData.Set("timeAccumulator", timeAccumulator);

        DynamicData.For(level.RendererList).Invoke("Update");
    }

    private static void Level_Update_Il(ILContext il) {
        var cursor = new ILCursor(il);

        cursor.GotoNext(
            instr => instr.OpCode == OpCodes.Ldarg_0,
            instr => instr.MatchCall<Scene>("Update"));

        cursor.Emit(OpCodes.Call, typeof(LevelExtensions).GetMethodUnconstrained(nameof(ModEnabled)));

        var elseLabel = cursor.DefineLabel();

        cursor.Emit(OpCodes.Brtrue_S, elseLabel);

        cursor.GotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Br);

        var endLabel = (ILLabel) cursor.Prev.Operand;

        cursor.MarkLabel(elseLabel);
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Call, typeof(LevelExtensions).GetMethodUnconstrained(nameof(OverrideBaseUpdate)));
        cursor.Emit(OpCodes.Br, endLabel);
    }
}