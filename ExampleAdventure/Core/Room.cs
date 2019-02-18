using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace ExampleAdventure.Core
{
	public class Room
	{
		public int Column { get; set; }
		public int Row { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public Rectangle Bounds => new Rectangle(Column, Row, Width, Height);
		public List<Point> Doorways { get; } = new List<Point>();
		public Point? WaterPoint { get; set; }

		public Rectangle GetPixelBounds(int tileWidth, int tileHeight) => new Rectangle(Column * tileWidth, Row * tileHeight, Width * tileWidth, Height * tileHeight);
	}
}
