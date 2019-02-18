using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using PolyPath;

namespace ExampleAdventure.Core
{
	public class Entity
	{
		private float _moveTimeElapsed = 0f;
		private float _moveDelay = 0.25f;

		public int Column { get; set; }
		public int Row { get; set; }
		public Path Path { get; set; } = new Path();

		public void Update(float delta)
		{
			if (Path.NextWaypoint != null)
			{
				_moveTimeElapsed += delta;
				if (_moveTimeElapsed >= _moveDelay)
				{
					var target = Path.NextWaypoint.Value;
					Column = (int)target.X;
					Row = (int)target.Y;
					Path.PopWaypoint();
					_moveTimeElapsed = 0f;
				}
			}
			else
			{
				_moveTimeElapsed = 0f;
			}
		}
}
}
