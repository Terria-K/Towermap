return {
    name: "LoopPlatform",
    texture: "loopPlatformTiles",
    width: 10,
    height: 10,
    tags: ["Solid", "Interactable", "Platform"],
    textureSize: [10, 10],
    resizableX: true,
    darkWorld: true,
    values: {
        Direction: "Left"
    },
    onRender: "TileRender3x1"
}