using Cosmiria;
using Riateu.Graphics;

namespace Towermap;

public static class AtlasImages
{
    private static ImageSource atlas;

    public static void Load(string path)
    {
        atlas = ImageSource.FromFile(path);
    }

    public static Node AtlasImage(this UIStack ui, TextureQuad quad, ImageFit fit, in Style style) =>
        ui.Image(atlas.Region(quad.Source.X, quad.Source.Y, quad.Source.Width, quad.Source.Height), fit, style)
            .Sampling(ImageSampling.Nearest);
}
