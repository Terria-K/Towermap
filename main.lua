local nfd = require("nfd")
require("src.utils.fs")
require("src.utils.dump")
Loveframes = require("LoveFrames.loveframes")
local editor = require("src.components.Editor.editor")
local toolbarTool = require("src.components.Editor.toolbar")
local ToolButton = require("src.components.toolbutton")
local Entities = require("src.components.Layer.Entities.entities")
local Layer = require("src.components.Layer.layer")
local LayerButton = require("src.components.Layer.layerbutton")
local Settings = require("src.components.Settings.settings")
local history  = require("src.components.Editor.history")


local width = 960;
local entityList


function love.load()
    love.graphics.setDefaultFilter("nearest", "nearest")
    editor:init()
    Loveframes.SetActiveSkin("Dark blue")

    toolbarTool:init()

    toolbarTool:addItem("Pen")
    toolbarTool:addItem("Rect")
    toolbarTool:addItem("HSym", function()
        editor:horizontalSymmetry()
    end)
    toolbarTool:addItem("VSym", function()
        editor:verticalSymmetry()
    end)

    local toolbar = Loveframes.Create("panel")
    toolbar:SetSize(width, 25)
    toolbar:SetPos(0, 0)


    local levelFrame = Loveframes.Create("frame")
    levelFrame:SetName("Levels")

    levelFrame:SetPos(0, 25)
    levelFrame:SetSize(120, love.graphics.getHeight() - 25)
    levelFrame:SetDraggable(false)
    levelFrame:ShowCloseButton(false)

    FileTree = Loveframes.Create("tree", levelFrame)
    FileTree:SetPos(5, 30)
    FileTree:SetSize(110, love.graphics.getHeight() - 60)
    FileTree.OnSelectNode = function(_, node)
        editor:setLevel("assets/".. node.text)
    end

    editor:setFolder("assets")

    ToolButton("File", 0, 40, {
        { text = "New" },
        {
            text = "Open",
            callback = function()
                local res = nfd.openFolder("")

                editor:clearItems()
                if res then
                    editor:setFolder(res)

                end
            end
        },
        {
            text = "Save",
            callback = function()
                editor:save()
            end
        },
        { text = "Convert .oel to .json" },
    })
    ToolButton("Settings", 40, 80, {
        {
            text = "Map Settings",
            callback = function ()
                editor.shouldDraw = false
                Settings(function() editor.shouldDraw = true end)
            end
        }
    })


    local layer = Layer()

    LayerButton("SolidTiles", layer, function()
         editor:changeLayer("SolidTiles")
         if entityList then
            entityList:Remove()
         end
    end)
    LayerButton("BGTiles", layer, function()
        editor:changeLayer("BGTiles")
        if entityList then
           entityList:Remove()
        end
    end)
    LayerButton("Entities", layer,
    function()
        editor:changeLayer("Entities")
        entityList = Entities(layer, function(name, o)
            editor:setCurrentEntity(name, o)
        end)
    end)
    LayerButton("BG", layer)
    LayerButton("Solids", layer)

    editor.offset = {
        x = 180,
        y = 90
    }
end


function love.update(dt)
    Loveframes.update(dt);
    editor:update()
end

function love.draw()
    editor:draw()
    Loveframes.draw();
end

function love.mousepressed(x, y, button)
	Loveframes.mousepressed(x, y, button)
    editor:mousepressed(x, y, button)
end

function love.mousereleased(x, y, button)
	Loveframes.mousereleased(x, y, button)
    editor:mousereleased(x, y, button)
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