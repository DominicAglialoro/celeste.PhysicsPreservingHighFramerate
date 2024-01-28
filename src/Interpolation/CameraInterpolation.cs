using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public class CameraInterpolation {
    private Vector2 previousPosition;
    private Vector2 position;
    private bool recorded;
    
    public void Record(Camera camera) {
        recorded = true;
        previousPosition = camera.Position;
    }

    public void Interpolate(Camera camera, float t) {
        position = camera.Position;
        
        if (recorded)
            camera.Position = Vector2.Lerp(previousPosition, position, t);
    }

    public void Restore(Camera camera) => camera.Position = position;

    public void Reset() => recorded = false;
}