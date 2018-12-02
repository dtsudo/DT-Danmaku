
namespace DTDanmakuLib
{
	using System;
	using DTLib;

	public class GameOverFrame : IFrame<IDTDanmakuAssets>
	{
		private int fps;
		private IDTDeterministicRandom rng;
		private GuidGenerator guidGenerator;
		private int soundVolume;
		private bool debugMode;

		public GameOverFrame(int fps, IDTDeterministicRandom rng, GuidGenerator guidGenerator, int soundVolume, bool debugMode)
		{
			this.fps = fps;
			this.rng = rng;
			this.guidGenerator = guidGenerator;
			this.soundVolume = soundVolume;
			this.debugMode = debugMode;
		}

		public IFrame<IDTDanmakuAssets> GetNextFrame(IKeyboard keyboardInput, IMouse mouseInput, IKeyboard previousKeyboardInput, IMouse previousMouseInput, IDisplay<IDTDanmakuAssets> display)
		{
			if (keyboardInput.IsPressed(Key.Enter) && !previousKeyboardInput.IsPressed(Key.Enter))
				return new TitleScreenFrame(fps: fps, rng: this.rng, guidGenerator: this.guidGenerator, soundVolume: this.soundVolume, debugMode: this.debugMode);

			return this;
		}

		public void Render(IDisplay<IDTDanmakuAssets> display)
		{
			// Draw background
			display.DrawRectangle(
				x: 0,
				y: 0,
				width: 1000,
				height: 700,
				color: new DTColor(r: 132, g: 245, b: 255), // teal
				fill: true);

			int width = (int)display.GetAssets().GetWidth(DTDanmakuImage.GameOver);
			int height = (int)display.GetAssets().GetHeight(DTDanmakuImage.GameOver);
			display.GetAssets().DrawImage(DTDanmakuImage.GameOver, 1000 / 2 - width / 2, 700 / 2 - height / 2);
		}
	}
}
