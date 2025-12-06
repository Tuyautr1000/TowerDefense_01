using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using TowerDefense.Models;

internal partial class main 
{
	private void GenerateMap(string filePath)
	{
		GD.Print("Géneration de la map à partir du fichier...");
		
		if(FileAccess.FileExists(filePath))
		{
			using (FileAccess file= FileAccess.Open(filePath, FileAccess.ModeFlags.Read))
			{
				string jsonContent = file.GetAsText();
				var mapData = JsonSerializer.Deserialize<List<CellData>>(jsonContent);
				
				foreach (var cell in mapData)
				{
					SetCell(
						cell.Layer,
						cell.Position.ToVector2I(),
						cell.CellId,
						cell.AtlasCoords.ToVector2I(),
						0
					);
				}
			}
		}
		else
		{
			GD.PrintErr($"Le fichier {filePath} n'existe pas.");
		}
		GD.Print("Map générée.");
	}

}
