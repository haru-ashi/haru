using System;

namespace Owan
{
    public static class ProjectileSystem
    {
        public static void Step(Match match, float dt)
        {
            for (int i = match.Projectiles.Count - 1; i >= 0; i--)
            {
                Projectile shot = match.Projectiles[i];
                float distance = Math.Min(match.Settings.ThrowSpeed * dt, match.Settings.ThrowRange - shot.Travelled);
                Point end = shot.Position + shot.Direction * distance;
                if (HitFirst(match, shot, end)) { match.Projectiles.RemoveAt(i); continue; }
                shot.Position = end;
                shot.Travelled += distance;
                if (shot.Travelled >= match.Settings.ThrowRange) match.Projectiles.RemoveAt(i);
            }
        }

        private static bool HitFirst(Match match, Projectile shot, Point end)
        {
            float first = float.MaxValue;
            Bowl hitBowl = null;
            Issun hitIssun = null;
            foreach (Bowl bowl in match.Bowls)
            {
                float time = EntryTime(shot.Position, end, bowl.Position, 0.48f);
                if (time < first) { first = time; hitBowl = bowl; hitIssun = null; }
            }
            foreach (Issun issun in match.Issuns)
            {
                float time = EntryTime(shot.Position, end, issun.Position, 0.28f);
                if (time < first) { first = time; hitIssun = issun; hitBowl = null; }
            }
            if (hitBowl != null) hitBowl.Ingredients |= GameData.Bit(shot.Ingredient);
            if (hitIssun != null) match.Transfer(hitIssun);
            return hitBowl != null || hitIssun != null;
        }

        private static float EntryTime(Point start, Point end, Point center, float radius)
        {
            // 線分の最初の交点を比較し、遠い対象へのすり抜けを防ぐ。
            Point offset = start - center;
            Point delta = end - start;
            float c = Dot(offset, offset) - radius * radius;
            if (c <= 0) return 0;
            float a = Dot(delta, delta);
            if (a < 0.0000001f) return float.MaxValue;
            float b = 2 * Dot(offset, delta);
            float discriminant = b * b - 4 * a * c;
            if (discriminant < 0) return float.MaxValue;
            float t = (-b - (float)Math.Sqrt(discriminant)) / (2 * a);
            return t >= 0 && t <= 1 ? t : float.MaxValue;
        }

        private static float Dot(Point a, Point b) { return a.X * b.X + a.Y * b.Y; }
    }
}
