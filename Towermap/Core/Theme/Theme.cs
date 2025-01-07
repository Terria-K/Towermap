using System;

namespace Towermap;

public record Theme(string Name, string SolidTilesetID, string BGTilesetID, Action<BackdropRenderer> BackdropRender);
