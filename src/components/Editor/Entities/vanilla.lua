local function playerDraw(editor, self, at, offsetX, offsetY)
    if self.texture then
        if at.x > 320 * 0.5 then
            love.graphics.draw(editor.atlas.image, self.texture.tex, self:getPositionX() + offsetX, self:getPositionY() + offsetY, 0, -1, 1, self.width, 0)
        else
            love.graphics.draw(editor.atlas.image, self.texture.tex, self:getPositionX() + offsetX, self:getPositionY() + offsetY)
        end
    end
end

return function(editor)
    RegisterEntityMetadata("PlayerSpawn", {
        width = 20, height = 20,
        originX = 10, originY = 10,
        texture = "player/statues/greenAlt",
        renderer = function(self, at, offsetX, offsetY) playerDraw(editor, self, at, offsetX, offsetY) end
    })
    RegisterEntityMetadata("TeamSpawn", {
        width = 20, height = 20,
        originX = 10, originY = 10,
        texture = "player/statues/pink",
        renderer = function(self, at, offsetX, offsetY) playerDraw(editor, self, at, offsetX, offsetY) end
    })
    RegisterEntityMetadata("TeamSpawnA", {
        width = 20, height = 20,
        originX = 10, originY = 10,
        texture = "player/statues/blue",
        renderer = function(self, at, offsetX, offsetY) playerDraw(editor, self, at, offsetX, offsetY) end
    })
    RegisterEntityMetadata("TeamSpawnB", {
        width = 20, height = 20,
        originX = 10, originY = 10,
        texture = "player/statues/red",
        renderer = function(self, at, offsetX, offsetY) playerDraw(editor, self, at, offsetX, offsetY) end
    })
    RegisterEntityMetadata("Spawner", {
        width = 20, height = 20,
        originX = 10, originY = 10,
        texture = { name = "spawnPortal", width = 20, height = 20}
    })
    RegisterEntityMetadata("EndlessPortal", {
        width = 50, height = 50,
        originX = 25, originY = 25,
        texture = { name = "nextLevelPortal", width = 50, height = 50}
    })
    RegisterEntityMetadata("TreasureChest", {
        width = 10, height = 10,
        originX = 5, originY = 5,
        texture = { name = "treasureChest", width = 10, height = 10}
    })
    RegisterEntityMetadata("BigTreasureChest", {
        width = 20, height = 20,
        originX = 10, originY = 10,
        texture = { name = "bigChest", width = 20, height = 20}
    })
    RegisterEntityMetadata("BGLantern", {
        width = 10, height = 10,
        originX = 5, originY = 5,
        texture = "details/lantern"
    })
    RegisterEntityMetadata("BGCrystal", {
        width = 10, height = 15,
        originX = 5, originY = 8,
        texture = { name = "details/wallCrystal", width = 10, height = 15 }
    })
    RegisterEntityMetadata("BGSkeleton", {
        width = 20, height = 20,
        originX = 10, originY = 10,
        texture = { name = "details/bones", width = 20, height = 20 }
    })
    RegisterEntityMetadata("FloorMiasma", {
        width = 10, height = 10,
        originX = 0, originY = 5,
        texture = { name = "quest/floorMiasma", width = 10, height = 10},
        renderer = function(self, at, offsetX, offsetY)
            for i = 1, self.width / 10 do
                love.graphics.draw(editor.atlas.image,
                self.texture.tex,
                (self:getPositionX() + (i - 1) * 10) + offsetX,
                self:getPositionY() + offsetY)
            end
        end
    })
    RegisterEntityMetadata("Cobwebs", {
        width = 20, height = 20,
        originX = 10, originY = 10,
        texture = { name = "details/cobwebs", width = 20, height = 20 }
    })
    RegisterEntityMetadata("JumpPad", {
        width = 20, height = 10,
        originX = 0, originY = 0,
        texture = "jumpPadOff",
        renderer = function(self, at, offsetX, offsetY)
            love.graphics.draw(editor.atlas.image,
            self.texture.tex,
            self:getPositionX() + offsetX,
            self:getPositionY() + offsetY, 0, self.width / (20 + 10), 1)
        end
    })
    RegisterEntityMetadata("OrbEd", {
        width = 12, height = 12,
        originX = 6, originY = 6,
        texture = "orb"
    })
    RegisterEntityMetadata("Orb", {
        width = 12, height = 12,
        originX = 6, originY = 6,
        texture = "orb"
    })
    RegisterEntityMetadata("ExplodingOrb", {
        width = 12, height = 12,
        originX = 6, originY = 6,
        texture = { name = "explodingOrb", width = 12, height = 12}
    })
    RegisterEntityMetadata("CrackedWall", {
        width = 20, height = 20,
        originX = 0, originY = 0,
        texture = "crackedWall"
    })
    RegisterEntityMetadata("KingIntro", {
        width = 20, height = 34,
        originX = 10, originY = 17,
        texture = {name = "throneRoom", width = 20, height = 34}
    })

    -- TODO all vanilla entities 
end