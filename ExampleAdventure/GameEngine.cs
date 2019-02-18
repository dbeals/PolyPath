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
using System.Collections.Generic;
using System.Linq;
using ExampleAdventure.Core;
using ExamplesCore;
using ExamplesCore.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PolyPath;

namespace ExampleAdventure
{
	public class GameEngine : GameEngineBase
	{
		#region Properties
		public Map Map { get; set; }
		public BrushSet Brushes { get; set; }
		public Entity Player { get; set; }
		public List<Entity> Entities { get; } = new List<Entity>();
		public Random Rng { get; set; } = new Random();
		#endregion

		#region Methods
		protected override void LoadContent()
		{
			base.LoadContent();

			InitializeGame();
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			foreach (var entity in Entities)
				entity.Update((float) gameTime.ElapsedGameTime.TotalSeconds);
		}

		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			Renderer.Begin();
			DrawMap();
			foreach (var entity in Entities)
				DrawEntity(entity);
			Renderer.End();
		}

		protected override void OnKeyStateChanged(object sender, KeyEventArgs e)
		{
			base.OnKeyStateChanged(sender, e);

			if (e.EventType == KeyState.Up && e.Key == Keys.F1)
				InitializeGame();
		}

		protected override void OnMouseButtonStateChanged(object sender, MouseButtonEventArgs e)
		{
			base.OnMouseButtonStateChanged(sender, e);

			if (e.EventType == ButtonState.Released && e.Button == MouseButtons.Left)
			{
				var column = e.X / TileWidth;
				var row = e.Y / TileHeight;

				var room = Map.GetRoomAt(column, row);
				if (room == null)
					return;
				var node = Map[column, row];

				var pathfinder = new Pathfinder
				{
					TrimPaths = false,
					CheckNode = (testColumn, testRow, userData) =>
					{
						var testNode = Map[testColumn, testRow];
						if (testNode.Material == Material.None || testNode.Material == Material.Wall || testNode.Material == Material.Water)
							return false;
						return true;
					}
				};

				var pathPoints = pathfinder.FindPath(Player.Column, Player.Row, column, row, out var depth, new PathfinderUserData(Map, Player));
				var path = new Path
				{
					Depth = depth
				};

				foreach (var point in pathPoints)
				{
					path.AddWaypoint(point.X, point.Y);
				}

				Player.Path = path;
			}
		}

		private void DrawMap()
		{
			for (var row = 0; row < Map.Height; ++row)
			{
				for (var column = 0; column < Map.Width; ++column)
				{
					var node = Map[column, row];
					var bounds = GetColumnRowPixelBounds(column, row);

					Brushes[node.Material].Draw(Renderer, bounds);
				}
			}

			for (var row = 0; row < Map.Height; ++row)
			{
				for (var column = 0; column < Map.Width; ++column)
				{
					var node = Map[column, row];
					var bounds = GetColumnRowPixelBounds(column, row);

					Brushes[node.Material].DrawBounds(Renderer, bounds);
				}
			}
		}

		private Rectangle GetColumnRowPixelBounds(int column, int row)
		{
			return new Rectangle(column * TileWidth, row * TileHeight, TileWidth, TileHeight);
		}

		private void DrawPath(Path path)
		{
			foreach (var waypoint in path.Waypoints)
			{
				var waypointBounds = GetColumnRowPixelBounds((int)waypoint.X, (int)waypoint.Y);
				waypointBounds = new Rectangle(waypointBounds.X + 4, waypointBounds.Y + 4, waypointBounds.Width - 8, waypointBounds.Height - 8);
				Renderer.FillRectangle(waypointBounds, Color.Red);
			}
		}

		private void DrawEntity(Entity entity)
		{
			DrawPath(entity.Path);

			var pixelBounds = GetColumnRowPixelBounds(entity.Column, entity.Row);
			pixelBounds = new Rectangle(pixelBounds.X + 4, pixelBounds.Y + 4, pixelBounds.Width - 8, pixelBounds.Height - 8);
			Renderer.FillRectangle(pixelBounds, Color.CornflowerBlue);
		}

		private void InitializeGame()
		{
			Map = MapGenerator.GenerateMap(Rng, GraphicsDevice.Viewport.Width / TileWidth, GraphicsDevice.Viewport.Height / TileHeight);

			Brushes = new BrushSet
			{
				[Material.None] = new Brush(Color.Black),
				[Material.Dirt] = new Brush(Color.SaddleBrown),
				[Material.Grass] = new Brush(Color.LawnGreen),
				[Material.Gravel] = new Brush(Color.DarkGray),
				[Material.Water] = new Brush(Color.Aqua),
				[Material.Wall] = new Brush(Color.SlateGray)
			};

			Entities.Clear();
			InitializePlayer();
		}

		private void InitializePlayer()
		{
			var room = Map.Rooms[Rng.Next(0, Map.Rooms.Count)];
			var column = Rng.Next(room.Bounds.Left + 1, room.Bounds.Right - 1);
			var row = Rng.Next(room.Bounds.Top + 1, room.Bounds.Bottom - 1);

			while(!Map.IsPassable(column, row))
			{
				column = Rng.Next(room.Bounds.Left + 1, room.Bounds.Right - 1);
				row = Rng.Next(room.Bounds.Top + 1, room.Bounds.Bottom - 1);
			}

			Player = new Entity
			{
				Column = column,
				Row = row
			};
			Entities.Add(Player);
		}
		#endregion

		public const int TileWidth = 16;
		public const int TileHeight = 16;
	}
}