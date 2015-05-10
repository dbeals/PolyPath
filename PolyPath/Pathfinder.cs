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
Description: The Pathfinder class, which is used to generate the actual
 * path; uses the A* algorithm.
****************************** Change Log ******************************
4/26/2015 3:49:53 PM - Created initial file. (dbeals)
***********************************************************************/
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
#endregion

namespace PolyPath
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class Pathfinder
	{
		#region Variables
		#endregion

		#region Properties
		/// <summary>
		/// When true, the Pathfinder will cut out extra nodes in straight lines.
		/// For example, when generating a path from 1,1 to 3,1 the Pathfinder will generate a list of waypoints: 1,1 to 2,1 to 3,1.
		/// With TrimPaths on, it will be simply 1,1 to 3,1.
		/// </summary>
		public bool TrimPaths
		{
			get;
			set;
		}

		public Func<int, int, bool> CheckNode
		{
			get;
			set;
		}
		#endregion

		#region Constructors
		public Pathfinder()
		{
		}
		#endregion

		#region Methods
		private bool ContinuesHorizontally(Point previousPoint, Point currentPoint, Point nextPoint)
		{
			return currentPoint.Y == nextPoint.Y && nextPoint.Y == previousPoint.Y && currentPoint.X != nextPoint.X;
		}

		private bool ContinuesVertically(Point previousPoint, Point currentPoint, Point nextPoint)
		{
			return currentPoint.X == nextPoint.X && nextPoint.X == previousPoint.X && currentPoint.Y != nextPoint.Y;
		}

		private bool ContinuesDiagonallyTest(Point previousPoint, Point currentPoint, Point nextPoint, int xOffset, int yOffset)
		{
			return (currentPoint.X + xOffset == nextPoint.X && currentPoint.Y + yOffset == nextPoint.Y) &&
				(currentPoint.X + -xOffset == previousPoint.X && currentPoint.Y + -yOffset == previousPoint.Y);
		}

		private bool ContinuesDiagonally(Point previousPoint, Point currentPoint, Point nextPoint)
		{
			return ContinuesDiagonallyTest(previousPoint, currentPoint, nextPoint, 1, -1) ||
				ContinuesDiagonallyTest(previousPoint, currentPoint, nextPoint, 1, 1) ||
				ContinuesDiagonallyTest(previousPoint, currentPoint, nextPoint, -1, 1) ||
				ContinuesDiagonallyTest(previousPoint, currentPoint, nextPoint, -1, -1);
		}

		private Point[] CreatePath(PathTreeNode node, out int depth)
		{
			var output = new List<Point>();
			var parent = node;
			while(parent != null)
			{
				output.Insert(0, parent.Position);
				parent = parent.Parent;
			}
			depth = output.Count;

			if(TrimPaths)
			{
				var indicesToRemove = new List<int>();
				for(var index = 1; index < output.Count - 1; ++index)
				{
					var previousPoint = output[index - 1];
					var currentPoint = output[index];
					var nextPoint = output[index + 1];

					if(ContinuesHorizontally(previousPoint, currentPoint, nextPoint) || ContinuesVertically(previousPoint, currentPoint, nextPoint) || ContinuesDiagonally(previousPoint, currentPoint, nextPoint))
						indicesToRemove.Add(index);
				}

				for(var index = indicesToRemove.Count - 1; index >= 0; --index)
					output.RemoveAt(indicesToRemove[index]);
			}
			return output.ToArray();
		}

		private bool CheckNodeList(IEnumerable<PathTreeNode> nodes, Point point)
		{
			return CheckNodeList(nodes, point.X, point.Y);
		}

		private bool CheckNodeList(IEnumerable<PathTreeNode> nodes, int column, int row)
		{
			foreach(var node in nodes)
			{
				if(node.Position.X == column && node.Position.Y == row)
					return true;
			}
			return false;
		}

		private PathTreeNode ProcessNode(PathTreeNode currentNode, int columnOffset, int rowOffset, List<PathTreeNode> openNodes, List<PathTreeNode> closedNodes)
		{
			var newNode = new PathTreeNode(currentNode.Position.X + columnOffset, currentNode.Position.Y + rowOffset, currentNode, 0);
			if((CheckNode == null || CheckNode(newNode.Position.X, newNode.Position.Y)) &&
				!CheckNodeList(closedNodes, newNode.Position) &&
				!CheckNodeList(openNodes, newNode.Position))
			{
				openNodes.Add(newNode);
				return newNode;
			}
			return null;
		}

		public Point[] FindPath(int startColumn, int startRow, int endColumn, int endRow, out int depth)
		{
			return FindPath(new Point(startColumn, startRow), new Point(endColumn, endRow), out depth);
		}

		public Point[] FindPath(Point startPosition, Point endPosition, out int depth)
		{
			var closedNodes = new List<PathTreeNode>();
			var openNodes = new List<PathTreeNode>();

			openNodes.Add(new PathTreeNode(startPosition, null, 0));

			while(true)
			{
				if(openNodes.Count == 0)
				{
					depth = 0;
					return null;
				}

				var currentNode = openNodes[0];
				var currentPosition = currentNode.Position;
				if(!closedNodes.Contains(currentNode))
				{
					if(currentPosition == endPosition)
						return CreatePath(currentNode, out depth);

					PathTreeNode left = null, up = null, right = null, down = null;

					left = ProcessNode(currentNode, -1, 0, openNodes, closedNodes);
					up = ProcessNode(currentNode, 0, -1, openNodes, closedNodes);
					right = ProcessNode(currentNode, 1, 0, openNodes, closedNodes);
					down = ProcessNode(currentNode, 0, 1, openNodes, closedNodes);

					if(left != null && up != null)
						ProcessNode(currentNode, -1, -1, openNodes, closedNodes);
					if(right != null && up != null)
						ProcessNode(currentNode, 1, -1, openNodes, closedNodes);
					if(right != null && down != null)
						ProcessNode(currentNode, 1, 1, openNodes, closedNodes);
					if(left != null && down != null)
						ProcessNode(currentNode, -1, 1, openNodes, closedNodes);

					closedNodes.Add(currentNode);
				}
				openNodes.RemoveAt(0);
			}
		}

		public Path FindPath(int startColumn, int startRow, int endColumn, int endRow, PathingPolygon pathingPolygon)
		{
			return FindPath(new Point(startColumn, startRow), new Point(endColumn, endRow), pathingPolygon);
		}

		public Path FindPath(Point startPosition, Point endPosition, PathingPolygon pathingPolygon)
		{
			int depth;
			var pathPoints = FindPath(startPosition, endPosition, out depth);
			if(pathPoints == null)
				return null;
			var output = new Path();
			output.Depth = depth;
			foreach(var point in pathPoints)
			{
				var node = pathingPolygon.GetNodeAtColumnRow(point.X, point.Y);
				output.AddWaypoint(node.Bounds.Center.ToVector2());
			}
			return output;
		}
		#endregion
	}
}
