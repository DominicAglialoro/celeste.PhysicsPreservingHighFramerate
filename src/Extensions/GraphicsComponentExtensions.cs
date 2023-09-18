using System;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.PhysicsPreservingHighFramerate; 

public static class GraphicsComponentExtensions {
    private static Hook On_Monocle_GraphicsComponent_get_RenderPosition;
    private static Hook On_Monocle_GraphicsComponent_set_RenderPosition;
    
    public static void Load() {
        On_Monocle_GraphicsComponent_get_RenderPosition = new Hook(typeof(GraphicsComponent).GetPropertyUnconstrained("RenderPosition").GetGetMethod(), GraphicsComponent_get_RenderPosition);
        On_Monocle_GraphicsComponent_set_RenderPosition = new Hook(typeof(GraphicsComponent).GetPropertyUnconstrained("RenderPosition").GetSetMethod(), GraphicsComponent_set_RenderPosition);
    }

    public static void Unload() {
        On_Monocle_GraphicsComponent_get_RenderPosition.Dispose();
        On_Monocle_GraphicsComponent_set_RenderPosition.Dispose();
    }

    private static Vector2 GraphicsComponent_get_RenderPosition(Func<GraphicsComponent, Vector2> get_RenderPosition, GraphicsComponent graphicsComponent) {
        if (!PhysicsPreservingHighFramerateModule.Settings.Enabled)
            return get_RenderPosition(graphicsComponent);

        var renderPosition = graphicsComponent.Entity?.GetRenderPosition() ?? Vector2.Zero;

        return renderPosition + graphicsComponent.Position;
    }
    
    private static void GraphicsComponent_set_RenderPosition(Action<GraphicsComponent, Vector2> set_RenderPosition, GraphicsComponent graphicsComponent, Vector2 value) {
        if (!PhysicsPreservingHighFramerateModule.Settings.Enabled) {
            set_RenderPosition(graphicsComponent, value);

            return;
        }

        var renderPosition = graphicsComponent.Entity?.GetRenderPosition() ?? Vector2.Zero;

        graphicsComponent.Position = value - renderPosition;
    }
}