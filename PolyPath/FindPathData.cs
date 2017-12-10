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
***********************************************************************/
#endregion

using Microsoft.Xna.Framework;

namespace PolyPath
{
	/// <summary>
	/// </summary>
	public class FindPathData
	{
		#region Properties
		public bool PopFirstWaypoint { get; set; }
		public int PopLastNWaypoints { get; set; }
		#endregion

		#region Methods
		/// <summary>
		/// Pops the waypoint test.
		/// </summary>
		/// <param name="waypointPosition">The waypoint position.</param>
		/// <param name="index">The zero-based index of the waypoint in the path.</param>
		/// <returns><c>true</c> if the waypoint should be popped; <c>false</c> otherwise.</returns>
		public virtual bool PopWaypointTest(Point waypointPosition, int index)
		{
			return false;
		}

		/// <summary>
		/// Gets the weight.
		/// </summary>
		/// <param name="waypointPosition">The waypoint position.</param>
		/// <param name="index">The zero-based index of the waypoint in the path.</param>
		/// <returns>The numeric weight of the node at the position.</returns>
		public virtual int GetWeight(Point waypointPosition, int index)
		{
			return 1;
		}
		#endregion
	}
}