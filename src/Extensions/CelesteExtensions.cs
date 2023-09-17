using System;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public static class CelesteExtensions {
    private static readonly TimeSpan FIXED_ELAPSED_TIME = TimeSpan.FromTicks(166667L);

    private static TimeSpan accumulatedTime = TimeSpan.Zero;
    
    public static float RawSmoothDeltaTime { get; private set; }
    
    public static float SmoothDeltaTime { get; private set; }

    public static void Load() {
        On.Celeste.Celeste.Update += Celeste_Update;
    }

    public static void Unload() {
        On.Celeste.Celeste.Update -= Celeste_Update;
    }

    public static void SetFramerate(this Celeste celeste, int frameRate)
        => celeste.TargetElapsedTime = TimeSpan.FromTicks(166667L * 60L / frameRate);

    private static void SmoothUpdate(this Celeste celeste, GameTime gameTime) {
        RawSmoothDeltaTime = (float) gameTime.ElapsedGameTime.TotalSeconds;
        SmoothDeltaTime = RawSmoothDeltaTime * Engine.TimeRate * Engine.TimeRateB;
        
        var dynamicData = DynamicData.For(celeste);
        var scene = dynamicData.Get<Scene>("scene");

        if (scene != null && !scene.Paused)
            scene.Entities.SmoothUpdate();
    }

    private static void Celeste_Update(On.Celeste.Celeste.orig_Update update, Celeste celeste, GameTime gameTime) {
        if (!PhysicsPreservingHighFramerateModule.Settings.Enabled) {
            update(celeste, gameTime);
            
            return;
        }
        
        accumulatedTime += gameTime.ElapsedGameTime;
        
        while (accumulatedTime >= FIXED_ELAPSED_TIME) {
            accumulatedTime -= FIXED_ELAPSED_TIME;
            update(celeste, new GameTime(gameTime.TotalGameTime, FIXED_ELAPSED_TIME, gameTime.IsRunningSlowly));
        }
        
        celeste.SmoothUpdate(gameTime);
    }
}