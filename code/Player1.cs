using Godot;
using System;

public partial class Player1 : CharacterBody2D
{
	[Export]
	public int HP = 10;
	[Export]
	public PackedScene SmallBulletScene;
	[Export]
	public PackedScene MediumBulletScene;
	[Export]
	public PackedScene LargeBulletScene;

	private AnimatedSprite2D sprite;
	private bool autoMove = false; // 自动移动开关
	private float moveSpeed = 100f; // 自动移动速度

	public override void _Ready()
	{
		sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		sprite.AnimationFinished += OnAnimationFinished;
	}

	private void OnAnimationFinished()
	{
		if (sprite.Animation == "hit" || sprite.Animation == "attack")
		{
			sprite.Play("idle");
		}
	}

	public override void _Process(double delta)
	{
		if (!autoMove)
		{
			int facingDir = sprite.FlipH ? -1 : 1;

			if (Input.IsActionJustPressed("fire_small_wave"))
				FireWave(SmallBulletScene, facingDir);
			else if (Input.IsActionJustPressed("fire_medium_wave"))
				FireWave(MediumBulletScene, facingDir);
			else if (Input.IsActionJustPressed("fire_large_wave"))
				FireWave(LargeBulletScene, facingDir);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (autoMove)
		{
			Velocity = new Vector2(moveSpeed, 0);
			MoveAndSlide();
			if (sprite.Animation != "run")
			{
				sprite.Play("run");
			}
		}
	}

	private void FireWave(PackedScene bulletScene, int direction)
	{
		if (bulletScene == null)
		{
			GD.Print("波场景未设置");
			return;
		}

		Node bulletInstance = bulletScene.Instantiate();
		if (bulletInstance is Node2D bullet)
		{
			bullet.GlobalPosition = new Vector2(300, 500);

			if (bullet.HasMeta("direction"))
				bullet.Set("direction", direction);
			else if (bullet is Bullet bulletScript)
				bulletScript.Direction = direction;

			GetTree().CurrentScene.AddChild(bullet);
		}

		sprite.Play("attack");
	}

	public void TakeDamage(int damage)
	{
		sprite.Play("hit");
		HP -= damage;
		GD.Print($"HP: {HP}");

		if (HP <= 0)
		{
			Die();
		}
	}

	private void Die()
	{
		GD.Print("Player died!");
		sprite.Play("death");
	}

	// 在战斗胜利后调用此函数
	public void StartAutoMove()
	{
		autoMove = true;
	}
}
