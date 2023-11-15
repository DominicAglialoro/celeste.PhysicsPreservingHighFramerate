using Monocle;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public static class SceneExtensions {
    public static void Load() => On.Monocle.Scene.Begin += Scene_Begin;

    public static void Unload() => On.Monocle.Scene.Begin -= Scene_Begin;

    private static void Scene_Begin(On.Monocle.Scene.orig_Begin begin, Scene scene) {
        Celeste.Instance.SetFramerate(PhysicsPreservingHighFramerateModule.Settings.GetFramerate());
        EngineExtensions.SetGameSpeed(PhysicsPreservingHighFramerateModule.Settings.GetGameSpeed());
        begin(scene);
    }
}