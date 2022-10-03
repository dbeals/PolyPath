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

using System.IO;
using ExamplesCore.Graphics;
using ExamplesCore.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ExamplesCore;

public abstract class GameEngineBase : Game
{
	#region Properties
	// Graphics
	public GraphicsDeviceManager Graphics { get; }

	// Input
	public InputManager Manager { get; } = new ();
	public Texture2D Background { get; private set; }
	public SpriteBatch Batch { get; private set; }
	public Renderer Renderer { get; private set; }
	public SpriteFont UIFont { get; private set; }
	#endregion

	#region Constructors
	protected GameEngineBase()
	{
		Graphics = new GraphicsDeviceManager(this);
		Content.RootDirectory = "Content";

		Manager.KeyStateChanged += OnKeyStateChanged;
		Manager.MouseButtonStateChanged += OnMouseButtonStateChanged;
		Manager.MouseMoved += OnMouseMoved;
	}
	#endregion

	#region Methods
	protected override void Initialize()
	{
		IsMouseVisible = true;
		base.Initialize();
	}

	protected override void LoadContent()
	{
		base.LoadContent();

		Batch = new SpriteBatch(GraphicsDevice);
		Renderer = new Renderer(Batch);
		Renderer.LoadContent();
		UIFont = Content.Load<SpriteFont>("UIFont");

		if (!File.Exists("Content/background.png"))
			return;

		using (var stream = File.OpenRead("Content/background.png"))
		{
			Background = Texture2D.FromStream(GraphicsDevice, stream);
		}
	}

	protected virtual void OnKeyStateChanged(object sender, KeyEventArgs e) { }

	protected virtual void OnMouseButtonStateChanged(object sender, MouseButtonEventArgs e) { }

	protected virtual void OnMouseMoved(object sender, MouseMoveEventArgs e) { }

	protected override void UnloadContent()
	{
		Background?.Dispose();
		Background = null;
		Renderer.UnloadContent();
		base.UnloadContent();
	}

	protected override void Update(GameTime gameTime)
	{
		Manager.Update();
		base.Update(gameTime);
	}
	#endregion
}