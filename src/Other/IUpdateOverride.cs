using Monocle;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public interface IUpdateOverride {
    void Update(Entity entity);
    void FixedUpdate(Entity entity);
}