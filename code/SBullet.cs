using Godot;
using System;

public partial class SBullet : Bullet
{
	public override int Damage { get; set; } = 1;
	public override int MPCost => 1;
}
