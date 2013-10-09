using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HatlessEngine
{
    /// <summary>
    /// Class using PhysicalObject to easily define buttons.
    /// Define BuiltinSprite to a Sprite with 3 frames in the constructor.
    /// Frame 1: nothing, 2: mouseover, 3: mousedown
    /// </summary>
    public class ButtonObject : PhysicalObject
    {
        private uint _State = 0;
        /// <summary>
        /// Current state of the button
        /// 0: nothing, 1: mouseover, 2: mousedown
        /// </summary>
        public uint State
        {
            get { return _State; }
            private set { _State = value; } 
        }

        internal override void AfterCreation()
        {
            BoundBoxRectangle.Width = BuiltinSprite.Size.Width;
            BoundBoxRectangle.Height = BuiltinSprite.Size.Height;
        }

        public sealed override void Step()
        {
            if (Input.MouseX >= BoundBoxRectangle.X && Input.MouseX <= BoundBoxRectangle.X2 && Input.MouseY >= BoundBoxRectangle.Y && Input.MouseY <= BoundBoxRectangle.Y2)
            {
                //mouseover
                if (State == 0)
                    State = 1;

                //pressed
                if (State == 1 && Input.IsPressed(Button.MOUSE_LEFT))
                    State = 2;

                //clicked
                if (State == 2 && Input.IsReleased(Button.MOUSE_LEFT))
                {
                    OnClick();
                    State = 1;
                }
            }
            else
                State = 0;

            BuiltinSpriteIndex = State;
        }

        public sealed override void Draw(float stepProgress) { }

        /// <summary>
        /// Triggers once when the mouse is released after being pressed on the button and never having left it.
        /// </summary>
        public virtual void OnClick() { }
    }
}
