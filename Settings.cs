using System;

namespace HatlessEngine
{
    public static class Settings
    {
        /// <summary>
        /// Whether gamepad input processing is enabled in the Input class
        /// (checks state to up to 32 buttons and 8 axes on 8 gamepads every step)
        /// </summary>
        public static bool GamepadInputEnabled = true;

        /// <summary>
        /// Whether the game will exit when the last window is closed.
        /// If this is set to true before a window is created it will immediately exit!
        /// It will be set to true if the game is started with the default window (lazy bum option)
        /// </summary>
        public static bool ExitOnLastWindowClose = false;

        public static bool ExitOnConsoleClose = false;
    }
}
