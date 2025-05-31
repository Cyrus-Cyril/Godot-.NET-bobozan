using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


public partial class AiBattlemanager : Node
{
	private Player1Ai player1Ai;
	private AiPlayer aiPlayer;

	private PlayerAction actionP1;
	private PlayerAction actionAI;

	private bool p1Locked = false;
	private bool aiLocked = false;

	private Timer cleanupTimer = null;
	
	private Button restartButton;
	
	public override void _Ready()
	{
		player1Ai = GetNode<Player1Ai>("../Player1_ai");
		aiPlayer = GetNode<AiPlayer>("../ai_player");
		restartButton = GetNode<Button>("../CanvasLayer/RestartButton");
		restartButton.Pressed += OnRestartButtonPressed;
		ResetRound();
	}

	private void OnRestartButtonPressed()
	{
		// 重置 HP/MP
		player1Ai.HP = player1Ai.MaxHP;
		player1Ai.MP = 0;
		player1Ai.UpdateUI();
		player1Ai.SavePlayerData();

		aiPlayer.HP = aiPlayer.MaxHP;
		aiPlayer.MP = 0;
		aiPlayer.UpdateUI();
		aiPlayer.SavePlayerData();
		
		player1Ai.sprite.Play("idle");
		aiPlayer.sprite.Play("idle");
		ResetRound();
		
		GD.Print("已重置双方HP/MP，并保存到数据库");
	}
	
	public override void _Process(double delta)
	{
		if (!p1Locked)
		{
			actionP1 = player1Ai.GetAction();
			if (actionP1 != null) p1Locked = true;
		}

		if (!aiLocked)
		{
			actionAI = aiPlayer.GetAction();
			if (actionAI != null) aiLocked = true;
		}

		if (p1Locked && aiLocked)
		{
			ResolveTurn(actionP1, actionAI);
			ResetRound();
		}
	}

	private async Task ResetAfterDelay()
	{
		await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
		ResetRound();
	}

	private void ResetRound()
	{
		p1Locked = false;
		aiLocked = false;
		actionP1 = null;
		actionAI = null;

		player1Ai.ResetAction();
		aiPlayer.ResetAction();

	}

	private void OnDelayedCleanup()
	{
		foreach (Node node in GetChildren())
		{
			if (node is Defend || node is Rebound)
				node.QueueFree();
		}
	}

	private async void FireWaveSequence(Node player, List<int> waves, int direction, string target, Vector2 position)
	{
		foreach (int w in waves)
		{
			PackedScene scene = GetWaveScene(player, w);
			if (scene != null)
			{
				if (player is Player1Ai p1 && p1.MP >= w)
				{
					p1.MP -= w;
					p1.UpdateUI();
				}
				else if (player is AiPlayer ai && ai.MP >= w)
				{
					ai.MP -= w;
					ai.UpdateUI();
				}
				else continue;

				Bullet bullet = scene.Instantiate<Bullet>();
				bullet.Direction = direction;
				// bullet.Target = target;
				bullet.GlobalPosition = position;
				AddChild(bullet);

				await ToSignal(GetTree().CreateTimer(0.3), "timeout");
			}
		}
	}

	private void ResolveTurn(PlayerAction a1, PlayerAction a2)
	{
		GD.Print("--- 发招阶段 (AI 模式) ---");

		if (a1.Type == "charge") { player1Ai.MP = Math.Min(player1Ai.MaxMP, player1Ai.MP + 1); player1Ai.UpdateUI(); }
		if (a2.Type == "charge") { aiPlayer.MP = Math.Min(aiPlayer.MaxMP, aiPlayer.MP + 1); aiPlayer.UpdateUI(); }

		if (a1.Type == "rebound")
		{
			var inst = player1Ai.ReboundScene.Instantiate<Rebound>();
			inst.Direction = 1;
			inst.UpdateFacing();
			inst.GlobalPosition = new Vector2(370, 500);
			AddChild(inst);
			player1Ai.MP -= 1;
			player1Ai.UpdateUI();
		}

		if (a2.Type == "rebound")
		{
			var inst = aiPlayer.ReboundScene.Instantiate<Rebound>();
			inst.Direction = -1;
			inst.UpdateFacing();
			inst.GlobalPosition = new Vector2(910, 500);
			AddChild(inst);
			aiPlayer.MP -= 1;
			aiPlayer.UpdateUI();
		}

		if (a1.Type == "defend")
		{
			var inst = player1Ai.DefendScene.Instantiate<Defend>();
			inst.Direction = 1;
			inst.UpdateFacing();
			inst.GlobalPosition = new Vector2(370, 500);
			AddChild(inst);
		}

		if (a2.Type == "defend")
		{
			var inst = aiPlayer.DefendScene.Instantiate<Defend>();
			inst.Direction = -1;
			inst.UpdateFacing();
			inst.GlobalPosition = new Vector2(910, 500);
			AddChild(inst);
		}

		if (a1.Type == "wave")
		{
			FireWaveSequence(player1Ai, a1.Waves, 1, "ai_player", new Vector2(380, 500));
			player1Ai.sprite.Play("attack");
		}

		if (a2.Type == "wave")
		{
			FireWaveSequence(aiPlayer, a2.Waves, -1, "player1Ai", new Vector2(880, 500));
			aiPlayer.sprite.Play("attack");
		}

		if (cleanupTimer != null && IsInstanceValid(cleanupTimer))
			cleanupTimer.QueueFree();

		cleanupTimer = new Timer
		{
			WaitTime = 1,
			OneShot = true
		};
		AddChild(cleanupTimer);
		cleanupTimer.Timeout += OnDelayedCleanup;
		cleanupTimer.Start();
		
		if (DatabaseManager.Instance != null)
		{
			DatabaseManager.Instance.SaveBattleState(
				"Player1Ai", player1Ai.HP, player1Ai.MP, player1Ai.MaxHP, player1Ai.MaxMP,
				"AiPlayer", aiPlayer.HP, aiPlayer.MP, aiPlayer.MaxHP, aiPlayer.MaxMP
			);
		}
	
		_ = ResetAfterDelay();
	}

	private PackedScene GetWaveScene(Node player, int level)
	{
		if (player is Player1Ai p1)
		{
			return level switch
			{
				1 => p1.SmallBulletScene,
				2 => p1.MediumBulletScene,
				3 => p1.LargeBulletScene,
				_ => null
			};
		}
		else if (player is AiPlayer ai)
		{
			return level switch
			{
				1 => ai.SmallBulletScene,
				2 => ai.MediumBulletScene,
				3 => ai.LargeBulletScene,
				_ => null
			};
		}
		return null;
	}
}
