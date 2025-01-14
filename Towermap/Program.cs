using System;
using Towermap;

class Program 
{
    [STAThread]
    static void Main() 
    {
        var game = new TowermapGame(new Riateu.WindowSettings("Towermap", 1024, 640), Riateu.GraphicsSettings.Debug);
        game.Run();
    }
}