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
        manager.AddActor(new ActorInfo("JumpPad", "jumpPadOff", 20, 10, 0, 0, true), new Point(10, 10), TileRender1x3);
        manager.AddActor(new ActorInfo("Dummy", "dummy/dummy", 12, 20, 6, 10), new Point(12, 20), PlayerRender);
        manager.AddActor(new ActorInfo("FloorMiasma", "quest/floorMiasma", 10, 10, 0, 5, true, customValues: new() {
            ["Group"] = 0
        }), new Point(10, 10), TileRender1x3);

        manager.AddActor(new ActorInfo("CrackedPlatform", "crackedPlatform", 20, 10, 0, 0, true), new Point(10, 10), TileRender1x3);

        manager.AddActor(new ActorInfo("CrackedWall", "crackedWall", 20, 20, 0, 0));
        manager.AddActor(new ActorInfo("CrumbleWall", "crumbleWall", 20, 20, 0, 0));
        manager.AddActor(new ActorInfo("CrumbleBlock", "crumbleBlockTiles", 20, 20, 0, 0, true, true), new Point(10, 10), TileRender5x5);
        manager.AddActor(new ActorInfo("GraniteBlock", "graniteTiles", 20, 20, 0, 0, true, true), new Point(10, 10), TileRender4x4);
        manager.AddActor(new ActorInfo("ProximityBlock", "proximityBlockAnim", 20, 20, 0, 0), new Point(20, 20));

        manager.AddActor(new ActorInfo("MovingPlatform", "movingTiles", 20, 20, 0, 0, true, true, true), new Point(10, 10), TileRender3x3);
        manager.AddActor(new ActorInfo("RedSwitchBlock", "redSwitchBlock", 20, 20, 0, 0, true, true), new Point(10, 10), TileRender3x3);
        manager.AddActor(new ActorInfo("BlueSwitchBlock", "blueSwitchBlock", 20, 20, 0, 0, true, true), new Point(10, 10), TileRender3x3);
        manager.AddActor(new ActorInfo("MoonGlassBlock", "moonGlassTiles", 20, 20, 0, 0, true, true), new Point(10, 10), TileRender3x3);

        manager.AddActor(new ActorInfo("BGLantern", "details/lantern", 10, 10, 5, 5, customValues: new () {
            ["Lit"] = true
        }));
        manager.AddActor(new ActorInfo("BGCrystal", "details/wallCrystal", 10, 15, 5, 8), new Point(10, 15));
        manager.AddActor(new ActorInfo("BGSkeleton", "details/bones", 20, 20, 10, 20), new Point(20, 20));
        manager.AddActor(new ActorInfo("BGBigMushroom", "details/bigMushroom", 10, 20, 5, 20), onRender: BigMushroomRender);
        manager.AddActor(new ActorInfo("BGMushroom", "details/wallMushroom", 10, 10, 5, 0), new Point(10, 10));
        manager.AddActor(new ActorInfo("Cobwebs", "details/cobwebs", 20, 20, 10, 10), new Point(20, 20));
        manager.AddActor(new ActorInfo("SnowEd", "details/snowDeposit", 10, 10, 0, 10, true), new Point(10, 10), TileRender1x3);
        manager.AddActor(new ActorInfo("SnowClump", "details/snowDeposit", 10, 10, 0, 10), new Point(10, 10));
        manager.AddActor(new ActorInfo("KingIntro", "throneRoom", 20, 35, 10, 20), new Point(20, 35));
    }

    private static void BigMushroomRender(LevelActor level, Actor actor, Vector2 position, Vector2 size, Batch spriteBatch, Color color) 
    {
        spriteBatch.Draw(Resource.Atlas["details/bigMushroomBase"], new Vector2(position.X - 5, position.Y - 10), color);
    }

    private static void BlockTileRender(LevelActor level, Actor actor, Vector2 position, Vector2 size, Batch spriteBatch, Color color) 
    {
        for (int x = 0; x < size.X / 10; x++) 
        {
            for (int y = 0; y < size.Y / 10; y++) 
            {
                spriteBatch.Draw(actor.Texture,
                    new Vector2(position.X + (x * 10), position.Y + (y * 10)), color, Vector2.One, actor.Origin);
            }
        }
    }

    private static void TileRender3x3(LevelActor level, Actor actor, Vector2 position, Vector2 size, Batch spriteBatch, Color color) 
    {
        {
            var topleft = actor.Texture;
            spriteBatch.Draw(topleft, new Vector2(position.X, position.Y), color, Vector2.One, actor.Origin);

            var topright = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                X = actor.Texture.Source.X + 20,
            });

            spriteBatch.Draw(topright, new Vector2(position.X + (size.X - 10), position.Y), color, Vector2.One, actor.Origin);

            var bottomleft = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                Y = actor.Texture.Source.Y + 20
            });

            spriteBatch.Draw(bottomleft, new Vector2(position.X, position.Y + (size.Y - 10)), color, Vector2.One, actor.Origin);

            var bottomright = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                X = actor.Texture.Source.X + 20,
                Y = actor.Texture.Source.Y + 20
            });

            spriteBatch.Draw(bottomright, new Vector2(
                position.X + (size.X - 10), 
                position.Y + (size.Y - 10)), color, Vector2.One, actor.Origin);

            var hleft = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                X = actor.Texture.Source.X + 10
            });

            var hright = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                X = actor.Texture.Source.X + 10,
                Y = actor.Texture.Source.Y + 20
            });

            var vtop = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                Y = actor.Texture.Source.Y + 10
            });

            var vbottom = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                X = actor.Texture.Source.X + 20,
                Y = actor.Texture.Source.Y + 10
            });

            for (int i = 1; i < (size.X / 10) - 1; i++) 
            {
                spriteBatch.Draw(hleft, new Vector2(position.X + (i * 10), position.Y), color, Vector2.One, actor.Origin);
                spriteBatch.Draw(hright, new Vector2(position.X + (i * 10), position.Y + (size.Y - 10)), color, Vector2.One, actor.Origin);
            }

            for (int i = 1; i < (size.Y / 10) - 1; i++) 
            {
                spriteBatch.Draw(vtop, new Vector2(position.X, position.Y + (i * 10)), color, Vector2.One, actor.Origin);
                spriteBatch.Draw(vbottom, new Vector2(position.X + (size.X - 10), position.Y + (i * 10)), color, Vector2.One, actor.Origin);
            }

            var mid = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                X = actor.Texture.Source.X + 10,
                Y = actor.Texture.Source.Y + 10
            });

            for (int x = 1; x < (size.X / 10) - 1; x++) 
            {
                for (int y = 1; y < (size.Y / 10) - 1; y++) 
                {
                    spriteBatch.Draw(mid, new Vector2(position.X + (x * 10), position.Y + (y * 10)), color, Vector2.One, actor.Origin);
                }
            }
        }
    }

    private static void TileRender4x4(LevelActor level, Actor actor, Vector2 position, Vector2 size, Batch spriteBatch, Color color) 
    {
        if (size.X == 10 && size.Y == 10) 
        {
            var full = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                X = actor.Texture.Source.X + 30,
                Y = actor.Texture.Source.Y + 30
            });
            spriteBatch.Draw(full, new Vector2(position.X, position.Y), color, Vector2.One, actor.Origin);
            return;
        }

        if (size.X == 10) 
        {
            var top = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                X = actor.Texture.Source.X + 30,
            });

            var mid = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                X = actor.Texture.Source.X + 30,
                Y = actor.Texture.Source.Y + 10
            });

            var bottom = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                X = actor.Texture.Source.X + 30,
                Y = actor.Texture.Source.Y + 20
            });

            spriteBatch.Draw(top, new Vector2(position.X, position.Y), color, Vector2.One, actor.Origin);
            spriteBatch.Draw(bottom, new Vector2(position.X, position.Y + (size.Y - 10)), color, Vector2.One, actor.Origin);

            for (int i = 1; i < (size.Y / 10) - 1; i++)
            {
                spriteBatch.Draw(mid, new Vector2(position.X, position.Y + (i * 10)), color, Vector2.One, actor.Origin);
            }
            return;
        }

        if (size.Y == 10) 
        {
            var left = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                Y = actor.Texture.Source.Y + 30,
            });

            var mid = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                X = actor.Texture.Source.X + 10,
                Y = actor.Texture.Source.Y + 30
            });

            var right = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                X = actor.Texture.Source.X + 20,
                Y = actor.Texture.Source.Y + 30
            });

            spriteBatch.Draw(left, new Vector2(position.X, position.Y), color, Vector2.One, actor.Origin);
            spriteBatch.Draw(right, new Vector2(position.X + (size.X - 10), position.Y), color, Vector2.One, actor.Origin);

            for (int i = 1; i < (size.X / 10) - 1; i++)
            {
                spriteBatch.Draw(mid, new Vector2(position.X + (i * 10), position.Y), color, Vector2.One, actor.Origin);
            }
            return;
        }

        {
            var topleft = actor.Texture;
            spriteBatch.Draw(topleft, new Vector2(position.X, position.Y), color, Vector2.One, actor.Origin);

            var topright = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                X = actor.Texture.Source.X + 20,
            });

            spriteBatch.Draw(topright, new Vector2(position.X + (size.X - 10), position.Y), color, Vector2.One, actor.Origin);

            var bottomleft = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                Y = actor.Texture.Source.Y + 20
            });

            spriteBatch.Draw(bottomleft, new Vector2(position.X, position.Y + (size.Y - 10)), color, Vector2.One, actor.Origin);

            var bottomright = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                X = actor.Texture.Source.X + 20,
                Y = actor.Texture.Source.Y + 20
            });

            spriteBatch.Draw(bottomright, new Vector2(
                position.X + (size.X - 10), 
                position.Y + (size.Y - 10)), color, Vector2.One, actor.Origin);

            var hleft = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                X = actor.Texture.Source.X + 10
            });

            var hright = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                X = actor.Texture.Source.X + 10,
                Y = actor.Texture.Source.Y + 20
            });

            var vtop = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                Y = actor.Texture.Source.Y + 10
            });

            var vbottom = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                X = actor.Texture.Source.X + 20,
                Y = actor.Texture.Source.Y + 10
            });

            for (int i = 1; i < (size.X / 10) - 1; i++) 
            {
                spriteBatch.Draw(hleft, new Vector2(position.X + (i * 10), position.Y), color, Vector2.One, actor.Origin);
                spriteBatch.Draw(hright, new Vector2(position.X + (i * 10), position.Y + (size.Y - 10)), color, Vector2.One, actor.Origin);
            }

            for (int i = 1; i < (size.Y / 10) - 1; i++) 
            {
                spriteBatch.Draw(vtop, new Vector2(position.X, position.Y + (i * 10)), color, Vector2.One, actor.Origin);
                spriteBatch.Draw(vbottom, new Vector2(position.X + (size.X - 10), position.Y + (i * 10)), color, Vector2.One, actor.Origin);
            }

            var mid = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                X = actor.Texture.Source.X + 10,
                Y = actor.Texture.Source.Y + 10
            });

            for (int x = 1; x < (size.X / 10) - 1; x++) 
            {
                for (int y = 1; y < (size.Y / 10) - 1; y++) 
                {
                    spriteBatch.Draw(mid, new Vector2(position.X + (x * 10), position.Y + (y * 10)), color, Vector2.One, actor.Origin);
                }
            }
        }
    }

    private static void TileRender5x5(LevelActor level, Actor actor, Vector2 position, Vector2 size, Batch spriteBatch, Color color) 
    {
        if (size.X == 10 && size.Y == 10) 
        {
            var full = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                X = actor.Texture.Source.X + 40,
                Y = actor.Texture.Source.Y + 40
            });
            spriteBatch.Draw(full, new Vector2(position.X, position.Y), color, Vector2.One, actor.Origin);
            return;
        }

        if (size.X == 10) 
        {
            var top = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                X = actor.Texture.Source.X + 40,
            });

            var mid = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                X = actor.Texture.Source.X + 40,
                Y = actor.Texture.Source.Y + 10
            });

            var bottom = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                X = actor.Texture.Source.X + 40,
                Y = actor.Texture.Source.Y + 30
            });

            spriteBatch.Draw(top, new Vector2(position.X, position.Y), color, Vector2.One, actor.Origin);
            spriteBatch.Draw(bottom, new Vector2(position.X, position.Y + (size.Y - 10)), color, Vector2.One, actor.Origin);

            for (int i = 1; i < (size.Y / 10) - 1; i++)
            {
                spriteBatch.Draw(mid, new Vector2(position.X, position.Y + (i * 10)), color, Vector2.One, actor.Origin);
            }
            return;
        }

        if (size.Y == 10) 
        {
            var left = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                Y = actor.Texture.Source.X + 40,
            });

            var mid = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                X = actor.Texture.Source.X + 40,
                Y = actor.Texture.Source.Y + 10
            });

            var right = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                X = actor.Texture.Source.X + 30,
                Y = actor.Texture.Source.Y + 40
            });

            spriteBatch.Draw(left, new Vector2(position.X, position.Y), color, Vector2.One, actor.Origin);
            spriteBatch.Draw(right, new Vector2(position.X + (size.X - 10), position.Y), color, Vector2.One, actor.Origin);

            for (int i = 1; i < (size.X / 10) - 1; i++)
            {
                spriteBatch.Draw(mid, new Vector2(position.X + (i * 10), position.Y), color, Vector2.One, actor.Origin);
            }
            return;
        }

        {
            var topleft = actor.Texture;
            spriteBatch.Draw(topleft, new Vector2(position.X, position.Y), color, Vector2.One, actor.Origin);

            var topright = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                X = actor.Texture.Source.X + 30,
            });

            spriteBatch.Draw(topright, new Vector2(position.X + (size.X - 10), position.Y), color, Vector2.One, actor.Origin);

            var bottomleft = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                Y = actor.Texture.Source.Y + 30
            });

            spriteBatch.Draw(bottomleft, new Vector2(position.X, position.Y + (size.Y - 10)), color, Vector2.One, actor.Origin);

            var bottomright = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                X = actor.Texture.Source.X + 30,
                Y = actor.Texture.Source.Y + 30
            });

            spriteBatch.Draw(bottomright, new Vector2(
                position.X + (size.X - 10), 
                position.Y + (size.Y - 10)), color, Vector2.One, actor.Origin);

            var hleft = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                X = actor.Texture.Source.X + 10
            });

            var hright = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                X = actor.Texture.Source.X + 20,
                Y = actor.Texture.Source.Y + 30
            });

            var vtop = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                Y = actor.Texture.Source.Y + 20
            });

            var vbottom = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                X = actor.Texture.Source.X + 30,
                Y = actor.Texture.Source.Y + 20
            });

            for (int i = 1; i < (size.X / 10) - 1; i++) 
            {
                spriteBatch.Draw(hleft, new Vector2(position.X + (i * 10), position.Y), color, Vector2.One, actor.Origin);
                spriteBatch.Draw(hright, new Vector2(position.X + (i * 10), position.Y + (size.Y - 10)), color, Vector2.One, actor.Origin);
            }

            for (int i = 1; i < (size.Y / 10) - 1; i++) 
            {
                spriteBatch.Draw(vtop, new Vector2(position.X, position.Y + (i * 10)), color, Vector2.One, actor.Origin);
                spriteBatch.Draw(vbottom, new Vector2(position.X + (size.X - 10), position.Y + (i * 10)), color, Vector2.One, actor.Origin);
            }

            var mid = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
                X = actor.Texture.Source.X + 10,
                Y = actor.Texture.Source.Y + 20
            });

            for (int x = 1; x < (size.X / 10) - 1; x++) 
            {
                for (int y = 1; y < (size.Y / 10) - 1; y++) 
                {
                    spriteBatch.Draw(mid, new Vector2(position.X + (x * 10), position.Y + (y * 10)), color, Vector2.One, actor.Origin);
                }
            }
        }
    }
 
    private static void PlayerRender(LevelActor level, Actor actor, Vector2 position, Vector2 size, Batch spriteBatch, Color color)
    {
        if (level == null) 
        {
            return;
        }
        if (position.X > (320 / 2)) 
        {
            if (!level.RenderFlipped) 
            {
                level.TextureQuad.FlipUV(FlipMode.Horizontal);
                level.RenderFlipped = true;
            }
        }
        else if (level.RenderFlipped)
        {
            level.TextureQuad.FlipUV(FlipMode.None);
            level.RenderFlipped = false;
        }
    }

    private static void VerticalTileRender(LevelActor level, Actor actor, Vector2 position, Vector2 size, Batch spriteBatch, Color color) 
    {
        if (size.Y > 10) 
        {
            for (int i = 1; i < size.Y / 10; i++) 
            {
                spriteBatch.Draw(actor.Texture,
                    new Vector2(position.X, position.Y + (i * 10)), color, Vector2.One, actor.Origin);
            }
        }
    }

    private static void TileRender1x3(LevelActor level, Actor actor, Vector2 position, Vector2 size, Batch spriteBatch, Color color) 
    {
        var mid = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
            X = actor.Texture.Source.X + 10
        });
        var end = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
            X = actor.Texture.Source.X + 20
        });
        if (size.X > 10) 
        {
            var len = size.X / 10;
            for (int i = 1; i < len; i++) 
            {
                if (i == len - 1) 
                {
                    spriteBatch.Draw(end,
                        new Vector2(position.X + (i * 10), position.Y), color, Vector2.One, actor.Origin);
                }
                else 
                {
                    spriteBatch.Draw(mid,
                        new Vector2(position.X + (i * 10), position.Y), color, Vector2.One, actor.Origin);
                }
            }
        }
    }
}