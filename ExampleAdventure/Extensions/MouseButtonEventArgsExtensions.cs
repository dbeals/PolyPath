using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExamplesCore.Input;
using Microsoft.Xna.Framework;

namespace ExampleAdventure.Extensions
{
	public static class MouseButtonEventArgsExtensions
	{
		public static Point GetMouseColumnRow(this MouseButtonEventArgs eventArgs, int tileWidth, int tileHeight)
		{
			return new Point(eventArgs.X / tileWidth, eventArgs.Y / tileHeight);
		}
	}

	public static class MouseMoveEventArgsExtensions
	{
		public static Point GetMouseColumnRow(this MouseMoveEventArgs eventArgs, int tileWidth, int tileHeight)
		{
			return new Point(eventArgs.X / tileWidth, eventArgs.Y / tileHeight);
		}
	}
}
