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

using Microsoft.Xna.Framework;
using PolyPath.Processors;

namespace PolyPath.Tests;

public sealed class PathfinderTests
{
	#region Nested types
	private sealed class WeightedPathData(IReadOnlyDictionary<Point, int> weights) : FindPathData
	{
		#region Methods
		public override int GetWeight(Point nodePosition, Point endPosition) => weights.GetValueOrDefault(nodePosition, 1);
		#endregion
	}

	private sealed class WeightedMovementPathData : FindPathData
	{
		#region Methods
		public override int GetMovementWeight(Point currentPosition, Point nodePosition)
		{
			var columnDistance = Math.Abs(currentPosition.X - nodePosition.X);
			var rowDistance = Math.Abs(currentPosition.Y - nodePosition.Y);
			return columnDistance == 1 && rowDistance == 1 ? 10 : 1;
		}
		#endregion
	}
	#endregion

	#region Methods
	[Fact]
	public void FindPathReturnsPathWhenDestinationIsReachable()
	{
		var pathfinder = CreateBoundedPathfinder(3, 1);
		var path = pathfinder.FindPath(0, 0, 2, 0, out var depth, ExactPathData());

		Assert.Equal(3, depth);
		Assert.Equal([new Point(0, 0), new Point(1, 0), new Point(2, 0)], path);
	}

	[Fact]
	public void FindPathReturnsEmptyPathWhenExactDestinationCannotBeReached()
	{
		var pathfinder = CreateBoundedPathfinder(3, 1, blockedNodes: [new Point(1, 0)]);
		var path = pathfinder.FindPath(0, 0, 2, 0, out var depth, ExactPathData());

		Assert.Equal(0, depth);
		Assert.Empty(path);
	}

	[Fact]
	public void FindPathFallsBackToReachableDestinationNeighbor()
	{
		var pathfinder = CreateBoundedPathfinder(3, 1, blockedNodes: [new Point(2, 0)]);
		var path = pathfinder.FindPath(0, 0, 2, 0, out var depth, new FindPathData());

		Assert.Equal(2, depth);
		Assert.Equal([new Point(0, 0), new Point(1, 0)], path);
	}

	[Fact]
	public void FindPathAppliesWaypointPoppingBeforeProcessors()
	{
		var pathfinder = CreateBoundedPathfinder(4, 1);
		var userData = ExactPathData();
		userData.PopFirstWaypoint = true;
		userData.PopLastNWaypoints = 1;

		var path = pathfinder.FindPath(0, 0, 3, 0, out var depth, userData);

		Assert.Equal(2, depth);
		Assert.Equal([new Point(1, 0), new Point(2, 0)], path);
	}

	[Fact]
	public void TrimPathProcessorRemovesIntermediateStraightLinePoints()
	{
		var processor = new TrimPathProcessor();
		var path = new List<Point>
		{
			new (0, 0),
			new (1, 0),
			new (2, 0),
			new (2, 1)
		};

		var trimmed = processor.Process(path, path.ToArray());

		Assert.Equal([new Point(0, 0), new Point(2, 0), new Point(2, 1)], trimmed);
	}

	[Fact]
	public void FindPathUpdatesPreviouslyDiscoveredNodeWhenCheaperRouteIsFound()
	{
		var userData = new WeightedPathData(new Dictionary<Point, int>
		{
			[new Point(1, 0)] = 20,
			[new Point(1, 1)] = 1,
			[new Point(2, 1)] = 1,
			[new Point(2, 0)] = 1
		});
		var pathfinder = CreateBoundedPathfinder(3, 2);

		var path = pathfinder.FindPath(0, 0, 2, 0, out _, userData);

		Assert.Equal([new Point(0, 0), new Point(1, 1), new Point(2, 1), new Point(2, 0)], path);
	}

	[Fact]
	public void FindPathUsesMovementWeightWhenChoosingBetweenCardinalAndDiagonalRoutes()
	{
		var pathfinder = CreateBoundedPathfinder(3, 2);
		var path = pathfinder.FindPath(0, 0, 2, 0, out _, new WeightedMovementPathData());

		Assert.Equal([new Point(0, 0), new Point(1, 0), new Point(2, 0)], path);
	}

	private static Pathfinder CreateBoundedPathfinder(int width, int height, IReadOnlyCollection<Point>? blockedNodes = null)
	{
		var blocked = blockedNodes?.ToHashSet() ?? [];
		return new Pathfinder
		{
			CheckNode = (column, row, _) =>
				column >= 0 &&
				column < width &&
				row >= 0 &&
				row < height &&
				!blocked.Contains(new Point(column, row))
		};
	}

	private static FindPathData ExactPathData() =>
		new ()
		{
			DestinationModeFlags = DestinationModeFlags.Exact
		};
	#endregion
}