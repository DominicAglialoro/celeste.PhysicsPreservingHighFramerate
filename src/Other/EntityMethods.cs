using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public abstract class EntityMethods {
    public virtual void SmoothUpdate(Entity entity) { }

    public virtual Vector2 GetRenderPosition(Entity entity) => entity.Position;
}