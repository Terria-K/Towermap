local function addEntity(text, list, callback)
    local playerSpawn = Loveframes.Create("button")
    playerSpawn:SetText(text)
    playerSpawn.OnClick = callback
    list:AddItem(playerSpawn)
end

return function (layer, callback)
    local entityList = Loveframes.Create("list", layer.frame)
    entityList:SetPos(5, 180)
    entityList:SetSize(170, 170)
    entityList:SetPadding(0)
    entityList:SetSpacing(0)

    for k, v in pairs(GetAllMetadatas()) do
        addEntity(k, entityList, function()
            callback(k, v)
        end)
    end
    return entityList
end