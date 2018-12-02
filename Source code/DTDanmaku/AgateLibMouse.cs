
namespace DTDanmaku
{
	using DTLib;
	using AgateLib.InputLib;

	public class AgateLibMouse : IMouse
	{
		public AgateLibMouse()
		{
		}

		public int GetX()
		{
			return Mouse.X;
		}

		public int GetY()
		{
			return Mouse.Y;
		}

		public bool IsLeftMouseButtonPressed()
		{
			return Mouse.Buttons[Mouse.MouseButtons.Primary];
		}

		public bool IsRightMouseButtonPressed()
		{
			return Mouse.Buttons[Mouse.MouseButtons.Secondary];
		}
	}
}
