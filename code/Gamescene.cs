using Godot;
using System;

public partial class Gamescene : Node2D
{
	public override void _Ready()
	{
		AddKeyBinding("fire_small_wave", Key.Q);
		AddKeyBinding("fire_medium_wave", Key.W);
		AddKeyBinding("fire_large_wave", Key.E);
	}

	private static void AddKeyBinding(string action, Key key)
	{
		var ev = new InputEventKey
		{
			Keycode = key
		};

		if (!InputMap.HasAction(action))
			InputMap.AddAction(action);

		InputMap.ActionAddEvent(action, ev);
	}

	public override void _Process(double delta) {}
}
