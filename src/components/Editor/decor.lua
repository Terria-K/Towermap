Decor = {}
Decor.__index = Decor

function Decor:new(spritesheet)
    self = setmetatable({}, Decor)
    self.spritesheet = spritesheet
    return self
end

function Decor:draw(decor)
    for y = 1, #decor do
        for x = 1, #decor[1] do
            local idx = decor[y][x]
            if idx ~= -1 then
                self.spritesheet:draw(y, x, idx)
            end
        end
    end
end

return Decor