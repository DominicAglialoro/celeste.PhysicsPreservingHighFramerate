using Monocle;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public class DefaultEntityMethods : EntityMethods {
    public void FixedUpdate(Entity entity) { }

    public void Update(Entity entity) => entity.Update();
}