using System;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public static class EngineExtensions {
    private static readonly TimeSpan FIXED_ELAPSED_TIME = TimeSpan.FromTicks(166667L);

    private static TimeSpan accumulatedTime = TimeSpan.Zero;
    private static float fixedDeltaTime;
    private static float rawFixedDeltaTime;

    public static void Load() {
        On.Monocle.Engine.Update += Engine_Update;
    }

    public static void Unload() {
        On.Monocle.Engine.Update -= Engine_Update;
    }

    public static void SetFramerate(this Engine engine, int frameRate)
        => engine.TargetElapsedTime = TimeSpan.FromTicks(166667L * 60L / frameRate);

    private static void FixedUpdate(this Engine engine, GameTime gameTime) {
        var dynamicData = DynamicData.For(engine);
        var scene = dynamicData.Get<Scene>("scene");
        
        rawFixedDeltaTime = (float) gameTime.ElapsedGameTime.TotalSeconds;
        fixedDeltaTime = rawFixedDeltaTime * Engine.TimeRate * Engine.TimeRateB * dynamicData.Invoke<float>("GetTimeRateComponentMultiplier", scene);
        
        dynamicData.Set("DeltaTime", fixedDeltaTime);
        dynamicData.Set("RawDeltaTime", rawFixedDeltaTime);

        if (Engine.FreezeTimer > 0f || scene == null)
            return;
        
        if (scene is Level level)
            level.FixedUpdate();
        else
            scene.FixedUpdate();
    }

    private static void Engine_Update(On.Monocle.Engine.orig_Update update, Engine engine, GameTime gameTime) {
        if (!PhysicsPreservingHighFramerateModule.Settings.Enabled) {
            update(engine, gameTime);
            
            return;
        }

        accumulatedTime += gameTime.ElapsedGameTime;
        
        while (accumulatedTime >= FIXED_ELAPSED_TIME) {
            long flooredTicks = gameTime.TotalGameTime.Ticks;

            flooredTicks -= flooredTicks % FIXED_ELAPSED_TIME.Ticks;
            accumulatedTime -= FIXED_ELAPSED_TIME;
            engine.FixedUpdate(new GameTime(TimeSpan.FromTicks(flooredTicks), FIXED_ELAPSED_TIME, gameTime.IsRunningSlowly));
        }
        
        update(engine, gameTime);
    }
}