using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace LeonsGameOfLife
{
    public class Input
    {
        public static KeyboardState Keyboard => Microsoft.Xna.Framework.Input.Keyboard.GetState();
        public static KeyboardState lastKeyboard;
        public static MouseState Mouse => Microsoft.Xna.Framework.Input.Mouse.GetState();
        public static MouseState lastMouse;

        public static void ToggleKeybind(Keys key, Action action)
        {
            if (!lastKeyboard.IsKeyDown(key) && Keyboard.IsKeyDown(key))
            {
                action.Invoke();
            }
        }
    }
}
