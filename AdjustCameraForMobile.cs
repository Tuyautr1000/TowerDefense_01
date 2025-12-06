using Godot;
using System;

internal partial class main 
{
	private void AdjustCameraForMobile()
	{
		if (camera == null)
		{
			GD.PrintErr("La caméra n'est pas initialisée");
			return;
		}
		
		float baseWidth = 1920.0f;
		float baseHeight = 1080.0f;
		Vector2 screenSize = GetViewportRect().Size;
		
		GD.Print(screenSize);
	
		float zoomFactor = Math.Min( baseWidth /screenSize.X, baseHeight / screenSize.Y);
		camera.Zoom = new Vector2((float)0.9, (float)0.9);
		
		camera.MakeCurrent();
		GD.Print($"Caméra ajustée pour le mobile avec un facteur de zoom : {zoomFactor}");
	}
}
