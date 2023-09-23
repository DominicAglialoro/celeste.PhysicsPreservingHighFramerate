using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

[Tracked(true)]
public class Interpolation : Component {
    private Vector2 startPosition;
    private Vector2 endPosition;
    private bool stored;
    
    public Interpolation() : base(true, false) { }

    public void BeforeUpdate() {
        if (stored)
            Entity.Position = endPosition;
    }

    public void AfterUpdate() {
        if (!Entity.Active || !Entity.Visible) {
            stored = false;
            
            return;
        }
        
        startPosition = stored ? endPosition : Entity.Position;
        endPosition = Entity.Position;
        stored = true;
    }

    public void SmoothUpdate() {
        if (stored)
            Entity.Position = Vector2.Lerp(startPosition, endPosition, EngineExtensions.TimeInterp);
    }
}