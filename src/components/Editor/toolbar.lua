local toolbar = {
    panel = {},
    list = {}
}

function toolbar:init()
    local panel = Loveframes.Create("panel")
    panel = Loveframes.Create("panel")
    panel:SetPos(120, 20)
    panel:SetSize(300, 40)
    self.panel = panel

    local list = Loveframes.Create("list", panel)
    list:SetDisplayType("horizontal")
    list:SetSize(300, 40)
    self.list = list
end

function toolbar:addItem(text, callback)
    local tool = Loveframes.Create("button")
    tool:SetText(text)
    tool.OnClick = callback
    self.list:AddItem(tool)
end

return toolbar