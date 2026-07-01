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

using Microsoft.Xna.Framework;

namespace PolyPath.Tests;

public sealed class PathingPolygonTests
{
	#region Methods
	[Fact]
	public void CreateGridStoresBoundsOriginAndNodeSize()
	{
		var polygon = CreateRectanglePolygon(10, 20, 58, 68);

		Assert.Equal(new Rectangle(10, 20, 48, 48), polygon.Bounds);
		Assert.Equal(new Point(10, 20), polygon.Origin);
		Assert.Equal(16, polygon.NodeWidth);
		Assert.Equal(16, polygon.NodeHeight);
	}

	[Fact]
	public void GetNodeAtXYCalculatesNodeFromWorldPosition()
	{
		var polygon = CreateRectanglePolygon(10, 20, 58, 68);

		var node = polygon.GetNodeAtXY(27, 37);

		Assert.Equal(1, node.Column);
		Assert.Equal(1, node.Row);
		Assert.True(node.IsPathable);
	}

	[Fact]
	public void TryGetNodeAtXYReturnsFalseForOutOfBoundsPosition()
	{
		var polygon = CreateRectanglePolygon(10, 20, 58, 68);

		var found = polygon.TryGetNodeAtXY(9, 20, out var node);

		Assert.False(found);
		Assert.Equal(-1, node.Column);
		Assert.Equal(-1, node.Row);
		Assert.False(node.IsPathable);
	}

	private static PathingPolygon CreateRectanglePolygon(int left, int top, int right, int bottom)
	{
		var polygon = new PathingPolygon
		{
			UseTightTests = true
		};
		polygon.Points.Add(new Point(left, top));
		polygon.Points.Add(new Point(right, top));
		polygon.Points.Add(new Point(right, bottom));
		polygon.Points.Add(new Point(left, bottom));
		polygon.Close();
		polygon.CreateGrid(16, 16);
		return polygon;
	}
	#endregion
}