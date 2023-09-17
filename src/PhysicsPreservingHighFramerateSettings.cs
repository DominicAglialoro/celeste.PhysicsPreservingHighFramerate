namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public class PhysicsPreservingHighFramerateSettings : EverestModuleSettings {
    private bool enabled;
    private int frameRate = 60;

    public bool Enabled {
        get => enabled;
        set {
            enabled = value;
            Celeste.Instance.SetFramerate(GetFramerate());
        }
    }

    [SettingRange(10, 999, true)]
    public int FrameRate {
        get => frameRate;
        set {
            frameRate = value;
            Celeste.Instance.SetFramerate(GetFramerate());
        }
    }

    public int GetFramerate() => enabled ? FrameRate : 60;
}