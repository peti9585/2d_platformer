using System.Collections.Generic;
using Godot;

namespace D_Platformer.Scripts;

public partial class Player : CharacterBody2D
{
    private static readonly float Gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
    
    private readonly List<string> whiteListedAnimationsList = ["Attack_Left", "Attack_Right", "Idle_Left", "Idle_Right"];
    
    private const int AttackAreaPosition = 18;
    private const float Friction = 0.3f;
    
    private bool facingLeft;
    private bool attacking;
    private bool canAttackEnemy;
    private Enemy currentEnemy;
    private Timer attackTimer;
    private Area2D attackArea;
    private AnimatedSprite2D animatedSprite;
    private ProgressBar healthBar;

    private int health = 100;

    #region Built-in functions
    
    public override void _Ready()
    {
        animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        attackTimer = GetNode<Timer>("AttackTimer");
        attackArea = GetNode<Area2D>("AttackArea"); 
        healthBar = GetNode<ProgressBar>("Camera2D/CanvasLayer/Control/HealthBar");
        
        healthBar.MaxValue = health;
        healthBar.Value = health;
    }

    public override void _Process(double delta)
    {
        MovePlayerBasedOnInput(delta);
    }
    
    #endregion

    #region Health-related functions
    
    public int GetPlayerHealth() => health;

    public void DamagePlayer(int damage)
    {
        health -= damage;
        healthBar.Value = health;

        if (health <= 0)
        {
            GD.Print("Game Over");
            GetTree().Paused = true;
        }
    }
    
    #endregion
    
    #region Movement-related functions
    
    private void MovePlayerBasedOnInput(double delta)
    {
        var velocity = Velocity;

        velocity.Y += Gravity * (float)delta;
        velocity.X = Lerp(velocity.X, 0, Friction);

        Velocity = GetInput(velocity);

        MoveAndSlide();
    }

    private Vector2 GetInput(Vector2 velocity)
    {
        if (CanMakeMovement("jump"))
        {
            velocity.Y = -300;
            
            animatedSprite.Play(facingLeft ? "Jump_Left" : "Jump_Right");
        }
        else if (Input.IsActionPressed("left"))
        {
            velocity.X = -200;
            attackArea.Position = new Vector2(-AttackAreaPosition, attackArea.Position.Y);
            facingLeft = true;

            if (IsOnFloor())
            {
                animatedSprite.Play("Run_Left");
            }
        }
        else if (Input.IsActionPressed("right"))
        {
            velocity.X = 200;
            attackArea.Position = new Vector2(AttackAreaPosition, attackArea.Position.Y);
            facingLeft = false;

            if (IsOnFloor())
            {
                animatedSprite.Play("Run_Right");
            }
        }
        else if (CanMakeMovement("attack") && !attacking)
        {
            GD.Print("Pressed attack");
            
            Attack();
        }
        else if (IsOnFloor() &&
                 !Input.IsAnythingPressed() &&
                 !whiteListedAnimationsList.Contains(animatedSprite.Animation))
        {
            animatedSprite.Stop();
            PlayIdleAnimation();
        }

        return velocity;
    }
    
    private bool CanMakeMovement(string actionPressed)
    {
        return Input.IsActionJustPressed(actionPressed) && IsOnFloor();
    }
    
    #endregion
    
    #region Animation-related functions

    private void OnAnimationFinished()
    {
        if (IsOnFloor())
        {
            PlayIdleAnimation();
        }
    }
    
    private void PlayIdleAnimation()
    {
        animatedSprite.Play(facingLeft ? "Idle_Left" : "Idle_Right");
    }
    
    #endregion
    
    #region Attack-related functions

    private void Attack()
    {
        attackTimer.Start();
        attacking = true;

        animatedSprite.Play(facingLeft ? "Attack_Left" : "Attack_Right");

        if (canAttackEnemy)
        {
            currentEnemy.DamageEnemy(10);
        }
    }
    
    private void AttackTimerOnTimeout()
    {
        attacking = false;
        attackTimer.Stop();
    }

    private void AttackCollisionBodyEntered(Node2D body)
    {
        if (IsEnemy(body))
        {
            currentEnemy = (Enemy)body;
            canAttackEnemy = true;
        }
    }

    private void AttackCollisionBodyExited(Node2D body)
    {
        if (IsEnemy(body))
        {
            canAttackEnemy = false;
        }
    }
    
    #endregion

    private static bool IsEnemy(Node2D body)
        => body.Name.ToString().Contains("Enemy");
    
    private static float Lerp(float firstFloat, float secondFloat, float by)
        => firstFloat * (1 - by) + secondFloat * by;
}