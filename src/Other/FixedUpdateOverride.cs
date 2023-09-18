using Monocle;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public class FixedUpdateOverride : IUpdateOverride {
    public void Update(Entity entity) { }

    public void FixedUpdate(Entity entity) => entity.Update();
}