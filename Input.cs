using System;
using System.Collections.Generic;

namespace HatlessEngine
{

    public static class Input
    {
        private static List<Button> PreviousState = new List<Button>();
        private static List<Button> CurrentState = new List<Button>();

        /// <summary>
        /// Buttons mapped to other buttons.
        /// Use this to manage maps.
        /// E.g. Add(Button.KB_UP, Button.KB_W) will simulate W and UP pressed when you press UP afterwards.
        /// </summary>
        public static Dictionary<Button, Button> ButtonMaps = new Dictionary<Button, Button>();
        
        /// <summary>
        /// For getting axis state without regarding the deadzone
        /// </summary>
        public static float[,] GamepadAxes = new float[9,9];

        private static float gamePadDeadZone = 0.2f;
        /// <summary>
        /// Value between 0 and 1 that the axes of a gamepad must have before triggering the Axes and the corresponding XBOX#_ buttons
        /// </summary>
        public static float GamePadDeadZone
        {
            get { return gamePadDeadZone; }
            set
            {
                if (value > 1)
                {
                    Log.Message("Input.GamePadDeadZone set to maximum of 1 (" + value.ToString() + ") given", ErrorLevel.WARNING);
                    gamePadDeadZone = 1;
                }
                else if (value < 0)
                {
                    Log.Message("Input.GamePadDeadZone set to minimum of 0 (" + value.ToString() + ") given", ErrorLevel.WARNING);
                    gamePadDeadZone = 0;
                }
                else
                    gamePadDeadZone = value;
            }
        }

        public static Position MouseGlobal { get; private set; }
        public static Position Mouse { get; private set; }

        public static bool IsPressed(Button button, bool global = false)
        {
            if (global || Resources.FocusedWindow != null)
                return (CurrentState.Contains(button) && !PreviousState.Contains(button));
            else
                return false;
        }
        /// <summary>
        /// Returns whether the given button is currently being pushed.
        /// </summary>
        /// <param name="button">Button</param>
        /// <param name="global">Whether to return even if none of the windows has focus.</param>
        /// <returns></returns>
        public static bool IsDown(Button button, bool global = false)
        {
            if (global || Resources.FocusedWindow != null)
                return CurrentState.Contains(button);
            else
                return false;
        }
        public static bool IsReleased(Button button, bool global = false)
        {
            if (global || Resources.FocusedWindow != null)
                return (!CurrentState.Contains(button) && PreviousState.Contains(button));
            else
                return false;
        }

        /// <summary>
        /// Returns given gamepad axis value with respect to deadzone and window focus
        /// </summary>
        /// <param name="gamePadNumber">1-8</param>
        /// <param name="axisNumber">1-8</param>
        /// <param name="global">Return even if no window is focused.</param>
        /// <returns>Axis value, 0 if not focused or not significant enough.</returns>
        public static float GetGamePadAxis(uint gamePadNumber, uint axisNumber, bool global = false)
        {
            if (global || Resources.FocusedWindow != null)
            {
                float axis = GamepadAxes[gamePadNumber, axisNumber];
                if (axis < -gamePadDeadZone || axis > gamePadDeadZone)
                    return axis;
                else
                    return 0;
            }
            else
                return 0;
        }

        public static Speed[] XboxLStick = new Speed[9];
        public static Speed[] XboxRStick = new Speed[9];

        /// <summary>
        /// Updates the input states for pending logic step. (can be called manually)
        /// </summary>
        public static void UpdateState()
        {
            PreviousState = new List<Button>(CurrentState);
            CurrentState.Clear();

            //Mouse
            SFML.Window.Vector2i mouseVector = SFML.Window.Mouse.GetPosition();
            MouseGlobal = new Position(mouseVector.X, mouseVector.Y);

            //check where mouse is on RenderPlane
            Mouse = new Position(0, 0);
            Window focusedWindow = Resources.FocusedWindow;
            if (focusedWindow != null)
            {
                foreach (View view in focusedWindow.ActiveViews)
                {
                    //if mouse is within view
                    if (focusedWindow.MouseXOnWindow >= view.Viewport.X &&
                        focusedWindow.MouseXOnWindow <= view.Viewport.X2 &&
                        focusedWindow.MouseYOnWindow >= view.Viewport.Y &&
                        focusedWindow.MouseYOnWindow <= view.Viewport.Y2)
                    {
                        float mouseX = view.Area.X + (focusedWindow.MouseXOnWindow - view.Viewport.X) / view.Viewport.Width * view.Area.Width;
                        float mouseY = view.Area.Y + (focusedWindow.MouseYOnWindow - view.Viewport.Y) / view.Viewport.Height * view.Area.Height;
                        Mouse = new Position(mouseX, mouseY);
                    }
                }
            }

            if (SFML.Window.Mouse.IsButtonPressed(SFML.Window.Mouse.Button.Left))
                CurrentState.Add(Button.MOUSE_LEFT);
            if (SFML.Window.Mouse.IsButtonPressed(SFML.Window.Mouse.Button.Right))
                CurrentState.Add(Button.MOUSE_RIGHT);
            if (SFML.Window.Mouse.IsButtonPressed(SFML.Window.Mouse.Button.Middle))
                CurrentState.Add(Button.MOUSE_MIDDLE);
            if (SFML.Window.Mouse.IsButtonPressed(SFML.Window.Mouse.Button.XButton1))
                CurrentState.Add(Button.MOUSE_X1);
            if (SFML.Window.Mouse.IsButtonPressed(SFML.Window.Mouse.Button.XButton2))
                CurrentState.Add(Button.MOUSE_X2);

            //Keyboard
            foreach (SFML.Window.Keyboard.Key key in Enum.GetValues(typeof(SFML.Window.Keyboard.Key)))
            {
                if (SFML.Window.Keyboard.IsKeyPressed(key))
                    CurrentState.Add((Button)((int)key)+2001);
            }
            
            //Gamepads
            if (Settings.GamepadInputEnabled)
            {
                SFML.Window.Joystick.Update();

                for (uint i = 0; i < 8; i++)
                {
                    if (SFML.Window.Joystick.IsConnected(i))
                    {
                        //buttons
                        for (uint j = 0; j < 32; j++)
                        {
                            if (SFML.Window.Joystick.IsButtonPressed(i, j))
                                CurrentState.Add((Button)(3001 + i * 32 + j));
                        }

                        //axes
                        for (uint k = 0; k < 8; k++)
                        {
                            GamepadAxes[i + 1, k + 1] = SFML.Window.Joystick.GetAxisPosition(i, (SFML.Window.Joystick.Axis)k) / 100;
                        }

                        //xbox axis to button conversion
                        //lstick
                        if (GamepadAxes[i + 1, 2] < -gamePadDeadZone)
                            CurrentState.Add((Button)(4001 + i * 14));
                        if (GamepadAxes[i + 1, 1] > gamePadDeadZone)
                            CurrentState.Add((Button)(4002 + i * 14));
                        if (GamepadAxes[i + 1, 2] > gamePadDeadZone)
                            CurrentState.Add((Button)(4003 + i * 14));
                        if (GamepadAxes[i + 1, 1] < -gamePadDeadZone)
                            CurrentState.Add((Button)(4004 + i * 14));
                        //rstick
                        if (GamepadAxes[i + 1, 4] < -gamePadDeadZone)
                            CurrentState.Add((Button)(4005 + i * 14));
                        if (GamepadAxes[i + 1, 5] > gamePadDeadZone)
                            CurrentState.Add((Button)(4006 + i * 14));
                        if (GamepadAxes[i + 1, 4] > gamePadDeadZone)
                            CurrentState.Add((Button)(4007 + i * 14));
                        if (GamepadAxes[i + 1, 5] < -gamePadDeadZone)
                            CurrentState.Add((Button)(4008 + i * 14));
                        //dpad
                        if (GamepadAxes[i + 1, 7] > 0.50f)
                            CurrentState.Add((Button)(4009 + i * 14));
                        if (GamepadAxes[i + 1, 8] > 0.50f)
                            CurrentState.Add((Button)(4010 + i * 14));
                        if (GamepadAxes[i + 1, 7] < -0.50f)
                            CurrentState.Add((Button)(4011 + i * 14));
                        if (GamepadAxes[i + 1, 8] < -0.50f)
                            CurrentState.Add((Button)(4012 + i * 14));
                        //triggers
                        if (GamepadAxes[i + 1, 3] > gamePadDeadZone)
                            CurrentState.Add((Button)(4013 + i * 14));
                        if (GamepadAxes[i + 1, 3] < -gamePadDeadZone)
                            CurrentState.Add((Button)(4014 + i * 14));

                        //update xbox stick interface lstick
                        if (GamepadAxes[i + 1, 1] < -gamePadDeadZone || GamepadAxes[i + 1, 1] > gamePadDeadZone)
                            XboxLStick[i + 1] = new Speed(GamepadAxes[i + 1, 1], XboxLStick[i + 1].Y);
                        else
                            XboxLStick[i + 1] = new Speed(0, XboxLStick[i + 1].Y);
                        if (GamepadAxes[i + 1, 2] < -gamePadDeadZone || GamepadAxes[i + 1, 2] > gamePadDeadZone)
                            XboxLStick[i + 1] = new Speed(XboxLStick[i + 1].X, GamepadAxes[i + 1, 2]);
                        else
                            XboxLStick[i + 1] = new Speed(XboxLStick[i + 1].X, 0);
                        //rstick
                        if (GamepadAxes[i + 1, 5] < -gamePadDeadZone || GamepadAxes[i + 1, 5] > gamePadDeadZone)
                            XboxRStick[i + 1] = new Speed(GamepadAxes[i + 1, 5], XboxRStick[i + 1].Y);
                        else
                            XboxRStick[i + 1] = new Speed(0, XboxRStick[i + 1].Y);
                        if (GamepadAxes[i + 1, 4] < -gamePadDeadZone || GamepadAxes[i + 1, 4] > gamePadDeadZone)
                            XboxRStick[i + 1] = new Speed(XboxRStick[i + 1].X, GamepadAxes[i + 1, 4]);
                        else
                            XboxRStick[i + 1] = new Speed(XboxRStick[i + 1].X, 0);
                    }
                }

                foreach (KeyValuePair<Button, Button> buttonPair in ButtonMaps)
                {
                    if (CurrentState.Contains(buttonPair.Key))
                        CurrentState.Add(buttonPair.Value);
                }
            }
        }

        public static string GetPressedButtons(bool global = false)
        {
            string str = "";

            if (global || Resources.FocusedWindow != null)
            {
                if (CurrentState.Count > 0)
                {
                    foreach (Button button in CurrentState)
                    {
                        str += button.ToString() + ", ";
                    }

                    str = str.Substring(0, str.Length - 2);
                }
            }
            return str;
        }
    }
}
