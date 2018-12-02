

namespace DTDanmakuLib
{
	using DTLib;

	public class TranslatedDTDanmakuDisplay
	{
		private IDisplay<IDTDanmakuAssets> display;
		private int xOffset;
		private int yOffset;

		public TranslatedDTDanmakuDisplay(IDisplay<IDTDanmakuAssets> display, int xOffset, int yOffset)
		{
			this.display = display;
			this.xOffset = xOffset;
			this.yOffset = yOffset;
		}

		public void DrawRectangle(int x, int y, int width, int height, DTColor color, bool fill)
		{
			this.display.DrawRectangle(x + this.xOffset, y + this.yOffset, width, height, color, fill);
		}

		public void DrawThickRectangle(int x, int y, int width, int height, int additionalThickness, DTColor color, bool fill)
		{
			this.display.DrawThickRectangle(x + this.xOffset, y + this.yOffset, width, height, additionalThickness, color, fill);
		}

		public void DrawImage(DTDanmakuImage image, int x, int y)
		{
			this.display.GetAssets().DrawImage(image, x + this.xOffset, y + this.yOffset);
		}

		public void DrawImageRotatedCounterclockwise(DTDanmakuImage image, int x, int y, int milliDegrees)
		{
			this.display.GetAssets().DrawImageRotatedCounterclockwise(image, x + this.xOffset, y + this.yOffset, milliDegrees);
		}

		public void DrawImageRotatedClockwise(DTDanmakuImage image, int x, int y, int milliDegrees)
		{
			this.DrawImageRotatedCounterclockwise(image: image, x: x, y: y, milliDegrees: -milliDegrees);
		}

		public long GetWidth(DTDanmakuImage image)
		{
			return this.display.GetAssets().GetWidth(image: image);
		}

		public long GetHeight(DTDanmakuImage image)
		{
			return this.display.GetAssets().GetHeight(image: image);
		}

		/// <summary>
		/// Volume ranges from 0 to 100 (both inclusive)
		/// </summary>
		public void PlaySound(DTDanmakuSound sound, int volume)
		{
			this.display.GetAssets().PlaySound(sound: sound, volume: volume);
		}

		/// <summary>
		/// Renders text for debugging.
		/// 
		/// The behavior of this method is not specified (should only be used for debugging).
		/// </summary>
		public void DebugPrint(int x, int y, string debugText)
		{
			this.display.DebugPrint(x: x + this.xOffset, y: y + this.yOffset, debugText: debugText);
		}
	}
}

