using System;

namespace Owan
{
    public static class CpuController
    {
        public static void Step(Match match, Player player, float dt)
        {
            player.ThinkTime -= dt;
            Bowl target = FindBowl(match, player);
            if (target == null) return;
            float flight = Math.Abs(player.Position.Y - target.Position.Y) / match.Settings.ThrowSpeed;
            Point goal = new Point(Match.Clamp(target.Position.X + Match.Direction(player.Team) * match.Settings.LaneSpeed * flight, -8.3f, 8.3f), player.Team == 0 ? 3.1f : -3.1f);
            match.MovePlayer(player, goal - player.Position, dt);
            if (player.ThinkTime > 0 || player.Cooldown > 0) return;
            player.ThinkTime = match.Settings.CpuReactionSeconds * (0.7f + (float)match.Random.NextDouble() * 0.6f);
            int selected = FindIngredient(match, player, target);
            if (selected < 0) return;
            player.Selected = selected;
            if (match.Random.NextDouble() < match.Settings.CpuMistakeChance) player.Selected = match.Random.Next(4);
            player.Aim = new Point(goal.X - player.Position.X, target.Position.Y - player.Position.Y);
            TryAttack(match, player);
            match.Throw(player);
        }

        private static Bowl FindBowl(Match match, Player player)
        {
            Bowl best = null;
            float nearest = float.MaxValue;
            foreach (Bowl bowl in match.Bowls)
            {
                if (bowl.Team != player.Team || FindIngredient(match, player, bowl) < 0) continue;
                // 完成不能な汚れたお椀に全員が固執するのを避ける。
                if ((bowl.Ingredients & ~match.Teams[player.Team].Order.Ingredients) != 0) continue;
                float distance = Math.Abs(bowl.Position.X - player.Position.X);
                if (distance < nearest) { nearest = distance; best = bowl; }
            }
            return best;
        }

        private static int FindIngredient(Match match, Player player, Bowl bowl)
        {
            int needed = match.Teams[player.Team].Order.Ingredients & ~bowl.Ingredients;
            Food[] foods = GameData.Inventory[player.Index % 2];
            for (int i = 0; i < foods.Length; i++) if (GameData.Contains(needed, foods[i])) return i;
            return -1;
        }

        private static void TryAttack(Match match, Player player)
        {
            if (match.Random.NextDouble() >= match.Settings.CpuAttackChance) return;
            foreach (Issun issun in match.Issuns)
                if (!issun.Good && issun.Team == player.Team)
                { player.Aim = issun.Position - player.Position; return; }
        }
    }
}
