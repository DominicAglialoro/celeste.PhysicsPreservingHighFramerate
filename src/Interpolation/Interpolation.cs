using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

[Tracked(true)]
public class Interpolation : Component {
    private Vector2 startPosition;
    private Vector2 endPosition;
    private bool doInterpolate;
    
    public Interpolation() : base(true, false) { }

    public override void EntityAdded(Scene scene) => Reset();

    public void BeforeUpdate() {
        if (!doInterpolate)
            return;
        
        Entity.Position = endPosition;
        startPosition = Entity.Position;
    }

    public void AfterUpdate() {
        if (!doInterpolate)
            startPosition = Entity.Position;
        
        endPosition = Entity.Position;
        doInterpolate = true;
    }

    public void Reset() => doInterpolate = false;

    public void Interpolate() {
        if (doInterpolate)
            Entity.Position = Vector2.Lerp(startPosition, endPosition, EngineExtensions.TimeInterp);
    }
}