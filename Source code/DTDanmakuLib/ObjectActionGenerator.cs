
namespace DTDanmakuLib
{
	using System;
	using System.Collections.Generic;

	public class ObjectActionGenerator
	{
		public static ObjectAction Noop()
		{
			return ObjectAction.IncreaseSpeed(
				   speedInMillipixelsPerMillisecond: MathExpression.Constant(0));
		}

		public static ObjectAction DoOnce(ObjectAction action)
		{
			return ObjectAction.ConditionalNextAction(
				currentAction: action,
				condition: BooleanExpression.True(),
				nextAction: ObjectActionGenerator.Noop());
		}

		public static ObjectAction Delay(
			ObjectAction action,
			long milliseconds,
			GuidGenerator guidGenerator)
		{
			string guid = guidGenerator.NextGuid();
			return ObjectAction.ConditionalNextAction(
				currentAction: ObjectAction.SetNumericVariable(guid, MathExpression.Constant(0)),
				condition: BooleanExpression.True(),
				nextAction: ObjectAction.ConditionalNextAction(
					currentAction: ObjectAction.SetNumericVariable(guid, MathExpression.Add(MathExpression.ElapsedMillisecondsPerIteration(), MathExpression.Variable(guid))),
					condition: BooleanExpression.GreaterThanOrEqualTo(MathExpression.Variable(guid), MathExpression.Constant(milliseconds)),
					nextAction: action));
		}

		public static ObjectAction BulletStraightDownAction(
			long bulletSpeedInPixelsPerSecond)
		{
			var enemyBulletMovementAction = ObjectAction.SetPosition(
				xMillis: MathExpression.XMillis(),
				yMillis: MathExpression.Add(
					MathExpression.YMillis(),
					MathExpression.Multiply(
						MathExpression.Constant(-bulletSpeedInPixelsPerSecond),
						MathExpression.ElapsedMillisecondsPerIteration())));

			var enemyBulletDestroyAction = ObjectAction.Condition(
				condition: BooleanExpression.LessThanOrEqualTo(MathExpression.YMillis(), MathExpression.Constant(-700 * 1000)),
				action: ObjectAction.Destroy());

			return ObjectAction.Union(enemyBulletMovementAction, enemyBulletDestroyAction);
		}

		public static ObjectAction ShootBulletStraightDownAction(
			IMathExpression initialShootCooldownInMillis,
			IMathExpression shootCooldownInMillis,
			// Where the bullet should spawn (relative to the enemy shooting the bullet)
			long xOffset,
			long yOffset,
			long bulletSpeedInPixelsPerSecond,
			Dictionary<string, DTDanmakuImage> spriteNameToImageDictionary,
			Dictionary<string, EnemyObjectTemplate> enemyObjectTemplates,
			GuidGenerator guidGenerator)
		{
			string cooldownVariableName = guidGenerator.NextGuid();
			string childObjectTemplateName = guidGenerator.NextGuid();
			string enemyBulletSpriteName = guidGenerator.NextGuid();

			ObjectAction initialStartCooldownAction = ObjectAction.SetNumericVariable(cooldownVariableName, initialShootCooldownInMillis);
			ObjectAction startCooldownAction = ObjectAction.SetNumericVariable(cooldownVariableName, shootCooldownInMillis);
			ObjectAction decrementCooldownAction = ObjectAction.SetNumericVariable(cooldownVariableName, MathExpression.Subtract(MathExpression.Variable(cooldownVariableName), MathExpression.ElapsedMillisecondsPerIteration()));
			ObjectAction createBulletAction = ObjectAction.SpawnChild(
				childXMillis: MathExpression.Add(MathExpression.XMillis(), MathExpression.Constant(xOffset)),
				childYMillis: MathExpression.Add(MathExpression.YMillis(), MathExpression.Constant(yOffset)),
				childObjectTemplateName: childObjectTemplateName,
				childInitialNumericVariables: null,
				childInitialBooleanVariables: null);

			List<ObjectBox> collisionBoxes = new List<ObjectBox>();
			collisionBoxes.Add(new ObjectBox(lowerXMillis: -3000, upperXMillis: 3000, lowerYMillis: -23000, upperYMillis: 23000));

			enemyObjectTemplates.Add(childObjectTemplateName,
				EnemyObjectTemplate.EnemyBullet(
					action: BulletStraightDownAction(bulletSpeedInPixelsPerSecond: bulletSpeedInPixelsPerSecond),
					initialMilliHP: null,
					damageBoxes: null,
					collisionBoxes: collisionBoxes,
					spriteName: enemyBulletSpriteName));

			spriteNameToImageDictionary.Add(enemyBulletSpriteName, DTDanmakuImage.EnemyBullet);

			var createBulletWhenCooldownFinishedAction = ObjectAction.Condition(
				condition: BooleanExpression.LessThanOrEqualTo(MathExpression.Variable(cooldownVariableName), MathExpression.Constant(0)),
				action: ObjectAction.Union(startCooldownAction, createBulletAction));

			return ObjectAction.ConditionalNextAction(
				currentAction: initialStartCooldownAction,
				condition: BooleanExpression.True(),
				nextAction: ObjectAction.Union(decrementCooldownAction, createBulletWhenCooldownFinishedAction));
		}

		private static Tuple<ObjectAction, BooleanExpression> MoveToSpecifiedLocations_Helper(
			long startingXMillis,
			long startingYMillis,
			long endingXMillis,
			long endingYMillis,
			long speedInPixelsPerSecond,
			bool shouldStrafe,
			GuidGenerator guidGenerator)
		{
			string variableName = guidGenerator.NextGuid();
			string variableName2 = guidGenerator.NextGuid();

			long? direction = DTDanmakuMath.GetMovementDirectionInMillidegrees(
				currentX: startingXMillis,
				currentY: startingYMillis,
				desiredX: endingXMillis,
				desiredY: endingYMillis);

			if (direction == null)
				throw new Exception();

			DTDanmakuMath.Offset offset = DTDanmakuMath.GetOffset(
				speedInMillipixelsPerMillisecond: speedInPixelsPerSecond, // millipixels/millisecond is equivalent to pixels/second
				movementDirectionInMillidegrees: direction.Value,
				elapsedMillisecondsPerIteration: 1000); // Use the 1000 to help prevent rounding issues

			ObjectAction moveAction = ObjectAction.SetPosition(
				xMillis: MathExpression.Add(MathExpression.XMillis(), MathExpression.Divide(MathExpression.Multiply(MathExpression.Constant(offset.DeltaXInMillipixels), MathExpression.ElapsedMillisecondsPerIteration()), MathExpression.Constant(1000))),
				yMillis: MathExpression.Add(MathExpression.YMillis(), MathExpression.Divide(MathExpression.Multiply(MathExpression.Constant(offset.DeltaYInMillipixels), MathExpression.ElapsedMillisecondsPerIteration()), MathExpression.Constant(1000))));

			ObjectAction setDirectionAction = shouldStrafe
				? ObjectAction.SetFacingDirection(facingDirectionInMillidegrees: MathExpression.Constant(180 * 1000))
				: ObjectAction.SetFacingDirection(facingDirectionInMillidegrees: MathExpression.Constant(direction.Value));

			IMathExpression deltaX = MathExpression.AbsoluteValue(MathExpression.Subtract(MathExpression.XMillis(), MathExpression.Constant(endingXMillis)));
			IMathExpression deltaY = MathExpression.AbsoluteValue(MathExpression.Subtract(MathExpression.YMillis(), MathExpression.Constant(endingYMillis)));
			IMathExpression totalDelta = MathExpression.Add(deltaX, deltaY);
			ObjectAction setDeltaToVariable = ObjectAction.Union(
				ObjectAction.SetNumericVariable(variableName: variableName2, variableValue: MathExpression.Variable(variableName)),
				ObjectAction.SetNumericVariable(variableName: variableName, variableValue: totalDelta));
			BooleanExpression reachedDestination = BooleanExpression.And(
				BooleanExpression.LessThanOrEqualTo(
					MathExpression.Variable(variableName2),
					MathExpression.Variable(variableName)),
				BooleanExpression.And(
					BooleanExpression.GreaterThanOrEqualTo(MathExpression.Variable(variableName), MathExpression.Constant(0)),
					BooleanExpression.GreaterThanOrEqualTo(MathExpression.Variable(variableName2), MathExpression.Constant(0))
					));

			ObjectAction initializeVariables = ObjectAction.Union(
				ObjectAction.SetNumericVariable(variableName: variableName, variableValue: MathExpression.Constant(-1)),
				ObjectAction.SetNumericVariable(variableName: variableName2, variableValue: MathExpression.Constant(-1)));

			return new Tuple<ObjectAction, BooleanExpression>(
				ObjectAction.Union(
					moveAction,
					setDirectionAction,
					ObjectAction.ConditionalNextAction(
						currentAction: initializeVariables,
						condition: BooleanExpression.True(),
						nextAction: setDeltaToVariable)),
				reachedDestination);
		}

		public static ObjectAction MoveToSpecifiedLocations(
			// enemy will teleport to this initial location
			long initialXMillis,
			long initialYMillis,
			List<Tuple<long, long>> movementPath,
			long speedInPixelsPerSecond,
			bool shouldStrafe,
			bool shouldDestroyAtEndOfMovementPath,
			GuidGenerator guidGenerator)
		{
			ObjectAction currentAction = shouldDestroyAtEndOfMovementPath
				? ObjectAction.Destroy()
				: ObjectActionGenerator.Noop();

			for (int i = movementPath.Count - 1; i >= 0; i--)
			{
				long endingX = movementPath[i].Item1;
				long endingY = movementPath[i].Item2;
				long startingX = i == 0
					? initialXMillis
					: movementPath[i - 1].Item1;
				long startingY = i == 0
					? initialYMillis
					: movementPath[i - 1].Item2;

				Tuple<ObjectAction, BooleanExpression> helper = MoveToSpecifiedLocations_Helper(
					startingXMillis: startingX,
					startingYMillis: startingY,
					endingXMillis: endingX,
					endingYMillis: endingY,
					speedInPixelsPerSecond: speedInPixelsPerSecond,
					shouldStrafe: shouldStrafe,
					guidGenerator: guidGenerator);

				currentAction = ObjectAction.ConditionalNextAction(
				   currentAction: helper.Item1,
				   condition: helper.Item2,
				   nextAction: currentAction);
			}

			ObjectAction teleportToInitialLocation = ObjectAction.SetPosition(
				xMillis: MathExpression.Constant(initialXMillis),
				yMillis: MathExpression.Constant(initialYMillis));

			return ObjectAction.ConditionalNextAction(
				currentAction: teleportToInitialLocation,
				condition: BooleanExpression.True(),
				nextAction: currentAction);
		}

		public static ObjectAction DestroyWhenHpIsZeroAndMaybeDropPowerUp(
			long chanceToDropPowerUpInMilliPercent, // ranges from 0 (meaning 0%) to 100,000 (meaning 100%)
			Dictionary<string, DTDanmakuImage> spriteNameToImageDictionary,
			Dictionary<string, EnemyObjectTemplate> enemyObjectTemplates,
			Dictionary<string, DTDanmakuSound> soundNameToSoundDictionary,
			GuidGenerator guidGenerator)
		{
			string soundEffectName = guidGenerator.NextGuid();

			BooleanExpression isHpZero = BooleanExpression.LessThanOrEqualTo(
				MathExpression.MilliHP(),
				MathExpression.Constant(0));

			ObjectAction playEnemyDeathSoundEffectAction = ObjectAction.PlaySoundEffect(soundEffectName: soundEffectName);
			soundNameToSoundDictionary.Add(soundEffectName, DTDanmakuSound.EnemyDeath);

			DestructionAnimationGenerator.GenerateDestructionAnimationResult generateDestructionAnimationResult = DestructionAnimationGenerator.GenerateDestructionAnimation(
					orderedSprites: new List<DTDanmakuImage>
					{
						DTDanmakuImage.Explosion1,
						DTDanmakuImage.Explosion2,
						DTDanmakuImage.Explosion3,
						DTDanmakuImage.Explosion4,
						DTDanmakuImage.Explosion5,
						DTDanmakuImage.Explosion6,
						DTDanmakuImage.Explosion7,
						DTDanmakuImage.Explosion8,
						DTDanmakuImage.Explosion9
					},
					millisecondsPerSprite: 20,
					guidGenerator: guidGenerator);

			foreach (var entry in generateDestructionAnimationResult.spriteNameToImageDictionary)
				spriteNameToImageDictionary.Add(entry.Key, entry.Value);
			foreach (var entry in generateDestructionAnimationResult.enemyObjectTemplates)
				enemyObjectTemplates.Add(entry.Key, entry.Value);

			ObjectAction possiblySpawnPowerUpAction = ObjectAction.Condition(
				condition: BooleanExpression.LessThan(
					MathExpression.RandomInteger(MathExpression.Constant(100 * 1000)),
					MathExpression.Constant(chanceToDropPowerUpInMilliPercent)),
				action: ObjectAction.SpawnPowerUp());

			return ObjectAction.Condition(
				condition: isHpZero,
				action: ObjectAction.Union(
					playEnemyDeathSoundEffectAction,
					generateDestructionAnimationResult.objectAction,
					possiblySpawnPowerUpAction,
					ObjectAction.Destroy()));
		}

		public static ObjectAction SpawnEnemyThatMovesToSpecificLocation(
			long initialXMillis,
			long initialYMillis,
			List<Tuple<long, long>> movementPath,
			long movementSpeedInPixelsPerSecond,
			bool shouldStrafe,
			long bulletXOffset,
			long bulletYOffset,
			IMathExpression initialShootCooldownInMillis,
			IMathExpression shootCooldownInMillis,
			long bulletSpeedInPixelsPerSecond,
			IMathExpression initialMilliHP,
			long chanceToDropPowerUpInMilliPercent, // ranges from 0 (meaning 0%) to 100,000 (meaning 100%)
			List<ObjectBox> damageBoxes, // nullable
			List<ObjectBox> collisionBoxes, // nullable
			DTDanmakuImage sprite,
			Dictionary<string, DTDanmakuImage> spriteNameToImageDictionary,
			Dictionary<string, EnemyObjectTemplate> enemyObjectTemplates,
			Dictionary<string, DTDanmakuSound> soundNameToSoundDictionary,
			GuidGenerator guidGenerator)
		{
			ObjectAction moveAction = MoveToSpecifiedLocations(
				initialXMillis: initialXMillis,
				initialYMillis: initialYMillis,
				movementPath: movementPath,
				speedInPixelsPerSecond: movementSpeedInPixelsPerSecond,
				shouldStrafe: shouldStrafe,
				shouldDestroyAtEndOfMovementPath: true,
				guidGenerator: guidGenerator);

			ObjectAction shootAction = ShootBulletStraightDownAction(
				initialShootCooldownInMillis: initialShootCooldownInMillis,
				shootCooldownInMillis: shootCooldownInMillis,
				xOffset: bulletXOffset,
				yOffset: bulletYOffset,
				bulletSpeedInPixelsPerSecond: bulletSpeedInPixelsPerSecond,
				spriteNameToImageDictionary: spriteNameToImageDictionary,
				enemyObjectTemplates: enemyObjectTemplates,
				guidGenerator: guidGenerator);

			ObjectAction destroyAction = DestroyWhenHpIsZeroAndMaybeDropPowerUp(
				chanceToDropPowerUpInMilliPercent: chanceToDropPowerUpInMilliPercent,
				spriteNameToImageDictionary: spriteNameToImageDictionary,
				enemyObjectTemplates: enemyObjectTemplates,
				soundNameToSoundDictionary: soundNameToSoundDictionary,
				guidGenerator: guidGenerator);

			string spriteName = guidGenerator.NextGuid();
			spriteNameToImageDictionary.Add(spriteName, sprite);

			EnemyObjectTemplate enemyObjectTemplate = EnemyObjectTemplate.Enemy(
				action: ObjectAction.Union(moveAction, shootAction, destroyAction),
				initialMilliHP: initialMilliHP,
				damageBoxes: damageBoxes,
				collisionBoxes: collisionBoxes,
				spriteName: spriteName);

			string templateName = guidGenerator.NextGuid();
			enemyObjectTemplates.Add(templateName, enemyObjectTemplate);

			return ObjectAction.SpawnChild(
				childXMillis: MathExpression.Constant(initialXMillis),
				childYMillis: MathExpression.Constant(initialYMillis),
				childObjectTemplateName: templateName,
				childInitialNumericVariables: null,
				childInitialBooleanVariables: null);
		}

		/*
			If (currentX, currentY) == (desiredX, desiredY), then
			the resulting movement direction is arbitrary.
		*/
		public static ObjectAction MoveTowardsLocation(
			IMathExpression currentX,
			IMathExpression currentY,
			IMathExpression desiredX,
			IMathExpression desiredY,
			/*
				Note that movement speed is not affected by shouldSnapshot
			*/
			IMathExpression movementSpeedInPixelsPerSecond,
			/*
				When true, will decide movement direction on the first frame, and keep that direction.
				When false, will continuously move towards (desiredX, desiredY)
			*/
			bool shouldSnapshot,
			GuidGenerator guidGenerator)
		{
			string deltaXVariable = guidGenerator.NextGuid();
			string deltaYVariable = guidGenerator.NextGuid();

			ObjectAction setDeltaX = ObjectAction.SetNumericVariable(deltaXVariable, MathExpression.Subtract(desiredX, currentX));
			ObjectAction setDeltaY = ObjectAction.SetNumericVariable(deltaYVariable, MathExpression.Subtract(desiredY, currentY));

			BooleanExpression areDeltasBothZero = BooleanExpression.And(
				BooleanExpression.Equal(MathExpression.Variable(deltaXVariable), MathExpression.Constant(0)),
				BooleanExpression.Equal(MathExpression.Variable(deltaYVariable), MathExpression.Constant(0)));

			string angleInMillidegreesVariable = guidGenerator.NextGuid();

			ObjectAction setAngle = ObjectAction.Union(
				shouldSnapshot ? ObjectActionGenerator.DoOnce(setDeltaX) : setDeltaX,
				shouldSnapshot ? ObjectActionGenerator.DoOnce(setDeltaY) : setDeltaY,
				ObjectAction.Condition(
					condition: areDeltasBothZero,
					action: ObjectAction.SetNumericVariable(angleInMillidegreesVariable, MathExpression.Constant(0))),
				ObjectAction.Condition(
					condition: BooleanExpression.Not(areDeltasBothZero),
					action: ObjectAction.SetNumericVariable(angleInMillidegreesVariable, MathExpression.ArcTangentScaled(MathExpression.Variable(deltaXVariable), MathExpression.Variable(deltaYVariable), false))),
				ObjectAction.SetNumericVariable(angleInMillidegreesVariable, MathExpression.Multiply(MathExpression.Variable(angleInMillidegreesVariable), MathExpression.Constant(-1))),
				ObjectAction.SetNumericVariable(angleInMillidegreesVariable, MathExpression.Add(MathExpression.Variable(angleInMillidegreesVariable), 90 * 1000)));

			IMathExpression xMillis =
				MathExpression.Divide(
					MathExpression.Multiply(
						movementSpeedInPixelsPerSecond,
						MathExpression.ElapsedMillisecondsPerIteration(),
						MathExpression.SineScaled(MathExpression.Variable(angleInMillidegreesVariable))),
					MathExpression.Constant(1000));
			IMathExpression yMillis =
				MathExpression.Divide(
					MathExpression.Multiply(
						movementSpeedInPixelsPerSecond,
						MathExpression.ElapsedMillisecondsPerIteration(),
						MathExpression.CosineScaled(MathExpression.Variable(angleInMillidegreesVariable))),
					MathExpression.Constant(1000));

			string xMillisVariable = guidGenerator.NextGuid();
			string yMillisVariable = guidGenerator.NextGuid();
			ObjectAction setXMillisVariable = shouldSnapshot
				? ObjectActionGenerator.DoOnce(ObjectAction.SetNumericVariable(xMillisVariable, xMillis))
				: ObjectAction.SetNumericVariable(xMillisVariable, xMillis);
			ObjectAction setYMillisVariable = shouldSnapshot
				? ObjectActionGenerator.DoOnce(ObjectAction.SetNumericVariable(yMillisVariable, yMillis))
				: ObjectAction.SetNumericVariable(yMillisVariable, yMillis);

			return ObjectAction.Union(
				setAngle,
				setXMillisVariable,
				setYMillisVariable,
				ObjectAction.SetPosition(
					xMillis: MathExpression.Add(MathExpression.XMillis(), MathExpression.Variable(xMillisVariable)),
					yMillis: MathExpression.Add(MathExpression.YMillis(), MathExpression.Variable(yMillisVariable))));
		}
	}
}
