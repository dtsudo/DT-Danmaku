
namespace DTLib
{
	/// <summary>
	/// An interface that defines how to render the display of a frame.
	/// </summary>
	public interface IDisplay<T> where T : class, IAssets
	{
		/// <summary>
		/// Renders a rectangle on the screen, with the top-left corner occurring
		/// at (x, y), and with the specified width and height.
		/// 
		/// Also takes in a color, indicating what color the rectangle should be, as well
		/// as a boolean (fill) indicating whether the rectangle should be filled in.
		/// </summary>
		void DrawRectangle(int x, int y, int width, int height, DTColor color, bool fill);

		/// <summary>
		/// Renders text for debugging.
		/// 
		/// The behavior of this method is not specified (should only be used for debugging).
		/// </summary>
		void DebugPrint(int x, int y, string debugText);

		T GetAssets();
	}

	public static class DisplayExtensions
	{
		public static void DrawThickRectangle<T>(this IDisplay<T> display, int x, int y, int width, int height, int additionalThickness, DTColor color, bool fill) where T : class, IAssets
		{
			display.DrawRectangle(x - additionalThickness, y - additionalThickness, width + additionalThickness * 2, 1 + additionalThickness * 2, color, true);
			display.DrawRectangle(x - additionalThickness, height - 1 + y - additionalThickness, width + additionalThickness * 2, 1 + additionalThickness * 2, color, true);
			display.DrawRectangle(x - additionalThickness, y - additionalThickness, 1 + additionalThickness * 2, height + additionalThickness * 2, color, true);
			display.DrawRectangle(width - 1 + x - additionalThickness, y - additionalThickness, 1 + additionalThickness * 2, height + additionalThickness * 2, color, true);

			if (fill)
				display.DrawRectangle(x, y, width, height, color, true);
		}
	}
}
