using Godot;

namespace D_Platformer.Scripts;

internal static class CharacterBaseHandler
{
    internal static bool IsTargetNode(Node2D body, string targetNodeName)
        => body.Name.ToString().Contains(targetNodeName);
    
    internal static float Lerp(float firstFloat, float secondFloat, float by)
        => firstFloat * (1 - by) + secondFloat * by;
}