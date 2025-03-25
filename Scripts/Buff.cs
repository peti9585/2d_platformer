using Godot;

namespace D_Platformer.Scripts;

public partial class Buff : Node2D
{
    private const double BoostMultiplier = 1.5;
    
    private static int _buffDurationInSeconds;
    
    private Area2D _buffArea;
    private Timer _buffTimer;
    private Player _player;
    private TextureProgressBar _buffTimerProgressBar;
    private StaticBody2D _buffBody;
    private int _playerDamageBeforeBoost;
    private int _buffDurationCounter;
        
    #region Built-in functions
    
    public override void _Ready()
    {
        _buffArea = GetNode<Area2D>("BuffBody/Area2D");
        _buffTimer = GetNode<Timer>("BuffTimer");
        _buffTimerProgressBar = GetNode<TextureProgressBar>("CanvasLayer/Control/BuffTimerProgressBar");
        _buffBody = GetNode<StaticBody2D>("BuffBody");

        _buffTimerProgressBar.Visible = false;
    }
    
    #endregion

    private void OnAreaEntered(Node2D node)
    {
        if (node is not Player player) return;
        _player = player;
        
        // Needs to be disconnected to avoid triggering the buff again
        _buffArea.Disconnect("body_entered", new Callable(this, nameof(OnAreaEntered)));
        
        _playerDamageBeforeBoost = player.GetPlayerDamage();
        var newDamageValue = (int)(_playerDamageBeforeBoost * BoostMultiplier);
        player.SetPlayerDamage(newDamageValue);

        _buffDurationInSeconds = (int)_buffTimerProgressBar.MaxValue;
        _buffTimer.Start();
        
        _buffTimerProgressBar.Visible = true;
        _buffTimerProgressBar.Value = _buffTimerProgressBar.MaxValue;
        
        // To hide the whole sword after the player picks up the buff
        _buffBody.Visible = false;
    }

    // This is called every second to adjust the ProgressBar value
    private void OnBuffTimerTimeout()
    {
        _buffDurationCounter++;
        _buffTimerProgressBar.Value -= 1;

        if (_buffDurationCounter >= _buffDurationInSeconds)
        {
            _player.SetPlayerDamage(_playerDamageBeforeBoost);
            QueueFree();
        }
    }
}
