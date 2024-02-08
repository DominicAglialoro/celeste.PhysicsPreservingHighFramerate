using Microsoft.Xna.Framework;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public static class VariousEntityExtensions {
    public static void Load() {
        On.Celeste.Actor.ctor += Actor_ctor;
        On.Celeste.AngryOshiro.ctor_Vector2_bool += AngryOshiro_ctor_Vector2_bool;
        On.Celeste.BadelineOldsite.ctor_Vector2_int += BadelineOldsite_ctor_Vector2_int;
        On.Celeste.Booster.ctor_Vector2_bool += Booster_ctor_Vector2_bool;
        On.Celeste.FinalBoss.ctor_Vector2_Vector2Array_int_float_bool_bool_bool += FinalBoss_ctor_Vector2_Vector2Array_int_float_bool_bool_bool;
        On.Celeste.FinalBossShot.ctor += FinalBossShot_ctor;
        On.Celeste.Key.ctor_Vector2_EntityID_Vector2Array += Key_ctor_Vector2_EntityID_Vector2Array;
        On.Celeste.Platform.ctor += Platform_ctor;
        On.Celeste.RotateSpinner.ctor += RotateSpinner_ctor;
        On.Celeste.Snowball.ctor += Snowball_ctor;
        On.Celeste.TrackSpinner.ctor += TrackSpinner_ctor;
    }

    public static void Unload() {
        On.Celeste.Actor.ctor -= Actor_ctor;
        On.Celeste.AngryOshiro.ctor_Vector2_bool -= AngryOshiro_ctor_Vector2_bool;
        On.Celeste.BadelineOldsite.ctor_Vector2_int -= BadelineOldsite_ctor_Vector2_int;
        On.Celeste.Booster.ctor_Vector2_bool -= Booster_ctor_Vector2_bool;
        On.Celeste.FinalBoss.ctor_Vector2_Vector2Array_int_float_bool_bool_bool -= FinalBoss_ctor_Vector2_Vector2Array_int_float_bool_bool_bool;
        On.Celeste.FinalBossShot.ctor -= FinalBossShot_ctor;
        On.Celeste.Key.ctor_Vector2_EntityID_Vector2Array -= Key_ctor_Vector2_EntityID_Vector2Array;
        On.Celeste.Platform.ctor -= Platform_ctor;
        On.Celeste.RotateSpinner.ctor -= RotateSpinner_ctor;
        On.Celeste.Snowball.ctor -= Snowball_ctor;
        On.Celeste.TrackSpinner.ctor -= TrackSpinner_ctor;
    }

    private static void Actor_ctor(On.Celeste.Actor.orig_ctor orig, Actor self, Vector2 position) {
        orig(self, position);
        self.Add(new Interpolation());
    }

    private static void AngryOshiro_ctor_Vector2_bool(On.Celeste.AngryOshiro.orig_ctor_Vector2_bool orig, AngryOshiro self, Vector2 position, bool fromcutscene) {
        orig(self, position, fromcutscene);
        self.Add(new Interpolation());
    }

    private static void BadelineOldsite_ctor_Vector2_int(On.Celeste.BadelineOldsite.orig_ctor_Vector2_int orig, BadelineOldsite self, Vector2 position, int index) {
        orig(self, position, index);
        self.Add(new Interpolation());
    }

    private static void Booster_ctor_Vector2_bool(On.Celeste.Booster.orig_ctor_Vector2_bool orig, Booster self, Vector2 position, bool red) {
        orig(self, position, red);
        self.Add(new Interpolation());
    }

    private static void FinalBoss_ctor_Vector2_Vector2Array_int_float_bool_bool_bool(On.Celeste.FinalBoss.orig_ctor_Vector2_Vector2Array_int_float_bool_bool_bool orig, FinalBoss self, Vector2 position, Vector2[] nodes, int patternindex, float cameraypastmax, bool dialog, bool starthit, bool cameralocky) {
        orig(self, position, nodes, patternindex, cameraypastmax, dialog, starthit, cameralocky);
        self.Add(new Interpolation());
    }

    private static void FinalBossShot_ctor(On.Celeste.FinalBossShot.orig_ctor orig, FinalBossShot self) {
        orig(self);
        self.Add(new Interpolation());
    }

    private static void Key_ctor_Vector2_EntityID_Vector2Array(On.Celeste.Key.orig_ctor_Vector2_EntityID_Vector2Array orig, Key self, Vector2 position, EntityID id, Vector2[] nodes) {
        orig(self, position, id, nodes);
        self.Add(new Interpolation());
    }

    private static void RotateSpinner_ctor(On.Celeste.RotateSpinner.orig_ctor orig, RotateSpinner self, EntityData data, Vector2 offset) {
        orig(self, data, offset);
        self.Add(new Interpolation());
    }

    private static void Platform_ctor(On.Celeste.Platform.orig_ctor orig, Platform self, Vector2 position, bool safe) {
        orig(self, position, safe);
        self.Add(new Interpolation());
    }

    private static void Snowball_ctor(On.Celeste.Snowball.orig_ctor orig, Snowball self) {
        orig(self);
        self.Add(new Interpolation());
    }

    private static void TrackSpinner_ctor(On.Celeste.TrackSpinner.orig_ctor orig, TrackSpinner self, EntityData data, Vector2 offset) {
        orig(self, data, offset);
        self.Add(new Interpolation());
    }
}