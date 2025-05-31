using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BattleManager : Node
{
	private Player1 player1;
	private Player2 player2;

	private PlayerAction actionP1;
	private PlayerAction actionP2;

	private bool p1Locked = false;
	private bool p2Locked = false;

	private Timer cleanupTimer = null;

	public override void _Ready()
	{
		player1 = GetNode<Player1>("../Player1");
		player2 = GetNode<Player2>("../Player2");
		ResetRound();
	}

	public override void _Process(double delta)
	{
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

	private void ResetRound()
	{
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

public class PlayerAction
{
	public string Type;
	public List<int> Waves = new();
	public PlayerAction(string type) => Type = type;
	public PlayerAction(string type, List<int> waves)
	{
		Type = type;
		Waves = waves;
	}
}
