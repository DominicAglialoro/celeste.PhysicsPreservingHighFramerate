using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public static class PlayerExtensions {
    public static void Load() => On.Celeste.Player.Added += Player_added;

    public static void Unload() => On.Celeste.Player.Added -= Player_added;

    private static void Player_added(On.Celeste.Player.orig_Added added, Player player, Scene scene) {
        added(player, scene);
        player.Add(new SimplePositionInterpolation(GetSmoothPosition));
    }

    private static Vector2 GetSmoothPosition(Entity entity, Vector2 position) {
        var player = (Player) entity;
        var dynamicData = DynamicData.For(player);

        if (player.StateMachine.State != 22)
            return player.Extrapolate(position, player.Speed, EngineExtensions.TimeDifference);

        var difference = dynamicData.Get<Vector2>("attractTo") - player.ExactPosition;
        var speed = 200f * difference.SafeNormalize();

        return player.Extrapolate(position, speed, EngineExtensions.TimeDifference, difference.Length());
    }
}