using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class NBattleManager : Node
{
	private Player1 player1;
	private Player2 player2;

	private PlayerAction actionP1;
	private PlayerAction actionP2;

	private bool p1Locked = false;
	private bool p2Locked = false;
	private bool gameEnded = false;
	private bool isPaused = false; // 新增暂停标志

	private Timer cleanupTimer = null;
	
	private Button restartButton;
	
	private Control gameEndPanel;
	private Label gameEndLabel;
	private Button backToMainButton;

	// 新增暂停菜单相关
	private Control pauseMenuPanel;
	private ColorRect pauseMask;
	private Button resumeButton;
	private Button backToMainButtonPause;

	private string mainMenuScenePath = "res://scenes/MainScene/main_scene.tscn"; 

	public override void _Ready()
	{
		player1 = GetNode<Player1>("../Player1");
		player2 = GetNode<Player2>("../Player2");
		restartButton = GetNode<Button>("../CanvasLayer/RestartButton");
		restartButton.Pressed += OnRestartButtonPressed;

		GetGameEndUIElements();
		GetPauseMenuUIElements(); // 新增

		ResetRound();
	}

	private void GetGameEndUIElements()
	{
		gameEndPanel = GetNode<Control>("../CanvasLayer/GameEndPanel");
		gameEndLabel = GetNode<Label>("../CanvasLayer/GameEndPanel/GameEndLabel");
		backToMainButton = GetNode<Button>("../CanvasLayer/GameEndPanel/BackToMainButton");
		backToMainButton.Pressed += OnBackToMainButtonPressed;
		gameEndPanel.Visible = false;
	}

	private void GetPauseMenuUIElements()
	{
		pauseMenuPanel = GetNode<Control>("../CanvasLayer/PauseMenuPanel");
		pauseMask = GetNode<ColorRect>("../CanvasLayer/PauseMenuPanel/PauseMask");
		resumeButton = GetNode<Button>("../CanvasLayer/PauseMenuPanel/ResumeButton");
		backToMainButtonPause = GetNode<Button>("../CanvasLayer/PauseMenuPanel/BackToMainButton");

		resumeButton.Pressed += OnResumeButtonPressed;
		backToMainButtonPause.Pressed += OnBackToMainButtonPressed;

		pauseMenuPanel.Visible = false;
		pauseMenuPanel.ProcessMode = ProcessModeEnum.Always;
	}

	// ========== 暂停相关 ==========
	public override void _UnhandledInput(InputEvent @event)
	{
		if (!gameEnded && !isPaused && @event.IsActionPressed("ui_cancel"))
		{
			PauseGame();
		}
	}

	private void PauseGame()
	{
		isPaused = true;
		pauseMenuPanel.Visible = true;
		GetTree().Paused = true;
	}

	private void OnResumeButtonPressed()
	{
		isPaused = false;
		pauseMenuPanel.Visible = false;
		GetTree().Paused = false;
	}

	private void OnBackToMainButtonPressed()
	{
		ResetCharacterStates();
		GD.Print("返回主界面前已重置双方HP/MP，并保存到数据库");
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile(mainMenuScenePath);
	}

	// ========== 游戏流程相关 ==========
	private void OnRestartButtonPressed()
	{
		gameEnded = false;
		gameEndPanel.Visible = false;
		
		player1.HP = player1.MaxHP;
		player1.MP = 0;
		player1.UpdateUI();

		player2.HP = player2.MaxHP;
		player2.MP = 0;
		player2.UpdateUI();
		
		player1.sprite.Play("idle");
		player2.sprite.Play("idle");
		ResetRound();
		
		GD.Print("已重置双方HP/MP，并保存到数据库");
	}

	private void ResetCharacterStates()
	{
		player1.HP = player1.MaxHP;
		player1.MP = 0;
		player1.UpdateUI();

		player2.HP = player2.MaxHP;
		player2.MP = 0;
		player2.UpdateUI();
		
		player1.sprite.Play("idle");
		player2.sprite.Play("idle");
		
		if (DatabaseManager.Instance != null)
		{
			DatabaseManager.Instance.SaveBattleState(
				"Player1", player1.HP, player1.MP, player1.MaxHP, player1.MaxMP,
				"Player2", player2.HP, player2.MP, player2.MaxHP, player2.MaxMP
			);
		}
	}

	public override void _Process(double delta)
	{
		if (gameEnded || isPaused) return;
		
		CheckGameEnd();
		
		if (!p1Locked)
		{
			actionP1 = player1.GetAction();
			if (actionP1 != null) p1Locked = true;
		}

		if (!p2Locked)
		{
			actionP2 = player2.GetAction();
			if (actionP2 != null) p2Locked = true;
		}

		if (p1Locked && p2Locked)
		{
			ResolveTurn(actionP1, actionP2);
			ResetRound();
		}
	}

	private void CheckGameEnd()
	{
		if (gameEnded) return;
		
		if (player1.HP <= 0)
		{
			ShowGameEnd("PLAYER 2 WINS!", Colors.Gold);
		}
		else if (player2.HP <= 0)
		{
			ShowGameEnd("PLAYER 1 WINS!", Colors.Gold);
		}
	}
	
	private void ShowGameEnd(string message, Color textColor)
	{
		gameEnded = true;
		gameEndLabel.Text = message;
		gameEndLabel.Modulate = textColor;
		gameEndPanel.Visible = true;
		// 结束时确保解除暂停
		isPaused = false;
		pauseMenuPanel.Visible = false;
		GetTree().Paused = false;
		GD.Print($"游戏结束: {message}");
	}

	private void ResetRound()
	{
		if (gameEnded) return;
		
		p1Locked = false;
		p2Locked = false;
		actionP1 = null;
		actionP2 = null;

		player1.ResetAction();
		player2.ResetAction();
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
				if (player is Player1 p1 && p1.MP >= w)
				{
					p1.MP -= w;
					p1.UpdateUI();
				}
				else if (player is Player2 p2 && p2.MP >= w)
				{
					p2.MP -= w;
					p2.UpdateUI();
				}
				else continue;

				Bullet bullet = scene.Instantiate<Bullet>();
				bullet.Direction = direction;
				//bullet.Target = target;
				bullet.GlobalPosition = position;
				AddChild(bullet);

				await ToSignal(GetTree().CreateTimer(0.3), "timeout");
			}
		}
	}

	private void ResolveTurn(PlayerAction a1, PlayerAction a2)
	{
		GD.Print("--- 发招阶段 ---");

		if (a1.Type == "charge") {player1.MP = Math.Min(player1.MaxMP, player1.MP + 1); player1.UpdateUI();}
		if (a2.Type == "charge") {player2.MP = Math.Min(player2.MaxMP, player2.MP + 1); player2.UpdateUI();}

		if (a1.Type == "rebound" && player1.ReboundScene != null)
		{
			var inst = player1.ReboundScene.Instantiate();
			if (inst is Rebound rebound)
			{
				rebound.Direction = 1;
				rebound.UpdateFacing();
				rebound.GlobalPosition = new Vector2(370, 500);
				AddChild(rebound);
			}
			player1.MP -= 1;
			player1.UpdateUI();
		}

		if (a2.Type == "rebound" && player2.ReboundScene != null)
		{
			var inst = player2.ReboundScene.Instantiate();
			if (inst is Rebound rebound)
			{
				rebound.Direction = -1;
				rebound.UpdateFacing();
				rebound.GlobalPosition = new Vector2(910, 500);
				AddChild(rebound);
			}
			player2.MP -= 1;
			player2.UpdateUI();
		}

		if (a1.Type == "defend" && player1.DefendScene != null)
		{
			var inst = player1.DefendScene.Instantiate();
			if (inst is Defend defend)
			{
				defend.Direction = 1;
				defend.UpdateFacing();
				defend.GlobalPosition = new Vector2(370, 500);
				AddChild(defend);
			}
		}

		if (a2.Type == "defend" && player2.DefendScene != null)
		{
			var inst = player2.DefendScene.Instantiate();
			if (inst is Defend defend)
			{
				defend.Direction = -1;
				defend.UpdateFacing();
				defend.GlobalPosition = new Vector2(910, 500);
				AddChild(defend);
			}
		}

		if (a1.Type == "wave")
		{
			FireWaveSequence(player1, a1.Waves, 1, "Player2", new Vector2(380, 500));
			player1.sprite.Play("attack");
		}

		if (a2.Type == "wave")
		{
			FireWaveSequence(player2, a2.Waves, -1, "Player1", new Vector2(880, 500));
			player2.sprite.Play("attack");
		}
		
		if (cleanupTimer != null && IsInstanceValid(cleanupTimer))
		{
			cleanupTimer.QueueFree();
		}
		cleanupTimer = new Timer
		{
			WaitTime = 3,
			OneShot = true
		};
		AddChild(cleanupTimer);
		cleanupTimer.Timeout += OnDelayedCleanup;
		cleanupTimer.Start();
		
		if (DatabaseManager.Instance != null)
		{
			DatabaseManager.Instance.SaveBattleState(
				"Player1", player1.HP, player1.MP, player1.MaxHP, player1.MaxMP,
				"Player2", player2.HP, player2.MP, player2.MaxHP, player2.MaxMP
			);
		}
	}
	
	private PackedScene GetWaveScene(object player, int level)
	{
		if (player is Player1 p1)
		{
			return level switch
			{
				1 => p1.SmallBulletScene,
				2 => p1.MediumBulletScene,
				3 => p1.LargeBulletScene,
				_ => null
			};
		}
		if (player is Player2 p2)
		{
			return level switch
			{
				1 => p2.SmallBulletScene,
				2 => p2.MediumBulletScene,
				3 => p2.LargeBulletScene,
				_ => null
			};
		}
		return null;
	}
}
