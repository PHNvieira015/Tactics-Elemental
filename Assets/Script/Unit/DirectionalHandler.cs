using UnityEngine;
using static TurnStateManager;

[RequireComponent(typeof(Unit))]
public class DirectionHandler : MonoBehaviour
{
    private Unit currentUnit;
    private AnimatorScript animatorScript;

    private void Awake()
    {
        currentUnit = GetComponent<Unit>();
        animatorScript = GetComponent<AnimatorScript>();
    }

    public void UpdateFacingDirectionForState(TurnState state, Vector2Int? targetPosition = null)
    {
        CharacterStat.Direction newDirection = currentUnit.characterStats.faceDirection; // Default to current

        switch (state)
        {
            case TurnState.Moving:
                if (targetPosition.HasValue)
                {
                    // Convert the unit's current position (Vector3) to Vector2Int by casting to Vector2
                    Vector2Int unitPosition = new Vector2Int((int)currentUnit.transform.position.x, (int)currentUnit.transform.position.y);
                    newDirection = GetDirectionFromVector(targetPosition.Value - unitPosition);
                }
                break;

            case TurnState.Attacking:
            case TurnState.SkillTargeting:
                if (targetPosition.HasValue)
                {
                    Vector2Int unitPosition = new Vector2Int((int)currentUnit.transform.position.x, (int)currentUnit.transform.position.y);
                    newDirection = GetDirectionFromVector(targetPosition.Value - unitPosition);
                }
                break;

            case TurnState.Spawning:
                Vector2Int spawnPosition = new Vector2Int((int)GetEnemySpawnerPosition().x, (int)GetEnemySpawnerPosition().y);
                newDirection = GetDirectionFromVector(spawnPosition - new Vector2Int((int)currentUnit.transform.position.x, (int)currentUnit.transform.position.y));
                break;
        }

        // Only update if there's a change in direction
        if (currentUnit.characterStats.faceDirection != newDirection)
        {
            currentUnit.characterStats.faceDirection = newDirection;
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

    private Vector2Int GetEnemySpawnerPosition()
    {
        // Replace this with actual logic to get the enemy spawner position
        return new Vector2Int(0, 0);
    }

    private void UpdateDirectionAnimation()
    {
        animatorScript.SetDirection(currentUnit.characterStats.faceDirection);
    }
}
