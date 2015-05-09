#region File Header
/***********************************************************************
This is free and unencumbered software released into the public domain.

Anyone is free to copy, modify, publish, use, compile, sell, or
distribute this software, either in source code form or as a compiled
binary, for any purpose, commercial or non-commercial, and by any
means.

In jurisdictions that recognize copyright laws, the author or authors
of this software dedicate any and all copyright interest in the
software to the public domain. We make this dedication for the benefit
of the public at large and to the detriment of our heirs and
successors. We intend this dedication to be an overt act of
relinquishment in perpetuity of all present and future rights to this
software under copyright law.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.

For more information, please refer to <http://unlicense.org>
************************************************************************
Author: Donald Beals
Description: TODO: Write a description of this file here.
****************************** Change Log ******************************
4/26/2015 10:54:14 PM - Created initial file. (dbeals)
***********************************************************************/
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework.Input;
#endregion

namespace ExampleGame.Input
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class InputManager
	{
		#region Variables
		private MouseState previousMouseState;
		private KeyboardState previousKeyboardState;

		public event EventHandler<MouseButtonEventArgs> MouseButtonStateChanged = delegate
		{
		};

		public event EventHandler<MouseMoveEventArgs> MouseMoved = delegate
		{
		};

		public event EventHandler<KeyEventArgs> KeyStateChanged = delegate
		{
		};
		#endregion

		#region Properties
		public MouseState MouseState
		{
			get;
			private set;
		}

		public KeyboardState KeyboardState
		{
			get;
			private set;
		}
		#endregion

		#region Constructors
		public InputManager()
		{
			Synchronize();
		}
		#endregion

		#region Methods
		public void Synchronize()
		{
			previousKeyboardState = Keyboard.GetState();
			previousMouseState = Mouse.GetState();
		}

		public void Update()
		{
			MouseState = Mouse.GetState();
			if(MouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
				MouseButtonStateChanged(this, new MouseButtonEventArgs(MouseButtons.Left, ButtonState.Pressed, MouseState.Position));
			else if(MouseState.LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed)
				MouseButtonStateChanged(this, new MouseButtonEventArgs(MouseButtons.Left, ButtonState.Released, MouseState.Position));

			if(MouseState.MiddleButton == ButtonState.Pressed && previousMouseState.MiddleButton == ButtonState.Released)
				MouseButtonStateChanged(this, new MouseButtonEventArgs(MouseButtons.Middle, ButtonState.Pressed, MouseState.Position));
			else if(MouseState.MiddleButton == ButtonState.Released && previousMouseState.MiddleButton == ButtonState.Pressed)
				MouseButtonStateChanged(this, new MouseButtonEventArgs(MouseButtons.Middle, ButtonState.Released, MouseState.Position));

			if(MouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Released)
				MouseButtonStateChanged(this, new MouseButtonEventArgs(MouseButtons.Right, ButtonState.Pressed, MouseState.Position));
			else if(MouseState.RightButton == ButtonState.Released && previousMouseState.RightButton == ButtonState.Pressed)
				MouseButtonStateChanged(this, new MouseButtonEventArgs(MouseButtons.Right, ButtonState.Released, MouseState.Position));

			if(MouseState.XButton1 == ButtonState.Pressed && previousMouseState.XButton1 == ButtonState.Released)
				MouseButtonStateChanged(this, new MouseButtonEventArgs(MouseButtons.X1, ButtonState.Pressed, MouseState.Position));
			else if(MouseState.XButton1 == ButtonState.Released && previousMouseState.XButton1 == ButtonState.Pressed)
				MouseButtonStateChanged(this, new MouseButtonEventArgs(MouseButtons.X1, ButtonState.Released, MouseState.Position));

			if(MouseState.XButton2 == ButtonState.Pressed && previousMouseState.XButton2 == ButtonState.Released)
				MouseButtonStateChanged(this, new MouseButtonEventArgs(MouseButtons.X2, ButtonState.Pressed, MouseState.Position));
			else if(MouseState.XButton2 == ButtonState.Released && previousMouseState.XButton2 == ButtonState.Pressed)
				MouseButtonStateChanged(this, new MouseButtonEventArgs(MouseButtons.X2, ButtonState.Released, MouseState.Position));

			if(MouseState.Position != previousMouseState.Position)
				MouseMoved(this, new MouseMoveEventArgs(MouseState.Position, MouseState.Position - previousMouseState.Position));

			KeyboardState = Keyboard.GetState();
			for(var index = 0; index < 256; ++index)
			{
				var key = (Keys)index;
				if(KeyboardState[key] == KeyState.Down && previousKeyboardState[key] == KeyState.Up)
					KeyStateChanged(this, new KeyEventArgs(key, KeyState.Down));
				else if(KeyboardState[key] == KeyState.Up && previousKeyboardState[key] == KeyState.Down)
					KeyStateChanged(this, new KeyEventArgs(key, KeyState.Up));
			}

			previousKeyboardState = KeyboardState;
			previousMouseState = MouseState;
		}
		#endregion
	}
}
