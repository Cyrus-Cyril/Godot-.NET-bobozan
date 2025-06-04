using Godot;
using System;

public partial class StatusPanel : VBoxContainer
{
	[Export] public Texture2D FullHeart;
	[Export] public Texture2D EmptyHeart;

	[Export] public Texture2D FullFlash;
	[Export] public Texture2D EmptyFlash;

	private HBoxContainer hpBar;
	private HBoxContainer mpBar;

	public override void _Ready()
	{
		hpBar = GetNode<HBoxContainer>("HPBar");
		mpBar = GetNode<HBoxContainer>("MPBar");
	}


	public void UpdateHP(int hp, int maxHp)
	{
		if (hpBar == null) return;

		foreach (Node child in hpBar.GetChildren())
			child.QueueFree();

		for (int i = 0; i < maxHp; i++)
		{
			var heart = new TextureRect
			{
				Texture = i < hp ? FullHeart : EmptyHeart,
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
				CustomMinimumSize = new Vector2(32, 32)
			};
			hpBar.AddChild(heart);
		}
	}

	public void UpdateMP(int mp, int maxMp)
	{
		if (mpBar == null) return;

		foreach (Node child in mpBar.GetChildren())
			child.QueueFree();

		for (int i = 0; i < maxMp; i++)
		{
			var flash = new TextureRect
			{
				Texture = i < mp ? FullFlash : EmptyFlash,
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
				CustomMinimumSize = new Vector2(20, 20)
			};
			mpBar.AddChild(flash);
		}
	}
}
