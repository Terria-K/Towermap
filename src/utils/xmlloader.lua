local handler = require("src.lib.xml2lua.xmlhandler.tree")
local xml2lua = require("src.lib.xml2lua.xml2lua")

local xmlloader = {}

function xmlloader.load(file)
    local str = love.filesystem.read(file)
    return xmlloader.parse(str)
end

function xmlloader.parse(str)
    local xmlHandler = handler:new()
    local parser = xml2lua.parser(xmlHandler)
    parser:parse(str)
    return xmlHandler
end

return xmlloader