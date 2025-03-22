using Godot;

namespace D_Platformer.Scripts;

public partial class Buff : Node2D
{
    private const double BoostMultiplier = 1.5;
        
    #region Built-in functions
    
    public override void _Ready()
    {
    }

    public override void _Process(double delta)
    {
    }
    
    #endregion

    private void OnAreaEntered(Node2D node)
    {
        if (node is not Player player) return;
        
        var newDamageValue = (int)(player.GetPlayerDamage() * BoostMultiplier);
        player.SetPlayerDamage(newDamageValue);
        QueueFree();
    }
}
