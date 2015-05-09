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
4/26/2015 9:36:47 PM - Created initial file. (dbeals)
***********************************************************************/
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace ExampleGame
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class Renderer
	{
		#region Variables
		private Texture2D pixelTexture;
		private SpriteBatch spriteBatch;
		#endregion

		#region Properties
		#endregion

		#region Constructors
		public Renderer(SpriteBatch spriteBatch)
		{
			this.spriteBatch = spriteBatch;
		}
		#endregion

		#region Methods
		public void LoadContent()
		{
			pixelTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
			pixelTexture.SetData(new[] { Color.White });
		}

		public void UnloadContent()
		{
			if(pixelTexture != null)
				pixelTexture.Dispose();

			pixelTexture = null;
		}

		public void Begin()
		{
			spriteBatch.Begin();
		}

		public void DrawLine(SpriteBatch spriteBatch, float startX, float startY, float endX, float endY, Color color)
		{
			var direction = new Vector2(endX, endY) - new Vector2(startX, startY);
			spriteBatch.Draw(pixelTexture, new Rectangle((int)startX, (int)startY, (int)direction.Length(), 1), null, color, (float)Math.Atan2(direction.Y, direction.X), Vector2.Zero, SpriteEffects.None, 0);
		}

		public void DrawRectangle(SpriteBatch spriteBatch, Rectangle bounds, Color color)
		{
			DrawLine(spriteBatch, bounds.Left, bounds.Top, bounds.Right, bounds.Top, color);
			DrawLine(spriteBatch, bounds.Right, bounds.Top, bounds.Right, bounds.Bottom, color);
			DrawLine(spriteBatch, bounds.Left, bounds.Bottom, bounds.Right, bounds.Bottom, color);
			DrawLine(spriteBatch, bounds.Left, bounds.Top, bounds.Left, bounds.Bottom, color);
		}

		public void FillRectangle(SpriteBatch spriteBatch, Rectangle bounds, Color color)
		{
			spriteBatch.Draw(pixelTexture, bounds, color);
		}

		public void End()
		{
			spriteBatch.End();
		}
		#endregion
	}
}
