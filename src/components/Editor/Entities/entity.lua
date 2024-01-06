local RegisteredEntities = {}

Entity = {}
Entity.__index = Entity

function Entity:new(name, x, y, width, height, originX, originY, texture, id, atlas)
    self = setmetatable({}, Entity)
    self.id = id
    self.name = name
    self.x = x
    self.y = y
    self.width = width or 20
    self.height = height or 20
    self.originX = originX or 0
    self.originY = originY or 0
    if type(texture) == "table" then
        if texture.width or texture.height then
            self.texture = { tex = atlas:newTextureQuad(texture.name, texture.width, texture.height),
                flip = texture.flip }
        else
            self.texture = { tex = atlas:getTexture(texture.name).quad, canFlip = texture.canFlip }
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
    if self.colliding then
        if button == 2 then
            return false
        end

        if button == 1 and editor.toolType == 1 then
            editor:selectEntity(self)
            self.lastHoldX = (self:getPositionX() - ((x - 130) / 2)) + self.originX
            self.lastHoldY = (self:getPositionY() - ((y - 90) / 2)) + self.originY
            self.holding = true
        end
    end

    return true
end

function Entity:mousereleased(x, y, button)
    self.holding = false
end

function Entity:imageDraw(offsetX, offsetY, atlas)
    if self.texture then
        if self.texture.canFlip and self.x > 320 * 0.5 then
            love.graphics.draw(atlas, self.texture.tex, self:getPositionX() + offsetX, self:getPositionY() + offsetY, 0, -1, 1, self.width, 0)
        else
            love.graphics.draw(atlas, self.texture.tex, self:getPositionX() + offsetX, self:getPositionY() + offsetY)
        end
    end
end

function Entity:draw(atlas)
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
    local entity = {
        width = o.width,
        height = o.height,
        originX = o.originX,
        originY = o.originY,
        texture = o.texture,
        attributes = o.attributes or {}
    }
    RegisteredEntities[name] = entity
end

function GetEntityMetadata(name)
    local entity = RegisteredEntities[name]
    return entity
end

function GetAllMetadatas()
    return RegisteredEntities
end

return Entity
