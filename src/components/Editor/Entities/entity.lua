local RegisteredEntities = {}

Entity = {}
Entity.__index = Entity

function Entity:new(x, y, width, height, originX, originY)
    self = setmetatable({}, Entity)
    self.x = x
    self.y = y
    self.width = width or 20
    self.height = height or 20
    self.originX = originX or 0
    self.originY = originY or 0

    self.colliding = false
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
end

function Entity:mousepressed(x, y, button)
    if self.colliding and button == 2 then
        return false
    end
    return true
end

function Entity:mousereleased(x, y, button)

end

function Entity:draw()
    if self.colliding then
        love.graphics.setColor(0, 255, 0, 255)
    end
    love.graphics.rectangle("line", self:getPositionX(), self:getPositionY(), self.width, self.height)
    if self.colliding then
        love.graphics.setColor(255, 255, 255, 255)
    end
end

function RegisterEntityMetadata(name, o)
    local entity = { width = o.width, height = o.height, originX = o.originX, originY = o.originY }
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
