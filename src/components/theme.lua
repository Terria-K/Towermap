local theme = {
    SacredGround = {
        solidTiles = "tilesets/sacredGround",
        bgTiles = "tilesets/sacredGroundBG",
        tileIndex = 1,
        backdropRender = function(texs)
            texs:createElement("distantSky")
            texs:createElement("darkSky", 0, 0)
            texs:createElement("moon", 211, 34)
            texs:createElement("slowClouds", 0, 0)
            texs:createElement("thickMist", 0, 0, 0.02)
            texs:createElement("fastClouds", 0, 0, 0.03)
            texs:createElement("cloudRain", 0, 0, 0.04)
            texs:createElement("fastRain", 0, 0, 0.05)
        end
    },
    TwilightSpire = {
        solidTiles = "tilesets/twilightSpire",
        bgTiles = "tilesets/twilightSpireBG",
        tileIndex = 2,
        backdropRender = function(texs)
            texs:createElement("distantSky")
            texs:createElement("shine1", 160, 50, 0.4, 60, 52)
            texs:createElement("shine1", 60, 120, 0.4, 60, 52)
            texs:createElement("shine1", 200, 180, 0.4, 60, 52)
            texs:createElement("shine2", 110, 90, 0.4, 64, 64)
            texs:createElement("shine3", 90, 40, 0.4, 64, 64)
            texs:createElement("magicSymbols")
            texs:createElement("thickMist", 0, 0, 0.02)
            texs:createElement("fastClouds", 0, 0, 0.01)
        end
    },
    Backfire = {
        solidTiles = "tilesets/backfire",
        bgTiles = "tilesets/backfireBG",
        tileIndex = 3,
        backdropRender = function(texs)
            texs:createElement("CavesBack")
            texs:createElement("cavesFlickerA", 222, 12, 1, 9, 13)
            texs:createElement("cavesFlickerB", 189, 94, 1, 10, 11)
            texs:createElement("cavesFlickerC", 204, 123, 1, 15, 10)
            texs:createElement("cavesFlickerD", 128, 83, 1, 4, 4)
            texs:createElement("IgnisFatuus", 160, 120, 0.3, 32, 32)
            texs:createElement("IgnisFatuus2", 160, 120, 0.4, 32, 32)
            texs:createElement("thickMist", 0, 0, 0.05)
        end
    },
    Flight = {
        solidTiles = "tilesets/flight",
        bgTiles = "tilesets/flightBG",
        tileIndex = 4,
        backdropRender = function(texs)
            texs:createElement("daySky")
            texs:createElement("flightMoon", 45, 30)
            texs:createElement("thickMist", 0, 0, 0.18)
            texs:createElement("deepMist", 0, 0, 0.2)
        end
    },
    Mirage = {
        solidTiles = "tilesets/mirage",
        bgTiles = "tilesets/mirageBG",
        tileIndex = 5,
        backdropRender = function(texs)
            texs:createElement("desertDusk")
            texs:createElement("deepMist", 0, 0, 0.1)
        end
    },
    Thornwood = {
        solidTiles = "tilesets/thornwood",
        bgTiles = "tilesets/thornwoodBG",
        tileIndex = 6,
        backdropRender = function(texs)
            texs:createElement("forestSky")
            texs:createElement("forestLeaves")
            texs:createElement("frontTrees")
            texs:createElement("farSilhouette", 75, 30, 1, 23, 23)
            texs:createElement("farSilhouette2", 246, 58, 1, 23, 23)
        end
    },
    FrostfangKeep = {
        solidTiles = "tilesets/frostfangKeep",
        bgTiles = "tilesets/frostfangKeepBG",
        tileIndex = 8,
        backdropRender = function(texs)
            texs:createElement("distantSky")
            texs:createElement("snow", 0, 0, 0.06)
        end
    },
    KingsCourt = {
        solidTiles = "tilesets/kingsCourt",
        bgTiles = "tilesets/kingsCourtBG",
        tileIndex = 7,
        backdropRender = function(texs)
            texs:createElement("capitolBack")
            texs:createElement("capitolTowers", 0, 0, 0.85)
            texs:createElement("deepMist", 0, 0, 0.2)
            texs:createElement("snow", 0, 0, 0.5)
        end
    },
    SunkenCity = {
        solidTiles = "tilesets/sunkenCity",
        bgTiles = "tilesets/sunkenCityBG",
        tileIndex = 10,
        backdropRender = function(texs)
            texs:createElement("oceanBack")
            texs:createElement("shine4", 160, 50, 1, 48, 48)
            texs:createElement("sunkenBack", 0, 0, 1)
            texs:createElement("sunkenBack2", 138, 144, 1)
            texs:createElement("bubble", 160, 50, 0.4, 10, 32)
            texs:createElement("bubble", 160, 200, 0.4, 10, 32)
            texs:createElement("shine4", 160, 50, 1, 48, 48)
            texs:createElement("oceanFilter", 0, 0, 0.2)
            texs:createElement("leak2", 160, 110, 0.8, 32, 32)
        end
    },
    Moonstone = {
        solidTiles = "tilesets/moonstone",
        bgTiles = "tilesets/moonstoneBG",
        tileIndex = 9,
        backdropRender = function(texs)
            texs:createElement("distantSky")
            texs:createElement("floatingRocks", 10, 100)
            texs:createElement("thickMist", 0, 0, 0.02)
            texs:createElement("snow", 0, 0, 0.03)
            texs:createElement("sea", 0, 180, 0.04)
            texs:createElement("sea", 0, 185, 0.35)
            texs:createElement("WaterSplash", 0, 240, 1, 320, 100)
        end
    },
    TowerForge = {
        solidTiles = "tilesets/towerforge",
        bgTiles = "tilesets/towerforgeBG",
        tileIndex = 11,
        backdropRender = function(texs)
            texs:createElement("coreBg2")
            texs:createElement("godHead", 80, 75)
            texs:createElement("lavaFall1", 70, 0, 0.9)
            texs:createElement("lavaSplash", 72, 181, 1, 32, 32)
            texs:createElement("lavaFall2", 210, 0, 0.9)
            texs:createElement("lavaSplash", 212, 181, 1, 32, 32)
            texs:createElement("bgLava", 0, 195)
            texs:createElement("bgLava3", 0, 196, 0.1)
            texs:createElement("bgLava2", 0, 195, 0.5)
            texs:createElement("fastClouds", 0, 30, 0.04)
            texs:createElement("redSmokeClouds", 0, 150, 0.3)
        end
    },
    Ascension = {
        solidTiles = "tilesets/ascension",
        bgTiles = "tilesets/ascensionBG",
        tileIndex = 12,
        backdropRender = function(texs)
            texs:createElement("distantSky")
            texs:createElement("SkyClouds1")
            texs:createElement("SkyClouds2", 0, 32)
            texs:createElement("SkyClouds3", 0, 64)
            texs:createElement("SkyClouds4", 0, 96)
            texs:createElement("SkyClouds5", 0, 128)

            texs:createElement("farMap", -110, 120, 0.95)
            texs:createElement("farMapClouds", 0, 120, 0.4)
            texs:createElement("lowClouds", 0, 150, 0.35)

            texs:createElement("cloudRain", 0, 0, 0.05)
            texs:createElement("thickMist", 0, 0, 0.05)
        end
    },
    GauntletA = {
        solidTiles = "tilesets/gauntlet",
        bgTiles = "tilesets/gauntletBG",
        tileIndex = 13,
        backdropRender = function(texs)
            texs:createElement("antarticSky")
            texs:createElement("SkyClouds1")
            texs:createElement("SkyClouds2", 0, 32)
            texs:createElement("SkyClouds3", 0, 64)
            texs:createElement("SkyClouds4", 0, 96)
            texs:createElement("SkyClouds5b", 0, 128)

            texs:createElement("cloudRain", 0, 0, 0.05)
            texs:createElement("thickMist", 0, 0, 0.05)
        end
    },
    GauntletB = {
        solidTiles = "tilesets/gauntlet",
        bgTiles = "tilesets/gauntletBG",
        tileIndex = 13,
        backdropRender = function(texs)
            texs:createElement("antarticSky")
            texs:createElement("SkyClouds1")
            texs:createElement("SkyClouds2", 0, 32)
            texs:createElement("SkyClouds3", 0, 64)
            texs:createElement("SkyClouds4", 0, 96)
            texs:createElement("SkyClouds5b", 0, 128)

            texs:createElement("cloudRain", 0, 0, 0.05)
            texs:createElement("thickMist", 0, 0, 0.05)
        end
    },
    TheAmaranth = {
        solidTiles = "tilesets/theAmaranth",
        bgTiles = "tilesets/theAmaranthBG",
        tileIndex = 16,
        backdropRender = function(texs)
            texs:createElement("distantSky")
            texs:createElement("thickMist", 0, 0, 0.02)
            texs:createElement("snow", 0, 0, 0.03)
            texs:createElement("sea", 0, 154)
            texs:createElement("sea", 0, 165)
            texs:createElement("ghostShip", 160, 195, 1, 129, 96)
            texs:createElement("sea2", 0, 170, 0.5)
            texs:createElement("snow", 0, 0, 0.03)
            texs:createElement("sea", 0, 180)
            texs:createElement("sea", 0, 185)
            texs:createElement("WaterSplash", 0, 240, 1, 320, 100)
            texs:createElement("fastClouds", 0, 0, 0.03)
            texs:createElement("cloudRain", 0, 0, 0.04)
            texs:createElement("fastRain", 0, 0, 0.05)
        end
    },
    Dreadwood = {
        solidTiles = "tilesets/dreadwood",
        bgTiles = "tilesets/dreadwoodBG",
        tileIndex = 15,
        backdropRender = function(texs)
            texs:createElement("dreadSky")
            texs:createElement("dreadLeaves")
            texs:createElement("dreadTrees")
        end
    },
    Darkfang = {
        solidTiles = "tilesets/darkfang",
        bgTiles = "tilesets/darkfangBG",
        tileIndex = 14,
        backdropRender = function(texs)
            texs:createElement("darkFangBack")
            texs:createElement("snow", 0, 0, 0.06)
            texs:createElement("thickMist", 0, 0, 0.2)
        end
    },
    Cataclysm = {
        solidTiles = "tilesets/cataclysm",
        bgTiles = "tilesets/cataclysmBG",
        tileIndex = 17,
        backdropRender = function(texs)
            texs:createElement("vortex0", -130, -160, 1)
        end
    },
    DarkGauntlet = {
        solidTiles = "tilesets/cataclysm",
        bgTiles = "tilesets/cataclysmBG",
        tileIndex = 17,
        backdropRender = function(texs)
            texs:createElement("vortex0", -130, -160, 1)
        end
    }
}

return theme