using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public static class EntityExtensions {
    private static readonly Type[] FIXED_UPDATE_TYPES = {
        typeof(AngryOshiro),
        typeof(BadelineBoost),
        typeof(BadelineOldsite),
        typeof(BladeRotateSpinner),
        typeof(BladeTrackSpinner),
        typeof(Bridge),
        typeof(BridgeTile),
        typeof(Bumper),
        typeof(CassetteBlock),
        typeof(CassetteBlockManager),
        typeof(Cloud),
        typeof(ClutterSwitch),
        typeof(CoreModeToggle),
        typeof(CrumblePlatform),
        typeof(CrushBlock),
        typeof(CrystalStaticSpinner),
        typeof(DashSwitch),
        typeof(DustRotateSpinner),
        typeof(DustStaticSpinner),
        typeof(DustTrackSpinner),
        typeof(FallingBlock),
        typeof(FinalBoss),
        typeof(FinalBossBeam),
        typeof(FinalBossMovingBlock),
        typeof(FinalBossShot),
        typeof(FireBall),
        typeof(FlingBird),
        typeof(FloatySpaceBlock),
        typeof(FlyFeather),
        typeof(Glider),
        typeof(GoldenBlock),
        typeof(IntroCar),
        typeof(IntroCrusher),
        typeof(Killbox),
        typeof(MoveBlock),
        typeof(MovingPlatform),
        typeof(PlayerSeeker),
        typeof(Puffer),
        typeof(Refill),
        typeof(RisingLava),
        typeof(RotateSpinner),
        typeof(RotatingPlatform),
        typeof(SandwichLava),
        typeof(Seeker),
        typeof(SeekerBarrier),
        typeof(SinkingPlatform),
        typeof(Slider),
        typeof(Snowball),
        typeof(SpaceController),
        typeof(StarJumpBlock),
        typeof(StarJumpController),
        typeof(StarRotateSpinner),
        typeof(StarTrackSpinner),
        typeof(Strawberry),
        typeof(StrawberrySeed),
        typeof(SummitCheckpoint),
        typeof(SwapBlock),
        typeof(TempleCrackedBlock),
        typeof(TempleGate),
        typeof(TempleMirrorPortal),
        typeof(TheoCrystal),
        typeof(TrackSpinner),
        typeof(TriggerSpikes),
        typeof(WhiteBlock),
        typeof(WindController),
        typeof(ZipMover)
    };
    
    private static Dictionary<Type, IUpdateOverride> updateOverrides = new();
    
    public static void Load() {
        foreach (var type in FIXED_UPDATE_TYPES)
            type.AddUpdateOverride(FixedUpdateOverride.Instance);
    }

    public static void Unload() {
        foreach (var type in FIXED_UPDATE_TYPES)
            type.RemoveUpdateOverride();
    }

    public static void AddUpdateOverride(this Type type, IUpdateOverride updateOverride)
        => updateOverrides.Add(type, updateOverride);

    public static void RemoveUpdateOverride(this Type type) => updateOverrides.Remove(type);

    public static bool TryGetUpdateOverride(this Type type, out IUpdateOverride updateOverride)
        => updateOverrides.TryGetValue(type, out updateOverride);
    
    public static void OverrideUpdate(this Entity entity) {
        if (PhysicsPreservingHighFramerateModule.Settings.Enabled
            && entity.GetType().TryGetUpdateOverride(out var updateOverride))
            updateOverride.Update(entity);
        else
            entity.Update();
    }
    
    public static Vector2 GetRenderPosition(this Entity entity)
        => DynamicData.For(entity).Get<Vector2?>("renderPosition") ?? entity.Position;
    
    public static void FixedUpdate(this Entity entity) {
        if (entity.GetType().TryGetUpdateOverride(out var updateOverride))
            updateOverride.FixedUpdate(entity);
    }
}