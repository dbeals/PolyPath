#region File Header
/***********************************************************************
This is free and unencumbered software released into the public domain.

Anyone is free to copy, modify, publish, use, compile, sell, or
distribute this software, either in source code form or as a compiled
binary, for any purpose, commercial or non-commercial, and by any
means.

In jurisdictions that recognize copyright laws, the author or authors
of this software dedicate any and all copyright interest in the
software to the public domain. We make this dedication for the benefit
of the public at large and to the detriment of our heirs and
successors. We intend this dedication to be an overt act of
relinquishment in perpetuity of all present and future rights to this
software under copyright law.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.

For more information, please refer to <http://unlicense.org>
************************************************************************
Author: Donald Beals
Description: The Path class, which contains the generated path as a set
 * of X/Y coordinates.
****************************** Change Log ******************************
4/26/2015 3:49:04 PM - Created initial file. (dbeals)
***********************************************************************/
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
#endregion

namespace PolyPath
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class Path
	{
		#region Variables
		private List<Vector2> waypoints = null;
		#endregion

		#region Properties
		public Vector2[] Waypoints
		{
			get
			{
				return waypoints.ToArray();
			}
			set
			{
				waypoints = new List<Vector2>(value == null ? new Vector2[0] : value);
			}
		}

		public Vector2? NextWaypoint
		{
			get
			{
				return waypoints.Count == 0 ? (Vector2?)null : waypoints.First();
			}
		}

		public Vector2? LastWaypoint
		{
			get
			{
				return waypoints.Count == 0 ? (Vector2?)null : waypoints.Last();
			}
		}

		public int Length
		{
			get
			{
				return waypoints.Count;
			}
		}

		public int Depth
		{
			get;
			set;
		}
		#endregion

		#region Constructors
		public Path()
		{
			waypoints = new List<Vector2>();
		}
		#endregion

		#region Methods
		public Vector2 GetDistanceVectorToNextWaypoint(Vector2 position)
		{
			return NextWaypoint == null ? Vector2.Zero : NextWaypoint.Value - position;
		}

		public Vector2 GetDirectionVectorToNextWaypoint(Vector2 position)
		{
			return NextWaypoint == null ? Vector2.Zero : Vector2.Normalize(NextWaypoint.Value - position);
		}

		public void AddWaypoint(float x, float y)
		{
			AddWaypoint(new Vector2(x, y));
		}

		public void AddWaypoint(Vector2 waypoint)
		{
			waypoints.Add(waypoint);
		}

		public void AddWaypoints(IEnumerable<Vector2> newWaypoints)
		{
			waypoints.AddRange(newWaypoints);
		}

		public Vector2 PopWaypoint()
		{
			if(waypoints == null || waypoints.Count == 0)
				return Vector2.Zero;
			var output = waypoints.First();
			waypoints.RemoveAt(0);
			return output;
		}

		public void Clear()
		{
			waypoints.Clear();
		}

		public void DebugDraw(Action<Point, Point, int> drawLine)
		{
			if(drawLine == null)
				return;

			for(var index = 0; index < waypoints.Count - 1; ++index)
			{
				var current = waypoints[index];
				var next = waypoints[index + 1];
				drawLine(current.ToPoint(), next.ToPoint(), index);
			}
		}
		#endregion
	}
}
