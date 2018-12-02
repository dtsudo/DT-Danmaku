
namespace DTDanmakuLib
{
	using DTLib;

	public enum DTDanmakuImage
	{
		TitleScreen,
		InstructionScreen,
		Version,
		YouWin,
		GameOver,
		PlayerShip,
		PlayerShipInvulnerable,
		PlayerBullet,
		PlayerLifeIcon,
		PowerUp,
		Pause,
		Continue,
		Quit,
		BasicEnemyShip,
		SniperEnemyShip,
		EliteSniperEnemyShip,
		OrbiterEnemyShip,
		EliteOrbiterEnemyShip,
		OrbiterSatellite,
		EliteOrbiterSatellite,
		BarrageEnemyShip,
		EliteBarrageEnemyShip,
		Boss,
		EnemyBullet,
		SniperEnemyBullet,
		EliteSniperEnemyBullet,
		OrbiterEnemyBullet,
		EliteOrbiterEnemyBullet,
		BarrageEnemyBullet,
		EliteBarrageEnemyBullet,
		BossPhase1EnemyBullet,
		BossPhase2EnemyBullet,
		BossPhase3EnemyBullet,
		Explosion1,
		Explosion2,
		Explosion3,
		Explosion4,
		Explosion5,
		Explosion6,
		Explosion7,
		Explosion8,
		Explosion9,
		SoundOff,
		SoundOn
	}

	public enum DTDanmakuSound
	{
		PlayerShoot,
		PlayerDeath,
		EnemyDeath
	}
	
	public interface IDTDanmakuAssets : IAssets
	{
		void DrawInitialLoadingScreen();

		/// <summary>
		/// Must be repeatedly invoked until it returns true before invoking DrawImage(), DrawImageRotatedCounterclockwise(),
		/// GetWidth(), or GetHeight()
		/// </summary>
		bool LoadImages();

		void DrawImage(DTDanmakuImage image, int x, int y);

		void DrawImageRotatedCounterclockwise(DTDanmakuImage image, int x, int y, int milliDegrees);

		long GetWidth(DTDanmakuImage image);

		long GetHeight(DTDanmakuImage image);

		/// <summary>
		/// Must be repeatedly invoked until it returns true before invoking PlaySound()
		/// </summary>
		/// <returns></returns>
		bool LoadSounds();

		/// <summary>
		/// Plays the specified sound.
		/// Volume ranges from 0 to 100 (both inclusive)
		/// </summary>
		void PlaySound(DTDanmakuSound sound, int volume);
	}
}
