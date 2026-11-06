using Godot;
using System;

public partial class Player : CharacterBody3D
{
	public const float Speed = 12.0f; 
	public const float JumpVelocity = 4.5f; 
	public const float MouseSensitivity = 0.003f;

	[Export] public int MaxHealth = 100;
	[Export] public float InvincibilityDuration = 0.8f; 
	
	private int _currentHealth;
	private bool _isInvincible = false;

	private Camera3D _camera;
	private RayCast3D _rayCast;

	public override void _Ready()
	{
		_camera = GetNode<Camera3D>("Camera3D");
		_rayCast = _camera.GetNode<RayCast3D>("RayCast3D");
		
		_currentHealth = MaxHealth;
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotion)
		{
			RotateY(-mouseMotion.Relative.X * MouseSensitivity);
			_camera.RotateX(-mouseMotion.Relative.Y * MouseSensitivity);
			
			Vector3 cameraRot = _camera.Rotation;
			cameraRot.X = Mathf.Clamp(cameraRot.X, Mathf.DegToRad(-80), Mathf.DegToRad(80));
			_camera.Rotation = cameraRot;
		}
		
		if (Input.IsActionJustPressed("ui_cancel")) 
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		if (!IsOnFloor()) 
			velocity.Y -= 9.8f * (float)delta; 

		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();

		if (Input.IsActionJustPressed("attack")) 
		{
			Shoot();
		}
	}

	private void Shoot()
	{
		if (_rayCast.IsColliding())
		{
			Node collider = (Node)_rayCast.GetCollider();
			
			if (collider is Enemy enemy)
			{
				enemy.TakeDamage(15); 
			}
		}
	}

	public async void TakeDamage(int amount)
	{
		if (_isInvincible) return;

		_currentHealth -= amount;
		GD.Print($"¡Jugador recibió daño! Vida restante: {_currentHealth}");

		if (_currentHealth <= 0)
		{
			Die();
			return; 
		}

		_isInvincible = true;
		await ToSignal(GetTree().CreateTimer(InvincibilityDuration), SceneTreeTimer.SignalName.Timeout);
		_isInvincible = false;
	}

	private void Die()
	{
		GD.Print("El jugador ha muerto. Reiniciando nivel...");
		Input.MouseMode = Input.MouseModeEnum.Visible;
		GetTree().ReloadCurrentScene();
	}
}
