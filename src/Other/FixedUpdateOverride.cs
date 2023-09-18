using Monocle;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public class FixedUpdateOverride : IUpdateOverride {
    public static FixedUpdateOverride Instance { get; } = new();
    
    public void Update(Entity entity) { }

    public void FixedUpdate(Entity entity) => entity.Update();
}