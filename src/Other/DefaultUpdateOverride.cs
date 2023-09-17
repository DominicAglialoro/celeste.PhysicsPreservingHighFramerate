using Monocle;

namespace Celeste.Mod.PhysicsPreservingHighFramerate.Other; 

public class DefaultUpdateOverride : IUpdateOverride {
    public Entity Entity { get; }
    
    public DefaultUpdateOverride(Entity entity) => Entity = entity;

    public void FixedUpdate() => Entity.Update();

    public void SmoothUpdate() { }
}