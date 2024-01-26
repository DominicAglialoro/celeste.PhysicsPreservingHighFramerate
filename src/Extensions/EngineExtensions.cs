using System;
using Monocle;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public static class EngineExtensions {
    public static void SetFramerate(this Engine engine, int frameRate)
        => engine.TargetElapsedTime = TimeSpan.FromTicks(166667L * 60L / frameRate);
}