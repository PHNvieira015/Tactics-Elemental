
public enum Stats
{
    Health,
    Mana,
    Strenght,
    Endurance,
    Speed,
    Intelligence,
    MoveRange,
    AttackRange,
    CurrentHealth,
    CurrentMana
}

public enum Operation
{
    Add,
    Minus,
    Multiply,
    Divide,
    AddByPercentage,
    MinusByPercentage
}

public enum TileTypes
{
    Traversable,
    NonTraversable,
    Effect,
    PlayerUnitBlocked,
    EnemyUnitBlocked,
    Spawner,
}

public enum MovementDirection
{
    Up = 0,
    Down = 1,
    Left = 2,
    Right = 3
}

public enum ArrowDirection
{
    None = 0,
    Up = 1,
    Down = 2,
    Left = 3,
    Right = 4,
    TopLeft = 5,
    BottomLeft = 6,
    TopRight = 7,
    BottomRight = 8,
    UpFinished = 9,
    DownFinished = 10,
    LeftFinished = 11,
    RightFinished = 12
}