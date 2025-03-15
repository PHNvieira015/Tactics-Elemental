using UnityEngine;

[RequireComponent(typeof(Unit))]
public class DirectionHandler : MonoBehaviour
{
    private Unit unit;
    private AnimatorScript animatorScript;

    private void Awake()
    {
        unit = GetComponent<Unit>();
        animatorScript = GetComponent<AnimatorScript>();
    }

    public void UpdateFacingDirection(Vector2Int movementDirection)
    {
        CharacterStat.Direction newDirection = GetDirectionFromVector(movementDirection);
        if (unit.characterStats.faceDirection != newDirection)
        {
            unit.characterStats.faceDirection = newDirection;
            UpdateDirectionAnimation();
        }
    }

    private CharacterStat.Direction GetDirectionFromVector(Vector2Int dir)
    {
        if (dir.x > 0)
            return dir.y > 0 ? CharacterStat.Direction.UpRight : CharacterStat.Direction.DownRight;
        else
            return dir.y > 0 ? CharacterStat.Direction.UpLeft : CharacterStat.Direction.DownLeft;
    }

    public void SetFaceDirectionAtTurnEnd()
    {
        // Logic to maintain facing direction at turn end
        UpdateDirectionAnimation();
    }

    private void UpdateDirectionAnimation()
    {
        animatorScript.SetDirection(unit.characterStats.faceDirection);
    }
}