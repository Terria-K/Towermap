local settings = {
    versusEvents = {
        onSaved = nil
    }
}

return function(tower, onClose)
    local settingsFrame = Loveframes.Create("frame")
    settingsFrame:SetName("Map Settings")
    settingsFrame:SetPos((960 * 0.5) - 500 * 0.5, (640 * 0.5) - 400 * 0.5)
    settingsFrame:SetSize(500, 400)
    settingsFrame:SetDraggable(false)
    settingsFrame:SetModal(true)

    settingsFrame.OnClose = function()
        if onClose then
            onClose()
        end
    end

    local tab = Loveframes.Create("tabs", settingsFrame)
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
    multiChoice:AddChoice("Mirage")
    multiChoice:AddChoice("Thornwood")
    multiChoice:AddChoice("KingsCourt")
    multiChoice:AddChoice("FrostfangKeep")
    multiChoice:AddChoice("SunkenCity")
    multiChoice:AddChoice("Moonstone")
    multiChoice:AddChoice("TowerForge")
    multiChoice:AddChoice("Ascension")
    multiChoice:AddChoice("GauntletA")
    multiChoice:AddChoice("GauntletB")
    multiChoice:AddChoice("TheAmaranth")
    multiChoice:AddChoice("Dreadwood")
    multiChoice:AddChoice("Darkfang")
    multiChoice:AddChoice("Cataclysm")
    multiChoice:AddChoice("DarkGauntlet")
    multiChoice:SelectChoice(tower.data.theme)

    local treasureChoice = Loveframes.Create("multichoice", versus)
    treasureChoice:SetPos(100, 50)
    treasureChoice:AddChoice("Arrows")
    treasureChoice:AddChoice("BombArrows")
    treasureChoice:AddChoice("Shield")
    treasureChoice:AddChoice("Wings")
    treasureChoice:AddChoice("ChaosOrb")

    local button = Loveframes.Create("button", versus)
    button:SetText("+ Treasure")
    button:SetPos(5, 50)

    local treasures = Loveframes.Create("list", versus)
    treasures:SetPos(5, 80)

    for i = 1, #tower.data.treasure do
        local text = Loveframes.Create("text")
        text:SetText(tower.data.treasure[i])
        treasures:AddItem(text)
    end

    local saveButton = Loveframes.Create("button", versus)
    saveButton:SetText("Save")
    saveButton:SetPos(5, 240)
    saveButton.OnClick = function()
        settings.versusEvents.onSaved(multiChoice:GetChoice())
    end


    CreateTab("Trials")
    CreateTab("Quest")
    CreateTab("Dark World")
    return settings
end