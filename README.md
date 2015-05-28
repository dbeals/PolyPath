#PolyPath
PolyPath is designed for use in a non-grid based game. You provide a polygon and the system generates a grid inside of that, which is then used to generate a path (using the A* algorithm.)

#Finding our way
The first step is to generate the pathing grid. We do so by adding points to the polygon, closing it and then creating the grid:

```CSharp
var pathingPolygon = new PolyPath.PathingPolygon();
pathingPolygon.UseTightTests = true;
// Create a cube
pathingPolygon.Points.Add(new Point(10, 10));
pathingPolygon.Points.Add(new Point(300, 10));
pathingPolygon.Points.Add(new Point(300, 300));
pathingPolygon.Points.Add(new Point(10, 300));
pathingPolygon.Close();
pathingPolygon.CreateGrid(16, 16);
```

We can now create our pathfinder and find the path:

```CSharp
var startPoint = new Point(30, 90);
var endPoint = new Point(200, 25);

var startNode = pathingPolygon.GetNodeAtXY(startPoint);
if(startNode == null || !startNode.IsPathable)
	return;

var endNode = pathingPolygon.GetNodeAtXY(endPoint);
if(endNode == null || !endPoint.IsPathable)
	return;
	
var pathfinder = new Pathfinder();
pathfinder.TrimPaths = true;
var path = pathfinder.FindPath(startNode.Column, startNode.Row, endNode.Column, endNode.Row, pathingPolygon);
// Note that you can also omit the pathingPolygon in this call and you'll receive a list of Points that are grid coordinates.
// You can then convert that to a path using the pathingPolygon class (this is what  the override used above does.)
```

We now have a set of waypoints and can move along them:

```CSharp
// delta is the frame-time delta in seconds.
// path is the value at the end of the previous example.
if(path.Length == 0)
	return;
	
if(path.GetDistanceVectorToNextWaypoint(PlayerObject.Position).Length < WereCloseEnoughToMoveToTheNextWaypointConstant))
	path.PopWaypoint();

if(path.NextWaypoint != null)
{
	PlayerObject.Position += (path.GetDirectionVectorToNextWaypoint() * delta);
}
```

#To-do
There are still a few things that need to be added:

**Weighting**
The algorithm does not currently incorporate weights.

#Change Log

+ 2015-05-27 (dbeals) - Added PopWaypointTest to FindPathData class to allow trimming of waypoints after a path has been found.