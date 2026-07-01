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
using Microsoft.Xna.Framework;

namespace PolyPath;

public sealed class PathingPolygon : IPathingGrid
{
	#region Properties
	public Point Origin => Bounds.Location;
	public int Height { get; private set; }
	public bool IsClosed { get; private set; }
	public Rectangle Bounds { get; private set; }
	public int NodeHeight { get; private set; }
	public PathingGridNode[] Nodes { get; private set; }
	public int NodeWidth { get; private set; }
	public List<Point> Points { get; private set; }

	/// <summary>
	///     When true, all 4 corners of a node will be used when determining if the node is inside of the polygon.
	///     When false, it will test the top left and bottom right, then the top right and bottom left. If either set is
	///     inside, it considers the node inside the polygon.
	/// </summary>
	public bool UseTightTests { get; set; }

	public int Width { get; private set; }
	#endregion

	#region Constructors
	public PathingPolygon()
	{
		IsClosed = false;
		Points = new List<Point>();
	}
	#endregion

	#region Methods
	/// <summary>
	///     Clears this instance.
	/// </summary>
	public void Clear()
	{
		Width = 0;
		Height = 0;
		Bounds = Rectangle.Empty;
		NodeWidth = 0;
		NodeHeight = 0;
		Nodes = new PathingGridNode[0];
		Points.Clear();
		IsClosed = false;
	}

	/// <summary>
	///     Closes the polygon by adding the first point again (if the first and last are not already the same.)
	/// </summary>
	public void Close()
	{
		if (Points.Last() != Points.First())
			Points.Add(Points.First());
		IsClosed = true;
	}

	/// <summary>
	///     Determines whether column/row is inside the bounds of the grid.
	/// </summary>
	/// <param name="column">The column.</param>
	/// <param name="row">The row.</param>
	/// <returns>
	///     <c>true</c> column/row is inside the bounds of the grid; otherwise, <c>false</c>.
	/// </returns>
	public bool ContainsColumnRow(int column, int row) => column >= 0 && column < Width && row >= 0 && row < Height;

	/// <summary>
	///     Creates the grid.
	/// </summary>
	/// <param name="nodeWidth">Width of each node in the grid.</param>
	/// <param name="nodeHeight">Height of each node in the grid.</param>
	public void CreateGrid(int nodeWidth, int nodeHeight)
	{
		var minX = int.MaxValue;
		var minY = int.MaxValue;
		var maxX = int.MinValue;
		var maxY = int.MinValue;

		foreach (var point in Points)
		{
			if (point.X < minX)
				minX = point.X;
			if (point.Y < minY)
				minY = point.Y;

			if (point.X > maxX)
				maxX = point.X;
			if (point.Y > maxY)
				maxY = point.Y;
		}

		Bounds = new Rectangle(minX, minY, maxX - minX, maxY - minY);
		NodeWidth = nodeWidth;
		NodeHeight = nodeHeight;
		Width = (int)Math.Ceiling((double)Bounds.Width / nodeWidth);
		Height = (int)Math.Ceiling((double)Bounds.Height / nodeHeight);
		var output = new PathingGridNode[Width * Height];

		var polygonPoints = Points.ToArray();
		for (var row = 0; row < Height; ++row)
		{
			for (var column = 0; column < Width; ++column)
			{
				var nodeBounds = new Rectangle(Bounds.X + column * nodeWidth, Bounds.Y + row * nodeHeight, nodeWidth, nodeHeight);
				output[row * Width + column] = new PathingGridNode(column, row, nodeBounds, IsRectangleInsidePolygon(polygonPoints, nodeBounds, UseTightTests));
			}
		}

		Nodes = output;
	}

	/// <summary>
	///     The debug draw function.
	/// </summary>
	/// <param name="drawLine">The callback to draw a line.</param>
	/// <param name="drawNode">The callback to draw a node.</param>
	public void DebugDraw(Action<Vector3, Vector3, int> drawLine, Action<PathingGridNode> drawNode)
	{
		if (drawNode != null && IsClosed)
		{
			foreach (var node in Nodes)
			{
				if (!node.IsPathable)
					continue;

				drawNode(node);
			}
		}

		if (drawLine == null || Points.Count <= 1)
			return;

		for (var index = 0; index < Points.Count - 1; ++index)
		{
			var start = new Vector3(Points[index].ToVector2(), 0f);
			var end = new Vector3(Points[index + 1].ToVector2(), 0f);
			drawLine(start, end, index);
		}
	}

	/// <summary>
	///     Gets the node at column/row.
	/// </summary>
	/// <param name="column">The column.</param>
	/// <param name="row">The row.</param>
	/// <returns>The node at the specified position or a blank node.</returns>
	public PathingGridNode GetNodeAtColumnRow(int column, int row) => Nodes[row * Width + column];

	/// <summary>
	///     Gets the node at column/row.
	/// </summary>
	/// <param name="point">The point.</param>
	/// <returns>The node at the specified position or a blank node.</returns>
	public PathingGridNode GetNodeAtColumnRow(Point point) => GetNodeAtColumnRow(point.X, point.Y);

	/// <summary>
	///     Tries to get the node at column/row.
	/// </summary>
	/// <param name="column">The column.</param>
	/// <param name="row">The row.</param>
	/// <param name="node">The node at the specified position if it exists; otherwise, an empty node.</param>
	/// <returns>
	///     <c>true</c> if column/row is inside the bounds of the grid; otherwise, <c>false</c>.
	/// </returns>
	public bool TryGetNodeAtColumnRow(int column, int row, out PathingGridNode node)
	{
		if (!ContainsColumnRow(column, row))
		{
			node = new PathingGridNode(-1, -1, Rectangle.Empty, false);
			return false;
		}

		node = GetNodeAtColumnRow(column, row);
		return true;
	}

	/// <summary>
	///     Tries to get the node at column/row.
	/// </summary>
	/// <param name="point">The point.</param>
	/// <param name="node">The node at the specified position if it exists; otherwise, an empty node.</param>
	/// <returns>
	///     <c>true</c> if the point is inside the bounds of the grid; otherwise, <c>false</c>.
	/// </returns>
	public bool TryGetNodeAtColumnRow(Point point, out PathingGridNode node) => TryGetNodeAtColumnRow(point.X, point.Y, out node);

	/// <summary>
	///     Determines whether column/row is inside the bounds of the grid and pathable.
	/// </summary>
	/// <param name="column">The column.</param>
	/// <param name="row">The row.</param>
	/// <returns>
	///     <c>true</c> if column/row is inside the bounds of the grid and pathable; otherwise, <c>false</c>.
	/// </returns>
	public bool IsPathable(int column, int row) => ContainsColumnRow(column, row) && GetNodeAtColumnRow(column, row).IsPathable;

	/// <summary>
	///     Determines whether the point is inside the bounds of the grid and pathable.
	/// </summary>
	/// <param name="point">The point.</param>
	/// <returns>
	///     <c>true</c> if the point is inside the bounds of the grid and pathable; otherwise, <c>false</c>.
	/// </returns>
	public bool IsPathable(Point point) => IsPathable(point.X, point.Y);

	/// <summary>
	///     Gets the node at 2D position.
	/// </summary>
	/// <param name="x">The x.</param>
	/// <param name="y">The y.</param>
	/// <returns>The node at the specified position or a blank node.</returns>
	public PathingGridNode GetNodeAtXY(int x, int y) => TryGetNodeAtXY(x, y, out var node) ? node : new PathingGridNode(-1, -1, Rectangle.Empty, false);

	/// <summary>
	///     Gets the node at 2D position.
	/// </summary>
	/// <param name="point">The point.</param>
	/// <returns>The node at the specified position or a blank node.</returns>
	public PathingGridNode GetNodeAtXY(Point point) => GetNodeAtXY(point.X, point.Y);

	/// <summary>
	///     Tries to get the node at 2D position.
	/// </summary>
	/// <param name="x">The x.</param>
	/// <param name="y">The y.</param>
	/// <param name="node">The node at the specified position if it exists; otherwise, an empty node.</param>
	/// <returns>
	///     <c>true</c> if x/y is inside the grid bounds; otherwise, <c>false</c>.
	/// </returns>
	public bool TryGetNodeAtXY(int x, int y, out PathingGridNode node)
	{
		if (Nodes == null || Nodes.Length == 0 || NodeWidth <= 0 || NodeHeight <= 0)
		{
			node = new PathingGridNode(-1, -1, Rectangle.Empty, false);
			return false;
		}

		var column = (x - Bounds.X) / NodeWidth;
		var row = (y - Bounds.Y) / NodeHeight;
		if (TryGetNodeAtColumnRow(column, row, out node) && node.Bounds.Contains(x, y))
			return true;

		node = new PathingGridNode(-1, -1, Rectangle.Empty, false);
		return false;
	}

	/// <summary>
	///     Tries to get the node at 2D position.
	/// </summary>
	/// <param name="point">The point.</param>
	/// <param name="node">The node at the specified position if it exists; otherwise, an empty node.</param>
	/// <returns>
	///     <c>true</c> if the point is inside the grid bounds; otherwise, <c>false</c>.
	/// </returns>
	public bool TryGetNodeAtXY(Point point, out PathingGridNode node) => TryGetNodeAtXY(point.X, point.Y, out node);

	/// <summary>
	///     Determines whether the specified testX/testY is inside the specified points.
	/// </summary>
	/// <param name="points">The points.</param>
	/// <param name="testX">The test x.</param>
	/// <param name="testY">The test y.</param>
	/// <returns>
	///     <c>true</c> if testX/testY is inside of the points; otherwise, <c>false</c>.
	/// </returns>
	private static bool IsPointInsidePolygon(IReadOnlyList<Point> points, int testX, int testY)
	{
		var counter = 0;
		var point1 = points[0];
		for (var index = 1; index <= points.Count; ++index)
		{
			var point2 = points[index % points.Count];
			if (testY > Math.Min(point1.Y, point2.Y) &&
				testY <= Math.Max(point1.Y, point2.Y) &&
				testX <= Math.Max(point1.X, point2.X) &&
				point1.Y != point2.Y)
			{
				var xinters = (testY - point1.Y) * (point2.X - point1.X) / (point2.Y - point1.Y) + point1.X;
				if (point1.X == point2.X || testX <= xinters)
					++counter;
			}

			point1 = point2;
		}

		return counter % 2 != 0;
	}

	/// <summary>
	///     Determines whether the specified rectangle is inside the specified points. If tightTest is true, all corners must
	///     be inside the points.
	/// </summary>
	/// <param name="points">The points.</param>
	/// <param name="node">The node.</param>
	/// <param name="tightTest">
	///     if set to <c>true</c>, all corners must be inside the points. Otherwise, only the diagonal
	///     pairs (top-left, bottom-right/top-right, bottom-left) must be inside the points.
	/// </param>
	/// <returns>
	///     <c>true</c> if the rectangle is inside the points; otherwise, <c>false</c>.
	/// </returns>
	private static bool IsRectangleInsidePolygon(Point[] points, Rectangle node, bool tightTest)
	{
		var leftTopRightBottom = IsPointInsidePolygon(points, node.Left, node.Top) && IsPointInsidePolygon(points, node.Right, node.Bottom);
		var rightTopLeftBottom = IsPointInsidePolygon(points, node.Right, node.Top) && IsPointInsidePolygon(points, node.Left, node.Bottom);

		if (tightTest)
			return leftTopRightBottom && rightTopLeftBottom;
		return leftTopRightBottom;
	}
	#endregion
}