
namespace DTDanmaku
{
	using DTLib;
	using DTDanmakuLib;
	using AgateLib;
	using AgateLib.DisplayLib;
	using AgateLib.Geometry;
	using System;
	using System.Globalization;

	public class Initializer
	{
		public static void Start(bool debugMode)
		{
			IDTDeterministicRandom rng = new DTDeterministicRandom(seed: 0);

			using (AgateSetup setup = new AgateSetup())
			{
				setup.InitializeDisplay();
				setup.InitializeInput();
				bool canSuccessfullyPlayAudio;
				try
				{
					setup.InitializeAudio();
					canSuccessfullyPlayAudio = true;
				}
				catch (Exception)
				{
					canSuccessfullyPlayAudio = false;
				}

				if (setup.WasCanceled)
					return;

				DisplayWindow window = DisplayWindow.CreateWindowed("DT Danmaku", 1000, 700);
				Display.VSync = false;

				int fps = 60;

				IFrame<IDTDanmakuAssets> frame = DTDanmaku.GetFirstFrame(
					fps: fps,
					rng: rng,
					guidGenerator: new GuidGenerator(guidString: "285419206161623102"),
					debugMode: debugMode);

				IKeyboard agateLibKeyboard = new AgateLibKeyboard();
				IMouse agateLibMouse = new AgateLibMouse();
				IDisplay<IDTDanmakuAssets> display = new DTDanmakuAgateLibDisplay(canSuccessfullyPlayAudio: canSuccessfullyPlayAudio);
				IKeyboard prevKeyboard = new EmptyKeyboard();
				IMouse prevMouse = new EmptyMouse();

				double elapsedTimeMs = 0.0;

				long timeForFpsCounter = DateTime.Now.Ticks;
				int currentDisplayFpsCount = 0;
				int displayFpsSnapshotValue = 0;
				
				int debugSlowDown = 0;
				int debugNumCyclesToSkip = 0;
				
				int numTimesFramesDropped = 0;

				while (Display.CurrentWindow.IsClosed == false)
				{
					Display.BeginFrame();

					Display.Clear(Color.White);

					frame.Render(display);

					elapsedTimeMs += Display.DeltaTime;

					// Run at 60 frames per second.

					// If for whatever reason, we're really behind, we'll try to catch up,
					// but only for a maximum of 5 consecutive frames.
					if (elapsedTimeMs > 1000.0 / fps * 5.0)
					{
						elapsedTimeMs = 1000.0 / fps * 5.0;
						
						numTimesFramesDropped++;
					}

					if (elapsedTimeMs > 1000.0 / fps)
					{
						if (debugMode)
						{
							if (agateLibKeyboard.IsPressed(Key.Six) && !prevKeyboard.IsPressed(Key.Six))
								debugSlowDown = (debugSlowDown + 1) % 4;
						}
						
						elapsedTimeMs = elapsedTimeMs - 1000.0 / fps;
						IKeyboard currentKeyboard = new CopiedKeyboard(agateLibKeyboard);
						IMouse currentMouse = new CopiedMouse(agateLibMouse);

						if (debugMode)
						{
							if (debugNumCyclesToSkip == 0)
								frame = frame.GetNextFrame(currentKeyboard, currentMouse, prevKeyboard, prevMouse, display);
						}
						else
						{
							frame = frame.GetNextFrame(currentKeyboard, currentMouse, prevKeyboard, prevMouse, display);
						}


						prevKeyboard = new CopiedKeyboard(currentKeyboard);
						prevMouse = new CopiedMouse(currentMouse);
						
						if (debugMode)
						{
							if (debugSlowDown > 0)
							{
								if (debugNumCyclesToSkip > 0)
									debugNumCyclesToSkip--;
								else
								{
									if (debugSlowDown == 1)
										debugNumCyclesToSkip = 1;
									if (debugSlowDown == 2)
										debugNumCyclesToSkip = 7;
									if (debugSlowDown == 3)
										debugNumCyclesToSkip = 31;
								}
							}
							else
							{
								debugNumCyclesToSkip = 0;
							}
						}
					}
					else
					{
						System.Threading.Thread.Sleep(millisecondsTimeout: 5);
					}

					if (debugMode)
					{
						currentDisplayFpsCount++;
						long milliSecondsElapsedForFpsCounter = (DateTime.Now.Ticks - timeForFpsCounter) / 10000L;
						if (milliSecondsElapsedForFpsCounter > 1000)
						{
							timeForFpsCounter += 10000L * 1000L;
							displayFpsSnapshotValue = currentDisplayFpsCount;
							currentDisplayFpsCount = 0;
						}
						display.DebugPrint(x: 10, y: 10, debugText: "fps: " + displayFpsSnapshotValue.ToString(CultureInfo.InvariantCulture));
						
						display.DebugPrint(x: 10, y: 70, debugText: "num frames dropped: " + numTimesFramesDropped.ToString(CultureInfo.InvariantCulture));
					}

					Display.EndFrame();

					Core.KeepAlive();
				}
			}
		}
	}
}
