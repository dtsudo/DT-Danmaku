
namespace DTDanmakuLib
{
	using System;
	using DTLib;

	public class GameInProgressFrame : IFrame<IDTDanmakuAssets>
	{
		private int fps;
		private IDTDeterministicRandom rng;
		private GuidGenerator guidGenerator;
		private GameLogic gameLogic;
		private bool debugMode;

		public GameInProgressFrame(
			int fps,
			IDTDeterministicRandom rng,
			GuidGenerator guidGenerator,
			int soundVolume,
			bool debugMode)
		{
			this.fps = fps;
			this.rng = rng;
			this.guidGenerator = guidGenerator;
			this.gameLogic = new GameLogic(
				fps: fps,
				rng: rng,
				guidGenerator: guidGenerator,
				soundVolume: soundVolume,
				debugMode: debugMode);

			this.debugMode = debugMode;
		}

		public void SetSoundVolume(int soundVolume)
		{
			this.gameLogic.SetSoundVolume(soundVolume: soundVolume);
		}

		public IFrame<IDTDanmakuAssets> GetNextFrame(IKeyboard keyboardInput, IMouse mouseInput, IKeyboard previousKeyboardInput, IMouse previousMouseInput, IDisplay<IDTDanmakuAssets> display)
		{
			/*
				Note that since these debug things bypass regular game logic, they may break other stuff or crash the program
				(Should be used for debugging / development only)
			*/
			if (this.debugMode)
			{
				if (keyboardInput.IsPressed(Key.Seven))
				{
					for (int i = 0; i < 6; i++)
					{
						this.gameLogic = this.gameLogic.GetNextFrame(new EmptyKeyboard(), new EmptyMouse(), new EmptyKeyboard(), new EmptyMouse());
					}

					if (keyboardInput.IsPressed(Key.Q))
					{
						for (int i = 0; i < 27; i++)
						{
							this.gameLogic = this.gameLogic.GetNextFrame(new EmptyKeyboard(), new EmptyMouse(), new EmptyKeyboard(), new EmptyMouse());
						}
					}
				}
			}
			
			this.gameLogic = this.gameLogic.GetNextFrame(keyboardInput, mouseInput, previousKeyboardInput, previousMouseInput);

			if (this.gameLogic.IsGameOver())
				return new GameOverFrame(fps: fps, rng: this.rng, guidGenerator: this.guidGenerator, soundVolume: this.gameLogic.GetSoundVolume(), debugMode: this.debugMode);

			if (this.gameLogic.HasPlayerWon())
				return new YouWinFrame(fps: fps, rng: this.rng, guidGenerator: this.guidGenerator, soundVolume: this.gameLogic.GetSoundVolume(), debugMode: this.debugMode);

			if (keyboardInput.IsPressed(Key.Esc) && !previousKeyboardInput.IsPressed(Key.Esc))
				return new GamePausedScreenFrame(frame: this, fps: this.fps, rng: this.rng, guidGenerator: this.guidGenerator, soundVolume: this.gameLogic.GetSoundVolume(), debugMode: this.debugMode);

			return this;
		}

		public void Render(IDisplay<IDTDanmakuAssets> display)
		{
			var translatedDisplay = new TranslatedDTDanmakuDisplay(display, xOffset: 0, yOffset: 0);

			this.gameLogic.Render(translatedDisplay);
		}
	}
}
