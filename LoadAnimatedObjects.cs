using System;
using Godot;
using System.Collections.Generic;
using System.Text.Json;
using TowerDefense.Models;

internal partial class main
{
	private void LoadAnimatedObjects(string filePath)
	{
		GD.Print("Chargement des objets animées...");
		
		// Vérifiez si le fichier existe
		if (!FileAccess.FileExists(filePath))
		{
			GD.PrintErr($"Le fichier {filePath} n'existe pas.");
		}
		
		using (FileAccess file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read))
		{
			string jsonContent = file.GetAsText();
			var animatedElements = JsonSerializer.Deserialize<List<AnimatedElementData>>(jsonContent);
		
			if (animatedElements == null || animatedElements.Count == 0)
			{
				GD.PrintErr("Aucun élément animé trouvé dans le fichier.");
				return;
			}
			
			// Référence au noeud parent qui contiendra les AnimatedSprite2D
			var animatedObjectsNode = GetNode<Node2D>("AnimatedObjects");
		
			if (animatedObjectsNode == null)
			{
				GD.PrintErr("Le noeud 'AnimatedObjects' est introuvable.");
				return;
			}
		
			foreach (var element in animatedElements)
			{
				var animatedSprite = new AnimatedSprite2D();
				var spriteFrames = ResourceLoader.Load<SpriteFrames>(element.AnimationType);
				if (spriteFrames == null) {
					continue;
				}
				
				animatedSprite.SpriteFrames = spriteFrames;
				animatedSprite.Position = element.Position.ToVector2();
				animatedSprite.Animation = element.AnimationName;
				animatedSprite.Name = $"{element.ParentFolder}_{element.AnimationName}";
				
				animatedObjectsNode.AddChild(animatedSprite);
				animatedSprite.Play(element.AnimationName);
				GD.Print($"Ajout de l'AnimatedSprite2D : {animatedSprite.Name}");
			}
		}
		GD.Print("Chargement des objects animés terminé.");
	}
}
