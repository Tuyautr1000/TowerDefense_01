using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using TowerDefense.Models;

internal partial class main : TileMap
{
	private Camera2D camera;

	public override void _Ready()
	{
		GD.Print("Initialisation du projet");
		camera = GetNode<Camera2D>("Camera2D");
		//ReadMap("res://Assets/Map/level1.json");
		//GenerateMap("res://Assets/Map/level1.json");
		//SaveAnimatedObjects("res://Assets/Map/AnimatedObjectsData.json");
		LoadAnimatedObjects("res://Assets/Map/AnimatedObjectsData.json");
		CenterCamera();
		AdjustCameraForMobile();

		// Assure la présence d'un BaseManager pour la barre de vie de la base
		if (GetNodeOrNull<BaseManager>("BaseManager") == null)
		{
			var baseMgr = new BaseManager();
			baseMgr.Name = "BaseManager";
			AddChild(baseMgr);
		}
	}
}
