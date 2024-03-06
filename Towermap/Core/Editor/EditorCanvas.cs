using MoonWorks.Graphics;
using Riateu;
using Riateu.Graphics;

namespace Towermap;

public class EditorCanvas : Canvas
{
    private InstanceBatch instanceBatch;
    public EditorCanvas(Scene scene, GraphicsDevice device) : base(scene, device, 320, 240)
    {
        instanceBatch = new InstanceBatch(device, 320, 240);
    }

    public override void Draw(CommandBuffer buffer, IBatch batch)
    {
        instanceBatch.Begin();
        Scene.EntityList.Draw(buffer, instanceBatch);
        instanceBatch.End(buffer);

        buffer.BeginRenderPass(new ColorAttachmentInfo(CanvasTexture, Color.DimGray, StoreOp.DontCare));
        buffer.BindGraphicsPipeline(GameContext.InstancedPipeline);
        instanceBatch.Draw(buffer);
        buffer.EndRenderPass();
    }
}