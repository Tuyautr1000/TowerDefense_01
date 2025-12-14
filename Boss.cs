using Godot;
using System;

public partial class Boss : Enemy
{
	// Stats exportées spécifiques aux boss (pour configuration dans l'éditeur si besoin)
	[Export] public float ScaleMultiplier { get; set; } = 1.5f;
	[Export] public Color BossColor { get; set; } = new Color(1.0f, 0.3f, 0.3f); // Rouge par défaut
	[Export] public string BossType { get; set; } = "Boss";
	
	private AnimatedSprite2D bossSprite;
	
	public override void _Ready()
	{
		// Récupérer les composants nécessaires (sans appeler base._Ready pour éviter les animations d'Enemy)
		bossSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		gold = GetNode<Goldmanager>("../../../UI/Panel/GoldManager");
		
		// Configurer l'Area2D pour être détecté par les tours
		var area2D = bossSprite.GetNode<Area2D>("Area2D");
		if (area2D != null)
		{
			area2D.CollisionLayer = 2; // Layer 2 pour les ennemis/boss
			area2D.CollisionMask = 0;
		}
		
		// Jouer l'animation spécifique au boss
		SetupBossAnimation(BossType);
		
		// Personnalisation visuelle du boss
		if (bossSprite != null)
		{
			// Augmenter la taille du boss
			bossSprite.Scale = new Vector2(ScaleMultiplier, ScaleMultiplier);
			
			// Appliquer une teinte pour différencier visuellement
			bossSprite.Modulate = BossColor;
			
			GD.Print($"🔥 Boss '{BossType}' créé - HP: {life}, ATK: {attack}, Speed: {speed}, Scale: {ScaleMultiplier}");
		}
		else
		{
			GD.PrintErr("AnimatedSprite2D introuvable dans Boss!");
		}
	}
	
	private void SetupBossAnimation(string type)
	{
		if (bossSprite == null || bossSprite.SpriteFrames == null)
		{
			GD.PrintErr("BossSprite ou SpriteFrames non initialisé!");
			return;
		}
		
		switch (type)
		{
			case "KingSlime":
				if (bossSprite.SpriteFrames.HasAnimation("KingSlime_Walk"))
				{
					bossSprite.Play("KingSlime_Walk");
				}
				else
				{
					GD.PrintErr("Animation 'KingSlime_Walk' introuvable!");
				}
				break;
				
			case "Ogre":
				if (bossSprite.SpriteFrames.HasAnimation("Ogre_Walk"))
				{
					bossSprite.Play("Ogre_Walk");
				}
				else
				{
					GD.PrintErr("Animation 'Ogre_Walk' introuvable!");
				}
				break;
				
			case "QueenBee":
				if (bossSprite.SpriteFrames.HasAnimation("QueenBee_Walk"))
				{
					bossSprite.Play("QueenBee_Walk");
				}
				else
				{
					GD.PrintErr("Animation 'QueenBee_Walk' introuvable!");
				}
				break;
				
			default:
				GD.PrintErr($"Type de boss inconnu : {type}");
				// Fallback sur la première animation disponible
				if (bossSprite.SpriteFrames.HasAnimation("KingSlime_Walk"))
				{
					bossSprite.Play("KingSlime_Walk");
				}
				break;
		}
	}
}

