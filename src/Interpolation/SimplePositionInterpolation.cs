using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public class SimplePositionInterpolation : Interpolation {
    private Vector2 position;
    private GetSmoothPosition getSmoothPosition;

    public SimplePositionInterpolation(GetSmoothPosition getSmoothPosition) => this.getSmoothPosition = getSmoothPosition;

    protected override void DoStore() => position = Entity.Position;

    protected override void DoRestore() => Entity.Position = position;

    protected override void DoSmoothUpdate() => Entity.Position = getSmoothPosition(Entity, position);

    public delegate Vector2 GetSmoothPosition(Entity entity, Vector2 position);
}