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
using System.IO;
using System.Linq;
using System.Text.Json;
using ExampleAdventure.Core;
using ExampleAdventure.Extensions;
using ExamplesCore;
using ExamplesCore.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PolyPath;
using PolyPath.Processors;

namespace ExampleAdventure;

public class GameEngine : GameEngineBase
{
	#region Variables
	private readonly Dictionary<string, IBrush> _characterBrushes = new ();
	private bool _editorIsDragging;
	private Material _editorMaterial = Material.None;
	private bool _isEditMode;
	#endregion

	#region Properties
	public List<Entity> Entities { get; } = new ();
	public BrushSet Brushes { get; set; }
	public Map Map { get; set; }
	public Entity Player { get; set; }
	public Random Rng { get; set; } = new ();
	#endregion

	#region Methods
	protected override void Draw(GameTime gameTime)
	{
		base.Draw(gameTime);

		Renderer.Begin();
		DrawMap();
		foreach (var entity in Entities)
			DrawEntity(entity);
		Renderer.End();
	}

	protected override void Initialize()
	{
		base.Initialize();

		Window.Title = "PolyPath - Example Adventure";
	}

	protected override void LoadContent()
	{
		base.LoadContent();

		InitializeCharacterTiles();
		InitializeGame(false);
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

	protected override void OnMouseButtonStateChanged(object sender, MouseButtonEventArgs e)
	{
		base.OnMouseButtonStateChanged(sender, e);

		if (_isEditMode)
			HandleEditorMouseInput(sender, e);
		else
			HandleGameMouseInput(sender, e);
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

	protected override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		foreach (var entity in Entities)
			entity.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
	}

	private void DrawEntity(Entity entity)
	{
		DrawPath(entity.Path);

		var pixelBounds = GetColumnRowPixelBounds(entity.Column, entity.Row);

		if (!_characterBrushes.TryGetValue(entity.CharacterClass, out var brush))
		{
			pixelBounds = new Rectangle(pixelBounds.X + 4, pixelBounds.Y + 4, pixelBounds.Width - 8, pixelBounds.Height - 8);
			Renderer.FillRectangle(pixelBounds, entity.IsPlayer ? Color.CornflowerBlue : Color.MonoGameOrange);
		}
		else
			brush.Draw(Renderer, pixelBounds);
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

	private void DrawPath(WaypointPath path)
	{
		foreach (var waypoint in path.Waypoints)
		{
			var waypointBounds = GetColumnRowPixelBounds((int)waypoint.X, (int)waypoint.Y);
			waypointBounds = new Rectangle(waypointBounds.X + 4, waypointBounds.Y + 4, waypointBounds.Width - 8, waypointBounds.Height - 8);
			Renderer.FillRectangle(waypointBounds, Color.Yellow);
		}
	}

	private Rectangle GetColumnRowPixelBounds(int column, int row) => new (column * TileWidth, row * TileHeight, TileWidth, TileHeight);

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

	private void HandleGameMouseInput(object sender, MouseButtonEventArgs e)
	{
		if (e.EventType != ButtonState.Released)
			return;

		if (e.Button != MouseButtons.Left)
			return;

		var (column, row) = e.GetMouseColumnRow(TileWidth, TileHeight);

		var room = Map.GetRoomAt(column, row);
		if (room == null)
			return;

		var pathfinder = new Pathfinder
		{
			CheckNode = (testColumn, testRow, userData) =>
			{
				var testNode = Map[testColumn, testRow];
				if (testNode.Material is Material.None or Material.Wall or Material.Water)
					return false;

				return !(from entity in Entities
					where entity.IsPlayer == false && entity.Column == testColumn && entity.Row == testRow
					select entity).Any();
			}
		};

		var pathPoints = pathfinder.FindPath(Player.Column, Player.Row, column, row, out var depth, new PathfinderUserData(Map, Entities, Player));

		Player.Path = new WaypointPath
		{
			Depth = depth,
			Waypoints = (pathfinder.PostProcessor ?? DirectPathPostProcessor.Instance).Process(pathPoints, null).ToArray()
		};
		;
	}

	private void InitializeCharacterTiles()
	{
		_characterBrushes["Rat"] = new ColoredBrush(Color.MonoGameOrange);
		_characterBrushes["Player"] = new ColoredBrush(Color.CornflowerBlue);

		if (!File.Exists("Content/Characters.json"))
			return;

		var tileInfo = JsonSerializer.Deserialize<TileSetInfo[]>(File.ReadAllText("Content/Characters.json"));
		if (tileInfo == null)
			return;

		foreach (var item in tileInfo)
		{
			var texture = Content.Load<Texture2D>(item.Texture);
			_characterBrushes[item.Name] = new TileBrush(texture, item.ParsedRegion);
		}
	}

	private void InitializeGame(bool custom)
	{
		var width = GraphicsDevice.Viewport.Width / TileWidth;
		var height = GraphicsDevice.Viewport.Height / TileHeight;

		while (true)
		{
			Map = MapGenerator.GenerateMap(Rng, width, height, custom ? 0 : 100);
			if (Map.Rooms.Count == 1)
				continue; // Only 1 room generated, we can do better.

			Brushes = new BrushSet
			{
				[Material.None] = new ColoredBrush(Color.Black),
				[Material.Dirt] = new ColoredBrush(Color.SaddleBrown),
				[Material.Grass] = new ColoredBrush(Color.LawnGreen),
				[Material.Gravel] = new ColoredBrush(Color.DarkGray),
				[Material.Water] = new ColoredBrush(Color.Aqua),
				[Material.Wall] = new ColoredBrush(Color.SlateGray)
			};

			TryLoadTileSet();

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
			CharacterClass = "Player",
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
			CharacterClass = "Rat",
			Column = column,
			Row = row,
			IsPlayer = false
		});
		return true;
	}

	private void SetMapNode(int column, int row, Material material)
	{
		if (Map.IsOutOfBounds(column, row))
			return;

		var node = Map[column, row];
		node.Material = material;
	}

	private void TryLoadTileSet()
	{
		if (!File.Exists("Content/Tiles.json"))
			return;

		var tileInfo = JsonSerializer.Deserialize<TileSetInfo[]>(File.ReadAllText("Content/Tiles.json"));
		if (tileInfo == null)
			return;

		foreach (var item in tileInfo)
		{
			var material = Enum.Parse<Material>(item.Material);
			var texture = Content.Load<Texture2D>(item.Texture);
			Brushes[material] = new TileBrush(texture, item.ParsedRegion);
		}
	}
	#endregion

	public const int TileHeight = 16;

	public const int TileWidth = 16;
}