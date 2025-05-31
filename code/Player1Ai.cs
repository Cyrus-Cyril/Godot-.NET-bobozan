using Godot;
using System;
using System.Collections.Generic;

public partial class Player1Ai : CharacterBody2D
{
	[Export] public int HP = 10;
	[Export] public PackedScene SmallBulletScene;
	[Export] public PackedScene MediumBulletScene;
	[Export] public PackedScene LargeBulletScene;
	[Export] public PackedScene DefendScene;
	[Export] public PackedScene ReboundScene;
	[Export] public int MaxMP { get; set; } = 15;
	[Export] public int MaxHP { get; set; } = 10;
	[Export] public NodePath StatusUIPath;
	[Export] public NodePath ActionPreviewPath;
	private ActionPreview ActionUI;
	private StatusPanel StatusUI;
	
	public int MP { get; set; } = 0;
	public AnimatedSprite2D sprite;
	private string pendingAction = null;
	private List<int> waveBuffer = new();
	private bool actionChosen = false;

	public override void _Ready()
	{
		sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		StatusUI = GetNode<StatusPanel>(StatusUIPath);
		ActionUI = GetNode<ActionPreview>(ActionPreviewPath);
		
		// 从数据库加载玩家数据
		LoadPlayerData();
		
		StatusUI.UpdateHP(HP, MaxHP);
		StatusUI.UpdateMP(MP, MaxMP);
		
		sprite.AnimationFinished += OnAnimationFinished;
	}
	
	private void LoadPlayerData()
	{
		if (DatabaseManager.Instance != null)
		{
			var stats = DatabaseManager.Instance.LoadPlayerStats("Player1Ai");
			HP = stats.HP;
			MP = stats.MP;
			MaxHP = stats.MaxHP;
			MaxMP = stats.MaxMP;
			
			GD.Print($"已加载 Player1 数据: HP={HP}, MP={MP}");
		}
	}
	
	public void SavePlayerData()
	{
		if (DatabaseManager.Instance != null)
		{
			DatabaseManager.Instance.SavePlayerStats("Player1Ai", HP, MP, MaxHP, MaxMP);
		}
	}

	private void OnAnimationFinished()
	{
		if (sprite.Animation == "hit" || sprite.Animation == "attack")
			sprite.Play("idle");
	}

	private string GetActionPreviewText()
	{
		if (pendingAction == "wave" || waveBuffer.Count > 0)
		{
			var waveNames = new List<string>();
			foreach (int w in waveBuffer)
			{
				if (w == 1) waveNames.Add("S_B");
				else if (w == 2) waveNames.Add("M_B");
				else if (w == 3) waveNames.Add("L_B");
			}
			return "" + string.Join("+", waveNames);
		}
		else if (pendingAction == "charge") return "Charge";
		else if (pendingAction == "defend") return "Defend";
		else if (pendingAction == "rebound") return "Reflect";
	
		return "";
	}

	public override void _Process(double delta)
	{
		if (actionChosen) return;

		ActionUI.ShowPreview(GetActionPreviewText());
		
		if (Input.IsActionJustPressed("charge")) pendingAction = "charge";
		else if (Input.IsActionJustPressed("defend")) pendingAction = "defend";
		else if (Input.IsActionJustPressed("rebound")) pendingAction = "rebound";
		else if (Input.IsActionJustPressed("fire_small_wave")) waveBuffer.Add(1);
		else if (Input.IsActionJustPressed("fire_medium_wave")) waveBuffer.Add(2);
		else if (Input.IsActionJustPressed("fire_large_wave")) waveBuffer.Add(3);

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
					ActionUI.ShowPreview(GetActionPreviewText(), true);
				}
				else
				{
					GD.Print("MP不足，清除波组合");
					waveBuffer.Clear();
				}
			}
			else if (!string.IsNullOrEmpty(pendingAction))
			{
				if (pendingAction == "rebound" && MP < 1)
				{
					GD.Print("MP不足，不能反弹");
					pendingAction = null;
					ActionUI.ShowPreview("");
					return;
				}
				actionChosen = true;
				ActionUI.ShowPreview(GetActionPreviewText(), true);
			}
		}
		ActionUI.ShowPreview(GetActionPreviewText());
	}

	public void UpdateUI()
	{
		StatusUI.UpdateHP(HP, MaxHP);
		StatusUI.UpdateMP(MP, MaxMP);
		
		// 每次UI更新时保存数据
		SavePlayerData();
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
		
		// 受伤后立即保存数据
		SavePlayerData();
		
		if (HP <= 0) Die();
	}

	private void Die()
	{
		GD.Print("Player1 died!");
		sprite.Play("death");
		SavePlayerData();
	}
}
