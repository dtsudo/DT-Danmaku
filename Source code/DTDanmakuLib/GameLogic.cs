
namespace DTDanmakuLib
{
	using System;
	using System.Collections.Generic;
	using DTLib;

	public class GameLogic
	{
		private int fps;
		private long elapsedTimeMillis;
		private int soundVolume; // between 0 and 100 (both inclusive)
		private bool debugMode;

		private IDTDeterministicRandom rng;

		private Player player;
		private List<PlayerBullet> playerBullets;
		private List<PowerUp> powerUps;
		private List<EnemyObject> enemyObjects;
		private Dictionary<string, DTDanmakuImage> spriteNameToImageDictionary;
		private Dictionary<string, EnemyObjectTemplate> enemyObjectTemplates;
		private Dictionary<string, DTDanmakuSound> soundNameToSoundDictionary;

		private bool shouldPlayPlayerShootSound;
		// When non-zero, is the number of milliseconds before the PlayerShoot sound can play again
		private long playerShootSoundCooldownInMillis;

		private List<DTDanmakuSound> soundsThatNeedToBePlayed;

		private BossHealthBar bossHealthBar;

		// If null, means game isn't over
		// If non-null, is number of milliseconds until transition to GameOverFrame
		private long? gameOverCountdownInMillis;
		// If null, means level isn't finished
		// If non-null, is number of milliseconds until level finishes (unless player dies,
		// in which case GameOver will take precedence)
		private long? levelFinishedCountdownInMillis;
		
		private bool debug_renderHitboxes;

		public GameLogic(
			int fps,
			IDTDeterministicRandom rng,
			GuidGenerator guidGenerator,
			int soundVolume,
			bool debugMode)
		{
			this.fps = fps;
			this.elapsedTimeMillis = 0;
			long elapsedMillisPerFrame = 1000 / this.fps;

			this.soundVolume = soundVolume;

			this.debugMode = debugMode;

			this.rng = rng;
			this.rng.Reset();

			this.spriteNameToImageDictionary = new Dictionary<string, DTDanmakuImage>();
			this.enemyObjectTemplates = new Dictionary<string, EnemyObjectTemplate>();
			this.soundNameToSoundDictionary = new Dictionary<string, DTDanmakuSound>();

			List<DTDanmakuImage> playerDeadExplosionSprites = new List<DTDanmakuImage>();
			playerDeadExplosionSprites.Add(DTDanmakuImage.Explosion1);
			playerDeadExplosionSprites.Add(DTDanmakuImage.Explosion2);
			playerDeadExplosionSprites.Add(DTDanmakuImage.Explosion3);
			playerDeadExplosionSprites.Add(DTDanmakuImage.Explosion4);
			playerDeadExplosionSprites.Add(DTDanmakuImage.Explosion5);
			playerDeadExplosionSprites.Add(DTDanmakuImage.Explosion6);
			playerDeadExplosionSprites.Add(DTDanmakuImage.Explosion7);
			playerDeadExplosionSprites.Add(DTDanmakuImage.Explosion8);
			playerDeadExplosionSprites.Add(DTDanmakuImage.Explosion9);
			DestructionAnimationGenerator.GenerateDestructionAnimationResult playerDeadDestructionAnimationResult = DestructionAnimationGenerator.GenerateDestructionAnimation(
				orderedSprites: playerDeadExplosionSprites,
				millisecondsPerSprite: 200,
				guidGenerator: guidGenerator);
			foreach (var entry in playerDeadDestructionAnimationResult.spriteNameToImageDictionary)
				this.spriteNameToImageDictionary.Add(entry.Key, entry.Value);
			foreach (var entry in playerDeadDestructionAnimationResult.enemyObjectTemplates)
				this.enemyObjectTemplates.Add(entry.Key, entry.Value);

			this.player = new Player(playerDeadDestructionAnimationResult.objectAction);
			this.playerBullets = new List<PlayerBullet>();
			this.powerUps = new List<PowerUp>();

			GenerateEnemiesResult enemies = GenerateEnemiesForLevel.GenerateEnemies(
				elapsedMillisecondsPerIteration: elapsedMillisPerFrame,
				rng: rng,
				guidGenerator: guidGenerator);

			this.enemyObjects = enemies.enemyObjects;

			foreach (var entry in enemies.spriteNameToImageDictionary)
				this.spriteNameToImageDictionary.Add(entry.Key, entry.Value);
			foreach (var entry in enemies.enemyObjectTemplates)
				this.enemyObjectTemplates.Add(entry.Key, entry.Value);
			foreach (var entry in enemies.soundNameToSoundDictionary)
				this.soundNameToSoundDictionary.Add(entry.Key, entry.Value);

			this.gameOverCountdownInMillis = null;
			this.levelFinishedCountdownInMillis = null;

			this.shouldPlayPlayerShootSound = false;
			this.playerShootSoundCooldownInMillis = 0;

			this.soundsThatNeedToBePlayed = new List<DTDanmakuSound>();
			
			this.bossHealthBar = new BossHealthBar();

			/*
				Note that since these debug things bypass regular game logic, they may break other stuff or crash the program
				(Should be used for debugging / development only)
			*/
			if (this.debugMode)
			{
				this.player.numLivesLeft = 200;
			}

			this.debug_renderHitboxes = false;
		}

		public void SetSoundVolume(int soundVolume)
		{
			this.soundVolume = soundVolume;
		}

		public int GetSoundVolume()
		{
			return this.soundVolume;
		}
		
		public GameLogic GetNextFrame(IKeyboard keyboardInput, IMouse mouseInput, IKeyboard previousKeyboardInput, IMouse previousMouseInput)
		{
			long elapsedMillisPerFrame = 1000 / this.fps;
			this.elapsedTimeMillis += elapsedMillisPerFrame;

			List<Player.PlayerSubFramePosition> playerSubFramePositions = Player.ProcessPlayerMovement(
				player: this.player,
				elapsedMillisPerFrame: elapsedMillisPerFrame,
				keyboardInput: keyboardInput);

			Player.PlayerShootResult playerShootResult = Player.ProcessPlayerShoot(
				player: this.player,
				elapsedMillisPerFrame: elapsedMillisPerFrame,
				keyboardInput: keyboardInput);
			foreach (PlayerBullet newBullet in playerShootResult.PlayerBullets)
				this.playerBullets.Add(newBullet);
			this.shouldPlayPlayerShootSound = playerShootResult.DidShoot || this.shouldPlayPlayerShootSound;
			this.playerShootSoundCooldownInMillis = this.playerShootSoundCooldownInMillis - elapsedMillisPerFrame;
			if (this.playerShootSoundCooldownInMillis < 0)
				this.playerShootSoundCooldownInMillis = 0;

			this.playerBullets = PlayerBullet.ProcessPlayerBulletMovement(
				playerBullets: this.playerBullets,
				elapsedMillisPerFrame: elapsedMillisPerFrame);

			PowerUp.PowerUpActionResult powerUpResult = PowerUp.ProcessPowerUpMovement(
				powerUps: this.powerUps,
				isPlayerDead: this.player.isDead,
				playerXMillis: this.player.xMillis,
				playerYMillis: this.player.yMillis,
				playerPowerUpLevel: this.player.playerPowerUpLevel,
				elapsedMillisPerFrame: elapsedMillisPerFrame);
			this.powerUps = powerUpResult.powerUps;
			this.player.playerPowerUpLevel = powerUpResult.playerPowerUpLevel;
			
			EnemyObjectUpdater.UpdateResult enemyObjectUpdateResult = EnemyObjectUpdater.Update(
				currentEnemyObjects: this.enemyObjects,
				playerXMillis: this.player.xMillis,
				playerYMillis: this.player.yMillis,
				elapsedMillisecondsPerIteration: elapsedMillisPerFrame,
				isPlayerDestroyed: this.player.isDead,
				enemyObjectTemplates: this.enemyObjectTemplates,
				rng: this.rng);

			this.enemyObjects = enemyObjectUpdateResult.NewEnemyObjects;
			foreach (Tuple<long, long> newPowerUp in enemyObjectUpdateResult.NewPowerUps)
				this.powerUps.Add(new PowerUp(xMillis: newPowerUp.Item1, yMillis: newPowerUp.Item2));
			if (enemyObjectUpdateResult.ShouldEndLevel)
			{
				this.levelFinishedCountdownInMillis = 3 * 1000;
			}
			foreach (string newSoundEffect in enemyObjectUpdateResult.NewSoundEffectsToPlay)
			{
				DTDanmakuSound sound = this.soundNameToSoundDictionary[newSoundEffect];
				this.soundsThatNeedToBePlayed.Add(sound);
			}
			
			this.bossHealthBar.ProcessBossHealthBar(
				bossHealthMeterNumber: enemyObjectUpdateResult.BossHealthMeterNumber,
				bossHealthMeterMilliPercentage: enemyObjectUpdateResult.BossHealthMeterMilliPercentage,
				elapsedMillisPerFrame: elapsedMillisPerFrame);

			EnemyObjectMover.UpdateEnemyPositions(
				enemyObjects: this.enemyObjects,
				elapsedMillisecondsPerIteration: elapsedMillisPerFrame);

			bool hasPlayerCollidedWithEnemy = CollisionDetector.HandleCollisionBetweenPlayerAndEnemyObjects(
				playerSubFramePositions: playerSubFramePositions,
				isPlayerDead: this.player.isDead,
				isPlayerInvulnerable: this.player.playerInvincibilityTimeRemainingMillis.HasValue && this.player.playerInvincibilityTimeRemainingMillis.Value > 0,
				enemyObjects: this.enemyObjects);
			if (hasPlayerCollidedWithEnemy)
			{
				this.soundsThatNeedToBePlayed.Add(DTDanmakuSound.PlayerDeath);

				bool stillHasLivesRemaining = this.player.DestroyPlayer(
					enemyObjects: this.enemyObjects,
					elapsedMillisecondsPerIteration: elapsedMillisPerFrame,
					rng: this.rng);

				if (!stillHasLivesRemaining)
					this.gameOverCountdownInMillis = 3 * 1000;
			}

			Player.ProcessPlayerRespawnAndInvincibility(
				player: this.player,
				elapsedMillisPerFrame: elapsedMillisPerFrame);

			this.playerBullets = CollisionDetector.HandleCollisionBetweenPlayerBulletsAndEnemyObjects(
				playerBullets: this.playerBullets,
				enemyObjects: this.enemyObjects);

			if (!this.player.isDead)
			{
				int playerXMillis;
				int playerYMillis;
				unchecked
				{
					playerXMillis = (int) this.player.xMillis;
					playerYMillis = (int) this.player.yMillis;
				}
				this.rng.AddSeed(playerXMillis);
				this.rng.AddSeed(playerYMillis);
			}

			if (this.gameOverCountdownInMillis.HasValue)
			{
				this.gameOverCountdownInMillis = this.gameOverCountdownInMillis.Value - elapsedMillisPerFrame;
				if (this.gameOverCountdownInMillis.Value < 0)
					this.gameOverCountdownInMillis = 0;
			}
			if (this.levelFinishedCountdownInMillis.HasValue)
			{
				this.levelFinishedCountdownInMillis = this.levelFinishedCountdownInMillis.Value - elapsedMillisPerFrame;
				if (this.levelFinishedCountdownInMillis.Value < 0)
					this.levelFinishedCountdownInMillis = 0;
			}
			
			/*
				Note that since these debug things bypass regular game logic, they may break other stuff or crash the program
				(Should be used for debugging / development only)
			*/
			if (this.debugMode)
			{
				if (keyboardInput.IsPressed(Key.One) && !previousKeyboardInput.IsPressed(Key.One))
					this.powerUps.Add(new PowerUp(xMillis: player.xMillis, yMillis: 710 * 1000));

				if (keyboardInput.IsPressed(Key.Two))
				{
					for (int i = 0; i < 500; i++)
						this.powerUps.Add(new PowerUp(xMillis: player.xMillis + (i - 250) * 1000, yMillis: 710 * 1000));
				}

				if (keyboardInput.IsPressed(Key.Three) && !previousKeyboardInput.IsPressed(Key.Three))
					this.gameOverCountdownInMillis = 0;
				
				if (keyboardInput.IsPressed(Key.Four) && !previousKeyboardInput.IsPressed(Key.Four))
					this.levelFinishedCountdownInMillis = 0;

				if (keyboardInput.IsPressed(Key.Five) && !previousKeyboardInput.IsPressed(Key.Five))
					this.debug_renderHitboxes = !this.debug_renderHitboxes;
			}
			
			return this;
		}

		public bool IsGameOver()
		{
			return this.gameOverCountdownInMillis.HasValue && this.gameOverCountdownInMillis.Value <= 0;
		}

		public bool HasPlayerWon()
		{
			return this.levelFinishedCountdownInMillis.HasValue
				&& this.levelFinishedCountdownInMillis.Value <= 0
				&& !this.gameOverCountdownInMillis.HasValue;
		}
		
		public void Render(TranslatedDTDanmakuDisplay display)
		{
			if (this.shouldPlayPlayerShootSound)
			{
				if (this.playerShootSoundCooldownInMillis <= 0)
				{
					display.PlaySound(sound: DTDanmakuSound.PlayerShoot, volume: this.soundVolume);
					this.playerShootSoundCooldownInMillis = 10;
				}
				this.shouldPlayPlayerShootSound = false;
			}

			foreach (DTDanmakuSound sound in this.soundsThatNeedToBePlayed)
			{
				display.PlaySound(sound: sound, volume: this.soundVolume);
			}
			this.soundsThatNeedToBePlayed = new List<DTDanmakuSound>();
			
			// Draw background
			display.DrawRectangle(
				x: 0,
				y: 0,
				width: 1000,
				height: 700,
				color: new DTColor(r: 132, g: 245, b: 255), // teal
				fill: true);

			EnemyObjectRenderer.Render(
				enemyObjects: this.enemyObjects,
				enemyObjectType: EnemyObjectType.Enemy,
				display: display,
				spriteNameToImageDictionary: this.spriteNameToImageDictionary);

			Player.RenderPlayer(
				player: this.player,
				display: display,
				debugMode: this.debugMode,
				debug_renderHitboxes: this.debug_renderHitboxes);

			PowerUp.RenderPowerUps(
				powerUps: this.powerUps,
				display: display);

			PlayerBullet.RenderPlayerBullets(
				playerBullets: this.playerBullets,
				display: display);

			EnemyObjectRenderer.Render(
				enemyObjects: this.enemyObjects,
				enemyObjectType: EnemyObjectType.EnemyBullet,
				display: display,
				spriteNameToImageDictionary: this.spriteNameToImageDictionary);

			// Placeholders shouldn't have any sprites, so this should be a no-op
			EnemyObjectRenderer.Render(
				enemyObjects: this.enemyObjects,
				enemyObjectType: EnemyObjectType.Placeholder,
				display: display,
				spriteNameToImageDictionary: this.spriteNameToImageDictionary);

			Player.RenderPlayerLifeIcons(
				numberOfLivesRemaining: this.player.numLivesLeft,
				display: display);
			
			this.bossHealthBar.RenderBossHealthBar(display: display);

			/*
				Note that since these debug things bypass regular game logic, they may break other stuff or crash the program
				(Should be used for debugging / development only)
			*/
			if (this.debugMode)
			{
				if (this.debug_renderHitboxes)
				{
					foreach (EnemyObject enemy in this.enemyObjects)
					{
						if (enemy.CollisionBoxes != null)
						{
							for (int i = 0; i < enemy.CollisionBoxes.Count; i++)
							{
								display.DrawRectangle(
									x: (int)((enemy.XMillis + enemy.CollisionBoxes[i].LowerXMillis) / 1000L),
									y: (int)(700 - ((enemy.YMillis + enemy.CollisionBoxes[i].UpperYMillis) / 1000L)),
									width: (int)((enemy.CollisionBoxes[i].UpperXMillis - enemy.CollisionBoxes[i].LowerXMillis) / 1000L + 1),
									height: (int)((enemy.CollisionBoxes[i].UpperYMillis - enemy.CollisionBoxes[i].LowerYMillis) / 1000L + 1),
									color: new DTColor(r: 0, g: 0, b: 255, alpha: 50),
									fill: true);
							}
						}
						if (enemy.DamageBoxes != null)
						{
							for (int i = 0; i < enemy.DamageBoxes.Count; i++)
							{
								display.DrawRectangle(
									x: (int)((enemy.XMillis + enemy.DamageBoxes[i].LowerXMillis) / 1000L),
									y: (int)(700 - ((enemy.YMillis + enemy.DamageBoxes[i].UpperYMillis) / 1000L)),
									width: (int)((enemy.DamageBoxes[i].UpperXMillis - enemy.DamageBoxes[i].LowerXMillis) / 1000L + 1),
									height: (int)((enemy.DamageBoxes[i].UpperYMillis - enemy.DamageBoxes[i].LowerYMillis) / 1000L + 1),
									color: new DTColor(r: 0, g: 0, b: 255, alpha: 50),
									fill: true);
							}
						}
					}
					foreach (PlayerBullet playerBullet in this.playerBullets)
					{
						for (int i = 0; i < playerBullet.CollisionBoxes.Count; i++)
						{
							ObjectBox playerBulletObjectBox = playerBullet.CollisionBoxes[i];
							display.DrawRectangle(
								x: (int)((playerBullet.xMillis + playerBulletObjectBox.LowerXMillis) / 1000L),
								y: (int)(700 - ((playerBullet.yMillis + playerBulletObjectBox.UpperYMillis) / 1000L)),
								width: (int)((playerBulletObjectBox.UpperXMillis - playerBulletObjectBox.LowerXMillis) / 1000L + 1),
								height: (int)((playerBulletObjectBox.UpperYMillis - playerBulletObjectBox.LowerYMillis) / 1000L + 1),
								color: new DTColor(r: 255, g: 0, b: 0, alpha: 50),
								fill: true);
						}
					}
				}
			}

			/*
				Note that since these debug things bypass regular game logic, they may break other stuff or crash the program
				(Should be used for debugging / development only)
			*/
			if (this.debugMode)
			{
				display.DebugPrint(
					x: 10,
					y: 10 + 30,
					debugText: "Number of objects: " + (this.powerUps.Count + this.playerBullets.Count + this.enemyObjects.Count));
			}
		}
	}
}
