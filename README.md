# PolyPath

PolyPath is a small pathfinding library for games that want to define walkable space with polygons, then path over a
generated grid inside that space. It is still early and the public API may continue to change as the library is refined.

The current search is a weighted grid search using a priority queue. It supports 8-way movement, custom node and
movement weights, dynamic validity checks, optional waypoint processing, and conversion from grid paths to world-space
waypoints.

## Basic Usage

Create a polygon, close it, and generate a pathing grid:

```csharp
using Microsoft.Xna.Framework;
using PolyPath;

var pathingPolygon = new PathingPolygon
{
	UseTightTests = true
};

pathingPolygon.Points.Add(new Point(10, 10));
pathingPolygon.Points.Add(new Point(300, 10));
pathingPolygon.Points.Add(new Point(300, 300));
pathingPolygon.Points.Add(new Point(10, 300));
pathingPolygon.Close();
pathingPolygon.CreateGrid(16, 16);
```

Convert world positions to grid nodes and find a path:

```csharp
using PolyPath.Processors;

var startPoint = new Point(30, 90);
var endPoint = new Point(200, 25);

var startNode = pathingPolygon.GetNodeAtXY(startPoint);
if (!startNode.IsPathable)
	return;

var endNode = pathingPolygon.GetNodeAtXY(endPoint);
if (!endNode.IsPathable)
	return;

var pathfinder = new Pathfinder
{
	PathingGrid = pathingPolygon
};

pathfinder.Processors.Add(new TrimPathProcessor());

var userData = new FindPathData
{
	PopFirstWaypoint = true
};

var path = pathfinder.FindPath(startNode.Column, startNode.Row, endNode.Column, endNode.Row, pathingPolygon, userData);
```

Move along the returned waypoints:

```csharp
if (path.Length == 0)
	return;

if (path.GetDistanceVectorToNextWaypoint(PlayerObject.Position).Length < closeEnough)
	path.PopWaypoint();

if (path.NextWaypoint != null)
	PlayerObject.Position += path.GetDirectionVectorToNextWaypoint(PlayerObject.Position) * delta;
```

`WaypointPath.PopWaypoint()` advances an internal cursor; it does not shift the backing list on every pop. `Waypoints`
returns the remaining waypoints, while `AllWaypoints` exposes the full stored path.

## Pathing Grids

`IPathingGrid` is the pathfinder's grid contract:

```csharp
public interface IPathingGrid
{
	bool ContainsColumnRow(int column, int row);
	PathingGridNode GetNodeAtColumnRow(int column, int row);
	bool IsPathable(int column, int row);
	bool IsPathable(Point point);
}
```

`PathingPolygon` implements `IPathingGrid`. It stores the generated `Bounds`, `Origin`, `NodeWidth`, `NodeHeight`,
`Width`, `Height`, and `Nodes`.

World-position lookup is available through:

```csharp
var node = pathingPolygon.GetNodeAtXY(x, y);

if (pathingPolygon.TryGetNodeAtXY(x, y, out var foundNode))
{
	// foundNode is inside the generated grid.
}
```

The `Get...` methods preserve the older sentinel behavior and return an invalid `PathingGridNode` with `Column == -1`
and `Row == -1` when no node is found. The `TryGet...` methods are preferred when callers need to distinguish
out-of-bounds lookup from blocked-but-existing nodes.

## Dynamic Path Rules

Use `Pathfinder.PathingGrid` for static grid validity and `Pathfinder.CheckNode` for dynamic overlays such as units,
doors, temporary blockers, or game-specific rules:

```csharp
var occupied = new HashSet<Point>
{
	new (5, 7)
};

var pathfinder = new Pathfinder
{
	PathingGrid = pathingPolygon,
	CheckNode = (column, row, userData) => !occupied.Contains(new Point(column, row))
};
```

Processors such as smoothing receive the same validity rules, so they should not bypass blocked nodes or dynamic
`CheckNode` restrictions.

## FindPathData

`FindPathData` carries per-search options and extension points.

### DestinationModeFlags

`DestinationModeFlags` determines whether the search can stop on the exact destination, a cardinal neighbor, an
intercardinal neighbor, or any combination. The default is `DestinationModeFlags.All`.

### PopWaypointTest

`PopWaypointTest(Point waypointPosition, int index)` is called from the end of the path toward the start before
first/last waypoint popping and processors run. Return `true` to remove trailing waypoints.

This is useful when pathing to an object where the object itself is pathable for targeting, but the mover should stop
just outside the object's occupied area.

### PopFirstWaypoint

When `PopFirstWaypoint` is `true`, the starting node is removed from the final path. This usually prevents a mover from
first walking to the center of the node it is already standing in.

### PopLastNWaypoints

`PopLastNWaypoints` removes a fixed number of waypoints from the end of the path before processors run. This is useful
for stopping short of a target object.

### GetMovementWeight

`GetMovementWeight(Point currentPosition, Point nodePosition)` returns the base cost of stepping from one grid node to
another. The default is `1`.

Override this to make diagonal movement more expensive, penalize turns, or model movement modes:

```csharp
public override int GetMovementWeight(Point currentPosition, Point nodePosition)
{
	var dx = Math.Abs(currentPosition.X - nodePosition.X);
	var dy = Math.Abs(currentPosition.Y - nodePosition.Y);
	return dx == 1 && dy == 1 ? 14 : 10;
}
```

### GetWeight

`GetWeight(Point nodePosition, Point endPosition)` returns extra cost for entering a node. The default is `0`.

Override this for terrain, danger, or preference maps:

```csharp
public override int GetWeight(Point nodePosition, Point endPosition)
{
	return dangerMap.TryGetValue(nodePosition, out var danger) ? danger : 0;
}
```

## Processors

Processors run after the raw grid path is found and after `PopWaypointTest`, `PopFirstWaypoint`, and `PopLastNWaypoints`
are applied.

### TrimPathProcessor

`TrimPathProcessor` removes intermediate points that continue in the same horizontal, vertical, or diagonal direction.

```csharp
pathfinder.Processors.Add(new TrimPathProcessor());
```

The BasicExample toggles trimming with `Space`.

### SmoothPathProcessor

`SmoothPathProcessor` is opt-in. It tries to replace path segments with direct grid-line shortcuts. It checks the same
pathing grid, dynamic `CheckNode` overlay, and movement/node costs that the pathfinder used.

```csharp
pathfinder.Processors.Add(new SmoothPathProcessor());
```

Smoothing will not pass through blocked nodes, dynamic blockers, diagonal corner cuts, or shortcuts that cost more than
the segment they replace. The BasicExample toggles smoothing with `Enter`.

Processor order matters. A common setup is:

```csharp
pathfinder.Processors.Add(new TrimPathProcessor());
pathfinder.Processors.Add(new SmoothPathProcessor());
```

## Post-Processors

`FindPath(..., out depth, userData)` returns grid coordinates as `Point[]`.

The overloads that accept an `IPathingGrid` or `PathingPolygon` return a `WaypointPath` and use an `IPathPostProcessor`
to convert grid points to `Vector3` waypoints.

- `DefaultPathPostProcessor` converts grid nodes to world-space node centers.
- `DirectPathPostProcessor` converts grid coordinates directly to `Vector3(x, y, 0)`.

## Examples

`BasicExample` demonstrates polygon editing, grid generation, pathfinding, weighted nodes, trimming, and smoothing.

Useful controls:

- `Left Click`: add polygon points, choose start/end nodes after closing the polygon
- `Right Click`: clear or reset
- `Space`: enable/disable trimming
- `Enter`: enable/disable smoothing
- `Tab`: enable/disable tight polygon tests
- `Shift + Left Click`: increase node weight
- `Ctrl + Left Click`: decrease node weight
- `F1`: show/hide help

`ExampleAdventure` demonstrates a more game-like integration. It paths over a tile map, rejects blocked materials and occupied entity cells through `CheckNode`, and uses a custom `FindPathData` implementation to apply material-based weights. It also uses `DirectPathPostProcessor` because its entities move in grid coordinates rather than world-space node centers.
