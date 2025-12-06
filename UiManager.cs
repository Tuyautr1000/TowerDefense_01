using Godot;
using System;

public partial class UiManager : Control
{
	[Export] public PackedScene TowerScene;
	[Export] public PackedScene CatapultScene;
	[Export] public PackedScene GuardianScene;
	[Export] public PackedScene MageScene;
	
	private Tower currentTower = null;
	private bool isPlacing = false;
	private PackedScene currentTowerScene = null;
	private int currentPrice = 100;
	
	// UI pour les actions sur les tours (améliorer/supprimer)
	private Control towerActionUI = null;
	private Tower selectedTower = null;
	
	// Prix des différentes tours (augmentés pour équilibrer l'économie)
	private int towerPrice = 25;      // +15 or
	private int catapultPrice = 30;    // +20 or
	private int guardianPrice = 40;   // +20 or
	private int magePrice = 35;        // +17 or
	
	private Goldmanager gold;
	private Node2D towersParent;

	public override void _Ready()
	{
		GD.Print("=== UIMANAGER INITIALISÉ - DEBUG ACTIF ===");
		GD.Print("=== TOUS LES MESSAGES DE DEBUG DOIVENT APPARAÎTRE ===");
		
		gold = GetNodeOrNull<Goldmanager>("./Panel/GoldManager");
		towersParent = GetNode<Node2D>("../Towers"); // ✅ parent Node2D pour placer les tours
		
		// Créer l'UI pour les actions sur les tours
		CreateTowerActionUI();
		

		// Connecter tous les boutons de tours
		GD.Print("Vérification des scènes de tours:");
		GD.Print("TowerScene: " + (TowerScene != null ? TowerScene.ResourcePath : "NULL"));
		GD.Print("CatapultScene: " + (CatapultScene != null ? CatapultScene.ResourcePath : "NULL"));
		GD.Print("GuardianScene: " + (GuardianScene != null ? GuardianScene.ResourcePath : "NULL"));
		GD.Print("MageScene: " + (MageScene != null ? MageScene.ResourcePath : "NULL"));
		
		ConnectTowerButton("Panel/VBoxContainer/TowerButton", TowerScene, towerPrice);
		// Utiliser les scènes assignées directement
		ConnectTowerButton("Panel/VBoxContainer/CatapultButton", CatapultScene, catapultPrice);
		ConnectTowerButton("Panel/VBoxContainer/GuardianButton", GuardianScene, guardianPrice);
		ConnectTowerButton("Panel/VBoxContainer/MageButton", MageScene, magePrice);
		
		// Test simple pour vérifier si les signaux fonctionnent
		GD.Print("=== TEST DE CONNEXION DES SIGNAUX ===");
		var testButton = GetNodeOrNull<TextureButton>("Panel/VBoxContainer/TowerButton");
		if (testButton != null)
		{
			GD.Print("Test bouton trouvé: " + testButton.Name);
			GD.Print("Nombre de connexions sur Pressed: " + testButton.GetSignalConnectionList("pressed").Count);
		}
		
		// Vérifier le VBoxContainer
		var vbox = GetNodeOrNull<VBoxContainer>("Panel/VBoxContainer");
		if (vbox != null)
		{
			GD.Print("VBoxContainer trouvé: " + vbox.Name);
			GD.Print("VBoxContainer - Position: " + vbox.GlobalPosition);
			GD.Print("VBoxContainer - Size: " + vbox.Size);
			GD.Print("VBoxContainer - Children count: " + vbox.GetChildCount());
			
			// Vérifier chaque enfant du VBoxContainer
			for (int i = 0; i < vbox.GetChildCount(); i++)
			{
				var child = vbox.GetChild(i);
				string position = "N/A";
				if (child is Node2D node2d)
					position = node2d.GlobalPosition.ToString();
				else if (child is Control control)
					position = control.GlobalPosition.ToString();
				
				GD.Print("Enfant " + i + ": " + child.Name + " (" + child.GetType().Name + ") - Position: " + position);
			}
		}
		
		// Forcer le repositionnement du VBoxContainer après connexion de tous les boutons
		GetTree().CreateTimer(0.1).Timeout += () => {
			var vbox = GetNodeOrNull<VBoxContainer>("Panel/VBoxContainer");
			if (vbox != null)
			{
				GD.Print("=== FORÇAGE DU REPOSITIONNEMENT DU VBOXCONTAINER ===");
				vbox.QueueRedraw();
				vbox.ForceUpdateTransform();
				
				// Vérifier les nouvelles positions
				for (int i = 0; i < vbox.GetChildCount(); i++)
				{
					var child = vbox.GetChild(i);
					if (child is Control control)
					{
						GD.Print("Nouvelle position " + child.Name + ": " + control.GlobalPosition);
					}
				}
			}
		};
		
		// Test simple : forcer le déclenchement d'un signal après 2 secondes (DÉSACTIVÉ)
		// GetTree().CreateTimer(2.0).Timeout += () => {
		//	GD.Print("=== TEST FORCÉ DU SIGNAL ===");
		//	OnTowerButtonPressed(TowerScene, towerPrice);
		// };

		if (gold == null)
		{
			GD.PrintErr("GoldManager non trouvé !");
		}

		if (towersParent == null)
		{
			GD.PrintErr("Towers parent non trouvé !");
		}
	}

	private (PackedScene scene, int price) GetTowerDataForButton(string buttonName)
	{
		switch (buttonName)
		{
			case "TowerButton":
				return (TowerScene, towerPrice);
			case "CatapultButton":
				return (CatapultScene, catapultPrice);
			case "GuardianButton":
				return (GuardianScene, guardianPrice);
			case "MageButton":
				return (MageScene, magePrice);
			default:
				return (null, 0);
		}
	}
	
	private TextureButton FindTowerButtonInHierarchy(Control control)
	{
		// Chercher dans la hiérarchie vers le haut (parents)
		Node current = control;
		while (current != null)
		{
			if (current is TextureButton button)
			{
				// Vérifier si c'est un bouton de tour
				if (button.Name == "TowerButton" || button.Name == "CatapultButton" || 
					button.Name == "GuardianButton" || button.Name == "MageButton")
				{
					return button;
				}
			}
			current = current.GetParent();
		}
		
		// Chercher dans la hiérarchie vers le bas (enfants)
		return FindTowerButtonInChildren(control);
	}
	
	private TextureButton FindTowerButtonInChildren(Node node)
	{
		if (node is TextureButton button)
		{
			// Vérifier si c'est un bouton de tour
			if (button.Name == "TowerButton" || button.Name == "CatapultButton" || 
				button.Name == "GuardianButton" || button.Name == "MageButton")
			{
				return button;
			}
		}
		
		// Chercher dans tous les enfants
		foreach (Node child in node.GetChildren())
		{
			TextureButton found = FindTowerButtonInChildren(child);
			if (found != null)
				return found;
		}
		
		return null;
	}
	
	private void ConnectTowerButton(string path, PackedScene towerScene, int price)
	{
		TextureButton button = GetNodeOrNull<TextureButton>(path);
		if (button != null)
		{
			GD.Print("Bouton de tour trouvé: " + button.Name);
			GD.Print("  - Visible: " + button.Visible);
			GD.Print("  - Disabled: " + button.Disabled);
			GD.Print("  - Modulate: " + button.Modulate);
			GD.Print("  - Position: " + button.GlobalPosition);
			GD.Print("  - Size: " + button.Size);
			
			// Créer une closure pour capturer les paramètres
			button.Pressed += () => OnTowerButtonPressed(towerScene, price);
			GD.Print("Bouton " + button.Name + " connecté au signal Pressed");
			
			// Test alternatif avec un signal personnalisé
			button.GuiInput += (inputEvent) => OnButtonGuiInput(inputEvent, towerScene, price);
			GD.Print("Bouton " + button.Name + " connecté au signal GuiInput");
			
			// S'assurer que le bouton peut recevoir des clics
			button.MouseFilter = Control.MouseFilterEnum.Pass;
			
			// Forcer le repositionnement du bouton dans le VBoxContainer
			if (button.GetParent() is VBoxContainer vbox)
			{
				// Forcer la mise à jour du layout du VBoxContainer
				vbox.QueueRedraw();
				vbox.ForceUpdateTransform();
			}
			
			GD.Print("Bouton " + button.Name + " - Parent: " + (button.GetParent() != null ? button.GetParent().Name : "null"));
			GD.Print("Bouton " + button.Name + " - MouseFilter: " + button.MouseFilter);
		}
		else
		{
			GD.PrintErr("Bouton de tour NON TROUVÉ ! Chemin: " + path);
		}
	}

	private void OnTowerButtonPressed(PackedScene towerScene, int price)
	{
		GD.Print("*** === BOUTON DE TOUR PRESSÉ - SIGNAL DÉCLENCHÉ ! === ***");
		GD.Print("Type de tour: " + (towerScene != null ? towerScene.ResourcePath : "NULL"));
		GD.Print("Prix: " + price);
		
		if (towerScene == null)
		{
			GD.PrintErr("ERREUR: Scène de tour NULL ! Veuillez assigner la scène dans l'éditeur Godot.");
			return;
		}
		
		StartTowerPlacement(towerScene, price);
	}

	private void OnButtonGuiInput(InputEvent @event, PackedScene towerScene, int price)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
		{
			GD.Print("=== BOUTON DE TOUR CLIQUÉ VIA GUIINPUT ===");
			GD.Print("Type de tour: " + (towerScene != null ? towerScene.ResourcePath : "null"));
			GD.Print("Prix: " + price);
			StartTowerPlacement(towerScene, price);
		}
	}

	private void StartTowerPlacement(PackedScene towerScene = null, int price = 100)
	{
		GD.Print("Début du placement de tour");
		
		if (isPlacing)
		{
			// Supprime l'ancienne tour temporaire
			currentTower.QueueFree();
			currentTower = null;
			currentTowerScene = null;
			currentPrice = 100;
			isPlacing = false;
			GD.Print("Ancienne tour supprimée");
		}

		// Utiliser les paramètres fournis ou les valeurs par défaut pour la compatibilité
		if (towerScene == null)
		{
			towerScene = TowerScene; // Fallback sur la scène par défaut
		}
		
		if (towerScene == null)
		{
			GD.PrintErr("Aucune scène de tour définie !");
			return;
		}

		// Masquer le bouton restart pendant le placement
		if (BaseManager.Instance != null)
		{
			BaseManager.Instance.SetRestartButtonVisible(false);
		}

		isPlacing = true;
		currentTowerScene = towerScene;
		currentPrice = price;
		currentTower = towerScene.Instantiate<Tower>();
		towersParent.AddChild(currentTower);
		GD.Print("Nouvelle tour créée et ajoutée au parent - Type: " + towerScene.ResourcePath + ", Prix: " + price);
	}

	private void PlaceTower()
	{
		if (currentTower == null || !isPlacing)
		{
			GD.PrintErr("Tentative de placement sans tour en cours !");
			return;
		}

		// Vérifier si le joueur a assez d'or
		if (gold != null && !gold.IsEnoughCoin(currentPrice))
		{
			GD.Print("Pas assez d'or pour placer cette tour !");
			return;
		}

		// Retirer l'or
		if (gold != null)
		{
			gold.SpendCoins(currentPrice);
			GD.Print("Or retiré: " + currentPrice);
		}

		// Finaliser la tour
		// Marquer la tour comme placée AVANT tout pour éviter que _Process() continue à modifier la tour
		currentTower.IsPlaced = true;
		
		// Fixer immédiatement le modulate sur TOUS les nœuds (parent + enfants récursivement)
		// Cela garantit qu'aucun clignotement ne se produit
		SetModulateRecursive(currentTower, new Color(1, 1, 1, 1));
		
		// Ajouter au groupe avant de nettoyer les références
		currentTower.AddToGroup("towers");
		
		// Nettoyer l'état de placement AVANT de mettre currentTower à null
		// pour éviter que _Process() continue à modifier la tour
		isPlacing = false;
		currentTower = null;
		currentTowerScene = null;
		currentPrice = 100;

		// Réafficher le bouton restart
		if (BaseManager.Instance != null)
		{
			BaseManager.Instance.SetRestartButtonVisible(true);
		}

		GD.Print("Tour placée avec succès !");
	}

	private void CancelTowerPlacement()
	{
		if (currentTower != null)
		{
			currentTower.QueueFree();
			currentTower = null;
		}
		
		currentTowerScene = null;
		currentPrice = 100;
		isPlacing = false;

		// Réafficher le bouton restart
		if (BaseManager.Instance != null)
		{
			BaseManager.Instance.SetRestartButtonVisible(true);
		}

		GD.Print("Placement de tour annulé");
	}

	public override void _Process(double delta)
	{
		// Ne modifier le modulate que si on est en train de placer ET que la tour n'est pas encore placée
		if (isPlacing && currentTower != null && !currentTower.IsPlaced)
		{
			Vector2 mousePos = GetGlobalMousePosition();
			currentTower.GlobalPosition = mousePos;

			// ✅ Feedback visuel : rouge si position invalide
			bool isValid = IsValidPlacement(currentTower.GlobalPosition);
			if (isValid)
			{
				currentTower.Modulate = new Color(1, 1, 1, 0.8f); // blanc transparent
			}
			else
			{
				currentTower.Modulate = new Color(1, 0.2f, 0.2f, 0.5f); // rouge transparent
			}
		}
	}

	public override void _Input(InputEvent @event)
	{
		// IMPORTANT : Si on clique sur un contrôle UI interactif (bouton, etc.), ne rien faire dans _Input
		// Cela permet aux boutons de recevoir les événements via leurs signaux
		// MAIS on doit permettre les clics sur la carte pour le placement des tours
		if (@event is InputEventMouseButton mouseEventUI && mouseEventUI.Pressed)
		{
			Control hoveredControl = GetViewport().GuiGetHoveredControl();
			if (hoveredControl != null && hoveredControl != this)
			{
				// Vérifier si c'est vraiment un contrôle UI interactif (bouton, texture button)
				// Ne pas bloquer les panels ou autres contrôles non-interactifs
				if (hoveredControl is Button || hoveredControl is TextureButton)
				{
					// Si on clique sur un bouton, laisser le bouton gérer l'événement
					return;
				}
				// Pour les autres contrôles (panels, etc.), continuer le traitement
				// pour permettre le placement des tours même si on survole un panel
			}
		}
		
		// Gestion de la touche Échap pour annuler le placement
		if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
		{
			if (isPlacing && currentTower != null)
			{
				GD.Print("=== ANNULATION DU PLACEMENT (ÉCHAP) ===");
				CancelTowerPlacement();
				return;
			}
		}

		// Debug : capturer TOUS les clics pour diagnostiquer
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
		{
			if (mouseEvent.ButtonIndex == MouseButton.Left)
			{
				GD.Print("=== CLIC GAUCHE DÉTECTÉ ===");
				Vector2 mousePos = GetGlobalMousePosition();
				GD.Print("Position clic: " + mousePos);
				
				Control hoveredControl = GetViewport().GuiGetHoveredControl();
				GD.Print("Contrôle survolé au clic: " + (hoveredControl != null ? hoveredControl.Name + " (" + hoveredControl.GetType().Name + ")" : "Aucun"));
				
				// Vérifier si on clique sur l'UI d'actions (ne pas traiter ce clic)
				if (towerActionUI != null && towerActionUI.Visible)
				{
					GD.Print("UI d'actions visible - vérification du clic");
					GD.Print("Position de l'UI: " + towerActionUI.GlobalPosition);
					GD.Print("Rect de l'UI: " + towerActionUI.GetGlobalRect());
					
					// Vérifier si le clic est sur un bouton de l'UI d'actions
					if (hoveredControl != null)
					{
						GD.Print("Contrôle survolé: " + hoveredControl.Name);
						// Vérifier si le contrôle survolé est dans l'UI d'actions
						Node parent = hoveredControl;
						int depth = 0;
						while (parent != null && depth < 10)
						{
							if (parent == towerActionUI)
							{
								GD.Print("Clic sur l'UI d'actions - ne pas traiter comme placement");
								return; // Ne pas traiter ce clic pour le placement
							}
							parent = parent.GetParent();
							depth++;
						}
					}
					
					// Vérifier si le clic est dans l'UI avec GetGlobalRect
					Rect2 uiRect = towerActionUI.GetGlobalRect();
					GD.Print("Rect de l'UI: " + uiRect);
					GD.Print("Point de clic: " + mousePos);
					GD.Print("Point dans rect: " + uiRect.HasPoint(mousePos));
					
					if (uiRect.HasPoint(mousePos))
					{
						GD.Print("Clic détecté dans le rect de l'UI d'actions");
						// Calculer les positions relatives des boutons
						var upgradeButton = towerActionUI.GetNodeOrNull<Button>("ActionButtons/UpgradeButton");
						var deleteButton = towerActionUI.GetNodeOrNull<Button>("ActionButtons/DeleteButton");
						
						// Position relative dans le panel
						Vector2 localPos = mousePos - towerActionUI.GlobalPosition;
						GD.Print("Position locale dans l'UI: " + localPos);
						
						// Les boutons sont dans un VBoxContainer centré
						// Position approximative : premier bouton en haut, second en bas
						// Le VBoxContainer est centré dans le panel
						
						if (upgradeButton != null)
						{
							Rect2 upgradeRect = upgradeButton.GetRect();
							Vector2 upgradeGlobalPos = upgradeButton.GlobalPosition;
							Rect2 upgradeGlobalRect = new Rect2(upgradeGlobalPos, upgradeRect.Size);
							GD.Print("Rect bouton Améliorer: " + upgradeGlobalRect);
							
							if (upgradeGlobalRect.HasPoint(mousePos))
							{
								GD.Print("Clic direct sur bouton Améliorer détecté !");
								OnUpgradeButtonPressed();
								return;
							}
						}
						
						if (deleteButton != null)
						{
							Rect2 deleteRect = deleteButton.GetRect();
							Vector2 deleteGlobalPos = deleteButton.GlobalPosition;
							Rect2 deleteGlobalRect = new Rect2(deleteGlobalPos, deleteRect.Size);
							GD.Print("Rect bouton Supprimer: " + deleteGlobalRect);
							
							if (deleteGlobalRect.HasPoint(mousePos))
							{
								GD.Print("Clic direct sur bouton Supprimer détecté !");
								OnDeleteButtonPressed();
								return;
							}
						}
						
						GD.Print("Clic sur l'UI mais pas sur un bouton spécifique - retour anticipé");
						return;
					}
					else
					{
						// Si on clique ailleurs que sur l'UI, fermer l'UI
						GD.Print("Clic hors de l'UI - fermeture");
						HideTowerActionUI();
					}
				}
				
				
				// Gestion du placement des tours
				if (isPlacing && currentTower != null)
				{
					GD.Print("=== TENTATIVE DE PLACEMENT DE TOUR ===");
					GD.Print("Position de clic: " + mousePos);
					
					// Vérifier si on clique sur l'UI (éviter de placer sur l'UI)
					Control uiControl = GetViewport().GuiGetHoveredControl();
					if (uiControl != null && uiControl.IsInGroup("ui"))
					{
						GD.Print("Clic sur l'UI - placement annulé");
						return;
					}
					
					// Vérifier si la position est valide
					if (IsValidPlacement(mousePos))
					{
						GD.Print("Position valide - placement confirmé");
						PlaceTower();
					}
					else
					{
						GD.Print("Position invalide - placement refusé");
					}
					return; // Important : éviter la détection des boutons pendant le placement
				}
				
				// Détection spécifique pour les clics dans la zone des boutons de tours
				// SEULEMENT si on n'est pas déjà en train de placer une tour
				if (!isPlacing && mousePos.X >= 1780 && mousePos.X <= 1870 && mousePos.Y >= 0 && mousePos.Y <= 532)
				{
					GD.Print("=== CLIC DANS LA ZONE DES BOUTONS DE TOURS ===");
					
					// Déterminer quel bouton a été cliqué basé sur la position Y
					int buttonIndex = (int)(mousePos.Y / 134); // Chaque bouton fait 134 pixels de haut
					GD.Print("Index du bouton calculé: " + buttonIndex);
					
					// Déclencher le bon signal basé sur l'index
					switch (buttonIndex)
					{
						case 0: // TowerButton
							GD.Print("Déclenchement forcé du signal TowerButton");
							OnTowerButtonPressed(TowerScene, towerPrice);
							break;
						case 1: // CatapultButton
							GD.Print("Déclenchement forcé du signal CatapultButton");
							OnTowerButtonPressed(CatapultScene, catapultPrice);
							break;
						case 2: // GuardianButton
							GD.Print("Déclenchement forcé du signal GuardianButton");
							OnTowerButtonPressed(GuardianScene, guardianPrice);
							break;
						case 3: // MageButton
							GD.Print("Déclenchement forcé du signal MageButton");
							OnTowerButtonPressed(MageScene, magePrice);
							break;
						default:
							GD.Print("Index de bouton invalide: " + buttonIndex);
							break;
					}
				}
			}
			else if (mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.Pressed)
			{
				GD.Print("=== CLIC DROIT DÉTECTÉ PAR UIMANAGER ===");
				Vector2 mousePosRight = GetGlobalMousePosition();
				GD.Print("Position clic droit: " + mousePosRight);
				
				// Si l'UI d'actions est visible, fermer l'UI au clic droit ailleurs
				if (towerActionUI != null && towerActionUI.Visible)
				{
					if (!towerActionUI.GetGlobalRect().HasPoint(mousePosRight))
					{
						HideTowerActionUI();
					}
					return; // Ne pas continuer si on vient de fermer l'UI
				}
				
				// Vérifier si on clique sur une tour
				Tower clickedTower = FindTowerAtPosition(mousePosRight);
				if (clickedTower != null && clickedTower.IsPlaced)
				{
					GD.Print("Tour trouvée au clic droit ! Affichage de l'UI d'actions");
					ShowTowerActionUI(clickedTower, clickedTower.GlobalPosition);
				}
				else
				{
					GD.Print("Aucune tour trouvée au clic droit");
				}
			}
			}
		}

	// Méthode pour capturer les événements non gérés par l'UI
	public override void _UnhandledInput(InputEvent @event)
	{
		// Debug : vérifier que _UnhandledInput est appelé
		if (@event is InputEventMouseButton mouseEventUnhandled)
		{
			GD.Print($"_UnhandledInput appelé avec événement souris - Pressed: {mouseEventUnhandled.Pressed}, Button: {mouseEventUnhandled.ButtonIndex}");
		}
		
		// Si _Input n'est pas appelé, utiliser _UnhandledInput pour les clics
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
		{
			Vector2 mousePos = GetGlobalMousePosition();
			
			
			// Gestion du placement des tours
			if (isPlacing && currentTower != null)
			{
				GD.Print("=== TENTATIVE DE PLACEMENT DE TOUR (via _UnhandledInput) ===");
				GD.Print("Position de clic: " + mousePos);
				
				// Vérifier si la position est valide
				if (IsValidPlacement(mousePos))
				{
					GD.Print("Position valide - placement confirmé");
					PlaceTower();
					GetViewport().SetInputAsHandled();
				}
				else
				{
					GD.Print("Position invalide - placement refusé");
				}
			}
		}
	}
	

	private bool IsValidPlacement(Vector2 position)
	{
		GD.Print("Vérification placement à: " + position);
		
		Area2D pathArea = GetNodeOrNull<Area2D>("../Path2D/NoBuildZone");
		GD.Print("Zone interdite trouvée: " + (pathArea != null));

		if (pathArea != null)
		{
			foreach (Node child in pathArea.GetChildren())
			{
				if (child is CollisionShape2D shape && shape.Shape is RectangleShape2D rect)
				{
					Vector2 shapePos = shape.GlobalPosition;
					Vector2 halfExtents = rect.Size / 1.5f;
					Vector2 basePosition = position + new Vector2(0, 35);

					if (basePosition.X > shapePos.X - halfExtents.X
						&& basePosition.X < shapePos.X + halfExtents.X
						&& basePosition.Y > shapePos.Y - halfExtents.Y
						&& basePosition.Y < shapePos.Y + halfExtents.Y)
					{
						GD.Print("Placement bloqué par zone interdite");
						return false;
					}
				}
			}
		}

		bool overlapping = IsOverlappingOtherTowers(position);
		GD.Print("Chevauchement avec autres tours: " + overlapping);
		
		if (overlapping)
			return false;

		GD.Print("Placement valide !");
		return true;
	}

	private bool IsOverlappingOtherTowers(Vector2 position)
	{
		foreach (Node tower in GetTree().GetNodesInGroup("towers"))
		{
			if (tower is Node2D towerNode && towerNode.GlobalPosition.DistanceSquaredTo(position) < 2500)
				return true;
		}
		return false;
	}

	// Fonction récursive pour fixer le modulate sur tous les nœuds enfants
	private void SetModulateRecursive(Node node, Color modulate)
	{
		if (node is CanvasItem canvasItem)
		{
			canvasItem.Modulate = modulate;
		}
		
		// Parcourir récursivement tous les enfants
		foreach (Node child in node.GetChildren())
		{
			SetModulateRecursive(child, modulate);
		}
	}

	private Tower FindTowerInHierarchy(Node node)
	{
		// Chercher une tour en remontant la hiérarchie
		Node current = node;
		while (current != null)
		{
			if (current is Tower tower)
			{
				return tower;
			}
			current = current.GetParent();
		}
		return null;
	}

	private void UpgradeTower(Tower tower)
	{
		GD.Print("=== AMÉLIORATION DE TOUR VIA UIMANAGER ===");
		GD.Print("Tour: " + tower.Name);
		GD.Print("Position: " + tower.GlobalPosition);
		
		// Appeler la méthode publique d'amélioration de la tour
		bool success = tower.TryUpgrade();
		if (success)
		{
			GD.Print("Amélioration réussie !");
			HideTowerActionUI(); // Fermer l'UI après amélioration
		}
		else
		{
			GD.Print("Échec de l'amélioration");
		}
	}

	// Créer l'UI pour les actions sur les tours
	private void CreateTowerActionUI()
	{
		// Créer un Panel pour contenir les boutons
		towerActionUI = new Panel();
		towerActionUI.Name = "TowerActionUI";
		towerActionUI.Visible = false;
		towerActionUI.MouseFilter = Control.MouseFilterEnum.Stop; // Empêcher les clics de passer à travers
		towerActionUI.ZIndex = 100; // S'assurer que l'UI est au-dessus
		
		// Style du Panel : fond sombre avec bordure et coins arrondis
		var panelStyle = new StyleBoxFlat();
		panelStyle.BgColor = new Color(0.15f, 0.15f, 0.2f, 0.95f); // Fond sombre semi-transparent
		panelStyle.BorderColor = new Color(0.4f, 0.4f, 0.5f, 1.0f); // Bordure gris clair
		panelStyle.BorderWidthTop = 2;
		panelStyle.BorderWidthRight = 2;
		panelStyle.BorderWidthBottom = 2;
		panelStyle.BorderWidthLeft = 2;
		panelStyle.CornerRadiusTopLeft = 8;
		panelStyle.CornerRadiusTopRight = 8;
		panelStyle.CornerRadiusBottomLeft = 8;
		panelStyle.CornerRadiusBottomRight = 8;
		towerActionUI.AddThemeStyleboxOverride("panel", panelStyle);
		
		// Taille fixe pour le panel (suffisante pour contenir les boutons avec marge confortable)
		towerActionUI.CustomMinimumSize = new Vector2(180, 100);
		towerActionUI.Size = new Vector2(180, 100);
		
		// Créer un VBoxContainer pour organiser les boutons verticalement
		var vbox = new VBoxContainer();
		vbox.Name = "ActionButtons";
		
		// Configuration simple du VBoxContainer : remplir tout le panel avec un padding généreux
		vbox.AnchorLeft = 0.0f;
		vbox.AnchorTop = 0.0f;
		vbox.AnchorRight = 1.0f;
		vbox.AnchorBottom = 1.0f;
		vbox.OffsetLeft = 12; // Padding gauche augmenté
		vbox.OffsetTop = 12; // Padding haut augmenté
		vbox.OffsetRight = -12; // Padding droit augmenté
		vbox.OffsetBottom = -12; // Padding bas augmenté
		
		// Espacement entre les boutons
		vbox.AddThemeConstantOverride("separation", 6);
		
		// Ajouter le VBoxContainer au Panel AVANT d'ajouter les boutons
		towerActionUI.AddChild(vbox);
		
		// Style pour le bouton Améliorer
		var upgradeButtonStyle = new StyleBoxFlat();
		upgradeButtonStyle.BgColor = new Color(0.2f, 0.6f, 0.3f, 1.0f); // Vert pour amélioration
		upgradeButtonStyle.BorderColor = new Color(0.3f, 0.7f, 0.4f, 1.0f);
		upgradeButtonStyle.BorderWidthTop = 1;
		upgradeButtonStyle.BorderWidthRight = 1;
		upgradeButtonStyle.BorderWidthBottom = 1;
		upgradeButtonStyle.BorderWidthLeft = 1;
		upgradeButtonStyle.CornerRadiusTopLeft = 4;
		upgradeButtonStyle.CornerRadiusTopRight = 4;
		upgradeButtonStyle.CornerRadiusBottomLeft = 4;
		upgradeButtonStyle.CornerRadiusBottomRight = 4;
		
		var upgradeButtonHoverStyle = new StyleBoxFlat();
		upgradeButtonHoverStyle.BgColor = new Color(0.25f, 0.7f, 0.35f, 1.0f); // Vert plus clair au survol
		upgradeButtonHoverStyle.BorderColor = new Color(0.4f, 0.8f, 0.5f, 1.0f);
		upgradeButtonHoverStyle.BorderWidthTop = 1;
		upgradeButtonHoverStyle.BorderWidthRight = 1;
		upgradeButtonHoverStyle.BorderWidthBottom = 1;
		upgradeButtonHoverStyle.BorderWidthLeft = 1;
		upgradeButtonHoverStyle.CornerRadiusTopLeft = 4;
		upgradeButtonHoverStyle.CornerRadiusTopRight = 4;
		upgradeButtonHoverStyle.CornerRadiusBottomLeft = 4;
		upgradeButtonHoverStyle.CornerRadiusBottomRight = 4;
		
		// Bouton Améliorer
		var upgradeButton = new Button();
		upgradeButton.Name = "UpgradeButton";
		upgradeButton.Text = "Améliorer";
		upgradeButton.CustomMinimumSize = new Vector2(0, 32); // Largeur flexible, hauteur fixe
		upgradeButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill; // Remplir la largeur
		upgradeButton.MouseFilter = Control.MouseFilterEnum.Stop;
		upgradeButton.Pressed += OnUpgradeButtonPressed;
		upgradeButton.AddThemeStyleboxOverride("normal", upgradeButtonStyle);
		upgradeButton.AddThemeStyleboxOverride("hover", upgradeButtonHoverStyle);
		upgradeButton.AddThemeStyleboxOverride("pressed", upgradeButtonHoverStyle);
		upgradeButton.AddThemeColorOverride("font_color", new Color(1, 1, 1, 1)); // Texte blanc
		upgradeButton.AddThemeColorOverride("font_hover_color", new Color(1, 1, 1, 1));
		upgradeButton.AddThemeFontSizeOverride("font_size", 14);
		GD.Print("Bouton Améliorer créé et connecté");
		vbox.AddChild(upgradeButton);
		
		// Style pour le bouton Supprimer
		var deleteButtonStyle = new StyleBoxFlat();
		deleteButtonStyle.BgColor = new Color(0.7f, 0.2f, 0.2f, 1.0f); // Rouge pour suppression
		deleteButtonStyle.BorderColor = new Color(0.8f, 0.3f, 0.3f, 1.0f);
		deleteButtonStyle.BorderWidthTop = 1;
		deleteButtonStyle.BorderWidthRight = 1;
		deleteButtonStyle.BorderWidthBottom = 1;
		deleteButtonStyle.BorderWidthLeft = 1;
		deleteButtonStyle.CornerRadiusTopLeft = 4;
		deleteButtonStyle.CornerRadiusTopRight = 4;
		deleteButtonStyle.CornerRadiusBottomLeft = 4;
		deleteButtonStyle.CornerRadiusBottomRight = 4;
		
		var deleteButtonHoverStyle = new StyleBoxFlat();
		deleteButtonHoverStyle.BgColor = new Color(0.8f, 0.25f, 0.25f, 1.0f); // Rouge plus clair au survol
		deleteButtonHoverStyle.BorderColor = new Color(0.9f, 0.4f, 0.4f, 1.0f);
		deleteButtonHoverStyle.BorderWidthTop = 1;
		deleteButtonHoverStyle.BorderWidthRight = 1;
		deleteButtonHoverStyle.BorderWidthBottom = 1;
		deleteButtonHoverStyle.BorderWidthLeft = 1;
		deleteButtonHoverStyle.CornerRadiusTopLeft = 4;
		deleteButtonHoverStyle.CornerRadiusTopRight = 4;
		deleteButtonHoverStyle.CornerRadiusBottomLeft = 4;
		deleteButtonHoverStyle.CornerRadiusBottomRight = 4;
		
		// Bouton Supprimer
		var deleteButton = new Button();
		deleteButton.Name = "DeleteButton";
		deleteButton.Text = "Supprimer";
		deleteButton.CustomMinimumSize = new Vector2(0, 32); // Largeur flexible, hauteur fixe
		deleteButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill; // Remplir la largeur
		deleteButton.MouseFilter = Control.MouseFilterEnum.Stop;
		deleteButton.Pressed += OnDeleteButtonPressed;
		deleteButton.AddThemeStyleboxOverride("normal", deleteButtonStyle);
		deleteButton.AddThemeStyleboxOverride("hover", deleteButtonHoverStyle);
		deleteButton.AddThemeStyleboxOverride("pressed", deleteButtonHoverStyle);
		deleteButton.AddThemeColorOverride("font_color", new Color(1, 1, 1, 1)); // Texte blanc
		deleteButton.AddThemeColorOverride("font_hover_color", new Color(1, 1, 1, 1));
		deleteButton.AddThemeFontSizeOverride("font_size", 14);
		GD.Print("Bouton Supprimer créé et connecté");
		vbox.AddChild(deleteButton);
		
		// Ajouter l'UI au UiManager (il sera positionné dynamiquement)
		AddChild(towerActionUI);
		
		GD.Print("UI d'actions créée avec succès");
	}

	// Afficher l'UI d'actions sur une tour
	public void ShowTowerActionUI(Tower tower, Vector2 towerPosition)
	{
		if (tower == null || !tower.IsPlaced)
			return;
		
		selectedTower = tower;
		
		if (towerActionUI == null)
		{
			CreateTowerActionUI();
		}
		
		// S'assurer que l'UI est bien dans la scène
		if (towerActionUI.GetParent() == null)
		{
			AddChild(towerActionUI);
			GD.Print("UI d'actions ajoutée à la scène");
		}
		
		// Convertir la position de la tour en coordonnées UI (coordonnées locales du UiManager)
		Vector2 screenPos = towerPosition;
		
		// S'assurer que la taille est bien définie
		if (towerActionUI.Size == Vector2.Zero)
		{
			towerActionUI.Size = new Vector2(180, 100);
		}
		
		// Positionner l'UI au-dessus de la tour (centré horizontalement)
		towerActionUI.Position = screenPos + new Vector2(-towerActionUI.Size.X / 2, -80);
		towerActionUI.Visible = true;
		towerActionUI.ZIndex = 100;
		
		GD.Print("UI d'actions affichée à: " + towerActionUI.GlobalPosition);
		GD.Print("UI d'actions - Position: " + towerActionUI.Position);
		GD.Print("UI d'actions - Visible: " + towerActionUI.Visible);
		GD.Print("UI d'actions - ZIndex: " + towerActionUI.ZIndex);
		
		// Mettre à jour le texte du bouton améliorer selon le niveau
		var upgradeButton = towerActionUI.GetNodeOrNull<Button>("ActionButtons/UpgradeButton");
		var deleteButton = towerActionUI.GetNodeOrNull<Button>("ActionButtons/DeleteButton");
		
		GD.Print("Bouton Améliorer trouvé: " + (upgradeButton != null));
		GD.Print("Bouton Supprimer trouvé: " + (deleteButton != null));
		
		if (upgradeButton != null)
		{
			GD.Print("Bouton Améliorer - Position: " + upgradeButton.GlobalPosition);
			GD.Print("Bouton Améliorer - Size: " + upgradeButton.Size);
			GD.Print("Bouton Améliorer - MouseFilter: " + upgradeButton.MouseFilter);
		}
		if (upgradeButton != null)
		{
			bool canUpgrade = tower.level < 5;
			upgradeButton.Disabled = !canUpgrade;
			
			if (canUpgrade)
			{
				var gold = GetNodeOrNull<Goldmanager>("./Panel/GoldManager");
				if (gold != null)
				{
					int upgradePrice = tower.GetUpgradePrice();
					if (upgradePrice > 0)
					{
						bool hasEnoughGold = gold.IsEnoughCoin(upgradePrice);
						upgradeButton.Disabled = !hasEnoughGold;
						upgradeButton.Text = $"Améliorer ({upgradePrice} or)";
					}
					else
					{
						upgradeButton.Text = "Améliorer";
					}
				}
				else
				{
					upgradeButton.Text = "Améliorer";
				}
			}
			else
			{
				upgradeButton.Text = "Niveau Max";
			}
		}
	}

	// Cacher l'UI d'actions
	public void HideTowerActionUI()
	{
		if (towerActionUI != null)
		{
			towerActionUI.Visible = false;
			selectedTower = null;
		}
	}

	// Appelé quand le bouton Améliorer est pressé
	private void OnUpgradeButtonPressed()
	{
		GD.Print("=== BOUTON AMÉLIORER PRESSÉ ===");
		if (selectedTower != null)
		{
			GD.Print("Tour sélectionnée: " + selectedTower.Name);
			UpgradeTower(selectedTower);
		}
		else
		{
			GD.PrintErr("Aucune tour sélectionnée !");
		}
	}

	// Appelé quand le bouton Supprimer est pressé
	private void OnDeleteButtonPressed()
	{
		GD.Print("=== BOUTON SUPPRIMER PRESSÉ ===");
		if (selectedTower != null)
		{
			GD.Print("Tour sélectionnée: " + selectedTower.Name);
			DeleteTower(selectedTower);
		}
		else
		{
			GD.PrintErr("Aucune tour sélectionnée !");
		}
	}

	// Supprimer une tour
	private void DeleteTower(Tower tower)
	{
		if (tower == null || !tower.IsPlaced)
			return;
		
		GD.Print("=== SUPPRESSION DE TOUR ===");
		GD.Print("Tour: " + tower.Name);
		GD.Print("Position: " + tower.GlobalPosition);
		
		// Retirer du groupe
		tower.RemoveFromGroup("towers");
		
		// Supprimer le nœud
		tower.QueueFree();
		
		// Fermer l'UI
		HideTowerActionUI();
		
		GD.Print("Tour supprimée !");
	}

	// Trouver une tour à une position donnée
	private Tower FindTowerAtPosition(Vector2 position)
	{
		// Méthode 1 : Chercher dans le groupe "towers" par distance
		var towers = GetTree().GetNodesInGroup("towers");
		GD.Print("Nombre de tours dans le groupe: " + towers.Count);
		
		foreach (Node towerNode in towers)
		{
			if (towerNode is Tower tower && tower.IsPlaced)
			{
				Vector2 towerCenter = tower.GlobalPosition + new Vector2(0, 27);
				float distance = position.DistanceTo(towerCenter);
				GD.Print($"Tour {tower.Name} à distance: {distance} (seuil: 50)");
				
				// Augmenter le seuil pour faciliter le clic
				if (distance < 50f)
				{
					GD.Print("Tour trouvée !");
					return tower;
				}
			}
		}
		
		// Méthode 2 : Utiliser une requête physique
		var spaceState = GetWorld2D().DirectSpaceState;
		var query = new PhysicsPointQueryParameters2D();
		query.Position = position;
		query.CollisionMask = 1; // Layer 1 pour les tours
		var result = spaceState.IntersectPoint(query);
		
		GD.Print("Nombre d'objets touchés par query: " + result.Count);
		
		foreach (var hit in result)
		{
			var collider = hit["collider"];
			if (collider.AsGodotObject() is CollisionShape2D collisionShape)
			{
				Node parent = collisionShape.GetParent();
				Tower tower = FindTowerInHierarchy(parent);
				if (tower != null && tower.IsPlaced)
				{
					GD.Print("Tour trouvée via physics query !");
					return tower;
				}
			}
		}
		
		return null;
	}


}
