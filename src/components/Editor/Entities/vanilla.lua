return function()
    RegisterEntityMetadata("PlayerSpawn", {
        width = 20, height = 20,
        originX = 10, originY = 10,
        texture = "player/statues/greenAlt"
    })
    RegisterEntityMetadata("TeamSpawn", {
        width = 20, height = 20,
        originX = 10, originY = 10
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
        width = 40, height = 40,
        originX = 20, originY = 20
    })
    -- TODO all vanilla entities 
end