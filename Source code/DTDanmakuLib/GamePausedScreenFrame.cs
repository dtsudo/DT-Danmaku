namespace DTDanmakuLib
{
	using DTLib;
	using System;
	
	public class GamePausedScreenFrame : IFrame<IDTDanmakuAssets>
	{
		private int fps;
		private IDTDeterministicRandom rng;
		private GuidGenerator guidGenerator;

		private SoundVolumePicker soundVolumePicker;

		private bool debugMode;

		private GameInProgressFrame gameInProgressFrame;
		private bool isContinueSelected; // as opposed to "quit" being selected

		public GamePausedScreenFrame(GameInProgressFrame frame, int fps, IDTDeterministicRandom rng, GuidGenerator guidGenerator, int soundVolume, bool debugMode)
		{
			this.fps = fps;
			this.rng = rng;
			this.guidGenerator = guidGenerator;

			this.soundVolumePicker = new SoundVolumePicker(xPos: 0, yPos: 650, initialVolume: soundVolume);

			this.gameInProgressFrame = frame;
			this.isContinueSelected = true;

			this.debugMode = debugMode;
		}
		
		public IFrame<IDTDanmakuAssets> GetNextFrame(
			IKeyboard keyboardInput,
			IMouse mouseInput,
			IKeyboard previousKeyboardInput,
			IMouse previousMouseInput,
			IDisplay<IDTDanmakuAssets> display)
		{
			this.soundVolumePicker.ProcessFrame(mouseInput: mouseInput, previousMouseInput: previousMouseInput);

			if (keyboardInput.IsPressed(Key.DownArrow) && !previousKeyboardInput.IsPressed(Key.DownArrow))
			{
				this.isContinueSelected = false;
			}
			if (keyboardInput.IsPressed(Key.UpArrow) && !previousKeyboardInput.IsPressed(Key.UpArrow))
			{
				this.isContinueSelected = true;
			}

			bool executeAction = keyboardInput.IsPressed(Key.Z) && !previousKeyboardInput.IsPressed(Key.Z)
				|| keyboardInput.IsPressed(Key.Enter) && !previousKeyboardInput.IsPressed(Key.Enter)
				|| keyboardInput.IsPressed(Key.Space) && !previousKeyboardInput.IsPressed(Key.Space);

			if (executeAction)
			{
				if (this.isContinueSelected)
				{
					this.gameInProgressFrame.SetSoundVolume(soundVolume: this.soundVolumePicker.GetCurrentSoundVolume());
					return this.gameInProgressFrame;
				}
				else
					return new TitleScreenFrame(fps: fps, rng: this.rng, guidGenerator: this.guidGenerator, soundVolume: this.soundVolumePicker.GetCurrentSoundVolume(), debugMode: this.debugMode);
			}

			return this;
		}

		public void Render(IDisplay<IDTDanmakuAssets> display)
		{
			this.gameInProgressFrame.Render(display: display);

			display.DrawRectangle(
				x: 0,
				y: 0,
				width: 1000,
				height: 700,
				color: new DTColor(r: 0, g: 0, b: 0, alpha: 64),
				fill: true);

			display.GetAssets().DrawImage(DTDanmakuImage.Pause, x: 436, y: 200);
			
			display.GetAssets().DrawImage(DTDanmakuImage.Continue, x: 420, y: 400);
			display.GetAssets().DrawImage(DTDanmakuImage.Quit, x: 462, y: 500);

			if (this.isContinueSelected)
			{
				display.DrawThickRectangle(
					x: 410,
					y: 390,
					width: 180,
					height: 20 + 34,
					additionalThickness: 2,
					color: new DTColor(r: 0, g: 0, b: 255),
					fill: false);
			}
			else
			{
				display.DrawThickRectangle(
					x: 410,
					y: 490,
					width: 180,
					height: 20 + 34,
					additionalThickness: 2,
					color: new DTColor(r: 0, g: 0, b: 255),
					fill: false);
			}

			this.soundVolumePicker.Render(display: display);
		}
	}
}
