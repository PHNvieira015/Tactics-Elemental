using UnityEngine;

public class ArrowTranslator
{
    public enum ArrowDirection
    {
        None = 0,          //Arrows_0 - nothing - None
        Up = 1,            //Arrows_4 - Midle - UpLeft
        Down = 2,          // Arrows_6 - Midle - DownRight
        Left = 3,          // Arrows_5 - Midle - DownRight or the opposite..
        Right = 4,         // Arrows_7 - Midle - DownRight or the opposite..
        TopLeft = 5,       // Arrows_8 - Midle - UpRight or the opposite..
        BottomLeft = 6,    //Arrows_11 - Corner -  
        TopRight = 7,      //Arrows_9 - Corner - 
        BottomRight = 8,   //Arrows_10 - Corner -  
        UpFinished = 9,    //Arrows_3 - Finisher - UpRight
        DownFinished = 10, //Arrows_1 - Finisher - BottonRight
        LeftFinished = 11, //Arrows_2 - Finisher - DownLeft
        RightFinished = 12 //Arrows_0 - Finisher - UpLeft
    }

    public ArrowDirection TranslateDirection(OverlayTile previousTile, OverlayTile currentTile, OverlayTile futureTile)
    {
        bool isFinal = futureTile == null;

        Vector2Int pastDirection = previousTile != null ? (Vector2Int)(currentTile.gridLocation - previousTile.gridLocation) : new Vector2Int(0, 0);
        Vector2Int futureDirection = futureTile != null ? (Vector2Int)(futureTile.gridLocation - currentTile.gridLocation) : new Vector2Int(0, 0);
        Vector2Int direction = pastDirection != futureDirection ? pastDirection + futureDirection : futureDirection;

        if (direction == new Vector2(0, 1) && !isFinal)
        {
            return ArrowDirection.Up;
        }

        if (direction == new Vector2(0, -1) && !isFinal)
        {
            return ArrowDirection.Down;
        }

        if (direction == new Vector2(1, 0) && !isFinal)
        {
            return ArrowDirection.Right;
        }

        if (direction == new Vector2(-1, 0) && !isFinal)
        {
            return ArrowDirection.Left;
        }

        if (direction == new Vector2(1, 1))
        {
            if (pastDirection.y < futureDirection.y)
                return ArrowDirection.BottomLeft;
            else
                return ArrowDirection.TopRight;
        }

        if (direction == new Vector2(-1, 1))
        {
            if (pastDirection.y < futureDirection.y)
                return ArrowDirection.BottomRight;
            else
                return ArrowDirection.TopLeft;
        }

        if (direction == new Vector2(1, -1))
        {
            if (pastDirection.y > futureDirection.y)
                return ArrowDirection.TopLeft;
            else
                return ArrowDirection.BottomRight;
        }

        if (direction == new Vector2(-1, -1))
        {
            if (pastDirection.y > futureDirection.y)
                return ArrowDirection.TopRight;
            else
                return ArrowDirection.BottomLeft;
        }

        if (direction == new Vector2(0, 1) && isFinal)
        {
            return ArrowDirection.UpFinished;
        }

        if (direction == new Vector2(0, -1) && isFinal)
        {
            return ArrowDirection.DownFinished;
        }

        if (direction == new Vector2(-1, 0) && isFinal)
        {
            return ArrowDirection.LeftFinished;
        }

        if (direction == new Vector2(1, 0) && isFinal)
        {
            return ArrowDirection.RightFinished;
        }

        return ArrowDirection.None;
    }


}