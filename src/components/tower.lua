local xmlloader = require("src.utils.xmlloader")

local tower = {
    towerType = "Versus",
    data = {}
}

function tower.loadTower(path)
    local xml = xmlloader.load(path)
    tower.parseVersus(xml.root.tower)
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