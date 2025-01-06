using System.Numerics;
using Riateu;
using Riateu.Graphics;

namespace Towermap;

public class EditorCanvas 
{
    private Batch instanceBatch;
    private Scene scene;
    private RenderTarget target;

    public RenderTarget CanvasTexture => target;
    public EditorCanvas(Scene scene, GraphicsDevice device) 
    {
        this.scene = scene;
        target = new RenderTarget(device, 320, 240);
        instanceBatch = new Batch(device, 320, 240);
    }

    public void Render(CommandBuffer buffer)
    {
        instanceBatch.Begin(Resource.BGAtlasTexture, DrawSampler.PointClamp);
        instanceBatch.Draw(Resource.BGAtlas["daySky"], Vector2.Zero, Color.White);
        instanceBatch.End();

        instanceBatch.Begin(Resource.TowerFallTexture, DrawSampler.PointClamp);
        scene.EntityList.Draw(instanceBatch);
        instanceBatch.End();

        var renderPass = buffer.BeginRenderPass(new ColorTargetInfo(target, Color.Black, true));
        renderPass.BindGraphicsPipeline(GameContext.DefaultMaterial.ShaderPipeline);
        instanceBatch.Render(renderPass);
        buffer.EndRenderPass(renderPass);
    }
}