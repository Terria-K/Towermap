local tinyxmlwriter = require("src.lib.lua-tinyxmlwriter.tinyxmlwriter")
local xmlloader = require("src.utils.xmlloader")

local tower = {
    towerType = "Versus",
    data = {},
    path = ""
}

function tower.loadTower(path)
    local xml = xmlloader.load(path)
    tower.parseVersus(xml.root.tower)
    tower.path = path
end

function tower.save()
    -- versus
    local xml = tinyxmlwriter:new()
    xml:startDocument("1.0", "utf-8")
    xml:startElement("tower")
        xml:startCloseElement("theme")
        xml:writeValue(tower.data.theme)
        xml:startCloseElement("treasure")
        print(tower.data.arrowRates)
        xml:addAttribut("arrows", tower.data.arrowRates)
        local result = ""
        for i = 1, #tower.data.treasure do
            local treasure = tower.data.treasure[i]
            local treatext
            if i ~= #tower.data.treasure then
                treatext = treasure .. ','
            else
                treatext = treasure
            end
            result = result .. treatext
        end

        xml:writeValue(result)
    xml:closeElement("tower")
    local output = xml:get(tinyxmlwriter.FORMAT_LINEBREAKS)

    local fs = io.open(tower.path, "w")
    if fs then
        fs:write(output)
        fs:close()
    end
    xml:flush()
end


function tower.parseVersus(xml)
    local treasure
    local arrowRates
    if xml.treasure[1] then
        treasure = xml.treasure[1]
        arrowRates = xml.treasure._attr.arrows
    else
        treasure = xml.treasure
    end
    treasure = SplitCSV(treasure, ',')
    tower.data = {
        theme = xml.theme,
        treasure = treasure,
        arrowRates = arrowRates
    }
end

return tower