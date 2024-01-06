--[[
Copyright (c) 2014 Wolfgang Hauptfleisch <dev@augmentedlogic.com>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
--]]


    _M = {}
    local mt = { __index = {} }

    _M.CDATA = 1
    _M.FORMAT_PLAIN = 2
    _M.FORMAT_LINEBREAKS = 3


    local function escape(s)
       s = string.gsub(s, "&", "&amp;")
       s = string.gsub(string.gsub(s, ">", "&gt;"), "<", "&lt;");
       s = string.gsub(string.gsub(s, "\"", "&quot;"), "\'", "&apos;")
    return s
    end

    ---
    -- 
    function _M.new()
        local object = setmetatable({ doc = { elements = {} , cur = 0 } },  mt)
    return object
    end


    function mt.__index:startDocument(version, encoding)
        self.doc['version'] = version or "1.0"
        self.doc['encoding'] = encoding or "utf-8"
    end


    function mt.__index:startElement(name)
        table.insert(self.doc.elements, { name = name, pos = "start" } )
    end

    function mt.__index:startCloseElement(name)
        table.insert(self.doc.elements, { name = name, pos = "startClose" } )
    end

    function mt.__index:singleElement(name)
        table.insert(self.doc.elements, { name = name, pos = "singleElement" } )
    end


    function mt.__index:addAttribut(attrname, attrvalue)
        local a = { attrname, escape(attrvalue) }
        local p = #self.doc.elements
        if not self.doc.elements[p].attr then
            self.doc.elements[p].attr = {}
        end
        table.insert(self.doc.elements[p].attr, a ) 
    end


    function mt.__index:closeElement(name)
        print(name)
        table.insert(self.doc.elements, { name = name, pos = "close" } )
    end


    function mt.__index:writeElement(name, value, cdata)
         local cadata = cdata or 0
         if cdata == 1 then
             value = "<![CDATA["..value.."]]>"
         else
             value = escape(value)
         end
         table.insert(self.doc.elements, { name = name, value = value, pos = "single" } )
    end

    function mt.__index:writeValue(value)
        local p = #self.doc.elements
        self.doc.elements[p].value = value
    end


    function render(doc_t, format)

        format = format or 2

        local t = {}
        local cur = 0

        table.insert(t, '<?xml version="'..doc_t['version']..'" encoding="'..doc_t['encoding']..'"?>') 

        for i, element in ipairs(doc_t.elements) do
            if element.pos == "startClose" then
                if element.attr then
                    local at = {}
                        for a,v in ipairs(element.attr) do
                            table.insert(at, v[1]..'="'..v[2]..'"')
                        end 
                    attributes = table.concat(at, " ")
                else
                    attributes = ""
                end

                --
                if element.attr then
                    if element.value then
                        table.insert(t, "<"..element.name.." "..attributes..">".. element.value.."</"..element.name..">")
                    else
                        table.insert(t, "<"..element.name.." "..attributes..">".."</"..element.name..">")
                    end
                else
                    if element.value then
                        table.insert(t, "<"..element.name..">".. element.value.."</"..element.name..">")
                    else
                        table.insert(t, "<"..element.name..">".."</"..element.name..">")
                    end
                end
            end

            if element.pos == "start" then

                if element.attr then
                    local at = {}
                        for a,v in ipairs(element.attr) do
                            table.insert(at, v[1]..'="'..v[2]..'"')
                        end 
                    attributes = table.concat(at, " ")
                else
                    attributes = ""
                end

                --
                if element.attr then
                    if element.value then
                        table.insert(t, "<"..element.name.." "..attributes..">".. element.value)
                    else
                        table.insert(t, "<"..element.name.." "..attributes..">")
                    end
                else
                    if element.value then
                        table.insert(t, "<"..element.name..">".. element.value)
                    else
                        table.insert(t, "<"..element.name..">")
                    end
                end

          elseif element.pos == "close" then

              table.insert(t, "</"..element.name..">")

          elseif element.pos == "single" then
              table.insert(t, "<"..element.name..">"..element.value.."</"..element.name..">")

          elseif element.pos == "singleElement" then
            if element.attr then
                local at = {}
                    for a,v in ipairs(element.attr) do
                        table.insert(at, v[1]..'="'..v[2]..'"')
                    end 
                attributes = table.concat(at, " ")
            else
                attributes = ""
            end

            --
            if element.attr then
                table.insert(t, "<"..element.name.." "..attributes.."/>")
            else
                table.insert(t, "<"..element.name.."/>")
            end
          end

       end

        local xmlstring

         if format == 3 then
             xmlstring = table.concat(t, "\n")
         else
             xmlstring = table.concat(t);
         end

    return xmlstring
    end


    function mt.__index:write(path, format)
        local file = io.open(path, "w")
        if file then
            file:write(render(self.doc, format))
            file:close()
        end
    end

    function mt.__index:get(format)
         return render(self.doc, format)
    end

    function mt.__index:flush()
        self.doc = {}
    end

return _M