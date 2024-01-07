local worldmouse = {
    x = 130,
    y = 90,
    -- might be useful for custom set
    width = 320,
    height = 240,
    --
    size = 2
}

function worldmouse.getMouseWorldCoords()
    local x = worldmouse.getMouseWorldCoordsX()
    local y = worldmouse.getMouseWorldCoordsY()

    return {
        x = x,
        y = y
    }
end

function worldmouse.getMouseWorldCoordsX()
    local x = (love.mouse.getX() - worldmouse.x) / worldmouse.size

    return x
end

function worldmouse.getMouseWorldCoordsY()
    local y = (love.mouse.getY() - worldmouse.y) / worldmouse.size

    return y
end

function worldmouse.toGrid(num)
    return math.floor(num / (5 * worldmouse.size))
end

function worldmouse.snapCoords(x, y, snapSize)
    local snapX = math.floor(x / snapSize) * snapSize
    local snapY = math.floor(y / snapSize) * snapSize

    return {
        x = snapX,
        y = snapY
    }
end

function worldmouse.snap(num, snapSize)
    local snap = math.floor(num / snapSize) * snapSize

    return snap
end

return worldmouse