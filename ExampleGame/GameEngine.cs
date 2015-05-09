using ExampleGame.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PolyPath;

namespace ExampleGame
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class GameEngine : Game
	{
		#region Variables
		private bool showHelp = true;

		// Graphics
		private GraphicsDeviceManager graphics;
		private SpriteBatch spriteBatch;
		private Renderer renderer;
		private SpriteFont uiFont;
		private Texture2D background;

		// Input
		private InputManager inputManager = new InputManager();

		// Pathing
		private Pathfinder pathfinder = new Pathfinder();
		private PathingPolygon pathingPolygon = new PathingPolygon();
		private PathingGridNode? startNode;
		private PathingGridNode? endNode;
		private Path path;
		private int selectPointIndex = -1;
		#endregion

		#region Constructors
		public GameEngine()
			: base()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";

			inputManager.KeyStateChanged += inputManager_KeyStateChanged;
			inputManager.MouseButtonStateChanged += inputManager_MouseButtonStateChanged;
			inputManager.MouseMoved += inputManager_MouseMoved;

			pathfinder.CheckNode = (column, row) =>
			{
				if(column < 0 || column >= pathingPolygon.Width)
					return false;
				if(row < 0 || row >= pathingPolygon.Height)
					return false;

				return pathingPolygon.Nodes[(row * pathingPolygon.Width) + column].IsPathable;
			};
		}
		#endregion

		#region Methods
		void inputManager_KeyStateChanged(object sender, KeyEventArgs e)
		{
			if(e.EventType == KeyState.Down)
				return;

			switch(e.Key)
			{
				case Keys.Escape:
				{
					Exit();
					break;
				}

				case Keys.Space:
				{
					pathfinder.TrimPaths = !pathfinder.TrimPaths;
					if(pathingPolygon.IsClosed && startNode != null && endNode != null)
						path = pathfinder.FindPath(startNode.Value.Column, startNode.Value.Row, endNode.Value.Column, endNode.Value.Row, pathingPolygon);
					break;
				}

				case Keys.Tab:
				{
					pathingPolygon.UseTightTests = !pathingPolygon.UseTightTests;
					if(pathingPolygon.Points.Count > 1)
					{
						pathingPolygon.Close();
						pathingPolygon.CreateGrid(16, 16);
					}
					if(pathingPolygon.IsClosed && startNode != null && endNode != null)
						path = pathfinder.FindPath(startNode.Value.Column, startNode.Value.Row, endNode.Value.Column, endNode.Value.Row, pathingPolygon);
					break;
				}

				case Keys.S:
				{
					if(pathingPolygon.Points.Count > 1)
					{
						using(var stream = System.IO.File.Create("polygon.txt"))
						{
							using(var writer = new System.IO.BinaryWriter(stream))
							{
								writer.Write(pathingPolygon.Points.Count);
								foreach(var point in pathingPolygon.Points)
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
					if(System.IO.File.Exists("polygon.txt"))
					{
						using(var stream = System.IO.File.OpenRead("polygon.txt"))
						{
							using(var reader = new System.IO.BinaryReader(stream))
							{
								startNode = null;
								endNode = null;
								pathingPolygon.Clear();
								path = null;

								var count = reader.ReadInt32();
								for(var index = 0; index < count; ++index)
									pathingPolygon.Points.Add(new Point(reader.ReadInt32(), reader.ReadInt32()));
								pathingPolygon.Close();
								pathingPolygon.CreateGrid(16, 16);
							}
						}
					}
					break;
				}

				case Keys.F1:
				{
					showHelp = !showHelp;
					break;
				}
			}
		}

		void inputManager_MouseButtonStateChanged(object sender, MouseButtonEventArgs e)
		{
			if(pathingPolygon.IsClosed && e.Button == MouseButtons.Right && e.EventType == ButtonState.Released)
			{
				var boxSize = 8;
				var boxOffset = boxSize / 2;
				var removedPoint = false;
				for(var index = 0; index < pathingPolygon.Points.Count; ++index)
				{
					var point = pathingPolygon.Points[index];
					var bounds = new Rectangle(point.X - boxOffset, point.Y - boxOffset, boxSize, boxSize);
					if(bounds.Contains(e.Position))
					{
						pathingPolygon.Points.RemoveAt(index);
						removedPoint = true;
						break;
					}
				}

				if(!removedPoint)
				{
					if(startNode != null)
					{
						startNode = null;
						endNode = null;
						path = null;
					}
					else
					{
						pathingPolygon.Clear();
						path = null;
					}
				}
			}
			else if(e.Button == MouseButtons.Left && e.EventType == ButtonState.Pressed)
			{
				var boxSize = 8;
				var boxOffset = boxSize / 2;
				selectPointIndex = -1;
				for(var index = 0; index < pathingPolygon.Points.Count; ++index)
				{
					var point = pathingPolygon.Points[index];
					var bounds = new Rectangle(point.X - boxOffset, point.Y - boxOffset, boxSize, boxSize);
					if(bounds.Contains(e.Position))
					{
						selectPointIndex = index;
						if(path != null)
							path.Clear();
						startNode = null;
						endNode = null;
						break;
					}
				}
			}
			else if(e.Button == MouseButtons.Left && e.EventType == ButtonState.Released)
			{
				if(selectPointIndex != -1)
				{
					if(selectPointIndex == 0)
						pathingPolygon.Close();

					selectPointIndex = -1;
					if(pathingPolygon.IsClosed)
						pathingPolygon.CreateGrid(16, 16);
				}
				else if(!pathingPolygon.IsClosed)
				{
					if(pathingPolygon.Points.Count > 1 && new Rectangle(pathingPolygon.Points[0].X - 5, pathingPolygon.Points[0].Y - 5, 10, 10).Contains(e.Position))
					{
						pathingPolygon.Close();
						pathingPolygon.CreateGrid(16, 16);
					}
					else
						pathingPolygon.Points.Add(e.Position);
				}
				else
				{
					var node = pathingPolygon.GetNodeAtXY(e.Position);
					if(node.IsPathable)
					{
						if(startNode == null)
							startNode = node;
						else
						{
							endNode = node;
							path = pathfinder.FindPath(startNode.Value.Column, startNode.Value.Row, endNode.Value.Column, endNode.Value.Row, pathingPolygon);
						}
					}
				}
			}
		}

		void inputManager_MouseMoved(object sender, MouseMoveEventArgs e)
		{
			if(inputManager.MouseState.LeftButton == ButtonState.Pressed)
			{
				if(pathingPolygon.IsClosed && startNode != null && selectPointIndex == -1)
				{
					var node = pathingPolygon.GetNodeAtXY(e.Position);
					if(node.IsPathable)
					{
						endNode = node;
						path = pathfinder.FindPath(startNode.Value.Column, startNode.Value.Row, endNode.Value.Column, endNode.Value.Row, pathingPolygon);
					}
				}
				else if(selectPointIndex != -1)
				{
					var offset = e.Offset;
					if(inputManager.KeyboardState.IsKeyDown(Keys.X))
						offset.Y = 0;
					else if(inputManager.KeyboardState.IsKeyDown(Keys.Y))
						offset.X = 0;
					pathingPolygon.Points[selectPointIndex] += offset;
					if(selectPointIndex == 0 && pathingPolygon.IsClosed)
						pathingPolygon.Points[pathingPolygon.Points.Count - 1] += offset;
				}
			}
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			this.IsMouseVisible = true;

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);
			renderer = new Renderer(spriteBatch);
			renderer.LoadContent();
			uiFont = Content.Load<SpriteFont>("UIFont");

			if(System.IO.File.Exists("Content/background.png"))
			{
				using(var stream = System.IO.File.OpenRead("Content/background.png"))
				{
					background = Texture2D.FromStream(GraphicsDevice, stream);
				}
			}
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
			if(background != null)
				background.Dispose();
			background = null;
			renderer.UnloadContent();
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			inputManager.Update();
			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			var polygonPoints = pathingPolygon.Points.ToArray();
			GraphicsDevice.Clear(Color.Black);

			spriteBatch.Begin();
			if(background != null)
				spriteBatch.Draw(background, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);

			if(pathingPolygon.IsClosed)
			{
				if(startNode != null)
					renderer.FillRectangle(spriteBatch, startNode.Value.Bounds, Color.Green);
				if(endNode != null)
					renderer.FillRectangle(spriteBatch, endNode.Value.Bounds, Color.Red);

				foreach(var node in pathingPolygon.Nodes)
				{
					if(!node.IsPathable)
						continue;
					renderer.DrawRectangle(spriteBatch, node.Bounds, Color.White);
				}
			}

			if(path != null && path.Length > 0)
			{
				var boxSize = 8;
				var boxOffset = boxSize / 2;
				renderer.FillRectangle(spriteBatch, new Rectangle((int)path.Waypoints[0].X - boxOffset, (int)path.Waypoints[0].Y - boxOffset, boxSize, boxSize), Color.Yellow);
				for(var index = 1; index < path.Length; ++index)
				{
					var previous = path.Waypoints[index - 1];
					var current = path.Waypoints[index];
					renderer.FillRectangle(spriteBatch, new Rectangle((int)path.Waypoints[index].X - boxOffset, (int)path.Waypoints[index].Y - boxOffset, boxSize, boxSize), Color.Yellow);
					renderer.DrawLine(spriteBatch, previous.X, previous.Y, current.X, current.Y, Color.Green);
				}
			}

			if(polygonPoints.Length > 1)
			{
				var boxSize = 8;
				var boxOffset = boxSize / 2;
				for(var index = 0; index < polygonPoints.Length; ++index)
				{
					if(index + 1 >= polygonPoints.Length)
						break;

					var start = polygonPoints[index];
					var end = polygonPoints[index + 1];
					if(index == 0)
						renderer.FillRectangle(spriteBatch, new Rectangle(start.X - boxOffset, start.Y - boxOffset, boxSize, boxSize), Color.Maroon);
					renderer.FillRectangle(spriteBatch, new Rectangle(end.X - boxOffset, end.Y - boxOffset, boxSize, boxSize), Color.Maroon);
					renderer.DrawLine(spriteBatch, start.X, start.Y, end.X, end.Y, Color.Red);
				}
			}

			if(showHelp)
			{
				string text = string.Empty;
				if(!pathingPolygon.IsClosed)
					text = "F1: Show/Hide help\nEsc: Exit\nSpace: {0} trim paths\nTab: {1} tight checks\nLeft Click: Add polygon points\nRight Click: Clear Polygon\nClick on the first point to close polygon";
				else if(startNode == null)
					text = "F1: Show/Hide help\nEsc: Exit\nSpace: {0} trim paths\nTab: {1} tight checks\nLeft Click Node: Set start point\nRight Click: Clear polygon";
				else
					text = "F1: Show/Hide help\nEsc: Exit\nSpace: {0} trim paths\nTab: {1} tight checks\nLeft Click Node: Set end point\nRight Click: Clear start node";

				text = string.Format(text, pathfinder.TrimPaths ? "Disable" : "Enable", pathingPolygon.UseTightTests ? "Disable" : "Enable");

				var textSize = uiFont.MeasureString(text);
				var textX = (GraphicsDevice.Viewport.Width - textSize.X) / 2;
				var textY = GraphicsDevice.Viewport.Height - textSize.Y - 10;
				spriteBatch.DrawString(uiFont, text, new Vector2(0, 0), Color.White);
			}
			spriteBatch.End();
			base.Draw(gameTime);
		}
		#endregion
	}
}
