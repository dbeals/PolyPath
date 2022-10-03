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
using ExamplesCore;
using ExamplesCore.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PolyPath;
using PolyPath.Processors;

namespace ExampleGame;

public class GameEngine : GameEngineBase
{
	#region Nested types
	private sealed class CustomFindPathData : FindPathData
	{
		#region Variables
		private readonly Dictionary<Tuple<int, int>, int> _weights;
		#endregion

		#region Constructors
		public CustomFindPathData() => _weights = new Dictionary<Tuple<int, int>, int>();
		#endregion

		#region Methods
		public void Clear()
		{
			_weights.Clear();
		}

		public void DecrementWeight(int column, int row)
		{
			var key = new Tuple<int, int>(column, row);
			if (!_weights.TryGetValue(key, out var weight))
				_weights[key] = 0;
			--weight;
			if (weight < 0)
				weight = 0;
			_weights[key] = weight;
		}

		public override int GetWeight(Point waypointPosition, Point endPosition)
		{
			var key = new Tuple<int, int>(waypointPosition.X, waypointPosition.Y);
			return (_weights.TryGetValue(key, out var weight) ? weight : 0) * Scalar;
		}

		public void IncrementWeight(int column, int row)
		{
			var key = new Tuple<int, int>(column, row);
			if (!_weights.TryGetValue(key, out var weight))
				_weights[key] = 0;
			++weight;
			_weights[key] = weight;
		}
		#endregion

		public const int Scalar = 10;
	}
	#endregion

	#region Variables
	// Pathing
	private readonly Pathfinder _pathfinder = new ();
	private readonly PathingPolygon _pathingPolygon = new ();
	private readonly CustomFindPathData _userData = new ();
	private bool _controlPressed;
	private PathingGridNode? _endNode;
	private WaypointPath _path;
	private int _selectPointIndex = -1;
	private bool _shiftPressed;

	private bool _showHelp = true;

	private PathingGridNode? _startNode;
	#endregion

	#region Constructors
	public GameEngine()
	{
		_pathfinder.CheckNode = (column, row, userData) => _pathingPolygon.ContainsColumnRow(column, row) && _pathingPolygon.Nodes[row * _pathingPolygon.Width + column].IsPathable;
	}
	#endregion

	#region Methods
	protected override void Draw(GameTime gameTime)
	{
		GraphicsDevice.Clear(Color.Black);

		Batch.Begin();
		if (Background != null)
			Batch.Draw(Background, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);

		var boxColor = Color.Maroon;
		var lineColor = Color.Red;

		void DrawLine(Vector3 start, Vector3 end, int index)
		{
			const int boxSize = 8;
			const int boxOffset = boxSize / 2;

			var (startX, startY, _) = start;
			if (index == 0)
				Renderer.FillRectangle(new Rectangle((int)startX - boxOffset, (int)startY - boxOffset, boxSize, boxSize), boxColor);

			var (endX, endY, _) = end;
			Renderer.FillRectangle(new Rectangle((int)endX - boxOffset, (int)endY - boxOffset, boxSize, boxSize), boxColor);
			Renderer.DrawLine(startX, startY, endX, endY, lineColor);
		}

		void DrawNode(PathingGridNode node)
		{
			var weight = _userData.GetWeight(new Point(node.Column, node.Row), Point.Zero);

			var color = Color.Lerp(Color.White, Color.Red, weight / (CustomFindPathData.Scalar * 3f));

			if (color == Color.White)
				Renderer.DrawRectangle(node.Bounds, color);
			else
				Renderer.FillRectangle(node.Bounds, color);
		}

		_pathingPolygon.DebugDraw(DrawLine, DrawNode);

		if (_pathingPolygon.IsClosed)
		{
			if (_startNode != null)
				Renderer.FillRectangle(_startNode.Value.Bounds, Color.Green);
			if (_endNode != null)
				Renderer.FillRectangle(_endNode.Value.Bounds, Color.Red);
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

			text = string.Format(text, TrimPathProcessor.IsTrimmingPaths(_pathfinder) ? "Disable" : "Enable", _pathingPolygon.UseTightTests ? "Disable" : "Enable");

			Batch.DrawString(UIFont, text, new Vector2(0, 0), Color.White);
		}

		Batch.End();
		base.Draw(gameTime);
	}

	protected override void OnKeyStateChanged(object sender, KeyEventArgs e)
	{
		if (e.EventType == KeyState.Down)
		{
			switch (e.Key)
			{
				case Keys.LeftShift:
				case Keys.RightShift:
				{
					_shiftPressed = true;
					break;
				}

				case Keys.LeftControl:
				case Keys.RightControl:
				{
					_controlPressed = true;
					break;
				}
			}

			return;
		}

		switch (e.Key)
		{
			case Keys.Escape:
			{
				Exit();
				break;
			}

			case Keys.Space:
			{
				TrimPathProcessor.ToggleTrimming(_pathfinder);
				if (_pathingPolygon.IsClosed && _startNode != null && _endNode != null)
					_path = _pathfinder.FindPath(_startNode.Value.Column, _startNode.Value.Row, _endNode.Value.Column, _endNode.Value.Row, _pathingPolygon, _userData);
				break;
			}

			case Keys.Tab:
			{
				_pathingPolygon.UseTightTests = !_pathingPolygon.UseTightTests;
				if (_pathingPolygon.Points.Count > 1)
				{
					_pathingPolygon.Close();
					_pathingPolygon.CreateGrid(16, 16);
					_userData.Clear();
				}

				if (_pathingPolygon.IsClosed && _startNode != null && _endNode != null)
					_path = _pathfinder.FindPath(_startNode.Value.Column, _startNode.Value.Row, _endNode.Value.Column, _endNode.Value.Row, _pathingPolygon, _userData);
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
							_userData.Clear();
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

			case Keys.LeftShift:
			case Keys.RightShift:
			{
				_shiftPressed = false;
				break;
			}

			case Keys.LeftControl:
			case Keys.RightControl:
			{
				_controlPressed = false;
				break;
			}
		}
	}

	protected override void OnMouseButtonStateChanged(object sender, MouseButtonEventArgs e)
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
			if (_shiftPressed)
			{
				var node = _pathingPolygon.GetNodeAtXY(e.Position);
				if (node.Column != -1 && node.Row != -1)
					_userData.IncrementWeight(node.Column, node.Row);
				return;
			}

			if (_controlPressed)
			{
				var node = _pathingPolygon.GetNodeAtXY(e.Position);
				if (node.Column != -1 && node.Row != -1)
					_userData.DecrementWeight(node.Column, node.Row);
				return;
			}

			if (_selectPointIndex != -1)
			{
				if (_selectPointIndex == 0)
					_pathingPolygon.Close();

				_selectPointIndex = -1;
				if (_pathingPolygon.IsClosed)
				{
					_pathingPolygon.CreateGrid(16, 16);
					_userData.Clear();
				}
			}
			else if (!_pathingPolygon.IsClosed)
			{
				if (_pathingPolygon.Points.Count > 1 && new Rectangle(_pathingPolygon.Points[0].X - 5, _pathingPolygon.Points[0].Y - 5, 10, 10).Contains(e.Position))
				{
					_pathingPolygon.Close();
					_pathingPolygon.CreateGrid(16, 16);
					_userData.Clear();
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
						_path = _pathfinder.FindPath(_startNode.Value.Column, _startNode.Value.Row, _endNode.Value.Column, _endNode.Value.Row, _pathingPolygon, _userData);
					}
				}
			}
		}
	}

	protected override void OnMouseMoved(object sender, MouseMoveEventArgs e)
	{
		if (Manager.MouseState.LeftButton == ButtonState.Pressed)
		{
			if (_pathingPolygon.IsClosed && _startNode != null && _selectPointIndex == -1)
			{
				var node = _pathingPolygon.GetNodeAtXY(e.Position);
				if (node.IsPathable)
				{
					_endNode = node;
					_path = _pathfinder.FindPath(_startNode.Value.Column, _startNode.Value.Row, _endNode.Value.Column, _endNode.Value.Row, _pathingPolygon, _userData);
				}
			}
			else if (_selectPointIndex != -1)
			{
				var offset = e.Offset;
				if (Manager.KeyboardState.IsKeyDown(Keys.X))
					offset.Y = 0;
				else if (Manager.KeyboardState.IsKeyDown(Keys.Y))
					offset.X = 0;
				_pathingPolygon.Points[_selectPointIndex] += offset;
				if (_selectPointIndex == 0 && _pathingPolygon.IsClosed)
					_pathingPolygon.Points[_pathingPolygon.Points.Count - 1] += offset;
			}
		}
	}
	#endregion
}