using UnityEngine;
using static TurnStateManager;

public class AnimatorScript : MonoBehaviour
{
    // Directional animation hashes for Idle
    private static readonly int Idle_UpRight = Animator.StringToHash("Idle_UpRight");
    private static readonly int Idle_UpLeft = Animator.StringToHash("Idle_UpLeft");
    private static readonly int Idle_DownRight = Animator.StringToHash("Idle_DownRight");
    private static readonly int Idle_DownLeft = Animator.StringToHash("Idle_DownLeft");

    // Directional animation hashes for Walk
    private static readonly int Walk_UpRight = Animator.StringToHash("Walk_UpRight");
    private static readonly int Walk_UpLeft = Animator.StringToHash("Walk_UpLeft");
    private static readonly int Walk_DownRight = Animator.StringToHash("Walk_DownRight");
    private static readonly int Walk_DownLeft = Animator.StringToHash("Walk_DownLeft");

    // Directional animation hashes for Attack
    private static readonly int Attack_UpRight = Animator.StringToHash("Attack_UpRight");
    private static readonly int Attack_UpLeft = Animator.StringToHash("Attack_UpLeft");
    private static readonly int Attack_DownRight = Animator.StringToHash("Attack_DownRight");
    private static readonly int Attack_DownLeft = Animator.StringToHash("Attack_DownLeft");

    // Directional animation hashes for Casting
    private static readonly int Casting_UpRight = Animator.StringToHash("Casting_UpRight");
    private static readonly int Casting_UpLeft = Animator.StringToHash("Casting_UpLeft");
    private static readonly int Casting_DownRight = Animator.StringToHash("Casting_DownRight");
    private static readonly int Casting_DownLeft = Animator.StringToHash("Casting_DownLeft");

    // Add similar hashes for Death and Extra if needed...

    private Animator animator;
    private AnimationState currentAnimationState;
    private CharacterStat.Direction currentDirection;

    public enum AnimationState
    {
        Idle,
        Walk,
        Attack,
        Casting,
        Death,
        Extra
    }

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void SetAnimation(AnimationState newState)
    {
        if (currentAnimationState == newState) return;

        currentAnimationState = newState;
        UpdateAnimation();
    }

    public void SetDirection(CharacterStat.Direction newDirection)
    {
        if (currentDirection == newDirection) return;

        currentDirection = newDirection;
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        int stateHash = GetAnimationHash(currentAnimationState, currentDirection);
        animator.CrossFade(stateHash, 0.1f, 0);
    }

    private int GetAnimationHash(AnimationState state, CharacterStat.Direction dir)
    {
        string baseName = state switch
        {
            AnimationState.Idle => "Idle",
            AnimationState.Walk => "Walk",
            AnimationState.Attack => "Attack",
            AnimationState.Casting => "Casting",
            AnimationState.Death => "Death",
            AnimationState.Extra => "Extra",
            _ => throw new System.ArgumentOutOfRangeException()
        };

        string directionSuffix = dir switch
        {
            CharacterStat.Direction.UpRight => "_UpRight",
            CharacterStat.Direction.UpLeft => "_UpLeft",
            CharacterStat.Direction.DownRight => "_DownRight",
            CharacterStat.Direction.DownLeft => "_DownLeft",
            _ => "_UpRight"
        };

        return Animator.StringToHash(baseName + directionSuffix);
    }



}
