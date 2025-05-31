using Godot;
using System;

public partial class ActionPreview : CanvasLayer
{
	private Panel box;
	private Label label;

	public override void _Ready()
	{
		box = GetNode<Panel>("PreviewBox");
		label = GetNode<Label>("PreviewBox/PreviewText");

		box.Modulate = new Color(1, 1, 1, 0); // 初始透明
	}

	public void ShowPreview(string text, bool locked = false)
	{
		label.Text = text;

		if (!locked)
		{
			// 选择中：直接半透明
			box.Modulate = new Color(1, 1, 1, 0.5f);
		}
		else
		{
			// 锁定后：先设为不透明
			box.Modulate = new Color(1, 1, 1, 1.0f);

			// 渐隐动画
			var tween = GetTree().CreateTween();
			tween.TweenInterval(0.8);
			tween.TweenProperty(box, "modulate:a", 0.0, 1.0);
		}
	}

}
