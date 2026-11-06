using System.Collections.Generic;
using UnityEngine;

namespace Owan
{
    public sealed class BoardView : MonoBehaviour
    {
        public Camera Camera { get; private set; }
        public static readonly Color Cream = new Color(0.97f, 0.94f, 0.84f);
        public static readonly Color Ink = new Color(0.12f, 0.19f, 0.2f);
        public static readonly Color[] TeamColors = { new Color(0.93f, 0.31f, 0.2f), new Color(0.07f, 0.52f, 0.59f) };
        public static readonly Color[] FoodColors = {
            new Color(0.34f, 0.16f, 0.1f), new Color(0.96f, 0.94f, 0.79f),
            new Color(0.77f, 0.44f, 0.12f), new Color(1f, 0.78f, 0.14f),
            new Color(1f, 0.4f, 0.08f), new Color(0.08f, 0.14f, 0.09f),
            new Color(0.8f, 0.12f, 0.18f), new Color(1f, 0.53f, 0.48f)
        };
        private readonly Dictionary<string, Material> m_materials = new Dictionary<string, Material>();
        private readonly Dictionary<string, Transform> m_actors = new Dictionary<string, Transform>();
        private readonly HashSet<string> m_visible = new HashSet<string>();
        private readonly List<string> m_remove = new List<string>();
        private readonly Transform[] m_players = new Transform[4];
        private Transform m_dynamicRoot;

        public void Build()
        {
            GameObject cameraObject = new GameObject("Shared overhead camera");
            cameraObject.transform.SetParent(transform);
            Camera = cameraObject.AddComponent<Camera>();
            Camera.transform.position = new Vector3(0, 18, -13);
            Camera.transform.LookAt(new Vector3(0, 0, 0));
            Camera.orthographic = true;
            Camera.orthographicSize = 7.6f;
            Camera.clearFlags = CameraClearFlags.SolidColor;
            Camera.backgroundColor = Cream;
            cameraObject.AddComponent<AudioListener>();
            GameObject lamp = new GameObject("Kitchen light");
            lamp.transform.SetParent(transform);
            Light light = lamp.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            lamp.transform.rotation = Quaternion.Euler(50, -30, 0);
            RenderSettings.ambientLight = new Color(0.7f, 0.73f, 0.72f);
            m_dynamicRoot = new GameObject("Moving pieces").transform;
            m_dynamicRoot.SetParent(transform);
            BuildBoard();
            for (int i = 0; i < 4; i++) BuildPlayer(i);
        }

        private void BuildBoard()
        {
            Shape("Table", PrimitiveType.Cube, new Vector3(0, -0.5f, 0), new Vector3(23, 0.7f, 12.4f), new Color(0.34f, 0.24f, 0.17f), transform);
            Shape("Counter", PrimitiveType.Cube, new Vector3(0, -0.1f, 0), new Vector3(22.6f, 0.15f, 12), new Color(0.87f, 0.79f, 0.62f), transform);
            for (int t = 0; t < 2; t++) BuildLane(t);
            Shape("Lane divider", PrimitiveType.Cube, new Vector3(0, 0.25f, 0), new Vector3(21, 0.4f, 0.12f), Ink, transform);
            for (int x = -10; x <= 10; x += 2)
                Shape("Counter inlay", PrimitiveType.Cube, new Vector3(x, 0.02f, 0), new Vector3(0.03f, 0.02f, 11.5f), new Color(0.72f, 0.61f, 0.44f), transform);
        }

        private void BuildLane(int team)
        {
            float z = Match.LaneY(team);
            Shape("Belt", PrimitiveType.Cube, new Vector3(0, 0.12f, z), new Vector3(20, 0.2f, 1.85f), Ink, transform);
            Shape("Team work area", PrimitiveType.Cube, new Vector3(0, 0.05f, team == 0 ? 4.2f : -4.2f), new Vector3(19.5f, 0.05f, 2.6f), Color.Lerp(TeamColors[team], Cream, 0.75f), transform);
            for (int i = -9; i <= 9; i++)
            {
                Shape("Belt stripe", PrimitiveType.Cube, new Vector3(i, 0.23f, z), new Vector3(0.025f, 0.025f, 1.65f), new Color(0.3f, 0.39f, 0.4f), transform);
            }
            for (int i = -8; i <= 8; i += 4)
                Label(Match.Direction(team) > 0 ? ">>" : "<<", new Vector3(i, 0.3f, z - 0.7f), 0.22f, TeamColors[team], transform);
            BuildMouth(team);
            for (int role = 0; role < 2; role++)
                for (int f = 0; f < 4; f++)
                {
                    Food food = GameData.Inventory[role][f];
                    float x = role == 0 ? -7.2f + f * 1.1f : 3.7f + f * 1.1f;
                    Shape("Ingredient display", PrimitiveType.Cylinder, new Vector3(x, 0.15f, team == 0 ? 5.6f : -5.6f), new Vector3(0.7f, 0.12f, 0.7f), FoodColors[(int)food], transform);
                }
        }

        private void BuildMouth(int team)
        {
            float x = 10.15f * Match.Direction(team);
            Transform mouth = new GameObject("Team " + (team + 1) + " mouth").transform;
            mouth.SetParent(transform);
            mouth.position = new Vector3(x, 0.65f, Match.LaneY(team));
            Shape("Face", PrimitiveType.Sphere, new Vector3(0, 0.4f, 0), new Vector3(1.65f, 1.8f, 1.6f), TeamColors[team], mouth);
            Shape("Open mouth", PrimitiveType.Sphere, new Vector3(0, 1.03f, -0.5f), new Vector3(0.85f, 0.6f, 0.7f), Ink, mouth);
            for (int side = -1; side <= 1; side += 2)
                Shape("Eye", PrimitiveType.Sphere, new Vector3(side * 0.36f, 1.16f, 0.12f), new Vector3(0.17f, 0.12f, 0.24f), Color.white, mouth);
        }

        private void BuildPlayer(int index)
        {
            Transform root = new GameObject("P" + (index + 1)).transform;
            root.SetParent(transform);
            m_players[index] = root;
            Color color = TeamColors[index / 2];
            Shape("Chef body", PrimitiveType.Capsule, new Vector3(0, 0.6f, 0), new Vector3(0.55f, 0.48f, 0.55f), color, root);
            Shape("Apron", PrimitiveType.Cube, new Vector3(0, 0.48f, -0.25f), new Vector3(0.35f, 0.5f, 0.07f), Cream, root);
            Shape("Chef head", PrimitiveType.Sphere, new Vector3(0, 1.12f, 0), new Vector3(0.48f, 0.48f, 0.48f), new Color(0.94f, 0.71f, 0.49f), root);
            Shape("Chef hat", PrimitiveType.Cylinder, new Vector3(0, 1.45f, 0), new Vector3(0.57f, 0.17f, 0.57f), Color.white, root);
            Label("P" + (index + 1), new Vector3(0, 1.95f, 0), 0.22f, Ink, root);
            Transform aim = Shape("Aim", PrimitiveType.Cube, new Vector3(0, 0.1f, 0), new Vector3(0.06f, 0.035f, 1.3f), color, root);
            aim.name = "Aim";
        }

        public void Render(Match match)
        {
            if (Camera == null) return;
            Camera.rect = new Rect(0, 0.19f, 1, 0.64f);
            Camera.orthographicSize = Mathf.Max(7.5f, 11.8f / Camera.aspect);
            m_visible.Clear();
            for (int i = 0; i < 4; i++) RenderPlayer(match.Players[i]);
            foreach (Bowl bowl in match.Bowls) RenderBowl(bowl);
            foreach (Issun issun in match.Issuns) RenderIssun(issun);
            foreach (Projectile shot in match.Projectiles) RenderShot(shot);
            m_remove.Clear();
            foreach (var pair in m_actors) if (!m_visible.Contains(pair.Key)) m_remove.Add(pair.Key);
            foreach (string key in m_remove) { Destroy(m_actors[key].gameObject); m_actors.Remove(key); }
        }

        private void RenderPlayer(Player player)
        {
            Transform root = m_players[player.Index];
            root.position = Position(player.Position, 0.2f);
            Transform aim = root.Find("Aim");
            Vector3 direction = new Vector3(player.Aim.X, 0, player.Aim.Y).normalized;
            aim.localPosition = direction * 0.9f + Vector3.up * 0.04f;
            if (direction.sqrMagnitude > 0) aim.rotation = Quaternion.LookRotation(direction);
        }

        private void RenderBowl(Bowl bowl)
        {
            Transform root = Actor("b" + bowl.Id, out bool fresh);
            if (fresh)
            {
                Shape("Bowl", PrimitiveType.Sphere, Vector3.zero, new Vector3(1.05f, 0.5f, 1.05f), TeamColors[bowl.Team], root);
                Shape("Rice", PrimitiveType.Cylinder, new Vector3(0, 0.2f, 0), new Vector3(0.87f, 0.04f, 0.87f), Cream, root);
                for (int i = 0; i < 8; i++)
                {
                    float angle = i * Mathf.PI * 0.25f;
                    Shape("Food" + i, PrimitiveType.Sphere, new Vector3(Mathf.Cos(angle) * 0.26f, 0.3f, Mathf.Sin(angle) * 0.26f), new Vector3(0.23f, 0.15f, 0.23f), FoodColors[i], root);
                }
            }
            root.position = Position(bowl.Position, 0.52f);
            for (int i = 0; i < 8; i++) root.Find("Food" + i).gameObject.SetActive((bowl.Ingredients & (1 << i)) != 0);
        }

        private void RenderIssun(Issun issun)
        {
            Transform root = Actor("i" + issun.Id, out bool fresh);
            if (fresh)
            {
                Color color = issun.Good ? new Color(0.3f, 0.75f, 0.36f) : new Color(0.57f, 0.29f, 0.75f);
                Shape("Issun", PrimitiveType.Capsule, new Vector3(0, 0.18f, 0), new Vector3(0.34f, 0.29f, 0.34f), color, root);
                Label(issun.Good ? "+" : "!", new Vector3(0, 0.9f, 0), 0.3f, color, root);
            }
            root.position = Position(issun.Position, 0.46f);
        }

        private void RenderShot(Projectile shot)
        {
            Transform root = Actor("s" + shot.Id, out bool fresh);
            if (fresh) Shape("Food", PrimitiveType.Cube, Vector3.zero, Vector3.one * 0.24f, FoodColors[(int)shot.Ingredient], root);
            root.position = Position(shot.Position, 0.85f + Mathf.Sin(shot.Travelled * 0.65f) * 0.16f);
            root.Rotate(180 * Time.deltaTime, 100 * Time.deltaTime, 0);
        }

        private Transform Actor(string key, out bool fresh)
        {
            m_visible.Add(key);
            fresh = !m_actors.TryGetValue(key, out Transform root);
            if (!fresh) return root;
            root = new GameObject(key).transform;
            root.SetParent(m_dynamicRoot);
            m_actors.Add(key, root);
            return root;
        }

        private Transform Shape(string name, PrimitiveType type, Vector3 position, Vector3 scale, Color color, Transform parent)
        {
            GameObject obj = GameObject.CreatePrimitive(type);
            obj.name = name;
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position;
            obj.transform.localScale = scale;
            Destroy(obj.GetComponent<Collider>());
            string key = ColorUtility.ToHtmlStringRGBA(color);
            if (!m_materials.TryGetValue(key, out Material material))
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                material = new Material(shader) { color = color };
                m_materials.Add(key, material);
            }
            obj.GetComponent<Renderer>().sharedMaterial = material;
            return obj.transform;
        }

        private void Label(string text, Vector3 position, float size, Color color, Transform parent)
        {
            GameObject obj = new GameObject(text);
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position;
            obj.transform.rotation = Camera.transform.rotation;
            TextMesh mesh = obj.AddComponent<TextMesh>();
            mesh.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            obj.GetComponent<MeshRenderer>().sharedMaterial = mesh.font.material;
            mesh.text = text;
            mesh.characterSize = size;
            mesh.fontSize = 48;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.color = color;
        }

        private static Vector3 Position(Point point, float height) { return new Vector3(point.X, height, point.Y); }

        private void OnDestroy()
        {
            foreach (Material material in m_materials.Values) Destroy(material);
        }
    }
}
