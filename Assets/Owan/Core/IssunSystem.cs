using System;

namespace Owan
{
    public static class IssunSystem
    {
        public static void Step(Match match, float dt)
        {
            foreach (Issun issun in match.Issuns)
            {
                issun.TransferCooldown = Math.Max(0, issun.TransferCooldown - dt);
                if (ShouldFlee(match, issun)) { match.Transfer(issun); continue; }
                Bowl target = FindTarget(match, issun);
                if (target == null) { Wander(match, issun, dt); continue; }
                Work(match, issun, target, dt);
            }
        }

        private static bool ShouldFlee(Match match, Issun good)
        {
            if (!good.Good || good.TransferCooldown > 0) return false;
            foreach (Issun other in match.Issuns)
                if (!other.Good && other.Team == good.Team && Point.Distance(good.Position, other.Position) < match.Settings.FearDistance) return true;
            return false;
        }

        private static Bowl FindTarget(Match match, Issun issun)
        {
            Bowl target = null;
            float nearest = float.MaxValue;
            foreach (Bowl bowl in match.Bowls)
            {
                if (bowl.Team != issun.Team || Removable(match, issun, bowl) == 0) continue;
                float distance = Point.Distance(issun.Position, bowl.Position);
                if (distance < nearest) { target = bowl; nearest = distance; }
            }
            return target;
        }

        private static int Removable(Match match, Issun issun, Bowl bowl)
        {
            return issun.Good ? bowl.Ingredients & ~match.Teams[issun.Team].Order.Ingredients : bowl.Ingredients;
        }

        private static void Work(Match match, Issun issun, Bowl bowl, float dt)
        {
            if (issun.TargetId != bowl.Id) { issun.TargetId = bowl.Id; issun.ActionTime = 0; }
            Point delta = bowl.Position - issun.Position;
            float speed = issun.Good ? match.Settings.GoodSpeed : match.Settings.BadSpeed;
            // 作業範囲に入ったらレーンと同速で付き添い、低速NPCでも食材に作用できる。
            if (delta.Length > 0.8f)
            {
                issun.Position += delta.Normalized * Math.Min(delta.Length, speed * dt);
                issun.ActionTime = 0;
                return;
            }
            issun.Position.X += Match.Direction(issun.Team) * match.Settings.LaneSpeed * dt;
            issun.ActionTime += dt;
            float duration = issun.Good ? match.Settings.GoodCleanSeconds : match.Settings.BadEatSeconds;
            if (issun.ActionTime < duration) return;
            int mask = Removable(match, issun, bowl);
            if (mask != 0) bowl.Ingredients &= ~GameData.Bit(GameData.First(mask));
            issun.ActionTime = 0;
        }

        private static void Wander(Match match, Issun issun, float dt)
        {
            issun.ActionTime = 0;
            issun.TargetId = -1;
            float speed = issun.Good ? match.Settings.GoodSpeed : match.Settings.BadSpeed;
            float direction = Math.Sin((match.Settings.MatchSeconds - match.Remaining) * 0.35f + issun.Id * 2) > 0 ? 1 : -1;
            issun.Position.X = Match.Clamp(issun.Position.X + direction * speed * dt, -8.5f, 8.5f);
        }
    }
}
