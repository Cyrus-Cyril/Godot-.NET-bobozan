using Godot;
using System;

public partial class CharacterSelectScene : Control
{
	private void _on_confirm_button_pressed()
	{
		// 跳转到游戏场景
		GetTree().ChangeSceneToFile("res://Scenes/GameScene/ai_gamescene.tscn");
	}
	private void _on_back_button_pressed()
	{
		// 跳转到主菜单场景
		GetTree().ChangeSceneToFile("res://Scenes/MainScene/main_scene.tscn");
	}
	
	private void OnCharacterSlotPressed(CharacterSlot slot)
	{
		var preview = GetNode<AnimatedSprite2D>("MenuFrame/PageRight/Panel/CharacterPreview");
		preview.SpriteFrames = slot.Animation;
		preview.Play("default");

		var info = GetNode<RichTextLabel>("MenuFrame/PageRight/Panel/CharacterInfo");
		info.Text = slot.Description;
		
		// ✅ 保证面板可见和接收点击
		var panel = GetNode<Panel>("MenuFrame/PageRight/Panel");
		panel.Modulate = new Color(1, 1, 1, 1);
		panel.MouseFilter = Control.MouseFilterEnum.Stop;
	}


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
		
		foreach (Node node in GetNode("MenuFrame/PageLeft/GridContainer").GetChildren())
		{
			if (node is CharacterSlot slot)
			{
				// 动态连接信号（Pressed 信号 → OnCharacterSlotPressed 函数）
				slot.Connect(CharacterSlot.SignalName.Pressed, new Callable(this, nameof(OnCharacterSlotPressed)));
			}
		}
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
