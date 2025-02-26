return {
    name: "MovingPlatform",
    texture: "movingTiles",
    width: 20,
    height: 20,
    resizableX: true,
    resizableY: true,
    hasNodes: true,
    tags: ["Solid", "Platform"],
    textureSize: [10, 10],
    onRender: "TileRender3x3"
}