using Godot;
using System;

public partial class Bullet : Area2D
{
	[Export]
	public float Speed = 900.0f;

	// 方向：1 向右，-1 向左
	[Export]
	public int Direction = 1;

	public virtual int Damage { get; set; } = 1;
	public virtual int MPCost => 0;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
		AreaEntered += OnAreaEntered;
		
		var sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
		if (sprite != null)
			sprite.FlipH = Direction < 0;

		var animSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		if (animSprite != null)
			animSprite.FlipH = Direction < 0;
	}

	public override void _PhysicsProcess(double delta)
	{
		GlobalPosition += new Vector2((float)(Speed * delta * Direction), 0);
	}

	private int GetBulletLevel()
	{
		if (this is SBullet) return 1;
		if (this is MBullet) return 2;
		if (this is LBullet) return 3;
		return 0;
	}

	private void OnAreaEntered(Area2D area)
	{
		GD.Print("Area Entered: " + area.Name);

		if (area is Rebound rebound)
		{
			Direction *= -1;

			var sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
			if (sprite != null)
				sprite.FlipH = Direction < 0;

			var animSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
			if (animSprite != null)
				animSprite.FlipH = Direction < 0;

			rebound.QueueFree();
			GD.Print("Bullet rebound!");
			return;
		}

		if (area is Defend)
		{
			int level = GetBulletLevel();
			if (level == 1)
			{
				QueueFree();
			}
			else if (level == 2)
			{
				QueueFree();
				area.QueueFree();
			}
			else if (level == 3)
			{
				area.QueueFree(); // 子弹继续飞
			}
			return;
		}

		if (area is Bullet otherBullet)
		{
			int myLevel = GetBulletLevel();
			int otherLevel = otherBullet.GetBulletLevel();

			if (myLevel == otherLevel)
			{
				QueueFree();
				otherBullet.QueueFree();
			}
			else if (myLevel > otherLevel)
			{
				otherBullet.QueueFree();
			}
			else
			{
				QueueFree();
			}
		}
	}

	private void OnBodyEntered(Node body)
	{
		// 如果是向右发射，目标是 Player2 或 AiPlayer
		if (Direction > 0)
		{
			if (body is Player2 p2)
			{
				p2.TakeDamage(Damage);
				QueueFree();
			}
			else if (body is AiPlayer ai)
			{
				ai.TakeDamage(Damage);
				QueueFree();
			}
		}
		// 如果是向左发射，目标是 Player1
		else if (Direction < 0)
		{
			if (body is Player1 p1)
			{
		   		p1.TakeDamage(Damage);
		   		QueueFree();
			}
			else if (body is Player1Ai p1ai)
			{
				p1ai.TakeDamage(Damage);
				QueueFree();
			}
		}
	}
}
