using System;
using Riateu;
using Riateu.Graphics;
using Towermap;

class Program 
{
    [STAThread]
    static void Main() 
    {
        var game = new TowermapGame(new Riateu.WindowSettings("Towermap", 1280, 640, WindowMode.Windowed, new Flags 
        {
            Borderless = true
        }, OnHitTest), Riateu.GraphicsSettings.Debug);
        game.Run();
    }

    private static HitTestResult OnHitTest(Window window, Point area) 
    {
        if (area.Y < 20)
        {
            return HitTestResult.Draggable;
        }

        return HitTestResult.Normal;
    }
}