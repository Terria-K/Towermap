using System;
using Towermap;

class Program 
{
    [STAThread]
    static void Main() 
    {
        var game = new TowermapGame("Towermap", 960, 640);
        game.Run();
    }
}