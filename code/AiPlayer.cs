using Godot;
using System;
using System.Collections.Generic;

public partial class AiPlayer : CharacterBody2D
{
	// 状态机枚举
	public enum AiState { Idle, ChoosingAction, ExecutingAction, Dead }
	private AiState currentState = AiState.Idle;

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
	private Random random = new();

	// 静态引用对手（由 ai_battlemanager 注入）
	public static Player1Ai EnemyPlayer { get; set; }

	public override void _Ready()
	{
		sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		StatusUI = GetNode<StatusPanel>(StatusUIPath);

		LoadPlayerData();

		StatusUI.UpdateHP(HP, MaxHP);
		StatusUI.UpdateMP(MP, MaxMP);

		sprite.AnimationFinished += OnAnimationFinished;
	}

	public override void _Process(double delta)
	{
		// 防御性检查
		if (EnemyPlayer == null)
		{
			GD.PrintErr("AI无法获取敌人引用！");
			return;
		}

		switch (currentState)
		{
			case AiState.Idle:
				currentState = AiState.ChoosingAction;
				break;

			case AiState.ChoosingAction:
				ChooseAction();
				currentState = AiState.ExecutingAction;
				break;

			case AiState.ExecutingAction:
				// 等外部系统执行完 action 后调用 ResetState()
				break;

			case AiState.Dead:
				// 死亡后不做任何事
				break;
		}
	}

	private void ChooseAction()
	{
		int enemyMP = EnemyPlayer?.MP ?? 0;
		bool enemyDangerous = enemyMP > 0;

		var weights = new Dictionary<string, float>();

		if (MP >= 1)
		{
			weights["wave"] = 2.0f;
			if (enemyDangerous) weights["rebound"] = 1.0f;
		}
		if (MP < MaxMP)
		{
			weights["charge"] = 1.5f;
		}
		if (enemyDangerous)
		{
			weights["defend"] = 0.8f;
		}

		if (weights.Count == 0)
		{
			weights["charge"] = 1.0f;
		}

		pendingAction = ChooseWeightedAction(weights);

		if (pendingAction == "wave")
		{
			GenerateWavePattern();
		}
	}

	private string ChooseWeightedAction(Dictionary<string, float> options)
	{
		float total = 0;
		foreach (var w in options.Values) total += w;
		float roll = (float)random.NextDouble() * total;

		foreach (var kv in options)
		{
			if (roll < kv.Value)
				return kv.Key;
			roll -= kv.Value;
		}
		return "defend";
	}

	private void GenerateWavePattern()
	{
		waveBuffer.Clear();
		int mp = MP;
		while (mp >= 1)
		{
			int w = random.Next(1, Math.Min(mp, 3) + 1);
			waveBuffer.Add(w);
			mp -= w;
			if (random.NextDouble() < 0.5) break;
		}
	}

	public PlayerAction GetAction()
	{
		if (currentState != AiState.ExecutingAction) return null;

		if (pendingAction == "wave")
			return new PlayerAction("wave", new List<int>(waveBuffer));
		else
			return new PlayerAction(pendingAction);
	}

	public void ResetState()
	{
		pendingAction = null;
		waveBuffer.Clear();
		currentState = HP > 0 ? AiState.Idle : AiState.Dead;
	}

	private void OnAnimationFinished()
	{
		if (sprite.Animation == "hit" || sprite.Animation == "attack")
			sprite.Play("idle");
	}

	public void TakeDamage(int damage)
	{
		if (currentState == AiState.Dead) return;

		sprite.Play("hit");
		HP -= damage;
		StatusUI.UpdateHP(HP, MaxHP);
		SavePlayerData();

		if (HP <= 0)
			Die();
	}

	private void Die()
	{
		GD.Print("AI died!");
		sprite.Play("death");
		SavePlayerData();
		currentState = AiState.Dead;
	}

	private void LoadPlayerData()
	{
		if (DatabaseManager.Instance != null)
		{
			var stats = DatabaseManager.Instance.LoadPlayerStats("AiPlayer");
			HP = stats.HP;
			MP = stats.MP;
			MaxHP = stats.MaxHP;
			MaxMP = stats.MaxMP;
			GD.Print($"已加载 AiPlayer 数据: HP={HP}, MP={MP}");
		}
	}

	public void SavePlayerData()
	{
		if (DatabaseManager.Instance != null)
			DatabaseManager.Instance.SavePlayerStats("AiPlayer", HP, MP, MaxHP, MaxMP);
	}

	public void UpdateUI()
	{
		StatusUI.UpdateHP(HP, MaxHP);
		StatusUI.UpdateMP(MP, MaxMP);
		SavePlayerData();
	}
}
