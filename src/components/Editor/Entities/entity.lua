local tableutils = require("src.utils.tableutils")

local RegisteredEntities = {}

Entity = {}
Entity.__index = Entity

function Entity:new(name, x, y, width, height, originX, originY, texture, id, atlas, renderer, nodes)
    self = setmetatable({}, Entity)
    self.id = id
    self.name = name
    self.x = x
    self.y = y
    self.width = width or 20
    self.height = height or 20
    self.originX = originX or 0
    self.originY = originY or 0
    self.renderer = renderer
    self.nodes = nodes
    if type(texture) == "table" then
        if texture.width or texture.height then
            self.texture = { tex = atlas:newTextureQuad(texture.name, texture.width, texture.height) }
        else
            self.texture = { tex = atlas:getTexture(texture.name).quad }
        end
    elseif texture then
        self.texture = { tex = atlas:getTexture(texture).quad }
    end
    self.attributes = {}

    self.colliding = false
    self.holding = false
    self.selected = false
    self.lastHoldX = 0
    self.lastHoldY = 0
    return self
end

function Entity:getPositionX()
    return self.x - self.originX
end

function Entity:getPositionY()
    return self.y - self.originY
end

function Entity:update(x, y)
    if x >= self:getPositionX() and y >= self:getPositionY() and
        x < self:getPositionX() + self.width and y < self:getPositionY() + self.height then
        self.colliding = true
    else
        self.colliding = false
    end

    if self.holding then
        local snapX = math.floor((x + self.lastHoldX) / 5) * 5
        local snapY = math.floor((y + self.lastHoldY) / 5) * 5
        self.x = snapX % 320
        self.y = snapY % 240
    end
end

function Entity:mousepressed(x, y, button, editor)
    if not self.colliding then
        return true
    end

    if button == 2 then
        return false
    end

    if button == 1 and editor.toolType == 1 then
        editor:selectEntity(self)
        self.lastHoldX = (self:getPositionX() - ((x - 130) / 2)) + self.originX
        self.lastHoldY = (self:getPositionY() - ((y - 90) / 2)) + self.originY
        self.holding = true
    end

    return true
end

function Entity:mousereleased(x, y, button, editor)
    self.holding = false
    editor.unsaved = true
end

function Entity:imageDraw(offsetX, offsetY, atlas)
    if self.renderer then
        self.renderer(self, { x = self.x, y = self.y }, offsetX, offsetY)
        return
    end
    if self.texture then
        love.graphics.draw(atlas, self.texture.tex, self:getPositionX() + offsetX, self:getPositionY() + offsetY)
    end
end

function Entity:draw(atlas)
    if self.nodes then
        if not self.selected then
            love.graphics.setColor(1, 1, 1, 0.2)
        end
        love.graphics.setLineStyle("smooth")
        love.graphics.setLineWidth(0.1)
        local index = 0
        while index <= #self.nodes do
            local pointA
            if index == 0 then
                pointA = { x = self:getPositionX(), y = self:getPositionY()}
            else
                pointA = self.nodes[index]
            end
            local pointB = self.nodes[index + 1]
            if not pointB then
                pointB = self.nodes[1]
            end
            love.graphics.line(pointA.x, pointA.y, pointB.x, pointB.y)
            index = index + 1
        end
        love.graphics.setLineWidth(1)
        love.graphics.setLineStyle("rough")
        love.graphics.setColor(1, 1, 1, 1)
    end
    local color
    if self.selected then
        color = {
            r = 0x20/255.0,
            g = 0xdf/255.0,
            b = 0x60/255.0,
            a = 0.5
        }
    else
        color = {
            r = 0xf1/255.0,
            g = 0xe0/255.0,
            b = 0x0e/255.0,
            a = 0.3
        }
    end
    if self:getPositionY() > (240 - self.height) then
        love.graphics.setColor(color.r, color.g, color.b, color.a)
        love.graphics.rectangle("fill", self:getPositionX(), self:getPositionY() - 240, self.width, self.height)
        love.graphics.setColor(1, 1, 1, 1)
        self:imageDraw(0, -240, atlas)
    end
    if self:getPositionX() > (320 - self.width) then
        love.graphics.setColor(color.r, color.g, color.b, color.a)
        love.graphics.rectangle("fill", self:getPositionX() - 320, self:getPositionY(), self.width, self.height)
        love.graphics.setColor(1, 1, 1, 1)
        self:imageDraw(-320, 0, atlas)
    end
    if self:getPositionX() < (self.width - 0) then
        love.graphics.setColor(color.r, color.g, color.b, color.a)
        love.graphics.rectangle("fill", self:getPositionX() + 320, self:getPositionY(), self.width, self.height)
        love.graphics.setColor(1, 1, 1, 1)
        self:imageDraw(320, 0, atlas)
    end
    if self:getPositionY() < self.height - 0 then
        love.graphics.setColor(color.r, color.g, color.b, color.a)
        love.graphics.rectangle("fill", self:getPositionX(), self:getPositionY() + 240, self.width, self.height)
        love.graphics.setColor(1, 1, 1, 1)
        self:imageDraw(0, 240, atlas)
    end
    love.graphics.setColor(color.r, color.g, color.b, color.a)
    love.graphics.rectangle("fill", self:getPositionX(), self:getPositionY(), self.width, self.height)
    love.graphics.setColor(1, 1, 1, 1)
    self:imageDraw(0, 0, atlas)
    love.graphics.setColor(color.r, color.g, color.b, color.a)
    if self.colliding then
        if self:getPositionY() - 3 > (240 - self.height + 6) then
            love.graphics.rectangle("fill", self:getPositionX() - 3, self:getPositionY() - 240 - 3, self.width + 6, self.height + 6)
        end
        if self:getPositionX() - 3 > (320 - self.width + 6) then
            love.graphics.rectangle("fill", self:getPositionX() - 320 - 3, self:getPositionY() - 3, self.width + 6, self.height + 6)
        end
        if self:getPositionX() - 3 < (self.width + 6) then
            love.graphics.rectangle("fill", self:getPositionX() + 320 - 3, self:getPositionY() - 3, self.width + 6, self.height + 6)
        end
        if self:getPositionY() - 3 < self.height + 6 then
            love.graphics.rectangle("fill", self:getPositionX() - 3, self:getPositionY() + 240 - 3, self.width + 6, self.height + 6)
        end
        love.graphics.rectangle("fill", self:getPositionX() - 3, self:getPositionY() - 3, self.width + 6, self.height + 6)
    end
    love.graphics.setColor(1, 1, 1, 1)
end

function RegisterEntityMetadata(name, o)
    o.attributes = o.attributes or {}
    RegisteredEntities[name] = o
end

function GetEntityMetadata(name)
    local entity = RegisteredEntities[name]
    return entity
end

function GetCopyEntityMetadata(name)
    local entity = RegisteredEntities[name]
    return tableutils.deepcopy(entity)
end

function GetAllMetadatas()
    return RegisteredEntities
end

return Entity
