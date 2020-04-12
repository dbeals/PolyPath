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
using System.Linq;
using Microsoft.Xna.Framework;

namespace ExampleAdventure.Core
{
	public static class MapGenerator
	{
		#region Methods
		public static Map GenerateMap(Random random, int width, int height, int maximumNumberOfRooms = 100)
		{
			var output = new Map(width, height);

			for (var row = 0; row < output.Height; ++row)
			{
				for (var column = 0; column < output.Width; ++column)
					output[column, row].Material = Material.None;
			}

			if (maximumNumberOfRooms > 0)
				GenerateRooms(output, random, maximumNumberOfRooms);
			return output;
		}

		private static void ConnectRooms(Random random, Room initialRoom, Room newRoom, Direction direction)
		{
			var room1Bounds = initialRoom.Bounds;
			var room2Bounds = newRoom.Bounds;

			var newRoom1Bounds = new Rectangle(room1Bounds.Left + 1, room1Bounds.Top + 1, room1Bounds.Width - 2, room1Bounds.Height - 2);
			var newRoom2Bounds = new Rectangle(room2Bounds.Left + 1, room2Bounds.Top + 1, room2Bounds.Width - 2, room2Bounds.Height - 2);

			switch (direction)
			{
				case Direction.North:
				{
					var left = Math.Max(newRoom1Bounds.Left, newRoom2Bounds.Left);
					var right = Math.Min(newRoom1Bounds.Right, newRoom2Bounds.Right);
					if (right - left <= 0)
						return;

					var doorColumn = random.Next(left, right);
					initialRoom.Doorways.Add(new Point(doorColumn - initialRoom.Column, 0));
					newRoom.Doorways.Add(new Point(doorColumn - newRoom.Column, newRoom.Height - 1));
					break;
				}

				case Direction.East:
				{
					var top = Math.Max(newRoom1Bounds.Top, newRoom2Bounds.Top);
					var bottom = Math.Min(newRoom1Bounds.Bottom, newRoom2Bounds.Bottom);
					if (bottom - top <= 0)
						return;

					var doorRow = random.Next(top, bottom);
					initialRoom.Doorways.Add(new Point(initialRoom.Width - 1, doorRow - initialRoom.Row));
					newRoom.Doorways.Add(new Point(0, doorRow - newRoom.Row));
					break;
				}

				case Direction.South:
				{
					var left = Math.Max(newRoom1Bounds.Left, newRoom2Bounds.Left);
					var right = Math.Min(newRoom1Bounds.Right, newRoom2Bounds.Right);
					if (right - left <= 0)
						return;

					var doorColumn = random.Next(left, right);
					initialRoom.Doorways.Add(new Point(doorColumn - initialRoom.Column, initialRoom.Height - 1));
					newRoom.Doorways.Add(new Point(doorColumn - newRoom.Column, 0));
					break;
				}

				case Direction.West:
				{
					var top = Math.Max(newRoom1Bounds.Top, newRoom2Bounds.Top);
					var bottom = Math.Min(newRoom1Bounds.Bottom, newRoom2Bounds.Bottom);
					if (bottom - top <= 0)
						return;

					var doorRow = random.Next(top, bottom);
					initialRoom.Doorways.Add(new Point(0, doorRow - initialRoom.Row));
					newRoom.Doorways.Add(new Point(newRoom.Width - 1, doorRow - newRoom.Row));
					break;
				}

				default: throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
			}
		}

		private static Room GenerateRoom(Random random, Room startingRoom, Direction direction, int maxWidth, int maxHeight)
		{
			var output = new Room
			{
				Width = random.Next(5, maxWidth),
				Height = random.Next(5, maxHeight)
			};

			var startingRoomBounds = startingRoom.Bounds;
			var column = random.Next(startingRoomBounds.Left + 1, startingRoomBounds.Right - 2);
			var row = random.Next(startingRoomBounds.Top + 1, startingRoomBounds.Bottom - 2);

			switch (direction)
			{
				case Direction.North:
				{
					row = startingRoomBounds.Top - output.Height;
					break;
				}

				case Direction.East:
				{
					column = startingRoomBounds.Right;
					break;
				}

				case Direction.South:
				{
					row = startingRoomBounds.Bottom;
					break;
				}

				case Direction.West:
				{
					column = startingRoomBounds.Left - output.Width;
					break;
				}
			}

			output.Column = column;
			output.Row = row;

			return output;
		}

		private static Room AddRoom(Random random, Map map, Room initialRoom, Direction direction, int maxWidth, int maxHeight)
		{
			var room = GenerateRoom(random, initialRoom, direction, maxWidth, maxHeight);

			var roomBounds = room.Bounds;
			var mapBounds = new Rectangle(0, 0, map.Width, map.Height);
			var newBounds = Rectangle.Intersect(roomBounds, mapBounds);

			room.Column = newBounds.Left;
			room.Row = newBounds.Top;
			room.Width = newBounds.Width;
			room.Height = newBounds.Height;
			if (room.Width < 3 || room.Height < 3)
				return null;

			if (map.Rooms.Any(room1 => room.Bounds.Intersects(room1.Bounds)))
				return null;

			if ((random.Next() + 10) % 2 == 0)
			{
				var waterColumn = random.Next(room.Column + 1, room.Column + room.Width - 1);
				var waterRow = random.Next(room.Row + 1, room.Row + room.Height - 1);
				var useWaterPoint = true;
				foreach (var doorway in room.Doorways)
				{
					if (waterColumn == doorway.X && waterRow == doorway.Y)
					{
						useWaterPoint = false;
						break;
					}

					if (waterColumn == doorway.X - 1 && waterRow == doorway.Y)
					{
						useWaterPoint = false;
						break;
					}

					if (waterColumn == doorway.X + 1 && waterRow == doorway.Y)
					{
						useWaterPoint = false;
						break;
					}

					if (waterColumn == doorway.X && waterRow == doorway.Y - 1)
					{
						useWaterPoint = false;
						break;
					}

					if (waterColumn == doorway.X && waterRow == doorway.Y + 1)
					{
						useWaterPoint = false;
						break;
					}
				}

				if (useWaterPoint)
					room.WaterPoint = new Point(waterColumn, waterRow);
			}

			//ConnectRooms(random, initialRoom, room, direction);
			map.Rooms.Add(room);
			return room;
		}

		private static void GenerateRooms(Map map, Random random, int maximumNumberOfRooms)
		{
			var initialRoomWidth = random.Next(5, 10);
			var initialRoomHeight = random.Next(5, 10);
			var initialRoomColumn = (map.Width - initialRoomWidth) / 2;
			var initialRoomRow = (map.Height - initialRoomHeight) / 2;

			var initialRoom = new Room
			{
				Column = initialRoomColumn,
				Row = initialRoomRow,
				Width = initialRoomWidth,
				Height = initialRoomHeight
			};
			map.Rooms.Add(initialRoom);

			var roomCount = random.Next(4, maximumNumberOfRooms);
			for (var index = 0; index < roomCount; ++index)
			{
				var direction = (Direction) random.Next(0, 4);
				var newRoom = AddRoom(random, map, initialRoom, direction, 10, 10);
				if (newRoom == null)
					continue;

				initialRoom = map.Rooms[random.Next(0, map.Rooms.Count)];
			}

			for (var index = 0; index < map.Rooms.Count; index++)
			{
				var room1 = map.Rooms[index];
				for (var indexB = index + 1; indexB < map.Rooms.Count; indexB++)
				{
					var room2 = map.Rooms[indexB];
					if (room1 == room2)
						continue;
					if (!RoomsTouch(room1, room2))
						continue;
					ConnectRooms(random, room1, room2, GetDirectionBetweenRooms(room1, room2));
				}
			}

			foreach (var room in map.Rooms)
				ApplyRoom(map, room);
		}

		private static Direction GetDirectionBetweenRooms(Room room1, Room room2)
		{
			var room1Bounds = room1.Bounds;
			var room2Bounds = room2.Bounds;

			var room1Center = room1Bounds.Center.ToVector2();
			var room2Center = room2Bounds.Center.ToVector2();
			var direction = room2Center - room1Center;

			var x = Math.Abs(direction.X);
			var y = Math.Abs(direction.Y);

			if (x > y)
				return direction.X <= 0 ? Direction.West : Direction.East;
			return direction.Y <= 0 ? Direction.North : Direction.South;
		}

		private static bool RoomsTouch(Room room1, Room room2)
		{
			var room1Bounds = room1.Bounds;
			var room2Bounds = room2.Bounds;
			var union = Rectangle.Union(room1Bounds, room2Bounds);

			return union.Width <= room1Bounds.Width + room2Bounds.Width && union.Height <= room1Bounds.Height + room2Bounds.Height;
		}

		private static void ApplyRoom(Map map, Room room)
		{
			for (var row = 0; row < room.Height; ++row)
			{
				for (var column = 0; column < room.Width; ++column)
				{
					var material = Material.Gravel;
					if (row == 0 || row == room.Height - 1 || column == 0 || column == room.Width - 1)
						material = Material.Wall;

					map[column + room.Column, row + room.Row].Material = material;
				}
			}

			foreach (var doorway in room.Doorways)
				map[doorway.X + room.Column, doorway.Y + room.Row].Material = Material.Gravel;

			if (room.WaterPoint != null)
			{
				var point = room.WaterPoint.Value;
				map[point.X, point.Y].Material = Material.Water;
			}
		}
		#endregion
	}
}