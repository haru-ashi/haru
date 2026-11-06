using System;

namespace Owan
{
    [Serializable]
    public sealed class Tuning
    {
        // 企画担当がここだけで試合のテンポを変えられるようにする。
        public float MatchSeconds = 120f;
        public float LaneSpeed = 0.95f;
        public float BowlInterval = 2.8f;
        public float PlayerSpeed = 5f;
        public float ThrowSpeed = 12f;
        public float ThrowRange = 13f;
        public float ThrowCooldown = 0.38f;
        public int BellyMaximum = 24;
        public float GoodSpeed = 0.65f;
        public float GoodCleanSeconds = 1.3f;
        public float FearDistance = 0.8f;
        public float BadSpeed = 0.42f;
        public float BadEatSeconds = 2.3f;
        public float CpuReactionSeconds = 0.65f;
        public float CpuMistakeChance = 0.07f;
        public float CpuAttackChance = 0.09f;
    }
}
