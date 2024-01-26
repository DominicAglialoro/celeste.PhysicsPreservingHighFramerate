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

    [SettingRange(60, 999, true)]
    public int FrameRate {
        get => frameRate;
        set {
            frameRate = value;
            Celeste.Instance.SetFramerate(GetFramerate());
        }
    }

    [SettingRange(1, 50, false)]
    public int GameSpeed { get; set; } = 10;

    public int GetFramerate() => enabled ? FrameRate : 60;

    public int GetGameSpeed() => enabled ? GameSpeed : 10;
}