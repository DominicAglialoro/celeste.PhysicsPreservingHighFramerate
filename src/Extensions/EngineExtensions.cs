using System;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public static class EngineExtensions {
    private static readonly TimeSpan FIXED_ELAPSED_TIME = TimeSpan.FromTicks(166667L);
    
    public static float RawSmoothDeltaTime { get; private set; }
    public static float SmoothDeltaTime { get; private set; }
    public static float TimeDifference => (float) (smoothTime - fixedTime).TotalSeconds;
    public static float TimeInterp => (float) (TimeDifference / FIXED_ELAPSED_TIME.TotalSeconds);
    
    private static TimeSpan smoothTime;
    private static TimeSpan fixedTime;

    public static void Load() => On.Monocle.Engine.Update += Engine_Update;

    public static void Unload() => On.Monocle.Engine.Update -= Engine_Update;

    public static void SetFramerate(this Engine engine, int frameRate)
        => engine.TargetElapsedTime = TimeSpan.FromTicks(166667L * 60L / frameRate);

    private static void Engine_Update(On.Monocle.Engine.orig_Update update, Engine engine, GameTime gameTime) {
        smoothTime = gameTime.TotalGameTime;
        
        while (fixedTime + FIXED_ELAPSED_TIME <= smoothTime) {
            fixedTime += FIXED_ELAPSED_TIME;
            update(engine, new GameTime(fixedTime, FIXED_ELAPSED_TIME, gameTime.IsRunningSlowly));
        }
        
        var dynamicData = DynamicData.For(engine);
        var scene = dynamicData.Get<Scene>("scene");
        
        RawSmoothDeltaTime = (float) gameTime.ElapsedGameTime.TotalSeconds;
        SmoothDeltaTime = RawSmoothDeltaTime * Engine.TimeRate * Engine.TimeRateB * dynamicData.Invoke<float>("GetTimeRateComponentMultiplier", scene);
        
        if (scene is Level level)
            level.SmoothUpdate();
        else
            scene?.SmoothUpdate();
    }
}