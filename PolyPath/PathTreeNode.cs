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
	internal sealed class PathTreeNode
	{
		#region Variables
		private Point _position;
		#endregion

		#region Properties
		public Point Position
		{
			get => _position;
			set => _position = value;
		}

		public int Column
		{
			get => _position.X;
			set => _position.X = value;
		}

		public int Row
		{
			get => _position.Y;
			set => _position.Y = value;
		}

		public bool IsInvalid => Column == -1 && Row == -1;

		public PathTreeNode Parent { get; set; }
		public int Weight { get; set; }
		#endregion

		#region Constructors
		public PathTreeNode()
		{
		}

		public PathTreeNode(Point position, PathTreeNode parent, int weight)
		{
			Position = position;
			Parent = parent;
			Weight = weight;
		}

		public PathTreeNode(int column, int row, PathTreeNode parent, int weight)
			: this(new Point(column, row), parent, weight)
		{
		}
		#endregion

		#region Methods
		public override string ToString()
		{
			return $"X:{Position.X} Y:{Position.Y} Weight:{Weight} Parent:{(Parent == null ? "null" : "PathTreeNode")}";
		}
		#endregion
	}
}