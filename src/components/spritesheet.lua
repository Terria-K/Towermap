Spritesheet = {}
Spritesheet.__index = Spritesheet

function Spritesheet:new(image)
    self = setmetatable({}, Spritesheet)
    self.image = love.graphics.newImage(image)
    self.quads = {}
    self.height = self.image:getHeight()
    self.width = self.image:getWidth()

    local i = 0
    local tileY = 1
    while tileY <= self.height / 10 do
        local tileX = 1
        while tileX <= self.width / 10 do
            self.quads[i] = love.graphics.newQuad(tileX * 10 - 10, tileY * 10 - 10, 10, 10, self.image)
            tileX = tileX + 1
            i = i + 1
        end
        tileY = tileY + 1
    end
    return self
end

function Spritesheet.getQuadByIndex(self, idx)
    if idx > -1 then
        return self.quads[idx]
    end
    return nil
end

function Spritesheet.draw(self, x, y, idx)
    local quad = self:getQuadByIndex(idx)
    love.graphics.draw(self.image, quad, x * 10 - 10, y * 10 - 10)
end

return Spritesheet