// /***********************************************************************
// This is free and unencumbered software released into the public domain.
// 
// Anyone is free to copy, modify, publish, use, compile, sell, or
// distribute this software, either in source code form or as a compiled
// binary, for any purpose, commercial or non-commercial, and by any
// means.
// 
// In jurisdictions that recognize copyright laws, the author or authors
// of this software dedicate any and all copyright interest in the
// software to the public domain. We make this dedication for the benefit
// of the public at large and to the detriment of our heirs and
// successors. We intend this dedication to be an overt act of
// relinquishment in perpetuity of all present and future rights to this
// software under copyright law.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
// OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// 
// For more information, please refer to <http://unlicense.org>
// ***********************************************************************/

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ExamplesCore.Input;

public sealed class InputManager
{
	#region Variables
	private KeyboardState _previousKeyboardState;
	private MouseState _previousMouseState;
	#endregion

	#region Properties
	public KeyboardState KeyboardState { get; private set; }
	public MouseState MouseState { get; private set; }
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
		_previousKeyboardState = Keyboard.GetState();
		_previousMouseState = Mouse.GetState();
	}

	public void Update()
	{
		MouseState = Mouse.GetState();
		HandleMouseButtonStates(MouseState, _previousMouseState);
		if (MouseState.Position != _previousMouseState.Position)
			MouseMoved(this, new MouseMoveEventArgs(MouseState.Position, MouseState.Position - _previousMouseState.Position));

		KeyboardState = Keyboard.GetState();
		for (var index = 0; index < 256; ++index)
		{
			var key = (Keys)index;
			if (KeyboardState[key] != _previousKeyboardState[key])
				KeyStateChanged(this, new KeyEventArgs(key, KeyboardState[key]));
		}

		_previousKeyboardState = KeyboardState;
		_previousMouseState = MouseState;
	}

	private void HandleMouseButtonState(ButtonState currentState, ButtonState previousState, MouseButtons button, Point currentPosition)
	{
		if (currentState != previousState)
			MouseButtonStateChanged(this, new MouseButtonEventArgs(button, currentState, currentPosition));
	}

	private void HandleMouseButtonStates(MouseState currentState, MouseState previousState)
	{
		HandleMouseButtonState(currentState.LeftButton, previousState.LeftButton, MouseButtons.Left, currentState.Position);
		HandleMouseButtonState(currentState.MiddleButton, previousState.MiddleButton, MouseButtons.Middle, currentState.Position);
		HandleMouseButtonState(currentState.RightButton, previousState.RightButton, MouseButtons.Right, currentState.Position);
		HandleMouseButtonState(currentState.XButton1, previousState.XButton1, MouseButtons.X1, currentState.Position);
		HandleMouseButtonState(currentState.XButton2, previousState.XButton2, MouseButtons.X2, currentState.Position);
	}
	#endregion

	#region Events
	public event EventHandler<MouseButtonEventArgs> MouseButtonStateChanged = delegate { };
	public event EventHandler<MouseMoveEventArgs> MouseMoved = delegate { };
	public event EventHandler<KeyEventArgs> KeyStateChanged = delegate { };
	#endregion
}