using Monocle;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public class FixedEntityMethods : EntityMethods {
    public static FixedEntityMethods Instance { get; } = new();
    
    public void Update(Entity entity) { }

    public void FixedUpdate(Entity entity) => entity.Update();
}