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
using Microsoft.Xna.Framework;
using PolyPath;

namespace ExampleAdventure.Core
{
	public class PathfinderUserData : FindPathData
	{
		#region Properties
		public Map Map { get; set; }
		public Entity Entity { get; set; }
		#endregion

		#region Constructors
		public PathfinderUserData(Map map, Entity entity)
		{
			Map = map;
			Entity = entity;
		}
		#endregion

		#region Methods
		public override int GetWeight(Point waypointPosition, Point endPosition)
		{
			var output = 0;
			var startPoint = waypointPosition.ToVector2();
			var endPoint = endPosition.ToVector2();
			while (startPoint != endPoint)
			{
				var node = Map[(int) startPoint.X, (int) startPoint.Y];
				var material = node.Material;
				output += (int) material;
				if (material == Material.None)
					output += 10000;
				else if (material == Material.Wall)
					output += 1000;
				else if (material == Material.Water)
					output += 100;

				var direction = Vector2.Normalize(endPoint - startPoint);
				startPoint += direction;
				startPoint = new Vector2((int) Math.Round(startPoint.X), (int) Math.Round(startPoint.Y));
			}

			return output;
		}
		#endregion
	}
}