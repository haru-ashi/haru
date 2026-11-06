using UnityEngine;
using UnityEngine.InputSystem;

namespace Owan
{
    public sealed class OwanGame : MonoBehaviour
    {
        public Tuning tuning = new Tuning();
        public Match Match { get; private set; }
        public bool Started { get; private set; }
        public bool Help;
        private BoardView m_view;
        private GameHud m_hud;
        private PlayerInput m_input;
        private bool m_pause;
        private int m_seed = 20;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Launch()
        {
            if (FindFirstObjectByType<OwanGame>() == null)
                new GameObject("Oh! WAN - Prototype").AddComponent<OwanGame>();
        }

        private void Awake()
        {
            Application.targetFrameRate = 60;
            // 添付のサンプルシーンは残し、プレイ中だけ重複カメラを止める。
            foreach (Camera camera in FindObjectsByType<Camera>(FindObjectsSortMode.None)) camera.enabled = false;
            foreach (Light light in FindObjectsByType<Light>(FindObjectsSortMode.None)) light.enabled = false;
            foreach (AudioListener listener in FindObjectsByType<AudioListener>(FindObjectsSortMode.None)) listener.enabled = false;
            TextAsset settings = Resources.Load<TextAsset>("OwanTuning");
            if (settings != null) JsonUtility.FromJsonOverwrite(settings.text, tuning);
            m_view = gameObject.AddComponent<BoardView>();
            m_hud = gameObject.AddComponent<GameHud>();
            m_input = new PlayerInput();
            Restart(false);
            m_view.Build();
            m_hud.Game = this;
        }

        public void Restart(bool start = true)
        {
            bool[] cpus = new bool[4];
            for (int i = 0; i < 4; i++) cpus[i] = Match == null ? i != 0 : Match.Players[i].Cpu;
            Match = new Match(tuning, ++m_seed);
            for (int i = 0; i < 4; i++) Match.Players[i].Cpu = cpus[i];
            Started = start;
            m_pause = false;
            Help = false;
        }

        public void StartMatch() { Started = true; }
        public void TogglePause() { if (Started && !Match.Finished) m_pause = !m_pause; }
        public bool Paused { get { return m_pause; } }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.escapeKey.wasPressedThisFrame) TogglePause();
                if (keyboard.f1Key.wasPressedThisFrame) Help = !Help;
                if (keyboard.enterKey.wasPressedThisFrame && (!Started || Match.Finished)) Restart();
            }
            Match.Paused = !Started || m_pause || Help;
            float dt = Mathf.Min(Time.deltaTime, 0.1f);
            if (!Match.Paused && !Match.Finished)
            {
                m_input.Step(Match, m_view.Camera, dt);
                Match.Step(dt);
            }
            m_view.Render(Match);
        }
    }
}
