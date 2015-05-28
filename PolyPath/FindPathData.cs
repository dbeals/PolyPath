#region File Header
/***********************************************************************
 * Copyright © 2015 Beals Software
 * All Rights Reserved
************************************************************************
Author: Donald Beals
Description: TODO: Write a description of this file here.
****************************** Change Log ******************************
5/10/2015 11:18:22 PM - Created initial file. (dbeals)
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
	public class FindPathData
	{
		#region Properties
		public bool PopFirstWaypoint
		{
			get;
			set;
		}

		public int PopLastNWaypoints
		{
			get;
			set;
		}
		#endregion

		#region Constructors
		public FindPathData()
		{
		}
		#endregion

		#region Methods
		public virtual bool PopWaypointTest(Point nodePosition, int index)
		{
			return false;
		}
		#endregion
	}
}
