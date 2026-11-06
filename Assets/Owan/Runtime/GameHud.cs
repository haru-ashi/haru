using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Owan
{
    public sealed class GameHud : MonoBehaviour
    {
        public OwanGame Game;
        private GUIStyle m_text;
        private GUIStyle m_title;
        private GUIStyle m_small;
        private GUIStyle m_button;
        private GUIStyle m_center;
        private const float kWidth = 1440f;
        private const float kHeight = 900f;

        private void InitializeStyles()
        {
            if (m_text != null) return;
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            m_text = new GUIStyle(GUI.skin.label) { font = font, fontSize = 21, fontStyle = FontStyle.Bold };
            m_text.normal.textColor = BoardView.Ink;
            m_title = new GUIStyle(m_text) { fontSize = 34 };
            m_small = new GUIStyle(m_text) { fontSize = 15, fontStyle = FontStyle.Normal };
            m_center = new GUIStyle(m_title) { alignment = TextAnchor.MiddleCenter };
            m_button = new GUIStyle(GUI.skin.button) { font = font, fontSize = 17, fontStyle = FontStyle.Bold };
            m_button.normal.textColor = BoardView.Ink;
        }

        private void OnGUI()
        {
            if (Game == null || Game.Match == null) return;
            InitializeStyles();
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width / kWidth, Screen.height / kHeight, 1));
            DrawHeader();
            DrawFooter();
            DrawNearbyIngredients();
            if (!Game.Started || Game.Match.Finished || Game.Paused || Game.Help) DrawOverlay();
        }

        private void DrawHeader()
        {
            Panel(new Rect(0, 0, kWidth, 151), BoardView.Cream);
            Text(new Rect(28, 10, 250, 50), "OH! WAN", m_title);
            Text(new Rect(30, 56, 250, 25), "TOSS. TEAM UP. TASTE VICTORY.", m_small);
            int seconds = Mathf.CeilToInt(Game.Match.Remaining);
            Text(new Rect(605, 5, 230, 50), (seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00"), m_center);
            Text(new Rect(595, 52, 250, 25), "2 vs 2  /  KITCHEN BATTLE", Centered(m_small));
            if (Button(new Rect(1115, 18, 130, 35), "CONTROLS / F1")) Game.Help = !Game.Help;
            if (Button(new Rect(1260, 18, 150, 35), Game.Paused ? "RESUME / ESC" : "PAUSE / ESC")) Game.TogglePause();
            DrawOrder(0, 28);
            DrawOrder(1, 858);
        }

        private void DrawOrder(int teamIndex, float x)
        {
            Team team = Game.Match.Teams[teamIndex];
            Color color = BoardView.TeamColors[teamIndex];
            Panel(new Rect(x, 88, 554, 55), Color.Lerp(color, BoardView.Cream, 0.8f));
            Text(new Rect(x + 10, 90, 138, 28), "TEAM " + (teamIndex + 1) + "   " + team.Score.ToString("00"), m_text, color);
            Text(new Rect(x + 150, 90, 210, 26), team.Order.Name + "  +" + team.Order.Points, m_text);
            if (team.FeedbackTime > 0) Text(new Rect(x + 370, 90, 174, 26), team.Feedback, m_text, color);
            float foodX = x + 150;
            for (int i = 0; i < 8; i++)
                if ((team.Order.Ingredients & (1 << i)) != 0)
                {
                    FoodChip(new Rect(foodX, 118, 86, 21), (Food)i, false);
                    foodX += 89;
                }
            Panel(new Rect(x + 10, 122, 125, 10), new Color(0.75f, 0.73f, 0.66f));
            Panel(new Rect(x + 10, 122, 125 * Mathf.Clamp01((float)team.Belly / Math.Max(1, Game.tuning.BellyMaximum)), 10), color);
        }

        private void DrawNearbyIngredients()
        {
            Camera camera = Game.GetComponent<BoardView>().Camera;
            if (camera == null) return;
            foreach (Player player in Game.Match.Players)
            {
                Vector3 screen = camera.WorldToScreenPoint(new Vector3(player.Position.X, 2.2f, player.Position.Y));
                float x = screen.x / Screen.width * kWidth - 110;
                float y = Mathf.Clamp((1 - screen.y / Screen.height) * kHeight, 158, 698);
                Food[] foods = GameData.Inventory[player.Index % 2];
                for (int f = 0; f < 4; f++)
                    FoodChip(new Rect(x + f * 56, y, 53, 19), foods[f], f == player.Selected);
            }
        }

        private void DrawFooter()
        {
            Panel(new Rect(0, 730, kWidth, 170), BoardView.Cream);
            for (int i = 0; i < 4; i++) DrawPlayer(i);
            Text(new Rect(25, 861, 1080, 30), "SPACE: THROW  |  Mouse: P1 aim  |  Green + cleans extra food  |  Purple ! steals food  |  Hit an Issun to switch its lane", m_small);
            Text(new Rect(1110, 861, 310, 30), "GAMEPADS: " + Gamepad.all.Count + " / 4", m_small);
        }

        private void DrawPlayer(int index)
        {
            Player player = Game.Match.Players[index];
            float x = 22 + index * 356;
            Color color = BoardView.TeamColors[player.Team];
            Panel(new Rect(x, 738, 336, 115), Color.Lerp(color, BoardView.Cream, 0.88f));
            Text(new Rect(x + 10, 742, 120, 29), "P" + (index + 1) + " / T" + (player.Team + 1), m_text, color);
            if (Button(new Rect(x + 203, 743, 120, 27), player.Cpu ? "CPU > HUMAN" : "HUMAN > CPU")) player.Cpu = !player.Cpu;
            Food[] foods = GameData.Inventory[index % 2];
            for (int f = 0; f < 4; f++)
            {
                Rect chip = new Rect(x + 9 + f * 80, 779, 77, 27);
                if (GUI.Button(chip, GUIContent.none, GUIStyle.none)) player.Selected = f;
                FoodChip(chip, foods[f], player.Selected == f);
            }
            string[] controls = { "WASD move / Q,E food / SPACE throw", "Arrows move / , . food / R-CTRL throw", "IJKL move / Y,O food / H throw", "NUM 8456 move / 7,9 food / 0 throw" };
            Text(new Rect(x + 10, 817, 325, 30), player.Cpu ? "CPU partner - click above to take over" : controls[index], m_small);
        }

        private void DrawOverlay()
        {
            Panel(new Rect(0, 151, kWidth, 579), new Color(0.07f, 0.12f, 0.13f, 0.88f));
            Panel(new Rect(345, 186, 750, 507), BoardView.Cream);
            if (Game.Match.Finished) { DrawResults(); return; }
            Text(new Rect(375, 205, 690, 55), Game.Help ? "HOW TO COOK" : Game.Paused ? "TAKE A BREATHER" : "WELCOME TO OH! WAN", m_center);
            Text(new Rect(394, 277, 660, 34), "Fill moving bowls. Match the order. Feed your team.", m_text);
            string[] lines = {
                "01  Each chef has 4 foods. Every order needs both chefs.",
                "02  Pick food with Q / E. Aim with the mouse. Hold SPACE.",
                "03  Extra or missing food? PEH! Only exact orders score.",
                "04  Shoot through gaps to spoil the other team's bowls.",
                "05  Green + fixes bowls. Purple ! eats food. Hit to transfer.",
                "06  P1 is human. P2-P4 are CPU. Toggle any slot below.",
                "PAD  Left stick: move / Right stick: aim / South: throw",
                "         D-pad: 4 foods / Shoulders: cycle / F1: controls"
            };
            for (int i = 0; i < lines.Length; i++) Text(new Rect(395, 323 + i * 31, 655, 29), lines[i], m_small);
            string button = Game.Help ? "BACK TO GAME" : Game.Paused ? "RESUME" : "LET'S COOK!  /  ENTER";
            if (Button(new Rect(495, 609, 450, 51), button))
            {
                if (Game.Help) Game.Help = false;
                else if (Game.Paused) Game.TogglePause();
                else Game.StartMatch();
            }
        }

        private void DrawResults()
        {
            Match match = Game.Match;
            string result = match.Winner < 0 ? "IT'S A DRAW!" : "TEAM " + (match.Winner + 1) + " WINS!";
            Text(new Rect(375, 222, 690, 65), result, m_center);
            Text(new Rect(400, 299, 640, 35), "THE KITCHEN IS CLOSED", Centered(m_text));
            for (int t = 0; t < 2; t++)
            {
                Team team = match.Teams[t];
                float x = 407 + t * 335;
                Text(new Rect(x, 365, 300, 45), "TEAM " + (t + 1) + "    " + team.Score + " PTS", m_title, BoardView.TeamColors[t]);
                Text(new Rect(x, 426, 300, 35), "SERVED     " + team.Served, m_text);
                Text(new Rect(x, 465, 300, 35), "REJECTED   " + team.Rejected, m_text);
            }
            if (Button(new Rect(495, 586, 450, 60), "ONE MORE ROUND  /  ENTER")) Game.Restart();
        }

        public static void Panel(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private void FoodChip(Rect rect, Food food, bool selected)
        {
            if (selected) Panel(new Rect(rect.x - 2, rect.y - 2, rect.width + 4, rect.height + 4), BoardView.Ink);
            Panel(rect, BoardView.FoodColors[(int)food]);
            Color ink = food == Food.Beef || food == Food.Nori || food == Food.Tuna ? Color.white : BoardView.Ink;
            Text(rect, food.ToString().ToUpperInvariant(), Centered(m_small), ink);
        }

        private static GUIStyle Centered(GUIStyle source) { return new GUIStyle(source) { alignment = TextAnchor.MiddleCenter }; }
        private void Text(Rect rect, string text, GUIStyle style, Color? color = null)
        {
            Color previous = style.normal.textColor;
            if (color.HasValue) style.normal.textColor = color.Value;
            GUI.Label(rect, text, style);
            style.normal.textColor = previous;
        }
        private bool Button(Rect rect, string text) { return GUI.Button(rect, text, m_button); }
    }
}
