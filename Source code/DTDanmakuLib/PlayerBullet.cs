

namespace DTDanmakuLib
{
	using System;
	using System.Collections.Generic;
	using DTLib;

	public class PlayerBullet
	{
		public const long SPRITE_NUM_X_PIXELS = 9;
		public const long SPRITE_NUM_Y_PIXELS = 54;
		public const long SPEED_IN_MILLIPIXELS_PER_SECOND = 1400 * 1000;

		public enum PlayerBulletType
		{
			Main,
			Side1Left,
			Side1Right,
			Side2Left,
			Side2Right,
			Side3Left,
			Side3Right
		}

		public long xMillis;
		public long yMillis;
		public PlayerBulletType bulletType;

		private DTImmutableList<ObjectBox> _collisionBoxes;
		public DTImmutableList<ObjectBox> CollisionBoxes { get { return this._collisionBoxes; } } // does not rotate (i.e. does not respect FacingDirectionInMillidegrees)

		public PlayerBullet(long xMillis, long yMillis, PlayerBulletType bulletType, DTImmutableList<ObjectBox> collisionBoxes)
		{
			this.xMillis = xMillis;
			this.yMillis = yMillis;
			this.bulletType = bulletType;
			this._collisionBoxes = collisionBoxes;
		}
		
		public static List<PlayerBullet> ProcessPlayerBulletMovement(
			List<PlayerBullet> playerBullets,
			long elapsedMillisPerFrame)
		{
			List<PlayerBullet> remainingBullets = new List<PlayerBullet>();

			foreach (var bullet in playerBullets)
			{
				long deltaMilliPixels = PlayerBullet.SPEED_IN_MILLIPIXELS_PER_SECOND / 1000 * elapsedMillisPerFrame;

				if (bullet.bulletType == PlayerBullet.PlayerBulletType.Main)
				{
					bullet.yMillis += deltaMilliPixels;
				}
				else if (bullet.bulletType == PlayerBullet.PlayerBulletType.Side1Left)
				{
					// sin 10 degrees = 0.1736
					bullet.xMillis -= deltaMilliPixels * 1736 / 10000;
					// cos 10 degrees = 0.9848
					bullet.yMillis += deltaMilliPixels * 9848 / 10000;
				}
				else if (bullet.bulletType == PlayerBullet.PlayerBulletType.Side1Right)
				{
					// sin 10 degrees = 0.1736
					bullet.xMillis += deltaMilliPixels * 1736 / 10000;
					// cos 10 degrees = 0.9848
					bullet.yMillis += deltaMilliPixels * 9848 / 10000;
				}
				else if (bullet.bulletType == PlayerBullet.PlayerBulletType.Side2Left)
				{
					// sin 20 degrees = 0.3420
					bullet.xMillis -= deltaMilliPixels * 3420 / 10000;
					// cos 20 degrees = 0.9397
					bullet.yMillis += deltaMilliPixels * 9397 / 10000;
				}
				else if (bullet.bulletType == PlayerBullet.PlayerBulletType.Side2Right)
				{
					// sin 20 degrees = 0.3420
					bullet.xMillis += deltaMilliPixels * 3420 / 10000;
					// cos 20 degrees = 0.9397
					bullet.yMillis += deltaMilliPixels * 9397 / 10000;
				}
				else if (bullet.bulletType == PlayerBullet.PlayerBulletType.Side3Left)
				{
					// sin 30 degrees = 0.5
					bullet.xMillis -= deltaMilliPixels * 5000 / 10000;
					// cos 30 degrees = 0.8660
					bullet.yMillis += deltaMilliPixels * 8660 / 10000;
				}
				else if (bullet.bulletType == PlayerBullet.PlayerBulletType.Side3Right)
				{
					// sin 30 degrees = 0.5
					bullet.xMillis += deltaMilliPixels * 5000 / 10000;
					// cos 30 degrees = 0.8660
					bullet.yMillis += deltaMilliPixels * 8660 / 10000;
				}
				else
				{
					throw new Exception();
				}

				bool isDead = bullet.yMillis >= (700 + PlayerBullet.SPRITE_NUM_Y_PIXELS / 2) * 1000;
				if (!isDead)
					remainingBullets.Add(bullet);
			}

			return remainingBullets;
		}

		public static void RenderPlayerBullets(
			List<PlayerBullet> playerBullets,
			TranslatedDTDanmakuDisplay display)
		{
			foreach (var playerBullet in playerBullets)
			{
				int playerBulletX = ((int)playerBullet.xMillis / 1000) - ((int)PlayerBullet.SPRITE_NUM_X_PIXELS) / 2;
				int playerBulletY = 700 - ((int)playerBullet.yMillis / 1000) - ((int)PlayerBullet.SPRITE_NUM_Y_PIXELS) / 2;
				if (playerBullet.bulletType == PlayerBullet.PlayerBulletType.Main)
					display.DrawImage(DTDanmakuImage.PlayerBullet, playerBulletX, playerBulletY);
				else if (playerBullet.bulletType == PlayerBullet.PlayerBulletType.Side1Left)
					display.DrawImageRotatedCounterclockwise(DTDanmakuImage.PlayerBullet, playerBulletX, playerBulletY, 10 * 1000);
				else if (playerBullet.bulletType == PlayerBullet.PlayerBulletType.Side1Right)
					display.DrawImageRotatedCounterclockwise(DTDanmakuImage.PlayerBullet, playerBulletX, playerBulletY, -10 * 1000);
				else if (playerBullet.bulletType == PlayerBullet.PlayerBulletType.Side2Left)
					display.DrawImageRotatedCounterclockwise(DTDanmakuImage.PlayerBullet, playerBulletX, playerBulletY, 20 * 1000);
				else if (playerBullet.bulletType == PlayerBullet.PlayerBulletType.Side2Right)
					display.DrawImageRotatedCounterclockwise(DTDanmakuImage.PlayerBullet, playerBulletX, playerBulletY, -20 * 1000);
				else if (playerBullet.bulletType == PlayerBullet.PlayerBulletType.Side3Left)
					display.DrawImageRotatedCounterclockwise(DTDanmakuImage.PlayerBullet, playerBulletX, playerBulletY, 30 * 1000);
				else if (playerBullet.bulletType == PlayerBullet.PlayerBulletType.Side3Right)
					display.DrawImageRotatedCounterclockwise(DTDanmakuImage.PlayerBullet, playerBulletX, playerBulletY, -30 * 1000);
				else
					throw new Exception();
			}
		}
	}
}
