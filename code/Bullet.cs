using Godot;
using System;

public partial class Bullet : Area2D
{
	[Export]
	public float Speed = 400.0f;
	[Export]
	public int Direction = 1;
	public virtual int Damage { get; set; } = 1;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}
	public override void _PhysicsProcess(double delta)
	{
		GlobalPosition += new Vector2((float)(Speed * delta * Direction), 0);
	}
	
	private void OnBodyEntered(Node body)
	{
		if (body is Player2 player)
		{
			player.TakeDamage(Damage);
			QueueFree(); // 销毁子弹
		}
	}

}
