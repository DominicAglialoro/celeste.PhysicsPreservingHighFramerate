using Monocle;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

[Tracked(true)]
public abstract class Interpolation : Component {
    private bool stored;
    
    protected Interpolation() : base(true, false) { }

    public void Store() {
        DoStore();
        stored = true;
    }

    public void Restore() {
        if (stored)
            DoRestore();

        stored = false;
    }

    public void SmoothUpdate() {
        if (stored)
            DoSmoothUpdate();
    }

    protected virtual void DoStore() { }

    protected virtual void DoRestore() { }

    protected virtual void DoSmoothUpdate() { }
}