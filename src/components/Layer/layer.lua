return function ()
    local layerFrame = Loveframes.Create("frame")
    layerFrame:SetName("Layer")
    layerFrame:SetPos(love.graphics.getWidth() - 180, 25)
    layerFrame:SetSize(180, love.graphics.getHeight() - 25)
    layerFrame:SetDraggable(false)
    layerFrame:ShowCloseButton(false)

    local layerList = Loveframes.Create("list", layerFrame)
    layerList:SetPos(5, 30)
    layerList:SetSize(170, 140)
    layerList:SetPadding(1)
    layerList:SetSpacing(2)

    return {
        frame = layerFrame,
        list = layerList
    }
end