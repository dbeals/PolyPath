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

public sealed class SmoothPathProcessorTests
{
	#region Nested types
	private sealed class TestPathingGrid(int width, int height, IReadOnlyCollection<Point>? blockedNodes = null) : IPathingGrid
	{
		#region Variables
		private readonly HashSet<Point> _blockedNodes = blockedNodes?.ToHashSet() ?? [];
		#endregion

		#region Methods
		public bool ContainsColumnRow(int column, int row) => column >= 0 && column < width && row >= 0 && row < height;
		public PathingGridNode GetNodeAtColumnRow(int column, int row) => new (column, row, new Rectangle(column, row, 1, 1), IsPathable(column, row));
		public bool IsPathable(int column, int row) => ContainsColumnRow(column, row) && !_blockedNodes.Contains(new Point(column, row));
		public bool IsPathable(Point point) => IsPathable(point.X, point.Y);
		#endregion
	}

	private sealed class WeightedPathData(IReadOnlyDictionary<Point, int> weights) : FindPathData
	{
		#region Methods
		public override int GetWeight(Point nodePosition, Point endPosition) => weights.GetValueOrDefault(nodePosition, 0);
		#endregion
	}
	#endregion

	#region Methods
	[Fact]
	public void ToggleSmoothingAddsAndRemovesSmoothPathProcessor()
	{
		var pathfinder = new Pathfinder();

		Assert.True(SmoothPathProcessor.ToggleSmoothing(pathfinder));
		Assert.True(SmoothPathProcessor.IsSmoothingPaths(pathfinder));
		Assert.False(SmoothPathProcessor.ToggleSmoothing(pathfinder));
		Assert.False(SmoothPathProcessor.IsSmoothingPaths(pathfinder));
	}

	[Fact]
	public void ProcessShortensPathWhenDirectGridLineIsClear()
	{
		var processor = new SmoothPathProcessor();
		var path = new List<Point>
		{
			new (0, 0),
			new (0, 1),
			new (1, 1),
			new (2, 1),
			new (3, 1)
		};

		var smoothed = processor.Process(path, CreateContext(new TestPathingGrid(4, 2)));

		Assert.Equal([new Point(0, 0), new Point(3, 1)], smoothed);
	}

	[Fact]
	public void ProcessDoesNotSmoothThroughBlockedNode()
	{
		var processor = new SmoothPathProcessor();
		var path = new List<Point>
		{
			new (0, 0),
			new (0, 1),
			new (1, 1),
			new (2, 1),
			new (2, 0)
		};

		var smoothed = processor.Process(path, CreateContext(new TestPathingGrid(3, 2, blockedNodes: [new Point(1, 0)])));

		Assert.Equal([new Point(0, 0), new Point(0, 1), new Point(2, 1), new Point(2, 0)], smoothed);
	}

	[Fact]
	public void ProcessDoesNotSmoothThroughHigherCostDangerNodes()
	{
		var processor = new SmoothPathProcessor();
		var path = new List<Point>
		{
			new (0, 0),
			new (0, 1),
			new (1, 1),
			new (2, 1),
			new (3, 1),
			new (3, 0)
		};
		var weights = new Dictionary<Point, int>
		{
			[new Point(1, 0)] = 100,
			[new Point(2, 0)] = 100
		};

		var smoothed = processor.Process(path, CreateContext(new TestPathingGrid(4, 2), new WeightedPathData(weights)));

		Assert.Equal([new Point(0, 0), new Point(0, 1), new Point(3, 1), new Point(3, 0)], smoothed);
	}

	[Fact]
	public void FindPathAppliesSmoothPathProcessorWithoutBypassingCheckNodeOverlay()
	{
		var pathfinder = new Pathfinder
		{
			PathingGrid = new TestPathingGrid(3, 2),
			CheckNode = (column, row, _) => column != 1 || row != 0
		};
		pathfinder.Processors.Add(new SmoothPathProcessor());

		var path = pathfinder.FindPath(0, 0, 2, 0, out _, ExactPathData());

		Assert.Equal([new Point(0, 0), new Point(0, 1), new Point(2, 1), new Point(2, 0)], path);
	}

	private static PathProcessorContext CreateContext(IPathingGrid pathingGrid, FindPathData? userData = null) =>
		new ()
		{
			GetNodeWeight = point => userData?.GetWeight(point, Point.Zero) ?? 0,
			GetStepWeight = (from, to) => (userData?.GetMovementWeight(from, to) ?? 1) + (userData?.GetWeight(to, Point.Zero) ?? 0),
			IsNodePathable = pathingGrid.IsPathable,
			PathingGrid = pathingGrid
		};

	private static FindPathData ExactPathData() =>
		new ()
		{
			DestinationModeFlags = DestinationModeFlags.Exact
		};
	#endregion
}