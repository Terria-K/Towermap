return function (text, layer, callback)
    local button = Loveframes.Create("button")
    button:SetText(text)
    button.OnClick = callback
    layer.list:AddItem(button)
end