using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Numerics;
using Jint;
using Jint.Native;
using Riateu;
using Riateu.Graphics;

namespace Towermap;

public static class VanillaActor 
{
    public static void Init(ActorManager manager, SaveState saveState) 
    {
        var entitiesPath = Path.GetFullPath("Entities");
        Engine engine = new Engine((opt) => opt.AllowClr().EnableModules(entitiesPath, true));
        var files = Directory.GetFiles(entitiesPath);

        for (int i = 0; i < files.Length; i += 1)
        {
            var jsPath = files[i];
            
            var jsObject = engine.Evaluate(File.ReadAllText(jsPath));
            var darkWorld = jsObject.Get("darkWorld");
            if (darkWorld == JsValue.Undefined)
            {
                darkWorld = false;
            }

            if (darkWorld.AsBoolean() && !saveState.DarkWorld)
            {
                continue;
            }


            var name = jsObject.Get("name").AsString();
            var texture = jsObject.Get("texture").AsString();
            var width = jsObject.Get("width");
            if (width == JsValue.Undefined)
            {
                width = 20;
            }

            var height = jsObject.Get("height");
            if (height == JsValue.Undefined)
            {
                height = 20;
            }

            var originX = jsObject.Get("originX");
            if (originX == JsValue.Undefined)
            {
                originX = 0;
            }
            var originY = jsObject.Get("originY");
            if (originY == JsValue.Undefined)
            {
                originY = 0;
            }

            var resizableX = jsObject.Get("resizableX");
            if (resizableX == JsValue.Undefined)
            {
                resizableX = false;
            }
            var resizableY = jsObject.Get("resizableY");
            if (resizableY == JsValue.Undefined)
            {
                resizableY = false;
            }

            var hasNodes = jsObject.Get("hasNodes");
            if (hasNodes == JsValue.Undefined)
            {
                hasNodes = false;
            }

            Point? point = null;
            var textureSize = jsObject.Get("textureSize");
            if (textureSize != JsValue.Undefined)
            {
                var pointArray = textureSize.AsArray();
                point = new Point((int)pointArray[0].AsNumber(), (int)pointArray[1].AsNumber());
            }

            string[] tags = null;
            var jsTags = jsObject.Get("tags");
            if (jsTags != JsValue.Undefined)
            {
                var tagArray = jsTags.AsArray();
                tags = new string[tagArray.Length];
                int j = 0;
                foreach (var t in tagArray)
                {
                    tags[j] = t.AsString();
                    j += 1;
                }
            }
            Dictionary<string, object> customValues = null;
            var values = jsObject.Get("values");
            if (values != JsValue.Undefined)
            {
                customValues = new Dictionary<string, object>();
                ExpandoObject expObj = (ExpandoObject)values.ToObject();
                foreach (var obj in expObj)
                {
                    customValues[obj.Key] = obj.Value;
                }
            }


            var onRender = jsObject.Get("onRender");

            ActorRender renderer = null;

            if (onRender.IsString())
            {
                var text = onRender.AsString();
                switch (text) 
                {
                    case "PlayerRender":
                        renderer = PlayerRender;
                        break;
                    case "TileRender3x1":
                        renderer = TileRender1x3;
                        break;
                    case "TileRender3x3":
                        renderer = TileRender3x3;
                        break;
                    case "TileRender4x4":
                        renderer = TileRender4x4;
                        break;
                    case "TileRender5x5":
                        renderer = TileRender5x5;
                        break;
                    case "VerticalTileRender":
                        renderer = VerticalTileRender;
                        break;
                    case "BigMushroomRender":
                        renderer = BigMushroomRender;
                        break;
                    case "OrbRender":
                        renderer = OrbRender;
                        break;
                    case "SpikeBallRender":
                        renderer = SpikeBallRender;
                        break;
                    case "SpikeBallEdRender":
                        renderer = SpikeBallEdRender;
                        break;
                    case "ShiftBlockRender":
                        renderer = ShiftBlockRender;
                        break;
                    case "SensorBlockRender":
                        renderer = SensorBlockRender;
                        break;
                }
            }
            else if (onRender != JsValue.Undefined)
            {
                var funcInstance = onRender.AsFunctionInstance();
                renderer = (level, actor, position, size, spriteBatch, color) => {
                    funcInstance.Call(jsObject, new Jint.Native.JsValue[] {
                        // level, actor, position, size, spriteBatch, color
                    });
                };
            }

            manager.AddActor(
                new ActorInfo(
                    name, 
                    texture, 
                    (int)width.AsNumber(), 
                    (int)height.AsNumber(), 
                    (int)originX.AsNumber(), 
                    (int)originY.AsNumber(),
                    resizableX.AsBoolean(),
                    resizableY.AsBoolean(),
                    hasNodes.AsBoolean(),
                    customValues
                ), 
                tags,
                point,
                renderer);
        }
    }

    private static void ShiftBlockRender(LevelActor level, Actor actor, Vector2 position, Vector2 size, Batch spriteBatch, Color color)
    {
        TileRender3x3(level, actor, position, size, spriteBatch, color);

        if (size.X > 20 && size.Y > 20)
        {
            var face = Resource.Atlas["shiftBlockFace"];
            var usingFace = new TextureQuad(Resource.TowerFallTexture, face.Source with {
                Width = 20,
                Height = 20
            });
            spriteBatch.Draw(usingFace, new Vector2(position.X + (size.X / 2f) - 10, position.Y + (size.Y / 2f) - 10), Color.White);
        }
        else if (size.Y <= 20)
        {
            var face = Resource.Atlas["shiftBlockFaceHorizontal"];
            var usingFace = new TextureQuad(Resource.TowerFallTexture, face.Source with {
                Width = 20,
                Height = 20
            });
            spriteBatch.Draw(usingFace, new Vector2(position.X + (size.X / 2f) - 10, position.Y + (size.Y / 2f) - 10), Color.White);
        }
        else 
        {
            var face = Resource.Atlas["shiftBlockFaceVertical"];
            var usingFace = new TextureQuad(Resource.TowerFallTexture, face.Source with {
                Width = 20,
                Height = 20
            });
            spriteBatch.Draw(usingFace, new Vector2(position.X + (size.X / 2f) - 10, position.Y + (size.Y / 2f) - 10), Color.White);
        }
    }

    private static void SensorBlockRender(LevelActor level, Actor actor, Vector2 position, Vector2 size, Batch spriteBatch, Color color)
    {
        var left = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
            Width = actor.Texture.Source.Width + 10
        });
        var mid = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
            X = actor.Texture.Source.X + 20,
            Width = actor.Texture.Source.Width
        });
        var right = new TextureQuad(Resource.TowerFallTexture, actor.Texture.Source with {
            X = actor.Texture.Source.X + 10,
            Width = actor.Texture.Source.Width
        });
        for (int i = 0; i < size.X; i += 10)
        {
            TextureQuad quad;
            if (i == 0)
            {
                quad = left;
            }
            else if (i == size.X - 10)
            {
                quad = mid;
            }
            else 
            {
                quad = right;
            }
            spriteBatch.Draw(quad, new Vector2(position.X + i, position.Y), Color.White);
        }

        var gemPos = new Vector2(position.X + (size.X / 2) - 5, position.Y + (size.Y / 2) - 5);

        if (size.X <= 40)
        {
            spriteBatch.Draw(
                Resource.Atlas["sensorGem"], 
                gemPos,
                Color.White);
        }
        else if (size.X <= 80)
        {
            var num = size.X / 3f;
            for (int i = 0; i < 2; i++)
            {
                spriteBatch.Draw(
                    Resource.Atlas["sensorGem"], 
                    gemPos,
                    Color.White,
                    Vector2.One,
                    Vector2.UnitX * (-num / 2f + num * (float)i)
                );
            }
        }
        else 
        {
            var num = size.X / 4f;
            for (int i = 0; i < 3; i++)
            {
                spriteBatch.Draw(
                    Resource.Atlas["sensorGem"], 
                    gemPos,
                    Color.White,
                    Vector2.One,
                    Vector2.UnitX * (-num + num * (float)i)
                );
            }
        }
    }

    private static void SpikeBallEdRender(LevelActor level, Actor actor, Vector2 position, Vector2 size, Batch spriteBatch, Color color)
    {
        if (level == null)
        {
            return;
        }
        var length = size.Y / 10;

        for (int i = 0; i < length - 1; i++)
        {
            spriteBatch.Draw(Resource.Atlas["spikeBallChain"], new Vector2(position.X - 4, position.Y + (i * 10) + 4), Color.White);
        }
    }

    private static void SpikeBallRender(LevelActor level, Actor actor, Vector2 position, Vector2 size, Batch spriteBatch, Color color)
    {
        if (level == null || level.Nodes.Count == 0)
        {
            return;
        }

        if ((bool)level.CustomData["Explodes"])
        {
            var explodingSpikeBall = Resource.Atlas["explodingSpikeBall"];

            level.TextureQuad = new TextureQuad(
                Resource.TowerFallTexture, 
                new Rectangle(explodingSpikeBall.Source.X, explodingSpikeBall.Source.Y, 22, 22)
            );
        }
        else 
        {
            level.TextureQuad = Resource.Atlas["spikeBall"];
        }

        var firstNode = level.Nodes[0];
        var dist = position.Y - firstNode.Y;
        var length = Math.Abs(dist) / 10;

        for (int i = 0; i < length; i++)
        {
            spriteBatch.Draw(Resource.Atlas["spikeBallChain"], new Vector2(position.X - 4, position.Y + (i * 10) + 4), Color.White);
        }
    }

    private static void OrbRender(LevelActor level, Actor actor, Vector2 position, Vector2 size, Batch spriteBatch, Color color)
    {
        spriteBatch.Draw(Resource.Atlas["details/orbHolder"], new Vector2(position.X - actor.Origin.X - 2, position.Y + actor.Origin.Y - 2), color);
    }

    private static void BigMushroomRender(LevelActor level, Actor actor, Vector2 position, Vector2 size, Batch spriteBatch, Color color) 
    {
        spriteBatch.Draw(Resource.Atlas["details/bigMushroomBase"], new Vector2(position.X - 5, position.Y - 10), color);
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
        if (position.X > (WorldUtils.WorldWidth / 2)) 
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