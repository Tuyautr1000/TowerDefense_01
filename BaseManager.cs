
using Godot;
using System;

public partial class BaseManager : Node2D
{
	public static BaseManager Instance { get; private set; }

	[Export] public int MaxHealth = 125;
	public int CurrentHealth { get; private set; }

	private Control uiRoot;
	private Control defeatScreen;
	private Control victoryScreen;
	private Button restartButton;
	private Button victoryRestartButton;
	private Button permanentRestartButton;
	private ProgressBar topHealthBar;
	private StyleBoxFlat topBgStyle;
	private StyleBoxFlat topFillStyle;

	public override void _Ready()
	{
		Instance = this;
		CurrentHealth = MaxHealth;
		SetupUIRoot();
		SetupTopHealthBar();
		SetupDefeatUI();
		SetupVictoryUI();
		SetupPermanentRestartButton();
	}

	private void SetupUIRoot()
	{
		uiRoot = GetNodeOrNull<Control>("../UI");
		if (uiRoot == null)
		{
			var canvas = new CanvasLayer();
			AddChild(canvas);
			uiRoot = new Control();
			canvas.AddChild(uiRoot);
		}
		// Assure que l'UI couvre tout l'écran pour que le centrage fonctionne
		uRoot_SetFullRect(uiRoot);
	}

	private void uRoot_SetFullRect(Control c)
	{
		c.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		c.OffsetLeft = 0;
		c.OffsetTop = 0;
		c.OffsetRight = 0;
		c.OffsetBottom = 0;
		// Pas besoin de définir Size manuellement avec FullRect
		// Godot gère automatiquement la taille avec les ancres
	}

	private void SetupTopHealthBar()
	{
		topHealthBar = uiRoot.GetNodeOrNull<ProgressBar>("TopBaseHealthBar");
		if (topHealthBar == null)
		{
			topHealthBar = new ProgressBar();
			topHealthBar.Name = "TopBaseHealthBar";
			uiRoot.AddChild(topHealthBar);
		}

		// Barre en haut, centrée
		topHealthBar.MinValue = 0;
		topHealthBar.MaxValue = MaxHealth;
		topHealthBar.Value = CurrentHealth;
		topHealthBar.CustomMinimumSize = new Vector2(600, 34);
		topHealthBar.SetAnchorsPreset(Control.LayoutPreset.TopWide);
		// Marges symétriques pour centrer visuellement
		topHealthBar.OffsetLeft = 600;
		topHealthBar.OffsetRight = -600;
		topHealthBar.OffsetTop = 12;
		topHealthBar.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		topHealthBar.AddThemeFontSizeOverride("font_size", 22);

		// Styles: fond sombre, remplissage variable
		topBgStyle = new StyleBoxFlat();
		topBgStyle.BgColor = new Color(0, 0, 0, 0.55f);
		topBgStyle.CornerRadiusTopLeft = 6;
		topBgStyle.CornerRadiusTopRight = 6;
		topBgStyle.CornerRadiusBottomLeft = 6;
		topBgStyle.CornerRadiusBottomRight = 6;
		topBgStyle.BorderWidthTop = 2;
		topBgStyle.BorderWidthRight = 2;
		topBgStyle.BorderWidthBottom = 2;
		topBgStyle.BorderWidthLeft = 2;
		topBgStyle.BorderColor = new Color(0, 0, 0, 0.9f);

		topFillStyle = new StyleBoxFlat();
		topFillStyle.CornerRadiusTopLeft = 6;
		topFillStyle.CornerRadiusTopRight = 6;
		topFillStyle.CornerRadiusBottomLeft = 6;
		topFillStyle.CornerRadiusBottomRight = 6;

		topHealthBar.AddThemeStyleboxOverride("background", topBgStyle);
		topHealthBar.AddThemeStyleboxOverride("fill", topFillStyle);

		UpdateTopHealthBar();
	}

	private void UpdateTopHealthBar()
	{
		if (topHealthBar == null) return;
		topHealthBar.MaxValue = MaxHealth;
		topHealthBar.Value = CurrentHealth;

		float ratio = MaxHealth > 0 ? (float)CurrentHealth / (float)MaxHealth : 0f;
		Color fillColor = ratio > 0.6f ? new Color(0.2f, 0.8f, 0.2f)
			: (ratio > 0.3f ? new Color(0.95f, 0.6f, 0.1f) : new Color(0.9f, 0.2f, 0.2f));
		if (topFillStyle != null)
		{
			topFillStyle.BgColor = fillColor;
		}
	}

	private void SetupDefeatUI()
	{
		defeatScreen = new ColorRect();
		(defeatScreen as ColorRect).Color = new Color(0, 0, 0, 0.7f);
		defeatScreen.Visible = false;
		defeatScreen.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		defeatScreen.OffsetLeft = 0;
		defeatScreen.OffsetTop = 0;
		defeatScreen.OffsetRight = 0;
		defeatScreen.OffsetBottom = 0;
		uiRoot.AddChild(defeatScreen);

		var center = new CenterContainer();
		center.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		center.OffsetLeft = 0;
		center.OffsetTop = 0;
		center.OffsetRight = 0;
		center.OffsetBottom = 0;
		defeatScreen.AddChild(center);

		var vbox = new VBoxContainer();
		vbox.Alignment = BoxContainer.AlignmentMode.Center;
		vbox.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		vbox.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
		center.AddChild(vbox);

		var title = new Label();
		title.Text = "Défaite";
		title.HorizontalAlignment = HorizontalAlignment.Center;
		title.VerticalAlignment = VerticalAlignment.Center;
		title.AddThemeFontSizeOverride("font_size", 36);
		title.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		vbox.AddChild(title);

		restartButton = new Button();
		restartButton.Name = "DefeatRestartButton";
		restartButton.Text = "Recommencer";
		restartButton.CustomMinimumSize = new Vector2(260, 44);
		restartButton.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		restartButton.Pressed += OnRestartPressed;
		restartButton.GuiInput += OnDefeatButtonInput;
		vbox.AddChild(restartButton);
		
		GD.Print("Bouton de défaite créé et connecté");
	}

	private void SetupVictoryUI()
	{
		victoryScreen = new ColorRect();
		(victoryScreen as ColorRect).Color = new Color(0, 0, 0, 0.7f);
		victoryScreen.Visible = false;
		victoryScreen.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		victoryScreen.OffsetLeft = 0;
		victoryScreen.OffsetTop = 0;
		victoryScreen.OffsetRight = 0;
		victoryScreen.OffsetBottom = 0;
		uiRoot.AddChild(victoryScreen);

		var center = new CenterContainer();
		center.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		center.OffsetLeft = 0;
		center.OffsetTop = 0;
		center.OffsetRight = 0;
		center.OffsetBottom = 0;
		victoryScreen.AddChild(center);

		var vbox = new VBoxContainer();
		vbox.Alignment = BoxContainer.AlignmentMode.Center;
		vbox.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		vbox.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
		center.AddChild(vbox);

		var title = new Label();
		title.Text = "Victoire !";
		title.HorizontalAlignment = HorizontalAlignment.Center;
		title.VerticalAlignment = VerticalAlignment.Center;
		title.AddThemeFontSizeOverride("font_size", 36);
		title.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		vbox.AddChild(title);

		victoryRestartButton = new Button();
		victoryRestartButton.Name = "VictoryRestartButton";
		victoryRestartButton.Text = "Recommencer";
		victoryRestartButton.CustomMinimumSize = new Vector2(260, 44);
		victoryRestartButton.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		victoryRestartButton.Pressed += OnRestartPressed;
		victoryRestartButton.GuiInput += OnVictoryButtonInput;
		vbox.AddChild(victoryRestartButton);
		
		GD.Print("Bouton de victoire créé et connecté");
	}

	private void SetupPermanentRestartButton()
	{
		permanentRestartButton = new Button();
		permanentRestartButton.Name = "PermanentRestartButton";
		permanentRestartButton.Text = "Recommencer";
		permanentRestartButton.CustomMinimumSize = new Vector2(120, 40);
		permanentRestartButton.AddThemeFontSizeOverride("font_size", 16);
		uiRoot.AddChild(permanentRestartButton);

		// Positionner le bouton en haut à droite
		permanentRestartButton.SetAnchorsPreset(Control.LayoutPreset.TopRight);
		permanentRestartButton.OffsetTop = 20;
		permanentRestartButton.OffsetRight = -20;
		permanentRestartButton.OffsetLeft = -140;
		permanentRestartButton.OffsetBottom = 60;

		// Style du bouton
		var buttonStyle = new StyleBoxFlat();
		buttonStyle.BgColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
		buttonStyle.CornerRadiusTopLeft = 8;
		buttonStyle.CornerRadiusTopRight = 8;
		buttonStyle.CornerRadiusBottomLeft = 8;
		buttonStyle.CornerRadiusBottomRight = 8;
		buttonStyle.BorderWidthTop = 2;
		buttonStyle.BorderWidthRight = 2;
		buttonStyle.BorderWidthBottom = 2;
		buttonStyle.BorderWidthLeft = 2;
		buttonStyle.BorderColor = new Color(0.8f, 0.8f, 0.8f, 0.9f);
		permanentRestartButton.AddThemeStyleboxOverride("normal", buttonStyle);

		// Style au survol
		var hoverStyle = new StyleBoxFlat();
		hoverStyle.BgColor = new Color(0.3f, 0.3f, 0.3f, 0.9f);
		hoverStyle.CornerRadiusTopLeft = 8;
		hoverStyle.CornerRadiusTopRight = 8;
		hoverStyle.CornerRadiusBottomLeft = 8;
		hoverStyle.CornerRadiusBottomRight = 8;
		hoverStyle.BorderWidthTop = 2;
		hoverStyle.BorderWidthRight = 2;
		hoverStyle.BorderWidthBottom = 2;
		hoverStyle.BorderWidthLeft = 2;
		hoverStyle.BorderColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
		permanentRestartButton.AddThemeStyleboxOverride("hover", hoverStyle);

		permanentRestartButton.Pressed += OnRestartPressed;
	}

	public void SetRestartButtonVisible(bool visible)
	{
		if (permanentRestartButton != null)
		{
			permanentRestartButton.Visible = visible;
		}
	}

	private void OnDefeatButtonInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
		{
			GD.Print("=== CLIC SUR BOUTON DE DÉFAITE DÉTECTÉ ===");
			OnRestartPressed();
		}
	}

	private void OnVictoryButtonInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
		{
			GD.Print("=== CLIC SUR BOUTON DE VICTOIRE DÉTECTÉ ===");
			OnRestartPressed();
		}
	}

	private void OnRestartPressed()
	{
		GD.Print("=== REDÉMARRAGE DU JEU DEMANDÉ ===");
		
		// S'assurer que le jeu n'est pas en pause
		GetTree().Paused = false;
		GD.Print("Jeu dé-pausé");
		
		// Nettoyer les ressources si nécessaire
		if (Instance != null)
		{
			Instance = null;
			GD.Print("Instance BaseManager nettoyée");
		}
		
		GD.Print("Rechargement de la scène en cours...");
		
		// Recharger la scène
		GetTree().ReloadCurrentScene();
	}

	public void TakeDamage(int amount)
	{
		if (CurrentHealth <= 0)
			return;

		CurrentHealth = Math.Max(0, CurrentHealth - amount);
		UpdateTopHealthBar();
		if (CurrentHealth == 0)
		{
			OnDefeat();
		}
	}

	private void OnDefeat()
	{
		GD.Print("=== DÉFAITE - AFFICHAGE DE L'ÉCRAN ===");
		
		if (defeatScreen != null)
		{
			defeatScreen.Visible = true;
			GD.Print("Écran de défaite affiché");
		}
		
		// Mettre en pause le jeu mais permettre l'interaction avec l'UI
		GetTree().Paused = true;
		
		// S'assurer que le bouton restart est cliquable
		if (restartButton != null)
		{
			restartButton.ProcessMode = Node.ProcessModeEnum.Always;
			GD.Print("Bouton de défaite configuré pour fonctionner en pause");
		}
		
		GD.Print("Jeu mis en pause");
	}

	public void OnVictory()
	{
		GD.Print("=== VICTOIRE - AFFICHAGE DE L'ÉCRAN ===");
		
		if (victoryScreen != null)
		{
			victoryScreen.Visible = true;
			GD.Print("Écran de victoire affiché");
		}
		
		// Mettre en pause le jeu mais permettre l'interaction avec l'UI
		GetTree().Paused = true;
		
		// S'assurer que le bouton restart est cliquable
		if (victoryRestartButton != null)
		{
			victoryRestartButton.ProcessMode = Node.ProcessModeEnum.Always;
			GD.Print("Bouton de victoire configuré pour fonctionner en pause");
		}
		
		GD.Print("Jeu mis en pause");
	}
}
