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
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Xna.Framework;
using PolyPath.Processors;

namespace PolyPath;

public sealed class Pathfinder
{
	#region Properties
	public ICollection<IPathProcessor> Processors { get; } = new Collection<IPathProcessor>();

	/// <summary>
	///     Gets or sets the check node callback.
	/// </summary>
	/// <value>
	///     A callback that checks whether or not a node is valid. Should return true if it is valid, false otherwise.
	/// </value>
	public Func<int, int, FindPathData, bool> CheckNode { get; set; }

	public IPathPostProcessor PostProcessor { get; set; }
	#endregion

	#region Methods
	/// <summary>
	///     Finds the path.
	/// </summary>
	/// <param name="startColumn">The start column.</param>
	/// <param name="startRow">The start row.</param>
	/// <param name="endColumn">The end column.</param>
	/// <param name="endRow">The end row.</param>
	/// <param name="depth">An output variable; the depth of the path.</param>
	/// <param name="userData">The user data used when processing nodes.</param>
	/// <returns>A list of points defining the found path.</returns>
	public Point[] FindPath(int startColumn, int startRow, int endColumn, int endRow, out int depth, FindPathData userData) => FindPath(new Point(startColumn, startRow), new Point(endColumn, endRow), out depth, userData);

	/// <summary>
	///     Finds the path.
	/// </summary>
	/// <param name="startPosition">The start position.</param>
	/// <param name="endPosition">The end position.</param>
	/// <param name="depth">An output variable; the depth of the path.</param>
	/// <param name="userData">The user data used when processing nodes.</param>
	/// <returns>A list of points defining the found path.</returns>
	public Point[] FindPath(Point startPosition, Point endPosition, out int depth, FindPathData userData)
	{
		userData.StartPosition = startPosition;
		userData.EndPosition = endPosition;
		var closedNodes = new List<PathTreeNode>();
		var possibleNodes = new List<PathTreeNode>();
		var openNodes = new List<PathTreeNode>
		{
			new (startPosition, null, 0)
		};

		var destinationPoints = GetDestinationPoints(endPosition, userData);

		while (true)
		{
			if (openNodes.Count == 0)
			{
				if (possibleNodes.Any())
					return CreatePath(possibleNodes.First(), out depth, userData);

				depth = 0;
				return Array.Empty<Point>();
			}

			openNodes.Sort((node1, node2) => node1.Weight.CompareTo(node2.Weight));

			var currentNode = openNodes[0];
			var currentPosition = currentNode.Position;
			if (!closedNodes.Contains(currentNode))
			{
				if (userData.DestinationModeFlags.HasFlag(DestinationModeFlags.Exact) && currentPosition == endPosition)
					return CreatePath(currentNode, out depth, userData);

				if (destinationPoints != null && destinationPoints.Any(x => x == currentPosition))
				{
					if (!userData.DestinationModeFlags.HasFlag(DestinationModeFlags.Exact))
						return CreatePath(currentNode, out depth, userData);
					possibleNodes.Add(currentNode);
				}

				var left = ProcessNode(currentNode, -1, 0, openNodes, closedNodes, endPosition, userData);
				var up = ProcessNode(currentNode, 0, -1, openNodes, closedNodes, endPosition, userData);
				var right = ProcessNode(currentNode, 1, 0, openNodes, closedNodes, endPosition, userData);
				var down = ProcessNode(currentNode, 0, 1, openNodes, closedNodes, endPosition, userData);

				if (left != null && up != null)
					ProcessNode(currentNode, -1, -1, openNodes, closedNodes, endPosition, userData);

				if (right != null && up != null)
					ProcessNode(currentNode, 1, -1, openNodes, closedNodes, endPosition, userData);

				if (right != null && down != null)
					ProcessNode(currentNode, 1, 1, openNodes, closedNodes, endPosition, userData);

				if (left != null && down != null)
					ProcessNode(currentNode, -1, 1, openNodes, closedNodes, endPosition, userData);

				closedNodes.Add(currentNode);
			}

			openNodes.RemoveAt(0);
		}
	}

	/// <summary>
	///     Finds the path.
	/// </summary>
	/// <param name="startColumn">The start column.</param>
	/// <param name="startRow">The start row.</param>
	/// <param name="endColumn">The end column.</param>
	/// <param name="endRow">The end row.</param>
	/// <param name="pathingPolygon">The pathing polygon to find the path inside of.</param>
	/// <param name="userData">The user data used when processing nodes.</param>
	/// <returns>A list of points defining the found path.</returns>
	public WaypointPath FindPath(int startColumn, int startRow, int endColumn, int endRow, PathingPolygon pathingPolygon, FindPathData userData) => FindPath(new Point(startColumn, startRow), new Point(endColumn, endRow), pathingPolygon, userData);

	/// <summary>
	///     Finds the path.
	/// </summary>
	/// <param name="startPosition">The start position.</param>
	/// <param name="endPosition">The end position.</param>
	/// <param name="pathingPolygon">The pathing polygon.</param>
	/// <param name="userData">The user data used when processing nodes.</param>
	/// <returns>A list of points defining the found path.</returns>
	public WaypointPath FindPath(Point startPosition, Point endPosition, PathingPolygon pathingPolygon, FindPathData userData)
	{
		var pathPoints = FindPath(startPosition, endPosition, out var depth, userData);
		if (!pathPoints.Any())
			return new WaypointPath();

		return new WaypointPath
		{
			Depth = depth,
			Waypoints = (PostProcessor ?? DefaultPathPostProcessor.Instance).Process(pathPoints, pathingPolygon).ToArray()
		};
	}

	/// <summary>
	///     Determines if any of the specified nodes are at the point.
	/// </summary>
	/// <param name="nodes">The nodes.</param>
	/// <param name="point">The point.</param>
	/// <returns>
	///     <c>true</c> if any of the points are at the point; otherwise, <c>false</c>.
	/// </returns>
	private static bool AnyNodeIsAtPoint(IEnumerable<PathTreeNode> nodes, Point point) => AnyNodeIsAtPoint(nodes, point.X, point.Y);

	/// <summary>
	///     Determines if any of the specified nodes are at the point.
	/// </summary>
	/// <param name="nodes">The nodes.</param>
	/// <param name="column">The column.</param>
	/// <param name="row">The row.</param>
	/// <returns>
	///     <c>true</c> if any of the points are at the point; otherwise, <c>false</c>.
	/// </returns>
	private static bool AnyNodeIsAtPoint(IEnumerable<PathTreeNode> nodes, int column, int row) => nodes.Any(node => node.Position.X == column && node.Position.Y == row);

	/// <summary>
	///     Creates the path.
	/// </summary>
	/// <param name="node">The node.</param>
	/// <param name="depth">An output variable; the depth of the path.</param>
	/// <param name="userData">The user data.</param>
	/// <returns>A list of points defining the found path.</returns>
	private Point[] CreatePath(PathTreeNode node, out int depth, FindPathData userData)
	{
		var output = new List<Point>();
		var parent = node;
		while (parent != null)
		{
			output.Insert(0, parent.Position);
			parent = parent.Parent;
		}

		if (userData != null)
		{
			for (var index = output.Count - 1; index >= 0; --index)
			{
				var poppingWaypoints = userData.PopWaypointTest(output[index], index);
				if (!poppingWaypoints)
					break;

				output.RemoveAt(index);
			}

			if (output.Count == 0)
			{
				depth = 0;
				return output.ToArray();
			}

			if (userData.PopFirstWaypoint)
				output.RemoveAt(0);

			if (userData.PopLastNWaypoints > 0)
			{
				if (userData.PopLastNWaypoints > output.Count)
				{
					depth = 0;
					output.Clear();
					return output.ToArray();
				}

				output.RemoveRange(output.Count - userData.PopLastNWaypoints, userData.PopLastNWaypoints);
			}
		}

		depth = output.Count;

		var initialWaypoints = output.ToArray();
		output = Processors.Aggregate(output, (current, processor) => processor.Process(current, initialWaypoints));
		return output.ToArray();
	}

	/// <summary>
	///     Calculates the neighbors of a specific point based on the DestinationMode property of <paramref name="userData" />.
	/// </summary>
	/// <param name="endPosition">The destination point.</param>
	/// <param name="userData">The data provided by the user.</param>
	/// <returns>
	///     Null if the DestinationMode property of <paramref name="userData" /> is
	///     <see cref="DestinationModeFlags.Exact" />, otherwise an array of neighbor points.
	/// </returns>
	/// <exception cref="ArgumentOutOfRangeException">
	///     If userData.DestinationMode is not a valid
	///     <see cref="DestinationModeFlags" /> value.
	/// </exception>
	private static Point[] GetDestinationPoints(Point endPosition, FindPathData userData)
	{
		if (userData.DestinationModeFlags == DestinationModeFlags.Exact)
			return null;

		var output = new List<Point>(8);
		if (userData.DestinationModeFlags.HasFlag(DestinationModeFlags.CardinalNeighbor))
		{
			output.Add(new Point(endPosition.X - 1, endPosition.Y));
			output.Add(new Point(endPosition.X, endPosition.Y - 1));
			output.Add(new Point(endPosition.X + 1, endPosition.Y));
			output.Add(new Point(endPosition.X, endPosition.Y + 1));
		}

		if (userData.DestinationModeFlags.HasFlag(DestinationModeFlags.IntercardinalNeighbor))
		{
			output.Add(new Point(endPosition.X - 1, endPosition.Y - 1));
			output.Add(new Point(endPosition.X + 1, endPosition.Y - 1));
			output.Add(new Point(endPosition.X + 1, endPosition.Y + 1));
			output.Add(new Point(endPosition.X - 1, endPosition.Y + 1));
		}

		return output.Count == 0 ? null : output.ToArray();
	}

	/// <summary>
	///     Processes the node.
	/// </summary>
	/// <param name="currentNode">The current node.</param>
	/// <param name="columnOffset">The column offset.</param>
	/// <param name="rowOffset">The row offset.</param>
	/// <param name="openNodes">The list of open nodes that will be added to as nodes are processed.</param>
	/// <param name="closedNodes">The closed nodes.</param>
	/// <param name="endPosition">The end position.</param>
	/// <param name="userData">The user data.</param>
	/// <returns>A new node positioned next to the current node based on columnOffset and rowOffset.</returns>
	private PathTreeNode ProcessNode(PathTreeNode currentNode, int columnOffset, int rowOffset, ICollection<PathTreeNode> openNodes, IEnumerable<PathTreeNode> closedNodes, Point endPosition, FindPathData userData)
	{
		var position = new Point(currentNode.Position.X + columnOffset, currentNode.Position.Y + rowOffset);
		var weight = currentNode.Weight + userData?.GetWeight(position, endPosition) ?? 0;
		var newNode = new PathTreeNode(position, currentNode, weight);
		if ((CheckNode != null && !CheckNode(newNode.Position.X, newNode.Position.Y, userData)) || AnyNodeIsAtPoint(closedNodes, newNode.Position) || AnyNodeIsAtPoint(openNodes, newNode.Position))
			return null;

		openNodes.Add(newNode);
		return newNode;
	}
	#endregion
}