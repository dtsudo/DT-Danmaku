
namespace DTDanmakuLib
{
	using System;
	using DTLib;

	public class TitleScreenFrame : IFrame<IDTDanmakuAssets>
	{
		private int fps;
		private IDTDeterministicRandom rng;
		private GuidGenerator guidGenerator;

		private SoundVolumePicker soundVolumePicker;

		private bool debugMode;

		public TitleScreenFrame(
			int fps,
			IDTDeterministicRandom rng,
			GuidGenerator guidGenerator,
			int? soundVolume,
			bool debugMode)
		{
			this.fps = fps;
			this.rng = rng;
			this.guidGenerator = guidGenerator;
			
			this.soundVolumePicker = soundVolume.HasValue
				? new SoundVolumePicker(xPos: 0, yPos: 650, initialVolume: soundVolume.Value)
				: new SoundVolumePicker(xPos: 0, yPos: 650);

			this.debugMode = debugMode;
		}

		public IFrame<IDTDanmakuAssets> GetNextFrame(
			IKeyboard keyboardInput,
			IMouse mouseInput,
			IKeyboard previousKeyboardInput,
			IMouse previousMouseInput,
			IDisplay<IDTDanmakuAssets> display)
		{
			this.soundVolumePicker.ProcessFrame(
				mouseInput: mouseInput,
				previousMouseInput: previousMouseInput);

			if (keyboardInput.IsPressed(Key.Enter) && !previousKeyboardInput.IsPressed(Key.Enter))
				return new InstructionScreenFrame(fps: this.fps, rng: this.rng, guidGenerator: this.guidGenerator, soundVolume: this.soundVolumePicker.GetCurrentSoundVolume(), debugMode: this.debugMode);

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

			display.GetAssets().DrawImage(DTDanmakuImage.TitleScreen, 317, 239);

			display.GetAssets().DrawImage(DTDanmakuImage.Version, 1000 - ((int)display.GetAssets().GetWidth(DTDanmakuImage.Version)) - 5, 700 - ((int)display.GetAssets().GetHeight(DTDanmakuImage.Version)) - 5);

			this.soundVolumePicker.Render(display: display);
		}
	}
}
