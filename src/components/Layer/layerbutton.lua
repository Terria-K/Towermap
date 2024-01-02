return function (text, layer)
    local button = Loveframes.Create("button")
    button:SetText(text)
    layer.list:AddItem(button)
end