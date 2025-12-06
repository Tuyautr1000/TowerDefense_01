using Godot;
using System;

public partial class Projectile : Node2D
{
	private Vector2 velocity;
	private Enemy target;
	private int damage;
	
	[Export]public float Speed = 300.0f;
	
	public void SetProjectileSprite(Texture2D texture, SpriteFrames spriteFrames = null)
	{
		// Trouver le sprite directement (ne dépend pas de _Ready())
		AnimatedSprite2D sprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		if (sprite == null)
			return;
			
		// Si on a des SpriteFrames, les utiliser
		if (spriteFrames != null)
		{
			sprite.SpriteFrames = spriteFrames;
			sprite.Play(); // Jouer l'animation par défaut
		}
		// Sinon, si on a une texture, créer un SpriteFrames simple avec cette texture
		else if (texture != null)
		{
			// Créer un nouveau SpriteFrames (frais, sans animations préexistantes)
			var frames = new SpriteFrames();
			// Utiliser un nom d'animation unique pour éviter les conflits
			string animName = "projectile_texture";
			frames.AddAnimation(animName);
			frames.AddFrame(animName, texture);
			sprite.SpriteFrames = frames;
			sprite.Play(animName);
		}
	}
	
	public void Launch(Vector2 startPos, Enemy targetEnemy, int dmg)
	{
		Position = startPos;
		target = targetEnemy;
		damage = dmg;
		
		if(target != null)
		{
			Vector2 enemyVelocity = target.Velocity;
			float timeToTarget = Position.DistanceTo(target.GlobalPosition) / Speed;
			Vector2 predictedPosition = target.GlobalPosition + enemyVelocity * timeToTarget;
			
			Vector2 direction = (predictedPosition - Position).Normalized();
			velocity = direction * Speed;
		}
	}
	
	public override void _Process(double delta)
	{
		if (target == null || !IsInstanceValid(target))
		{
			QueueFree();
			return;
		}
		
		Vector2 direction = (target.GlobalPosition - Position).Normalized();
		velocity = direction * Speed;
		
		Position += velocity *(float)delta;
		
		if (Position.DistanceTo(target.GlobalPosition) < 10f)
			{
				target.TakeDamage(damage);
				QueueFree();
			}
	}
}
