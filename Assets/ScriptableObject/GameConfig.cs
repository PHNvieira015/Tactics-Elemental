using UnityEngine;

namespace TacticsToolkit
{
    [CreateAssetMenu(fileName = "GameCOnfig", menuName = "Tactical RPG/GameCOnfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("Game Settings")]
        public int jumpCost;

        [Header("Overlay Colors")]
        public Color MoveRangeColor;
        public Color AttackRangeColor;
        public Color BlockedTileColor;

        [Header("Character Leveling")]
        public XPScalingMode xpScalingMode;

        [Header("Constant Leveling")]
        public int LevelIncreaseAmount;

        [Header("Animation Curve Leveling")]
        public AnimationCurve expCurve;
        public int MaxLevel;
        public int MaxRequiredExp;

        public enum XPScalingMode
        {
            Constant,
            Disgaea,
            AnimationCurve
        }

        public int GetRequiredExp(int level)
        {
            int requiredExperience = 0;
            float xp;
            switch (xpScalingMode)
            {
                case XPScalingMode.Constant:
                    xp = (LevelIncreaseAmount * level);
                    requiredExperience = Mathf.CeilToInt(xp);
                    break;
                case XPScalingMode.Disgaea:
                    requiredExperience = Mathf.RoundToInt(0.04f * Mathf.Pow(level, 3) + 0.8f * Mathf.Pow(level, 2) + 2 * level);
                    break;
                case XPScalingMode.AnimationCurve:
                    requiredExperience = Mathf.RoundToInt(expCurve.Evaluate(Mathf.InverseLerp(0, MaxLevel, level)) * MaxRequiredExp);
                    break;

                default:
                    break;
            }

            return requiredExperience;
        }
    }
}
