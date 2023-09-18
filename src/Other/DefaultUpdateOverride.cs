using Monocle;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public class DefaultUpdateOverride : IUpdateOverride {
    public void FixedUpdate(Entity entity) { }

    public void Update(Entity entity) => entity.Update();
}