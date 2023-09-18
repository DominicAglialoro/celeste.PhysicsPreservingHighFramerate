namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public static class PlayerExtensions {
    public static void Load() {
        typeof(Player).AddUpdateOverride(new FixedUpdateOverride());
    }

    public static void Unload() {
        typeof(Player).RemoveUpdateOverride();
    }
}