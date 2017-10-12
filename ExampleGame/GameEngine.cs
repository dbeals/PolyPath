using System.IO;
using ExampleGame.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PolyPath;
using Path = PolyPath.Path;

namespace ExampleGame
{
	/// <summary>
	///     This is the main type for your game
	/// </summary>
	public class GameEngine : Game
	{
		#region Variables
		// Input
		private readonly InputManager _inputManager = new InputManager();

		// Pathing
		private readonly Pathfinder _pathfinder = new Pathfinder();
		private readonly PathingPolygon _pathingPolygon = new PathingPolygon();

		private bool _showHelp = true;

		// Graphics
		private GraphicsDeviceManager _graphics;

		private SpriteBatch _spriteBatch;
		private Renderer _renderer;
		private SpriteFont _uiFont;
		private Texture2D _background;
		private PathingGridNode? _startNode;
		private PathingGridNode? _endNode;
		private Path _path;
		private int _selectPointIndex = -1;
		#endregion

		#region Constructors
		public GameEngine()
		{
			_graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";

			_inputManager.KeyStateChanged += inputManager_KeyStateChanged;
			_inputManager.MouseButtonStateChanged += inputManager_MouseButtonStateChanged;
			_inputManager.MouseMoved += inputManager_MouseMoved;

			_pathfinder.CheckNode = (column, row, userData) => _pathingPolygon.ContainsColumnRow(column, row) && _pathingPolygon.Nodes[(row * _pathingPolygon.Width) + column].IsPathable;
		}
		#endregion

		#region Methods
		/// <summary>
		///     Allows the game to perform any initialization it needs to before starting to run.
		///     This is where it can query for any required services and load any non-graphic
		///     related content.  Calling base.Initialize will enumerate through any components
		///     and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			IsMouseVisible = true;

			base.Initialize();
		}

		/// <summary>
		///     LoadContent will be called once per game and is the place to load
		///     all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);
			_renderer = new Renderer(_spriteBatch);
			_renderer.LoadContent();
			_uiFont = Content.Load<SpriteFont>("UIFont");

			if (File.Exists("Content/background.png"))
			{
				using (var stream = File.OpenRead("Content/background.png"))
				{
					_background = Texture2D.FromStream(GraphicsDevice, stream);
				}
			}
		}

		/// <summary>
		///     UnloadContent will be called once per game and is the place to unload
		///     all content.
		/// </summary>
		protected override void UnloadContent()
		{
			_background?.Dispose();
			_background = null;
			_renderer.UnloadContent();
		}

		/// <summary>
		///     Allows the game to run logic such as updating the world,
		///     checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			_inputManager.Update();
			base.Update(gameTime);
		}

		/// <summary>
		///     This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);

			_spriteBatch.Begin();
			if (_background != null)
				_spriteBatch.Draw(_background, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);

			var boxColor = Color.Maroon;
			var lineColor = Color.Red;

			void DrawLine(Point start, Point end, int index)
			{
				var boxSize = 8;
				var boxOffset = boxSize / 2;

				if (index == 0)
					_renderer.FillRectangle(new Rectangle(start.X - boxOffset, start.Y - boxOffset, boxSize, boxSize), boxColor);
				_renderer.FillRectangle(new Rectangle(end.X - boxOffset, end.Y - boxOffset, boxSize, boxSize), boxColor);
				_renderer.DrawLine(start.X, start.Y, end.X, end.Y, lineColor);
			}

			void DrawNode(PathingGridNode node)
			{
				_renderer.DrawRectangle(node.Bounds, Color.White);
			}

			_pathingPolygon.DebugDraw(DrawLine, DrawNode);

			if (_pathingPolygon.IsClosed)
			{
				if (_startNode != null)
					_renderer.FillRectangle(_startNode.Value.Bounds, Color.Green);
				if (_endNode != null)
					_renderer.FillRectangle(_endNode.Value.Bounds, Color.Red);
			}

			if (_path != null)
			{
				boxColor = Color.Yellow;
				lineColor = Color.Green;
				_path.DebugDraw(DrawLine);
			}

			if (_showHelp)
			{
				string text;
				if (!_pathingPolygon.IsClosed)
					text = "F1: Show/Hide help\nEsc: Exit\nSpace: {0} trim paths\nTab: {1} tight checks\nLeft Click: Add polygon points\nRight Click: Clear Polygon\nClick on the first point to close polygon";
				else if (_startNode == null)
					text = "F1: Show/Hide help\nEsc: Exit\nSpace: {0} trim paths\nTab: {1} tight checks\nLeft Click Node: Set start point\nRight Click: Clear polygon";
				else
					text = "F1: Show/Hide help\nEsc: Exit\nSpace: {0} trim paths\nTab: {1} tight checks\nLeft Click Node: Set end point\nRight Click: Clear start node";

				text = string.Format(text, _pathfinder.TrimPaths ? "Disable" : "Enable", _pathingPolygon.UseTightTests ? "Disable" : "Enable");

				_spriteBatch.DrawString(_uiFont, text, new Vector2(0, 0), Color.White);
			}
			_spriteBatch.End();
			base.Draw(gameTime);
		}

		private void inputManager_KeyStateChanged(object sender, KeyEventArgs e)
		{
			if (e.EventType == KeyState.Down)
				return;

			switch (e.Key)
			{
				case Keys.Escape:
				{
					Exit();
					break;
				}

				case Keys.Space:
				{
					_pathfinder.TrimPaths = !_pathfinder.TrimPaths;
					if (_pathingPolygon.IsClosed && _startNode != null && _endNode != null)
						_path = _pathfinder.FindPath(_startNode.Value.Column, _startNode.Value.Row, _endNode.Value.Column, _endNode.Value.Row, _pathingPolygon);
					break;
				}

				case Keys.Tab:
				{
					_pathingPolygon.UseTightTests = !_pathingPolygon.UseTightTests;
					if (_pathingPolygon.Points.Count > 1)
					{
						_pathingPolygon.Close();
						_pathingPolygon.CreateGrid(16, 16);
					}
					if (_pathingPolygon.IsClosed && _startNode != null && _endNode != null)
						_path = _pathfinder.FindPath(_startNode.Value.Column, _startNode.Value.Row, _endNode.Value.Column, _endNode.Value.Row, _pathingPolygon);
					break;
				}

				case Keys.S:
				{
					if (_pathingPolygon.Points.Count > 1)
					{
						using (var stream = File.Create("polygon.txt"))
						{
							using (var writer = new BinaryWriter(stream))
							{
								writer.Write(_pathingPolygon.Points.Count);
								foreach (var point in _pathingPolygon.Points)
								{
									writer.Write(point.X);
									writer.Write(point.Y);
								}
							}
						}
					}
					break;
				}

				case Keys.L:
				{
					if (File.Exists("polygon.txt"))
					{
						using (var stream = File.OpenRead("polygon.txt"))
						{
							using (var reader = new BinaryReader(stream))
							{
								_startNode = null;
								_endNode = null;
								_pathingPolygon.Clear();
								_path = null;

								var count = reader.ReadInt32();
								for (var index = 0; index < count; ++index)
									_pathingPolygon.Points.Add(new Point(reader.ReadInt32(), reader.ReadInt32()));
								_pathingPolygon.Close();
								_pathingPolygon.CreateGrid(16, 16);
							}
						}
					}
					break;
				}

				case Keys.F1:
				{
					_showHelp = !_showHelp;
					break;
				}
			}
		}

		private void inputManager_MouseButtonStateChanged(object sender, MouseButtonEventArgs e)
		{
			if (_pathingPolygon.IsClosed && e.Button == MouseButtons.Right && e.EventType == ButtonState.Released)
			{
				var boxSize = 8;
				var boxOffset = boxSize / 2;
				var removedPoint = false;
				for (var index = 0; index < _pathingPolygon.Points.Count; ++index)
				{
					var point = _pathingPolygon.Points[index];
					var bounds = new Rectangle(point.X - boxOffset, point.Y - boxOffset, boxSize, boxSize);
					if (bounds.Contains(e.Position))
					{
						_pathingPolygon.Points.RemoveAt(index);
						removedPoint = true;
						break;
					}
				}

				if (!removedPoint)
				{
					if (_startNode != null)
					{
						_startNode = null;
						_endNode = null;
						_path = null;
					}
					else
					{
						_pathingPolygon.Clear();
						_path = null;
					}
				}
			}
			else if (e.Button == MouseButtons.Left && e.EventType == ButtonState.Pressed)
			{
				var boxSize = 8;
				var boxOffset = boxSize / 2;
				_selectPointIndex = -1;
				for (var index = 0; index < _pathingPolygon.Points.Count; ++index)
				{
					var point = _pathingPolygon.Points[index];
					var bounds = new Rectangle(point.X - boxOffset, point.Y - boxOffset, boxSize, boxSize);
					if (bounds.Contains(e.Position))
					{
						_selectPointIndex = index;
						_path?.Clear();
						_startNode = null;
						_endNode = null;
						break;
					}
				}
			}
			else if (e.Button == MouseButtons.Left && e.EventType == ButtonState.Released)
			{
				if (_selectPointIndex != -1)
				{
					if (_selectPointIndex == 0)
						_pathingPolygon.Close();

					_selectPointIndex = -1;
					if (_pathingPolygon.IsClosed)
						_pathingPolygon.CreateGrid(16, 16);
				}
				else if (!_pathingPolygon.IsClosed)
				{
					if (_pathingPolygon.Points.Count > 1 && new Rectangle(_pathingPolygon.Points[0].X - 5, _pathingPolygon.Points[0].Y - 5, 10, 10).Contains(e.Position))
					{
						_pathingPolygon.Close();
						_pathingPolygon.CreateGrid(16, 16);
					}
					else
						_pathingPolygon.Points.Add(e.Position);
				}
				else
				{
					var node = _pathingPolygon.GetNodeAtXY(e.Position);
					if (node.IsPathable)
					{
						if (_startNode == null)
							_startNode = node;
						else
						{
							_endNode = node;
							_path = _pathfinder.FindPath(_startNode.Value.Column, _startNode.Value.Row, _endNode.Value.Column, _endNode.Value.Row, _pathingPolygon);
						}
					}
				}
			}
		}

		private void inputManager_MouseMoved(object sender, MouseMoveEventArgs e)
		{
			if (_inputManager.MouseState.LeftButton == ButtonState.Pressed)
			{
				if (_pathingPolygon.IsClosed && _startNode != null && _selectPointIndex == -1)
				{
					var node = _pathingPolygon.GetNodeAtXY(e.Position);
					if (node.IsPathable)
					{
						_endNode = node;
						_path = _pathfinder.FindPath(_startNode.Value.Column, _startNode.Value.Row, _endNode.Value.Column, _endNode.Value.Row, _pathingPolygon);
					}
				}
				else if (_selectPointIndex != -1)
				{
					var offset = e.Offset;
					if (_inputManager.KeyboardState.IsKeyDown(Keys.X))
						offset.Y = 0;
					else if (_inputManager.KeyboardState.IsKeyDown(Keys.Y))
						offset.X = 0;
					_pathingPolygon.Points[_selectPointIndex] += offset;
					if (_selectPointIndex == 0 && _pathingPolygon.IsClosed)
						_pathingPolygon.Points[_pathingPolygon.Points.Count - 1] += offset;
				}
			}
		}
		#endregion
	}
}