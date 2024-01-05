Atlas = {}
Atlas.__index = Atlas

function Atlas:new(xml, texture)
    self = setmetatable({}, Atlas)
    self.textures = {}
    self.image = texture

    for _, v in ipairs(xml.root.TextureAtlas.SubTexture) do
        local name = v._attr.name
        local x = v._attr.x
        local y = v._attr.y
        local width = v._attr.width
        local height = v._attr.height

        local quad = love.graphics.newQuad(x, y, width, height, texture)

        self.textures[name] = {
            x = x, y = y,
            width = width, height = height,
            quad = quad
        }
    end

    return self
end

function Atlas:getTexture(name)
    return self.textures[name]
end

function Atlas:newTextureQuad(name, width, height)
    local tex = self.textures[name]
    local quad = love.graphics.newQuad(tex.x, tex.y, width or tex.width, height or tex.height, self.image)
    return quad
end

function Atlas:createSpriteSheet(name, image)
    local texture = self.textures[name]
    local sheet = Spritesheet:new(image, texture.x, texture.y, texture.width, texture.height)
    return sheet
end

return Atlas