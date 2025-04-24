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
    private bool _canAttackEnemy;
    private List<Enemy> _currentEnemyList = [];
    private Timer _attackTimer;
    private Area2D _attackArea;
    private AnimatedSprite2D _animatedSprite;
    private TextureProgressBar _healthBar;
    private CharacterState _state = CharacterState.Idle;

    private int _health = 100;
    private int _damage = 20;

    #region Built-in functions
    
    public override void _Ready()
    {
        _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _attackTimer = GetNode<Timer>("AttackTimer");
        _attackArea = GetNode<Area2D>("AttackArea"); 
        _healthBar = GetNode<TextureProgressBar>("Camera2D/CanvasLayer/Control/HealthBar");
        
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
        velocity.X = CharacterBaseHandler.Lerp(velocity.X, 0, Friction);

        Velocity = GetInput(velocity);
        CharacterAnimationHandler.PlayAnimationBasedOnCharacterState(_state, _animatedSprite, _facingLeft);

        MoveAndSlide();
    }

    private Vector2 GetInput(Vector2 velocity)
    {
        if (CanMakeMovement("jump"))
        {
            _state = CharacterState.Jumping;
            velocity.Y = -300;
        }
        else if (Input.IsActionPressed("left"))
        {
            velocity.X = -200;
            _attackArea.Position = new Vector2(-AttackAreaPosition, _attackArea.Position.Y);
            _facingLeft = true;

            if (IsOnFloor())
            {
                _state = CharacterState.Running;
            }
        }
        else if (Input.IsActionPressed("right"))
        {
            velocity.X = 200;
            _attackArea.Position = new Vector2(AttackAreaPosition, _attackArea.Position.Y);
            _facingLeft = false;

            if (IsOnFloor())
            {
                _state = CharacterState.Running;
            }
        }
        else if (CanMakeMovement("attack"))
        {
            Attack();
        }
        else if (IsOnFloor() &&
                 !Input.IsAnythingPressed() &&
                 !_whiteListedAnimationsList.Contains(_animatedSprite.Animation))
        {
            _animatedSprite.Stop();
            _state = CharacterState.Idle;
        }

        return velocity;
    }

    private bool CanMakeMovement(string action)
        => Input.IsActionPressed(action) && _state is CharacterState.Idle or CharacterState.Running;
    
    #endregion
    
    #region Animation-related functions

    private void OnAnimationFinished()
    {
        if (IsOnFloor())
        {
            _state = CharacterState.Idle;
        }
    }
    
    #endregion
    
    #region Attack-related functions
    
    public int GetPlayerDamage() => _damage;
    public void SetPlayerDamage(int damage) => _damage = damage;

    private void Attack()
    {
        _attackTimer.Start();
        _state = CharacterState.Attacking;
        
        if (!_canAttackEnemy) return;
        
        foreach (var enemy in _currentEnemyList) enemy.DamageEnemy(_damage);
    }
    
    private void AttackTimerOnTimeout()
    {
        _state = CharacterState.Idle;
        _attackTimer.Stop();
    }

    private void AttackCollisionBodyEntered(Node2D body)
    {
        if (CharacterBaseHandler.IsTargetNode(body, Constants.Enemy))
        {
            _currentEnemyList.Add((Enemy)body);
            SetCanAttackEnemyStatus();
        }
    }

    private void AttackCollisionBodyExited(Node2D body)
    {
        if (CharacterBaseHandler.IsTargetNode(body, Constants.Enemy))
        {
            _currentEnemyList.Remove((Enemy)body);
            SetCanAttackEnemyStatus();
        }
    }

    private void SetCanAttackEnemyStatus() => _canAttackEnemy = _currentEnemyList.Count != 0;
    
    #endregion
}