using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class WaveManager : Node2D
{
	[Export] public PackedScene EnemyScene {get; set;}
	[Export] public Path2D EnemyPath {get; set;}
	
	private List<PathFollow2D> _activeEnemies = new List<PathFollow2D>();
	private List<Enemy> _enemies  =new List<Enemy>();
	private int _waveNumber = 0;
	private int _enemyNumber = 5; // Commence à 5 pour faciliter le début
	private int _idGenerator = 0;
	private float _pathLength = 3623.46f;
	private RandomNumberGenerator rng = new RandomNumberGenerator();
	private double randomValue = 0.0;
	private float  spwanInterval = 0.8f; // Réduit de 1.0 à 0.8 pour vagues plus rapides
	private Label waveLabel;
	private const int MAX_WAVES = 10;
	
	public override void _Ready()
	{
		rng.Randomize();
		EnsureBaseManager();
		SetupWaveLabel();
		UpdateWaveLabel();
		StartWave(_enemyNumber);
	}
	public async void StartWave(int enemyCount)
	{
		_waveNumber++;
		UpdateWaveLabel();
		GD.Print($"Lancement de la vague {_waveNumber}... ");
		
		// Vérifier la condition de victoire
		if (_waveNumber > MAX_WAVES)
		{
			OnVictory();
			return;
		}
		
		for (int i = 0; i < enemyCount; i++)
		{
			SpwanEnemy();
			_idGenerator += 1;
			await Task.Delay((int)(spwanInterval * 1000));
		}
	}
	
	public override async void  _Process(double delta)
	{
		if (_enemies.Count == 0) {
			Task.Delay((int)(spwanInterval * 1000));
			// Multiplicateur progressif : plus agressif après la vague 3
			float multiplier = _waveNumber <= 3 ? 1.2f : 1.3f; // 1.2 pour début, 1.3 après vague 3
			_enemyNumber = (int)(_enemyNumber * multiplier);
			StartWave(_enemyNumber);
		}
		
	//	for (int i =_activeEnemies.Count -1; i >=0; i--)
	//	{
	//		PathFollow2D pathFollow =_activeEnemies [i];
	//		
	//		if (pathFollow !=null)
	//		{
				
				
	//			if(pathFollow.ProgressRatio >= 1.0f)
	//			{
	//				GD.Print("Ennemi arrivé au bout du chemin!");
						
	//				_activeEnemies.RemoveAt(i);
	//				pathFollow.QueueFree();
	//			}
 	//		}
	//	}
	}
	
	private void SpwanEnemy()
	{
		if (EnemyScene == null || EnemyPath == null )
		{
			GD.PrintErr("EnemyScene ou EnemyPath n'est pas défini!");
			return;
		}
		
		PathFollow2D pathFollow = new PathFollow2D();
		pathFollow.Progress = 0;
		pathFollow.Loop = false;
		
		Node2D newEnemy = (Node2D)EnemyScene.Instantiate();
		Enemy enemyInstance = newEnemy as Enemy;
		_enemies.Add(enemyInstance);
		
		randomValue = rng.Randf();
		
		// Scaling progressif : les ennemis deviennent plus forts avec les vagues
		// Multiplicateur basé sur la vague (1.0 pour vague 1, augmente progressivement)
		float waveMultiplier = 1.0f + (_waveNumber - 1) * 0.12f; // +12% par vague après la première
		if (waveMultiplier < 1.0f) waveMultiplier = 1.0f; // Minimum 1.0 pour la vague 1
		
		// Statistiques de base (plus faibles au début, augmentent avec les vagues)
		int wolfHP = (int)(120 * waveMultiplier);      // Base réduite de 150 à 120
		int wolfDmg = (int)(70 * waveMultiplier);     // Base réduite de 80 à 70
		int wolfReward = 12; // Récompense réduite de 20 à 12 (pas de scaling)
		
		int orcHP = (int)(300 * waveMultiplier);      // Base réduite de 400 à 300
		int orcDmg = (int)(150 * waveMultiplier);     // Base réduite de 200 à 150
		int orcReward = 30; // Récompense réduite de 60 à 30 (pas de scaling)
		
		int slimeHP = (int)(100 * waveMultiplier);    // Base réduite de 120 à 100
		int slimeDmg = (int)(50 * waveMultiplier);    // Base réduite de 60 à 50
		int slimeReward = 12; // Récompense réduite de 20 à 12 (pas de scaling)
		
		int beeHP = (int)(60 * waveMultiplier);       // Base réduite de 80 à 60
		int beeDmg = (int)(35 * waveMultiplier);      // Base réduite de 40 à 35
		int beeReward = 10; // Récompense réduite de 15 à 10 (pas de scaling)
		
		// Statistiques avec scaling progressif et récompenses réduites
		if (randomValue >= 0.0 && randomValue <= 0.25)
			enemyInstance.Initialize("Wolf", wolfHP, wolfDmg, 110.0f, pathFollow, wolfReward, _idGenerator);
		else if (randomValue > 0.25 && randomValue <= 0.5)
			enemyInstance.Initialize("Orc", orcHP, orcDmg, 60.0f, pathFollow, orcReward, _idGenerator);
		else if (randomValue > 0.5 && randomValue <= 0.75)
			enemyInstance.Initialize("Slime", slimeHP, slimeDmg, 120.0f, pathFollow, slimeReward, _idGenerator);
		else if (randomValue > 0.75 && randomValue <= 1)
			enemyInstance.Initialize("Bee", beeHP, beeDmg, 140.0f, pathFollow, beeReward, _idGenerator);
		
		enemyInstance.Connect("EnemyDied", new Callable(this, nameof(OnEnemyDied)));
		pathFollow.AddChild(enemyInstance);
		EnemyPath.AddChild(pathFollow);
		_activeEnemies.Add(pathFollow);
	}
	
	public void OnEnemyDied(Enemy enemy) {
		_enemies.Remove(enemy);
		//pathFollow.QueueFree();
	}

	private void EnsureBaseManager()
	{
		// Cherche un BaseManager existant dans la scène
		var existing = GetTree().Root.FindChild("BaseManager", true, false) as BaseManager;
		if (existing != null)
			return;
		// Crée et ajoute un BaseManager sous le parent de l’UI et de WaveManager (racine actuelle)
		var baseMgr = new BaseManager();
		baseMgr.Name = "BaseManager";
		GetParent().AddChild(baseMgr);
	}

	private void SetupWaveLabel()
	{
		// Utilise la racine UI pour un positionnement absolu en haut à gauche de l'écran
		var ui = GetNodeOrNull<Control>("../UI");
		if (ui == null)
			return;

		// Récupère un label existant où qu'il soit sous UI
		waveLabel = ui.GetNodeOrNull<Label>("WaveLabel");
		if (waveLabel == null)
		{
			waveLabel = new Label();
			waveLabel.Name = "WaveLabel";
			ui.AddChild(waveLabel);
		}

		// Position absolue en haut à gauche du viewport UI
		waveLabel.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
		waveLabel.Position = new Vector2(10, 10);
		waveLabel.HorizontalAlignment = HorizontalAlignment.Left;
		waveLabel.VerticalAlignment = VerticalAlignment.Top;
		waveLabel.AddThemeFontSizeOverride("font_size", 28);
	}

	private void UpdateWaveLabel()
	{
		if (waveLabel != null)
		{
			waveLabel.Text = $"Vague: {_waveNumber}";
		}
	}

	private void OnVictory()
	{
		GD.Print("=== VICTOIRE - 10 VAGUES TERMINÉES ! ===");
		
		// Notifier le BaseManager pour afficher l'écran de victoire
		if (BaseManager.Instance != null)
		{
			BaseManager.Instance.OnVictory();
		}
	}
}
