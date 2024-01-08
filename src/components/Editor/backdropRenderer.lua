local theme = require("src.components.theme")


local backdropRenderer = {
    textures = {},
    atlas = {}
}


function backdropRenderer.textures:createElement(tex, x, y, opacity, w, h)
    local texture
    if not w and not h then
        texture = backdropRenderer.atlas:getTexture(tex).quad
    else
        texture = backdropRenderer.atlas:newTextureQuad(tex, w, h)
    end
    x = x or 0
    y = y or 0
    opacity = opacity or 1
    table.insert(self, { x = x, y = y, tex = texture, opacity = opacity })
end

function backdropRenderer:setTheme(backdropName, atlas)
    for _ = 1, #backdropRenderer.textures do
        table.remove(self.textures, 1)
    end
    self.atlas = atlas
    theme[backdropName].backdropRender(self.textures)
end

function backdropRenderer:draw()
    for i = 1, #self.textures do
        local layer = self.textures[i]
        love.graphics.setColor(1, 1, 1, layer.opacity)
        love.graphics.draw(self.atlas.image, layer.tex, layer.x, layer.y)
        love.graphics.setColor(1, 1, 1, 1)
    end
end

return backdropRenderer