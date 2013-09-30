using SFML.Window;
using System;
using System.Collections.Generic;

namespace HatlessEngine
{

    public static class Input
    {
        private static List<Button> PreviousState = new List<Button>();
        private static List<Button> CurrentState = new List<Button>();
        
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
                    Log.WriteLine("Input.GamePadDeadZone set to maximum of 1 (" + value.ToString() + ") given", ErrorLevel.WARNING);
                    gamePadDeadZone = 1;
                }
                else if (value < 0)
                {
                    Log.WriteLine("Input.GamePadDeadZone set to minimum of 0 (" + value.ToString() + ") given", ErrorLevel.WARNING);
                    gamePadDeadZone = 0;
                }
                else
                    gamePadDeadZone = value;
            }
        }

        public static int MouseXGlobal { get; private set; }
        public static int MouseYGlobal { get; private set; }
        public static float MouseX { get; private set; }
        public static float MouseY { get; private set; }

        public static bool IsPressed(Button button, bool global = false)
        {
            if (global || Game.FocusedWindow != null)
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
            if (global || Game.FocusedWindow != null)
                return CurrentState.Contains(button);
            else
                return false;
        }
        public static bool IsReleased(Button button, bool global = false)
        {
            if (global || Game.FocusedWindow != null)
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
            if (global || Game.FocusedWindow != null)
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

        /// <summary>
        /// Updates the input states for pending logic step. (can be called manually)
        /// </summary>
        public static void UpdateState()
        {
            PreviousState = new List<Button>(CurrentState);
            CurrentState.Clear();

            //Mouse
            Vector2i mouseVector = Mouse.GetPosition();
            MouseXGlobal = mouseVector.X;
            MouseYGlobal = mouseVector.Y;

            //check where mouse is on RenderPlane
            Window focusedWindow = Game.FocusedWindow;
            if (focusedWindow != null)
            {
                foreach (View view in focusedWindow.ActiveViews)
                {
                    //if mouse is within view
                    if (focusedWindow.MouseXOnWindow >= view.WindowX &&
                        focusedWindow.MouseXOnWindow <= view.WindowX + view.WindowHeight &&
                        focusedWindow.MouseYOnWindow >= view.WindowY &&
                        focusedWindow.MouseYOnWindow <= view.WindowY + view.WindowHeight)
                    {
                        MouseX = view.X + (focusedWindow.MouseXOnWindow - view.WindowX) / view.WindowWidth * view.Width;
                        MouseY = view.Y + (focusedWindow.MouseYOnWindow - view.WindowY) / view.WindowHeight * view.Height;
                    }
                }
            }
            else
            {
                MouseX = 0;
                MouseY = 0;
            }

            if (Mouse.IsButtonPressed(Mouse.Button.Left))
                CurrentState.Add(Button.MOUSE_LEFT);
            if (Mouse.IsButtonPressed(Mouse.Button.Right))
                CurrentState.Add(Button.MOUSE_RIGHT);
            if (Mouse.IsButtonPressed(Mouse.Button.Middle))
                CurrentState.Add(Button.MOUSE_MIDDLE);
            if (Mouse.IsButtonPressed(Mouse.Button.XButton1))
                CurrentState.Add(Button.MOUSE_X1);
            if (Mouse.IsButtonPressed(Mouse.Button.XButton2))
                CurrentState.Add(Button.MOUSE_X2);

            //Keyboard
            foreach (Keyboard.Key key in Enum.GetValues(typeof(Keyboard.Key)))
            {
                if (Keyboard.IsKeyPressed(key))
                    CurrentState.Add((Button)((int)key)+2001);
            }
            
            //Gamepads
            if (Settings.GamepadInputEnabled)
            {
                Joystick.Update();

                for (uint i = 0; i < 8; i++)
                {
                    if (Joystick.IsConnected(i))
                    {
                        //buttons
                        for (uint j = 0; j < 32; j++)
                        {
                            if (Joystick.IsButtonPressed(i, j))
                                CurrentState.Add((Button)(3001 + i * 32 + j));
                        }

                        //axes
                        for (uint k = 0; k < 8; k++)
                        {
                            GamepadAxes[i + 1, k + 1] = Joystick.GetAxisPosition(i, (Joystick.Axis)k) / 100;
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
                    }
                }
            }
        }

        public static string GetPressedButtons(bool global = false)
        {
            string str = "";

            if (global || Game.FocusedWindow != null)
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
