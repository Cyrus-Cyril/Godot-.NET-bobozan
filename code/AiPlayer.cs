using Godot;
using System;
using System.Collections.Generic;

public partial class AiPlayer : CharacterBody2D
{
	[Export] public int HP = 10;
	[Export] public int MaxHP = 10;
	[Export] public int MaxMP = 15;
	[Export] public PackedScene SmallBulletScene;
	[Export] public PackedScene MediumBulletScene;
	[Export] public PackedScene LargeBulletScene;
	[Export] public PackedScene DefendScene;
	[Export] public PackedScene ReboundScene;
	[Export] public NodePath StatusUIPath;

	private StatusPanel StatusUI;
	public AnimatedSprite2D sprite;
	public int MP { get; set; } = 0;

	private string pendingAction = null;
	private List<int> waveBuffer = new();
	private bool actionChosen = false;
	private Random random = new();

	public void UpdateUI()
	{
		StatusUI.UpdateHP(HP, MaxHP);
		StatusUI.UpdateMP(MP, MaxMP);
	}
	
	public override void _Ready()
	{
		sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		StatusUI = GetNode<StatusPanel>(StatusUIPath);
		StatusUI.UpdateHP(HP, MaxHP);
		StatusUI.UpdateMP(MP, MaxMP);
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

		List<string> options = new();

		if (MP >= 1) options.Add("rebound");
		if (MP >= 1) options.Add("wave");
		if (MP < MaxMP) options.Add("charge");
		options.Add("defend");

		pendingAction = options[random.Next(options.Count)];

		if (pendingAction == "wave")
		{
			waveBuffer.Clear();
			int mp = MP;
			while (mp >= 1)
			{
				int w = random.Next(1, Math.Min(mp, 3) + 1);
				waveBuffer.Add(w);
				mp -= w;
				if (random.NextDouble() < 0.5) break; // 有一半概率就停
			}
		}

		actionChosen = true;
	}

	public PlayerAction GetAction()
	{
		if (!actionChosen) return null;
		return pendingAction == "wave"
			? new PlayerAction("wave", new List<int>(waveBuffer))
			: new PlayerAction(pendingAction);
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
		StatusUI.UpdateHP(HP, MaxHP);
		if (HP <= 0) Die();
	}

	private void Die()
	{
		GD.Print("AI died!");
		sprite.Play("death");
		if (IsInstanceValid(StatusUI))
			StatusUI.QueueFree();
	}
}
