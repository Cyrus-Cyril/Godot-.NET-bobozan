using Godot;
using System;

public partial class MainScene : Control
{
	private void _on_button_single_pressed()
	{
		// 跳转到单人游戏场景
		GetTree().ChangeSceneToFile("res://Scenes/CharacterSelect/character_select_scene.tscn");		
	}
	private void _on_button_multiplayer_pressed()
	{
		//跳转到多人游戏场景
		GetTree().ChangeSceneToFile("res://Scenes/MultiplayerLobbyScene/multiplayer_lobby_scene.tscn");
	}
	private void _on_button_settings_pressed()
	{
		// 跳转到设置场景
		GetTree().ChangeSceneToFile("res://Scenes/SettingsScene/settings_scene.tscn");
	}
	private void _on_button_exit_pressed()
	{
		// 退出游戏
		GetTree().Quit();
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
