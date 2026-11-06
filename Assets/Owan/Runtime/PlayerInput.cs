using UnityEngine;
using UnityEngine.InputSystem;

namespace Owan
{
    public sealed class PlayerInput
    {
        private static readonly Key[][] s_keys = {
            new[] { Key.W, Key.S, Key.A, Key.D, Key.Q, Key.E, Key.Space },
            new[] { Key.UpArrow, Key.DownArrow, Key.LeftArrow, Key.RightArrow, Key.Comma, Key.Period, Key.RightCtrl },
            new[] { Key.I, Key.K, Key.J, Key.L, Key.Y, Key.O, Key.H },
            new[] { Key.Numpad8, Key.Numpad5, Key.Numpad4, Key.Numpad6, Key.Numpad7, Key.Numpad9, Key.Numpad0 }
        };

        public void Step(Match match, Camera camera, float dt)
        {
            for (int i = 0; i < 4; i++)
            {
                Player player = match.Players[i];
                if (player.Cpu) continue;
                KeyboardInput(match, player, camera, dt);
                if (i < Gamepad.all.Count) GamepadInput(match, player, Gamepad.all[i], dt);
            }
        }

        private void KeyboardInput(Match match, Player player, Camera camera, float dt)
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;
            Key[] keys = s_keys[player.Index];
            float x = (keyboard[keys[3]].isPressed ? 1 : 0) - (keyboard[keys[2]].isPressed ? 1 : 0);
            float y = (keyboard[keys[0]].isPressed ? 1 : 0) - (keyboard[keys[1]].isPressed ? 1 : 0);
            match.MovePlayer(player, new Point(x, y), dt);
            if (keyboard[keys[4]].wasPressedThisFrame) player.Selected = (player.Selected + 3) % 4;
            if (keyboard[keys[5]].wasPressedThisFrame) player.Selected = (player.Selected + 1) % 4;
            // 横移動で投げる向きを変えず、選んだお椀を狙いやすくする。
            if (player.Index == 0) ReadMouse(player, camera);
            if (keyboard[keys[6]].isPressed) match.Throw(player);
        }

        private void ReadMouse(Player player, Camera camera)
        {
            Mouse mouse = Mouse.current;
            if (mouse == null) return;
            Vector2 cursor = mouse.position.ReadValue();
            if (!camera.pixelRect.Contains(cursor)) return;
            Ray ray = camera.ScreenPointToRay(cursor);
            Plane plane = new Plane(Vector3.up, new Vector3(0, 0.65f, 0));
            if (plane.Raycast(ray, out float distance))
            {
                Vector3 point = ray.GetPoint(distance);
                player.Aim = new Point(point.x - player.Position.X, point.z - player.Position.Y).Normalized;
            }
            // 左クリックはUIと競合するため、投擲はSpaceに統一する。
        }

        private void GamepadInput(Match match, Player player, Gamepad pad, float dt)
        {
            Vector2 move = pad.leftStick.ReadValue();
            match.MovePlayer(player, new Point(move.x, move.y), dt);
            Vector2 aim = pad.rightStick.ReadValue();
            if (aim.sqrMagnitude > 0.15f) player.Aim = new Point(aim.x, aim.y).Normalized;
            if (pad.dpad.up.wasPressedThisFrame) player.Selected = 0;
            if (pad.dpad.right.wasPressedThisFrame) player.Selected = 1;
            if (pad.dpad.down.wasPressedThisFrame) player.Selected = 2;
            if (pad.dpad.left.wasPressedThisFrame) player.Selected = 3;
            if (pad.leftShoulder.wasPressedThisFrame) player.Selected = (player.Selected + 3) % 4;
            if (pad.rightShoulder.wasPressedThisFrame) player.Selected = (player.Selected + 1) % 4;
            if (pad.buttonSouth.isPressed) match.Throw(player);
        }
    }
}
