local tableutils = {}

function tableutils.deepcopy(orig)
    local orig_type = type(orig)
    local copy
    if orig_type == 'table' then
        copy = {}
        for orig_key, orig_value in next, orig, nil do
            copy[tableutils.deepcopy(orig_key)] = tableutils.deepcopy(orig_value)
        end
        setmetatable(copy, tableutils.deepcopy(getmetatable(orig)))
    else -- number, string, boolean, etc
        copy = orig
    end
    return copy
end
return tableutils