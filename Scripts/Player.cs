using System.Collections.Generic;
using Godot;

namespace D_Platformer.Scripts;

public partial class Player : CharacterBody2D
{
    private static readonly float Gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
    
    private readonly List<string> _whiteListedAnimationsList = ["Attack_Left", "Attack_Right", "Idle_Left", "Idle_Right"];
    
    private const int AttackAreaPosition = 18;
    private const float Friction = 0.3f;
    
    private bool _facingLeft;
    private bool _attacking;
    private bool _canAttackEnemy;
    private List<Enemy> _currentEnemyList = [];
    private Timer _attackTimer;
    private Area2D _attackArea;
    private AnimatedSprite2D _animatedSprite;
    private ProgressBar _healthBar;

    private int _health = 100;
    private int _damage = 20;

    #region Built-in functions
    
    public override void _Ready()
    {
        _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _attackTimer = GetNode<Timer>("AttackTimer");
        _attackArea = GetNode<Area2D>("AttackArea"); 
        _healthBar = GetNode<ProgressBar>("Camera2D/CanvasLayer/Control/HealthBar");
        
        _healthBar.MaxValue = _health;
        _healthBar.Value = _health;
    }

    public override void _Process(double delta)
    {
        MovePlayerBasedOnInput(delta);
    }
    
    #endregion

    #region Health-related functions
    
    public int GetPlayerHealth() => _health;

    public void DamagePlayer(int damage)
    {
        _health -= damage;
        _healthBar.Value = _health;

        if (_health <= 0)
        {
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
            
            _animatedSprite.Play(_facingLeft ? "Jump_Left" : "Jump_Right");
        }
        else if (Input.IsActionPressed("left"))
        {
            velocity.X = -200;
            _attackArea.Position = new Vector2(-AttackAreaPosition, _attackArea.Position.Y);
            _facingLeft = true;

            if (IsOnFloor())
            {
                _animatedSprite.Play("Run_Left");
            }
        }
        else if (Input.IsActionPressed("right"))
        {
            velocity.X = 200;
            _attackArea.Position = new Vector2(AttackAreaPosition, _attackArea.Position.Y);
            _facingLeft = false;

            if (IsOnFloor())
            {
                _animatedSprite.Play("Run_Right");
            }
        }
        else if (CanMakeMovement("attack") && !_attacking)
        {
            Attack();
        }
        else if (IsOnFloor() &&
                 !Input.IsAnythingPressed() &&
                 !_whiteListedAnimationsList.Contains(_animatedSprite.Animation))
        {
            _animatedSprite.Stop();
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
        _animatedSprite.Play(_facingLeft ? "Idle_Left" : "Idle_Right");
    }
    
    #endregion
    
    #region Attack-related functions
    
    public int GetPlayerDamage() => _damage;
    public void SetPlayerDamage(int damage) => _damage = damage;

    private void Attack()
    {
        _attackTimer.Start();
        _attacking = true;

        _animatedSprite.Play(_facingLeft ? "Attack_Left" : "Attack_Right");
        
        if (!_canAttackEnemy) return;
        
        foreach (var enemy in _currentEnemyList) enemy.DamageEnemy(_damage);
    }
    
    private void AttackTimerOnTimeout()
    {
        _attacking = false;
        _attackTimer.Stop();
    }

    private void AttackCollisionBodyEntered(Node2D body)
    {
        if (IsEnemy(body))
        {
            _currentEnemyList.Add((Enemy)body);
            SetCanAttackEnemyStatus();
        }
    }

    private void AttackCollisionBodyExited(Node2D body)
    {
        if (IsEnemy(body))
        {
            _currentEnemyList.Remove((Enemy)body);
            SetCanAttackEnemyStatus();
        }
    }

    private void SetCanAttackEnemyStatus() => _canAttackEnemy = _currentEnemyList.Count != 0;
    
    #endregion

    private static bool IsEnemy(Node2D body)
        => body.Name.ToString().Contains("Enemy");
    
    private static float Lerp(float firstFloat, float secondFloat, float by)
        => firstFloat * (1 - by) + secondFloat * by;
}