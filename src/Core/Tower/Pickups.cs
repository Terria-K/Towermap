namespace Towermap;

public static class Pickups 
{
    public static PickupData[] AllPickups = [
        new PickupData("Arrows"),
        new PickupData("BombArrows"),
        new PickupData("SuperBombArrows"),
		new PickupData("LaserArrows"),
		new PickupData("BrambleArrows"),
		new PickupData("DrillArrows"),
		new PickupData("BoltArrows"),
		new PickupData("FeatherArrows"),
		new PickupData("TriggerArrows"),
		new PickupData("PrismArrows"),
		new PickupData("Shield"),
		new PickupData("Wings"),
		new PickupData("SpeedBoots"),
		new PickupData("Mirror"),
		new PickupData("TimeOrb"),
		new PickupData("DarkOrb"),
		new PickupData("LavaOrb"),
		new PickupData("SpaceOrb"),
		new PickupData("ChaosOrb"),
		new PickupData("Bomb")
    ];

    public static string[] PickupNames = [
        "Arrows",
        "BombArrows",
        "SuperBombArrows",
		"LaserArrows",
		"BrambleArrows",
		"DrillArrows",
		"BoltArrows",
		"FeatherArrows",
		"TriggerArrows",
		"PrismArrows",
		"Shield",
		"Wings",
		"SpeedBoots",
		"Mirror",
		"TimeOrb",
		"DarkOrb",
		"LavaOrb",
		"SpaceOrb",
		"ChaosOrb",
		"Bomb"
    ];

    public record struct PickupData(string Name);
    
}