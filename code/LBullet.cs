using Godot;
using System;

public partial class LBullet : Bullet
{
	public override int Damage { get; set; } = 3;
	public override int MPCost => 3;
}
