require("src.utils.dump")
Loveframes = require("LoveFrames.loveframes")
local ToolButton = require("src.components.toolbutton")
local Entities = require("src.components.Layer.Entities.entities")
local Layer = require("src.components.Layer.layer")
local LayerButton = require("src.components.Layer.layerbutton")
local Settings = require("src.components.Settings.settings")


love.window.setMode(960, 640)
local width = 960;

function love.load()
    Loveframes.SetActiveSkin("Dark blue")

    local toolbar = Loveframes.Create("panel")
    toolbar:SetSize(width, 25)
    toolbar:SetPos(0, 0)

    ToolButton("File", 0, 40, {
        { text = "New" },
        { text = "Open" },
        { text = "Save" },
        { text = "Save As" }
    })
    ToolButton("Settings", 40, 80, {
        {
            text = "Map Settings",
            callback = function ()
                Settings()
            end
        }
    })

    local levelFrame = Loveframes.Create("frame")
    levelFrame:SetName("Levels")

    levelFrame:SetPos(0, 25)
    levelFrame:SetSize(120, love.graphics.getHeight() - 25)
    levelFrame:SetDraggable(false)
    levelFrame:ShowCloseButton(false)

    local tree = Loveframes.Create("tree", levelFrame)
    tree:SetPos(5, 30)
    tree:SetSize(110, love.graphics.getHeight() - 60)
    tree:AddNode("00.oel")
    tree:AddNode("01.oel")
    tree:AddNode("02.oel")
    tree:AddNode("03.oel")

    local layer = Layer()

    LayerButton("SolidTiles", layer)
    LayerButton("BackgroundTiles", layer)
    LayerButton("Entities", layer)
    LayerButton("BG", layer)
    LayerButton("Solids", layer)

    Entities(layer)
end


function love.update(dt)
    Loveframes.update(dt);
end

function love.draw()
    Loveframes.draw();
end

function love.mousepressed(x, y, button)
	Loveframes.mousepressed(x, y, button)
end

function love.mousereleased(x, y, button)
	Loveframes.mousereleased(x, y, button)
end

function love.wheelmoved(x, y)
	Loveframes.wheelmoved(x, y)
end

function love.keypressed(key, isrepeat)
	Loveframes.keypressed(key, isrepeat)
	if key == "f1" then
		local debug = Loveframes.config["DEBUG"]
		Loveframes.config["DEBUG"] = not debug
    end
end

function love.keyreleased(key)
	Loveframes.keyreleased(key)
end

function love.textinput(text)
	Loveframes.textinput(text)
end