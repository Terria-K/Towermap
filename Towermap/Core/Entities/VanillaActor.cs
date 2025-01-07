using System.Numerics;
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
        manager.AddActor(new ActorInfo("Spawner", "spawnPortal", 20, 20, 10, 10, hasNodes: true, customValues: new() {
            ["name"] = "..."
        }), new Point(20, 20));
        manager.AddActor(new ActorInfo("EndlessPortal", "nextLevelPortal", 50, 50, 25, 25), new Point(50, 50));
        manager.AddActor(new ActorInfo("OrbEd", "orb", 12, 12, 6, 6));
        manager.AddActor(new ActorInfo("ExplodingOrb", "explodingOrb", 12, 12, 6, 6), new Point(12, 12));

        manager.AddActor(new ActorInfo("Chain", "chain", 10, 10, 0, 0, false, true), new Point(10, 10), VerticalTileRender);
        manager.AddActor(new ActorInfo("Lantern", "lantern", 10, 10, 0, 0), new Point(10, 10));
        manager.AddActor(new ActorInfo("JumpPad", "jumpPadOff", 20, 10, 0, 0, true), new Point(10, 10), HorizontalThreeTileRender);
        manager.AddActor(new ActorInfo("Dummy", "dummy/dummy", 12, 20, 6, 10), new Point(12, 20), PlayerRender);
        manager.AddActor(new ActorInfo("FloorMiasma", "quest/floorMiasma", 10, 10, 0, 5, true, customValues: new() {
            ["Group"] = 0
        }), new Point(10, 10), HorizontalThreeTileRender);
        manager.AddActor(new ActorInfo("CrumbleBlock", "crumbleBlockTiles", 20, 20, 0, 0, true, true), new Point(10, 10), TileRender);
        manager.AddActor(new ActorInfo("GraniteBlock", "graniteTiles", 20, 20, 0, 0, true, true), new Point(10, 10), TileRender);

        manager.AddActor(new ActorInfo("BGLantern", "details/lantern", 10, 10, 5, 5, customValues: new () {
            ["Lit"] = true
        }));
        manager.AddActor(new ActorInfo("BGCrystal", "details/wallCrystal", 10, 15, 5, 8), new Point(10, 15));
        manager.AddActor(new ActorInfo("BGSkeleton", "details/bones", 20, 20, 10, 10), new Point(20, 20));
        manager.AddActor(new ActorInfo("Cobwebs", "details/cobwebs", 20, 20, 10, 10), new Point(20, 20));
        manager.AddActor(new ActorInfo("SnowEd", "details/snowDeposit", 10, 10, 0, 10, true), new Point(10, 10), TileRender);
    }

    private static void TileRender(LevelActor actor, Vector2 position, Batch spriteBatch) 
    {
        for (int x = 0; x < actor.Width / 10; x++) 
        {
            for (int y = 0; y < actor.Height / 10; y++) 
            {
                spriteBatch.Draw(actor.TextureQuad,
                    new Vector2(position.X + (x * 10), position.Y + (y * 10)), Color.White, Vector2.One, actor.Data.Origin);
            }
        }
    }
 
    private static void PlayerRender(LevelActor actor, Vector2 position, Batch spriteBatch)
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
            actor.TextureQuad.FlipUV(FlipMode.None);
            actor.RenderFlipped = false;
        }
    }

    private static void VerticalTileRender(LevelActor actor, Vector2 position, Batch spriteBatch) 
    {
        if (actor.Height > 10) 
        {
            for (int i = 1; i < actor.Height / 10; i++) 
            {
                spriteBatch.Draw(actor.TextureQuad,
                    new Vector2(position.X, position.Y + (i * 10)), Color.White, Vector2.One, actor.Data.Origin);
            }
        }
    }

    private static void HorizontalThreeTileRender(LevelActor actor, Vector2 position, Batch spriteBatch) 
    {
        var mid = new TextureQuad(Resource.TowerFallTexture, actor.TextureQuad.Source with {
            X = actor.TextureQuad.Source.X + 10
        });
        var end = new TextureQuad(Resource.TowerFallTexture, actor.TextureQuad.Source with {
            X = actor.TextureQuad.Source.X + 20
        });
        if (actor.Width > 10) 
        {
            var len = actor.Width / 10;
            for (int i = 1; i < len; i++) 
            {
                if (i == len - 1) 
                {
                    spriteBatch.Draw(end,
                        new Vector2(position.X + (i * 10), position.Y), Color.White, Vector2.One, actor.Data.Origin);
                }
                else 
                {
                    spriteBatch.Draw(mid,
                        new Vector2(position.X + (i * 10), position.Y), Color.White, Vector2.One, actor.Data.Origin);
                }
            }
        }
    }
}