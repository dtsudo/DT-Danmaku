
namespace DTDanmakuLib
{
	using System.Collections.Generic;

	public class EliteBarrageEnemyGenerator
	{
		private static ObjectAction GetMoveAndSetCanShootVariableAction(
			IMathExpression xMillis,
			GuidGenerator guidGenerator)
		{
			string variableName = guidGenerator.NextGuid();

			ObjectAction initializeVariable = ObjectAction.SetNumericVariable(
				variableName: variableName,
				variableValue: MathExpression.Constant(0));

			ObjectAction incrementVariable = ObjectAction.SetNumericVariable(
				variableName: variableName,
				variableValue: MathExpression.Add(MathExpression.Variable(variableName), MathExpression.ElapsedMillisecondsPerIteration()));

			ObjectAction initializeAndIncrementVariable = ObjectAction.ConditionalNextAction(
				currentAction: initializeVariable,
				condition: BooleanExpression.True(),
				nextAction: incrementVariable);

			ObjectAction teleportToInitialLocation = ObjectAction.SetPosition(
				xMillis: xMillis,
				yMillis: MathExpression.Constant(800 * 1000));

			ObjectAction moveDownSpeed1 = ObjectAction.SetPosition(
				xMillis: MathExpression.XMillis(),
				yMillis: MathExpression.Subtract(MathExpression.YMillis(), MathExpression.Multiply(MathExpression.Constant(100), MathExpression.ElapsedMillisecondsPerIteration())));

			ObjectAction moveDownSpeed2 = ObjectAction.SetPosition(
				xMillis: MathExpression.XMillis(),
				yMillis: MathExpression.Subtract(MathExpression.YMillis(), MathExpression.Multiply(MathExpression.Constant(70), MathExpression.ElapsedMillisecondsPerIteration())));

			ObjectAction moveDownSpeed3 = ObjectAction.SetPosition(
				xMillis: MathExpression.XMillis(),
				yMillis: MathExpression.Subtract(MathExpression.YMillis(), MathExpression.Multiply(MathExpression.Constant(30), MathExpression.ElapsedMillisecondsPerIteration())));

			ObjectAction stopMovement = ObjectAction.Union(
				ObjectActionGenerator.Noop(),
				ObjectActionGenerator.DoOnce(ObjectAction.SetBooleanVariable("can shoot", BooleanExpression.True())));

			ObjectAction moveUpSpeed1 = ObjectAction.Union(
				ObjectActionGenerator.DoOnce(ObjectAction.SetBooleanVariable("can shoot", BooleanExpression.False())),
				ObjectAction.SetPosition(
					xMillis: MathExpression.XMillis(),
					yMillis: MathExpression.Add(MathExpression.YMillis(), MathExpression.Multiply(MathExpression.Constant(30), MathExpression.ElapsedMillisecondsPerIteration()))));

			ObjectAction moveUpSpeed2 = ObjectAction.SetPosition(
				xMillis: MathExpression.XMillis(),
				yMillis: MathExpression.Add(MathExpression.YMillis(), MathExpression.Multiply(MathExpression.Constant(70), MathExpression.ElapsedMillisecondsPerIteration())));

			ObjectAction moveUpSpeed3 = ObjectAction.SetPosition(
				xMillis: MathExpression.XMillis(),
				yMillis: MathExpression.Add(MathExpression.YMillis(), MathExpression.Multiply(MathExpression.Constant(100), MathExpression.ElapsedMillisecondsPerIteration())));

			ObjectAction destroyAction = ObjectAction.Condition(
				condition: BooleanExpression.GreaterThanOrEqualTo(MathExpression.YMillis(), MathExpression.Constant(800 * 1000)),
				action: ObjectAction.Destroy());

			return ObjectAction.Union(
				ObjectActionGenerator.DoOnce(ObjectAction.SetBooleanVariable("can shoot", BooleanExpression.False())),
				initializeAndIncrementVariable,
				ObjectActionGenerator.DoOnce(teleportToInitialLocation),
				ObjectAction.ConditionalNextAction(
					currentAction: moveDownSpeed1,
					condition: BooleanExpression.GreaterThanOrEqualTo(MathExpression.Variable(variableName), MathExpression.Constant(3000)),
					nextAction: ObjectAction.ConditionalNextAction(
						currentAction: moveDownSpeed2,
						condition: BooleanExpression.GreaterThanOrEqualTo(MathExpression.Variable(variableName), MathExpression.Constant(4000)),
						nextAction: ObjectAction.ConditionalNextAction(
							currentAction: moveDownSpeed3,
							condition: BooleanExpression.GreaterThanOrEqualTo(MathExpression.Variable(variableName), MathExpression.Constant(5000)),
							nextAction: ObjectAction.ConditionalNextAction(
								currentAction: stopMovement,
								condition: BooleanExpression.GreaterThanOrEqualTo(MathExpression.Variable(variableName), MathExpression.Constant(12000)),
								nextAction: ObjectAction.ConditionalNextAction(
									currentAction: moveUpSpeed1,
									condition: BooleanExpression.GreaterThanOrEqualTo(MathExpression.Variable(variableName), MathExpression.Constant(13000)),
									nextAction: ObjectAction.ConditionalNextAction(
										currentAction: moveUpSpeed2,
										condition: BooleanExpression.GreaterThanOrEqualTo(MathExpression.Variable(variableName), MathExpression.Constant(14000)),
										nextAction: ObjectAction.Union(moveUpSpeed3, destroyAction))))))));
		}
		
		private static ObjectAction SpawnBullet(
			IMathExpression bulletDirectionInMillidegrees,
			Dictionary<string, DTDanmakuImage> spriteNameToImageDictionary,
			Dictionary<string, EnemyObjectTemplate> enemyObjectTemplates,
			GuidGenerator guidGenerator)
		{
			string bulletSpriteName = guidGenerator.NextGuid();
			spriteNameToImageDictionary.Add(bulletSpriteName, DTDanmakuImage.EliteBarrageEnemyBullet);

			string snapshotBulletDirectionInMillidegreesVariable = guidGenerator.NextGuid();

			ObjectAction snapshotDirectionAction = ObjectActionGenerator.DoOnce(ObjectAction.SetNumericVariable(
				snapshotBulletDirectionInMillidegreesVariable,
				bulletDirectionInMillidegrees));

			long buffer = 100;

			BooleanExpression shouldDestroy = BooleanExpression.Or(
				BooleanExpression.GreaterThan(MathExpression.XMillis(), MathExpression.Constant((1000 + buffer) * 1000)),
				BooleanExpression.LessThan(MathExpression.XMillis(), MathExpression.Constant((0 - buffer) * 1000)),
				BooleanExpression.GreaterThan(MathExpression.YMillis(), MathExpression.Constant((700 + buffer) * 1000)),
				BooleanExpression.LessThan(MathExpression.YMillis(), MathExpression.Constant((0 - buffer) * 1000)));

			ObjectAction destroyAction = ObjectAction.Condition(
				condition: shouldDestroy,
				action: ObjectAction.Destroy());

			DTDanmakuMath.MathExpressionOffset offset = DTDanmakuMath.GetOffset(
				millipixels: MathExpression.Multiply(MathExpression.Constant(200), MathExpression.ElapsedMillisecondsPerIteration()),
				movementDirectionInMillidegrees: MathExpression.Variable(snapshotBulletDirectionInMillidegreesVariable));

			List<ObjectBox> collisionBoxes = new List<ObjectBox>();
			collisionBoxes.Add(new ObjectBox(lowerXMillis: -3000, upperXMillis: 3000, lowerYMillis: -5000, upperYMillis: 5000));
			collisionBoxes.Add(new ObjectBox(lowerXMillis: -5000, upperXMillis: 5000, lowerYMillis: -3000, upperYMillis: 3000));

			EnemyObjectTemplate bulletTemplate = EnemyObjectTemplate.EnemyBullet(
				action: ObjectAction.Union(
					snapshotDirectionAction,
					ObjectAction.SetFacingDirection(MathExpression.Variable(snapshotBulletDirectionInMillidegreesVariable)),
					ObjectAction.SetPosition(
						xMillis: MathExpression.Add(MathExpression.XMillis(), offset.DeltaXInMillipixels),
						yMillis: MathExpression.Add(MathExpression.YMillis(), offset.DeltaYInMillipixels)),
					destroyAction),
				initialMilliHP: null,
				damageBoxes: null,
				collisionBoxes: collisionBoxes,
				spriteName: bulletSpriteName);

			string templateName = guidGenerator.NextGuid();
			enemyObjectTemplates.Add(templateName, bulletTemplate);
			
			return ObjectAction.SpawnChild(
				childXMillis: MathExpression.XMillis(),
				childYMillis: MathExpression.Subtract(MathExpression.YMillis(), MathExpression.Constant(25000)),
				childObjectTemplateName: templateName,
				childInitialNumericVariables: null,
				childInitialBooleanVariables: null);
		}
		
		private static ObjectAction GetShootBulletAction(
			Dictionary<string, DTDanmakuImage> spriteNameToImageDictionary,
			Dictionary<string, EnemyObjectTemplate> enemyObjectTemplates,
			GuidGenerator guidGenerator)
		{
			string angleVariable = guidGenerator.NextGuid();
			string shootCooldownVariable = guidGenerator.NextGuid();

			ObjectAction initializeAngleVariable = ObjectActionGenerator.DoOnce(
				ObjectAction.SetNumericVariable(angleVariable, MathExpression.RandomInteger(360 * 1000)));

			ObjectAction updateAngleVariableAction = ObjectAction.Union(
				ObjectAction.SetNumericVariable(
					angleVariable,
					MathExpression.Add(MathExpression.Variable(angleVariable), MathExpression.Multiply(MathExpression.Constant(400), MathExpression.ElapsedMillisecondsPerIteration()))),
				ObjectAction.Condition(
					condition: BooleanExpression.GreaterThan(MathExpression.Variable(angleVariable), MathExpression.Constant(360 * 1000)),
					action: ObjectAction.SetNumericVariable(angleVariable, MathExpression.Subtract(MathExpression.Variable(angleVariable), MathExpression.Constant(360 * 1000)))),
				ObjectAction.Condition(
					condition: BooleanExpression.GreaterThan(MathExpression.Variable(angleVariable), MathExpression.Constant(360 * 1000)),
					action: ObjectAction.SetNumericVariable(angleVariable, MathExpression.Constant(0))));

			ObjectAction createBulletAction = ObjectAction.Condition(
				condition: BooleanExpression.Variable("can shoot"),
				action: SpawnBullet(
					bulletDirectionInMillidegrees: MathExpression.Multiply(MathExpression.ParentVariable(angleVariable), MathExpression.Constant(-1)),
					spriteNameToImageDictionary: spriteNameToImageDictionary,
					enemyObjectTemplates: enemyObjectTemplates,
					guidGenerator: guidGenerator));

			IMathExpression shootCooldownInMillis = MathExpression.Constant(20);
			ObjectAction startCooldownAction = ObjectAction.SetNumericVariable(shootCooldownVariable, shootCooldownInMillis);
			ObjectAction decrementCooldownAction = ObjectAction.SetNumericVariable(shootCooldownVariable, MathExpression.Subtract(MathExpression.Variable(shootCooldownVariable), MathExpression.ElapsedMillisecondsPerIteration()));
			ObjectAction createBulletWhenCooldownFinishedAction = ObjectAction.Condition(
				condition: BooleanExpression.LessThanOrEqualTo(MathExpression.Variable(shootCooldownVariable), MathExpression.Constant(0)),
				action: ObjectAction.Union(startCooldownAction, createBulletAction));

			return ObjectAction.Union(
				initializeAngleVariable,
				updateAngleVariableAction,
				ObjectActionGenerator.DoOnce(startCooldownAction),
				decrementCooldownAction,
				createBulletWhenCooldownFinishedAction);
		}
		
		public static ObjectAction SpawnEliteBarrageEnemy(
			IMathExpression xMillis,
			Dictionary<string, DTDanmakuImage> spriteNameToImageDictionary,
			Dictionary<string, EnemyObjectTemplate> enemyObjectTemplates,
			Dictionary<string, DTDanmakuSound> soundNameToSoundDictionary,
			GuidGenerator guidGenerator)
		{
			// Should be called first so it sets the "can shoot" variable that the shootBullet action will need
			ObjectAction moveAction = GetMoveAndSetCanShootVariableAction(
				xMillis: xMillis,
				guidGenerator: guidGenerator);

			ObjectAction shootAction = GetShootBulletAction(
				spriteNameToImageDictionary: spriteNameToImageDictionary,
				enemyObjectTemplates: enemyObjectTemplates,
				guidGenerator: guidGenerator);

			ObjectAction destroyAction = ObjectActionGenerator.DestroyWhenHpIsZeroAndMaybeDropPowerUp(
				chanceToDropPowerUpInMilliPercent: 15 * 1000,
				spriteNameToImageDictionary: spriteNameToImageDictionary,
				enemyObjectTemplates: enemyObjectTemplates,
				soundNameToSoundDictionary: soundNameToSoundDictionary,
				guidGenerator: guidGenerator);

			string spriteName = guidGenerator.NextGuid();
			spriteNameToImageDictionary.Add(spriteName, DTDanmakuImage.EliteBarrageEnemyShip);

			List<ObjectBox> damageBoxes = new List<ObjectBox>();
			damageBoxes.Add(new ObjectBox(lowerXMillis: -46500, upperXMillis: 46500, lowerYMillis: 0, upperYMillis: 40000));
			damageBoxes.Add(new ObjectBox(lowerXMillis: -31500, upperXMillis: 31500, lowerYMillis: -35000, upperYMillis: 30000));

			List<ObjectBox> collisionBoxes = new List<ObjectBox>();
			collisionBoxes.Add(new ObjectBox(lowerXMillis: -10000, upperXMillis: 10000, lowerYMillis: -25000, upperYMillis: 25000));
			collisionBoxes.Add(new ObjectBox(lowerXMillis: -25000, upperXMillis: 25000, lowerYMillis: -10000, upperYMillis: 10000));

			EnemyObjectTemplate enemyObjectTemplate = EnemyObjectTemplate.Enemy(
				action: ObjectAction.Union(moveAction, shootAction, destroyAction),
				initialMilliHP: MathExpression.Add(MathExpression.Constant(58000), MathExpression.RandomInteger(MathExpression.Constant(10000))),
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
