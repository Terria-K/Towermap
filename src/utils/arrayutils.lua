local arrayutils = {}


function arrayutils.stringCSVToArray(str)
    local arr = {}

    for x1 = 1, 32 do
        arr[x1] = {}
        for y1 =1, 24 do
            arr[x1][y1] = -1
        end
    end
    local rows = {}
    for line in str:gmatch("[^\n]*\n?") do
        table.insert(rows, line);
    end

    local y = 1
    while y <= 24 and y <= #rows do
        local nums = SplitCSVToNumber(rows[y], ',')
        local x = 1
        while x <= 32 and x <= #nums do
            local num = tonumber(nums[x])
            if num then
                arr[x][y] = num
            end
            x = x + 1
        end
        y = y + 1
    end
    return arr
end

function arrayutils.arrayToStringCSV(arr)
    local str = ""
    for y = 1, 24 do
        for x = 1, 32 do
            str = str .. arr[x][y]
            if x ~= 32 then
                str = str .. ','
            end
        end
        str = str .. '\n'
    end
    return str
end

function arrayutils.stringToArray(str)
    str = str:match'^%s*(.*)'
    local arr = {}
    local x = 1
    local y = 1
    arr[y] = {}
    for c in str:gmatch"." do
        if c == '\n' then
            x = 1
            y = y + 1
            arr[y] = {}
        elseif c == '0' then
            arr[y][x] = false
            x = x + 1
        else
            arr[y][x] = true
            x = x + 1
        end
    end
    return arr
end

function arrayutils.arrayToString(arr)
    local str = ""
    for x = 1, 24 do
        for y = 1, 32 do
            if arr[x][y] then
                str = str .. "1"
            else
                str = str .. "0"
            end
        end
        if x ~= 24 then
            str = str .. "\n"
        end
    end
    str = str:match'^(.*%S)%s*$'
    return str
end



return arrayutils