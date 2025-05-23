using Godot;
using System;

public partial class Defend : Area2D
{
	[Export]
	public int Direction = 1; // 1: 右，-1: 左

	public override void _Ready()
	{
		UpdateFacing();
	}
	// 手动调用以更新贴图朝向（设置 Direction 后调用）
	public void UpdateFacing()
	{
		if (HasNode("Sprite2D"))
		{
			var sprite = GetNode<Sprite2D>("Sprite2D");
			sprite.FlipH = Direction < 0;
		}
	}
}
