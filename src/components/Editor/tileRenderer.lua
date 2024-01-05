local tileRenderer = {}

function tileRenderer:init()
    self.framebuffer = love.graphics.newCanvas(320, 240)
end

function tileRenderer:beginDraw()
    love.graphics.setCanvas(self.framebuffer)
    love.graphics.clear()
end

function tileRenderer:draw()
    love.graphics.draw(self.framebuffer, 0, 0)
end

function tileRenderer:endDraw()
    love.graphics.setCanvas()
end

return tileRenderer