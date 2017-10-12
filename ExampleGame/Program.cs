#region Using Statements
using System;
#endregion

namespace ExampleGame
{
#if WINDOWS || LINUX
	/// <summary>
	///     The main class.
	/// </summary>
	public static class Program
	{
		#region Methods
		/// <summary>
		///     The main entry point for the application.
		/// </summary>
		[STAThread]
		private static void Main()
		{
			using (var game = new GameEngine())
			{
				game.Run();
			}
		}
		#endregion
	}
#endif
}