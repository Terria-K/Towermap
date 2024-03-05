using System.Collections.Generic;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;
using Riateu;
using Riateu.Graphics;

namespace Towermap;

public static class VanillaActor 
{
    public static void Init(ActorManager manager) 
    {
        manager.AddActor(new ActorInfo("PlayerSpawn", "player/statues/greenAlt", originX: 10, originY: 10), onRender: PlayerRender);
        manager.AddActor(new ActorInfo("TeamSpawn", "player/statues/pink", originX: 10, originY: 10), onRender: PlayerRender);
        manager.AddActor(new ActorInfo("TeamSpawnA", "player/statues/blue", originX: 10, originY: 10), onRender: PlayerRender);
        manager.AddActor(new ActorInfo("TeamSpawnB", "player/statues/red", originX: 10, originY: 10), onRender: PlayerRender);
        manager.AddActor(new ActorInfo("TreasureChest", "treasureChest", 10, 10, 5, 5, customValues: new () {
            ["Mode"] = "Normal",
            ["Treasure"] = "Arrows",
            ["Type"] = "Normal",
            ["Timer"] = 300
        }), new Point(10, 10));
        manager.AddActor(new ActorInfo("BigTreasureChest", "bigChest", 20, 20, 10, 10), new Point(20, 20));
        manager.AddActor(new ActorInfo("Spawner", "spawnPortal", 20, 20, 10, 10, customValues: new() {
            ["name"] = "..."
        }), new Point(20, 20));
        manager.AddActor(new ActorInfo("EndlessPortal", "nextLevelPortal", 50, 50, 25, 25), new Point(50, 50));
        manager.AddActor(new ActorInfo("BGLantern", "details/lantern", 10, 10, 5, 5, customValues: new () {
            ["Lit"] = false
        }));
        manager.AddActor(new ActorInfo("BGCrystal", "details/wallCrystal", 10, 15, 5, 8), new Point(10, 15));
        manager.AddActor(new ActorInfo("BGSkeleton", "details/bones", 20, 20, 10, 10), new Point(20, 20));
        manager.AddActor(new ActorInfo("CrackedWall", "crackedWall", 20, 20, 0, 0));
        manager.AddActor(new ActorInfo("Chain", "chain", 10, 10, 0, 0, false, true), new Point(10, 10), ChainRender);
    }

    private static void PlayerRender(LevelActor actor, Vector2 position, IBatch spriteBatch)
    {
        if (position.X > (320 / 2)) 
        {
            if (!actor.RenderFlipped) 
            {
                actor.TextureQuad.FlipUV(FlipMode.Horizontal);
                actor.RenderFlipped = true;
            }
        }
        else if (actor.RenderFlipped)
        {
            actor.TextureQuad.FlipUV(~FlipMode.Horizontal);
            actor.RenderFlipped = false;
        }
    }

    private static void ChainRender(LevelActor actor, Vector2 position, IBatch spriteBatch) 
    {
        if (actor.Height > 10) 
        {
            for (int i = 1; i < actor.Height / 10; i++) 
            {
                spriteBatch.Add(actor.TextureQuad, Resource.TowerFallTexture, GameContext.GlobalSampler,
                    new Vector2(position.X, position.Y + (i * 10)), Color.White, Vector2.One, actor.Data.Origin);
            }
        }
    }
}