using UnityEngine;

public class AnimatorScript : MonoBehaviour
{
    [SerializeField] private AnimationState testAnimationState; // Serialized field for testing animation state
    [SerializeField] private CharacterStat.Direction testDirection; // Serialized field for testing direction

    private Unit currentUnit;

    // Directional animation hashes for Idle
    private static readonly int Idle_UpRight = Animator.StringToHash("Idle_UpRight");
    private static readonly int Idle_UpLeft = Animator.StringToHash("Idle_UpLeft");
    private static readonly int Idle_DownRight = Animator.StringToHash("Idle_DownRight");
    private static readonly int Idle_DownLeft = Animator.StringToHash("Idle_DownLeft");

    // Directional animation hashes for Walk
    private static readonly int Walking_UpRight = Animator.StringToHash("Walking_UpRight");
    private static readonly int Walking_UpLeft = Animator.StringToHash("Walking_UpLeft");
    private static readonly int Walking_DownRight = Animator.StringToHash("Walking_DownRight");
    private static readonly int Walking_DownLeft = Animator.StringToHash("Walking_DownLeft");

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
        Walking,
        Attacking,
        Casting,
        Death,
        Extra
    }

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found on this GameObject or its children!", this);
        }
        else if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError("Animator does not have an AnimatorController assigned!", this);
        }

        // Hardcode for testing
        SetAnimation(AnimationState.Walking);
        SetDirection(CharacterStat.Direction.UpLeft);
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
        Debug.Log($"Playing animation: {currentAnimationState} {currentDirection} (Hash: {stateHash})");
        animator.CrossFade(stateHash, 0f, 0); // Instant transition
    }

    private int GetAnimationHash(AnimationState state, CharacterStat.Direction dir)
    {
        // Return the pre-defined animation hash based on the state and direction
        return (state, dir) switch
        {
            (AnimationState.Idle, CharacterStat.Direction.UpRight) => Idle_UpRight,
            (AnimationState.Idle, CharacterStat.Direction.UpLeft) => Idle_UpLeft,
            (AnimationState.Idle, CharacterStat.Direction.DownRight) => Idle_DownRight,
            (AnimationState.Idle, CharacterStat.Direction.DownLeft) => Idle_DownLeft,

            (AnimationState.Walking, CharacterStat.Direction.UpRight) => Walking_UpRight,
            (AnimationState.Walking, CharacterStat.Direction.UpLeft) => Walking_UpLeft,
            (AnimationState.Walking, CharacterStat.Direction.DownRight) => Walking_DownRight,
            (AnimationState.Walking, CharacterStat.Direction.DownLeft) => Walking_DownLeft,

            (AnimationState.Attacking, CharacterStat.Direction.UpRight) => Attack_UpRight,
            (AnimationState.Attacking, CharacterStat.Direction.UpLeft) => Attack_UpLeft,
            (AnimationState.Attacking, CharacterStat.Direction.DownRight) => Attack_DownRight,
            (AnimationState.Attacking, CharacterStat.Direction.DownLeft) => Attack_DownLeft,

            (AnimationState.Casting, CharacterStat.Direction.UpRight) => Casting_UpRight,
            (AnimationState.Casting, CharacterStat.Direction.UpLeft) => Casting_UpLeft,
            (AnimationState.Casting, CharacterStat.Direction.DownRight) => Casting_DownRight,
            (AnimationState.Casting, CharacterStat.Direction.DownLeft) => Casting_DownLeft,

            // Add cases for Death and Extra if needed...
            _ => throw new System.ArgumentOutOfRangeException()
        };
    }
}