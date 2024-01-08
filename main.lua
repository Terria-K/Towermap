local nfd = require("nfd")
require("src.utils.fs")
require("src.utils.dump")
require("src.utils.stringutils")
Loveframes = require("LoveFrames.loveframes")
local editor = require("src.components.Editor.editor")
local toolbarTool = require("src.components.Editor.toolbar")
local ToolButton = require("src.components.toolbutton")
local Entities = require("src.components.Layer.Entities.entities")
local Layer = require("src.components.Layer.layer")
local LayerButton = require("src.components.Layer.layerbutton")
local Settings = require("src.components.Settings.settings")
local tower = require("src.components.tower")


local width = 960;
local entityList
local entityData

local modalOpen = false

function love.load()
    tower.loadTower("assets/tower.xml")
    love.graphics.setDefaultFilter("nearest", "nearest")
    editor:init()
    editor:setTheme(tower.data.theme)
    Loveframes.SetActiveSkin("Dark blue")

    toolbarTool:init()

    toolbarTool:addItem("Pen (1)", function()
        editor.toolType = 0
    end)
    toolbarTool:addItem("Rect (2)", function()
        editor.toolType = 1
    end)
    toolbarTool:addItem("HSym (h)", function()
        editor:horizontalSymmetry()
    end)
    toolbarTool:addItem("VSym (v)", function()
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
    FileTree:SetSize(110, love.graphics.getHeight() - 400)
    FileTree.OnSelectNode = function(_, node)
        editor:setLevel("assets/".. node.text)
    end

    editor:setFolder("assets")

    local toggleGrid = Loveframes.Create("checkbox")
    toggleGrid:SetPos(400, 5)
    toggleGrid:SetText("Toggle Grid")
    toggleGrid:SetChecked(true)
    toggleGrid.OnChanged = function(toggle)
        local check = toggle:GetChecked()
        editor.config.toggleGrid = check
    end

    ToolButton("File", 0, 40, {
        { text = "New" },
        {
            text = "Open",
            callback = function()
                local res = nfd.openFolder("")

                if res then
                    editor:clearItems()
                    editor:setFolder(res)
                end
            end
        },
        {
            text = "Save",
            callback = function()
                editor:save()
            end
        }
    })
    ToolButton("Settings", 40, 80, {
        {
            text = "Map Settings",
            callback = function ()
                modalOpen = true
                local settings = Settings(tower, function() modalOpen = false end)

                settings.versusEvents.onSaved = function(choice)
                    tower.data.theme = choice
                    editor:setTheme(choice)
                    tower.save()
                end
            end
        }
    })
    ToolButton("Level", love.graphics.getWidth() - 80, 80, {
        {
            text = "Settings",
            callback = function ()
                modalOpen = true
            end
        }
    })


    local layer = Layer()

    LayerButton("SolidTiles (q)", layer, function()
         editor:changeLayer("SolidTiles")
         if entityList then
            entityList:Remove()
            entityList = nil
         end
    end)
    LayerButton("BGTiles (w)", layer, function()
        editor:changeLayer("BGTiles")
        if entityList then
           entityList:Remove()
           entityList = nil
        end
    end)
    LayerButton("Entities (e)", layer,
    function()
        editor:changeLayer("Entities")
        if entityList then
            return
        end
        entityList = Entities(layer, function(name, o)
            editor:setCurrentEntity(name, o)
        end)

    end)
    LayerButton("BG (r)", layer,
    function()
        editor:changeLayer("BG")
        if entityList then
           entityList:Remove()
           entityList = nil
        end
    end)
    LayerButton("Solids (t)", layer,
    function()
        editor:changeLayer("Solids")
        if entityList then
           entityList:Remove()
           entityList = nil
        end
    end)

    editor.offset = {
        x = 180,
        y = 90
    }
    editor.onSelectEntity = function(entity)
        if entityData then
            entityData:Remove()
            entityData = nil
        end
        entityData = Loveframes.Create("panel", layer.frame)
        entityData:SetPos(5, 360)
        entityData:SetSize(170, 250)

        local idText = Loveframes.Create("text", entityData)
        idText:SetPos(5, 5)
        idText:SetText("ID: " .. entity.id)

        local posText = Loveframes.Create("text", entityData)
        posText:SetPos(5, 20)
        posText:SetText("X: " .. entity.x .. " Y: " .. entity.y)

        local uiWidth = 20

        uiWidth = uiWidth + 20
        local ewidth = Loveframes.Create("text", entityData)
        ewidth:SetPos(5, uiWidth)
        ewidth:SetText("Width: " .. entity.width)

        uiWidth = uiWidth + 20
        local widthSlider = Loveframes.Create("slider", entityData)
        widthSlider:SetPos(5, uiWidth)
        widthSlider:SetMinMax(10, 320)
        widthSlider:SetWidth(165)
        widthSlider:SetScrollIncrease(10)
        widthSlider:SetScrollDecrease(10)
        widthSlider:SetValue(editor.currentSelectedEntity.width - 0)
        widthSlider.OnValueChanged = function(o, v)
            local value = math.floor(v / 10) * 10
            editor.currentSelectedEntity.width = value
        end

        uiWidth = uiWidth + 20
        local eheight = Loveframes.Create("text", entityData)
        eheight:SetPos(5, uiWidth)
        eheight:SetText("Height: " .. entity.height)

        uiWidth = uiWidth + 20
        local heightSlider = Loveframes.Create("slider", entityData)
        heightSlider:SetPos(5, uiWidth)
        heightSlider:SetMinMax(10, 240)
        heightSlider:SetWidth(165)
        heightSlider:SetScrollIncrease(10)
        heightSlider:SetScrollDecrease(10)
        heightSlider:SetValue(editor.currentSelectedEntity.height - 0)
        heightSlider.OnValueChanged = function(o, v)
            local value = math.floor(v / 10) * 10
            editor.currentSelectedEntity.height = value
        end
    end
    editor.onUnselectEntity = function(entity)
        if entityData then
            entityData:Remove()
            entityData = nil
        end
    end
end


function love.update(dt)
    Loveframes.update(dt);
    if not modalOpen then
        editor:update()
    end
end

function love.draw()
    if not modalOpen then
        editor:draw()
    end
    Loveframes.draw();
    if not modalOpen then
        editor:afterdraw()
    end
end

function love.mousepressed(x, y, button)
	Loveframes.mousepressed(x, y, button)
    if not modalOpen then
        editor:mousepressed(x, y, button)
    end
end

function love.mousereleased(x, y, button)
	Loveframes.mousereleased(x, y, button)
    if not modalOpen then
        editor:mousereleased(x, y, button)
    end
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
    if not modalOpen then
        if key == '1' then
            editor.toolType = 0
        end
        if key == '2' then
            editor.toolType = 1
        end
        if key == 'h' then
            editor:horizontalSymmetry()
        end
        if key == 'v' then
            editor:verticalSymmetry()
        end
    end
	Loveframes.keyreleased(key)
end

function love.textinput(text)
	Loveframes.textinput(text)
end