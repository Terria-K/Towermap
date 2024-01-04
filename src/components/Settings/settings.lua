local open = false;

return function(onClose)
    if (open) then
        return
    end
    open = true
    local settings = Loveframes.Create("frame")
    settings:SetName("Map Settings")
    settings:SetPos((960 * 0.5) - 500 * 0.5, (640 * 0.5) - 400 * 0.5)
    settings:SetSize(500, 400)
    settings:SetDraggable(false)

    settings.OnClose = function()
        open = false
        if onClose then
            onClose()
        end
    end

    local tab = Loveframes.Create("tabs", settings)
    tab:SetPos(5, 30)
    tab:SetSize(450, 330)

    local function CreateTab(text)
        local tabPanel = Loveframes.Create("panel")
        tab:AddTab(text, tabPanel)
        return tabPanel
    end

    local versus = CreateTab("Versus")

    local theme = Loveframes.Create("text", versus)
    theme:SetText("Themes: ")
    theme:SetPos(5, 10)

    local multiChoice = Loveframes.Create("multichoice", versus)
    multiChoice:SetPos(60, 5)
    multiChoice:AddChoice("SacredGround")
    multiChoice:AddChoice("TwilightSpire")
    multiChoice:AddChoice("Backfire")
    multiChoice:AddChoice("Flight")

    local button = Loveframes.Create("button", versus)
    button:SetText("+ Treasure")
    button:SetPos(5, 50)

    local treasures = Loveframes.Create("list", versus)
    treasures:SetPos(5, 80)

    local treasureItem = Loveframes.Create("text")
    treasureItem:SetText("ChaosOrb")

    local arrow= Loveframes.Create("text")
    arrow:SetText("Arrows")

    treasures:AddItem(arrow)
    treasures:AddItem(treasureItem)

    CreateTab("Trials")
    CreateTab("Quest")
    CreateTab("Dark World")
end