using Godot;
using System;

public partial class Enemy : Node2D
{
	private AnimatedSprite2D animatedSprite;
	private PathFollow2D pathFollow;
	private Goldmanager gold;
	private float pathSpeed;
	public int ID;
	private int reward;
	
	public int life;
	public int attack;
	public float speed;
	
	public Vector2 Velocity {get; private set; }
	
	[Export] public string EnemyType {get ; set ; }
	
	[Signal] public delegate void EnemyDiedEventHandler(Enemy ensemy);
	
	public void Initialize(string type, int hp, int atk,  float spd, PathFollow2D pathFollowNode, int rwd,int id)
	{
		EnemyType = type;
		life = hp;
		attack = atk;
		speed = spd;
		pathFollow = pathFollowNode;
		pathSpeed = speed;
		reward = rwd;
		ID = id;
	}
	
	public override void _Ready()
	{
		animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		gold = GetNode<Goldmanager>("../../../UI/Panel/GoldManager");
		
		// Configurer l'Area2D pour être détecté par les tours
		var area2D = animatedSprite.GetNode<Area2D>("Area2D");
		if (area2D != null)
		{
			area2D.CollisionLayer = 2; // Layer 2 pour les ennemis
			area2D.CollisionMask = 0; // Pas besoin de masque pour les ennemis
		}
		
		SetupEnemy(EnemyType);
	}
	
	private void SetupEnemy(string type)
	{
		switch (type)
		{
			case "Wolf":
				if (animatedSprite.SpriteFrames.HasAnimation("W_FrontWalk"))
				{
					animatedSprite.Play("W_FrontWalk");
				}
				else
				{
					GD.PrintErr("Animation 'W_FrontWalk' introuvable !");
				}
				break;
			case "Orc":
				if (animatedSprite.SpriteFrames.HasAnimation("O_FrontWalk"))
				{
					animatedSprite.Play("O_FrontWalk");
				}
				else
				{
					GD.PrintErr("Animation 'O_FrontWalk' introuvable !");
				}
				break;
			case "Slime":
				if (animatedSprite.SpriteFrames.HasAnimation("S_FrontWalk"))
				{
					animatedSprite.Play("S_FrontWalk");
				}
				else
				{
					GD.PrintErr("Animation 'S_FrontWalk' introuvable !");
				}
				break;
			case "Bee":
				if (animatedSprite.SpriteFrames.HasAnimation("B_FrontWalk"))
				{
					animatedSprite.Play("B_FrontWalk");
				}
				else
				{
					GD.PrintErr("Animation 'B_FrontWalk' introuvable !");
				}
				break;
			default:
				GD.PrintErr("Type d'ennemi inconnu : " + type);
				break;
		}
	}
	
	public void TakeDamage(int damage)
	{
		life -= damage;
		if (life <= 0) {
			gold.EarnCoins(reward);
			EmitSignal(nameof(EnemyDied), this);
			QueueFree();
		}
	}
	
	public override void _Process(double delta)
	{
		if (pathFollow == null)
			return;
			
		pathFollow.Progress += (float)(pathSpeed * delta);

		if (pathFollow.ProgressRatio >= 1.0f)
		{
			GD.Print("Ennemi arrivé au bout du chemin !");
			var baseMgr = BaseManager.Instance ?? (GetTree().Root.FindChild("BaseManager", true, false) as BaseManager);
			if (baseMgr != null)
			{
				baseMgr.TakeDamage(attack);
			}
			EmitSignal(nameof(EnemyDied), this);
			QueueFree();
		}
	}
	
	public void Die()
	{
		EmitSignal(nameof(EnemyDied),this);
		QueueFree();
	}
}
