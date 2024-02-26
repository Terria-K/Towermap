using MoonWorks.Math.Float;
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
        manager.AddActor(new ActorInfo("TreasureChest", "treasureChest", 10, 10, 5, 5), new Point(10, 10));
        manager.AddActor(new ActorInfo("Spawner", "spawnPortal", 20, 20, 10, 10), new Point(20, 20));
    }

    private static void PlayerRender(LevelActor actor, Vector2 position, IBatch spriteBatch)
    {
        actor.Sprite.FlipX = position.X > (320 / 2);
    }
}