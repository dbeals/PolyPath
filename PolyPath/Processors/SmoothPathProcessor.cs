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

namespace PolyPath.Processors;

public sealed class SmoothPathProcessor : IPathProcessor
{
	#region Methods
	/// <summary>
	///     Helper method to determine if paths are being smoothed in a pathfinder.
	/// </summary>
	/// <param name="pathfinder">The pathfinder to check.</param>
	/// <returns>
	///     <c>true</c> if paths are being smoothed; otherwise, <c>false</c>.
	/// </returns>
	public static bool IsSmoothingPaths(Pathfinder pathfinder) => pathfinder.Processors.Any(x => x is SmoothPathProcessor);

	/// <summary>
	///     Helper method to toggle smoothing of paths in a pathfinder.
	/// </summary>
	/// <param name="pathfinder">The pathfinder to toggle smoothing in.</param>
	/// <returns>
	///     <c>true</c> if paths are now being smoothed; otherwise, <c>false</c>.
	/// </returns>
	public static bool ToggleSmoothing(Pathfinder pathfinder)
	{
		var processor = pathfinder.Processors.FirstOrDefault(x => x is SmoothPathProcessor);
		if (processor != null)
		{
			pathfinder.Processors.Remove(processor);
			return false;
		}

		pathfinder.Processors.Add(new SmoothPathProcessor());
		return true;
	}

	public List<Point> Process(List<Point> input, Point[] initialWaypoints) =>
		Process(input, new PathProcessorContext
		{
			InitialWaypoints = initialWaypoints
		});

	public List<Point> Process(List<Point> input, PathProcessorContext context)
	{
		if (input is not { Count: > 2 })
			return input;

		var output = new List<Point>
		{
			input[0]
		};
		var currentIndex = 0;
		while (currentIndex < input.Count - 1)
		{
			var nextIndex = currentIndex + 1;
			for (var testIndex = input.Count - 1; testIndex > currentIndex + 1; --testIndex)
			{
				if (!CanTravelDirectly(input[currentIndex], input[testIndex], GetPathCost(input, currentIndex, testIndex, context), context))
					continue;

				nextIndex = testIndex;
				break;
			}

			output.Add(input[nextIndex]);
			currentIndex = nextIndex;
		}

		return output;
	}

	private static bool CanTravelDirectly(Point start, Point end, int maxCost, PathProcessorContext context)
	{
		var cost = 0;
		var previous = start;
		foreach (var point in GetLineCells(start, end))
		{
			if (point == start)
				continue;

			if (!context.CanEnter(point))
				return false;

			var columnOffset = point.X - previous.X;
			var rowOffset = point.Y - previous.Y;
			if (Math.Abs(columnOffset) == 1 && Math.Abs(rowOffset) == 1)
			{
				var horizontalNeighbor = new Point(previous.X + columnOffset, previous.Y);
				var verticalNeighbor = new Point(previous.X, previous.Y + rowOffset);
				if (!context.CanEnter(horizontalNeighbor) || !context.CanEnter(verticalNeighbor))
					return false;

				cost += context.GetExtraWeight(horizontalNeighbor) + context.GetExtraWeight(verticalNeighbor);
			}

			cost += context.GetWeight(previous, point);
			if (cost > maxCost)
				return false;

			previous = point;
		}

		return true;
	}

	private static int GetPathCost(IReadOnlyList<Point> path, int startIndex, int endIndex, PathProcessorContext context)
	{
		var output = 0;
		for (var index = startIndex + 1; index <= endIndex; ++index)
			output += context.GetWeight(path[index - 1], path[index]);
		return output;
	}

	private static IEnumerable<Point> GetLineCells(Point start, Point end)
	{
		var columnDistance = end.X - start.X;
		var rowDistance = end.Y - start.Y;
		var steps = Math.Max(Math.Abs(columnDistance), Math.Abs(rowDistance));
		if (steps == 0)
		{
			yield return start;
			yield break;
		}

		for (var step = 0; step <= steps; ++step)
		{
			var x = start.X + (int)Math.Round(columnDistance * (step / (double)steps), MidpointRounding.AwayFromZero);
			var y = start.Y + (int)Math.Round(rowDistance * (step / (double)steps), MidpointRounding.AwayFromZero);
			yield return new Point(x, y);
		}
	}
	#endregion
}