using Godot;
using System;

public partial class Enemy : CharacterBody3D
{
	[Export] public float Speed = 5.0f;
	[Export] public int MaxHealth = 30;
	[Export] public int Damage = 10;

	private int _currentHealth;
	private Node3D _player;

	public override void _Ready()
	{
		_currentHealth = MaxHealth;
		_player = (Node3D)GetTree().GetFirstNodeInGroup("Player");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_player == null) return;

		Vector3 direction = GlobalPosition.DirectionTo(_player.GlobalPosition);
		direction.Y = 0;

		Velocity = direction.Normalized() * Speed;

		if (!IsOnFloor())
		{
			Vector3 vel = Velocity;
			vel.Y -= 9.8f * (float)delta;
			Velocity = vel;
		}

		MoveAndSlide();

		for (int i = 0; i < GetSlideCollisionCount(); i++)
		{
			KinematicCollision3D collision = GetSlideCollision(i);
			if (collision.GetCollider() is Player player)
			{
				player.TakeDamage(Damage); 
			}
		}
	}

	public void TakeDamage(int amount)
	{
		_currentHealth -= amount;
		GD.Print($"¡Enemigo recibió daño! Vida restante: {_currentHealth}");

		if (_currentHealth <= 0)
		{
			QueueFree(); 
		}
	}
}
