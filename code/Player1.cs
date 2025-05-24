using Godot;
using System;
using System.Collections.Generic;

public partial class Player1 : CharacterBody2D
{
	[Export] public int HP = 10;
	[Export] public PackedScene SmallBulletScene;
	[Export] public PackedScene MediumBulletScene;
	[Export] public PackedScene LargeBulletScene;
	[Export] public PackedScene DefendScene;
	[Export] public PackedScene ReboundScene;
	[Export] public int MaxMP { get; set; } = 100;
	[Export] public int MaxHP { get; set; } = 10;
	[Export] public StatusPanel StatusUI;
		
	public int MP { get; set; } = 0;


	public AnimatedSprite2D sprite;
	private string pendingAction = null;
	private List<int> waveBuffer = new();
	private bool actionChosen = false;

	
	public override void _Ready()
	{
		sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		StatusUI.UpdateHP(HP, MaxHP);
		
		sprite.AnimationFinished += OnAnimationFinished;
	}

	private void OnAnimationFinished()
	{
		if (sprite.Animation == "hit" || sprite.Animation == "attack")
			sprite.Play("idle");
	}

	public override void _Process(double delta)
	{
		if (actionChosen) return;

		if (Input.IsActionJustPressed("charge")) pendingAction = "charge";
		else if (Input.IsActionJustPressed("defend")) pendingAction = "defend";
		else if (Input.IsActionJustPressed("rebound") && MP >= 1) pendingAction = "rebound";
		else if (Input.IsActionJustPressed("fire_small_wave")) waveBuffer.Add(1);
		else if (Input.IsActionJustPressed("fire_medium_wave")) waveBuffer.Add(2);
		else if (Input.IsActionJustPressed("fire_large_wave")) waveBuffer.Add(3);

		// 玩家按下确认键
		if (Input.IsActionJustPressed("confirm_action_p1"))
		{
			if (pendingAction == "wave" || waveBuffer.Count > 0)
			{
				int total = 0;
				foreach (int w in waveBuffer)
					total += w;
				if (total <= MP)
				{
					pendingAction = "wave";
					actionChosen = true;
				}
				else
				{
					GD.Print("MP不足，清除波组合");
					waveBuffer.Clear();
				}
			}
			else if (!string.IsNullOrEmpty(pendingAction))
			{
				actionChosen = true;
			}
		}
	}

	public PlayerAction GetAction()
	{
		if (!actionChosen) return null;
		return pendingAction == "wave" ? new PlayerAction("wave", new List<int>(waveBuffer)) : new PlayerAction(pendingAction);
	}

	public void ResetAction()
	{
		pendingAction = null;
		actionChosen = false;
		waveBuffer.Clear();
	}

	public void TakeDamage(int damage)
	{
		sprite.Play("hit");
		HP -= damage;
		GD.Print($"HP: {HP}");
		StatusUI.UpdateHP(HP, MaxHP);
		
		if (HP <= 0) Die();
	}

	private void Die()
	{
		GD.Print("Player1 died!");
		sprite.Play("death");
	}
}
