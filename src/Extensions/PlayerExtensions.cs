using Celeste.Mod.PhysicsPreservingHighFramerate.Other;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public static class PlayerExtensions {
    public static void Load() {
        typeof(Player).AddUpdateOverride(entity => new PlayerUpdateOverride((Player) entity));
        On.Celeste.Player.ctor += Player_ctor;
        On.Celeste.Player.Render += Player_Render;
    }

    private static void Player_ctor(On.Celeste.Player.orig_ctor ctor, Player player, Vector2 position, PlayerSpriteMode spritemode) {
        ctor(player, position, spritemode);
        DynamicData.For(player).Set("renderPosition", player.Position);
    }

    public static void Unload() {
        typeof(Player).RemoveUpdateOverride();
        On.Celeste.Player.Render -= Player_Render;
    }

    private static void Player_Render(On.Celeste.Player.orig_Render render, Player player) {
        if (!DynamicData.For(player).TryGet("renderPosition", out Vector2 renderPosition)) {
            render(player);
            
            return;
        }
        
        var previousPosition = player.Position;
        
        player.Position = renderPosition;
        render(player);
        player.Position = previousPosition;
    }

    private class PlayerUpdateOverride : IUpdateOverride {
        public Entity Entity => player;

        private Player player;
        
        public PlayerUpdateOverride(Player player) => this.player = player;

        public void FixedUpdate() {
            player.Update();
            DynamicData.For(player).Set("renderPosition", player.Position);
        }

        public void SmoothUpdate() {
            var dynamicData = DynamicData.For(player);
            
            if (!dynamicData.TryGet("renderPosition", out Vector2 renderPosition))
                renderPosition = player.Position;
            
            dynamicData.Set("renderPosition", renderPosition + player.Speed * CelesteExtensions.SmoothDeltaTime);
        }
    }
}