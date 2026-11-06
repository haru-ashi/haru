using System;

namespace Owan
{
    public enum Food { Beef, Onion, Cutlet, Egg, Shrimp, Nori, Tuna, Salmon }

    public sealed class Recipe
    {
        public readonly string Name;
        public readonly int Ingredients;
        public readonly int Points;

        public Recipe(string name, int points, params Food[] foods)
        {
            Name = name;
            Points = points;
            foreach (Food food in foods) Ingredients |= GameData.Bit(food);
        }
    }

    public static class GameData
    {
        // 配分と注文を一か所に置き、必ず相方の食材も必要な形にする。
        public static readonly Food[][] Inventory = {
            new[] { Food.Beef, Food.Cutlet, Food.Shrimp, Food.Tuna },
            new[] { Food.Onion, Food.Egg, Food.Nori, Food.Salmon }
        };
        public static readonly Recipe[] Recipes = {
            new Recipe("GYUDON", 2, Food.Beef, Food.Onion),
            new Recipe("TENDON", 2, Food.Shrimp, Food.Nori),
            new Recipe("KATSUDON", 3, Food.Cutlet, Food.Egg, Food.Onion),
            new Recipe("KAISENDON", 4, Food.Tuna, Food.Salmon, Food.Egg, Food.Nori)
        };
        public static int Bit(Food food) { return 1 << (int)food; }
        public static bool Contains(int mask, Food food) { return (mask & Bit(food)) != 0; }
        public static int Count(int mask)
        {
            int count = 0;
            for (int i = 0; i < 8; i++) if ((mask & (1 << i)) != 0) count++;
            return count;
        }
        public static Food First(int mask)
        {
            for (int i = 0; i < 8; i++) if ((mask & (1 << i)) != 0) return (Food)i;
            return Food.Beef;
        }
    }

    public struct Point
    {
        public float X;
        public float Y;
        public Point(float x, float y) { X = x; Y = y; }
        public float Length { get { return (float)Math.Sqrt(X * X + Y * Y); } }
        public Point Normalized { get { float n = Length; return n < 0.001f ? new Point() : this * (1f / n); } }
        public static Point operator +(Point a, Point b) { return new Point(a.X + b.X, a.Y + b.Y); }
        public static Point operator -(Point a, Point b) { return new Point(a.X - b.X, a.Y - b.Y); }
        public static Point operator *(Point a, float n) { return new Point(a.X * n, a.Y * n); }
        public static float Distance(Point a, Point b) { return (a - b).Length; }
    }
}
