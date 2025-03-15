using System.Collections.Generic;
using UnityEngine;

public class DirectionHandler : MonoBehaviour
{
    private ArrowTranslator _arrowTranslator = new ArrowTranslator();

    public List<CharacterStat.Direction> GetFacingDirections(List<OverlayTile> path)
    {
        List<CharacterStat.Direction> facingDirections = new List<CharacterStat.Direction>();

        for (int i = 0; i < path.Count; i++)
        {
            OverlayTile previousTile = i > 0 ? path[i - 1] : null;
            OverlayTile currentTile = path[i];
            OverlayTile futureTile = i < path.Count - 1 ? path[i + 1] : null;

            ArrowTranslator.ArrowDirection arrowDir =
                _arrowTranslator.TranslateDirection(previousTile, currentTile, futureTile);

            // Convert ArrowDirection to CharacterStat.Direction
            CharacterStat.Direction direction = ConvertArrowDirection(arrowDir);
            facingDirections.Add(direction);
        }

        return facingDirections;
    }

    private CharacterStat.Direction ConvertArrowDirection(ArrowTranslator.ArrowDirection arrowDir)
    {
        switch (arrowDir)
        {
            case ArrowTranslator.ArrowDirection.Up:
                return CharacterStat.Direction.UpRight; // North
            case ArrowTranslator.ArrowDirection.Down:
                return CharacterStat.Direction.DownLeft; // South
            case ArrowTranslator.ArrowDirection.Left:
                return CharacterStat.Direction.UpLeft; // West
            case ArrowTranslator.ArrowDirection.Right:
                return CharacterStat.Direction.DownRight; // East
            default:
                return CharacterStat.Direction.UpRight; // Default to North
        }
    }
}