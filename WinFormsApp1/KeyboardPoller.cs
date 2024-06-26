using System.Runtime.InteropServices;

namespace ChungusEngine
{
    public static class KeyboardPoller
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetKeyboardState(byte[] lpKeyState);

        private static byte[] State = new byte[256];

        private static byte GetVirtualKeyCode(Keys key)
        {
            int value = (int)key;
            return (byte)(value & 0xFF);
        }

        public static bool IsPressed(this Keys key) => KeyIsPressed(key);

        public static bool KeyIsPressed(Keys key)
        {
            GetKeyboardState(State);
            var code = GetVirtualKeyCode(key);
            return (State[code] & 0x80) != 0;
        }

        public static void PollAndHandleKeyboardState()
        {
            if (Keys.W.IsPressed())
            {
                WindowProvider.Camera.HandleKeyboardInput(Keys.W);
            }

            if (Keys.A.IsPressed())
            {
                WindowProvider.Camera.HandleKeyboardInput(Keys.A);
            }

            if (Keys.S.IsPressed())
            {
                WindowProvider.Camera.HandleKeyboardInput(Keys.S);
            }

            if (Keys.D.IsPressed())
            {
                WindowProvider.Camera.HandleKeyboardInput(Keys.D);
            }

            if (Keys.Escape.IsPressed())
            {
                Environment.Exit(0);
            }
        }
    }
}
