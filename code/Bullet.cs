using Godot;
using System;

public partial class Bullet : Area2D
{
	[Export]
	public float Speed = 600.0f;

	// 方向：1 向右，-1 向左
	[Export]
	public int Direction = 1;

	public virtual int Damage { get; set; } = 1;
	public virtual int MPCost => 0;
	// 标记当前的目标是谁,默认Player2
	[Export]
	public string Target = "Player2";

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
		return 0; // 默认不识别
	}
	
	private void OnAreaEntered(Area2D area)
	{
		GD.Print("Area Entered: " + area.Name);
		if (area is Rebound rebound)
		{
			if (Target == "Player1")
				Target = "Player2";
			else if (Target == "Player2")
				Target = "Player1";	

			// 反转子弹方向
			Direction *= -1;

			// 反转贴图方向
			var sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
			if (sprite != null)
				sprite.FlipH = Direction < 0;

			var animSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
			if (animSprite != null)
				animSprite.FlipH = Direction < 0;

			GD.Print("Bullet rebound!");
			return;
		}
		// 防御逻辑（Bullet vs Defend）
		if (area is Defend)
		{
			int level = GetBulletLevel();
			if (level == 1)
			{
				QueueFree();
			}
			else if(level==2)
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
		if (body.Name == Target)
		{
			if (body is Player1 p1)
				p1.TakeDamage(Damage);
			else if (body is Player2 p2)
				p2.TakeDamage(Damage);
	
			QueueFree();
		}
	}
}	
