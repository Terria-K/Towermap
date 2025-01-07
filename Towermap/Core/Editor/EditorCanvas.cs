using System.Numerics;
using Riateu;
using Riateu.Graphics;

namespace Towermap;

public class EditorCanvas 
{
    private Batch spriteBatch;
    private Scene scene;
    private RenderTarget target;
    private BackdropRenderer backdropRenderer;

    public RenderTarget CanvasTexture => target;
    public EditorCanvas(Scene scene, GraphicsDevice device, BackdropRenderer backdropRenderer) 
    {
        this.scene = scene;
        target = new RenderTarget(device, 320, 240);
        spriteBatch = new Batch(device, 320, 240);
        this.backdropRenderer = backdropRenderer;
    }

    public void Render(CommandBuffer buffer)
    {
        spriteBatch.Begin(Resource.BGAtlasTexture, DrawSampler.PointClamp);
        backdropRenderer.Draw(spriteBatch);
        spriteBatch.End();

        spriteBatch.Begin(Resource.TowerFallTexture, DrawSampler.PointClamp);
        scene.EntityList.Draw(spriteBatch);
        spriteBatch.End();

        var renderPass = buffer.BeginRenderPass(new ColorTargetInfo(target, Color.Black, true));
        renderPass.BindGraphicsPipeline(GameContext.DefaultMaterial.ShaderPipeline);
        spriteBatch.Render(renderPass);
        buffer.EndRenderPass(renderPass);
    }
}