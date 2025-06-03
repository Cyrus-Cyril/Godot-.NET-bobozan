using Godot;
using System;

public partial class Global : Node
{
	public static bool fullScreen { get; private set; } = false;
	
	public static void ChangeFull() {
		fullScreen = !fullScreen;
		if (fullScreen) {
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
		} else {
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
		}
	}
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
