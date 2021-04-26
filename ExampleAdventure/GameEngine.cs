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
using ExampleAdventure.Extensions;
using ExamplesCore;
using ExamplesCore.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PolyPath;

namespace ExampleAdventure
{
	public class GameEngine : GameEngineBase
	{
		#region Variables
		private bool _editorIsDragging;
		private bool _isEditMode;
		private Material _editorMaterial = Material.None;
		#endregion

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

			InitializeGame(false);
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

			if (e.EventType == KeyState.Up && e.Key == Keys.F5)
				_isEditMode = !_isEditMode;

			if (_isEditMode)
				HandleEditorKeyboardInput(sender, e);
			else
				HandleGameKeyboardInput(sender, e);
		}

		protected override void OnMouseMoved(object sender, MouseMoveEventArgs e)
		{
			base.OnMouseMoved(sender, e);

			if (_editorIsDragging)
			{
				var (column, row) = e.GetMouseColumnRow(TileWidth, TileHeight);
				SetMapNode(column, row, _editorMaterial);
			}
		}

		protected override void OnMouseButtonStateChanged(object sender, MouseButtonEventArgs e)
		{
			base.OnMouseButtonStateChanged(sender, e);

			if (_isEditMode)
				HandleEditorMouseInput(sender, e);
			else
				HandleGameMouseInput(sender, e);
		}

		private void HandleGameKeyboardInput(object sender, KeyEventArgs e)
		{
			if (e.EventType == KeyState.Up)
			{
				switch (e.Key)
				{
					case Keys.F1:
					{
						InitializeGame(false);
						_isEditMode = false;
						break;
					}

					case Keys.F2:
					{
						InitializeGame(true);
						_isEditMode = true;
						break;
					}
				}
			}
		}

		private void HandleEditorKeyboardInput(object sender, KeyEventArgs e)
		{
			if (e.EventType == KeyState.Up)
			{
				switch (e.Key)
				{
					case Keys.D0:
					{
						_editorMaterial = Material.None;
						break;
					}
					case Keys.D1:
					{
						_editorMaterial = Material.Dirt;
						break;
					}
					case Keys.D2:
					{
						_editorMaterial = Material.Grass;
						break;
					}
					case Keys.D3:
					{
						_editorMaterial = Material.Gravel;
						break;
					}
					case Keys.D4:
					{
						_editorMaterial = Material.Water;
						break;
					}
					case Keys.D5:
					{
						_editorMaterial = Material.Wall;
						break;
					}
				}
			}
		}

		private void HandleGameMouseInput(object sender, MouseButtonEventArgs e)
		{
			if (e.EventType == ButtonState.Released)
			{
				if (e.Button == MouseButtons.Left)
				{
					var (column, row) = e.GetMouseColumnRow(TileWidth, TileHeight);

					var room = Map.GetRoomAt(column, row);
					if (room == null)
						return;

					var pathfinder = new Pathfinder
					{
						TrimPaths = false,
						CheckNode = (testColumn, testRow, userData) =>
						{
							var testNode = Map[testColumn, testRow];
							if (testNode.Material == Material.None || testNode.Material == Material.Wall || testNode.Material == Material.Water)
								return false;

							return !(from entity in Entities
								where entity.IsPlayer == false && entity.Column == testColumn && entity.Row == testRow
								select entity).Any();
						}
					};

					var pathPoints = pathfinder.FindPath(Player.Column, Player.Row, column, row, out var depth, new PathfinderUserData(Map, Entities, Player)
					{
						DestinationModeFlags = DestinationModeFlags.All
					});
					var path = new Path
					{
						Depth = depth
					};

					foreach (var (x, y) in pathPoints)
						path.AddWaypoint(x, y, 0f);

					Player.Path = path;
				}
			}
		}

		private void HandleEditorMouseInput(object sender, MouseButtonEventArgs e)
		{
			if (e.EventType == ButtonState.Pressed)
			{
				if (e.Button == MouseButtons.Left)
					_editorIsDragging = true;
			}

			if (e.EventType == ButtonState.Released)
			{
				_editorIsDragging = false;
				if (e.Button == MouseButtons.Left)
				{
					var (column, row) = e.GetMouseColumnRow(TileWidth, TileHeight);
					SetMapNode(column, row, _editorMaterial);
				}
				else if (e.Button == MouseButtons.Right && Player != null)
				{
					var (column, row) = e.GetMouseColumnRow(TileWidth, TileHeight);
					Player.Column = column;
					Player.Row = row;
				}
			}
		}

		private void SetMapNode(int column, int row, Material material)
		{
			if (Map.IsOutOfBounds(column, row))
				return;

			var node = Map[column, row];
			node.Material = material;
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

			if (_isEditMode)
			{
				var (column, row) = Mouse.GetState().GetMouseColumnRow(TileWidth, TileHeight);
				var mouseTileBounds = new Rectangle(column * TileWidth, row * TileHeight, TileWidth, TileHeight);

				var brush = Brushes[_editorMaterial];
				brush.Draw(Renderer, mouseTileBounds);
				brush.DrawBounds(Renderer, mouseTileBounds, Color.Red);
			}
		}

		private Rectangle GetColumnRowPixelBounds(int column, int row) => new Rectangle(column * TileWidth, row * TileHeight, TileWidth, TileHeight);

		private void DrawPath(Path path)
		{
			foreach (var waypoint in path.Waypoints)
			{
				var waypointBounds = GetColumnRowPixelBounds((int) waypoint.X, (int) waypoint.Y);
				waypointBounds = new Rectangle(waypointBounds.X + 4, waypointBounds.Y + 4, waypointBounds.Width - 8, waypointBounds.Height - 8);
				Renderer.FillRectangle(waypointBounds, Color.Yellow);
			}
		}

		private void DrawEntity(Entity entity)
		{
			DrawPath(entity.Path);

			var pixelBounds = GetColumnRowPixelBounds(entity.Column, entity.Row);
			pixelBounds = new Rectangle(pixelBounds.X + 4, pixelBounds.Y + 4, pixelBounds.Width - 8, pixelBounds.Height - 8);
			Renderer.FillRectangle(pixelBounds, entity.IsPlayer ? Color.CornflowerBlue : Color.MonoGameOrange);
		}

		private void InitializeGame(bool custom)
		{
			while(true)
			{
				Map = MapGenerator.GenerateMap(Rng, GraphicsDevice.Viewport.Width / TileWidth, GraphicsDevice.Viewport.Height / TileHeight, custom ? 0 : 100);
				if (Map.Rooms.Count == 1)
					continue; // Only 1 room generated, we can do better.

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
				if (!InitializePlayer())
					continue; // We failed to place the player, try regenerating.

				var numberOfRats = Rng.Next(1, Map.Rooms.Count);
				for (var ratIndex = 0; ratIndex < numberOfRats; ++ratIndex)
					InitializeRat();
				break;
			}
		}

		private bool InitializePlayer()
		{
			var column = 0;
			var row = 0;
			var tryCount = 0;

			if (Map.Rooms.Any())
			{
				var room = Map.Rooms[Rng.Next(0, Map.Rooms.Count)];
				column = Rng.Next(room.Bounds.Left + 1, room.Bounds.Right - 1);
				row = Rng.Next(room.Bounds.Top + 1, room.Bounds.Bottom - 1);

				while (!Map.IsPassable(column, row))
				{
					column = Rng.Next(room.Bounds.Left + 1, room.Bounds.Right - 1);
					row = Rng.Next(room.Bounds.Top + 1, room.Bounds.Bottom - 1);

					++tryCount;
					if (tryCount >= 15)
						return false; // We failed to place it 15 times, give up.
				}
			}

			Player = new Entity
			{
				Column = column,
				Row = row,
				IsPlayer = true
			};
			Entities.Add(Player);
			return true;
		}

		private bool InitializeRat()
		{
			var column = 0;
			var row = 0;
			var tryCount = 0;

			if (Map.Rooms.Any())
			{
				var room = Map.Rooms[Rng.Next(0, Map.Rooms.Count)];
				column = Rng.Next(room.Bounds.Left + 2, room.Bounds.Right - 2);
				row = Rng.Next(room.Bounds.Top + 2, room.Bounds.Bottom - 2);

				while (!Map.IsPassable(column, row) || (column == Player.Column && row == Player.Row))
				{
					column = Rng.Next(room.Bounds.Left + 2, room.Bounds.Right - 2);
					row = Rng.Next(room.Bounds.Top + 2, room.Bounds.Bottom - 2);
					++tryCount;
					if (tryCount >= 15)
						return false; // We failed to place it 15 times, give up.
				}
			}

			Entities.Add(new Entity
			{
				Column = column,
				Row = row,
				IsPlayer = false
			});
			return true;
		}
		#endregion

		public const int TileWidth = 16;
		public const int TileHeight = 16;
	}
}