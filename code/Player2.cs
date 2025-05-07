using Godot;
using System;

public partial class Player2 : CharacterBody2D
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
	private Player1 player1; // 引用 Player1

	public override void _Ready()
	{
		sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		sprite.AnimationFinished += OnAnimationFinished;

		// Player1 与 Player2 在同一父节点下，并命名为 "Player1"
		player1 = GetParent().GetNode<Player1>("Player1");
	}

	private void OnAnimationFinished()
	{
		if (sprite.Animation == "hit")
		{
			sprite.Play("idle");
		}
		if (sprite.Animation == "attack")
		{
			sprite.Play("idle");
		}
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
		GD.Print("Enemy died!");
		sprite.Play("death");
		
	CollisionShape2D collision = GetNode<CollisionShape2D>("CollisionShape2D");
	if (collision != null)
	{
		collision.SetDeferred("disabled", true);
	}
		// 通知 Player1 开始自动移动
		player1?.StartAutoMove();
	}
}
