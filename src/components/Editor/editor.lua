local lfs = require("src.lib.lfs.lfs")
local tinyxmlwriter = require("src.lib.lua-tinyxmlwriter.tinyxmlwriter")
local xml2lua = require("src.lib.xml2lua.xml2lua")
local json = require("src.lib.json.json")
local handler = require("src.lib.xml2lua.xmlhandler.tree")
local Entity = require("src.components.Editor.Entities.entity")
local RegisterVanilla = require("src.components.Editor.Entities.vanilla")
local history = require("src.components.Editor.history")
local Tiler = require("src.components.Editor.tiler")
local Spritesheet = require("src.components.spritesheet")
local mousePressed = false
local mouseButton = 0

local function stringToArray(str)
    local arr = {}
    local x = 1
    local y = 1
    arr[y] = {}
    for c in str:gmatch"." do
        if c == '\n' then
            x = 1
            y = y + 1
            arr[y] = {}
        elseif c == '0' then
            arr[y][x] = false
            x = x + 1
        else
            arr[y][x] = true
            x = x + 1
        end
    end
    return arr
end

local function arrayToString(arr)
    local str = ""
    for x = 1, 24 do
        for y = 1, 32 do
            if arr[x][y] then
                str = str .. "1"
            else
                str = str .. "0"
            end
        end
        str = str .. "\n"
    end
    return str
end


local editor = {
    solids = {},
    bgs = {},
    entities = {},
    bgTiles = {},
    solidTiles = {},
    items = {},
    framebuffer = {},
    currentTile = {},
    xml = {},
    filename = "",
    shouldDraw = true,
    layerType = 0,
    currentEntity = {},
    solidTiler = {},
    bgTiler = {}
}

function editor:init()
    self.framebuffer = love.graphics.newCanvas(320, 240)

    for x = 1, 24 do
        self.bgs[x] = {}
        self.solids[x] = {}
        for y = 1, 32 do
            self.bgs[x][y] = false
            self.solids[x][y] = false
        end
    end
    RegisterVanilla()
    local solidSpritesheet = Spritesheet:new("assets/flight.png")
    self.solidTiler = Tiler:new(solidSpritesheet)
    self.solidTiler:init("assets/tilesetData.xml", 7)
    local bgSpritesheet = Spritesheet:new("assets/flightBG.png")
    self.bgTiler = Tiler:new(bgSpritesheet)
    self.bgTiler:init("assets/tilesetData.xml", 8)
end

function editor:setFolder(folder)
    local items = {}
    for file in lfs.dir(folder) do
        if file:sub(-#"oel") == "oel" or file:sub(-#"json") == "json" then
            table.insert(items, file)
        end
    end
    self.items = items
    for i = 1, #self.items do
        FileTree:AddNode(self.items[i])
    end
end

function editor:clearItems()
    for _ = 1, #self.items do
        table.remove(self.items, 1)
    end
    for i = 1, #FileTree.children do
        FileTree:RemoveNode(1)
    end
end

function editor:setLevel(file)
    local str = love.filesystem.read(file)
    if not FileExists(file) then
        for i = 1, #self.items do
            if self.items[i] == file then
                table.remove(self.items, i)
            end
        end
        return
    end
    if file:sub(-#"oel") == "oel" then
        local xmlHandler = handler:new()
        local parser = xml2lua.parser(xmlHandler)
        parser:parse(str)
        self.solids = stringToArray(xmlHandler.root.level.Solids[1])
        self.bgs = stringToArray(xmlHandler.root.level.BG[1])
        self.solidTiles = xmlHandler.root.level.SolidTiles[1]
        self.bgTiles = xmlHandler.root.level.BGTiles[1]
        self.xml = xmlHandler.root
    else
        local decoded = json.decode(str)
        self.xml = {}
        self.solids = stringToArray(decoded.layers.Solids)
        self.bgs = stringToArray(decoded.layers.BG)
    end
    self.currentTile = self.solids
    self.filename = file
end

local function save_xml(xml, filename)
    local str = xml
    local fs = io.open(filename, "w")
    if fs then
        fs:write(str)
        fs:close()
    end
end

function editor:save()
    local xml = tinyxmlwriter:new()
    local solidStr = arrayToString(self.solids)
    local bgStr = arrayToString(self.bgs)
    local filename = self.filename

    xml:startDocument("1.0", "utf-8")
    xml:startElement("level")
        xml:startElement("Solids")
        xml:addAttribut("exportMode", "BitString")
        xml:writeValue(solidStr)
        xml:closeElement("Solids")

        xml:startElement("BG")
        xml:addAttribut("exportMode", "BitString")
        xml:writeValue(bgStr)
        xml:closeElement("BG")

        xml:startElement("BGTiles")
        xml:addAttribut("exportMode", "TrimmedCSV")
        xml:writeValue(self.xml.level.BGTiles[1])
        xml:closeElement("BGTiles")

        xml:startElement("SolidTiles")
        xml:addAttribut("exportMode", "TrimmedCSV")
        xml:writeValue(self.xml.level.SolidTiles[1])
        xml:closeElement("SolidTiles")
    xml:closeElement("level")

    local output = xml:get(tinyxmlwriter.FORMAT_LINEBREAKS)

    save_xml(output, filename)
    xml:flush()
end


function editor:drawArray(arr, atlas, quad)
    love.graphics.setColor(255, 255, 255, 1)
    for x = 1, #arr, 1 do
        for y = 1, #arr[x], 1 do
            if arr[x][y] then
                love.graphics.draw(atlas, quad, (y - 1) * 10, (x - 1) * 10)
            end
        end
    end
    love.graphics.setColor(255, 255, 255)

end

function editor:draw()
    love.graphics.setCanvas(self.framebuffer)
    love.graphics.clear()
    love.graphics.setColor(255, 255, 255, 0.3)
    love.graphics.setLineWidth(1)
    love.graphics.setLineStyle("rough")
    for i = 1, 24 do
        love.graphics.line(0, i * 10, 320, i * 10)
    end
    for i = 1, 32 do
        love.graphics.line(i * 10, 0, i * 10, 240)
    end
    love.graphics.setColor(255, 255, 255, 1)
    -- self:drawArray(self.solids, self.atlas, solidQuad)
    self.bgTiler:tile(self.bgs, self.solids)
    self.solidTiler:tile(self.solids)

    for i = 1, #self.entities do
        self.entities[i]:draw()
    end
    if self.layerType == 2 and self.currentEntity.width then
        local rx = (love.mouse.getX() - 130) / 2
        local ry = (love.mouse.getY() - 90) / 2
        local snapX = math.floor(rx / 5) * 5
        local snapY = math.floor(ry / 5) * 5
        local width = self.currentEntity.width
        local height = self.currentEntity.height
        love.graphics.rectangle("line",
        snapX - self.currentEntity.originX, snapY - self.currentEntity.originY,
        width, height)
    end

    love.graphics.setCanvas()

    love.graphics.draw(self.framebuffer, 130, 90, 0, 2)
end

function editor:changeLayer(layerName)
    if layerName == "SolidTiles" then
        self.currentTile = self.solids
        self.layerType = 0
    elseif layerName == "BGTiles" then
        self.currentTile = self.bgs
        self.layerType = 1
    elseif layerName == "Entities" then
        self.layerType = 2
    end
end

function editor:setCurrentEntity(name, o)
    self.currentEntity = o
end

function editor:mousepressed(x, y, button)
    if not mousePressed and self.shouldDraw then
        local rx = (x - 130) / 2
        local ry = (y - 90) / 2

        if not (rx < 0 or ry < 0 or rx > 320 or ry > 240) then
            if self.layerType == 2 and button == 1 then
                self:addEntity(rx, ry)
            end
            if self.layerType == 0 or self.layerType == 1 then
                history:pushCommit(self.layerType, self.currentTile)
            end
        end
    end

    mousePressed = true
    mouseButton = button

    if self.layerType == 2 then
        local toRemove = { }
        for i = 1, #self.entities do
            local shouldStay = self.entities[i]:mousepressed(x, y, button)
            if not shouldStay then
                table.insert(toRemove, i)
            end
        end
        for k, v in ipairs(toRemove) do
            table.remove(self.entities, v)
        end
    end
end

function editor:mousereleased(x, y, button)
    mousePressed = false
    mouseButton = button

    if self.layerType == 2 then
        for i = 1, #self.entities do
            self.entities[i]:mousereleased(x, y, button)
        end
    end
end

local saved = false
local undo = false

function editor:update()
    if love.keyboard.isDown("lctrl") then
        if love.keyboard.isDown("s") then
            if not saved then
                saved = true
                self:save()
            end
        elseif love.keyboard.isDown("z") and not undo and not mousePressed then
            if history.currentCommit ~= 0 then
                local commit = history:popCommit()
                if commit.layerType == 0 then
                    self.solids = commit.tiles
                    if self.layerType == 0 then
                        self.currentTile = self.solids
                    end
                elseif commit.layerType == 1 then
                    self.bgs = commit.tiles
                    if self.layerType == 1 then
                        self.currentTile = self.bgs
                    end
                end
                undo = true
            end
        elseif undo and not love.keyboard.isDown("z") then
            undo = false
        end
    else
        if saved then
            saved = false
        end
        if undo then
            undo = false
        end
    end
    local x = love.mouse.getX()
    local y = love.mouse.getY()

    if self.layerType == 2 then
        for i = 1, #self.entities do
            self.entities[i]:update((x - 130) / 2, (y - 90) / 2)
        end
    end

    if (self.layerType == 0 or self.layerType == 1) and self.shouldDraw and mousePressed then
        x = math.floor((x - 130) / (10 * 2) + 1)
        y = math.floor((y - 90) / (10 * 2) + 1)
        if x <= 0 or y <= 0 or x > 40 or y > 24 then
            return
        end
        if mouseButton == 1 then
            self:placeTile(x, y)
        elseif mouseButton == 2 then
            self:removeTile(x, y)
        end
    end
end

function editor:addEntity(x, y)
    local snapX = math.floor(x / 5) * 5
    local snapY = math.floor(y / 5) * 5
    local ent = Entity:new(snapX, snapY,
        self.currentEntity.width,
        self.currentEntity.height,
        self.currentEntity.originX,
        self.currentEntity.originY)
    table.insert(self.entities, #self.entities + 1, ent)
end

function editor:placeTile(x, y)
    if self.currentTile and self.currentTile[1] then
        self.currentTile[y][x] = true
    end
end

function editor:removeTile(x, y)
    if self.currentTile and self.currentTile[1] then
        self.currentTile[y][x] = false
    end
end

function editor:horizontalSymmetry()
    history:pushCommit(self.layerType, self.currentTile)
    for y = 1, 24 do
        for x = 1, 32 * 0.5 do
            self.currentTile[y][33 - x] = self.currentTile[y][x]
        end
    end
end

function editor:verticalSymmetry()
    history:pushCommit(self.layerType, self.currentTile)
    for y = 1, 24 * 0.5 do
        for x = 1, 32 do
            self.currentTile[25 - y][x] = self.currentTile[y][x]
        end
    end
end

return editor