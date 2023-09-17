using Monocle;

namespace Celeste.Mod.PhysicsPreservingHighFramerate.Other; 

public interface IUpdateOverride {
    Entity Entity { get; }
    
    void FixedUpdate();
    void SmoothUpdate();
}