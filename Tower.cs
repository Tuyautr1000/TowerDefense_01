using Godot;
using System.Collections.Generic;

public partial class Tower : Node2D
{
	private Area2D detectionArea;
	private List<Enemy> enemiesInRange = new List<Enemy>();
	[Export] public int Damage = 30; // Réduit de 50 à 30 pour équilibrer
	[Export] public float AttackSpeed = 0.8f; // Réduit de 1.0 à 0.8 pour équilibrer
	public int level = 1;
	private Goldmanager gold;
	[Export] public PackedScene ProjectileScene;
	[Export] public Texture2D ProjectileTexture; // Texture du projectile spécifique à cette tour
	[Export] public SpriteFrames ProjectileSpriteFrames; // SpriteFrames du projectile (si animé)
	private bool _isPlaced = false;
	public bool IsPlaced
	{
		get => _isPlaced;
		set
		{
			_isPlaced = value;
			if (_isPlaced)
			{
				TryStartAttackIfEnemiesOverlap();
			}
		}
	}
	private bool isAttacking = false;
	private AnimatedSprite2D soldier;
	private AnimatedSprite2D tower;
	private Timer attackTimer;
	private List<(int Price, int Damage, float AttackSpeed, bool Visible)> _levels;
	private bool hasIdleAnimation = false; // Pour détecter si la tour a une animation Idle
	private string towerType = ""; // Type de tour (catapult, mage, etc.)
	
	// Méthode publique pour obtenir le prix d'amélioration
	public int GetUpgradePrice()
	{
		if (_levels == null || level >= 5)
			return 0;
		return _levels[level - 1].Price;
	}
	
	// Méthode helper pour trouver l'UiManager récursivement
	private UiManager FindUiManager(Node node)
	{
		if (node is UiManager um)
			return um;
		
		foreach (Node child in node.GetChildren())
		{
			var found = FindUiManager(child);
			if (found != null)
				return found;
		}
		
		return null;
	}

	public override void _Ready()
	{
		detectionArea = GetNodeOrNull<Area2D>("AnimatedSprite2D/Area2D");
		tower = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		soldier = GetNodeOrNull<AnimatedSprite2D>("Soldier");
		gold = GetNodeOrNull<Goldmanager>("../../UI/Panel/GoldManager");

		// Détecter le type de tour basé sur le nom de la scène
		var scene = SceneFilePath;
		if (!string.IsNullOrEmpty(scene))
		{
			if (scene.Contains("catapult"))
				towerType = "catapult";
			else if (scene.Contains("mage"))
				towerType = "mage";
			else if (scene.Contains("guardian"))
				towerType = "guardian";
			else
				towerType = "tower";
		}
		else
		{
			// Si on n'a pas le chemin de la scène, utiliser le nom du nœud
			string nodeName = Name.ToString().ToLower();
			if (nodeName.Contains("catapult"))
				towerType = "catapult";
			else if (nodeName.Contains("mage"))
				towerType = "mage";
			else if (nodeName.Contains("guardian"))
				towerType = "guardian";
			else
				towerType = "tower";
		}

		// Vérifier si l'animation Idle existe pour le soldat
		if (soldier != null && soldier.SpriteFrames != null)
		{
			hasIdleAnimation = soldier.SpriteFrames.HasAnimation("Idle");
		}

		// Ajuster le positionnement du soldat pour catapultes, mages et guardians
		// La position de la tour ne doit pas être modifiée, seule celle du soldier change
		// Le positionnement sera recalculé dynamiquement à chaque amélioration
		if (soldier != null && tower != null && (towerType == "catapult" || towerType == "mage" || towerType == "guardian"))
		{
			AdjustSoldierPosition();
		}
		
		// Pour les autres tours, s'assurer que le soldat est visible
		if (soldier != null && towerType != "tower")
		{
			soldier.Visible = true;
		}

		// Crée et configure le timer d'attaque
		attackTimer = new Timer
		{
			OneShot = false,
			WaitTime = 1.0f / AttackSpeed,
			Autostart = false
		};
		AddChild(attackTimer);
		attackTimer.Timeout += Attack;

		tower.Play("Level " + level);
		
		// Ne jouer Idle que si l'animation existe
		if (soldier != null && hasIdleAnimation)
		{
			soldier.Play("Idle");
		}

		detectionArea.AreaEntered += OnEnemyEnter;
		detectionArea.AreaExited += OnEnemyExit;
		detectionArea.InputEvent += OnAreaInputEvent;
		
		// S'assurer que l'Area2D peut recevoir les événements d'input
		detectionArea.InputPickable = true;
		
		// Configurer l'Area2D pour détecter les ennemis ET les clics
		detectionArea.CollisionLayer = 1; // Layer 1 pour les tours
		detectionArea.CollisionMask = 2; // Layer 2 pour détecter les ennemis
		
		// S'assurer que l'Area2D peut recevoir les événements d'input même avec un collision mask
		detectionArea.Monitoring = true; // Pour détecter les ennemis
		detectionArea.Monitorable = true; // Pour être détectable par les clics
		
		// Debug pour diagnostiquer l'Area2D
		GD.Print("=== CONFIGURATION AREA2D ===");
		GD.Print("Area2D trouvé: " + (detectionArea != null));
		if (detectionArea != null)
		{
			GD.Print("InputPickable: " + detectionArea.InputPickable);
			GD.Print("Collision Layer: " + detectionArea.CollisionLayer);
			GD.Print("Collision Mask: " + detectionArea.CollisionMask);
			GD.Print("Position: " + detectionArea.GlobalPosition);
			GD.Print("Visible: " + detectionArea.Visible);
			GD.Print("Process Mode: " + detectionArea.ProcessMode);
		}
		

		// Améliorations rééquilibrées : prix augmentés, dégâts et vitesse réduits
		_levels = new List<(int, int, float, bool)>
		{
			(100, 40, 1.0f, false), // lvl 2: +25 or, -20 dégâts, -0.1 att/s
			(150, 50, 1.1f, true),  // lvl 3: +25 or, -20 dégâts, -0.1 att/s
			(200, 60, 1.2f, true),  // lvl 4: +25 or, -20 dégâts, -0.1 att/s
			(250, 70, 1.3f, false)  // lvl 5: +25 or, -15 dégâts, -0.1 att/s
		};
	}

	public override void _Process(double delta)
	{
		if (!IsPlaced)
			return;

		if (enemiesInRange.Count > 0)
		{
			Enemy target = enemiesInRange[0];

			if (IsInstanceValid(target))
			{
				string attackAnimation = GetAnimationForDirection(target.GlobalPosition);
				Vector2 direction = (target.GlobalPosition - GlobalPosition).Normalized();

				soldier.Scale = new Vector2(direction.X > 0 ? -1 : 1, 1);

				if (soldier.SpriteFrames.HasAnimation(attackAnimation))
					soldier.Play(attackAnimation);
			}
		}
		else
		{
			// Ne jouer Idle que si l'animation existe (pas pour catapultes)
			if (soldier != null && hasIdleAnimation)
			{
				soldier.Play("Idle");
			}
		}
	}

	private void OnAreaInputEvent(Node viewport, InputEvent @event, long shapeIdx)
	{
		GD.Print("=== OnAreaInputEvent APPELÉ ===");
		GD.Print("Event: " + @event.GetType().Name);
		GD.Print("IsPlaced: " + IsPlaced);
		GD.Print("ShapeIdx: " + shapeIdx);
		
		if (!IsPlaced)
		{
			GD.Print("Tour non placée - amélioration impossible");
			return;
		}
			
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.Pressed)
		{
			GD.Print("=== CLIC DROIT DÉTECTÉ SUR TOUR ===");
			
			// Informer l'UiManager pour afficher l'UI d'actions
			// Chercher l'UiManager dans la scène (plusieurs chemins possibles)
			UiManager uiManager = GetNodeOrNull<UiManager>("../../UI");
			if (uiManager == null)
			{
				uiManager = GetNodeOrNull<UiManager>("/root/Main/UI");
			}
			if (uiManager == null)
			{
				// Chercher dans toute la scène
				uiManager = GetTree().GetFirstNodeInGroup("ui_manager") as UiManager;
				if (uiManager == null)
				{
					// Dernière tentative : chercher par type dans la scène racine
					var sceneRoot = GetTree().Root;
					foreach (Node child in sceneRoot.GetChildren())
					{
						if (child is UiManager um)
						{
							uiManager = um;
							break;
						}
						// Chercher récursivement
						uiManager = FindUiManager(child);
						if (uiManager != null) break;
					}
				}
			}
			
			if (uiManager != null)
			{
				GD.Print("UiManager trouvé ! Affichage de l'UI d'actions");
				uiManager.ShowTowerActionUI(this, GlobalPosition);
			}
			else
			{
				GD.PrintErr("UiManager non trouvé ! Vérifiez le chemin dans Tower.cs");
			}
		}
	}

	private Rect2 GetRect()
	{
		Vector2 size = tower.SpriteFrames.GetFrameTexture(tower.Animation, tower.Frame).GetSize();
		Vector2 origin = tower.Centered ? -size / 2 : Vector2.Zero;
		return new Rect2(origin, size);
	}

	private void OnEnemyEnter(Area2D area)
	{
		Enemy e = area.GetParent().GetParent() as Enemy;
		if (e == null)
			return;

		enemiesInRange.Add(e);
		GD.Print($"Ennemi détecté ! Total ennemis en portée: {enemiesInRange.Count}");

		if (!isAttacking && IsPlaced)
		{
			isAttacking = true;
			attackTimer.Start();
			GD.Print("Attaque démarrée !");
		}
	}

	private void OnEnemyExit(Node2D body)
	{
		if (body is Area2D area)
		{
			Node parent = area.GetParent()?.GetParent();

			if (parent is Enemy e)
			{
				enemiesInRange.Remove(e);

				if (enemiesInRange.Count == 0)
				{
					isAttacking = false;
					attackTimer.Stop();
				}
			}
		}
	}

	private string GetAnimationForDirection(Vector2 enemyPosition)
	{
		Vector2 direction = (enemyPosition - GlobalPosition).Normalized();

		if (Mathf.Abs(direction.X) > Mathf.Abs(direction.Y))
			return "SideAttack";
		else
			return direction.Y > 0 ? "frontAttack" : "BackAttack";
	}

	private void Attack()
	{
		if (!IsPlaced || enemiesInRange.Count == 0)
		{
			isAttacking = false;
			attackTimer.Stop();
			return;
		}

		enemiesInRange.RemoveAll(e => !IsInstanceValid(e));

		if (enemiesInRange.Count == 0)
		{
			isAttacking = false;
			attackTimer.Stop();
			return;
		}

		Enemy target = enemiesInRange[0];
		GD.Print($"Attaque ! Cible: {target.EnemyType}, Dégâts: {Damage}");

		if (ProjectileScene != null)
		{
			Projectile projectile = (Projectile)ProjectileScene.Instantiate();
			
			// Sauvegarder les références pour configurer le sprite après l'ajout à la scène
			Texture2D projTexture = ProjectileTexture;
			SpriteFrames projFrames = ProjectileSpriteFrames;
			
			projectile.Launch(GlobalPosition, target, Damage);
			
			// Ajouter le projectile à la scène
			GetParent().CallDeferred("add_child", projectile);
			
			// Configurer le sprite du projectile après qu'il soit ajouté à la scène
			// Utiliser un Timer très court pour s'assurer que le nœud est bien ajouté avant de configurer le sprite
			GetTree().CreateTimer(0.001f).Timeout += () => {
				if (IsInstanceValid(projectile))
				{
					projectile.SetProjectileSprite(projTexture, projFrames);
				}
			};
			
			GD.Print("Projectile lancé !");
		}
		else
		{
			GD.PrintErr("ProjectileScene est null !");
		}
	}

	private void TryStartAttackIfEnemiesOverlap()
	{
		if (detectionArea == null)
			return;

		var overlaps = detectionArea.GetOverlappingAreas();
		foreach (Area2D area in overlaps)
		{
			Enemy e = area.GetParent()?.GetParent() as Enemy;
			if (e != null && !enemiesInRange.Contains(e))
			{
				enemiesInRange.Add(e);
			}
		}

		if (enemiesInRange.Count > 0 && !isAttacking)
		{
			isAttacking = true;
			attackTimer.Start();
		}
	}

	// Méthode publique pour permettre l'amélioration depuis l'extérieur
	public bool TryUpgrade()
	{
		if (!IsPlaced || level >= 5)
		{
			GD.Print("Impossible d'améliorer - tour non placée ou niveau max atteint");
			return false;
		}

		if (gold == null || !gold.IsEnoughCoin(_levels[level - 1].Price))
		{
			GD.Print("Impossible d'améliorer - pas assez d'or");
			return false;
		}

		// Effectuer l'amélioration
		gold.SpendCoins(_levels[level - 1].Price);
		Damage = _levels[level - 1].Damage;
		AttackSpeed = _levels[level - 1].AttackSpeed;
		
		// Le soldat ne disparaît que pour la tour d'archer, pas pour les autres tours
		if (towerType == "tower")
		{
			soldier.Visible = _levels[level - 1].Visible;
		}
		else if (towerType == "mage" || towerType == "guardian")
		{
			// Pour les mages et guardians, la visibilité sera gérée par AdjustSoldierPosition() selon le niveau
			// Ne rien faire ici, AdjustSoldierPosition() s'en chargera
		}
		else
		{
			// Pour les autres tours (catapult), le soldat reste toujours visible
			if (soldier != null)
				soldier.Visible = true;
		}

		level += 1;
		tower.Play("Level " + level);

		// Ajuster dynamiquement le positionnement du soldat après amélioration
		// La position de la tour ne doit pas être modifiée, seule celle du soldier change
		if (soldier != null && tower != null && (towerType == "catapult" || towerType == "mage" || towerType == "guardian"))
		{
			AdjustSoldierPosition();
		}

		// Met à jour la cadence du timer
		attackTimer.WaitTime = 1.0f / AttackSpeed;
		
		GD.Print($"Tour améliorée ! Niveau {level} - Dégâts: {Damage}, Vitesse: {AttackSpeed}");
		return true;
	}

	// Ajuster la hauteur de la tour elle-même pour catapultes et mages
	// Cette méthode monte la tour pour qu'elle soit mieux positionnée visuellement
	private void AdjustTowerHeight()
	{
		if (tower == null)
			return;

		// Obtenir la taille du sprite de la tour actuelle (selon le niveau)
		Vector2 towerSize = Vector2.Zero;
		if (tower.SpriteFrames != null && tower.Animation != null)
		{
			var texture = tower.SpriteFrames.GetFrameTexture(tower.Animation, 0);
			if (texture != null)
			{
				towerSize = texture.GetSize();
			}
		}

		// Si on n'a pas pu obtenir la taille, utiliser une valeur par défaut
		if (towerSize == Vector2.Zero)
		{
			towerSize = new Vector2(70, 130); // Taille par défaut des tours
		}

		// Calculer le décalage vertical pour monter la tour
		// Les catapultes et mages doivent être montées d'environ 10-15% de leur hauteur
		float heightOffset = towerSize.Y * 0.12f; // 12% de la hauteur
		
		// Ajustement selon le type de tour
		if (towerType == "catapult")
		{
			heightOffset = towerSize.Y * 0.15f; // 15% pour catapult
		}
		else if (towerType == "mage")
		{
			heightOffset = towerSize.Y * 0.10f; // 10% pour mage
		}
		
		// Ajuster la position Y de la tour (monter la tour)
		Vector2 currentPos = tower.Position;
		tower.Position = new Vector2(currentPos.X, currentPos.Y - heightOffset);
		
		GD.Print($"Hauteur de la tour {towerType} ajustée niveau {level}: {tower.Position} (hauteur sprite: {towerSize.Y}, offset: {heightOffset})");
	}

	// Ajuster le positionnement du soldat basé sur la taille du sprite de la tour
	// Cette méthode est appelée dynamiquement quand la tour change de niveau
	private void AdjustSoldierPosition()
	{
		if (soldier == null || tower == null)
			return;

		// Pour la catapult, utiliser des coordonnées fixes : X=0, Y selon le niveau
		if (towerType == "catapult")
		{
			switch (level)
			{
				case 1:
					soldier.Position = new Vector2(0, 18);
					break;
				case 2:
					soldier.Position = new Vector2(0, 16);
					break;
				case 3:
					soldier.Position = new Vector2(0, 11);
					break;
				case 4:
					soldier.Position = new Vector2(0, -4);
					break;
				case 5:
					soldier.Position = new Vector2(0, -18);
					break;
			}
			
			GD.Print($"Position du soldat ajustée pour catapult niveau {level}: {soldier.Position}");
			return;
		}

		// Pour la tour mage, utiliser des coordonnées fixes : X=0, Y selon le niveau
		if (towerType == "mage")
		{
			switch (level)
			{
				case 1:
					soldier.Position = new Vector2(0, 15);
					soldier.Visible = true;
					break;
				case 2:
					soldier.Position = new Vector2(0, 10);
					soldier.Visible = true;
					break;
				case 3:
					soldier.Position = new Vector2(0, 3);
					soldier.Visible = true;
					break;
				case 4:
					soldier.Position = new Vector2(0, -3);
					soldier.Visible = true;
					break;
				case 5:
					soldier.Visible = false; // Cacher le soldier au niveau 5
					break;
			}
			
			GD.Print($"Position du soldat ajustée pour mage niveau {level}: {soldier.Position}, Visible: {soldier.Visible}");
			return;
		}

		// Pour la tour guardian, utiliser des coordonnées fixes : X=0, Y selon le niveau
		if (towerType == "guardian")
		{
			switch (level)
			{
				case 1:
					soldier.Position = new Vector2(0, 11);
					soldier.Visible = true;
					break;
				case 2:
					soldier.Position = new Vector2(0, 9);
					soldier.Visible = true;
					break;
				case 3:
					soldier.Position = new Vector2(0, 9);
					soldier.Visible = true;
					break;
				case 4:
					soldier.Visible = false; // Cacher le soldier au niveau 4
					break;
				case 5:
					soldier.Visible = false; // Cacher le soldier au niveau 5
					break;
			}
			
			GD.Print($"Position du soldat ajustée pour guardian niveau {level}: {soldier.Position}, Visible: {soldier.Visible}");
			return;
		}

		// Pour les autres tours, utiliser le calcul dynamique
		// Obtenir la taille du sprite de la tour actuelle (selon le niveau)
		Vector2 towerSize = Vector2.Zero;
		if (tower.SpriteFrames != null && tower.Animation != null)
		{
			var texture = tower.SpriteFrames.GetFrameTexture(tower.Animation, 0);
			if (texture != null)
			{
				towerSize = texture.GetSize();
			}
		}

		// Si on n'a pas pu obtenir la taille, utiliser une valeur par défaut
		if (towerSize == Vector2.Zero)
		{
			towerSize = new Vector2(70, 130); // Taille par défaut des tours
		}

		// Calculer le décalage vertical basé sur la hauteur de la tour
		// Pour catapultes et mages, monter le soldat d'environ 15-20% de la hauteur de la tour
		// Le décalage augmente avec le niveau car la tour grandit
		float baseHeightOffset = towerSize.Y * 0.15f; // 15% de la hauteur de base
		
		// Ajustement supplémentaire selon le niveau (la tour grandit avec les niveaux)
		float levelMultiplier = 1.0f + (level - 1) * 0.1f; // +10% par niveau
		float heightOffset = baseHeightOffset * levelMultiplier;
		
		// Position de base du soldat (depuis la scène)
		// Pour mages, on part de la position initiale et on ajuste
		Vector2 basePosition = new Vector2(0, 4); // Position de base pour mage
		
		// Ajuster la position Y (monter le soldat proportionnellement à la hauteur de la tour)
		soldier.Position = new Vector2(basePosition.X, basePosition.Y - heightOffset);
		
		GD.Print($"Position du soldat ajustée pour {towerType} niveau {level}: {soldier.Position} (hauteur tour: {towerSize.Y}, offset: {heightOffset})");
	}
}
