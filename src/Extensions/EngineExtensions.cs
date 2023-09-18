using System;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public static class EngineExtensions {
    public static TimeSpan FixedElapsedTime { get; } = TimeSpan.FromTicks(166667L);
    public static TimeSpan Time { get; private set; }
    public static TimeSpan FixedTime { get; private set; }
    public static float TimeDifference => (float) (Time - FixedTime).TotalSeconds;
    public static float TimeInterp => (float) (TimeDifference / FixedElapsedTime.TotalSeconds);

    public static void Load() {
        On.Monocle.Engine.Update += Engine_Update;
        IL.Monocle.Engine.Update += Engine_Update_IL;
    }

    public static void Unload() {
        On.Monocle.Engine.Update -= Engine_Update;
        IL.Monocle.Engine.Update -= Engine_Update_IL;
    }

    public static void SetFramerate(this Engine engine, int frameRate)
        => engine.TargetElapsedTime = TimeSpan.FromTicks(166667L * 60L / frameRate);

    private static void DoFixedUpdates(Engine engine) {
        if (!PhysicsPreservingHighFramerateModule.Settings.Enabled || FixedTime + FixedElapsedTime > Time)
            return;
        
        var dynamicData = DynamicData.For(engine);
        var scene = dynamicData.Get<Scene>("scene");
        float rawDeltaTime = dynamicData.Get<float>("RawDeltaTime");
        float deltaTime = dynamicData.Get<float>("DeltaTime");
        float rawFixedDeltaTime = (float) FixedElapsedTime.TotalSeconds;
        float fixedDeltaTime = rawFixedDeltaTime * Engine.TimeRate * Engine.TimeRateB * dynamicData.Invoke<float>("GetTimeRateComponentMultiplier", scene);

        dynamicData.Set("RawDeltaTime", rawFixedDeltaTime);
        dynamicData.Set("DeltaTime", fixedDeltaTime);
        
        while (FixedTime + FixedElapsedTime <= Time) {
            FixedTime += FixedElapsedTime;
            
            if (scene is Level level)
                level.FixedUpdate();
            else
                scene.FixedUpdate();
        }
        
        dynamicData.Set("RawDeltaTime", rawDeltaTime);
        dynamicData.Set("DeltaTime", deltaTime);
    }

    private static void Engine_Update(On.Monocle.Engine.orig_Update update, Engine engine, GameTime gameTime) {
        Time = gameTime.TotalGameTime;
        update(engine, gameTime);
        
        while (FixedTime + FixedElapsedTime <= Time)
            FixedTime += FixedElapsedTime;
    }

    private static void Engine_Update_IL(ILContext il) {
        var cursor = new ILCursor(il);
        
        cursor.GotoNext(MoveType.After, instr => instr.MatchCallvirt<Scene>(nameof(Scene.BeforeUpdate)));

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Call, typeof(EngineExtensions).GetMethodUnconstrained(nameof(DoFixedUpdates)));
    }
}