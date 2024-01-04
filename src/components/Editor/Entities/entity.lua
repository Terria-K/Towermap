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
    return self
end

function Entity:draw()
    love.graphics.rectangle("line", self.x - self.originX, self.y - self.originY, self.width, self.height)
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
