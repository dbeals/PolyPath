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
Description: The PathingPolygon class, which is used to house the polygon
 * and generate the pathing grid.
****************************** Change Log ******************************
4/26/2015 4:24:47 PM - Created initial file. (dbeals)
***********************************************************************/
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
#endregion

namespace PolyPath
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class PathingPolygon
	{
		#region Variables
		#endregion

		#region Properties
		public List<Point> Points
		{
			get;
			private set;
		}

		public PathingGridNode[] Nodes
		{
			get;
			private set;
		}

		public int NodeWidth
		{
			get;
			private set;
		}

		public int NodeHeight
		{
			get;
			private set;
		}

		public int Width
		{
			get;
			private set;
		}

		public int Height
		{
			get;
			private set;
		}

		public bool IsClosed
		{
			get;
			private set;
		}

		/// <summary>
		/// When true, all 4 corners of a node will be used when determing if the node is inside of the polygon.
		/// When false, it will test the top left and bottom right, then the top right and bottom left. If either set is inside, it considers the node inside the polygon.
		/// </summary>
		public bool UseTightTests
		{
			get;
			set;
		}
		#endregion

		#region Constructors
		public PathingPolygon()
		{
			IsClosed = false;
			Points = new List<Point>();
		}
		#endregion

		#region Methods
		private static bool IsPointInsidePolygon(Point[] points, int testX, int testY)
		{
			var counter = 0;
			var point1 = points[0];
			for(var index = 1; index <= points.Length; ++index)
			{
				var point2 = points[index % points.Length];
				if(testY > Math.Min(point1.Y, point2.Y) &&
					testY <= Math.Max(point1.Y, point2.Y) &&
					testX <= Math.Max(point1.X, point2.X) &&
					point1.Y != point2.Y)
				{
					var xinters = (testY - point1.Y) * (point2.X - point1.X) / (point2.Y - point1.Y) + point1.X;
					if(point1.X == point2.X || testX <= xinters)
						++counter;
				}
				point1 = point2;
			}

			return counter % 2 != 0;
		}

		private static bool IsRectangleInsidePolygon(Point[] points, Rectangle node, bool tightTest)
		{
			var leftTopRightBottom = (IsPointInsidePolygon(points, node.Left, node.Top) && IsPointInsidePolygon(points, node.Right, node.Bottom));
			var rightTopLeftBottom = (IsPointInsidePolygon(points, node.Right, node.Top) && IsPointInsidePolygon(points, node.Left, node.Bottom));

			if(tightTest)
				return leftTopRightBottom && rightTopLeftBottom;
			return leftTopRightBottom || leftTopRightBottom;
		}

		public void Close()
		{
			if(Points.Last() != Points.First())
				Points.Add(Points.First());
			IsClosed = true;
		}

		public void Clear()
		{
			Width = 0;
			Height = 0;
			NodeWidth = 0;
			NodeHeight = 0;
			Nodes = new PathingGridNode[0];
			Points.Clear();
			IsClosed = false;
		}

		public void CreateGrid(int nodeWidth, int nodeHeight)
		{
			var minX = int.MaxValue;
			var minY = int.MaxValue;
			var maxX = int.MinValue;
			var maxY = int.MinValue;

			foreach(var point in Points)
			{
				if(point.X < minX)
					minX = point.X;
				if(point.Y < minY)
					minY = point.Y;

				if(point.X > maxX)
					maxX = point.X;
				if(point.Y > maxY)
					maxY = point.Y;
			}

			var bounds = new Rectangle(minX, minY, maxX - minX, maxY - minY);
			Width = (int)Math.Ceiling((double)bounds.Width / nodeWidth);
			Height = (int)Math.Ceiling((double)bounds.Height / nodeHeight);
			var output = new PathingGridNode[Width * Height];

			var polygonPoints = Points.ToArray();
			for(var row = 0; row < Height; ++row)
			{
				for(var column = 0; column < Width; ++column)
				{
					var nodeBounds = new Rectangle(bounds.X + (column * nodeWidth), bounds.Y + (row * nodeHeight), nodeWidth, nodeHeight);
					output[(row * Width) + column] = new PathingGridNode(column, row, nodeBounds, IsRectangleInsidePolygon(polygonPoints, nodeBounds, UseTightTests));
				}
			}
			Nodes = output;
		}

		public PathingGridNode GetNodeAtXY(int x, int y)
		{
			foreach(var node in Nodes)
			{
				if(node.Bounds.Contains(x, y))
					return node;
			}
			return new PathingGridNode(-1, -1, Rectangle.Empty, false);
		}

		public PathingGridNode GetNodeAtXY(Point point)
		{
			return GetNodeAtXY(point.X, point.Y);
		}

		public PathingGridNode GetNodeAtColumnRow(int column, int row)
		{
			return Nodes[(row * Width) + column];
		}

		public PathingGridNode GetNodeAtColumnRow(Point point)
		{
			return GetNodeAtColumnRow(point.X, point.Y);
		}

		public bool ContainsColumnRow(int column, int row)
		{
			return column >= 0 && column < Width && row >= 0 && row < Height;
		}

		public void DebugDraw(Action<Point, Point, int> drawLine, Action<PathingGridNode> drawNode)
		{
			if(drawNode != null && IsClosed)
			{
				foreach(var node in Nodes)
				{
					if(!node.IsPathable)
						continue;

					drawNode(node);
				}
			}

			if(drawLine != null && Points.Count > 1)
			{
				var boxSize = 8;
				var boxOffset = boxSize / 2;
				for(var index = 0; index < Points.Count - 1; ++index)
				{
					var start = Points[index];
					var end = Points[index + 1];
					drawLine(start, end, index);
				}
			}
		}
		#endregion
	}

	public struct PathingGridNode
	{
		public readonly int Column;
		public readonly int Row;
		public readonly Rectangle Bounds;
		public readonly bool IsPathable;

		public PathingGridNode(int column, int row, Rectangle bounds, bool isPathable)
		{
			Column = column;
			Row = row;
			Bounds = bounds;
			IsPathable = isPathable;
		}
	}
}
