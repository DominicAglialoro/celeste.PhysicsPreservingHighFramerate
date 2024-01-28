using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

[Tracked(true)]
public class Interpolation : Component {
    private Vector2 previousPosition;
    private Vector2 position;
    private bool recorded;

    public Interpolation() : base(true, false) { }

    public override void EntityAdded(Scene scene) {
        base.EntityAdded(scene);
        Reset();
    }

    public void Record() {
        recorded = true;
        previousPosition = Entity.Position;
    }

    public void Interpolate(float t) {
        position = Entity.Position;
        
        if (recorded)
            Entity.Position = Vector2.Lerp(previousPosition, position, t);
    }

    public void Restore() => Entity.Position = position;

    public void Reset() => recorded = false;
}