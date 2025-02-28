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
        XmlDocument document = new XmlDocument();
        document.Load(Path.Combine(saveState.TFPath, "Content", "Atlas", "GameData", "tilesetData.xml"));

        themes.Add("SacredGround", new Theme(document["TilesetData"], "SacredGround", "SacredGround", "SacredGroundBG", (renderer) => {
            renderer.AddElement("distantSky");
            renderer.AddElement("darkSky");
            renderer.AddElement("moon", new Vector2(211, 34));
            renderer.AddElement("slowClouds", Vector2.Zero);
            renderer.AddElement("thickMist", Vector2.Zero, 0.02f);
            renderer.AddElement("fastClouds", Vector2.Zero, 0.03f);
            renderer.AddElement("cloudRain", Vector2.Zero, 0.04f);
            renderer.AddElement("fastRain", Vector2.Zero, 0.05f);
        }));

        themes.Add("TwilightSpire", new Theme(document["TilesetData"], "TwilightSpire", "TwilightSpire", "TwilightSpireBG", (renderer) => {
            renderer.AddElement("distantSky");
            renderer.AddElement("shine1", new Vector2(160, 50), 0.4f, new Point(60, 52));
            renderer.AddElement("shine1", new Vector2(60, 120), 0.4f, new Point(60, 52));
            renderer.AddElement("shine1", new Vector2(200, 180), 0.4f, new Point(60, 52));
            renderer.AddElement("shine2", new Vector2(110, 90), 0.4f, new Point(64, 64));
            renderer.AddElement("shine3", new Vector2(90, 40), 0.4f, new Point(64, 64));
            renderer.AddElement("magicSymbols");
            renderer.AddElement("thickMist", Vector2.Zero, 0.02f);
            renderer.AddElement("fastClouds", Vector2.Zero, 0.01f);
        }));

        themes.Add("Backfire", new Theme(document["TilesetData"], "Backfire", "Backfire", "BackfireBG", (renderer) => {
            renderer.AddElement("CavesBack");
            renderer.AddElement("cavesFlickerA", new Vector2(222, 12), 1, new Point(9, 13));
            renderer.AddElement("cavesFlickerB", new Vector2(189, 94), 1, new Point(10, 11));
            renderer.AddElement("cavesFlickerC", new Vector2(204, 123), 1, new Point(15, 10));
            renderer.AddElement("cavesFlickerD", new Vector2(128, 83), 1, new Point(4, 4));
            renderer.AddElement("IgnisFatuus", new Vector2(160, 120), 0.3f, new Point(32, 32));
            renderer.AddElement("IgnisFatuus2", new Vector2(160, 120), 0.4f, new Point(32, 32));
            renderer.AddElement("thickMist", Vector2.Zero, 0.05f);
        }));

        themes.Add("Flight", new Theme(document["TilesetData"], "Flight", "Flight", "FlightBG", (renderer) => {
            renderer.AddElement("daySky");
            renderer.AddElement("flightMoon", new Vector2(45, 30));
            renderer.AddElement("thickMist", Vector2.Zero, 0.18f);
            renderer.AddElement("deepMist", Vector2.Zero, 0.2f);
        }));

        themes.Add("Mirage", new Theme(document["TilesetData"], "Mirage", "Mirage", "MirageBG", (renderer) => {
            renderer.AddElement("desertDusk");
            renderer.AddElement("deepMist", Vector2.Zero, 0.1f);
        }));

        themes.Add("Thornwood", new Theme(document["TilesetData"], "Thornwood", "Thornwood", "ThornwoodBG", (renderer) => {
            renderer.AddElement("forestSky");
            renderer.AddElement("forestLeaves");
            renderer.AddElement("frontTrees");
            renderer.AddElement("farSilhouette", new Vector2(75, 30), 1, new Point(23, 23));
            renderer.AddElement("farSilhouette2", new Vector2(246, 58), 1, new Point(23, 23));
        }));

        themes.Add("FrostfangKeep", new Theme(document["TilesetData"], "FrostfangKeep", "FrostfangKeep", "FrostfangKeepBG", (renderer) => {
            renderer.AddElement("distantSky");
            renderer.AddElement("snow", Vector2.Zero, 0.06f);
        }));

        themes.Add("KingsCourt", new Theme(document["TilesetData"], "KingsCourt", "KingsCourt", "KingsCourtBG", (renderer) => {
            renderer.AddElement("capitolBack");
            renderer.AddElement("capitolTowers", Vector2.Zero, 0.85f);
            renderer.AddElement("deepMist", Vector2.Zero, 0.2f);
            renderer.AddElement("snow", Vector2.Zero, 0.5f);
        }));

        themes.Add("SunkenCity", new Theme(document["TilesetData"], "SunkenCity", "SunkenCity", "SunkenCityBG", (renderer) => {
            renderer.AddElement("oceanBack");
            renderer.AddElement("shine4", new Vector2(160, 50), 1, new Point(48, 48));
            renderer.AddElement("sunkenBack", Vector2.Zero, 1);
            renderer.AddElement("sunkenBack2", new Vector2(138, 144), 1);
            renderer.AddElement("bubble", new Vector2(160, 50), 0.4f, new Point(10, 32));
            renderer.AddElement("bubble", new Vector2(160, 200), 0.4f, new Point(10, 32));
            renderer.AddElement("shine4", new Vector2(160, 50), 1, new Point(48, 48));
            renderer.AddElement("oceanFilter", Vector2.Zero, 0.2f);
            renderer.AddElement("leak2", new Vector2(160, 110), 0.8f, new Point(32, 32));
        }));

        themes.Add("Moonstone", new Theme(document["TilesetData"], "Moonstone", "Moonstone", "MoonstoneBG", (renderer) => {
            renderer.AddElement("distantSky");
            renderer.AddElement("floatingRocks", new Vector2(10, 100));
            renderer.AddElement("thickMist", Vector2.Zero, 0.02f);
            renderer.AddElement("snow", Vector2.Zero, 0.03f);
            renderer.AddElement("sea", new Vector2(0, 180), 0.04f);
            renderer.AddElement("sea", new Vector2(0, 185), 0.35f);
            renderer.AddElement("WaterSplash", new Vector2(0, WorldUtils.WorldHeight), 1, new Point((int)WorldUtils.WorldWidth, 100));
        }));

        themes.Add("TowerForge", new Theme(document["TilesetData"], "TowerForge", "TowerForge", "TowerForgeBG", (renderer) => {
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
        }));

        themes.Add("Ascension", new Theme(document["TilesetData"], "Ascension", "Ascension", "AscensionBG", (renderer) => {
            renderer.AddElement("distantSky");
            renderer.AddElement("SkyClouds1");
            renderer.AddElement("SkyClouds2", new Vector2(0, 32));
            renderer.AddElement("SkyClouds3", new Vector2(0, 64));
            renderer.AddElement("SkyClouds4", new Vector2(0, 96));
            renderer.AddElement("SkyClouds5", new Vector2(0, 128));
        }));

        themes.Add("GauntletA", new Theme(document["TilesetData"], "GauntletA", "Gauntlet", "GauntletBG", (renderer) => {
            renderer.AddElement("antarticSky");
            renderer.AddElement("SkyClouds1");
            renderer.AddElement("SkyClouds2", new Vector2(0, 32));
            renderer.AddElement("SkyClouds3", new Vector2(0, 64));
            renderer.AddElement("SkyClouds4", new Vector2(0, 96));
            renderer.AddElement("SkyClouds5b", new Vector2(0, 128));
        }));

        themes.Add("GauntletB", new Theme(document["TilesetData"], "GauntletB", "Gauntlet", "GauntletBG", (renderer) => {
            renderer.AddElement("antarticSky");
            renderer.AddElement("SkyClouds1");
            renderer.AddElement("SkyClouds2", new Vector2(0, 32));
            renderer.AddElement("SkyClouds3", new Vector2(0, 64));
            renderer.AddElement("SkyClouds4", new Vector2(0, 96));
            renderer.AddElement("SkyClouds5b", new Vector2(0, 128));
        }));

        if (saveState.DarkWorld)
        {
            themes.Add("TheAmaranth", new Theme(document["TilesetData"], "TheAmaranth", "TheAmaranth", "TheAmaranthBG", (renderer) => {
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
            }));

            themes.Add("Dreadwood", new Theme(document["TilesetData"], "Dreadwood", "Dreadwood", "DreadwoodBG", (renderer) => {
                renderer.AddElement("dreadSky");
                renderer.AddElement("dreadLeaves");
                renderer.AddElement("dreadTrees");
            }));

            themes.Add("Darkfang", new Theme(document["TilesetData"], "Darkfang", "Darkfang", "DarkfangBG", (renderer) => {
                renderer.AddElement("darkFangBack");
                renderer.AddElement("snow", Vector2.Zero, 0.06f);
                renderer.AddElement("thickMist", Vector2.Zero, 0.2f);
            }));

            themes.Add("Cataclysm", new Theme(document["TilesetData"], "Cataclysm", "Cataclysm", "CataclysmBG", (renderer) => {
                renderer.AddElement("vortex0", new Vector2(-130, -160), 1);
            }));

            themes.Add("DarkGauntlet", new Theme(document["TilesetData"], "DarkGauntlet", "Cataclysm", "CataclysmBG", (renderer) => {
                renderer.AddElement("vortex0", new Vector2(-130, -160), 1);
            }));
        }

        ThemeNames = themes.Values.Select(x => x.Name).ToArray();
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