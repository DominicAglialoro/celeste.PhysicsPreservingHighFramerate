﻿using System;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public class PhysicsPreservingHighFramerateModule : EverestModule {
    public static PhysicsPreservingHighFramerateModule Instance { get; private set; }

    public override Type SettingsType => typeof(PhysicsPreservingHighFramerateSettings);
    public static PhysicsPreservingHighFramerateSettings Settings => (PhysicsPreservingHighFramerateSettings) Instance._Settings;

    public PhysicsPreservingHighFramerateModule() {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(PhysicsPreservingHighFramerateModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(PhysicsPreservingHighFramerateModule), LogLevel.Info);
#endif
    }

    public override void Load() {
        LevelExtensions.Load();
        SceneExtensions.Load();
        VariousEntityExtensions.Load();
    }

    public override void Unload() {
        LevelExtensions.Unload();
        SceneExtensions.Unload();
        VariousEntityExtensions.Unload();
    }
}