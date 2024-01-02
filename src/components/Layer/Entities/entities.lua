local function addEntity(text, list)
    local playerSpawn = Loveframes.Create("button")
    playerSpawn:SetText(text)
    list:AddItem(playerSpawn)
end

return function (layer)
    local entityList = Loveframes.Create("list", layer.frame)
    entityList:SetPos(5, 180)
    entityList:SetSize(170, 400)
    entityList:SetPadding(0)
    entityList:SetSpacing(0)
    addEntity("PlayerSpawn", entityList)
    addEntity("TeamSpawn", entityList)
    addEntity("Spawner", entityList)
    addEntity("TreasureChest", entityList)
    addEntity("BGLantern", entityList)
    addEntity("EndlessPortal", entityList)
end