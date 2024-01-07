local worldmouse = require("src.worldmouse")

local tile = {
    started = false,
    x = 0,
    y = 0,
    width = 0,
    height = 0,
    buttonType = 0
}

function tile.start(x, y, button)
    tile.x = x
    tile.y = y
    tile.started = true
    tile.buttonType = button
end

function tile:update(x, y)
    local rx = math.floor(((x - worldmouse.x)  / worldmouse.size) / 10) * 10
    local ry = math.floor(((y - worldmouse.y) / worldmouse.size) / 10) * 10

    local width = self.x - rx
    self.width = -width

    local height = self.y - ry
    self.height = -height
end

function tile:adjustIfNeeded()
    if self.width < 0 then
        self.x = self.x + self.width
        self.width = -self.width
    end

    if self.height < 0 then
        self.y = self.y + self.height
        self.height = -self.height
    end
end

function tile:draw()
    if not self.started then return end
    if self.buttonType == 1 then
        love.graphics.setColor(1, 1, 0, 0.5)
    elseif self.buttonType == 2 then
        love.graphics.setColor(1, 0, 0, 0.5)
    end

    love.graphics.rectangle("fill", self.x, self.y,
        self.width, self.height)
    love.graphics.setColor(1, 1, 1, 1)
end

return tile