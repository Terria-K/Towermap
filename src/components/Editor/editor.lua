local lfs = require("src.lib.lfs.lfs")
local tinyxmlwriter = require("src.lib.lua-tinyxmlwriter.tinyxmlwriter")
local xml2lua = require("src.lib.xml2lua.xml2lua")
local handler = require("src.lib.xml2lua.xmlhandler.tree")
local Entity = require("src.components.Editor.Entities.entity")
local RegisterVanilla = require("src.components.Editor.Entities.vanilla")
local history = require("src.components.Editor.history")
local Tiler = require("src.components.Editor.tiler")
local Decor = require("src.components.Editor.decor")
local Atlas = require("src.components.atlas")
local decorpanel = require("src.components.Editor.decorpanel")
local backdropRenderer = require("src.components.Editor.backdropRenderer")
local tileRenderer = require("src.components.Editor.tileRenderer")
local mousePressed = false
local mouseButton = 0

local function stringCSVToArray(str)
    local arr = {}

    for x1 = 1, 32 do
        arr[x1] = {}
        for y1 =1, 24 do
            arr[x1][y1] = -1
        end
    end
    local rows = {}
    for line in str:gmatch("[^\n]*\n?") do
        table.insert(rows, line);
    end

    local y = 1
    while y <= 24 and y <= #rows do
        local nums = SplitCSVToNumber(rows[y], ',')
        local x = 1
        while x <= 32 and x <= #nums do
            local num = tonumber(nums[x])
            if num then
                arr[x][y] = num
            end
            x = x + 1
        end
        y = y + 1
    end
    return arr
end

local function arrayToStringCSV(arr)
    local str = ""
    for y = 1, 24 do
        for x = 1, 32 do
            str = str .. arr[x][y]
            if x ~= 32 then
                str = str .. ','
            end
        end
        str = str .. '\n'
    end
    return str
end

local function stringToArray(str)
    str = str:match'^%s*(.*)'
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
        if x ~= 24 then
            str = str .. "\n"
        end
    end
    str = str:match'^(.*%S)%s*$'
    return str
end


local editor = {
    -- Layers
    solids = {},
    bgs = {},
    entities = {},
    bgTiles = {},
    solidTiles = {},
    -- Editor
    items = {},
    framebuffer = {},
    currentTile = {},
    xml = {},
    filename = "",
    shouldDraw = true,
    layerType = 0,
    currentEntity = {},
    solidTiler = {},
    bgTiler = {},
    solidDecor = {},
    bgDecor = {},
    toolType = 0,
    currentID = 0,
    atlas = {},
    bgAtlas = {},
    currentSelectedEntity = nil,
    onSelectEntity = nil,
    onUnselectEntity = nil,
    dirty = true,
    unsaved = false
}

function editor:init()
    self.framebuffer = love.graphics.newCanvas(320, 240)
    tileRenderer:init()

    for x = 1, 24 do
        self.bgs[x] = {}
        self.solids[x] = {}
        for y = 1, 32 do
            self.bgs[x][y] = false
            self.solids[x][y] = false
        end
    end
    RegisterVanilla()
    local imageAtlas = love.graphics.newImage("assets/atlas.png")
    local str = love.filesystem.read("assets/atlas.xml")
    local xmlHandler = handler:new()
    local parser = xml2lua.parser(xmlHandler)
    parser:parse(str)

    local bgAtlas = love.graphics.newImage("assets/bgAtlas.png")
    local str2 = love.filesystem.read("assets/bgAtlas.xml")
    local xmlBgAtlas = handler:new()
    local parser2 = xml2lua.parser(xmlBgAtlas)
    parser2:parse(str2)

    self.bgAtlas = Atlas:new(xmlBgAtlas, bgAtlas)
    backdropRenderer:init("Flight", self.bgAtlas)

    self.atlas = Atlas:new(xmlHandler, imageAtlas)
    local solidSpritesheet = self.atlas:createSpriteSheet("tilesets/flight", imageAtlas)
    self.solidTiler = Tiler:new(solidSpritesheet)
    self.solidTiler:init("assets/tilesetData.xml", 7)
    self.solidDecor = Decor:new(solidSpritesheet)
    local bgSpritesheet = self.atlas:createSpriteSheet("tilesets/flightBG", imageAtlas)
    self.bgTiler = Tiler:new(bgSpritesheet)
    self.bgTiler:init("assets/tilesetData.xml", 8)
    self.bgDecor = Decor:new(bgSpritesheet)

end

function editor:setFolder(folder)
    local items = {}
    for file in lfs.dir(folder) do
        if file:sub(-#"oel") == "oel" then
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
    history:clearHistory()
    self.dirty = true
    self:removeAllEntities()
    local str = love.filesystem.read(file)
    if not FileExists(file) then
        for i = 1, #self.items do
            if self.items[i] == file then
                table.remove(self.items, i)
            end
        end
        return
    end
    local xmlHandler = handler:new()
    local parser = xml2lua.parser(xmlHandler)
    parser:parse(str)

    self.solids = stringToArray(xmlHandler.root.level.Solids[1])
    self.bgs = stringToArray(xmlHandler.root.level.BG[1])
    if xmlHandler.root.level.SolidTiles[1] then
        self.solidTiles = stringCSVToArray(xmlHandler.root.level.SolidTiles[1])
    end
    if xmlHandler.root.level.BGTiles[1] then
        self.bgTiles = stringCSVToArray(xmlHandler.root.level.BGTiles[1])
    end
    self.xml = xmlHandler.root
    self:addAllEntities(xmlHandler.root.level.Entities)

    self.currentTile = self.solids
    self.filename = file
end

function editor:removeAllEntities()
    for _ = 1, #self.entities do
        table.remove(self.entities, 1)
    end
end

function editor:assignAttributes(entity)
    for k, v in pairs(entity._attr) do
        if k == "id" then
            if entity._attr.id - 0 > self.currentID then
                self.currentID = tonumber(entity._attr.id)
            end
            self.currentEntity.id = tonumber(v)
        elseif k == "width" then
            self.currentEntity.width = entity._attr.width
        elseif k == "height" then
            self.currentEntity.height = entity._attr.height
        else
            self.currentEntity.attributes[k] = v
        end
    end
end

function editor:addAllEntities(xml)
    for k, v in pairs(xml) do
        local metadata = GetEntityMetadata(k)
        if metadata then
            self.currentEntity = metadata
            self.currentEntity.name = k
            if v[1] then
                for i = 1, #v do
                    local entity = v[i]
                    self:assignAttributes(entity)
                    self:addEntity(entity._attr.x, entity._attr.y, self.currentEntity.id)
                end
            elseif v then
                local entity = v
                self:assignAttributes(entity)
                self:addEntity(entity._attr.x, entity._attr.y, self.currentEntity.id)
            else
                print("Missing entity: '" .. k .. "'")
            end
        end
    end
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
    local bgTilesStr = arrayToStringCSV(self.bgTiles)
    local solidTilesStr = arrayToStringCSV(self.solidTiles)
    local filename = self.filename

    xml:startDocument("1.0", "utf-8")
    xml:startElement("level")
        xml:startCloseElement("Solids")
        xml:addAttribut("exportMode", "Bitstring")
        xml:writeValue(solidStr)

        xml:startCloseElement("BG")
        xml:addAttribut("exportMode", "Bitstring")
        xml:writeValue(bgStr)

        xml:startCloseElement("BGTiles")
        xml:addAttribut("exportMode", "TrimmedCSV")
        xml:writeValue(bgTilesStr)

        xml:startCloseElement("SolidTiles")
        xml:addAttribut("exportMode", "TrimmedCSV")
        xml:writeValue(solidTilesStr)

        xml:startElement("Entities")
            for _, v in ipairs(self.entities) do
                xml:singleElement(v.name)
                xml:addAttribut("id", v.id)
                xml:addAttribut("x", v.x)
                xml:addAttribut("y", v.y)
                xml:addAttribut("width", v.width)
                xml:addAttribut("height", v.height)
                for k2, v2 in pairs(v.attributes) do
                    if not (k2 == "x" or k2 == "y") then
                        xml:addAttribut(k2, v2)
                    end
                end
            end
        xml:closeElement("Entities")
    xml:closeElement("level")

    local output = xml:get(tinyxmlwriter.FORMAT_LINEBREAKS)

    save_xml(output, filename)
    xml:flush()
    self.unsaved = false
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
    if self.dirty then
        tileRenderer:beginDraw()
        self.bgTiler:draw(self.bgs, self.solids)
        self.solidTiler:draw(self.solids)
        tileRenderer:endDraw()
        self.dirty = false
    end

    love.graphics.setCanvas(self.framebuffer)
    love.graphics.clear()
    backdropRenderer:draw()
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

    tileRenderer:draw()
    self.bgDecor:draw(self.bgTiles)
    self.solidDecor:draw(self.solidTiles)

    for i = 1, #self.entities do
        self.entities[i]:draw(self.atlas.image)
    end
    if self.toolType == 0 and self.layerType == 2 and self.currentEntity.width then
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

function editor:afterdraw()
    decorpanel:draw()
end

function editor:changeLayer(layerName)
    if layerName == "SolidTiles" then
        self.currentTile = self.solids
        self.layerType = 0
        self:unselectEntity()
        decorpanel:destroy()
    elseif layerName == "BGTiles" then
        self.currentTile = self.bgs
        self.layerType = 1
        self:unselectEntity()
        decorpanel:destroy()
    elseif layerName == "Entities" then
        self.layerType = 2
        decorpanel:destroy()
    elseif layerName == "BG" then
        decorpanel:destroy()
        self.layerType = 3
        local flightBGAtlas = self.atlas:getTexture("tilesets/flightBG")
        decorpanel:init(self.atlas.image, flightBGAtlas.quad, flightBGAtlas.width, flightBGAtlas.height, self.layerType)
    elseif layerName == "Solids" then
        decorpanel:destroy()
        self.layerType = 4
        local flightAtlas = self.atlas:getTexture("tilesets/flight")
        decorpanel:init(self.atlas.image, flightAtlas.quad, flightAtlas.width, flightAtlas.height, self.layerType)
    end
end

function editor:setCurrentEntity(name, o)
    self.currentEntity = o
    self.currentEntity.name = name
end

function editor:selectEntity(entity)
    self:unselectEntity()
    entity.selected = true
    self.currentSelectedEntity = entity
    if self.onSelectEntity then
        self.onSelectEntity(entity)
    end
end

function editor:unselectEntity()
    if self.onUnselectEntity then
        self.onUnselectEntity(self.currentSelectedEntity)
    end
    if self.currentSelectedEntity then
        self.currentSelectedEntity.selected = false
        self.currentSelectedEntity = nil
    end
end

function editor:mousepressed(x, y, button)
    if button == 3 then
        print(Dump(self.entities))
    end
    if not mousePressed and self.shouldDraw then
        local rx = (x - 130) / 2
        local ry = (y - 90) / 2

        if not (rx < 0 or ry < 0 or rx > 320 or ry > 240) then
            if self.toolType == 0 and self.layerType == 2 and button == 1 then
                self:addEntity(rx, ry)
                self.toolType = 1
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
            local shouldStay = self.entities[i]:mousepressed(x, y, button, self)
            if not shouldStay then
                table.insert(toRemove, i)
                break
            end
        end
        for _, v in ipairs(toRemove) do
            table.remove(self.entities, v)
        end
    end
    decorpanel:mousepressed(x, y, button)
end

function editor:mousereleased(x, y, button)
    mousePressed = false
    mouseButton = button

    if self.layerType == 2 then
        for i = 1, #self.entities do
            self.entities[i]:mousereleased(x, y, button, self)
        end
    end

    decorpanel:mousereleased(x, y, button)
end

local saved = false
local undo = false

function editor:update()
    if not mousePressed then
        self.shouldDraw = not decorpanel:isHovering()
    end
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
                self.dirty = true
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

    if (self.layerType == 0 or self.layerType == 1 or self.layerType == 3 or self.layerType == 4) 
        and self.shouldDraw and mousePressed then
        x = math.floor((x - 130) / (10 * 2) + 1)
        y = math.floor((y - 90) / (10 * 2) + 1)
        if x <= 0 or y <= 0 or x > 40 or y > 24 then
            return
        end
        if mouseButton == 1 and (self.layerType == 0 or self.layerType == 1) then
            self:placeTile(x, y)
        elseif mouseButton == 1 and (self.layerType == 3 or self.layerType == 4) then
            local idxs = decorpanel:getIndexes()
            for idx = 1, #idxs do
                for idy = 1, #idxs[1] do
                    self:placeTileIndex(x + idx - 1, y + idy - 1, idxs[idx][idy])
                end
            end
        elseif mouseButton == 2 then
            self:removeTile(x, y)
        end
    end
    decorpanel:update(x, y)
end

function editor:addEntity(x, y, id)
    local snapX = math.floor(x / 5) * 5
    local snapY = math.floor(y / 5) * 5
    local ent = Entity:new(self.currentEntity.name, snapX, snapY,
        self.currentEntity.width,
        self.currentEntity.height,
        self.currentEntity.originX,
        self.currentEntity.originY,
        self.currentEntity.texture,
        id or self.currentID, self.atlas)
    ent.attributes = self.currentEntity.attributes
    table.insert(self.entities, #self.entities + 1, ent)
    self.currentID = self.currentID + 1
    self.unsaved = true
end

function editor:placeTile(x, y)
    if (self.layerType == 0 or self.layerType == 1) and self.currentTile and self.currentTile[1] and not self.currentTile[y][x] then
        self.currentTile[y][x] = true
        self.dirty = true
        self.unsaved = true
    end
end

function editor:removeTile(x, y)
    if (self.layerType == 0 or self.layerType == 1) and self.currentTile and self.currentTile[1] and self.currentTile[y][x] then
        self.currentTile[y][x] = false
        self.dirty = true
        self.unsaved = true
    elseif self.layerType == 3 then
        self.bgTiles[x][y] = -1
    elseif self.layerType == 4 then
         self.solidTiles[x][y] = -1
    end
end

function editor:placeTileIndex(x, y, i)
    if x > 32 or x < 1 or y > 24 or y < 1 then return end
    if self.layerType == 3 then
        self.bgTiles[x][y] = i
    elseif self.layerType == 4 then
         self.solidTiles[x][y] = i
    end
end

function editor:horizontalSymmetry()
    editor.dirty = true
    history:pushCommit(self.layerType, self.currentTile)
    for y = 1, 24 do
        for x = 1, 32 * 0.5 do
            self.currentTile[y][33 - x] = self.currentTile[y][x]
        end
    end
    self.unsaved = true
end

function editor:verticalSymmetry()
    editor.dirty = true
    history:pushCommit(self.layerType, self.currentTile)
    for y = 1, 24 * 0.5 do
        for x = 1, 32 do
            self.currentTile[25 - y][x] = self.currentTile[y][x]
        end
    end
    self.unsaved = true
end

return editor