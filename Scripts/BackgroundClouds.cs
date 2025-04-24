using Godot;

namespace D_Platformer.Scripts;

public partial class BackgroundClouds : ParallaxLayer
{
    private const float CloudSpeed = -20;
    
    #region Built-in functions
    
    public override void _Process(double delta)
    {
        var calculatedMotionOffsetX = MotionOffset.X + CloudSpeed * (float)delta;
        MotionOffset = new Vector2(calculatedMotionOffsetX, MotionOffset.Y);
    }
    
    #endregion
}
