using System;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public static class EngineExtensions {
    private static readonly TimeSpan FIXED_ELAPSED_TIME = TimeSpan.FromTicks(166667L);

    public static float TimeInterp => (float) ((smoothTime - fixedTime).TotalSeconds / updateInterval.TotalSeconds);
    
    private static TimeSpan smoothTime;
    private static TimeSpan fixedTime;
    private static TimeSpan updateInterval;

    public static void Load() => On.Monocle.Engine.Update += Engine_Update;

    public static void Unload() => On.Monocle.Engine.Update -= Engine_Update;

    public static void SetGameSpeed(int gameSpeed) => updateInterval = new TimeSpan(FIXED_ELAPSED_TIME.Ticks * 10 / gameSpeed);

    public static void SetFramerate(this Engine engine, int frameRate)
        => engine.TargetElapsedTime = TimeSpan.FromTicks(166667L * 60L / frameRate);

    private static void Engine_Update(On.Monocle.Engine.orig_Update update, Engine engine, GameTime gameTime) {
        smoothTime = gameTime.TotalGameTime;
        
        var dynamicData = DynamicData.For(engine);
        var scene = dynamicData.Get<Scene>("scene");
        var settings = PhysicsPreservingHighFramerateModule.Settings;
        
        if (!settings.Enabled || scene is not Level level) {
            update(engine, gameTime);
            fixedTime = smoothTime;
            
            return;
        }

        while (fixedTime + updateInterval <= smoothTime) {
            fixedTime += updateInterval;
            update(engine, new GameTime(fixedTime, FIXED_ELAPSED_TIME, gameTime.IsRunningSlowly));
        }

        float rawSmoothDeltaTime = (float) gameTime.ElapsedGameTime.TotalSeconds;
        float smoothDeltaTime = rawSmoothDeltaTime * Engine.TimeRate * Engine.TimeRateB * dynamicData.Invoke<float>("GetTimeRateComponentMultiplier", scene);

        dynamicData.Set("RawDeltaTime", rawSmoothDeltaTime);
        dynamicData.Set("DeltaTime", smoothDeltaTime);
        level.SmoothUpdate();
    }
}