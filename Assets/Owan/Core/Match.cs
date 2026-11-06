using System;
using System.Collections.Generic;

namespace Owan
{
    public sealed class Match
    {
        public readonly Tuning Settings;
        public readonly Player[] Players = new Player[4];
        public readonly Team[] Teams = { new Team(), new Team() };
        public readonly List<Bowl> Bowls = new List<Bowl>();
        public readonly List<Issun> Issuns = new List<Issun>();
        public readonly List<Projectile> Projectiles = new List<Projectile>();
        public readonly Random Random;
        public float Remaining;
        public bool Paused;
        public bool Finished { get { return Remaining <= 0; } }
        public int Winner { get { return Teams[0].Score == Teams[1].Score ? -1 : Teams[0].Score > Teams[1].Score ? 0 : 1; } }
        private readonly float[] m_spawnTimes = { 0f, 0f };
        private int m_nextId;

        public Match(Tuning settings, int seed = 21)
        {
            Settings = settings;
            Random = new Random(seed);
            Remaining = settings.MatchSeconds;
            for (int i = 0; i < 4; i++) CreatePlayer(i);
            for (int t = 0; t < 2; t++)
            {
                Teams[t].OrderIndex = t;
                CreateIssun(t, true, -3f);
                CreateIssun(t, false, 4f);
            }
        }

        public static float LaneY(int team) { return team == 0 ? 1.25f : -1.25f; }
        public static float Direction(int team) { return team == 0 ? 1f : -1f; }
        public static float Clamp(float value, float min, float max) { return Math.Max(min, Math.Min(max, value)); }

        private void CreatePlayer(int index)
        {
            int team = index / 2;
            Players[index] = new Player {
                Index = index, Cpu = index != 0,
                Position = new Point(index % 2 == 0 ? -3f : 3f, team == 0 ? 4f : -4f),
                Aim = new Point(0, team == 0 ? -1 : 1)
            };
        }

        private void CreateIssun(int team, bool good, float x)
        {
            Issuns.Add(new Issun { Id = m_nextId++, Team = team, Good = good,
                Position = new Point(x, LaneY(team) + (good ? 0.35f : -0.35f)) });
        }

        public void Step(float delta)
        {
            if (Paused || Finished || delta <= 0) return;
            // 長いフレームでも当たり判定とタイマーの精度を保つ。
            bool reachesEnd = delta >= Remaining;
            float total = Math.Min(delta, Remaining);
            while (total > 0.00001f)
            {
                float dt = Math.Min(total, 0.025f);
                Tick(dt);
                Remaining = Math.Max(0, Remaining - dt);
                total -= dt;
            }
            if (reachesEnd) Remaining = 0;
        }

        private void Tick(float dt)
        {
            for (int t = 0; t < 2; t++)
            {
                Teams[t].FeedbackTime = Math.Max(0, Teams[t].FeedbackTime - dt);
                m_spawnTimes[t] -= dt;
                if (m_spawnTimes[t] <= 0)
                {
                    SpawnBowl(t);
                    m_spawnTimes[t] += Math.Max(0.3f, Settings.BowlInterval);
                }
            }
            foreach (Player p in Players)
            {
                p.Cooldown = Math.Max(0, p.Cooldown - dt);
                if (p.Cpu) CpuController.Step(this, p, dt);
            }
            MoveBowls(dt);
            IssunSystem.Step(this, dt);
            ProjectileSystem.Step(this, dt);
        }

        public Bowl SpawnBowl(int team)
        {
            Bowl bowl = new Bowl { Id = m_nextId++, Team = team,
                Position = new Point(-9.3f * Direction(team), LaneY(team)) };
            Bowls.Add(bowl);
            return bowl;
        }

        private void MoveBowls(float dt)
        {
            for (int i = Bowls.Count - 1; i >= 0; i--)
            {
                Bowl bowl = Bowls[i];
                bowl.Position.X += Direction(bowl.Team) * Settings.LaneSpeed * dt;
                if (bowl.Position.X * Direction(bowl.Team) >= 9.3f)
                {
                    Serve(bowl);
                    Bowls.RemoveAt(i);
                }
            }
        }

        public bool Serve(Bowl bowl)
        {
            Team team = Teams[bowl.Team];
            bool accepted = bowl.Ingredients == team.Order.Ingredients;
            if (accepted)
            {
                team.Score += team.Order.Points;
                team.Belly = Math.Min(Settings.BellyMaximum, team.Belly + team.Order.Points);
                team.Served++;
                team.Feedback = "YUM! +" + team.Order.Points;
                // 同じ注文が続かないように、残りの三つから選ぶ。
                team.OrderIndex = (team.OrderIndex + 1 + Random.Next(GameData.Recipes.Length - 1)) % GameData.Recipes.Length;
            }
            else { team.Rejected++; team.Feedback = "PEH!"; }
            team.FeedbackTime = 1.8f;
            return accepted;
        }

        public void MovePlayer(Player player, Point move, float dt)
        {
            if (Paused || Finished) return;
            if (move.Length > 1) move = move.Normalized;
            player.Position += move * Settings.PlayerSpeed * dt;
            player.Position.X = Clamp(player.Position.X, -8.5f, 8.5f);
            player.Position.Y = player.Team == 0 ? Clamp(player.Position.Y, 3f, 5.2f) : Clamp(player.Position.Y, -5.2f, -3f);
        }

        public bool Throw(Player player)
        {
            if (Paused || Finished || player.Cooldown > 0 || player.Aim.Length < 0.01f) return false;
            Projectiles.Add(new Projectile { Id = m_nextId++, Ingredient = player.Ingredient,
                Position = player.Position, Direction = player.Aim.Normalized, Team = player.Team });
            player.Cooldown = Settings.ThrowCooldown;
            return true;
        }

        public void Transfer(Issun issun)
        {
            issun.Team = 1 - issun.Team;
            issun.Position.Y = LaneY(issun.Team) + (issun.Good ? 0.35f : -0.35f);
            issun.TransferCooldown = 1.5f;
            issun.ActionTime = 0;
            issun.TargetId = -1;
        }
    }
}
