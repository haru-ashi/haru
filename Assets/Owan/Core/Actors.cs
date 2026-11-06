namespace Owan
{
    public sealed class Player
    {
        public int Index;
        public int Team { get { return Index / 2; } }
        public Point Position;
        public Point Aim;
        public int Selected;
        public bool Cpu;
        public float Cooldown;
        public float ThinkTime;
        public Food Ingredient { get { return GameData.Inventory[Index % 2][Selected]; } }
    }

    public sealed class Bowl
    {
        public int Id;
        public int Team;
        public Point Position;
        public int Ingredients;
    }

    public sealed class Issun
    {
        public int Id;
        public int Team;
        public bool Good;
        public Point Position;
        public float ActionTime;
        public float TransferCooldown;
        public int TargetId = -1;
    }

    public sealed class Projectile
    {
        public int Id;
        public Food Ingredient;
        public Point Position;
        public Point Direction;
        public float Travelled;
        public int Team;
    }

    public sealed class Team
    {
        public int Score;
        public int Served;
        public int Rejected;
        public int OrderIndex;
        public int Belly;
        public string Feedback = "READY!";
        public float FeedbackTime;
        public Recipe Order { get { return GameData.Recipes[OrderIndex]; } }
    }
}
