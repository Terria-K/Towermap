local backdropRenderer = {
    tex = {},
    atlas = {}
}

function backdropRenderer:init(backdropName, atlas)
    self.tex = atlas:getTexture("daySky")
    self.atlas = atlas
end

function backdropRenderer:draw()
    love.graphics.draw(self.atlas.image, self.tex.quad, 0, 0)
end

return backdropRenderer