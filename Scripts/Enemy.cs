using Godot;

namespace D_Platformer.Scripts;

public partial class Enemy : CharacterBody2D
{
	private static readonly float Gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
	
	private const float Friction = 0.3f;
	private const float EnemySpeed = 1.4f;
	private const int AttackAreaPosition = 18;
	
	private int health = 100;
	private bool isPlayerInEnemyArea;
	private Player player;
	private AnimatedSprite2D animatedSprite;
	private Timer healthBarTimer;
	private Timer attackTimer;
	private ProgressBar healthBar;
	private Area2D attackArea;
	private Tween tween;
	private RayCast2D rayCastToPreventFalling;

	#region Built-in functions
	
	public override void _Ready()
	{
		player = GetTree().Root
			.GetNode("Main")
			.GetNode<Player>("Player");
		animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		healthBarTimer = GetNode<Timer>("HealthBar/HealthBarTimer");
		attackTimer = GetNode<Timer>("AttackArea/AttackTimer");
		healthBar = GetNode<ProgressBar>("HealthBar");
		attackArea = GetNode<Area2D>("AttackArea");
		rayCastToPreventFalling = GetNode<RayCast2D>("RayCast2D");
		
		healthBar.MaxValue = health;
		healthBar.Value = health;
	}

	public override void _Process(double delta)
	{
		var velocity = Velocity;
		velocity.Y += Gravity * (float)delta;
		velocity.X = Lerp(velocity.X, 0, Friction);
		Velocity = new Vector2(velocity.X * EnemySpeed, velocity.Y);
		MoveAndSlide();

		if (isPlayerInEnemyArea && rayCastToPreventFalling.IsColliding() && CanMoveToPlayer())
		{
			MoveTowardsPlayer(delta);
		}
		else if (!rayCastToPreventFalling.IsColliding())
		{
			Velocity = Vector2.Zero;
			animatedSprite.Play("Idle");
		}

		if ((player.Position.X < Position.X && rayCastToPreventFalling.Position.X > 0)
		    || (player.Position.X > Position.X && rayCastToPreventFalling.Position.X < 0))
		{
			rayCastToPreventFalling.Position = new Vector2(rayCastToPreventFalling.Position.X * -1, rayCastToPreventFalling.Position.Y);
		}
	}
	
	#endregion
	
	#region Attack-related functions

	public void DamageEnemy(int damage)
	{
		health -= damage;
		healthBar.Value = health;

		if (healthBar.Modulate.A < 1)
		{
			healthBar.SetIndexed("modulate:a", 1);
			tween?.Kill();
		}

		if (health <= 0)
		{
			animatedSprite.Play("Die");
		}

		ShowHealthBarForSeconds(2);
	}
	
	private void OnAttackAreaBodyEntered(Node2D body)
	{
		if (IsPlayer(body))
		{
			isPlayerInEnemyArea = false;
			if (player.Position.X < Position.X)
			{
				animatedSprite.Play("Attack_Left");
			}
			else
			{
				animatedSprite.Play("Attack_Right");
			}
			
			attackTimer.Start(2);
		}
	}

	private void OnAttackTimerTimeout()
	{
		player.DamagePlayer(10);
	}

	private void OnAttackAreaBodyExited(Node2D body)
	{
		if (IsPlayer(body))
		{
			attackTimer.Stop();
			isPlayerInEnemyArea = true;
		}
	}
	
	#endregion
	
	#region Health-related functions
	
	private void ShowHealthBarForSeconds(float seconds)
	{
		healthBarTimer.WaitTime = seconds;
		healthBarTimer.Start();
        
		healthBar.Show();
	}

	private async void OnHealthBarTimerTimeout()
	{
		healthBarTimer.Stop();

		tween?.Kill();

		GD.Print("le fog allni");
		tween = GetTree().CreateTween();
		tween.TweenProperty(healthBar, "modulate:a", 0, 3);
		await ToSignal(tween, "finished");
        
		healthBar.Hide();
		healthBar.SetIndexed("modulate:a", 1);
	}
	
	#endregion
	
	#region Movement-related functions

	private void MoveTowardsPlayer(double delta)
	{
		if (player.Position.X < Position.X)
		{
			attackArea.Position = new Vector2(-AttackAreaPosition, attackArea.Position.Y);
			animatedSprite.Play("Run_Left");
		}
		else
		{
			attackArea.Position = new Vector2(AttackAreaPosition, attackArea.Position.Y);
			animatedSprite.Play("Run_Right");
		}
		var velocity = Velocity;
		velocity.Y += Gravity * (float)delta;
		velocity.X = Lerp(velocity.X, 0, Friction);

		velocity.X += Position.DirectionTo(player.Position).X;
		Velocity = new Vector2(velocity.X * EnemySpeed, velocity.Y);

		MoveAndSlide();
	}

	private void OnDetectAreaBodyEntered(Node2D body)
	{
		if (IsPlayer(body))
		{
			isPlayerInEnemyArea = true;
		}
	}

	private void OnDetectAreaBodyExited(Node2D body)
	{
		if (IsPlayer(body))
		{
			isPlayerInEnemyArea = false;
			
			animatedSprite.Play("Idle");
			
			var velocity = Velocity;
			Velocity = new Vector2(0, velocity.Y);
		}
	}
	
	private bool CanMoveToPlayer()
	{
		return (player.Position.X < Position.X && rayCastToPreventFalling.Position.X < 0) 
		       || (player.Position.X > Position.X && rayCastToPreventFalling.Position.X > 0);
	}
	
	#endregion

	private void OnAnimationFinished()
	{
		if (animatedSprite.GetAnimation() == "Die")
		{
			QueueFree();
		}
	}
	
	private static float Lerp(float firstFloat, float secondFloat, float by) 
		=> firstFloat * (1 - by) + secondFloat * by;
	
	private static bool IsPlayer(Node2D body)
		=> body.Name.ToString().Contains("Player");
}