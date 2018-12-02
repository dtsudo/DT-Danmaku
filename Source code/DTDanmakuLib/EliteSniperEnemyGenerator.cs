
namespace DTDanmakuLib
{
	using System;
	using System.Collections.Generic;

	public class EliteSniperEnemyGenerator
	{
		private static ObjectAction GetMoveAction(
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
				xMillis: MathExpression.Add(50 * 1000, MathExpression.RandomInteger(900 * 1000)),
				yMillis: MathExpression.Constant(800 * 1000));

			ObjectAction faceTowardsPlayer = ObjectAction.Move(
				moveToXMillis: MathExpression.PlayerXMillis(),
				moveToYMillis: MathExpression.Min(MathExpression.PlayerYMillis(), 300 * 1000));

			ObjectAction setInitialSpeed = ObjectActionGenerator.DoOnce(
				ObjectAction.SetSpeed(
					speedInMillipixelsPerMillisecond: MathExpression.Constant(60)));

			ObjectAction slowDownSpeed1 = ObjectAction.Condition(
				condition: BooleanExpression.GreaterThanOrEqualTo(
					MathExpression.Variable(variableName),
					1 * 1000),
				action: ObjectActionGenerator.DoOnce(
					ObjectAction.SetSpeed(
						speedInMillipixelsPerMillisecond: MathExpression.Constant(40))));

			ObjectAction slowDownSpeed2 = ObjectAction.Condition(
				condition: BooleanExpression.GreaterThanOrEqualTo(
					MathExpression.Variable(variableName),
					2 * 1000),
				action: ObjectActionGenerator.DoOnce(
					ObjectAction.SetSpeed(
						speedInMillipixelsPerMillisecond: MathExpression.Constant(20))));

			ObjectAction stopMovement = ObjectAction.Condition(
				condition: BooleanExpression.GreaterThanOrEqualTo(
					MathExpression.Variable(variableName),
					3 * 1000),
				action: ObjectActionGenerator.DoOnce(
					ObjectAction.SetSpeed(
						speedInMillipixelsPerMillisecond: MathExpression.Constant(0))));

			BooleanExpression ShouldLeave1 = BooleanExpression.GreaterThanOrEqualTo(
				MathExpression.Variable(variableName),
				10 * 1000);

			ObjectAction leave1 = ObjectAction.SetPosition(
				xMillis: MathExpression.XMillis(),
				yMillis: MathExpression.Add(MathExpression.YMillis(), MathExpression.Multiply(MathExpression.ElapsedMillisecondsPerIteration(), MathExpression.Constant(25))));

			ObjectAction leave1ConditionalAction = ObjectAction.Condition(
				condition: ShouldLeave1,
				action: leave1);

			BooleanExpression shouldLeave2 = BooleanExpression.GreaterThanOrEqualTo(
				MathExpression.Variable(variableName),
				11 * 1000);

			ObjectAction leave2 = ObjectAction.SetPosition(
				xMillis: MathExpression.XMillis(),
				yMillis: MathExpression.Add(MathExpression.YMillis(), MathExpression.Multiply(MathExpression.ElapsedMillisecondsPerIteration(), MathExpression.Constant(50))));

			ObjectAction leave2ConditionalAction = ObjectAction.ConditionalNextAction(
				currentAction: leave1ConditionalAction,
				condition: shouldLeave2,
				nextAction: leave2);

			ObjectAction shouldDestroy = ObjectAction.Condition(
				condition: BooleanExpression.And(shouldLeave2, BooleanExpression.GreaterThanOrEqualTo(MathExpression.YMillis(), 800 * 1000)),
				action: ObjectAction.Destroy());

			return ObjectAction.ConditionalNextAction(
				currentAction: teleportToInitialLocation,
				condition: BooleanExpression.True(),
				nextAction: ObjectAction.Union(faceTowardsPlayer, initializeAndIncrementVariable, setInitialSpeed, slowDownSpeed1, slowDownSpeed2, stopMovement, leave2ConditionalAction, shouldDestroy));
		}

		private static ObjectAction GetBulletAnimationAction(
			GuidGenerator guidGenerator)
		{
			string variableName = guidGenerator.NextGuid();

			return ObjectAction.Union(
				ObjectActionGenerator.DoOnce(ObjectAction.SetNumericVariable(variableName, MathExpression.RandomInteger(360 * 1000))),
				ObjectAction.SetNumericVariable(variableName, MathExpression.Add(MathExpression.Variable(variableName), MathExpression.Multiply(MathExpression.ElapsedMillisecondsPerIteration(), MathExpression.Constant(315)))),
				ObjectAction.Condition(
					condition: BooleanExpression.GreaterThan(MathExpression.Variable(variableName), MathExpression.Constant(360 * 1000)),
					action: ObjectAction.SetNumericVariable(variableName, MathExpression.Subtract(MathExpression.Variable(variableName), MathExpression.Constant(360 * 1000)))),
				ObjectAction.Condition(
					condition: BooleanExpression.GreaterThan(MathExpression.Variable(variableName), MathExpression.Constant(360 * 1000)),
					action: ObjectAction.SetNumericVariable(variableName, MathExpression.Constant(0))),
				ObjectAction.SetFacingDirection(MathExpression.Multiply(MathExpression.Variable(variableName), MathExpression.Constant(-1))));
		}

		private static ObjectAction GetBulletMovementAction(
			GuidGenerator guidGenerator)
		{
			long bulletSpeedInPixelsPerSecond = 250;

			ObjectAction moveAction = ObjectActionGenerator.MoveTowardsLocation(
				currentX: MathExpression.XMillis(),
				currentY: MathExpression.YMillis(),
				desiredX: MathExpression.PlayerXMillis(),
				desiredY: MathExpression.Min(MathExpression.PlayerYMillis(), 300 * 1000),
				movementSpeedInPixelsPerSecond: MathExpression.Constant(bulletSpeedInPixelsPerSecond),
				shouldSnapshot: true,
				guidGenerator: guidGenerator);

			long buffer = 100;

			BooleanExpression shouldDestroy = BooleanExpression.Or(
				BooleanExpression.GreaterThan(MathExpression.XMillis(), MathExpression.Constant((1000 + buffer) * 1000)),
				BooleanExpression.LessThan(MathExpression.XMillis(), MathExpression.Constant((0 - buffer) * 1000)),
				BooleanExpression.GreaterThan(MathExpression.YMillis(), MathExpression.Constant((700 + buffer) * 1000)),
				BooleanExpression.LessThan(MathExpression.YMillis(), MathExpression.Constant((0 - buffer) * 1000)));

			ObjectAction destroyAction = ObjectAction.Condition(
				condition: shouldDestroy,
				action: ObjectAction.Destroy());

			return ObjectAction.Union(moveAction, destroyAction);
		}

		private static ObjectAction GetShootBulletAction(
			Dictionary<string, DTDanmakuImage> spriteNameToImageDictionary,
			Dictionary<string, EnemyObjectTemplate> enemyObjectTemplates,
			GuidGenerator guidGenerator)
		{
			IMathExpression initialShootCooldownInMillis = MathExpression.RandomInteger(2750);
			IMathExpression shootCooldownInMillis = MathExpression.Add(1750, MathExpression.RandomInteger(1000));

			string cooldownVariableName = guidGenerator.NextGuid();
			string childObjectTemplateName = guidGenerator.NextGuid();
			string enemyBulletSpriteName = guidGenerator.NextGuid();

			IMathExpression facingAngleInMillidegrees = DTDanmakuMath.GetMovementDirectionInMillidegrees(
				currentX: MathExpression.XMillis(),
				currentY: MathExpression.YMillis(),
				desiredX: MathExpression.PlayerXMillis(),
				desiredY: MathExpression.Min(MathExpression.PlayerYMillis(), 300 * 1000));
			DTDanmakuMath.MathExpressionOffset deltaXAndY = DTDanmakuMath.GetOffset(
				millipixels: MathExpression.Constant(40000),
				movementDirectionInMillidegrees: facingAngleInMillidegrees);

			ObjectAction initialStartCooldownAction = ObjectAction.SetNumericVariable(cooldownVariableName, initialShootCooldownInMillis);
			ObjectAction startCooldownAction = ObjectAction.SetNumericVariable(cooldownVariableName, shootCooldownInMillis);
			ObjectAction decrementCooldownAction = ObjectAction.SetNumericVariable(cooldownVariableName, MathExpression.Subtract(MathExpression.Variable(cooldownVariableName), MathExpression.ElapsedMillisecondsPerIteration()));
			ObjectAction createBulletAction = ObjectAction.SpawnChild(
				childXMillis: MathExpression.Add(MathExpression.XMillis(), deltaXAndY.DeltaXInMillipixels),
				childYMillis: MathExpression.Add(MathExpression.YMillis(), deltaXAndY.DeltaYInMillipixels),
				childObjectTemplateName: childObjectTemplateName,
				childInitialNumericVariables: null,
				childInitialBooleanVariables: null);

			List<ObjectBox> collisionBoxes = new List<ObjectBox>();
			collisionBoxes.Add(new ObjectBox(lowerXMillis: -4000, upperXMillis: 4000, lowerYMillis: -4000, upperYMillis: 4000));

			enemyObjectTemplates.Add(childObjectTemplateName,
				EnemyObjectTemplate.EnemyBullet(
					action: ObjectAction.Union(GetBulletMovementAction(guidGenerator: guidGenerator), GetBulletAnimationAction(guidGenerator: guidGenerator)),
					initialMilliHP: null,
					damageBoxes: null,
					collisionBoxes: collisionBoxes,
					spriteName: enemyBulletSpriteName));

			spriteNameToImageDictionary.Add(enemyBulletSpriteName, DTDanmakuImage.EliteSniperEnemyBullet);

			var createBulletWhenCooldownFinishedAction = ObjectAction.Condition(
				condition: BooleanExpression.LessThanOrEqualTo(MathExpression.Variable(cooldownVariableName), MathExpression.Constant(0)),
				action: ObjectAction.Union(startCooldownAction, createBulletAction));

			return ObjectAction.ConditionalNextAction(
				currentAction: initialStartCooldownAction,
				condition: BooleanExpression.True(),
				nextAction: ObjectAction.Union(decrementCooldownAction, createBulletWhenCooldownFinishedAction));
		}

		public static ObjectAction SpawnEliteSniperEnemy(
			Dictionary<string, DTDanmakuImage> spriteNameToImageDictionary,
			Dictionary<string, EnemyObjectTemplate> enemyObjectTemplates,
			Dictionary<string, DTDanmakuSound> soundNameToSoundDictionary,
			GuidGenerator guidGenerator)
		{
			ObjectAction moveAction = GetMoveAction(
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
			spriteNameToImageDictionary.Add(spriteName, DTDanmakuImage.EliteSniperEnemyShip);

			List<ObjectBox> damageBoxes = new List<ObjectBox>();
			damageBoxes.Add(new ObjectBox(lowerXMillis: -46500, upperXMillis: 46500, lowerYMillis: -10000, upperYMillis: 30000));
			damageBoxes.Add(new ObjectBox(lowerXMillis: -31500, upperXMillis: 31500, lowerYMillis: -35000, upperYMillis: 30000));

			List<ObjectBox> collisionBoxes = new List<ObjectBox>();
			collisionBoxes.Add(new ObjectBox(lowerXMillis: -10000, upperXMillis: 10000, lowerYMillis: -25000, upperYMillis: 25000));
			collisionBoxes.Add(new ObjectBox(lowerXMillis: -25000, upperXMillis: 25000, lowerYMillis: -10000, upperYMillis: 10000));

			EnemyObjectTemplate enemyObjectTemplate = EnemyObjectTemplate.Enemy(
				action: ObjectAction.Union(moveAction, shootAction, destroyAction),
				initialMilliHP: MathExpression.Add(MathExpression.Constant(29000), MathExpression.RandomInteger(MathExpression.Constant(5000))),
				damageBoxes: damageBoxes,
				collisionBoxes: collisionBoxes,
				spriteName: spriteName);

			string templateName = guidGenerator.NextGuid();
			enemyObjectTemplates.Add(templateName, enemyObjectTemplate);

			return ObjectAction.SpawnChild(
				childXMillis: MathExpression.Constant(-100 * 1000),
				childYMillis: MathExpression.Constant(-100 * 1000),
				childObjectTemplateName: templateName,
				childInitialNumericVariables: null,
				childInitialBooleanVariables: null);
		}
	}
}
