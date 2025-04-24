using Godot;

namespace D_Platformer.Scripts;

public partial class Enemy : CharacterBody2D
{
	private static readonly float Gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
	
	private const float Friction = 0.3f;
	private const float EnemySpeed = 1.3f;
	private const int AttackAreaPosition = 18;
	
	private int _health = 100;
	private bool _isPlayerInEnemyArea;
	private Player _player;
	private AnimatedSprite2D _animatedSprite;
	private Timer _healthBarTimer;
	private Timer _attackTimer;
	private ProgressBar _healthBar;
	private Area2D _attackArea;
	private Tween _tween;
	private RayCast2D _rayCastToPreventFalling;
	private CharacterState _enemyState = CharacterState.Idle;

	[Export]
	public int EnemyDamage;

	#region Built-in functions
	
	public override void _Ready()
	{
		_player = GetTree().Root
			.GetNode("Main")
			.GetNode<Player>(Constants.Player);
		_animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_healthBarTimer = GetNode<Timer>("HealthBar/HealthBarTimer");
		_attackTimer = GetNode<Timer>("AttackArea/AttackTimer");
		_healthBar = GetNode<ProgressBar>("HealthBar");
		_attackArea = GetNode<Area2D>("AttackArea");
		_rayCastToPreventFalling = GetNode<RayCast2D>("RayCast2D");
		
		_healthBar.MaxValue = _health;
		_healthBar.Value = _health;
	}

	public override void _Process(double delta)
	{
		var velocity = Velocity;
		velocity.Y += Gravity * (float)delta;
		velocity.X = CharacterBaseHandler.Lerp(velocity.X, 0, Friction);
		Velocity = new Vector2(velocity.X * EnemySpeed, velocity.Y);
		MoveAndSlide();

		if (_isPlayerInEnemyArea && _rayCastToPreventFalling.IsColliding() && CanMoveToPlayer())
		{
			MoveTowardsPlayer(delta);
		}
		else if (!_rayCastToPreventFalling.IsColliding())
		{
			Velocity = Vector2.Zero;
		}

		if ((_player.Position.X < Position.X && _rayCastToPreventFalling.Position.X > 0)
		    || (_player.Position.X > Position.X && _rayCastToPreventFalling.Position.X < 0))
		{
			_rayCastToPreventFalling.Position = new Vector2(_rayCastToPreventFalling.Position.X * -1, _rayCastToPreventFalling.Position.Y);
		}
		CharacterAnimationHandler.PlayAnimationBasedOnCharacterState(_enemyState, _animatedSprite, _player.Position.X < Position.X);
	}
	
	#endregion
	
	#region Attack-related functions

	public void DamageEnemy(int damage)
	{
		_health -= damage;
		_healthBar.Value = _health;

		if (_healthBar.Modulate.A < 1)
		{
			_healthBar.SetIndexed("modulate:a", 1);
			_tween?.Kill();
		}

		if (_health <= 0) _enemyState = CharacterState.Dead;

		ShowHealthBarForSeconds(2);
	}
	
	private void OnAttackAreaBodyEntered(Node2D body)
	{
		if (!CharacterBaseHandler.IsTargetNode(body, Constants.Player)) return;
		
		_isPlayerInEnemyArea = false;
		_enemyState = CharacterState.Attacking;

		_attackTimer.Start(2);
	}

	private void OnAttackTimerTimeout()
	{
		_player.DamagePlayer(EnemyDamage);
	}

	private void OnAttackAreaBodyExited(Node2D body)
	{
		if (CharacterBaseHandler.IsTargetNode(body, Constants.Player))
		{
			_attackTimer.Stop();
			_isPlayerInEnemyArea = true;
		}
	}
	
	#endregion
	
	#region Health-related functions
	
	private void ShowHealthBarForSeconds(float seconds)
	{
		_healthBarTimer.WaitTime = seconds;
		_healthBarTimer.Start();
        
		_healthBar.Show();
	}

	private async void OnHealthBarTimerTimeout()
	{
		_healthBarTimer.Stop();

		_tween?.Kill();
		
		_tween = GetTree().CreateTween();
		_tween.TweenProperty(_healthBar, "modulate:a", 0, 3);
		await ToSignal(_tween, "finished");
        
		_healthBar.Hide();
		_healthBar.SetIndexed("modulate:a", 1);
	}
	
	#endregion
	
	#region Movement-related functions

	private void MoveTowardsPlayer(double delta)
	{
		_enemyState = CharacterState.Running;
		
		_attackArea.Position = _player.Position.X < Position.X ?
			new Vector2(-AttackAreaPosition, _attackArea.Position.Y) :
			new Vector2(AttackAreaPosition, _attackArea.Position.Y);
		
		var velocity = Velocity;
		velocity.Y += Gravity * (float)delta;
		velocity.X = CharacterBaseHandler.Lerp(velocity.X, 0, Friction);

		velocity.X += Position.DirectionTo(_player.Position).X;
		Velocity = new Vector2(velocity.X * EnemySpeed, velocity.Y);

		MoveAndSlide();
	}

	private void OnDetectAreaBodyEntered(Node2D body)
	{
		if (CharacterBaseHandler.IsTargetNode(body, Constants.Player))
		{
			_isPlayerInEnemyArea = true;
		}
	}

	private void OnDetectAreaBodyExited(Node2D body)
	{
		if (CharacterBaseHandler.IsTargetNode(body, Constants.Player))
		{
			_enemyState = CharacterState.Idle;
			_isPlayerInEnemyArea = false;
			
			var velocity = Velocity;
			Velocity = new Vector2(0, velocity.Y);
		}
	}
	
	private bool CanMoveToPlayer()
	{
		return (_player.Position.X < Position.X && _rayCastToPreventFalling.Position.X < 0) 
		       || (_player.Position.X > Position.X && _rayCastToPreventFalling.Position.X > 0);
	}
	
	#endregion

	#region Animation-related functions
	
	private void OnAnimationFinished()
	{
		if (_enemyState == CharacterState.Dead) QueueFree();
	}
	
	#endregion
}