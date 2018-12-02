
namespace DTDanmakuLib
{
	using DTLib;
	using System;
	using System.Collections.Generic;

	public class GenerateEnemiesForLevel
	{
		private static ObjectAction SpawnBasicEnemy(
			long initialXMillis,
			long initialYMillis,
			List<Tuple<long, long>> movementPath,
			Dictionary<string, DTDanmakuImage> spriteNameToImageDictionary,
			Dictionary<string, EnemyObjectTemplate> enemyObjectTemplates,
			Dictionary<string, DTDanmakuSound> soundNameToSoundDictionary,
			GuidGenerator guidGenerator)
		{
			List<ObjectBox> damageBoxes = new List<ObjectBox>();
			damageBoxes.Add(new ObjectBox(lowerXMillis: -46500, upperXMillis: 46500, lowerYMillis: 0, upperYMillis: 40000));
			damageBoxes.Add(new ObjectBox(lowerXMillis: -31500, upperXMillis: 31500, lowerYMillis: -25000, upperYMillis: 40000));
			damageBoxes.Add(new ObjectBox(lowerXMillis: -21500, upperXMillis: 21500, lowerYMillis: -35000, upperYMillis: 40000));

			List<ObjectBox> collisionBoxes = new List<ObjectBox>();
			collisionBoxes.Add(new ObjectBox(lowerXMillis: -10000, upperXMillis: 10000, lowerYMillis: -25000, upperYMillis: 25000));
			collisionBoxes.Add(new ObjectBox(lowerXMillis: -25000, upperXMillis: 25000, lowerYMillis: -10000, upperYMillis: 10000));

			return ObjectActionGenerator.SpawnEnemyThatMovesToSpecificLocation(
				initialXMillis: initialXMillis,
				initialYMillis: initialYMillis,
				movementPath: movementPath,
				movementSpeedInPixelsPerSecond: 100,
				shouldStrafe: true,
				bulletXOffset: 0,
				bulletYOffset: -52000,
				initialShootCooldownInMillis: MathExpression.RandomInteger(5000),
				shootCooldownInMillis: MathExpression.Add(3000, MathExpression.RandomInteger(2000)),
				bulletSpeedInPixelsPerSecond: 250,
				initialMilliHP: MathExpression.Add(MathExpression.Constant(6000), MathExpression.RandomInteger(MathExpression.Constant(5000))),
				chanceToDropPowerUpInMilliPercent: 15 * 1000,
				damageBoxes: damageBoxes,
				collisionBoxes: collisionBoxes,
				sprite: DTDanmakuImage.BasicEnemyShip,
				spriteNameToImageDictionary: spriteNameToImageDictionary,
				enemyObjectTemplates: enemyObjectTemplates,
				soundNameToSoundDictionary: soundNameToSoundDictionary,
				guidGenerator: guidGenerator);
		}

		private static EnemyObject CreateActionExecutor(
			long millisecondsToWait,
			ObjectAction action,
			long elapsedMillisecondsPerIteration,
			IDTDeterministicRandom rng,
			GuidGenerator guidGenerator)
		{
			return new EnemyObject(
				template: EnemyObjectTemplate.Placeholder(
					action: ObjectActionGenerator.Delay(
						action: ObjectAction.Union(action, ObjectAction.Destroy()),
						milliseconds: millisecondsToWait,
						guidGenerator: guidGenerator)),
				initialXMillis: 0,
				initialYMillis: 0,
				playerXMillis: 0,
				playerYMillis: 0,
				elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
				isPlayerDestroyed: false,
				parent: null,
				initialNumericVariables: null,
				initialBooleanVariables: null,
				rng: rng);
		}

		private static ObjectAction GenerateWave1Enemy(
			Dictionary<string, DTDanmakuImage> spriteNameToImageDictionary,
			Dictionary<string, EnemyObjectTemplate> enemyObjectTemplates,
			Dictionary<string, DTDanmakuSound> soundNameToSoundDictionary,
			GuidGenerator guidGenerator)
		{
			List<Tuple<long, long>> movementPath = new List<Tuple<long, long>>();
			movementPath.Add(new Tuple<long, long>(1100 * 1000, 600 * 1000));

			return SpawnBasicEnemy(
				initialXMillis: -100 * 1000,
				initialYMillis: 600 * 1000,
				movementPath: movementPath,
				spriteNameToImageDictionary: spriteNameToImageDictionary,
				enemyObjectTemplates: enemyObjectTemplates,
				soundNameToSoundDictionary: soundNameToSoundDictionary,
				guidGenerator: guidGenerator);
		}

		private static ObjectAction GenerateWave2Enemy(
			Dictionary<string, DTDanmakuImage> spriteNameToImageDictionary,
			Dictionary<string, EnemyObjectTemplate> enemyObjectTemplates,
			Dictionary<string, DTDanmakuSound> soundNameToSoundDictionary,
			GuidGenerator guidGenerator)
		{
			List<Tuple<long, long>> movementPath = new List<Tuple<long, long>>();
			movementPath.Add(new Tuple<long, long>(-100 * 1000, 500 * 1000));

			return SpawnBasicEnemy(
				initialXMillis: 1100 * 1000,
				initialYMillis: 500 * 1000,
				movementPath: movementPath,
				spriteNameToImageDictionary: spriteNameToImageDictionary,
				enemyObjectTemplates: enemyObjectTemplates,
				soundNameToSoundDictionary: soundNameToSoundDictionary,
				guidGenerator: guidGenerator);
		}

		private static ObjectAction GenerateWave3Enemy(
			Dictionary<string, DTDanmakuImage> spriteNameToImageDictionary,
			Dictionary<string, EnemyObjectTemplate> enemyObjectTemplates,
			Dictionary<string, DTDanmakuSound> soundNameToSoundDictionary,
			GuidGenerator guidGenerator)
		{
			List<Tuple<long, long>> movementPath = new List<Tuple<long, long>>();
			movementPath.Add(new Tuple<long, long>(400 * 1000, -100 * 1000));

			return SpawnBasicEnemy(
				initialXMillis: 400 * 1000,
				initialYMillis: 800 * 1000,
				movementPath: movementPath,
				spriteNameToImageDictionary: spriteNameToImageDictionary,
				enemyObjectTemplates: enemyObjectTemplates,
				soundNameToSoundDictionary: soundNameToSoundDictionary,
				guidGenerator: guidGenerator);
		}

		private static ObjectAction GenerateWave4Enemy(
			Dictionary<string, DTDanmakuImage> spriteNameToImageDictionary,
			Dictionary<string, EnemyObjectTemplate> enemyObjectTemplates,
			Dictionary<string, DTDanmakuSound> soundNameToSoundDictionary,
			GuidGenerator guidGenerator)
		{
			List<Tuple<long, long>> movementPath = new List<Tuple<long, long>>();
			movementPath.Add(new Tuple<long, long>(800 * 1000, -100 * 1000));

			return SpawnBasicEnemy(
				initialXMillis: 800 * 1000,
				initialYMillis: 800 * 1000,
				movementPath: movementPath,
				spriteNameToImageDictionary: spriteNameToImageDictionary,
				enemyObjectTemplates: enemyObjectTemplates,
				soundNameToSoundDictionary: soundNameToSoundDictionary,
				guidGenerator: guidGenerator);
		}

		public static GenerateEnemiesResult GenerateEnemies(
			long elapsedMillisecondsPerIteration,
			IDTDeterministicRandom rng,
			GuidGenerator guidGenerator)
		{
			List<EnemyObject> enemyObjects = new List<EnemyObject>();
			Dictionary<string, DTDanmakuImage> spriteNameToImageDictionary = new Dictionary<string, DTDanmakuImage>();
			Dictionary<string, EnemyObjectTemplate> enemyObjectTemplates = new Dictionary<string, EnemyObjectTemplate>();
			Dictionary<string, DTDanmakuSound> soundNameToSoundDictionary = new Dictionary<string, DTDanmakuSound>();
			
			ObjectAction spawnWave1Enemy = GenerateWave1Enemy(
				spriteNameToImageDictionary: spriteNameToImageDictionary,
				enemyObjectTemplates: enemyObjectTemplates,
				soundNameToSoundDictionary: soundNameToSoundDictionary,
				guidGenerator: guidGenerator);

			ObjectAction spawnWave2Enemy = GenerateWave2Enemy(
				spriteNameToImageDictionary: spriteNameToImageDictionary,
				enemyObjectTemplates: enemyObjectTemplates,
				soundNameToSoundDictionary: soundNameToSoundDictionary,
				guidGenerator: guidGenerator);

			ObjectAction spawnWave3Enemy = GenerateWave3Enemy(
				spriteNameToImageDictionary: spriteNameToImageDictionary,
				enemyObjectTemplates: enemyObjectTemplates,
				soundNameToSoundDictionary: soundNameToSoundDictionary,
				guidGenerator: guidGenerator);

			ObjectAction spawnWave4Enemy = GenerateWave4Enemy(
				spriteNameToImageDictionary: spriteNameToImageDictionary,
				enemyObjectTemplates: enemyObjectTemplates,
				soundNameToSoundDictionary: soundNameToSoundDictionary,
				guidGenerator: guidGenerator);

			ObjectAction spawnSniperEnemy = SniperEnemyGenerator.SpawnSniperEnemy(
				spriteNameToImageDictionary: spriteNameToImageDictionary,
				enemyObjectTemplates: enemyObjectTemplates,
				soundNameToSoundDictionary: soundNameToSoundDictionary,
				guidGenerator: guidGenerator);

			ObjectAction spawnEliteSniperEnemy = EliteSniperEnemyGenerator.SpawnEliteSniperEnemy(
				spriteNameToImageDictionary: spriteNameToImageDictionary,
				enemyObjectTemplates: enemyObjectTemplates,
				soundNameToSoundDictionary: soundNameToSoundDictionary,
				guidGenerator: guidGenerator);

			ObjectAction spawnOrbiterEnemy = OrbiterEnemyGenerator.SpawnOrbiterEnemy(
				xMillis: MathExpression.Add(MathExpression.Constant(300 * 1000), MathExpression.RandomInteger(400 * 1000)),
				spriteNameToImageDictionary: spriteNameToImageDictionary,
				enemyObjectTemplates: enemyObjectTemplates,
				soundNameToSoundDictionary: soundNameToSoundDictionary,
				guidGenerator: guidGenerator);

			ObjectAction spawnEliteOrbiterEnemy = EliteOrbiterEnemyGenerator.SpawnEliteOrbiterEnemy(
				xMillis: MathExpression.Add(MathExpression.Constant(300 * 1000), MathExpression.RandomInteger(400 * 1000)),
				spriteNameToImageDictionary: spriteNameToImageDictionary,
				enemyObjectTemplates: enemyObjectTemplates,
				soundNameToSoundDictionary: soundNameToSoundDictionary,
				guidGenerator: guidGenerator);

			ObjectAction spawnBarrageEnemy = BarrageEnemyGenerator.SpawnBarrageEnemy(
				xMillis: MathExpression.Add(MathExpression.Constant(300 * 1000), MathExpression.RandomInteger(400 * 1000)),
				spriteNameToImageDictionary: spriteNameToImageDictionary,
				enemyObjectTemplates: enemyObjectTemplates,
				soundNameToSoundDictionary: soundNameToSoundDictionary,
				guidGenerator: guidGenerator);

			ObjectAction spawnEliteBarrageEnemy = EliteBarrageEnemyGenerator.SpawnEliteBarrageEnemy(
				xMillis: MathExpression.Add(MathExpression.Constant(300 * 1000), MathExpression.RandomInteger(400 * 1000)),
				spriteNameToImageDictionary: spriteNameToImageDictionary,
				enemyObjectTemplates: enemyObjectTemplates,
				soundNameToSoundDictionary: soundNameToSoundDictionary,
				guidGenerator: guidGenerator);
			
			ObjectAction spawnBoss = BossEnemyGenerator.SpawnBossEnemy(
				spriteNameToImageDictionary: spriteNameToImageDictionary,
				enemyObjectTemplates: enemyObjectTemplates,
				soundNameToSoundDictionary: soundNameToSoundDictionary,
				guidGenerator: guidGenerator);
			
			for (int i = 0; i < 10; i++)
				enemyObjects.Add(CreateActionExecutor(
					millisecondsToWait: 1000 * i,
					action: spawnWave1Enemy,
					elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
					rng: rng,
					guidGenerator: guidGenerator));

			for (int i = 0; i < 10; i++)
				enemyObjects.Add(CreateActionExecutor(
					millisecondsToWait: 3000 + 1000 * i,
					action: spawnWave2Enemy,
					elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
					rng: rng,
					guidGenerator: guidGenerator));

			for (int i = 0; i < 6; i++)
				enemyObjects.Add(CreateActionExecutor(
					millisecondsToWait: 16000 + 1000 * i,
					action: spawnSniperEnemy,
					elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
					rng: rng,
					guidGenerator: guidGenerator));

			for (int i = 0; i < 6; i++)
				enemyObjects.Add(CreateActionExecutor(
					millisecondsToWait: 20000 + 6000 * i,
					action: spawnOrbiterEnemy,
					elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
					rng: rng,
					guidGenerator: guidGenerator));

			for (int i = 0; i < 6; i++)
				enemyObjects.Add(CreateActionExecutor(
					millisecondsToWait: 23000 + 1500 * i,
					action: spawnWave3Enemy,
					elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
					rng: rng,
					guidGenerator: guidGenerator));

			for (int i = 0; i < 6; i++)
				enemyObjects.Add(CreateActionExecutor(
					millisecondsToWait: 25500 + 1500 * i,
					action: spawnWave4Enemy,
					elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
					rng: rng,
					guidGenerator: guidGenerator));

			for (int i = 0; i < 4; i++)
				enemyObjects.Add(CreateActionExecutor(
					millisecondsToWait: 29000 + 1700 * i,
					action: spawnBarrageEnemy,
					elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
					rng: rng,
					guidGenerator: guidGenerator));
				
			for (int i = 0; i < 7; i++)
				enemyObjects.Add(CreateActionExecutor(
					millisecondsToWait: 33000 + 1250 * i,
					action: spawnEliteSniperEnemy,
					elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
					rng: rng,
					guidGenerator: guidGenerator));

			for (int i = 0; i < 4; i++)
				enemyObjects.Add(CreateActionExecutor(
					millisecondsToWait: 38000 + 2100 * i,
					action: spawnEliteBarrageEnemy,
					elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
					rng: rng,
					guidGenerator: guidGenerator));
				
			for (int i = 0; i < 8; i++)
				enemyObjects.Add(CreateActionExecutor(
					millisecondsToWait: 43000 + 500 * i,
					action: spawnSniperEnemy,
					elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
					rng: rng,
					guidGenerator: guidGenerator));

			for (int i = 0; i < 8; i++)
				enemyObjects.Add(CreateActionExecutor(
					millisecondsToWait: 43250 + 500 * i,
					action: spawnEliteSniperEnemy,
					elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
					rng: rng,
					guidGenerator: guidGenerator));

			for (int i = 0; i < 2; i++)
				enemyObjects.Add(CreateActionExecutor(
					millisecondsToWait: 56000 + 6000 * i,
					action: spawnEliteOrbiterEnemy,
					elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
					rng: rng,
					guidGenerator: guidGenerator));
				
			enemyObjects.Add(CreateActionExecutor(
				millisecondsToWait: 82000,
				action: spawnBoss,
				elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
				rng: rng,
				guidGenerator: guidGenerator));
			
			return new GenerateEnemiesResult(
				enemyObjects: enemyObjects,
				spriteNameToImageDictionary: spriteNameToImageDictionary,
				enemyObjectTemplates: enemyObjectTemplates,
				soundNameToSoundDictionary: soundNameToSoundDictionary);
		}
	}
}
