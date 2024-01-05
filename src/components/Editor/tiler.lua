local handler = require("src.lib.xml2lua.xmlhandler.tree")
local xml2lua = require("src.lib.xml2lua.xml2lua")

Tiler = {}
Tiler.__index = Tiler

function Tiler:new(spritesheet)
    self = setmetatable({}, Tiler)
    self.spritesheet = spritesheet
    self.bit = {
        center = {},
        single = {},
        singleHorizontalLeft = {},
        singleHorizontalCenter = {},
        singleHorizontalRight = {},
        singleVerticalTop = {},
        singleVerticalCenter = {},
        singleVerticalBottom = {},
        top = {},
        bottom = {},
        left = {},
        right = {},
        topLeft = {},
        topRight = {},
        bottomLeft = {},
        bottomRight = {},
        insideTopLeft = {},
        insideTopRight = {},
        insideBottomLeft = {},
        insideBottomRight = {},
        below = {},
    }
    return self
end

function Tiler:init(dataFile, idx)
    local str = love.filesystem.read(dataFile)
    local xmlHandler = handler:new()
    local parser = xml2lua.parser(xmlHandler)
    parser:parse(str)
    local flight = xmlHandler.root.TilesetData.Tileset[idx]
    self:assignBit(flight)
end

local function splitCSV(inputstr, sep)
    if sep == nil then
            sep = "%s"
    end
    local t={}
    for str in string.gmatch(inputstr, "([^"..sep.."]+)") do
            table.insert(t, tonumber(str))
    end
    return t
end

function Tiler:assignBit(tileData)
    self.bit.center = splitCSV(tileData.Center, ',')
    self.bit.right = splitCSV(tileData.Right, ',')
    self.bit.left = splitCSV(tileData.Left, ',')
    self.bit.top = splitCSV(tileData.Top, ',')
    self.bit.bottom = splitCSV(tileData.Bottom, ',')
    self.bit.topLeft = splitCSV(tileData.TopLeft, ',')
    self.bit.topRight = splitCSV(tileData.TopRight, ',')
    self.bit.bottomLeft = splitCSV(tileData.BottomLeft, ',')
    self.bit.bottomRight = splitCSV(tileData.BottomRight, ',')
    self.bit.single = splitCSV(tileData.Single, ',')
    self.bit.insideBottomLeft = splitCSV(tileData.InsideBottomLeft, ',')
    self.bit.insideBottomRight = splitCSV(tileData.InsideBottomRight, ',')
    self.bit.insideTopLeft = splitCSV(tileData.InsideTopLeft, ',')
    self.bit.insideTopRight = splitCSV(tileData.InsideTopRight, ',')
    self.bit.singleHorizontalCenter = splitCSV(tileData.SingleHorizontalCenter, ',')
    self.bit.singleHorizontalLeft = splitCSV(tileData.SingleHorizontalLeft, ',')
    self.bit.singleHorizontalRight = splitCSV(tileData.SingleHorizontalRight, ',')
    self.bit.singleVerticalBottom = splitCSV(tileData.SingleVerticalBottom, ',')
    self.bit.singleVerticalCenter = splitCSV(tileData.SingleVerticalCenter, ',')
    self.bit.singleVerticalTop = splitCSV(tileData.SingleVerticalTop, ',')
end

function Tiler:reset()
    self.tileX = 0
    self.tileY = 0
    self.current = false
    self.left = false
    self.right = false
    self.up = false
    self.down = false
    self.upLeft = false
    self.upRight = false
    self.downLeft = false
    self.downRight = false
end

local function check(x, y, data)
    if not (x <= #data and y < #data[1] and x > 0 and y > 0) then
        return true
    end
    if data[x][y] then
        return true
    end
    return false
end

function Tiler:checkAll(bits)
    self.left = check(self.tileX, self.tileY - 1, bits)
    self.right = check(self.tileX, self.tileY + 1, bits)
    self.up = check(self.tileX - 1, self.tileY, bits)
    self.down = check(self.tileX + 1, self.tileY, bits)
    self.upLeft = check(self.tileX - 1, self.tileY - 1, bits)
    self.upRight = check(self.tileX - 1, self.tileY + 1, bits)
    self.downLeft = check(self.tileX + 1, self.tileY - 1, bits)
    self.downRight = check(self.tileX + 1, self.tileY + 1, bits)
end

function Tiler:checkAllAlso(bits, also)
    self.left = check(self.tileX, self.tileY - 1, bits) or check(self.tileX, self.tileY - 1, also)
    self.right = check(self.tileX, self.tileY + 1, bits) or check(self.tileX, self.tileY + 1, also)
    self.up = check(self.tileX - 1, self.tileY, bits) or check(self.tileX - 1, self.tileY, also)
    self.down = check(self.tileX + 1, self.tileY, bits) or check(self.tileX + 1, self.tileY, also)
    self.upLeft = check(self.tileX - 1, self.tileY - 1, bits) or check(self.tileX - 1, self.tileY - 1, also)
    self.upRight = check(self.tileX - 1, self.tileY + 1, bits) or check(self.tileX - 1, self.tileY + 1, also)
    self.downLeft = check(self.tileX + 1, self.tileY - 1, bits) or check(self.tileX + 1, self.tileY - 1, also)
    self.downRight = check(self.tileX + 1, self.tileY + 1, bits) or check(self.tileX + 1, self.tileY + 1, also)
end

function Tiler:tile(bits, also)
    local bitHeight = #bits
    local bitWidth = #bits[1]

    self.tileX = 1
    while self.tileX <= bitHeight do
        self.tileY = 1
        while self.tileY <= bitWidth do
            self.current = bits[self.tileX][self.tileY]
            if also then
                self:checkAllAlso(bits, also)
            else
                self:checkAll(bits)
            end

            local tiles = self:tileHandle()
            if tiles then
                local idx = tiles[1]
                if idx then
                    self:draw(idx, self.tileY, self.tileX)
                end
            end
            self.tileY = self.tileY + 1
        end

        self.tileX = self.tileX + 1
    end
end
function Tiler:draw(idx, x, y)
    self.spritesheet:draw(x, y, idx)
end



function Tiler:tileHandle()
    if self.current then
        if self.left and self.right and
            self.up and self.down and
            self.upLeft and self.upRight and
            self.downLeft and self.downRight then
            return self.bit.center
        end
        if not self.up and not self.down then
            if self.left and self.right then
                return self.bit.singleHorizontalCenter
            end
            if not self.left and not self.right then
                return self.bit.single
            end
            if self.left then
                return self.bit.singleHorizontalRight
            end
            return self.bit.singleHorizontalLeft
        elseif not self.left and not self.right then
            if self.up and self.down then
                return self.bit.singleVerticalCenter
            end
            if self.down then
                return self.bit.singleVerticalTop
            end
            return self.bit.singleVerticalBottom
        else

            if self.up and self.down and self.left and not self.right then
                return self.bit.right
            end
            if self.up and self.down and not self.left and self.right then
                return self.bit.left
            end
            if self.up and not self.down and not self.left and self.right then
                return self.bit.bottomLeft
            end
            if self.up and not self.down and self.left and not self.right then
                return self.bit.bottomRight
            end
            if not self.up and self.down and not self.left and self.right then
                return self.bit.topLeft
            end
            if not self.up and self.down and self.left and not self.right then
                return self.bit.topRight
            end
            if self.up and self.down and self.downLeft and not self.downRight then
                return self.bit.insideTopLeft
            end
            if self.up and self.down and not self.downLeft and self.downRight then
                return self.bit.insideTopRight
            end
            if self.up and self.down and self.upLeft and not self.upRight then
                return self.bit.insideBottomLeft
            end
            if self.up and self.down and not self.upLeft and self.upRight then
                return self.bit.insideBottomRight
            end
            if not self.down then

                return self.bit.bottom
            end
            return self.bit.top
        end
    else
        if self.up then
            return self.bit.below
        end
    end

    return nil
end

Tiler:reset()

return Tiler