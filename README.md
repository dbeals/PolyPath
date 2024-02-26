# PolyPath

**Please note that this library is still in it's early stages and has not been used with any projects. As such, it may go through a few major changes as we develop it.**

PolyPath is designed for use in a non-grid based game. You provide a polygon and the system generates a grid inside of that, which is then used to generate a path (using the A* algorithm.)

# Finding our way
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
pathfinder.Processors.Add(new TrimPathProcessor());
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

# Using the new FindPathData
We have added a new class named FindPathData. This is considered a base class, but it can be used by itself. It has two properties: PopFirstWaypoint and PopLastNWaypoints and two methods: PopWaypointTest() and GetWeight.

**PopWaypointTest**

The system iterates backward over the points in the original path and offers the chance to verify that the node should be included. This affords the option to make sure that, while pathable, this is a point to be included in the final path (i.e. making sure that a character stays far enough away from a trap without setting it off when they have clicked on the trap.)

This process is done before any other trimming or popping is done.

An example usage for this is pathing to an object/character. Pathfinder.CheckNode would check node bounds against all objects except for the player and their target (if you check the target then you won't get a path as the node is blocked.) Then PopWaypointTest() will check nodes against the target's bounds. This will cause the system to path to the target and then trim the path to the closest node outside of their collision box.

**PopFirstWaypoint**

PopFirstWaypoint is an item that should most likely be set to true, otherwise your mover will move to the center of the node they are standing on before moving to the next waypoint.

The first node is popped BEFORE trimming is done.

**PopLastNWaypoints**

PopLastNWaypoints is meant to be used when moving an object to another object. Instead of trying to move them to a certain point within a range of the target object, simply have the system pop the last 2 or 3 waypoints off (depending on the size of the grid you're using; with our current project we're using 16 pixel nodes and popping 2 still seemed too close, so we pop 3.)

The nodes are popped BEFORE trimming is done.

**DestinationModeFlags**

DestinationModeFlags is used to determine how the pathfinder handles the destination. The default value is DestinationModeFlags.All, meaning that the pathfinder will try to navigate to the destination, but if it fails on CheckNode then it will fall back to the neighbor with the shortest path.

**StartPosition**

The start position provided to FindPath(). FindPath() populates this so that PopWayPointTest(), CheckNode(), etc. have access to the start position if needed.

**EndPosition**

The end position provided to FindPath(). FindPath() populates this so that PopWayPointTest(), CheckNode(), etc. have access to the end position if needed.

**GetWeight**

GetWeight takes a node position (grid indices, not X/Y) and an end position (again, grid indices). The game can then calculate a weight for that specific node. Check out the PathfinderUserData.cs file in the ExampleAdventure project for an example of calculating the weight. *Please note that the weight of the previous node is added to this value during pathfinding*.