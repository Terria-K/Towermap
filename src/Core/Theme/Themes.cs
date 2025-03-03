using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Xml;
using Riateu;
using Riateu.Graphics;

namespace Towermap;

public static class Themes 
{
    private static Dictionary<string, Theme> themes = new Dictionary<string, Theme>();
    public static string[] ThemeNames;

    public static void InitThemes(SaveState saveState) 
    {
        var themeData = Path.Combine(saveState.TFPath, "Content", "Atlas", "GameData", "themeData.xml");
        var document = new XmlDocument();
        document.Load(themeData);

        var root = document["ThemeData"];
        foreach (XmlElement themeXml in root.GetElementsByTagName("Theme"))
        {
            var id = themeXml.Attr("id");
            var tileset = themeXml["Tileset"].InnerText;
            var bgtileset = themeXml["BGTileset"].InnerText;
            Theme theme = new Theme(tileset, bgtileset);
            theme.ID = id;
            theme.Name = themeXml["Name"].InnerText;
            theme.Icon = themeXml["Icon"].InnerText;
            theme.Music = themeXml["Music"].InnerText;
            theme.TowerType = themeXml["TowerType"].InnerText;
            var mapPosition = themeXml["MapPosition"];
            theme.MapPosition = new Vector2(mapPosition.AttrInt("x"), mapPosition.AttrInt("y"));
            theme.Background = themeXml["Background"].InnerText;

            theme.Lanterns = themeXml["Lanterns"]?.InnerText;
            theme.DrillParticleColor = themeXml["DrillParticleColor"]?.InnerText;
            theme.CrackedBlockColor = themeXml["CrackedBlockColor"]?.InnerText;
            theme.World = themeXml["World"]?.InnerText;
            if (float.TryParse(themeXml["Wind"]?.InnerText, out float wres))
            {
                theme.Wind = wres;
            }
            if (float.TryParse(themeXml["DarknessOpacity"]?.InnerText, out float darknessOpacity))
            {
                theme.DarknessOpacity = darknessOpacity;
            }
            if (bool.TryParse(themeXml["Raining"]?.InnerText, out bool rres))
            {
                theme.Raining = rres;
            }

            if (bool.TryParse(themeXml["Cold"]?.InnerText, out bool cres))
            {
                theme.Cold = cres;
            }
            theme.DarknessColor = themeXml["DarknessColor"]?.InnerText;
            themes.Add(id, theme);
        }

        themes["SacredGround"].SetRenderer((renderer) => {
            renderer.AddElement("distantSky");
            renderer.AddElement("darkSky");
            renderer.AddElement("moon", new Vector2(211, 34));
            renderer.AddElement("slowClouds", Vector2.Zero);
            renderer.AddElement("thickMist", Vector2.Zero, 0.02f);
            renderer.AddElement("fastClouds", Vector2.Zero, 0.03f);
            renderer.AddElement("cloudRain", Vector2.Zero, 0.04f);
            renderer.AddElement("fastRain", Vector2.Zero, 0.05f);
        });

        themes["TwilightSpire"].SetRenderer((renderer) => {
            renderer.AddElement("distantSky");
            renderer.AddElement("shine1", new Vector2(160, 50), 0.4f, new Point(60, 52));
            renderer.AddElement("shine1", new Vector2(60, 120), 0.4f, new Point(60, 52));
            renderer.AddElement("shine1", new Vector2(200, 180), 0.4f, new Point(60, 52));
            renderer.AddElement("shine2", new Vector2(110, 90), 0.4f, new Point(64, 64));
            renderer.AddElement("shine3", new Vector2(90, 40), 0.4f, new Point(64, 64));
            renderer.AddElement("magicSymbols");
            renderer.AddElement("thickMist", Vector2.Zero, 0.02f);
            renderer.AddElement("fastClouds", Vector2.Zero, 0.01f);
        });

        themes["Backfire"].SetRenderer((renderer) => {
            renderer.AddElement("CavesBack");
            renderer.AddElement("cavesFlickerA", new Vector2(222, 12), 1, new Point(9, 13));
            renderer.AddElement("cavesFlickerB", new Vector2(189, 94), 1, new Point(10, 11));
            renderer.AddElement("cavesFlickerC", new Vector2(204, 123), 1, new Point(15, 10));
            renderer.AddElement("cavesFlickerD", new Vector2(128, 83), 1, new Point(4, 4));
            renderer.AddElement("IgnisFatuus", new Vector2(160, 120), 0.3f, new Point(32, 32));
            renderer.AddElement("IgnisFatuus2", new Vector2(160, 120), 0.4f, new Point(32, 32));
            renderer.AddElement("thickMist", Vector2.Zero, 0.05f);
        });

        themes["Flight"].SetRenderer((renderer) => {
            renderer.AddElement("daySky");
            renderer.AddElement("flightMoon", new Vector2(45, 30));
            renderer.AddElement("thickMist", Vector2.Zero, 0.18f);
            renderer.AddElement("deepMist", Vector2.Zero, 0.2f);
        });

        themes["Mirage"].SetRenderer((renderer) => {
            renderer.AddElement("desertDusk");
            renderer.AddElement("deepMist", Vector2.Zero, 0.1f);
        });

        themes["Thornwood"].SetRenderer((renderer) => {
            renderer.AddElement("forestSky");
            renderer.AddElement("forestLeaves");
            renderer.AddElement("frontTrees");
            renderer.AddElement("farSilhouette", new Vector2(75, 30), 1, new Point(23, 23));
            renderer.AddElement("farSilhouette2", new Vector2(246, 58), 1, new Point(23, 23));
        });

        themes["FrostfangKeep"].SetRenderer((renderer) => {
            renderer.AddElement("distantSky");
            renderer.AddElement("snow", Vector2.Zero, 0.06f);
        });

        themes["KingsCourt"].SetRenderer((renderer) => {
            renderer.AddElement("capitolBack");
            renderer.AddElement("capitolTowers", Vector2.Zero, 0.85f);
            renderer.AddElement("deepMist", Vector2.Zero, 0.2f);
            renderer.AddElement("snow", Vector2.Zero, 0.5f);
        });

        themes["SunkenCity"].SetRenderer((renderer) => {
            renderer.AddElement("oceanBack");
            renderer.AddElement("shine4", new Vector2(160, 50), 1, new Point(48, 48));
            renderer.AddElement("sunkenBack", Vector2.Zero, 1);
            renderer.AddElement("sunkenBack2", new Vector2(138, 144), 1);
            renderer.AddElement("bubble", new Vector2(160, 50), 0.4f, new Point(10, 32));
            renderer.AddElement("bubble", new Vector2(160, 200), 0.4f, new Point(10, 32));
            renderer.AddElement("shine4", new Vector2(160, 50), 1, new Point(48, 48));
            renderer.AddElement("oceanFilter", Vector2.Zero, 0.2f);
            renderer.AddElement("leak2", new Vector2(160, 110), 0.8f, new Point(32, 32));
        });

        themes["Moonstone"].SetRenderer((renderer) => {
            renderer.AddElement("distantSky");
            renderer.AddElement("floatingRocks", new Vector2(10, 100));
            renderer.AddElement("thickMist", Vector2.Zero, 0.02f);
            renderer.AddElement("snow", Vector2.Zero, 0.03f);
            renderer.AddElement("sea", new Vector2(0, 180), 0.04f);
            renderer.AddElement("sea", new Vector2(0, 185), 0.35f);
            renderer.AddElement("WaterSplash", new Vector2(0, WorldUtils.WorldHeight), 1, new Point((int)WorldUtils.WorldWidth, 100));
        });

        themes["TowerForge"].SetRenderer((renderer) => {
            renderer.AddElement("coreBg2");
            renderer.AddElement("godHead", new Vector2(80, 75));
            renderer.AddElement("lavaFall1", new Vector2(70, 0), 0.9f);
            renderer.AddElement("lavaSplash", new Vector2(72, 181), 1, new Point(32, 32));
            renderer.AddElement("lavaFall2", new Vector2(210, 0), 0.9f);
            renderer.AddElement("lavaSplash", new Vector2(212, 181), 1, new Point(32, 32));
            renderer.AddElement("bgLava", new Vector2(0, 195));
            renderer.AddElement("bgLava3", new Vector2(0, 196), 0.1f);
            renderer.AddElement("bgLava2", new Vector2(0, 195), 0.5f);
            renderer.AddElement("fastClouds", new Vector2(0, 30), 0.04f);
            renderer.AddElement("redSmokeClouds", new Vector2(0, 150), 0.3f);
        });

        themes["Ascension"].SetRenderer((renderer) => {
            renderer.AddElement("distantSky");
            renderer.AddElement("SkyClouds1");
            renderer.AddElement("SkyClouds2", new Vector2(0, 32));
            renderer.AddElement("SkyClouds3", new Vector2(0, 64));
            renderer.AddElement("SkyClouds4", new Vector2(0, 96));
            renderer.AddElement("SkyClouds5", new Vector2(0, 128));
        });

        themes["GauntletA"].SetRenderer((renderer) => {
            renderer.AddElement("antarticSky");
            renderer.AddElement("SkyClouds1");
            renderer.AddElement("SkyClouds2", new Vector2(0, 32));
            renderer.AddElement("SkyClouds3", new Vector2(0, 64));
            renderer.AddElement("SkyClouds4", new Vector2(0, 96));
            renderer.AddElement("SkyClouds5b", new Vector2(0, 128));
        });

        themes["GauntletB"].SetRenderer((renderer) => {
            renderer.AddElement("antarticSky");
            renderer.AddElement("SkyClouds1");
            renderer.AddElement("SkyClouds2", new Vector2(0, 32));
            renderer.AddElement("SkyClouds3", new Vector2(0, 64));
            renderer.AddElement("SkyClouds4", new Vector2(0, 96));
            renderer.AddElement("SkyClouds5b", new Vector2(0, 128));
        });

        if (saveState.DarkWorld)
        {
            themes["TheAmaranth"].SetRenderer((renderer) => {
                renderer.AddElement("distantSky");
                renderer.AddElement("thickMist", Vector2.Zero, 0.02f);
                renderer.AddElement("snow", Vector2.Zero, 0.03f);
                renderer.AddElement("sea", new Vector2(0, 154));
                renderer.AddElement("sea", new Vector2(0, 165));
                renderer.AddElement("ghostShip", new Vector2(160, 195), 1, new Point(129, 96));
                renderer.AddElement("sea2", new Vector2(0, 170), 0.5f);
                renderer.AddElement("snow", Vector2.Zero, 0.03f);
                renderer.AddElement("sea", new Vector2(0, 180));
                renderer.AddElement("sea", new Vector2(0, 185));
                renderer.AddElement("WaterSplash", new Vector2(0, WorldUtils.WorldHeight), 1, new Point((int)WorldUtils.WorldWidth, 100));
                renderer.AddElement("fastClouds", Vector2.Zero, 0.03f);
                renderer.AddElement("cloudRain", Vector2.Zero, 0.04f);
                renderer.AddElement("fastRain", Vector2.Zero, 0.05f);
            });

            themes["Dreadwood"].SetRenderer((renderer) => {
                renderer.AddElement("dreadSky");
                renderer.AddElement("dreadLeaves");
                renderer.AddElement("dreadTrees");
            });

            themes["Darkfang"].SetRenderer((renderer) => {
                renderer.AddElement("darkFangBack");
                renderer.AddElement("snow", Vector2.Zero, 0.06f);
                renderer.AddElement("thickMist", Vector2.Zero, 0.2f);
            });

            themes["Cataclysm"].SetRenderer((renderer) => {
                renderer.AddElement("vortex0", new Vector2(-130, -160), 1);
            });

            themes["DarkGauntlet"].SetRenderer((renderer) => {
                renderer.AddElement("vortex0", new Vector2(-130, -160), 1);
            });
       }

        ThemeNames = themes.Values.Select(x => x.ID).ToArray();
    }

    public static bool TryGetTheme(string name, out Theme theme) 
    {
        if (themes.TryGetValue(name, out theme)) 
        {
            return true;
        }
        Logger.Error($"Theme '{name}' cannot be found");
        theme = themes["Flight"];
        return false;
    }
}