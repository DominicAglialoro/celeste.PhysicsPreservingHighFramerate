using Monocle;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public static class SceneExtensions {
    public static void Load() => On.Monocle.Scene.Begin += Scene_Begin;

    public static void Unload() => On.Monocle.Scene.Begin -= Scene_Begin;

    public static void FixedUpdate(this Scene scene) {
        if (!scene.Paused)
            scene.Entities.FixedUpdate();
    }

    private static void Scene_Begin(On.Monocle.Scene.orig_Begin begin, Scene scene) {
        Celeste.Instance.SetFramerate(PhysicsPreservingHighFramerateModule.Settings.GetFramerate());
        begin(scene);
    }
}