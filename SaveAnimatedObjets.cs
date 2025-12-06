using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using TowerDefense.Models;

internal partial class main
{
	private void SaveAnimatedObjects(string filePath)
	{
		GD.Print("Lecture des éléments animés...");
		var animatedElements = new List<AnimatedElementData>();
		
		Node2D animatedObjectsNode = GetNode<Node2D>("AnimatedObjects");
		if (animatedObjectsNode == null)
		{
			GD.PrintErr("Le noeud 'AnimatedObjects' est introuvable.");
			return;
		}
		
		
		foreach (Node folderNode in animatedObjectsNode.GetChildren())
		{
			if (folderNode is Node2D folder)
			{
				foreach (Node subNode in folder.GetChildren())
				{
					if (subNode is AnimatedSprite2D animatedSprite)
					{
						animatedElements.Add(
							new AnimatedElementData
							{
								Position = new SerializableVector2I(
									new Vector2I(
										(int)animatedSprite.Position.X,
										(int)animatedSprite.Position.Y
									)
								),
								AnimationType = animatedSprite.SpriteFrames.ResourcePath,
								AnimationName = animatedSprite.Animation,
								ParentFolder = folder.Name,
							}
						);
					}
				}
			}
		}
		string jsonString = JsonSerializer.Serialize(
			animatedElements,
			new JsonSerializerOptions{ WriteIndented = true }
		);
		
		using (FileAccess file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write))
		{
			file.StoreString(jsonString);
		}
		
	}
}
