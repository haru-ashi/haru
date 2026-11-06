using System;
using Owan;

internal static class Program
{
    private static int s_passed;
    private static void Main()
    {
        Test("every recipe needs both chefs", RecipeSplit);
        Test("exact order scores and changes order", ExactOrder);
        Test("missing/extra/empty bowls reject without changing order", Rejection);
        Test("duplicate ingredients are a set", Duplicate);
        Test("bowls travel in opposite directions and reach mouths", Lanes);
        Test("food hits first bowl and can reach opposite lane", Collision);
        Test("food hits Issun before bowl and transfers it", HitIssun);
        Test("good Issun cleans only extra food", Clean);
        Test("bad Issun eats one food at a time", Steal);
        Test("good Issun flees nearby bad Issun", Fear);
        Test("player stays in team area and respects cooldown", PlayerLimits);
        Test("pause and timer end freeze match", Timer);
        Test("CPU partners finish dishes in a full 120 second match", FullMatch);
        Console.WriteLine("PASS: " + s_passed + " tests");
    }
    private static void Test(string name, Action test)
    {
        test(); s_passed++; Console.WriteLine("PASS " + name);
    }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
    private static Match Empty(Tuning settings = null)
    {
        Match match = new Match(settings ?? new Tuning(), 42);
        foreach (Player p in match.Players) p.Cpu = false;
        match.Issuns.Clear();
        return match;
    }
    private static void RecipeSplit()
    {
        foreach (Recipe recipe in GameData.Recipes)
            foreach (Food[] foods in GameData.Inventory)
            {
                int mask = 0;
                foreach (Food food in foods) mask |= GameData.Bit(food);
                Check((mask & recipe.Ingredients) != 0 && (recipe.Ingredients & ~mask) != 0, recipe.Name);
            }
    }
    private static void ExactOrder()
    {
        Match m = Empty();
        int order = m.Teams[0].OrderIndex;
        Bowl b = new Bowl { Team = 0, Ingredients = m.Teams[0].Order.Ingredients };
        Check(m.Serve(b), "exact order rejected");
        Check(m.Teams[0].Score == 2 && m.Teams[0].Belly == 2 && m.Teams[0].Served == 1, "score wrong");
        Check(m.Teams[0].OrderIndex != order, "order unchanged");
    }
    private static void Rejection()
    {
        Match m = Empty();
        int order = m.Teams[0].OrderIndex;
        foreach (int mask in new[] { 0, GameData.Bit(Food.Beef), m.Teams[0].Order.Ingredients | GameData.Bit(Food.Salmon) })
            Check(!m.Serve(new Bowl { Team = 0, Ingredients = mask }), "invalid dish accepted");
        Check(m.Teams[0].Score == 0 && m.Teams[0].OrderIndex == order && m.Teams[0].Rejected == 3, "rejection mutated order");
    }
    private static void Duplicate()
    {
        Match m = Empty();
        Bowl b = m.SpawnBowl(0); b.Position = new Point(0, 1.25f);
        for (int n = 0; n < 2; n++)
        {
            m.Projectiles.Add(new Projectile { Ingredient = Food.Beef, Position = new Point(0, 3), Direction = new Point(0, -1) });
            ProjectileSystem.Step(m, 0.2f);
        }
        Check(GameData.Count(b.Ingredients) == 1, "duplicate counted twice");
    }
    private static void Lanes()
    {
        Match m = Empty(new Tuning { LaneSpeed = 5, BowlInterval = 100 });
        m.Step(0.01f);
        Bowl a = m.Bowls[0], b = m.Bowls[1];
        float ax = a.Position.X, bx = b.Position.X;
        m.Step(0.2f);
        Check(a.Position.X > ax && b.Position.X < bx, "direction wrong");
        m.Step(5);
        Check(m.Teams[0].Rejected == 1 && m.Teams[1].Rejected == 1 && m.Bowls.Count == 0, "mouth not reached");
    }
    private static void Collision()
    {
        Match m = Empty();
        Bowl first = m.SpawnBowl(0), second = m.SpawnBowl(1);
        first.Position = new Point(0, 1.25f); second.Position = new Point(0, -1.25f);
        m.Projectiles.Add(new Projectile { Ingredient = Food.Tuna, Position = new Point(0, 4), Direction = new Point(0, -1) });
        ProjectileSystem.Step(m, 0.8f);
        Check(first.Ingredients == GameData.Bit(Food.Tuna) && second.Ingredients == 0, "first collision failed");
        first.Position.X = 3;
        m.Projectiles.Add(new Projectile { Ingredient = Food.Salmon, Position = new Point(0, 4), Direction = new Point(0, -1) });
        ProjectileSystem.Step(m, 0.8f);
        Check(second.Ingredients == GameData.Bit(Food.Salmon), "opponent lane unreachable");
        m.Projectiles.Add(new Projectile { Position = new Point(8, 4), Direction = new Point(0, -1) });
        ProjectileSystem.Step(m, 2);
        Check(m.Projectiles.Count == 0, "range did not expire");
    }
    private static void HitIssun()
    {
        Match m = Empty();
        Issun npc = new Issun { Team = 0, Good = false, Position = new Point(0, 2) };
        m.Issuns.Add(npc);
        Bowl b = m.SpawnBowl(0); b.Position = new Point(0, 1.25f);
        m.Projectiles.Add(new Projectile { Ingredient = Food.Egg, Position = new Point(0, 4), Direction = new Point(0, -1) });
        ProjectileSystem.Step(m, 0.6f);
        Check(npc.Team == 1 && b.Ingredients == 0 && m.Projectiles.Count == 0, "Issun collision failed");
    }
    private static void Clean()
    {
        Match m = Empty(new Tuning { GoodCleanSeconds = 0.1f, LaneSpeed = 0 });
        Bowl b = m.SpawnBowl(0); b.Position = new Point(0, 1.25f);
        b.Ingredients = m.Teams[0].Order.Ingredients | GameData.Bit(Food.Egg);
        m.Issuns.Add(new Issun { Good = true, Team = 0, Position = b.Position });
        IssunSystem.Step(m, 0.2f);
        Check(b.Ingredients == m.Teams[0].Order.Ingredients, "clean removed required food");
        m.Teams[0].OrderIndex = 1;
        IssunSystem.Step(m, 0.2f);
        Check(GameData.Count(b.Ingredients) == 1, "clean ignored new order");
    }
    private static void Steal()
    {
        Match m = Empty(new Tuning { BadEatSeconds = 0.1f, LaneSpeed = 0 });
        Bowl b = m.SpawnBowl(0); b.Position = new Point(0, 1.25f); b.Ingredients = m.Teams[0].Order.Ingredients;
        m.Issuns.Add(new Issun { Good = false, Team = 0, Position = b.Position });
        IssunSystem.Step(m, 0.2f);
        Check(GameData.Count(b.Ingredients) == 1, "bad ate wrong count");
    }
    private static void Fear()
    {
        Match m = Empty();
        Issun good = new Issun { Good = true, Team = 0, Position = new Point(0, 1.25f) };
        m.Issuns.Add(good); m.Issuns.Add(new Issun { Good = false, Team = 0, Position = new Point(0.1f, 1.25f) });
        IssunSystem.Step(m, 0.02f);
        Check(good.Team == 1 && good.TransferCooldown > 0, "good did not flee");
    }
    private static void PlayerLimits()
    {
        Match m = Empty(); Player p = m.Players[0];
        m.MovePlayer(p, new Point(1, -1), 100);
        Check(p.Position.X <= 8.5f && p.Position.Y >= 3, "escaped area");
        Check(m.Throw(p) && !m.Throw(p), "cooldown not enforced");
    }
    private static void Timer()
    {
        Match m = Empty(new Tuning { MatchSeconds = 1 });
        m.Paused = true; m.Step(1); Check(m.Remaining == 1 && m.Bowls.Count == 0, "pause advanced");
        m.Paused = false; m.Step(2); Check(m.Finished && m.Remaining == 0, "timer wrong");
        int count = m.Bowls.Count;
        Check(!m.Throw(m.Players[0]), "can throw after end");
        m.Step(1); Check(m.Bowls.Count == count, "end advanced");
        Check(m.Winner == -1, "tie wrong");
    }
    private static void FullMatch()
    {
        Match m = new Match(new Tuning(), 42);
        foreach (Player p in m.Players) p.Cpu = true;
        for (int i = 0; i < 7501; i++) m.Step(0.016f);
        Console.WriteLine("  full match scores: " + m.Teams[0].Score + " / " + m.Teams[1].Score);
        Check(m.Finished, "full match never ended");
        Check(m.Teams[0].Served > 0 && m.Teams[1].Served > 0, "CPU failed to serve");
        Check(m.Bowls.Count < 30 && m.Projectiles.Count < 100, "objects leak");
    }
}
