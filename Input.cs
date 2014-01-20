using System;
using System.Drawing;
using System.Collections.Generic;
using OpenTK.Input;

namespace HatlessEngine
{
    public static class Input
    {
        private static List<Button> PreviousState = new List<Button>();
		internal static List<Button> CurrentState = new List<Button>();

        /// <summary>
        /// Buttons mapped to other buttons.
        /// Use this to manage maps.
        /// E.g. Add(Button.KB_UP, Button.KB_W) will simulate W and UP pressed when you press UP afterwards.
        /// </summary>
        public static Dictionary<Button, Button> ButtonMaps = new Dictionary<Button, Button>();
        
		#region Gamepad
        /// <summary>
        /// Value between 0 and 1 that the axes of a gamepad must have before triggering the Axes and the corresponding XBOX#_ buttons
        /// </summary>
		public static float GamePadDeadZone = 0.2f;

		public static bool[] ConnectedGamepads = new bool[8];
		private static JoystickCapabilities[] GamepadCapabilities = new JoystickCapabilities[8];
		private static List<Button>[] GamepadPreviousStates = new List<Button>[8];
		private static List<Button>[] GamepadCurrentStates = new List<Button>[8];
		private static float[,] GamepadAxisValues = new float[8,16];
		#endregion Gamepad

		public static PointF MousePosition { get; private set; }

		//to trigger multiple mousewheel presses
		private static int MouseWheelDelta = 0;

		/// <summary>
		/// Returns true when the specified button is pressed (one step only).
		/// Searches the gamepadstate if gamepadNumber is given. (1-8)
		/// </summary>
		public static bool IsPressed(Button button, byte gamepadNumber = 0)
        {
			if (gamepadNumber == 0)
				return (CurrentState.Contains(button) && !PreviousState.Contains(button));
			else
				return (GamepadCurrentStates[gamepadNumber - 1].Contains(button) && !GamepadPreviousStates[gamepadNumber - 1].Contains(button));
        }
		/// <summary>
		/// Returns true when the specified button is being held down (every step).
		/// Searches the gamepadstate if gamepadNumber is given. (1-8)
		/// </summary>
		public static bool IsDown(Button button, byte gamepadNumber = 0)
        {
			if (gamepadNumber == 0)
				return CurrentState.Contains(button);
			else
				return (GamepadCurrentStates[gamepadNumber - 1].Contains(button));
        }
		/// <summary>
		/// Returns true when the specified button is released (one step only).
		/// Searches the gamepadstate if gamepadNumber is given. (1-8)
		/// </summary>
		public static bool IsReleased(Button button, byte gamepadNumber = 0)
        {
			if (gamepadNumber == 0)
            	return (!CurrentState.Contains(button) && PreviousState.Contains(button));
			else
				return (!GamepadCurrentStates[gamepadNumber - 1].Contains(button) && GamepadPreviousStates[gamepadNumber - 1].Contains(button));
        }

		/// <summary>
		/// Get raw gamepad-axis value. This does not respect the deadzone, for that use the axis buttons.
		/// </summary>
		public static float GetGamePadAxis(byte gamepad, byte axis)
		{
			return GamepadAxisValues[gamepad - 1, axis - 1];
		}

		/// <summary>
		/// Adds currently pressed buttons to the state.
		/// </summary>
		internal static void UpdateState()
		{
			//gamepads
			for(byte i = 0; i < 8; i++)
			{
				JoystickState jState = Joystick.GetState(i);
				if (jState.IsConnected)
				{
					//it just connected, check capabilities
					if (!ConnectedGamepads[i])
					{
						//initialize currentstate
						GamepadCurrentStates[i] = new List<Button>();

						GamepadCapabilities[i] = Joystick.GetCapabilities(i);
						ConnectedGamepads[i] = true;
					}

					//push and clear state
					GamepadPreviousStates[i] = new List<Button>(GamepadCurrentStates[i]);
					GamepadCurrentStates[i].Clear();

					//update gamepad buttons
					for(byte j = 0; j < GamepadCapabilities[i].ButtonCount; j++)
					{
						if (jState.IsButtonDown((JoystickButton)j))
						{
							GamepadCurrentStates[i].Add((Button)(3001 + i * 32 + j));
						}
					}

					//update axes
					for(byte j = 0; j < GamepadCapabilities[i].AxisCount; j++)
					{
						float axisValue = jState.GetAxis((JoystickAxis)j);

						GamepadAxisValues[i,j] = axisValue;

						if (axisValue >= GamePadDeadZone)
							GamepadCurrentStates[i].Add((Button)(3033 + j * 2));
						if (axisValue <= -GamePadDeadZone)
							GamepadCurrentStates[i].Add((Button)(3034 + j * 2)); 
					}
				}
				else
				{
					if (ConnectedGamepads[i]) //just removed -> remove pressed buttons from list
					{
						GamepadPreviousStates[i].Clear();
						GamepadCurrentStates[i].Clear();
						
						ConnectedGamepads[i] = false;
					}
				}
			}

			//buttonmaps
			foreach (KeyValuePair<Button, Button> buttonPair in ButtonMaps)
			{
				if (CurrentState.Contains(buttonPair.Key) && !CurrentState.Contains(buttonPair.Value))
					CurrentState.Add(buttonPair.Value);
			}
		}

		/// <summary>
		/// Updates buttonstate at end of step to be in time for GameWindow keyboard and mouse events.
		/// Yes it's a reference.
		/// </summary>
		internal static void PushButtons()
		{
			//remove mousewheel from both states to prevent Released from occuring (pressed has already been detected)
			CurrentState.Remove(Button.MOUSE_WHEELDOWN);
			CurrentState.Remove(Button.MOUSE_WHEELUP);

			//actually push the state
			PreviousState = new List<Button>(CurrentState);

			//mousewheel buttons should be pressed multiple steps even though it might've actually been performed in one (delta > 1 or < -1)
			if (MouseWheelDelta >= 1)
			{
				CurrentState.Add(Button.MOUSE_WHEELUP);
				MouseWheelDelta--;
			}
			if (MouseWheelDelta <= -1)
			{
				CurrentState.Add(Button.MOUSE_WHEELDOWN);
				MouseWheelDelta++;
			}
		}

		/// <summary>
		/// Returns a string with all pressed buttons, for debugging purposes only.
		/// </summary>
		public static string GetStateInformation()
        {
			//mouse/keyboard info
			string str = "Pressed buttons: ";
			if (CurrentState.Count > 0)
			{
				foreach(Button button in CurrentState)
				{
					str += button.ToString() + ", ";
				}

				str = str.Substring(0, str.Length - 2);
			}
			str += "\n";

			//gamepad info
			for(byte i = 0; i < 8; i++)
			{
				if (ConnectedGamepads[i])
				{
					str += "Gamepad " + (i + 1).ToString() + ": ";

					if (GamepadCurrentStates[i].Count > 0)
					{
						foreach(Button button in GamepadCurrentStates[i])
						{
							str += button.ToString() + ", ";
						}

						str = str.Substring(0, str.Length - 2);
					}
					str += "\n";
				}
			}
				
            return str;
		}

		//OpenTK event integration
		internal static void MouseMove(object sender, MouseMoveEventArgs e)
		{
			PointF positionOnWindow = new PointF((float)e.X / Game.Window.Width, (float)e.Y / Game.Window.Height);

			//decide on which viewport the mouse currently is
			foreach (View view in Resources.Views)
			{
				if (view.Viewport.Contains(positionOnWindow))
				{
					//calculate position on virtual gamespace
					float x = view.Area.X + (positionOnWindow.X - view.Viewport.X) / view.Viewport.Width * view.Area.Width;
					float y = view.Area.Y + (positionOnWindow.Y - view.Viewport.Y) / view.Viewport.Height * view.Area.Height;

					MousePosition = new PointF(x, y);

					break;
				}
			}
		}
		internal static void MouseButtonChange(object sender, MouseButtonEventArgs e)
		{
			Button HEButton = (Button)(1001 + (int)e.Button);

			if (e.IsPressed)
			{
				if (!CurrentState.Contains(HEButton))
					CurrentState.Add(HEButton);
			}
			else
				CurrentState.Remove(HEButton);
		}
		internal static void MouseWheelChange(object sender, MouseWheelEventArgs e)
		{
			if (e.Delta >= 1)
			{
				if (!CurrentState.Contains(Button.MOUSE_WHEELUP))
					CurrentState.Add(Button.MOUSE_WHEELUP);

				//will simulate wheelups every remaining step
				MouseWheelDelta += e.Delta - 1;
			}
			else if (e.Delta <= -1)
			{
				if (!CurrentState.Contains(Button.MOUSE_WHEELDOWN))
					CurrentState.Add(Button.MOUSE_WHEELDOWN);
				MouseWheelDelta += e.Delta + 1;
			}
		}
		internal static void KeyboardKeyDown(object sender, KeyboardKeyEventArgs e)
		{
			Button HEButton = (Button)(2001 + (int)e.Key);
			if (!CurrentState.Contains(HEButton))
				CurrentState.Add(HEButton);
		}
		internal static void KeyboardKeyUp(object sender, KeyboardKeyEventArgs e)
		{
			Button HEButton = (Button)(2001 + (int)e.Key);
			CurrentState.Remove(HEButton);
		}

		//public static event EventHandler<GamepadConnectedEventArgs> GamepadConnected; //dangerous maybe because this class is static
    }
}
