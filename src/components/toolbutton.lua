return function (text, pos, width, options)
    local fileButton = Loveframes.Create("button")
    fileButton:SetPos(pos, 0)
    fileButton:SetSize(width, 25)
    fileButton:SetText(text)
    fileButton.OnClick = function(object, x, y)

        local fileMenu = Loveframes.Create("menu")
        fileMenu:SetPos(pos, 25)
        fileMenu:SetSize(60, 25)

        for _, v in ipairs(options) do
            fileMenu:AddOption(v.text, false, v.callback)
        end
    end
end