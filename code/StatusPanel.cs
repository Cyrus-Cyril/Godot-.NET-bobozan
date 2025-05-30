using Godot;

public partial class StatusPanel : HBoxContainer
{
	[Export] public Texture2D FullHeart;
	[Export] public Texture2D EmptyHeart;

	public void UpdateHP(int hp, int maxHp)
	{
		foreach (Node child in GetChildren())
			child.QueueFree();

		for (int i = 0; i < maxHp; i++)
		{
			var heart = new TextureRect
			{
				Texture = i < hp ? FullHeart : EmptyHeart,
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
				CustomMinimumSize = new Vector2(32, 32)
			};
			AddChild(heart);
		}
	}
}
