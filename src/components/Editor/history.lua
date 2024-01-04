local history = {
    tileCommit = {},
    currentCommit = 0
}

local function deepcopy(orig)
    local orig_type = type(orig)
    local copy
    if orig_type == 'table' then
        copy = {}
        for orig_key, orig_value in next, orig, nil do
            copy[deepcopy(orig_key)] = deepcopy(orig_value)
        end
        setmetatable(copy, deepcopy(getmetatable(orig)))
    else -- number, string, boolean, etc
        copy = orig
    end
    return copy
end

function history:pushCommit(layerType, tiles)
    local copyTiles = deepcopy(tiles)
    local commit = {
        tiles = copyTiles,
        layerType = layerType
    }
    table.insert(self.tileCommit, commit)
    self.currentCommit = self.currentCommit + 1
end

function history:popCommit()
    local commit = self.tileCommit[self.currentCommit]
    local newCommit = {
        tiles = commit.tiles,
        layerType = commit.layerType
    }
    table.remove(self.tileCommit, self.currentCommit)
    self.currentCommit = self.currentCommit - 1
    print(Dump(self.tileCommit))

    return newCommit
end

return history