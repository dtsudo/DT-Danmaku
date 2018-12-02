namespace DTDanmakuLib
{
	using DTLib;
	
	public class InitialLoadingScreenFrame : IFrame<IDTDanmakuAssets>
	{
		private int fps;
		private IDTDeterministicRandom rng;
		private GuidGenerator guidGenerator;
		private bool debugMode;
		
		public InitialLoadingScreenFrame(int fps, IDTDeterministicRandom rng, GuidGenerator guidGenerator, bool debugMode)
		{
			this.fps = fps;
			this.rng = rng;
			this.guidGenerator = guidGenerator;
			this.debugMode = debugMode;
		}
		
		public IFrame<IDTDanmakuAssets> GetNextFrame(
			IKeyboard keyboardInput,
			IMouse mouseInput,
			IKeyboard previousKeyboardInput,
			IMouse previousMouseInput,
			IDisplay<IDTDanmakuAssets> display)
		{
			bool isDoneLoadingImages = display.GetAssets().LoadImages();

			if (isDoneLoadingImages)
			{
				bool isDoneLoadingSounds = display.GetAssets().LoadSounds();

				if (isDoneLoadingSounds)
				{
					return new TitleScreenFrame(fps: this.fps, rng: this.rng, guidGenerator: this.guidGenerator, soundVolume: null, debugMode: this.debugMode);
				}
				else
				{
					return this;
				}
			}
			else
			{
				return this;
			}
		}

		public void Render(IDisplay<IDTDanmakuAssets> display)
		{
			display.GetAssets().DrawInitialLoadingScreen();
		}
	}
}
