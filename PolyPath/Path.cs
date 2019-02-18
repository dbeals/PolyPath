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

namespace PolyPath
{
	public sealed class Path
	{
		#region Variables
		private List<Vector2> _waypoints;
		#endregion

		#region Properties
		public Vector2[] Waypoints
		{
			get => _waypoints.ToArray();
			set => _waypoints = new List<Vector2>(value ?? new Vector2[0]);
		}

		public Vector2? NextWaypoint => _waypoints.Count == 0 ? (Vector2?) null : _waypoints.First();
		public Vector2? LastWaypoint => _waypoints.Count == 0 ? (Vector2?) null : _waypoints.Last();
		public int Length => _waypoints.Count;
		public int Depth { get; set; }
		#endregion

		#region Constructors
		public Path() => _waypoints = new List<Vector2>();
		#endregion

		#region Methods
		/// <summary>
		///     Gets the distance vector to next waypoint.
		/// </summary>
		/// <param name="position">The position to calculate the distance from.</param>
		/// <returns></returns>
		public Vector2 GetDistanceVectorToNextWaypoint(Vector2 position) => NextWaypoint == null ? Vector2.Zero : NextWaypoint.Value - position;

		/// <summary>
		///     Gets the direction vector to next waypoint.
		/// </summary>
		/// <param name="position">The position to calculate the direction from.</param>
		/// <returns></returns>
		public Vector2 GetDirectionVectorToNextWaypoint(Vector2 position) => NextWaypoint == null ? Vector2.Zero : Vector2.Normalize(NextWaypoint.Value - position);

		/// <summary>
		///     Adds the waypoint.
		/// </summary>
		/// <param name="x">The x.</param>
		/// <param name="y">The y.</param>
		public void AddWaypoint(float x, float y)
		{
			AddWaypoint(new Vector2(x, y));
		}

		/// <summary>
		///     Adds the waypoint.
		/// </summary>
		/// <param name="waypoint">The waypoint.</param>
		public void AddWaypoint(Vector2 waypoint)
		{
			_waypoints.Add(waypoint);
		}

		/// <summary>
		///     Adds the waypoints.
		/// </summary>
		/// <param name="newWaypoints">The new waypoints.</param>
		public void AddWaypoints(IEnumerable<Vector2> newWaypoints)
		{
			_waypoints.AddRange(newWaypoints);
		}

		/// <summary>
		///     Adds the waypoints.
		/// </summary>
		/// <param name="newWaypoints">The new waypoints.</param>
		public void AddWaypoints(params Vector2[] newWaypoints)
		{
			_waypoints.AddRange(newWaypoints);
		}

		/// <summary>
		///     Pops the last waypoint.
		/// </summary>
		/// <returns></returns>
		public Vector2 PopWaypoint()
		{
			if (_waypoints == null || _waypoints.Count == 0)
				return Vector2.Zero;
			var output = _waypoints.First();
			_waypoints.RemoveAt(0);
			return output;
		}

		/// <summary>
		///     Clears all of the waypoints.
		/// </summary>
		public void Clear()
		{
			_waypoints.Clear();
		}

		/// <summary>
		///     Debugs the draw.
		/// </summary>
		/// <param name="drawLine">A callback that draws the line.</param>
		public void DebugDraw(Action<Point, Point, int> drawLine)
		{
			if (drawLine == null)
				return;

			for (var index = 0; index < _waypoints.Count - 1; ++index)
			{
				var current = _waypoints[index];
				var next = _waypoints[index + 1];
				drawLine(current.ToPoint(), next.ToPoint(), index);
			}
		}
		#endregion
	}
}