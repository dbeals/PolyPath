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

using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace PolyPath.Processors;

public class TrimPathProcessor : IPathProcessor
{
	#region Methods
	/// <summary>
	///     Helper method to determine if paths are being trimmed in a pathfinder.
	/// </summary>
	/// <param name="pathfinder">The pathfinder to check.</param>
	/// <returns>
	///     <c>true</c> if paths are being trimmed; otherwise, <c>false</c>.
	/// </returns>
	public static bool IsTrimmingPaths(Pathfinder pathfinder) => pathfinder.Processors.Any(x => x is TrimPathProcessor);

	public List<Point> Process(List<Point> input, Point[] initialWaypoints)
	{
		var indicesToRemove = new List<int>();
		for (var index = 1; index < input.Count - 1; ++index)
		{
			var previousPoint = input[index - 1];
			var currentPoint = input[index];
			var nextPoint = input[index + 1];

			if (PointsContinueHorizontally(previousPoint, currentPoint, nextPoint) || PointsContinuesVertically(previousPoint, currentPoint, nextPoint) || PointsContinueDiagonally(previousPoint, currentPoint, nextPoint))
				indicesToRemove.Add(index);
		}

		for (var index = indicesToRemove.Count - 1; index >= 0; --index)
			input.RemoveAt(indicesToRemove[index]);

		return input;
	}

	/// <summary>
	///     Helper method to toggle trimming of paths in a pathfinder.
	/// </summary>
	/// <param name="pathfinder">The pathfinder to toggle trimming in.</param>
	/// <returns>
	///     <c>true</c> if paths are now being trimmed; otherwise, <c>false</c>.
	/// </returns>
	public static bool ToggleTrimming(Pathfinder pathfinder)
	{
		var processor = pathfinder.Processors.FirstOrDefault(x => x is TrimPathProcessor);
		if (processor != null)
		{
			pathfinder.Processors.Remove(processor);
			return false;
		}

		pathfinder.Processors.Add(new TrimPathProcessor());
		return true;
	}

	/// <summary>
	///     Determines whether or not three points are diagonally next to each other.
	/// </summary>
	/// <param name="previousPoint">The previous point.</param>
	/// <param name="currentPoint">The current point.</param>
	/// <param name="nextPoint">The next point.</param>
	/// <param name="xOffset">The x offset.</param>
	/// <param name="yOffset">The y offset.</param>
	/// <returns>
	///     <c>true</c> if all three points are vertically next to each other; otherwise, <c>false</c>.
	/// </returns>
	private static bool PointsContinueDiagonally(Point previousPoint, Point currentPoint, Point nextPoint, int xOffset, int yOffset) => currentPoint.X + xOffset == nextPoint.X && currentPoint.Y + yOffset == nextPoint.Y && currentPoint.X + -xOffset == previousPoint.X && currentPoint.Y + -yOffset == previousPoint.Y;

	/// <summary>
	///     Determines whether or not three points are diagonally next to each other.
	/// </summary>
	/// <param name="previousPoint">The previous point.</param>
	/// <param name="currentPoint">The current point.</param>
	/// <param name="nextPoint">The next point.</param>
	/// <returns>
	///     <c>true</c> if all three points are vertically next to each other; otherwise, <c>false</c>.
	/// </returns>
	private static bool PointsContinueDiagonally(Point previousPoint, Point currentPoint, Point nextPoint) =>
		PointsContinueDiagonally(previousPoint, currentPoint, nextPoint, 1, -1) ||
		PointsContinueDiagonally(previousPoint, currentPoint, nextPoint, 1, 1) ||
		PointsContinueDiagonally(previousPoint, currentPoint, nextPoint, -1, 1) ||
		PointsContinueDiagonally(previousPoint, currentPoint, nextPoint, -1, -1);

	/// <summary>
	///     Determines whether or not three points are horizontally next to each other.
	/// </summary>
	/// <param name="previousPoint">The previous point.</param>
	/// <param name="currentPoint">The current point.</param>
	/// <param name="nextPoint">The next point.</param>
	/// <returns>
	///     <c>true</c> if all three points are horizontally next to each other; otherwise, <c>false</c>.
	/// </returns>
	private static bool PointsContinueHorizontally(Point previousPoint, Point currentPoint, Point nextPoint) => currentPoint.Y == nextPoint.Y && nextPoint.Y == previousPoint.Y && currentPoint.X != nextPoint.X;

	/// <summary>
	///     Determines whether or not three points are vertically next to each other.
	/// </summary>
	/// <param name="previousPoint">The previous point.</param>
	/// <param name="currentPoint">The current point.</param>
	/// <param name="nextPoint">The next point.</param>
	/// <returns>
	///     <c>true</c> if all three points are vertically next to each other; otherwise, <c>false</c>.
	/// </returns>
	private static bool PointsContinuesVertically(Point previousPoint, Point currentPoint, Point nextPoint) => currentPoint.X == nextPoint.X && nextPoint.X == previousPoint.X && currentPoint.Y != nextPoint.Y;
	#endregion
}