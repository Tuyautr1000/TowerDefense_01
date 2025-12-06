using Godot;
using System;

public partial class PathFollow2DController : PathFollow2D
{
	[Export]
	public float Speed = 100.0f;
	private Node2D enemyNode;
	private AnimatedSprite2D _animatedSprite;
	private float _pathLength = 3260.0f;
	
	public override void _Ready()
	{
		enemyNode = GetNodeOrNull<Node2D>("enemy");
		if (enemyNode == null)
		{
			GD.PrintErr("AnimatedSprite2D n'a pas été trouvé !");
			return;
		}
		
		_animatedSprite = enemyNode.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		if (_animatedSprite == null)
		{
			
		}
	}
}
