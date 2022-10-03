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
using Microsoft.Xna.Framework.Graphics;

namespace ExamplesCore.Graphics;

public sealed class Renderer
{
	#region Variables
	private readonly SpriteBatch _spriteBatch;
	private Texture2D _pixelTexture;
	#endregion

	#region Constructors
	public Renderer(SpriteBatch spriteBatch) => _spriteBatch = spriteBatch;
	#endregion

	#region Methods
	public void Begin()
	{
		_spriteBatch.Begin();
	}

	public void DrawLine(float startX, float startY, float endX, float endY, Color color)
	{
		var direction = new Vector2(endX, endY) - new Vector2(startX, startY);
		_spriteBatch.Draw(_pixelTexture, new Rectangle((int)startX, (int)startY, (int)direction.Length(), 1), null, color, (float)Math.Atan2(direction.Y, direction.X), Vector2.Zero, SpriteEffects.None, 0);
	}

	public void DrawRectangle(Rectangle bounds, Color color)
	{
		DrawLine(bounds.Left, bounds.Top, bounds.Right, bounds.Top, color);
		DrawLine(bounds.Right, bounds.Top, bounds.Right, bounds.Bottom, color);
		DrawLine(bounds.Left, bounds.Bottom, bounds.Right, bounds.Bottom, color);
		DrawLine(bounds.Left, bounds.Top, bounds.Left, bounds.Bottom, color);
	}

	public void End()
	{
		_spriteBatch.End();
	}

	public void FillRectangle(Rectangle bounds, Color color)
	{
		_spriteBatch.Draw(_pixelTexture, bounds, color);
	}

	public void LoadContent()
	{
		_pixelTexture = new Texture2D(_spriteBatch.GraphicsDevice, 1, 1);
		_pixelTexture.SetData(new[]
		{
			Color.White
		});
	}

	public void UnloadContent()
	{
		_pixelTexture?.Dispose();

		_pixelTexture = null;
	}
	#endregion
}