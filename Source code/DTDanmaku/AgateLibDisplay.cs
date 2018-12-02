
namespace DTDanmaku
{
	using DTLib;
	using DTDanmakuLib;
	using AgateLib.DisplayLib;
	using AgateLib.Geometry;
	using System;
	using System.IO;
	using System.Collections.Generic;
	using AgateLib.AudioLib;

	public abstract class AgateLibDisplay<T> : IDisplay<T> where T : class, IAssets
	{
		private FontSurface debugFontSurface;

		public AgateLibDisplay()
		{
			this.debugFontSurface = null;
		}

		public void DrawRectangle(int x, int y, int width, int height, DTColor color, bool fill)
		{
			Color agateLibColor = Color.FromArgb(color.Alpha, color.R, color.G, color.B);

			if (fill)
			{
				Display.FillRect(x, y, width, height, agateLibColor);
			}
			else
			{
				// Unclear why the "- 1" are necessary, but it seems to make the display render better.
				Display.DrawRect(x, y, width - 1, height - 1, agateLibColor);
			}
		}

		public void DebugPrint(int x, int y, string debugText)
		{
			if (debugFontSurface == null)
			{
				debugFontSurface = new FontSurface("Arial", 12);
				debugFontSurface.Color = Color.Black;
			}
			debugFontSurface.DrawText(destX: x, destY: y, text: debugText);
		}

		public abstract T GetAssets();
	}

	public class DTDanmakuAgateLibDisplay : AgateLibDisplay<IDTDanmakuAssets>
	{
		private DTDanmakuAssets assets;

		private class DTDanmakuAssets : IDTDanmakuAssets
		{
			private Surface loadingImage;
			
			private Dictionary<DTDanmakuImage, string> imageToFileNameMapping;
			private Dictionary<DTDanmakuImage, Surface> imageToSurfaceMapping;

			private class SoundInfo
			{
				public string FileName { get; private set; }

				/// <summary>
				/// From 0.0 to 1.0
				/// </summary>
				public double Volume { get; private set; }

				public SoundInfo(string fileName, double volume)
				{
					this.FileName = fileName;
					this.Volume = volume;
				}
			}
			private Dictionary<DTDanmakuSound, SoundInfo> soundToFileNameMapping;
			private Dictionary<DTDanmakuSound, SoundBuffer> soundToSoundBufferMapping;

			private bool canSuccessfullyPlayAudio;

			private static string GetImagesDirectory()
			{
				string path = Util.GetExecutablePath();

				// The images are expected to be in the /Data/Images/ folder
				// relative to the executable.
				if (Directory.Exists(path + "/Data/Images"))
					return path + "/Data/Images" + "/";

				// However, if the folder doesn't exist, search for the /Data/Images folder
				// using some heuristic.
				while (true)
				{
					int i = Math.Max(path.LastIndexOf("/", StringComparison.Ordinal), path.LastIndexOf("\\", StringComparison.Ordinal));

					if (i == -1)
						throw new Exception("Cannot find images directory");

					path = path.Substring(0, i);

					if (Directory.Exists(path + "/Data/Images"))
						return path + "/Data/Images" + "/";
				}
			}
			
			private static string GetAudioDirectory()
			{
				string path = Util.GetExecutablePath();

				// The images are expected to be in the /Data/Audio/ folder
				// relative to the executable.
				if (Directory.Exists(path + "/Data/Audio"))
					return path + "/Data/Audio" + "/";

				// However, if the folder doesn't exist, search for the /Data/Audio folder
				// using some heuristic.
				while (true)
				{
					int i = Math.Max(path.LastIndexOf("/", StringComparison.Ordinal), path.LastIndexOf("\\", StringComparison.Ordinal));

					if (i == -1)
						throw new Exception("Cannot find audio directory");

					path = path.Substring(0, i);

					if (Directory.Exists(path + "/Data/Audio"))
						return path + "/Data/Audio" + "/";
				}
			}

			public DTDanmakuAssets(bool canSuccessfullyPlayAudio)
			{
				this.canSuccessfullyPlayAudio = canSuccessfullyPlayAudio;

				string imagesDirectory = GetImagesDirectory();

				this.loadingImage = new Surface(imagesDirectory + "LoadingImage.png");

				this.imageToFileNameMapping = new Dictionary<DTDanmakuImage, string>();
				this.imageToSurfaceMapping = new Dictionary<DTDanmakuImage, Surface>();

				this.imageToFileNameMapping.Add(DTDanmakuImage.TitleScreen, "TitleScreen.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.InstructionScreen, "InstructionScreen.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.Version, "Version.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.YouWin, "YouWin.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.GameOver, "GameOver.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.PlayerShip, "PlayerShip.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.PlayerShipInvulnerable, "PlayerShipInvulnerable.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.PlayerBullet, "PlayerBullet.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.PlayerLifeIcon, "PlayerLifeIcon.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.PowerUp, "PowerUp.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.Pause, "Paused.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.Continue, "Continue.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.Quit, "Quit.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.BasicEnemyShip, "BasicEnemyShip.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.SniperEnemyShip, "SniperEnemyShip.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.EliteSniperEnemyShip, "EliteSniperEnemyShip.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.OrbiterEnemyShip, "OrbiterEnemyShip.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.EliteOrbiterEnemyShip, "EliteOrbiterEnemyShip.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.OrbiterSatellite, "OrbiterSatellite.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.EliteOrbiterSatellite, "EliteOrbiterSatellite.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.BarrageEnemyShip, "BarrageEnemyShip.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.EliteBarrageEnemyShip, "EliteBarrageEnemyShip.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.Boss, "Boss.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.EnemyBullet, "EnemyBullet.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.SniperEnemyBullet, "SniperEnemyBullet.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.EliteSniperEnemyBullet, "EliteSniperEnemyBullet.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.OrbiterEnemyBullet, "OrbiterEnemyBullet.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.EliteOrbiterEnemyBullet, "EliteOrbiterEnemyBullet.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.BarrageEnemyBullet, "BarrageEnemyBullet.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.EliteBarrageEnemyBullet, "EliteBarrageEnemyBullet.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.BossPhase1EnemyBullet, "BossPhase1EnemyBullet.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.BossPhase2EnemyBullet, "BossPhase2EnemyBullet.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.BossPhase3EnemyBullet, "BossPhase3EnemyBullet.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.Explosion1, "Explosion1.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.Explosion2, "Explosion2.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.Explosion3, "Explosion3.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.Explosion4, "Explosion4.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.Explosion5, "Explosion5.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.Explosion6, "Explosion6.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.Explosion7, "Explosion7.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.Explosion8, "Explosion8.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.Explosion9, "Explosion9.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.SoundOff, "SoundOff.png");
				this.imageToFileNameMapping.Add(DTDanmakuImage.SoundOn, "SoundOn.png");

				this.soundToFileNameMapping = new Dictionary<DTDanmakuSound, SoundInfo>();
				this.soundToSoundBufferMapping = new Dictionary<DTDanmakuSound, SoundBuffer>();

				if (this.canSuccessfullyPlayAudio)
				{
					this.soundToFileNameMapping.Add(DTDanmakuSound.PlayerShoot, new SoundInfo(fileName: "PlayerShoot.ogg", volume: 0.20));
					this.soundToFileNameMapping.Add(DTDanmakuSound.PlayerDeath, new SoundInfo(fileName: "PlayerDeath.ogg", volume: 0.20));
					this.soundToFileNameMapping.Add(DTDanmakuSound.EnemyDeath, new SoundInfo(fileName: "EnemyDeath.ogg", volume: 0.10));
				}
			}

			private Surface GetSurface(DTDanmakuImage image)
			{
				return this.imageToSurfaceMapping[image];
			}

			public void DrawInitialLoadingScreen()
			{
				int x = 0;
				int y = 0;
				// Unclear why the "- 1" is necessary, but it seems to make the image render better.
				this.loadingImage.Draw(x - 1, y);
			}
			
			public bool LoadImages()
			{
				string imagesDirectory = GetImagesDirectory();

				foreach (DTDanmakuImage image in this.imageToFileNameMapping.Keys)
				{
					if (!this.imageToSurfaceMapping.ContainsKey(image))
					{
						string filename = this.imageToFileNameMapping[image];
						this.imageToSurfaceMapping.Add(image, new Surface(imagesDirectory + filename));
						return false;
					}
				}

				return true;
			}

			public void DrawImage(DTDanmakuImage image, int x, int y)
			{
				this.DrawImageRotatedCounterclockwise(image: image, x: x, y: y, milliDegrees: 0);
			}

			public void DrawImageRotatedCounterclockwise(DTDanmakuImage image, int x, int y, int milliDegrees)
			{
				Surface surface = this.GetSurface(image);
				surface.RotationCenter = OriginAlignment.Center;
				surface.RotationAngle = milliDegrees / 1000.0 * 2.0 * Math.PI / 360.0;
				
				// Unclear why the "- 1" is necessary, but it seems to make the image render better.
				surface.Draw(x - 1, y);
			}

			public long GetWidth(DTDanmakuImage image)
			{
				return this.GetSurface(image: image).DisplayWidth;
			}

			public long GetHeight(DTDanmakuImage image)
			{
				return this.GetSurface(image: image).DisplayHeight;
			}

			public bool LoadSounds()
			{
				if (!this.canSuccessfullyPlayAudio)
					return true;
				
				string audioDirectory = GetAudioDirectory();

				foreach (DTDanmakuSound sound in this.soundToFileNameMapping.Keys)
				{
					if (!this.soundToSoundBufferMapping.ContainsKey(sound))
					{
						string filename = this.soundToFileNameMapping[sound].FileName;
						this.soundToSoundBufferMapping.Add(sound, new SoundBuffer(audioDirectory + filename));
						return false;
					}
				}

				return true;
			}

			public void PlaySound(DTDanmakuSound sound, int volume)
			{
				if (!this.canSuccessfullyPlayAudio)
					return;

				double finalVolume = this.soundToFileNameMapping[sound].Volume * (volume / 100.0);
				if (finalVolume > 1.0)
					finalVolume = 1.0;
				if (finalVolume < 0.0)
					finalVolume = 0.0;
				if (finalVolume > 0.0)
				{
					SoundBuffer soundBuffer = this.soundToSoundBufferMapping[sound];
					soundBuffer.Volume = finalVolume;
					soundBuffer.Play();
				}
			}
		}

		public DTDanmakuAgateLibDisplay(bool canSuccessfullyPlayAudio)
		{
			this.assets = new DTDanmakuAssets(canSuccessfullyPlayAudio: canSuccessfullyPlayAudio);
		}

		public override IDTDanmakuAssets GetAssets()
		{
			return this.assets;
		}
	}
}
