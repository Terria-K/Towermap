return {
    name: "SpikeBall",
    texture: "spikeBall",
    width: 22,
    height: 22,
    originX: 11,
    originY: 11,
    tags: ["Hazard"],
    hasNodes: true,
    values: {
        Explodes: false 
    },
    onRender: "SpikeBallRender"
}