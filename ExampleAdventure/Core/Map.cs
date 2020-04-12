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

using System.Collections.Generic;

namespace ExampleAdventure.Core
{
	public class Map
	{
		#region Variables
		private readonly MapNode[] _nodes;
		#endregion

		#region Properties
		public int Width { get; }
		public int Height { get; }

		public List<Room> Rooms { get; } = new List<Room>();

		public MapNode this[int column, int row]
		{
			get
			{
				var index = row * Width + column;
				return _nodes[index] ?? (_nodes[index] = new MapNode());
			}
		}
		#endregion

		#region Constructors
		public Map(int width, int height)
		{
			Width = width;
			Height = height;
			_nodes = new MapNode[width * height];
		}
		#endregion

		#region Methods
		public Room GetRoomAt(int column, int row)
		{
			foreach (var room in Rooms)
			{
				if (room.Bounds.Contains(column, row))
					return room;
			}

			return null;
		}

		public bool IsPassable(int column, int row)
		{
			if (IsOutOfBounds(column, row))
				return false;

			var node = this[column, row];
			if (node.Material == Material.None)
				return false;
			if (node.Material == Material.Wall)
				return false;
			if (node.Material == Material.Water)
				return false;

			return true;
		}

		public bool Contains(int column, int row) => !IsOutOfBounds(column, row);
		public bool IsOutOfBounds(int column, int row) => column < 0 || row < 0 || column >= Width || row >= Height;
		#endregion
	}
}