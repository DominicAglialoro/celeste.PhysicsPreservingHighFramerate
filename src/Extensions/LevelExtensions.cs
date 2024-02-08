using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public static class LevelExtensions {
    private const float ONE_OVER_SIXTY = (float) (166667L * 1E-07);
    
    public static void Load() {
        On.Celeste.Level.ctor += Level_ctor;
        On.Celeste.Level.Update += Level_Update;
        IL.Celeste.Level.Update += Level_Update_Il;
        On.Celeste.Level.BeforeRender += Level_BeforeRender;
        On.Celeste.Level.AfterRender += Level_AfterRender;
    }

    public static void Unload() {
        On.Celeste.Level.ctor -= Level_ctor;
        On.Celeste.Level.Update -= Level_Update;
        IL.Celeste.Level.Update -= Level_Update_Il;
        On.Celeste.Level.BeforeRender -= Level_BeforeRender;
        On.Celeste.Level.AfterRender -= Level_AfterRender;
    }

    private static bool ShouldInterpolate(Level level)
        => PhysicsPreservingHighFramerateModule.Settings.Enabled && !level.FrozenOrPaused && !level.Transitioning;

    private static void OverrideBaseUpdate(Level level) {
        var levelData = DynamicData.For(level);
        var engineData = DynamicData.For(Engine.Instance);
        float timeRate = PhysicsPreservingHighFramerateModule.Settings.GetGameSpeed() / 10f;
        float rawDeltaTime = Engine.RawDeltaTime;
        float deltaTime = timeRate * Engine.DeltaTime;
        
        engineData.Set("DeltaTime", deltaTime);

        foreach (var entity in level[Tags.HUD])
            entity.Update();

        float timeAccumulator = levelData.Get<float>("timeAccumulator");

        timeAccumulator += timeRate * rawDeltaTime;

        if (timeAccumulator > ONE_OVER_SIXTY) {
            engineData.Set("RawDeltaTime", ONE_OVER_SIXTY);
            engineData.Set("DeltaTime", ONE_OVER_SIXTY * Engine.TimeRate * Engine.TimeRateB * engineData.Invoke<float>("GetTimeRateComponentMultiplier", level));
            
            while (timeAccumulator > ONE_OVER_SIXTY) {
                foreach (var component in level.Tracker.GetComponents<Interpolation>())
                    ((Interpolation) component).Record();
                
                levelData.Get<CameraInterpolation>("cameraInterpolation").Record(level.Camera);
                
                foreach (var entity in level.Entities) {
                    if ((entity.Tag & Tags.HUD) == 0)
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

    private static void BeforeHairUpdate(Level level) {
        if (!ShouldInterpolate(level))
            return;
        
        float timeAccumulator = DynamicData.For(level).Get<float>("timeAccumulator");
        
        level.Tracker.GetEntity<Player>()?.Components.Get<Interpolation>()?.Interpolate(MathHelper.Clamp(timeAccumulator * 60f, 0f, 1f));
    }

    private static void AfterHairUpdate(Level level) {
        if (ShouldInterpolate(level))
            level.Tracker.GetEntity<Player>()?.Components.Get<Interpolation>()?.Restore();
    }

    private static void Level_ctor(On.Celeste.Level.orig_ctor ctor, Level level) {
        ctor(level);
        
        var levelData = DynamicData.For(level);
        
        levelData.Set("cameraInterpolation", new CameraInterpolation());
        levelData.Set("timeAccumulator", 0f);
    }

    private static void Level_Update(On.Celeste.Level.orig_Update update, Level level) {
        update(level);
        
        if (ShouldInterpolate(level))
            return;
        
        foreach (var component in level.Tracker.GetComponents<Interpolation>())
            ((Interpolation) component).Reset();
        
        DynamicData.For(level).Get<CameraInterpolation>("cameraInterpolation").Reset();
    }

    private static void Level_Update_Il(ILContext il) {
        var cursor = new ILCursor(il);

        cursor.GotoNext(
            instr => instr.OpCode == OpCodes.Ldarg_0,
            instr => instr.MatchCall<Scene>("Update"));

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Call, typeof(LevelExtensions).GetMethodUnconstrained(nameof(ShouldInterpolate)));

        var elseLabel = cursor.DefineLabel();

        cursor.Emit(OpCodes.Brtrue_S, elseLabel);

        cursor.GotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Br);

        var endLabel = (ILLabel) cursor.Prev.Operand;

        cursor.MarkLabel(elseLabel);
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Call, typeof(LevelExtensions).GetMethodUnconstrained(nameof(OverrideBaseUpdate)));
        cursor.Emit(OpCodes.Br, endLabel);

        cursor.GotoNext(
            instr => instr.OpCode == OpCodes.Ldloc_S,
            instr => instr.OpCode == OpCodes.Isinst,
            instr => instr.MatchCallvirt<PlayerHair>("AfterUpdate"));
        
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Call, typeof(LevelExtensions).GetMethodUnconstrained(nameof(BeforeHairUpdate)));
        cursor.Index += 3;
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Call, typeof(LevelExtensions).GetMethodUnconstrained(nameof(AfterHairUpdate)));
    }

    private static void Level_BeforeRender(On.Celeste.Level.orig_BeforeRender beforeRender, Level level) {
        if (ShouldInterpolate(level)) {
            var levelData = DynamicData.For(level);
            float timeAccumulator = levelData.Get<float>("timeAccumulator");
            float t = MathHelper.Clamp(timeAccumulator * 60f, 0f, 1f);

            foreach (var component in level.Tracker.GetComponents<Interpolation>())
                ((Interpolation) component).Interpolate(t);

            levelData.Get<CameraInterpolation>("cameraInterpolation").Interpolate(level.Camera, t);
        }
        
        beforeRender(level);
    }

    private static void Level_AfterRender(On.Celeste.Level.orig_AfterRender afterRender, Level level) {
        afterRender(level);

        if (!ShouldInterpolate(level))
            return;
        
        foreach (var component in level.Tracker.GetComponents<Interpolation>())
            ((Interpolation) component).Restore();
        
        DynamicData.For(level).Get<CameraInterpolation>("cameraInterpolation").Restore(level.Camera);
    }
}