using Godot;

namespace D_Platformer.Scripts;

public partial class Buff : Node2D
{
    private const double BoostMultiplier = 1.5;
    
    private Area2D _buffArea;
    private Timer _buffTimer;
    private Player _player;
    private int _playerDamageBeforeBoost;
        
    #region Built-in functions
    
    public override void _Ready()
    {
        _buffArea = GetNode<Area2D>("BuffBody/Area2D");
        _buffTimer = GetNode<Timer>("BuffTimer");
    }

    public override void _Process(double delta)
    {
    }
    
    #endregion

    private void OnAreaEntered(Node2D node)
    {
        if (node is not Player player) return;
        _player = player;
        
        _buffArea.Disconnect("body_entered", new Callable(this, nameof(OnAreaEntered)));
        
        _playerDamageBeforeBoost = player.GetPlayerDamage();
        var newDamageValue = (int)(_playerDamageBeforeBoost * BoostMultiplier);
        player.SetPlayerDamage(newDamageValue);
        
        _buffTimer.Start();
    }

    private void OnBuffTimerTimeout()
    {
        _player.SetPlayerDamage(_playerDamageBeforeBoost);
        QueueFree();
    }
}
