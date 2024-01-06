return function()
    RegisterEntityMetadata("PlayerSpawn", {
        width = 20, height = 20,
        originX = 10, originY = 10,
        texture = { name = "player/statues/greenAlt", canFlip = true }
    })
    RegisterEntityMetadata("TeamSpawn", {
        width = 20, height = 20,
        originX = 10, originY = 10,
        texture = { name = "player/statues/pink", canFlip = true }
    })
    RegisterEntityMetadata("TeamSpawnA", {
        width = 20, height = 20,
        originX = 10, originY = 10,
        texture = { name = "player/statues/blue", canFlip = true }
    })
    RegisterEntityMetadata("TeamSpawnB", {
        width = 20, height = 20,
        originX = 10, originY = 10,
        texture = { name = "player/statues/red", canFlip = true }
    })
    RegisterEntityMetadata("Spawner", {
        width = 20, height = 20,
        originX = 10, originY = 10,
        texture = { name = "spawnPortal", width = 20, height = 20}
    })
    RegisterEntityMetadata("TreasureChest", {
        width = 10, height = 10,
        originX = 5, originY = 5,
        texture = { name = "treasureChest", width = 10, height = 10}
    })
    RegisterEntityMetadata("BGLantern", {
        width = 10, height = 10,
        originX = 5, originY = 5,
        texture = "details/lantern"
    })
    RegisterEntityMetadata("FloorMiasma", {
        width = 20, height = 10,
        originX = 0, originY = 5
    })
    RegisterEntityMetadata("Cobwebs", {
        width = 20, height = 20,
        originX = 10, originY = 10
    })
    RegisterEntityMetadata("JumpPad", {
        width = 20, height = 10,
        originX = 0, originY = 0,
        texture = "jumpPadOff"
    })
    RegisterEntityMetadata("EndlessPortal", {
        width = 50, height = 50,
        originX = 20, originY = 20,
        texture = { name = "nextLevelPortal", width = 50, height = 50}
    })
    -- TODO all vanilla entities 
end