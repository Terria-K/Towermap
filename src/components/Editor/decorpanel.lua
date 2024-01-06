local decorpanel = {
    frame = nil,
    width = 0,
    height = 0,
    quad = nil,
    image = {},
    localPos = nil,
    isHolding = false
}

function decorpanel:init(image, quad, width, height, layerType)
    self.localPos = { x = 0, y = 0, width = 10, height = 10 }
    self.frame = {}
    local frame = Loveframes.Create("frame")
    frame:SetName("Decoration")
    frame:SetPos(130, 80)
    if layerType == 3 then
        frame:SetSize(450, 200)
    else
        frame:SetSize(200, 450)
    end
    frame:ShowCloseButton(false)

    self.frame = frame
    self.quad = quad
    self.image = image
    self.width = width
    self.height = height
    return self
end

function decorpanel:isHovering()
    if self.frame then
        return self.frame.hover
    end
    return false
end

function decorpanel:update(x, y)
    if self.frame == {} or not self:isHovering() then return end
    local fx = self.frame.x + 15
    local fy = self.frame.y + 30
    local rx = math.floor(((x - fx) / 2) / 10) * 10
    local ry = math.floor(((y - fy) / 2) / 10) * 10
    if self.isHolding then
        if rx < 0 or ry < 0
            or rx > self.width or ry > self.height then return end
        local width = self.localPos.x - rx
        if width < 0 then
            self.localPos.width = -width
        end
        local height = self.localPos.y - ry
        if height < 0 then
            self.localPos.height = -height
        end
    end
end

function decorpanel:mousepressed(x, y, button)
    if self.frame == {} or not self:isHovering() then return end
    local fx = self.frame.x + 15
    local fy = self.frame.y + 30
    local rx = math.floor(((x - fx) / 2) / 10) * 10
    local ry = math.floor(((y - fy) / 2) / 10) * 10

    if rx < 0 or ry < 0 
        or rx > self.width - 10 or ry > self.height - 10
    then return end

    self.localPos = {
        x = rx,
        y = ry,
        width = 10,
        height = 10
    }
    self.isHolding = true
end

function decorpanel:mousereleased(x, y, button)
    self.isHolding = false
end

function decorpanel:getIndex()
    return self:getIndexWithPos(self.localPos.x, self.localPos.y)
end

function decorpanel:getIndexWithPos(x, y)
    return ((y / 10) * (self.width / 10) + (x / 10))
end

function decorpanel:getIndexes()
    local width = self.localPos.width / 10
    local height = self.localPos.height / 10
    local indexes = {}
    for x = 1, width do
        indexes[x] = {}
        for y = 1, height do
            local rx = self.localPos.x + ((x - 1) * 10)
            local ry = self.localPos.y + ((y - 1) * 10)
            indexes[x][y] = self:getIndexWithPos(rx, ry)
        end
    end
    return indexes
end

function decorpanel:draw()
    if not self.frame then return end
    local x = self.frame.x + 15
    local y = self.frame.y + 30

    love.graphics.draw(self.image, self.quad, x, y, 0, 2, 2)
    love.graphics.setColor(1, 1, 0, 1)
    if self.localPos.width == 0 then
        self.localPos.width = 10
    end
    if self.localPos.height == 0 then
        self.localPos.height = 10
    end
    love.graphics.rectangle("line", x + self.localPos.x * 2, y + self.localPos.y * 2,
        self.localPos.width * 2, self.localPos.height * 2)
    love.graphics.setColor(1, 1, 1, 1)
end

function decorpanel:destroy()
    if self.frame then
        self.frame:Remove()
        self.frame = nil
    end
end

return decorpanel