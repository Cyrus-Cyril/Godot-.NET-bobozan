using Godot;
using System;

public partial class CharacterSlot : Panel
{
	private bool isSelected = false;
	private Texture2D icon;
	private SpriteFrames animation;
	private string description;

	[Export]
	public Texture2D Icon
	{
		get => icon;
		set
		{
			icon = value;
			GetNode<TextureButton>("TextureButton").TextureNormal = value;
		}
	}

	[Export]
	public SpriteFrames Animation
	{
		get => animation;
		set => animation = value;
	}

	[Export(PropertyHint.MultilineText)]
	public string Description
	{
		get => description;
		set => description = value;
	}

	[Signal]
	public delegate void PressedEventHandler(CharacterSlot slot); // 改为传递自身引用

	public override void _Ready()
	{
		GetNode<Line2D>("Line2D").Hide();
	}

	private void _on_texture_button_pressed()
	{
		if (isSelected)
		{
			Deselect();
			return;
		}

		Node parent = GetParent();
		foreach (Node child in parent.GetChildren())
		{
			if (child is CharacterSlot other && other != this)
				other.Deselect();
		}

		isSelected = true;
		GetNode<Line2D>("Line2D").Show();
		EmitSignal(SignalName.Pressed, this); // 传递 this，方便主场景处理
	}

	public void Deselect()
	{
		isSelected = false;
		GetNode<Line2D>("Line2D").Hide();
	}

	public bool IsSelected() => isSelected;
}
