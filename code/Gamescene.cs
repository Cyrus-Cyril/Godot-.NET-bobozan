using Godot;
using System;

public partial class Gamescene : Node2D
{
	public override void _Ready()
	{
		// 添加输入绑定
		AddKeyBinding("fire_small_wave", Key.Q);
		AddKeyBinding("fire_medium_wave", Key.W);
		AddKeyBinding("fire_large_wave", Key.E);
		AddKeyBinding("charge", Key.S);
		AddKeyBinding("defend", Key.D);
		AddKeyBinding("rebound", Key.F);
		AddKeyBinding("confirm_action_p1",Key.Enter);

		AddKeyBinding("fire_small_wave_p2", Key.Kp7);
		AddKeyBinding("fire_medium_wave_p2", Key.Kp8);
		AddKeyBinding("fire_large_wave_p2", Key.Kp9);
		AddKeyBinding("charge_p2", Key.Kp4);
		AddKeyBinding("defend_p2", Key.Kp5);
		AddKeyBinding("rebound_p2", Key.Kp6);
		AddKeyBinding("confirm_action_p2",Key.KpEnter);
	}

	private void AddKeyBinding(string action, Key key)
	{
		// 检查输入映射中是否已存在该操作
		if (!InputMap.HasAction(action))
		{
			// 如果没有，动态添加该操作
			InputMap.AddAction(action);
		}

		// 创建一个新的按键事件并绑定
		var ev = new InputEventKey
		{
			Keycode = key
		};

		// 将事件添加到操作中
		InputMap.ActionAddEvent(action, ev);
	}

	public override void _Process(double delta)
	{
		// 在这里添加每帧逻辑
	}
}
