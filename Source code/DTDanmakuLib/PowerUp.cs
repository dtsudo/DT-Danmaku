
namespace DTDanmakuLib
{
	using System;
	using System.Collections.Generic;

	public class PowerUp
	{
		public const long SPRITE_NUM_X_PIXELS = 34;
		public const long SPRITE_NUM_Y_PIXELS = 33;

		public long xMillis;
		public long yMillis;

		public PowerUp(long xMillis, long yMillis)
		{
			this.xMillis = xMillis;
			this.yMillis = yMillis;
		}

		public class PowerUpActionResult
		{
			public List<PowerUp> powerUps;
			public long playerPowerUpLevel;

			public PowerUpActionResult(
				List<PowerUp> powerUps,
				long playerPowerUpLevel)
			{
				this.powerUps = powerUps;
				this.playerPowerUpLevel = playerPowerUpLevel;
			}
		}

		public static PowerUpActionResult ProcessPowerUpMovement(
			List<PowerUp> powerUps,
			bool isPlayerDead,
			long playerXMillis,
			long playerYMillis,
			long playerPowerUpLevel,
			long elapsedMillisPerFrame)
		{
			List<PowerUp> remainingPowerUps = new List<PowerUp>();

			foreach (PowerUp powerUp in powerUps)
			{
				long powerUpMoveSpeedInMilliPixelsPerSecond = 100 * 1000;
				powerUp.yMillis -= powerUpMoveSpeedInMilliPixelsPerSecond / 1000 * elapsedMillisPerFrame;

				bool isPowerUpDead = false;

				// collision with player
				if (!isPlayerDead)
				{
					if (Math.Abs(powerUp.xMillis - playerXMillis) <= (PowerUp.SPRITE_NUM_X_PIXELS + Player.SPRITE_NUM_X_PIXELS) * 1000 / 2
						&& Math.Abs(powerUp.yMillis - playerYMillis) <= (PowerUp.SPRITE_NUM_Y_PIXELS + Player.SPRITE_NUM_Y_PIXELS) * 1000 / 2)
					{
						if (playerPowerUpLevel < 3)
							playerPowerUpLevel += 1;
						isPowerUpDead = true;
					}

				}

				if (powerUp.yMillis < 0 - PowerUp.SPRITE_NUM_Y_PIXELS * 1000 / 2)
				{
					isPowerUpDead = true;
				}

				if (!isPowerUpDead)
				{
					remainingPowerUps.Add(powerUp);
				}
			}

			return new PowerUpActionResult(
				powerUps: remainingPowerUps,
				playerPowerUpLevel: playerPowerUpLevel);
		}

		public static void RenderPowerUps(
			List<PowerUp> powerUps,
			TranslatedDTDanmakuDisplay display)
		{
			foreach (var powerUp in powerUps)
			{
				int powerUpX = ((int)powerUp.xMillis / 1000) - ((int)PowerUp.SPRITE_NUM_X_PIXELS) / 2;
				int powerUpY = 700 - ((int)powerUp.yMillis / 1000) - ((int)PowerUp.SPRITE_NUM_Y_PIXELS) / 2;
				display.DrawImage(DTDanmakuImage.PowerUp, powerUpX, powerUpY);
			}
		}
	}
}
