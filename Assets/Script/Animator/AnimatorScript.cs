using UnityEngine;

public class AnimatorScript : MonoBehaviour
{
    // Directional animation hashes
    private static readonly int Idle_UpRight = Animator.StringToHash("Idle_UpRight");
    private static readonly int Idle_UpLeft = Animator.StringToHash("Idle_UpLeft");
    private static readonly int Idle_DownRight = Animator.StringToHash("Idle_DownRight");
    private static readonly int Idle_DownLeft = Animator.StringToHash("Idle_DownLeft");
    // Add similar hashes for Walk, Attack, etc.

    private Animator animator;
    private AnimationState currentAnimationState;
    private CharacterStat.Direction currentDirection;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        InitializeDefaultAnimation();
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
            _ => "Idle"
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

    private void InitializeDefaultAnimation()
    {
        SetAnimation(AnimationState.Idle);
        SetDirection(CharacterStat.Direction.UpRight);
    }
}