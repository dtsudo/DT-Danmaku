

namespace DTDanmakuLib
{
	using DTLib;
	using System.Collections.Generic;
	using System;

	public class Player
	{
		public const long SPAWN_LOCATION_X_MILLIS = 500 * 1000;
		public const long SPAWN_LOCATION_Y_MILLIS = 100 * 1000;

		public const long SPRITE_NUM_X_PIXELS = 99;
		public const long SPRITE_NUM_Y_PIXELS = 75;

		// How many milliseconds must elapse before a subsequent bullet can be shot
		public const long PLAYER_SHOOT_COOLDOWN_IN_MILLIS = 100;

		// Standard math (x, y); i.e. +y axis goes up
		// 1000 xMillis = one horizontal pixel
		// 1000 yMillis = one vertical pixel
		public long xMillis;
		public long yMillis;
		public bool isDead;
		// Non-null if this.isDead is true and player should respawn
		// Denotes the number of milliseconds until player respawns
		// Null if player isn't dead OR if player died but has no lives left
		public long? respawnTimeRemainingMillis;

		// When player is killed, this variable should decrement immediately
		// 0 = no extra lives, but isn't game over unless player gets hit again
		public int numLivesLeft;

		// Null = player is not invincible
		// Non-null = number of milliseconds of invulnerability remaining
		public long? playerInvincibilityTimeRemainingMillis;

		public long playerShootCooldownInMillis;

		public long playerPowerUpLevel;

		private ObjectAction playerDeathSpawnDestructionAnimationAction;
		public Dictionary<PlayerBullet.PlayerBulletType, DTImmutableList<ObjectBox>> playerBulletTypeToCollisionBoxesMapping;

		public Player(ObjectAction playerDeathSpawnDestructionAnimationAction)
		{
			this.xMillis = SPAWN_LOCATION_X_MILLIS;
			this.yMillis = SPAWN_LOCATION_Y_MILLIS;
			this.isDead = false;
			this.respawnTimeRemainingMillis = null;
			this.numLivesLeft = 3;
			this.playerInvincibilityTimeRemainingMillis = null;
			this.playerShootCooldownInMillis = 0;
			this.playerPowerUpLevel = 0;
			this.playerDeathSpawnDestructionAnimationAction = playerDeathSpawnDestructionAnimationAction;

			this.playerBulletTypeToCollisionBoxesMapping = new Dictionary<PlayerBullet.PlayerBulletType, DTImmutableList<ObjectBox>>();
			
			List<ObjectBox> collisionBoxesMain = new List<ObjectBox>();
			collisionBoxesMain.Add(new ObjectBox(-4000, 4000, -4000, 4000));
			collisionBoxesMain.Add(new ObjectBox(-4000, 4000, -25000, -17000));
			collisionBoxesMain.Add(new ObjectBox(-4000, 4000, 17000, 25000));

			this.playerBulletTypeToCollisionBoxesMapping[PlayerBullet.PlayerBulletType.Main] = new DTImmutableList<ObjectBox>(collisionBoxesMain);

			List<ObjectBox> collisionBoxesSide1Left = new List<ObjectBox>();
			collisionBoxesSide1Left.Add(new ObjectBox(-4000, 4000, -4000, 4000));
			collisionBoxesSide1Left.Add(new ObjectBox(-1000, 7000, -24000, -16000));
			collisionBoxesSide1Left.Add(new ObjectBox(-7000, 1000, 16000, 24000));

			this.playerBulletTypeToCollisionBoxesMapping[PlayerBullet.PlayerBulletType.Side1Left] = new DTImmutableList<ObjectBox>(collisionBoxesSide1Left);

			List<ObjectBox> collisionBoxesSide1Right = new List<ObjectBox>();
			collisionBoxesSide1Right.Add(new ObjectBox(-4000, 4000, -4000, 4000));
			collisionBoxesSide1Right.Add(new ObjectBox(-7000, 1000, -24000, -16000));
			collisionBoxesSide1Right.Add(new ObjectBox(-1000, 7000, 16000, 24000));

			this.playerBulletTypeToCollisionBoxesMapping[PlayerBullet.PlayerBulletType.Side1Right] = new DTImmutableList<ObjectBox>(collisionBoxesSide1Right);

			List<ObjectBox> collisionBoxesSide2Left = new List<ObjectBox>();
			collisionBoxesSide2Left.Add(new ObjectBox(-4000, 4000, -4000, 4000));
			collisionBoxesSide2Left.Add(new ObjectBox(3000, 11000, -23000, -15000));
			collisionBoxesSide2Left.Add(new ObjectBox(-11000, -3000, 15000, 23000));

			this.playerBulletTypeToCollisionBoxesMapping[PlayerBullet.PlayerBulletType.Side2Left] = new DTImmutableList<ObjectBox>(collisionBoxesSide2Left);

			List<ObjectBox> collisionBoxesSide2Right = new List<ObjectBox>();
			collisionBoxesSide2Right.Add(new ObjectBox(-4000, 4000, -4000, 4000));
			collisionBoxesSide2Right.Add(new ObjectBox(-11000, -3000, -23000, -15000));
			collisionBoxesSide2Right.Add(new ObjectBox(3000, 11000, 15000, 23000));

			this.playerBulletTypeToCollisionBoxesMapping[PlayerBullet.PlayerBulletType.Side2Right] = new DTImmutableList<ObjectBox>(collisionBoxesSide2Right);

			List<ObjectBox> collisionBoxesSide3Left = new List<ObjectBox>();
			collisionBoxesSide3Left.Add(new ObjectBox(-4000, 4000, -4000, 4000));
			collisionBoxesSide3Left.Add(new ObjectBox(7000, 15000, -22000, -14000));
			collisionBoxesSide3Left.Add(new ObjectBox(-15000, -7000, 14000, 22000));

			this.playerBulletTypeToCollisionBoxesMapping[PlayerBullet.PlayerBulletType.Side3Left] = new DTImmutableList<ObjectBox>(collisionBoxesSide3Left);

			List<ObjectBox> collisionBoxesSide3Right = new List<ObjectBox>();
			collisionBoxesSide3Right.Add(new ObjectBox(-4000, 4000, -4000, 4000));
			collisionBoxesSide3Right.Add(new ObjectBox(-15000, -7000, -22000, -14000));
			collisionBoxesSide3Right.Add(new ObjectBox(7000, 15000, 14000, 22000));

			this.playerBulletTypeToCollisionBoxesMapping[PlayerBullet.PlayerBulletType.Side3Right] = new DTImmutableList<ObjectBox>(collisionBoxesSide3Right);
		}

		// Returns true if player still has at least one life remaining
		// Returns false if game over
		public bool DestroyPlayer(
			List<EnemyObject> enemyObjects,
			long elapsedMillisecondsPerIteration,
			IDTDeterministicRandom rng)
		{
			if (this.isDead)
				throw new Exception();

			long playerXMillis = this.xMillis;
			long playerYMillis = this.yMillis;

			EnemyObjectTemplate template = EnemyObjectTemplate.Placeholder(action: ObjectAction.ConditionalNextAction(
				currentAction: this.playerDeathSpawnDestructionAnimationAction,
				condition: BooleanExpression.True(),
				nextAction: ObjectAction.Destroy()));
			enemyObjects.Add(new EnemyObject(
				template: template,
				initialXMillis: playerXMillis,
				initialYMillis: playerYMillis,
				playerXMillis: playerXMillis,
				playerYMillis: playerYMillis,
				elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
				isPlayerDestroyed: true,
				parent: null,
				initialNumericVariables: null,
				initialBooleanVariables: null,
				rng: rng));

			this.isDead = true;

			if (this.numLivesLeft > 0)
			{
				this.numLivesLeft = this.numLivesLeft - 1;
				this.respawnTimeRemainingMillis = 750;
				return true;
			}
			else
				return false;
		}

		public static void ProcessPlayerRespawnAndInvincibility(
			Player player,
			long elapsedMillisPerFrame)
		{
			if (player.respawnTimeRemainingMillis.HasValue)
			{
				player.respawnTimeRemainingMillis = player.respawnTimeRemainingMillis.Value - elapsedMillisPerFrame;
				if (player.respawnTimeRemainingMillis.Value <= 0)
				{
					player.xMillis = SPAWN_LOCATION_X_MILLIS;
					player.yMillis = SPAWN_LOCATION_Y_MILLIS;
					player.isDead = false;
					player.respawnTimeRemainingMillis = null;
					player.playerInvincibilityTimeRemainingMillis = 3 * 1000;
					player.playerPowerUpLevel = 0;
				}
			}

			if (player.playerInvincibilityTimeRemainingMillis.HasValue)
			{
				player.playerInvincibilityTimeRemainingMillis = player.playerInvincibilityTimeRemainingMillis.Value - elapsedMillisPerFrame;
				if (player.playerInvincibilityTimeRemainingMillis.Value <= 0)
					player.playerInvincibilityTimeRemainingMillis = null;
			}
		}

		/// <summary>
		/// When the player moves, it's possible that from frame to frame, the player moves so fast that
		/// in one frame, the player is to the left of an enemy bullet and in the next frame, the player
		/// is to the right of the enemy bullet, and the collision detection does not see a collision
		/// because at no point was the player colliding with the bullet.
		/// 
		/// So instead of checking the player's position at the end of the frame, we return multiple points
		/// (denoting the path the player's ship took) and check collision on a sub-frame basis.
		/// </summary>
		public class PlayerSubFramePosition
		{
			public long XMillis { get; private set; }
			public long YMillis { get; private set; }

			public PlayerSubFramePosition(long xMillis, long yMillis)
			{
				this.XMillis = xMillis;
				this.YMillis = yMillis;
			}
		}

		public static List<PlayerSubFramePosition> ProcessPlayerMovement(
			Player player,
			long elapsedMillisPerFrame,
			IKeyboard keyboardInput)
		{
			List<PlayerSubFramePosition> subFramePositions = new List<PlayerSubFramePosition>();

			if (!player.isDead)
			{
				bool isLeftPressed = keyboardInput.IsPressed(Key.LeftArrow) && !keyboardInput.IsPressed(Key.RightArrow);
				bool isRightPressed = !keyboardInput.IsPressed(Key.LeftArrow) && keyboardInput.IsPressed(Key.RightArrow);
				bool isUpPressed = keyboardInput.IsPressed(Key.UpArrow) && !keyboardInput.IsPressed(Key.DownArrow);
				bool isDownPressed = !keyboardInput.IsPressed(Key.UpArrow) && keyboardInput.IsPressed(Key.DownArrow);

				long fastPlayerMoveSpeedInMilliPixelsPerSecond = 450 * 1000;
				long slowPlayerMoveSpeedInMilliPixelsPerSecond = 150 * 1000;
				long playerMoveSpeedInMilliPixelsPerSecond = keyboardInput.IsPressed(Key.Shift) ? slowPlayerMoveSpeedInMilliPixelsPerSecond : fastPlayerMoveSpeedInMilliPixelsPerSecond;

				long deltaChangeInXMilliPixels = 0;
				long deltaChangeInYMilliPixels = 0;

				// 1 / Sqrt(2) = 0.707
				long deltaChangeInOneDirection = playerMoveSpeedInMilliPixelsPerSecond / 1000 * elapsedMillisPerFrame;
				long deltaChangeInTwoDirections = deltaChangeInOneDirection * 707 / 1000;
				if (isLeftPressed && isUpPressed)
				{
					deltaChangeInXMilliPixels -= deltaChangeInTwoDirections;
					deltaChangeInYMilliPixels += deltaChangeInTwoDirections;
				}
				else if (isLeftPressed && isDownPressed)
				{
					deltaChangeInXMilliPixels -= deltaChangeInTwoDirections;
					deltaChangeInYMilliPixels -= deltaChangeInTwoDirections;
				}
				else if (isRightPressed && isUpPressed)
				{
					deltaChangeInXMilliPixels += deltaChangeInTwoDirections;
					deltaChangeInYMilliPixels += deltaChangeInTwoDirections;
				}
				else if (isRightPressed && isDownPressed)
				{
					deltaChangeInXMilliPixels += deltaChangeInTwoDirections;
					deltaChangeInYMilliPixels -= deltaChangeInTwoDirections;
				}
				else if (isLeftPressed)
				{
					deltaChangeInXMilliPixels -= deltaChangeInOneDirection;
				}
				else if (isRightPressed)
				{
					deltaChangeInXMilliPixels += deltaChangeInOneDirection;
				}
				else if (isUpPressed)
				{
					deltaChangeInYMilliPixels += deltaChangeInOneDirection;
				}
				else if (isDownPressed)
				{
					deltaChangeInYMilliPixels -= deltaChangeInOneDirection;
				}

				long deltaSteps = 3; // arbitrary
				for (long i = 0; i < deltaSteps; i++)
				{
					player.xMillis += deltaChangeInXMilliPixels / deltaSteps;
					player.yMillis += deltaChangeInYMilliPixels / deltaSteps;

					long minXMillis = Player.SPRITE_NUM_X_PIXELS * 1000 / 2;
					if (player.xMillis < minXMillis)
						player.xMillis = minXMillis;

					long maxXMillis = 1000 * 1000 - Player.SPRITE_NUM_X_PIXELS * 1000 / 2;
					if (player.xMillis > maxXMillis)
						player.xMillis = maxXMillis;

					long minYMillis = Player.SPRITE_NUM_Y_PIXELS * 1000 / 2;
					if (player.yMillis < minYMillis)
						player.yMillis = minYMillis;

					long maxYMillis = 700 * 1000 - Player.SPRITE_NUM_Y_PIXELS * 1000 / 2;
					if (player.yMillis > maxYMillis)
						player.yMillis = maxYMillis;

					subFramePositions.Add(new PlayerSubFramePosition(xMillis: player.xMillis, yMillis: player.yMillis));
				}
			}
			else
			{
				subFramePositions.Add(new PlayerSubFramePosition(xMillis: player.xMillis, yMillis: player.yMillis));
			}

			return subFramePositions;
		}

		public class PlayerShootResult
		{
			public List<PlayerBullet> PlayerBullets { get; private set; }
			public bool DidShoot { get; private set; }

			public PlayerShootResult(List<PlayerBullet> playerBullets, bool didShoot)
			{
				this.PlayerBullets = playerBullets;
				this.DidShoot = didShoot;
			}
		}

		public static PlayerShootResult ProcessPlayerShoot(
			Player player,
			long elapsedMillisPerFrame,
			IKeyboard keyboardInput)
		{
			List<PlayerBullet> newPlayerBullets = new List<PlayerBullet>();
			bool didShoot = false;

			player.playerShootCooldownInMillis -= elapsedMillisPerFrame;

			if (!player.isDead && player.playerShootCooldownInMillis <= 0 && keyboardInput.IsPressed(Key.Z))
			{
				didShoot = true;

				long xMillis = player.xMillis;
				long yMillis = player.yMillis + Player.SPRITE_NUM_Y_PIXELS * 1000 / 2 + PlayerBullet.SPRITE_NUM_X_PIXELS * 1000 / 2;
				newPlayerBullets.Add(new PlayerBullet(
					xMillis: xMillis,
					yMillis: yMillis,
					bulletType: PlayerBullet.PlayerBulletType.Main,
					collisionBoxes: player.playerBulletTypeToCollisionBoxesMapping[PlayerBullet.PlayerBulletType.Main]));

				if (player.playerPowerUpLevel >= 1)
				{
					// sin 10 degrees = 0.1736
					long side1XOffset = PlayerBullet.SPRITE_NUM_Y_PIXELS * 1000L / 2 * 1736 / 10000;
					// 1 - (cos 10 degrees) = 0.0152
					long side1YOffset = PlayerBullet.SPRITE_NUM_Y_PIXELS * 1000L / 2 * 152 / 10000;
					newPlayerBullets.Add(new PlayerBullet(xMillis: xMillis - side1XOffset, yMillis: yMillis - side1YOffset, bulletType: PlayerBullet.PlayerBulletType.Side1Left, collisionBoxes: player.playerBulletTypeToCollisionBoxesMapping[PlayerBullet.PlayerBulletType.Side1Left]));
					newPlayerBullets.Add(new PlayerBullet(xMillis: xMillis + side1XOffset, yMillis: yMillis - side1YOffset, bulletType: PlayerBullet.PlayerBulletType.Side1Right, collisionBoxes: player.playerBulletTypeToCollisionBoxesMapping[PlayerBullet.PlayerBulletType.Side1Right]));

					if (player.playerPowerUpLevel >= 2)
					{
						// sin 20 degrees = 0.3420
						long side2XOffset = PlayerBullet.SPRITE_NUM_Y_PIXELS * 1000L / 2 * 3420 / 10000;
						// 1 - (cos 20 degrees) = 0.0603
						long side2YOffset = PlayerBullet.SPRITE_NUM_Y_PIXELS * 1000L / 2 * 603 / 10000;
						newPlayerBullets.Add(new PlayerBullet(xMillis: xMillis - side2XOffset, yMillis: yMillis - side2YOffset, bulletType: PlayerBullet.PlayerBulletType.Side2Left, collisionBoxes: player.playerBulletTypeToCollisionBoxesMapping[PlayerBullet.PlayerBulletType.Side2Left]));
						newPlayerBullets.Add(new PlayerBullet(xMillis: xMillis + side2XOffset, yMillis: yMillis - side2YOffset, bulletType: PlayerBullet.PlayerBulletType.Side2Right, collisionBoxes: player.playerBulletTypeToCollisionBoxesMapping[PlayerBullet.PlayerBulletType.Side2Right]));

						if (player.playerPowerUpLevel >= 3)
						{
							// sin 30 degrees = 0.5
							long side3XOffset = PlayerBullet.SPRITE_NUM_Y_PIXELS * 1000L / 2 * 5000 / 10000;
							// 1 - (cos 30 degrees) = 0.1340
							long side3YOffset = PlayerBullet.SPRITE_NUM_Y_PIXELS * 1000L / 2 * 1340 / 10000;
							newPlayerBullets.Add(new PlayerBullet(xMillis: xMillis - side3XOffset, yMillis: yMillis - side3YOffset, bulletType: PlayerBullet.PlayerBulletType.Side3Left, collisionBoxes: player.playerBulletTypeToCollisionBoxesMapping[PlayerBullet.PlayerBulletType.Side3Left]));
							newPlayerBullets.Add(new PlayerBullet(xMillis: xMillis + side3XOffset, yMillis: yMillis - side3YOffset, bulletType: PlayerBullet.PlayerBulletType.Side3Right, collisionBoxes: player.playerBulletTypeToCollisionBoxesMapping[PlayerBullet.PlayerBulletType.Side3Right]));
						}
					}
				}
				player.playerShootCooldownInMillis += Player.PLAYER_SHOOT_COOLDOWN_IN_MILLIS;
			}

			if (player.playerShootCooldownInMillis < 0)
				player.playerShootCooldownInMillis = 0;

			return new PlayerShootResult(playerBullets: newPlayerBullets, didShoot: didShoot);
		}

		public static void RenderPlayer(
			Player player,
			TranslatedDTDanmakuDisplay display,
			bool debugMode,
			bool debug_renderHitboxes)
		{
			if (!player.isDead)
			{
				int playerX = ((int)player.xMillis / 1000) - ((int)Player.SPRITE_NUM_X_PIXELS) / 2;
				int playerY = 700 - ((int)player.yMillis / 1000) - ((int)Player.SPRITE_NUM_Y_PIXELS) / 2;

				if (player.playerInvincibilityTimeRemainingMillis.HasValue && player.playerInvincibilityTimeRemainingMillis.Value > 0)
					display.DrawImage(DTDanmakuImage.PlayerShipInvulnerable, playerX, playerY);
				else
					display.DrawImage(DTDanmakuImage.PlayerShip, playerX, playerY);

				/*
					Note that since these debug things bypass regular game logic, they may break other stuff or crash the program
					(Should be used for debugging / development only)
				*/
				if (debugMode)
				{
					if (debug_renderHitboxes)
					{
						display.DrawRectangle(
							x: ((int)(player.xMillis / 1000L)) - 3,
							y: 700 - ((int)(player.yMillis / 1000L)) - 3,
							width: 7,
							height: 7,
							color: new DTColor(255, 0, 0),
							fill: true);
					}
				}
			}
		}

		public static void RenderPlayerLifeIcons(
			int numberOfLivesRemaining,
			TranslatedDTDanmakuDisplay display)
		{
			int width = (int)display.GetWidth(DTDanmakuImage.PlayerLifeIcon);

			for (int i = 0; i < numberOfLivesRemaining; i++)
			{
				display.DrawImage(DTDanmakuImage.PlayerLifeIcon,
					x: 1000 - 1 - (i + 1) * (width + 1),
					y: 3);

			}
		}
	}
}
