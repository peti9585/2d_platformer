using System;
using Godot;

namespace D_Platformer.Scripts;

internal static class CharacterAnimationHandler
{
    internal static void PlayAnimationBasedOnCharacterState(CharacterState state, AnimatedSprite2D animatedSprite, bool facingLeft)
    {
        switch (state)
        {
            case CharacterState.Idle: 
                animatedSprite.Play(facingLeft ? "Idle_Left" : "Idle_Right"); 
                break;
            case CharacterState.Running:
                animatedSprite.Play(facingLeft ? "Run_Left" : "Run_Right");
                break;
            case CharacterState.Jumping:
                animatedSprite.Play(facingLeft ? "Jump_Left" : "Jump_Right");
                break;
            case CharacterState.Attacking:
                animatedSprite.Play(facingLeft ? "Attack_Left" : "Attack_Right");
                break;
            case CharacterState.Dead:
                animatedSprite.Play("Die");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}