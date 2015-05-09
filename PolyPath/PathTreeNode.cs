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
Description: The PathTreeNode class, which is used when generating the
 * actual path.
****************************** Change Log ******************************
4/26/2015 3:50:08 PM - Created initial file. (dbeals)
***********************************************************************/
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
#endregion

namespace PolyPath
{
	/// <summary>
	/// 
	/// </summary>
	internal sealed class PathTreeNode
	{
		#region Properties
		public Point Position
		{
			get;
			set;
		}

		public PathTreeNode Parent
		{
			get;
			set;
		}

		public int Weight
		{
			get;
			set;
		}
		#endregion

		#region Constructors
		public PathTreeNode()
		{
		}

		public PathTreeNode(Point position, PathTreeNode parent, int weight)
		{
			this.Position = position;
			this.Parent = parent;
			this.Weight = weight;
		}

		public PathTreeNode(int column, int row, PathTreeNode parent, int weight)
			: this(new Point(column, row), parent, weight)
		{
		}
		#endregion

		#region Methods
		public override string ToString()
		{
			return string.Format("X:{0} Y:{1} Weight:{2} Parent:{3}", Position.X, Position.Y, Weight, Parent == null ? "null" : "PathTreeNode");
		}
		#endregion
	};
}
