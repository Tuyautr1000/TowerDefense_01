using Godot;
using System;

internal partial class main
{
	private void  CenterCamera()
	{
		if (TileSet != null)
		{
			Vector2I cellSize = TileSet.TileSize;
			Vector2 mapSize = GetUsedRect().Size * cellSize;
			Vector2 centerPosition = GetUsedRect().Position * cellSize + mapSize / 2;
			
			if (camera != null)
			{
				camera.Position = new Vector2I((int)centerPosition.X, (int)centerPosition.Y);
			}
		}
		
	}
}
