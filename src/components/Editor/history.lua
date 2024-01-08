local tableutils = require("src.utils.tableutils")

local history = {
    tileCommit = {},
    currentCommit = 0
}

function history:pushCommit(layerType, tiles)
    local copyTiles = tableutils.deepcopy(tiles)
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

    return newCommit
end

function history:clearHistory()
    self.tileCommit = {}
    self.currentCommit = 0
end

return history