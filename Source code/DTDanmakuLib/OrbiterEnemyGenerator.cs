
namespace DTDanmakuLib
{
	using System;
	using System.Collections.Generic;

	public class OrbiterEnemyGenerator
	{
		private static ObjectAction GetMoveAction(IMathExpression xMillis)
		{
			ObjectAction teleportToInitialLocation = ObjectAction.SetPosition(
				xMillis: xMillis,
				yMillis: MathExpression.Constant(900 * 1000));

			ObjectAction moveDown = ObjectAction.SetPosition(
				xMillis: MathExpression.XMillis(),
				yMillis: MathExpression.Subtract(MathExpression.YMillis(), MathExpression.Multiply(MathExpression.Constant(35), MathExpression.ElapsedMillisecondsPerIteration())));

			ObjectAction destroyAction = ObjectAction.Condition(
				condition: BooleanExpression.LessThanOrEqualTo(MathExpression.YMillis(), -200 * 1000),
				action: ObjectAction.Destroy());

			return ObjectAction.Union(
				ObjectActionGenerator.DoOnce(teleportToInitialLocation),
				moveDown, 
				destroyAction);
		}
		
		private static ObjectAction GetSpawnOrbiterSatellitesAction(
			Dictionary<string, DTDanmakuImage> spriteNameToImageDictionary,
			Dictionary<string, EnemyObjectTemplate> enemyObjectTemplates,
			GuidGenerator guidGenerator)
		{
			string spriteName = guidGenerator.NextGuid();
			spriteNameToImageDictionary.Add(spriteName, DTDanmakuImage.OrbiterSatellite);

			List<ObjectAction> objectActions = new List<ObjectAction>();

			for (long i = 0; i < 4; i++)
			{
				EnemyObjectTemplate orbiterSatelliteTemplate = EnemyObjectTemplate.EnemyBullet(
					action: ObjectAction.Union(
						GetSatelliteMoveAndDestroyAction(
							initialAngleInMillidegrees: 90L * 1000L * i,
							guidGenerator: guidGenerator),
						GetSatelliteShootAction(
							spriteNameToImageDictionary: spriteNameToImageDictionary,
							enemyObjectTemplates: enemyObjectTemplates,
							guidGenerator: guidGenerator)),
					initialMilliHP: null,
					damageBoxes: null,
					collisionBoxes: null,
					spriteName: spriteName);

				string templateName = guidGenerator.NextGuid();
				enemyObjectTemplates.Add(templateName, orbiterSatelliteTemplate);

				ObjectAction spawnSatelliteAction = ObjectAction.SpawnChild(
					childXMillis: MathExpression.Constant(-1000 * 1000),
					childYMillis: MathExpression.Constant(-1000 * 1000),
					childObjectTemplateName: templateName,
					childInitialNumericVariables: null,
					childInitialBooleanVariables: null);

				objectActions.Add(spawnSatelliteAction);
			}

			return ObjectActionGenerator.DoOnce(ObjectAction.Union(objectActions));
		}
		
		private static ObjectAction GetSatelliteMoveAndDestroyAction(
			long initialAngleInMillidegrees,
			GuidGenerator guidGenerator)
		{
			string angleVariableName = guidGenerator.NextGuid();

			ObjectAction initializeAngleVariableAction = ObjectActionGenerator.DoOnce(
				ObjectAction.SetNumericVariable(angleVariableName, MathExpression.Constant(initialAngleInMillidegrees)));

			ObjectAction updateAngleVariableAction = ObjectAction.Union(
				ObjectAction.SetNumericVariable(
					angleVariableName,
					MathExpression.Add(MathExpression.Variable(angleVariableName), MathExpression.Multiply(MathExpression.Constant(50), MathExpression.ElapsedMillisecondsPerIteration()))),
				ObjectAction.Condition(
					condition: BooleanExpression.GreaterThan(MathExpression.Variable(angleVariableName), MathExpression.Constant(360 * 1000)),
					action: ObjectAction.SetNumericVariable(angleVariableName, MathExpression.Subtract(MathExpression.Variable(angleVariableName), MathExpression.Constant(360 * 1000)))),
				ObjectAction.Condition(
					condition: BooleanExpression.GreaterThan(MathExpression.Variable(angleVariableName), MathExpression.Constant(360 * 1000)),
					action: ObjectAction.SetNumericVariable(angleVariableName, MathExpression.Constant(0))));

			long radius = 110;

			ObjectAction setPositionAction = ObjectAction.SetPosition(
				xMillis: MathExpression.Add(MathExpression.ParentXMillis(), MathExpression.Multiply(MathExpression.Constant(radius), MathExpression.CosineScaled(MathExpression.Variable(angleVariableName)))),
				yMillis: MathExpression.Add(MathExpression.ParentYMillis(), MathExpression.Multiply(MathExpression.Constant(radius), MathExpression.SineScaled(MathExpression.Variable(angleVariableName)))));

			string facingAngleVariableName = guidGenerator.NextGuid();

			ObjectAction initializeFacingAngleVariableAction = ObjectActionGenerator.DoOnce(
				ObjectAction.SetNumericVariable(facingAngleVariableName, MathExpression.RandomInteger(360 * 1000)));
			ObjectAction updateFacingAngleVariableAction = ObjectAction.Union(
				ObjectAction.SetNumericVariable(
					facingAngleVariableName,
					MathExpression.Add(MathExpression.Variable(facingAngleVariableName), MathExpression.Multiply(MathExpression.Constant(500), MathExpression.ElapsedMillisecondsPerIteration()))),
				ObjectAction.Condition(
					condition: BooleanExpression.GreaterThan(MathExpression.Variable(facingAngleVariableName), MathExpression.Constant(360 * 1000)),
					action: ObjectAction.SetNumericVariable(facingAngleVariableName, MathExpression.Subtract(MathExpression.Variable(facingAngleVariableName), MathExpression.Constant(360 * 1000)))),
				ObjectAction.Condition(
					condition: BooleanExpression.GreaterThan(MathExpression.Variable(facingAngleVariableName), MathExpression.Constant(360 * 1000)),
					action: ObjectAction.SetNumericVariable(facingAngleVariableName, MathExpression.Constant(0))));
			ObjectAction setFacingDirectionAction = ObjectAction.SetFacingDirection(MathExpression.Variable(facingAngleVariableName));

			ObjectAction destroyAction = ObjectAction.Condition(
				condition: BooleanExpression.IsParentDestroyed(),
				action: ObjectAction.Destroy());

			return ObjectAction.Union(
				initializeAngleVariableAction,
				updateAngleVariableAction,
				setPositionAction,
				initializeFacingAngleVariableAction,
				updateFacingAngleVariableAction,
				setFacingDirectionAction,
				destroyAction);
		}
		
		private static ObjectAction GetSatelliteShootAction(
			Dictionary<string, DTDanmakuImage> spriteNameToImageDictionary,
			Dictionary<string, EnemyObjectTemplate> enemyObjectTemplates,
			GuidGenerator guidGenerator)
		{
			IMathExpression initialShootCooldownInMillis = MathExpression.RandomInteger(4000);
			IMathExpression shootCooldownInMillis = MathExpression.Add(2000, MathExpression.RandomInteger(2000));

			string cooldownVariableName = guidGenerator.NextGuid();

			ObjectAction initialStartCooldownAction = ObjectAction.SetNumericVariable(cooldownVariableName, initialShootCooldownInMillis);
			ObjectAction startCooldownAction = ObjectAction.SetNumericVariable(cooldownVariableName, shootCooldownInMillis);
			ObjectAction decrementCooldownAction = ObjectAction.SetNumericVariable(cooldownVariableName, MathExpression.Subtract(MathExpression.Variable(cooldownVariableName), MathExpression.ElapsedMillisecondsPerIteration()));
			ObjectAction createBulletAction = SpawnSatelliteBullet(
				spriteNameToImageDictionary: spriteNameToImageDictionary,
				enemyObjectTemplates: enemyObjectTemplates,
				guidGenerator: guidGenerator);
			
			var createBulletWhenCooldownFinishedAction = ObjectAction.Condition(
				condition: BooleanExpression.LessThanOrEqualTo(MathExpression.Variable(cooldownVariableName), MathExpression.Constant(0)),
				action: ObjectAction.Union(startCooldownAction, createBulletAction));

			return ObjectAction.ConditionalNextAction(
				currentAction: initialStartCooldownAction,
				condition: BooleanExpression.True(),
				nextAction: ObjectAction.Union(decrementCooldownAction, createBulletWhenCooldownFinishedAction));
		}
		
		// Spawns 3 bullets (120 degrees apart)
		private static ObjectAction SpawnSatelliteBullet(
			Dictionary<string, DTDanmakuImage> spriteNameToImageDictionary,
			Dictionary<string, EnemyObjectTemplate> enemyObjectTemplates,
			GuidGenerator guidGenerator)
		{
			string singleBulletSpriteName = guidGenerator.NextGuid();
			spriteNameToImageDictionary.Add(singleBulletSpriteName, DTDanmakuImage.OrbiterEnemyBullet);

			long buffer = 100;

			BooleanExpression shouldDestroySingleBullet = BooleanExpression.Or(
				BooleanExpression.GreaterThan(MathExpression.XMillis(), MathExpression.Constant((1000 + buffer) * 1000)),
				BooleanExpression.LessThan(MathExpression.XMillis(), MathExpression.Constant((0 - buffer) * 1000)),
				BooleanExpression.GreaterThan(MathExpression.YMillis(), MathExpression.Constant((700 + buffer) * 1000)),
				BooleanExpression.LessThan(MathExpression.YMillis(), MathExpression.Constant((0 - buffer) * 1000)));
			
			ObjectAction destroySingleBulletAction = ObjectAction.Condition(
				condition: shouldDestroySingleBullet,
				action: ObjectAction.Destroy());

			DTDanmakuMath.MathExpressionOffset singleBulletOffset = DTDanmakuMath.GetOffset(
				millipixels: MathExpression.Multiply(MathExpression.Constant(100), MathExpression.ElapsedMillisecondsPerIteration()),
				movementDirectionInMillidegrees: MathExpression.Variable("direction"));

			List<ObjectBox> collisionBoxes = new List<ObjectBox>();
			collisionBoxes.Add(new ObjectBox(lowerXMillis: -3000, upperXMillis: 3000, lowerYMillis: -5000, upperYMillis: 5000));
			collisionBoxes.Add(new ObjectBox(lowerXMillis: -5000, upperXMillis: 5000, lowerYMillis: -3000, upperYMillis: 3000));

			EnemyObjectTemplate singleBulletTemplate = EnemyObjectTemplate.EnemyBullet(
				action: ObjectAction.Union(
					ObjectAction.SetFacingDirection(MathExpression.Variable("direction")),
					ObjectAction.SetPosition(
						xMillis: MathExpression.Add(MathExpression.XMillis(), singleBulletOffset.DeltaXInMillipixels),
						yMillis: MathExpression.Add(MathExpression.YMillis(), singleBulletOffset.DeltaYInMillipixels)),
					destroySingleBulletAction),
				initialMilliHP: null,
				damageBoxes: null,
				collisionBoxes: collisionBoxes,
				spriteName: singleBulletSpriteName);

			string singleBulletTemplateName = guidGenerator.NextGuid();
			enemyObjectTemplates.Add(singleBulletTemplateName, singleBulletTemplate);

			List<ObjectAction.InitialChildNumericVariableInfo> initialNumericVariables1 = new List<ObjectAction.InitialChildNumericVariableInfo>();
			initialNumericVariables1.Add(new ObjectAction.InitialChildNumericVariableInfo(name: "direction", value: MathExpression.Add(MathExpression.Variable("random offset"), MathExpression.Constant(0))));

			ObjectAction spawnSingleBulletAction1 = ObjectAction.SpawnChild(
				childXMillis: MathExpression.XMillis(),
				childYMillis: MathExpression.YMillis(),
				childObjectTemplateName: singleBulletTemplateName,
				childInitialNumericVariables: initialNumericVariables1,
				childInitialBooleanVariables: null);

			List<ObjectAction.InitialChildNumericVariableInfo> initialNumericVariables2 = new List<ObjectAction.InitialChildNumericVariableInfo>();
			initialNumericVariables2.Add(new ObjectAction.InitialChildNumericVariableInfo(name: "direction", value: MathExpression.Add(MathExpression.Variable("random offset"), MathExpression.Constant(120 * 1000))));

			ObjectAction spawnSingleBulletAction2 = ObjectAction.SpawnChild(
				childXMillis: MathExpression.XMillis(),
				childYMillis: MathExpression.YMillis(),
				childObjectTemplateName: singleBulletTemplateName,
				childInitialNumericVariables: initialNumericVariables2,
				childInitialBooleanVariables: null);

			List<ObjectAction.InitialChildNumericVariableInfo> initialNumericVariables3 = new List<ObjectAction.InitialChildNumericVariableInfo>();
			initialNumericVariables3.Add(new ObjectAction.InitialChildNumericVariableInfo(name: "direction", value: MathExpression.Add(MathExpression.Variable("random offset"), MathExpression.Constant(240 * 1000))));

			ObjectAction spawnSingleBulletAction3 = ObjectAction.SpawnChild(
				childXMillis: MathExpression.XMillis(),
				childYMillis: MathExpression.YMillis(),
				childObjectTemplateName: singleBulletTemplateName,
				childInitialNumericVariables: initialNumericVariables3,
				childInitialBooleanVariables: null);

			EnemyObjectTemplate placeholderTemplate = EnemyObjectTemplate.Placeholder(
				action: ObjectAction.Union(
					ObjectAction.SetNumericVariable("random offset", MathExpression.RandomInteger(MathExpression.Constant(360 * 1000))),
					spawnSingleBulletAction1,
					spawnSingleBulletAction2,
					spawnSingleBulletAction3,
					ObjectAction.Destroy()));

			string placeholderTemplateName = guidGenerator.NextGuid();
			enemyObjectTemplates.Add(placeholderTemplateName, placeholderTemplate);

			return ObjectAction.SpawnChild(
				childXMillis: MathExpression.XMillis(),
				childYMillis: MathExpression.YMillis(),
				childObjectTemplateName: placeholderTemplateName,
				childInitialNumericVariables: null,
				childInitialBooleanVariables: null);
		}
		
		public static ObjectAction SpawnOrbiterEnemy(
			IMathExpression xMillis,
			Dictionary<string, DTDanmakuImage> spriteNameToImageDictionary,
			Dictionary<string, EnemyObjectTemplate> enemyObjectTemplates,
			Dictionary<string, DTDanmakuSound> soundNameToSoundDictionary,
			GuidGenerator guidGenerator)
		{
			ObjectAction moveAction = GetMoveAction(
				xMillis: xMillis);

			ObjectAction destroyAction = ObjectActionGenerator.DestroyWhenHpIsZeroAndMaybeDropPowerUp(
				chanceToDropPowerUpInMilliPercent: 15 * 1000,
				spriteNameToImageDictionary: spriteNameToImageDictionary,
				enemyObjectTemplates: enemyObjectTemplates,
				soundNameToSoundDictionary: soundNameToSoundDictionary,
				guidGenerator: guidGenerator);

			string spriteName = guidGenerator.NextGuid();
			spriteNameToImageDictionary.Add(spriteName, DTDanmakuImage.OrbiterEnemyShip);

			List<ObjectBox> damageBoxes = new List<ObjectBox>();
			damageBoxes.Add(new ObjectBox(lowerXMillis: -68000, upperXMillis: 68000, lowerYMillis: -30000, upperYMillis: 50000));
			damageBoxes.Add(new ObjectBox(lowerXMillis: -46000, upperXMillis: 46000, lowerYMillis: -48000, upperYMillis: 68000));
			damageBoxes.Add(new ObjectBox(lowerXMillis: -26000, upperXMillis: 26000, lowerYMillis: -68000, upperYMillis: 74000));

			List<ObjectBox> collisionBoxes = new List<ObjectBox>();
			collisionBoxes.Add(new ObjectBox(lowerXMillis: -58000, upperXMillis: 58000, lowerYMillis: -35000, upperYMillis: 50000));
			
			ObjectAction spawnOrbiterSatellitesAction = GetSpawnOrbiterSatellitesAction(
				spriteNameToImageDictionary: spriteNameToImageDictionary,
				enemyObjectTemplates: enemyObjectTemplates,
				guidGenerator: guidGenerator);

			EnemyObjectTemplate enemyObjectTemplate = EnemyObjectTemplate.Enemy(
				action: ObjectAction.Union(moveAction, spawnOrbiterSatellitesAction, destroyAction),
				initialMilliHP: MathExpression.Add(MathExpression.Constant(18000), MathExpression.RandomInteger(MathExpression.Constant(15000))),
				damageBoxes: damageBoxes,
				collisionBoxes: collisionBoxes,
				spriteName: spriteName);

			string templateName = guidGenerator.NextGuid();
			enemyObjectTemplates.Add(templateName, enemyObjectTemplate);

			return ObjectAction.SpawnChild(
				childXMillis: MathExpression.Constant(-1000 * 1000),
				childYMillis: MathExpression.Constant(-1000 * 1000),
				childObjectTemplateName: templateName,
				childInitialNumericVariables: null,
				childInitialBooleanVariables: null);
		}
	}
}
