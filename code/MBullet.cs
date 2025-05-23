using Godot;
using System;

public partial class MBullet : Bullet
{
	public override int Damage { get; set; } = 2;
	public override int MPCost => 2;
}
