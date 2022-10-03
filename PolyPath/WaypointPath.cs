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

namespace PolyPath;

public sealed class WaypointPath
{
	#region Variables
	private List<Vector3> _waypoints = new ();
	#endregion

	#region Properties
	public Vector3? LastWaypoint => _waypoints.Count == 0 ? null : _waypoints.Last();
	public int Length => _waypoints.Count;

	public Vector3? NextWaypoint => _waypoints.Count == 0 ? null : _waypoints.First();
	public int Depth { get; set; }

	public Vector3[] Waypoints
	{
		get => _waypoints.ToArray();
		set => _waypoints = new List<Vector3>(value ?? Array.Empty<Vector3>());
	}
	#endregion

	#region Methods
	/// <summary>
	///     Adds the waypoint.
	/// </summary>
	/// <param name="x">The x-axis value of the waypoint.</param>
	/// <param name="y">The y-axis value of the waypoint.</param>
	/// <param name="z">The z-axis value of the waypoint.</param>
	public void AddWaypoint(float x, float y, float z) => AddWaypoint(new Vector3(x, y, z));

	/// <summary>
	///     Adds the waypoint.
	/// </summary>
	/// <param name="waypoint">The waypoint.</param>
	/// <param name="z">The z-axis value of the waypoint.</param>
	public void AddWaypoint(Vector2 waypoint, float z) => _waypoints.Add(new Vector3(waypoint, z));

	/// <summary>
	///     Adds the waypoint.
	/// </summary>
	/// <param name="waypoint">The waypoint.</param>
	public void AddWaypoint(Vector3 waypoint) => _waypoints.Add(waypoint);

	/// <summary>
	///     Adds the waypoints.
	/// </summary>
	/// <param name="newWaypoints">The new waypoints.</param>
	public void AddWaypoints(IEnumerable<Vector3> newWaypoints) => _waypoints.AddRange(newWaypoints);

	/// <summary>
	///     Adds the waypoints.
	/// </summary>
	/// <param name="newWaypoints">The new waypoints.</param>
	public void AddWaypoints(params Vector3[] newWaypoints) => _waypoints.AddRange(newWaypoints);

	/// <summary>
	///     Clears all of the waypoints.
	/// </summary>
	public void Clear() => _waypoints.Clear();

	/// <summary>
	///     Draw the path for debug-mode.
	/// </summary>
	/// <param name="drawLine">A callback that draws the line.</param>
	public void DebugDraw(Action<Vector3, Vector3, int> drawLine)
	{
		if (drawLine == null)
			return;

		for (var index = 0; index < _waypoints.Count - 1; ++index)
		{
			var current = _waypoints[index];
			var next = _waypoints[index + 1];
			drawLine(current, next, index);
		}
	}

	/// <summary>
	///     Gets the direction vector to next waypoint.
	/// </summary>
	/// <param name="position">The position to calculate the direction from.</param>
	/// <returns></returns>
	public Vector3 GetDirectionVectorToNextWaypoint(Vector3 position) => NextWaypoint == null ? Vector3.Zero : Vector3.Normalize(NextWaypoint.Value - position);

	/// <summary>
	///     Gets the distance vector to next waypoint.
	/// </summary>
	/// <param name="position">The position to calculate the distance from.</param>
	/// <returns></returns>
	public Vector3 GetDistanceVectorToNextWaypoint(Vector3 position) => NextWaypoint == null ? Vector3.Zero : NextWaypoint.Value - position;

	/// <summary>
	///     Pops the last waypoint.
	/// </summary>
	/// <returns></returns>
	public Vector3 PopWaypoint()
	{
		if (_waypoints == null || _waypoints.Count == 0)
			return Vector3.Zero;
		var output = _waypoints.First();
		_waypoints.RemoveAt(0);
		return output;
	}
	#endregion
}