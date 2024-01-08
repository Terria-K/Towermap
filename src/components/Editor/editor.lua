local lfs = require("src.lib.lfs.lfs")
local tinyxmlwriter = require("src.lib.lua-tinyxmlwriter.tinyxmlwriter")
local Entity = require("src.components.Editor.Entities.entity")
local RegisterVanilla = require("src.components.Editor.Entities.vanilla")
local history = require("src.components.Editor.history")
local Tiler = require("src.components.Editor.tiler")
local Decor = require("src.components.Editor.decor")
local Atlas = require("src.components.atlas")
local decorpanel = require("src.components.Editor.decorpanel")
local backdropRenderer = require("src.components.Editor.backdropRenderer")
local tileRenderer = require("src.components.Editor.tileRenderer")
local arrayUtils = require("src.utils.arrayutils")
local fs = require("src.utils.fs")
local tile = require("src.components.Editor.tile")
local theme = require("src.components.theme")
local worldmouse = require("src.worldmouse")
local xmlloader = require("src.utils.xmlloader")
local xml2lua = require("src.lib.xml2lua.xml2lua")
local mousePressed = false
local mouseButton = 0

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
    themeName = "",
    currentSelectedEntity = nil,
    onSelectEntity = nil,
    onUnselectEntity = nil,
    dirty = true,
    unsaved = false
}

function editor:init()
    self.framebuffer = love.graphics.newCanvas(worldmouse.width, worldmouse.height)
    tileRenderer:init()

    for x = 1, 24 do
        self.bgs[x] = {}
        self.solids[x] = {}
        for y = 1, 32 do
            self.bgs[x][y] = false
            self.solids[x][y] = false
        end
    end
    RegisterVanilla(self)
    local atlas = love.graphics.newImage("assets/atlas.png")
    local xmlAtlas = xmlloader.load("assets/atlas.xml")

    local bgAtlas = love.graphics.newImage("assets/bgAtlas.png")
    local xmlBgAtlas = xmlloader.load("assets/bgAtlas.xml")

    self.bgAtlas = Atlas:new(xmlBgAtlas, bgAtlas)
    self.atlas = Atlas:new(xmlAtlas, atlas)
end

function editor:setTheme(themeName)
    self.themeName = themeName
    self.dirty = true
    local t = theme[themeName]

    local solidSpritesheet = self.atlas:createSpriteSheet(t.solidTiles, self.atlas.image)
    self.solidTiler = Tiler:new(solidSpritesheet)
    self.solidTiler:init("assets/tilesetData.xml", t.tileIndex * 2 - 1)
    self.solidDecor = Decor:new(solidSpritesheet)
    local bgSpritesheet = self.atlas:createSpriteSheet(t.bgTiles, self.atlas.image)
    self.bgTiler = Tiler:new(bgSpritesheet)
    self.bgTiler:init("assets/tilesetData.xml", t.tileIndex * 2)
    self.bgDecor = Decor:new(bgSpritesheet)
    backdropRenderer:setTheme(self.themeName, self.bgAtlas)
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
    if not fs.fileExists(file) then
        for i = 1, #self.items do
            if self.items[i] == file then
                table.remove(self.items, i)
            end
        end
        return
    end

    local xmlHandler = xmlloader.parse(str)

    self.solids = arrayUtils.stringToArray(xmlHandler.root.level.Solids[1])
    self.bgs = arrayUtils.stringToArray(xmlHandler.root.level.BG[1])
    if xmlHandler.root.level.SolidTiles[1] then
        self.solidTiles = arrayUtils.stringCSVToArray(xmlHandler.root.level.SolidTiles[1])
    end
    if xmlHandler.root.level.BGTiles[1] then
        self.bgTiles = arrayUtils.stringCSVToArray(xmlHandler.root.level.BGTiles[1])
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
    local function nodeToArrVec2(nodes)
        local arr = {}
        for i = 1, #nodes do
            local attr = nodes[i]._attr
            arr[i] = { x = tonumber(attr.x), y = tonumber(attr.y) }
        end
        return arr
    end

    local function assignAndAdd(entity)
        if entity.node then
            self.currentEntity.nodes = nodeToArrVec2(entity.node)
        end
        self:assignAttributes(entity)
        self:addEntity(entity._attr.x, entity._attr.y, self.currentEntity.id)
    end

    for k, v in pairs(xml) do
        local metadata = GetCopyEntityMetadata(k)
        if metadata then
            self.currentEntity = metadata
            self.currentEntity.name = k
            if v[1] then
                for i = 1, #v do
                    local entity = v[i]
                    assignAndAdd(entity)
                end
            elseif v then
                local entity = v
                assignAndAdd(entity)
            else
                print("Missing entity: '" .. k .. "'")
            end
        end
    end
end

function editor:save()
    local xml = tinyxmlwriter:new()
    local solidStr = arrayUtils.arrayToString(self.solids)
    local bgStr = arrayUtils.arrayToString(self.bgs)
    local bgTilesStr = arrayUtils.arrayToStringCSV(self.bgTiles)
    local solidTilesStr = arrayUtils.arrayToStringCSV(self.solidTiles)
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
                if v.nodes then
                    xml:startElement(v.name)
                else
                    xml:singleElement(v.name)
                end
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
                if v.nodes then
                    for _, n in ipairs(v.nodes) do
                        xml:singleElement("node")
                        xml:addAttribut("x", n.x)
                        xml:addAttribut("y", n.y)
                    end
                    xml:closeElement(v.name)
                end
            end
        xml:closeElement("Entities")
    xml:closeElement("level")

    local output = xml:get(tinyxmlwriter.FORMAT_LINEBREAKS)

    local file = io.open(filename, "w")
    if file then
        file:write(output)
        file:close()
    end
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
    love.graphics.setColor(255, 255, 255, 0.1)
    love.graphics.setLineWidth(1)
    love.graphics.setLineStyle("rough")
    for i = 1, worldmouse.height / 10 do
        love.graphics.line(0, i * 10, worldmouse.width, i * 10)
    end
    for i = 1, worldmouse.width / 10 do
        love.graphics.line(i * 10, 0, i * 10, worldmouse.height)
    end
    love.graphics.setColor(255, 255, 255, 1)

    tileRenderer:draw()
    self.bgDecor:draw(self.bgTiles)
    self.solidDecor:draw(self.solidTiles)

    for i = 1, #self.entities do
        self.entities[i]:draw(self.atlas.image)
    end
    if self.toolType == 0 and self.layerType == 2 and self.currentEntity.width then
        local mousePos = worldmouse.getMouseWorldCoords()
        local snap = worldmouse.snapCoords(mousePos.x, mousePos.y, 5)
        local width = self.currentEntity.width
        local height = self.currentEntity.height
        love.graphics.rectangle("line",
        snap.x - self.currentEntity.originX, snap.y - self.currentEntity.originY,
            width, height)
    end

    tile:draw()

    love.graphics.setCanvas()

    love.graphics.draw(self.framebuffer, worldmouse.x, worldmouse.y, 0, worldmouse.size)
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
        local atlas = self.atlas:getTexture(theme[self.themeName].bgTiles)
        decorpanel:init(self.atlas.image, atlas.quad, atlas.width, atlas.height, self.layerType)
    elseif layerName == "Solids" then
        decorpanel:destroy()
        self.layerType = 4
        local atlas = self.atlas:getTexture(theme[self.themeName].solidTiles)
        decorpanel:init(self.atlas.image, atlas.quad, atlas.width, atlas.height, self.layerType)
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
        local mousePos = worldmouse.getMouseWorldCoords()

        if editor.isInWorldBounds(mousePos.x, mousePos.y) then
            if self.toolType == 0 and self.layerType == 2 and button == 1 then
                self:addEntity(mousePos.x, mousePos.y)
                self.toolType = 1
            end
            if self.layerType == 0 or self.layerType == 1 then
                history:pushCommit(self.layerType, self.currentTile)
            elseif self.layerType == 3 then
                history:pushCommit(self.layerType, self.bgTiles)
            elseif self.layerType == 4 then
                history:pushCommit(self.layerType, self.solidTiles)
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

    if tile.started then
        tile.started = false
        tile:adjustIfNeeded()

        local dx = 0
        while dx < tile.width / 10 do
            local dy = 0
            while dy < tile.height / 10 do
                local gridX = worldmouse.toGrid(tile.x) + dx + 1
                local gridY = worldmouse.toGrid(tile.y) + dy + 1
                if editor.isInGridBounds(gridX, gridY) then
                    if mouseButton == 1 then
                        editor:placeTile(gridX, gridY)
                    elseif mouseButton == 2 then
                        editor:removeTile(gridX, gridY)
                    end
                end

                dy = dy + 1
            end
            dx = dx + 1
        end
        tile.width = 0
        tile.height = 0
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
                elseif commit.layerType == 3 then
                    self.bgTiles = commit.tiles
                elseif commit.layerType == 4 then
                    self.solidTiles = commit.tiles
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
            self.entities[i]:update((x - worldmouse.x) / worldmouse.size, (y - worldmouse.y) / worldmouse.size)
        end
    end

    if tile.started then
        tile:update(x, y)
    end

    if (self.layerType == 0 or self.layerType == 1 or self.layerType == 3 or self.layerType == 4)
        and self.shouldDraw and not self.modalOpen and mousePressed then
        local gridX = math.floor((x - worldmouse.x) / (10 * worldmouse.size) + 1)
        local gridY = math.floor((y - worldmouse.y) / (10 * worldmouse.size) + 1)
        if not self.isInGridBounds(gridX, gridY) then
            return
        end

        if self.toolType == 1 then
            if not tile.started then
                local snap = worldmouse.snapCoords((x - worldmouse.x) / worldmouse.size, 
                (y - worldmouse.y) / worldmouse.size, 10)
                tile.start(snap.x, snap.y, mouseButton)
            end
        else
            if mouseButton == 1 and (self.layerType == 0 or self.layerType == 1) then
                    self:placeTile(gridX, gridY)
            elseif mouseButton == 1 and (self.layerType == 3 or self.layerType == 4) then
                local idxs = decorpanel:getIndexes()
                for idx = 1, #idxs do
                    for idy = 1, #idxs[1] do
                        self:placeTileIndex(gridX + idx - 1, gridY + idy - 1, idxs[idx][idy])
                    end
                end
            elseif mouseButton == 2 then
                self:removeTile(gridX, gridY)
            end
        end
    end
    decorpanel:update(x, y)
end

function editor:addEntity(x, y, id)
    local snap = worldmouse.snapCoords(x, y, 5)
    local ent = Entity:new(self.currentEntity.name, snap.x, snap.y,
        self.currentEntity.width,
        self.currentEntity.height,
        self.currentEntity.originX,
        self.currentEntity.originY,
        self.currentEntity.texture,
        id or self.currentID, self.atlas, 
        self.currentEntity.renderer,
        self.currentEntity.nodes)
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
    if not editor.isInGridBounds(x, y) then return end
    if self.layerType == 3 then
        self.bgTiles[x][y] = i
    elseif self.layerType == 4 then
         self.solidTiles[x][y] = i
    end
end

function editor:horizontalSymmetry()
    editor.dirty = true
    history:pushCommit(self.layerType, self.currentTile)
    print(worldmouse.height / 10)
    for y = 1, worldmouse.height / 10 do
        for x = 1, worldmouse.width / 10 * 0.5 do
            self.currentTile[y][(worldmouse.width / 10 + 1) - x] = self.currentTile[y][x]
        end
    end

    self.unsaved = true
end

function editor:verticalSymmetry()
    editor.dirty = true
    history:pushCommit(self.layerType, self.currentTile)
    for y = 1, worldmouse.height / 10 * 0.5 do
        for x = 1, worldmouse.width / 10 do
            self.currentTile[(worldmouse.height / 10 + 1) - y][x] = self.currentTile[y][x]
        end
    end

    self.unsaved = true
end

function editor.isInWorldBounds(x, y) 
    if x < 0 or y < 0 or x > worldmouse.width or y > worldmouse.height then
        return false
    end
    return true
end

function editor.isInGridBounds(x, y)
    if x < 1 or y < 1 or x > worldmouse.width / 10 or y > worldmouse.height / 10 then
        return false
    end
    return true
end

return editor