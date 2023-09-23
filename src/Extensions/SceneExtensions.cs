using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public static class SceneExtensions {
    public static void Load() {
        On.Monocle.Scene.Begin += Scene_Begin;
        IL.Monocle.Scene.Update += Scene_Update_IL;
    }

    public static void Unload() {
        On.Monocle.Scene.Begin -= Scene_Begin;
        IL.Monocle.Scene.Update -= Scene_Update_IL;
    }

    public static void SmoothUpdate(this Scene scene) {
        if (!scene.Paused)
            DynamicData.For(scene.RendererList).Invoke("Update");
    }

    private static void Scene_Begin(On.Monocle.Scene.orig_Begin begin, Scene scene) {
        Celeste.Instance.SetFramerate(PhysicsPreservingHighFramerateModule.Settings.GetFramerate());
        begin(scene);
    }

    private static void Scene_Update_IL(ILContext il) {
        var cursor = new ILCursor(il);

        cursor.GotoNext(instr => instr.MatchCallvirt<RendererList>("Update"));

        cursor.Remove();
        cursor.Emit(OpCodes.Pop);
    }
}