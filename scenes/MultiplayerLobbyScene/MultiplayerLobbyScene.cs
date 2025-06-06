using Godot;
using System;
using System.Net.PeerToPeer.Collaboration;



public partial class MultiplayerLobbyScene : Control
{	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _on_button_local_pressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/GameScene/gamescene.tscn");
	}

	public void _on_button_net_pressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/GameScene/Netgamescene.tscn");
	}

	private void _on_back_button_pressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/MainScene/main_scene.tscn");
	}
}
