
namespace DTDanmakuLib
{
	using System.Collections.Generic;

	public class BossEnemyGenerator
	{
		private static string CurrentPhaseVariableName = "current phase";
		
		private static ObjectAction GetMoveAction_Phase0()
		{
			ObjectAction teleportToInitialLocation = ObjectActionGenerator.DoOnce(
				ObjectAction.SetPosition(
					xMillis: MathExpression.Constant(500 * 1000),
					yMillis: MathExpression.Constant(850 * 1000)));

			ObjectAction moveAction = ObjectAction.Union(
				ObjectAction.StrafeMove(moveToXMillis: MathExpression.XMillis(), moveToYMillis: MathExpression.Constant(0)),
				ObjectAction.SetSpeed(MathExpression.Constant(20)));

			BooleanExpression shouldStopMoving = BooleanExpression.LessThanOrEqualTo(
				MathExpression.YMillis(),
				MathExpression.Constant(500 * 1000));

			ObjectAction stopMovingAction = ObjectAction.SetSpeed(MathExpression.Constant(0));
			ObjectAction setPhaseAction = ObjectAction.SetNumericVariable(CurrentPhaseVariableName, MathExpression.Constant(1));

			return ObjectAction.ConditionalNextAction(
				currentAction: ObjectAction.Union(teleportToInitialLocation, moveAction),
				condition: shouldStopMoving,
				nextAction: ObjectActionGenerator.DoOnce(ObjectAction.Union(stopMovingAction, setPhaseAction)));
		}
		
		private static ObjectAction Phase1ShootAction(
			Dictionary<string, DTDanmakuImage> spriteNameToImageDictionary,
			Dictionary<string, EnemyObjectTemplate> enemyObjectTemplates,
			GuidGenerator guidGenerator)
		{
			string singleBulletSpriteName = guidGenerator.NextGuid();
			spriteNameToImageDictionary.Add(singleBulletSpriteName, DTDanmakuImage.BossPhase1EnemyBullet);

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
				millipixels: MathExpression.Multiply(MathExpression.Variable("speed"), MathExpression.ElapsedMillisecondsPerIteration()),
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

			List<ObjectAction> spawnBulletActions = new List<ObjectAction>();

			string randomOffsetVariable = guidGenerator.NextGuid();

			for (int i = 0; i < 36; i++)
			{
				List<ObjectAction.InitialChildNumericVariableInfo> initialNumericVariables = new List<ObjectAction.InitialChildNumericVariableInfo>();
				initialNumericVariables.Add(
					new ObjectAction.InitialChildNumericVariableInfo(
						name: "direction",
						value: MathExpression.Add(MathExpression.Variable(randomOffsetVariable), MathExpression.Constant(i * 10 * 1000))));
				initialNumericVariables.Add(
					new ObjectAction.InitialChildNumericVariableInfo(
						name: "speed",
						value: MathExpression.Add(MathExpression.Constant(100), MathExpression.RandomInteger(100))));

				spawnBulletActions.Add(ObjectAction.SpawnChild(
					childXMillis: MathExpression.XMillis(),
					childYMillis: MathExpression.Add(MathExpression.YMillis(), MathExpression.Constant(-60000)),
					childObjectTemplateName: singleBulletTemplateName,
					childInitialNumericVariables: initialNumericVariables,
					childInitialBooleanVariables: null));
			}

			List<ObjectAction> actionList = new List<ObjectAction>();
			actionList.Add(ObjectAction.SetNumericVariable(randomOffsetVariable, MathExpression.RandomInteger(MathExpression.Constant(360 * 1000))));
			foreach (ObjectAction spawnBulletAction in spawnBulletActions)
				actionList.Add(spawnBulletAction);

			ObjectAction shootBulletAction = ObjectAction.Union(actionList);

			string shootCooldown = guidGenerator.NextGuid();

			ObjectAction initializeShootCooldown = ObjectAction.SetNumericVariable(
				variableName: shootCooldown,
				variableValue: MathExpression.Constant(2000));

			ObjectAction decrementShootCooldown = ObjectAction.SetNumericVariable(
				variableName: shootCooldown,
				variableValue: MathExpression.Subtract(MathExpression.Variable(shootCooldown), MathExpression.ElapsedMillisecondsPerIteration()));

			ObjectAction shootWhenCooldownIsZero = ObjectAction.Condition(
				condition: BooleanExpression.LessThanOrEqualTo(MathExpression.Variable(shootCooldown), MathExpression.Constant(0)),
				action: ObjectAction.Union(initializeShootCooldown, shootBulletAction));

			return ObjectAction.Union(
				ObjectActionGenerator.DoOnce(initializeShootCooldown),
				decrementShootCooldown,
				shootWhenCooldownIsZero);
		}
		
		private static ObjectAction Phase2ShootAction(
			Dictionary<string, DTDanmakuImage> spriteNameToImageDictionary,
			Dictionary<string, EnemyObjectTemplate> enemyObjectTemplates,
			GuidGenerator guidGenerator)
		{
			string singleBulletSpriteName = guidGenerator.NextGuid();
			spriteNameToImageDictionary.Add(singleBulletSpriteName, DTDanmakuImage.BossPhase2EnemyBullet);

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
				millipixels: MathExpression.Multiply(MathExpression.Variable("speed"), MathExpression.ElapsedMillisecondsPerIteration()),
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

			IMathExpression direction = DTDanmakuMath.GetMovementDirectionInMillidegrees(
				currentX: MathExpression.XMillis(),
				currentY: MathExpression.Add(MathExpression.YMillis(), MathExpression.Constant(-60000)),
				desiredX: MathExpression.PlayerXMillis(),
				desiredY: MathExpression.PlayerYMillis());

			direction = MathExpression.Add(
							direction,
							MathExpression.Add(
								MathExpression.Constant(-30 * 1000),
								MathExpression.RandomInteger(60 * 1000)));

			List <ObjectAction.InitialChildNumericVariableInfo> initialNumericVariables = new List<ObjectAction.InitialChildNumericVariableInfo>();
			initialNumericVariables.Add(
				new ObjectAction.InitialChildNumericVariableInfo(
					name: "direction",
					value: direction));
			initialNumericVariables.Add(
				new ObjectAction.InitialChildNumericVariableInfo(
					name: "speed",
					value: MathExpression.Add(MathExpression.Constant(300), MathExpression.RandomInteger(200))));

			ObjectAction spawnBullet = ObjectAction.SpawnChild(
				childXMillis: MathExpression.XMillis(),
				childYMillis: MathExpression.Add(MathExpression.YMillis(), MathExpression.Constant(-60000)),
				childObjectTemplateName: singleBulletTemplateName,
				childInitialNumericVariables: initialNumericVariables,
				childInitialBooleanVariables: null);
			
			string shootCooldown = guidGenerator.NextGuid();

			ObjectAction initializeShootCooldown = ObjectAction.SetNumericVariable(
				variableName: shootCooldown,
				variableValue: MathExpression.Constant(50));

			ObjectAction decrementShootCooldown = ObjectAction.SetNumericVariable(
				variableName: shootCooldown,
				variableValue: MathExpression.Subtract(MathExpression.Variable(shootCooldown), MathExpression.ElapsedMillisecondsPerIteration()));

			ObjectAction shootWhenCooldownIsZero = ObjectAction.Condition(
				condition: BooleanExpression.LessThanOrEqualTo(MathExpression.Variable(shootCooldown), MathExpression.Constant(0)),
				action: ObjectAction.Union(initializeShootCooldown, spawnBullet));

			return ObjectAction.Union(
				ObjectActionGenerator.DoOnce(ObjectAction.SetNumericVariable(
					variableName: shootCooldown,
					variableValue: MathExpression.Constant(1000))),
				decrementShootCooldown,
				shootWhenCooldownIsZero);
		}

		private static ObjectAction SpawnPhase3Bullet(
			IMathExpression bulletDirectionInMillidegrees,
			Dictionary<string, DTDanmakuImage> spriteNameToImageDictionary,
			Dictionary<string, EnemyObjectTemplate> enemyObjectTemplates,
			GuidGenerator guidGenerator)
		{
			string bulletSpriteName = guidGenerator.NextGuid();
			spriteNameToImageDictionary.Add(bulletSpriteName, DTDanmakuImage.BossPhase3EnemyBullet);

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
				childYMillis: MathExpression.Subtract(MathExpression.YMillis(), MathExpression.Constant(60000)),
				childObjectTemplateName: templateName,
				childInitialNumericVariables: null,
				childInitialBooleanVariables: null);
		}
		
		private static ObjectAction Phase3ShootAction(
			Dictionary<string, DTDanmakuImage> spriteNameToImageDictionary,
			Dictionary<string, EnemyObjectTemplate> enemyObjectTemplates,
			GuidGenerator guidGenerator)
		{
			string angleVariable = guidGenerator.NextGuid();
			string shootCooldownVariable = guidGenerator.NextGuid();

			ObjectAction initializeAngleVariable = ObjectActionGenerator.DoOnce(
				ObjectAction.SetNumericVariable(angleVariable, MathExpression.Constant(0)));

			ObjectAction updateAngleVariableAction = ObjectAction.Union(
				ObjectAction.SetNumericVariable(
					angleVariable,
					MathExpression.Add(MathExpression.Variable(angleVariable), MathExpression.Multiply(MathExpression.Constant(431), MathExpression.ElapsedMillisecondsPerIteration()))),
				ObjectAction.Condition(
					condition: BooleanExpression.GreaterThan(MathExpression.Variable(angleVariable), MathExpression.Constant(360 * 1000)),
					action: ObjectAction.SetNumericVariable(angleVariable, MathExpression.Subtract(MathExpression.Variable(angleVariable), MathExpression.Constant(360 * 1000)))),
				ObjectAction.Condition(
					condition: BooleanExpression.GreaterThan(MathExpression.Variable(angleVariable), MathExpression.Constant(360 * 1000)),
					action: ObjectAction.SetNumericVariable(angleVariable, MathExpression.Constant(0))));

			ObjectAction createBulletAction1 = SpawnPhase3Bullet(
					bulletDirectionInMillidegrees: MathExpression.Multiply(MathExpression.ParentVariable(angleVariable), MathExpression.Constant(1)),
					spriteNameToImageDictionary: spriteNameToImageDictionary,
					enemyObjectTemplates: enemyObjectTemplates,
					guidGenerator: guidGenerator);
			ObjectAction createBulletAction2 = SpawnPhase3Bullet(
					bulletDirectionInMillidegrees: MathExpression.Multiply(MathExpression.ParentVariable(angleVariable), MathExpression.Constant(-1)),
					spriteNameToImageDictionary: spriteNameToImageDictionary,
					enemyObjectTemplates: enemyObjectTemplates,
					guidGenerator: guidGenerator);

			IMathExpression shootCooldownInMillis = MathExpression.Constant(20);
			ObjectAction startCooldownAction = ObjectAction.SetNumericVariable(shootCooldownVariable, shootCooldownInMillis);
			ObjectAction decrementCooldownAction = ObjectAction.SetNumericVariable(shootCooldownVariable, MathExpression.Subtract(MathExpression.Variable(shootCooldownVariable), MathExpression.ElapsedMillisecondsPerIteration()));
			ObjectAction createBulletWhenCooldownFinishedAction = ObjectAction.Condition(
				condition: BooleanExpression.LessThanOrEqualTo(MathExpression.Variable(shootCooldownVariable), MathExpression.Constant(0)),
				action: ObjectAction.Union(startCooldownAction, createBulletAction1, createBulletAction2));

			return ObjectAction.Union(
				initializeAngleVariable,
				updateAngleVariableAction,
				ObjectActionGenerator.DoOnce(ObjectAction.SetNumericVariable(shootCooldownVariable, MathExpression.Constant(1000))),
				decrementCooldownAction,
				createBulletWhenCooldownFinishedAction);
		}

		private static ObjectAction GetMoveAction(
			GuidGenerator guidGenerator)
		{
			string movementCooldownVariable = guidGenerator.NextGuid();

			ObjectAction setMovementCooldownAction = ObjectAction.SetNumericVariable(
				movementCooldownVariable,
				MathExpression.Add(MathExpression.Constant(500), MathExpression.RandomInteger(5000)));

			ObjectAction decrementMovementCooldownAction = ObjectAction.SetNumericVariable(
				movementCooldownVariable,
				MathExpression.Subtract(MathExpression.Variable(movementCooldownVariable), MathExpression.ElapsedMillisecondsPerIteration()));

			BooleanExpression shouldChangeMovement = BooleanExpression.LessThanOrEqualTo(
				MathExpression.Variable(movementCooldownVariable),
				MathExpression.Constant(0));

			IMathExpression randomMilliAngle = MathExpression.RandomInteger(MathExpression.Constant(360 * 1000));

			string angleVariable = guidGenerator.NextGuid();
			ObjectAction writeNewAngleToVariable = ObjectAction.SetNumericVariable(
				angleVariable,
				randomMilliAngle);

			ObjectAction normalizeAngleVariable = ObjectAction.Union(
				ObjectAction.Condition(
					condition: BooleanExpression.LessThan(MathExpression.Variable(angleVariable), MathExpression.Constant(0)),
					action: ObjectAction.SetNumericVariable(angleVariable, MathExpression.Add(MathExpression.Variable(angleVariable), MathExpression.Constant(360 * 1000)))),
				ObjectAction.Condition(
					condition: BooleanExpression.GreaterThanOrEqualTo(MathExpression.Variable(angleVariable), MathExpression.Constant(360 * 1000)),
					action: ObjectAction.SetNumericVariable(angleVariable, MathExpression.Subtract(MathExpression.Variable(angleVariable), MathExpression.Constant(360 * 1000)))));
			
			BooleanExpression tooCloseToLeftEdge = BooleanExpression.LessThanOrEqualTo(
				MathExpression.XMillis(),
				MathExpression.Constant(200 * 1000));

			ObjectAction updateAngleVariableLeft = ObjectAction.Condition(
				condition: BooleanExpression.And(tooCloseToLeftEdge, BooleanExpression.And(
					BooleanExpression.GreaterThanOrEqualTo(MathExpression.Variable(angleVariable), MathExpression.Constant(180 * 1000)),
					BooleanExpression.LessThanOrEqualTo(MathExpression.Variable(angleVariable), MathExpression.Constant(360 * 1000)))),
				action: ObjectAction.SetNumericVariable(
					angleVariable,
					MathExpression.Multiply(MathExpression.Variable(angleVariable), MathExpression.Constant(-1))));

			BooleanExpression tooCloseToRightEdge = BooleanExpression.GreaterThanOrEqualTo(
				MathExpression.XMillis(),
				MathExpression.Constant(800 * 1000));

			ObjectAction updateAngleVariableRight = ObjectAction.Condition(
				condition: BooleanExpression.And(tooCloseToRightEdge, BooleanExpression.And(
					BooleanExpression.GreaterThanOrEqualTo(MathExpression.Variable(angleVariable), MathExpression.Constant(0 * 1000)),
					BooleanExpression.LessThanOrEqualTo(MathExpression.Variable(angleVariable), MathExpression.Constant(180 * 1000)))),
				action: ObjectAction.SetNumericVariable(
					angleVariable,
					MathExpression.Multiply(MathExpression.Variable(angleVariable), MathExpression.Constant(-1))));

			BooleanExpression tooCloseToTopEdge = BooleanExpression.GreaterThanOrEqualTo(
				MathExpression.YMillis(),
				MathExpression.Constant(600 * 1000));
			
			ObjectAction updateAngleVariableTop = ObjectAction.Condition(
				condition: BooleanExpression.And(tooCloseToTopEdge, BooleanExpression.Or(
					BooleanExpression.GreaterThanOrEqualTo(MathExpression.Variable(angleVariable), MathExpression.Constant(270 * 1000)),
					BooleanExpression.LessThanOrEqualTo(MathExpression.Variable(angleVariable), MathExpression.Constant(90 * 1000)))),
				action: ObjectAction.SetNumericVariable(
					angleVariable,
					MathExpression.Subtract(
						MathExpression.Multiply(MathExpression.Add(MathExpression.Variable(angleVariable), MathExpression.Constant(90 * 1000)), MathExpression.Constant(-1)),
						MathExpression.Constant(90 * 1000))));
			
			BooleanExpression tooCloseToBottomEdge = BooleanExpression.LessThanOrEqualTo(
				MathExpression.YMillis(),
				MathExpression.Constant(400 * 1000));
			
			ObjectAction updateAngleVariableBottom = ObjectAction.Condition(
				condition: BooleanExpression.And(tooCloseToBottomEdge, BooleanExpression.And(
					BooleanExpression.GreaterThanOrEqualTo(MathExpression.Variable(angleVariable), MathExpression.Constant(90 * 1000)),
					BooleanExpression.LessThanOrEqualTo(MathExpression.Variable(angleVariable), MathExpression.Constant(270 * 1000)))),
				action: ObjectAction.SetNumericVariable(
					angleVariable,
					MathExpression.Subtract(
						MathExpression.Multiply(MathExpression.Add(MathExpression.Variable(angleVariable), MathExpression.Constant(90 * 1000)), MathExpression.Constant(-1)),
						MathExpression.Constant(90 * 1000))));

			string angleSnapshotVariable = guidGenerator.NextGuid();

			ObjectAction initializeAngleSnapshotVariable = ObjectActionGenerator.DoOnce(
				ObjectAction.SetNumericVariable(angleSnapshotVariable, MathExpression.RandomInteger(360 * 1000)));

			DTDanmakuMath.MathExpressionOffset offset = DTDanmakuMath.GetOffset(
				millipixels: MathExpression.Multiply(MathExpression.Constant(80), MathExpression.ElapsedMillisecondsPerIteration()),
				movementDirectionInMillidegrees: MathExpression.Variable(angleSnapshotVariable));

			ObjectAction updatePositionAction = ObjectAction.SetPosition(
				xMillis: MathExpression.Add(MathExpression.XMillis(), offset.DeltaXInMillipixels),
				yMillis: MathExpression.Add(MathExpression.YMillis(), offset.DeltaYInMillipixels));

			ObjectAction updatePositionWhenCooldownIsZero = ObjectAction.Condition(
				condition: shouldChangeMovement,
				action: ObjectAction.Union(
					ObjectAction.SetNumericVariable(angleSnapshotVariable, MathExpression.Variable(angleVariable)),
					setMovementCooldownAction));

			ObjectAction immediateMoveWhenTooClose = ObjectAction.Condition(
				condition: BooleanExpression.Or(tooCloseToLeftEdge, tooCloseToRightEdge, tooCloseToTopEdge, tooCloseToBottomEdge),
				action: ObjectAction.SetNumericVariable(movementCooldownVariable, MathExpression.Constant(0)));

			List<ObjectAction> actionList = new List<ObjectAction>();

			actionList.Add(initializeAngleSnapshotVariable);
			actionList.Add(ObjectActionGenerator.DoOnce(setMovementCooldownAction));
			actionList.Add(decrementMovementCooldownAction);
			actionList.Add(writeNewAngleToVariable);
			actionList.Add(updateAngleVariableLeft);
			actionList.Add(normalizeAngleVariable);
			actionList.Add(updateAngleVariableRight);
			actionList.Add(normalizeAngleVariable);
			actionList.Add(updateAngleVariableTop);
			actionList.Add(normalizeAngleVariable);
			actionList.Add(normalizeAngleVariable);
			actionList.Add(updateAngleVariableBottom);
			actionList.Add(normalizeAngleVariable);
			actionList.Add(normalizeAngleVariable);
			actionList.Add(updatePositionWhenCooldownIsZero);
			actionList.Add(updatePositionAction);
			actionList.Add(immediateMoveWhenTooClose);

			return ObjectAction.Union(actionList);
		}
		
		private static ObjectAction GetPhase1Action(
			Dictionary<string, DTDanmakuImage> spriteNameToImageDictionary,
			Dictionary<string, EnemyObjectTemplate> enemyObjectTemplates,
			GuidGenerator guidGenerator)
		{
			string milliHpVariable = guidGenerator.NextGuid();
			string amountOfDamageTakenVariableName = guidGenerator.NextGuid();

			ObjectAction setInitialDamageTakenAction = ObjectActionGenerator.DoOnce(ObjectAction.SetNumericVariable(amountOfDamageTakenVariableName, MathExpression.Constant(0)));

			ObjectAction setInitialMilliHpVariable = ObjectActionGenerator.DoOnce(ObjectAction.SetNumericVariable(milliHpVariable, MathExpression.MilliHP()));

			ObjectAction setDamageTakenVariableAction = ObjectAction.SetNumericVariable(
				amountOfDamageTakenVariableName,
				MathExpression.Add(
					MathExpression.Variable(amountOfDamageTakenVariableName),
					MathExpression.Subtract(MathExpression.Variable(milliHpVariable), MathExpression.MilliHP())));

			ObjectAction setMilliHpVariable = ObjectAction.SetNumericVariable(milliHpVariable, MathExpression.MilliHP());

			long phase1InitialMilliHp = 200 * 1000;

			IMathExpression currentMilliHp = MathExpression.Max(
					MathExpression.Subtract(MathExpression.Constant(phase1InitialMilliHp), MathExpression.Variable(amountOfDamageTakenVariableName)),
					MathExpression.Constant(0));

			ObjectAction displayHpBarAction = ObjectAction.DisplayBossHealthBar(
				healthBarMeterNumber: MathExpression.Constant(3),
				healthBarMilliPercentage: MathExpression.Divide(MathExpression.Multiply(currentMilliHp, MathExpression.Constant(100 * 1000)), MathExpression.Constant(phase1InitialMilliHp)));
			
			ObjectAction goToPhase2Action = ObjectAction.Condition(
				condition: BooleanExpression.Equal(currentMilliHp, MathExpression.Constant(0)),
				action: ObjectActionGenerator.DoOnce(ObjectAction.SetNumericVariable(CurrentPhaseVariableName, MathExpression.Constant(2))));

			ObjectAction shootBulletAction = Phase1ShootAction(
				spriteNameToImageDictionary: spriteNameToImageDictionary,
				enemyObjectTemplates: enemyObjectTemplates,
				guidGenerator: guidGenerator);

			return ObjectAction.Union(
				setInitialDamageTakenAction,
				setInitialMilliHpVariable,
				setDamageTakenVariableAction,
				setMilliHpVariable,
				displayHpBarAction,
				GetMoveAction(guidGenerator: guidGenerator),
				goToPhase2Action,
				shootBulletAction);
		}
		
		private static ObjectAction GetPhase2Action(
			Dictionary<string, DTDanmakuImage> spriteNameToImageDictionary,
			Dictionary<string, EnemyObjectTemplate> enemyObjectTemplates,
			GuidGenerator guidGenerator)
		{
			string milliHpVariable = guidGenerator.NextGuid();
			string amountOfDamageTakenVariableName = guidGenerator.NextGuid();

			ObjectAction setInitialDamageTakenAction = ObjectActionGenerator.DoOnce(ObjectAction.SetNumericVariable(amountOfDamageTakenVariableName, MathExpression.Constant(0)));

			ObjectAction setInitialMilliHpVariable = ObjectActionGenerator.DoOnce(ObjectAction.SetNumericVariable(milliHpVariable, MathExpression.MilliHP()));

			ObjectAction setDamageTakenVariableAction = ObjectAction.SetNumericVariable(
				amountOfDamageTakenVariableName,
				MathExpression.Add(
					MathExpression.Variable(amountOfDamageTakenVariableName),
					MathExpression.Subtract(MathExpression.Variable(milliHpVariable), MathExpression.MilliHP())));

			ObjectAction setMilliHpVariable = ObjectAction.SetNumericVariable(milliHpVariable, MathExpression.MilliHP());
			
			long phase2InitialMilliHp = 200 * 1000;

			IMathExpression currentMilliHp = MathExpression.Max(
					MathExpression.Subtract(MathExpression.Constant(phase2InitialMilliHp), MathExpression.Variable(amountOfDamageTakenVariableName)),
					MathExpression.Constant(0));

			ObjectAction displayHpBarAction = ObjectAction.DisplayBossHealthBar(
				healthBarMeterNumber: MathExpression.Constant(2),
				healthBarMilliPercentage: MathExpression.Divide(MathExpression.Multiply(currentMilliHp, MathExpression.Constant(100 * 1000)), MathExpression.Constant(phase2InitialMilliHp)));

			ObjectAction goToPhase3Action = ObjectAction.Condition(
				condition: BooleanExpression.Equal(currentMilliHp, MathExpression.Constant(0)),
				action: ObjectActionGenerator.DoOnce(ObjectAction.SetNumericVariable(CurrentPhaseVariableName, MathExpression.Constant(3))));

			ObjectAction shootBulletAction = Phase2ShootAction(
				spriteNameToImageDictionary: spriteNameToImageDictionary,
				enemyObjectTemplates: enemyObjectTemplates,
				guidGenerator: guidGenerator);

			return ObjectAction.Union(
				setInitialDamageTakenAction,
				setInitialMilliHpVariable,
				setDamageTakenVariableAction,
				setMilliHpVariable,
				displayHpBarAction,
				GetMoveAction(guidGenerator: guidGenerator),
				goToPhase3Action,
				shootBulletAction);
		}
		
		private static ObjectAction GetPhase3Action(
			Dictionary<string, DTDanmakuImage> spriteNameToImageDictionary,
			Dictionary<string, EnemyObjectTemplate> enemyObjectTemplates,
			Dictionary<string, DTDanmakuSound> soundNameToSoundDictionary,
			GuidGenerator guidGenerator)
		{
			string milliHpVariable = guidGenerator.NextGuid();
			string amountOfDamageTakenVariableName = guidGenerator.NextGuid();

			ObjectAction setInitialDamageTakenAction = ObjectActionGenerator.DoOnce(ObjectAction.SetNumericVariable(amountOfDamageTakenVariableName, MathExpression.Constant(0)));

			ObjectAction setInitialMilliHpVariable = ObjectActionGenerator.DoOnce(ObjectAction.SetNumericVariable(milliHpVariable, MathExpression.MilliHP()));

			ObjectAction setDamageTakenVariableAction = ObjectAction.SetNumericVariable(
				amountOfDamageTakenVariableName,
				MathExpression.Add(
					MathExpression.Variable(amountOfDamageTakenVariableName),
					MathExpression.Subtract(MathExpression.Variable(milliHpVariable), MathExpression.MilliHP())));

			ObjectAction setMilliHpVariable = ObjectAction.SetNumericVariable(milliHpVariable, MathExpression.MilliHP());

			long phase3InitialMilliHp = 200 * 1000;

			IMathExpression currentMilliHp = MathExpression.Max(
					MathExpression.Subtract(MathExpression.Constant(phase3InitialMilliHp), MathExpression.Variable(amountOfDamageTakenVariableName)),
					MathExpression.Constant(0));

			ObjectAction displayHpBarAction = ObjectAction.DisplayBossHealthBar(
				healthBarMeterNumber: MathExpression.Constant(1),
				healthBarMilliPercentage: MathExpression.Divide(MathExpression.Multiply(currentMilliHp, MathExpression.Constant(100 * 1000)), MathExpression.Constant(phase3InitialMilliHp)));
			
			string soundEffectName = guidGenerator.NextGuid();
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
			
			ObjectAction destroyBoss = ObjectAction.Union(
					playEnemyDeathSoundEffectAction,
					generateDestructionAnimationResult.objectAction,
					ObjectAction.Destroy());

			ObjectAction destroyAndEndLevelAction = ObjectAction.Condition(
				condition: BooleanExpression.Equal(currentMilliHp, MathExpression.Constant(0)),
				action: ObjectAction.Union(
					ObjectAction.EndLevel(),
					destroyBoss));

			ObjectAction shootBulletAction = Phase3ShootAction(
				spriteNameToImageDictionary: spriteNameToImageDictionary,
				enemyObjectTemplates: enemyObjectTemplates,
				guidGenerator: guidGenerator);

			return ObjectAction.Union(
				setInitialDamageTakenAction,
				setInitialMilliHpVariable,
				setDamageTakenVariableAction,
				setMilliHpVariable,
				displayHpBarAction,
				GetMoveAction(guidGenerator: guidGenerator),
				destroyAndEndLevelAction,
				shootBulletAction);
		}
		
		private static ObjectAction GetBossAction(
			Dictionary<string, DTDanmakuImage> spriteNameToImageDictionary,
			Dictionary<string, EnemyObjectTemplate> enemyObjectTemplates,
			Dictionary<string, DTDanmakuSound> soundNameToSoundDictionary,
			GuidGenerator guidGenerator)
		{
			ObjectAction setPhaseVariableAction = ObjectActionGenerator.DoOnce(ObjectAction.SetNumericVariable(CurrentPhaseVariableName, MathExpression.Constant(0)));
			
			ObjectAction phase0Action = ObjectAction.Condition(
				condition: BooleanExpression.Equal(MathExpression.Variable(CurrentPhaseVariableName), MathExpression.Constant(0)),
				action: GetMoveAction_Phase0());

			ObjectAction phase1Action = ObjectAction.Condition(
				condition: BooleanExpression.Equal(MathExpression.Variable(CurrentPhaseVariableName), MathExpression.Constant(1)),
				action: GetPhase1Action(
					spriteNameToImageDictionary: spriteNameToImageDictionary,
					enemyObjectTemplates: enemyObjectTemplates,
					guidGenerator: guidGenerator));

			ObjectAction phase2Action = ObjectAction.Condition(
				condition: BooleanExpression.Equal(MathExpression.Variable(CurrentPhaseVariableName), MathExpression.Constant(2)),
				action: GetPhase2Action(
					spriteNameToImageDictionary: spriteNameToImageDictionary,
					enemyObjectTemplates: enemyObjectTemplates,
					guidGenerator: guidGenerator));

			ObjectAction phase3Action = ObjectAction.Condition(
				condition: BooleanExpression.Equal(MathExpression.Variable(CurrentPhaseVariableName), MathExpression.Constant(3)),
				action: GetPhase3Action(
					spriteNameToImageDictionary: spriteNameToImageDictionary,
					enemyObjectTemplates: enemyObjectTemplates,
					soundNameToSoundDictionary: soundNameToSoundDictionary,
					guidGenerator: guidGenerator));

			return ObjectAction.Union(
				setPhaseVariableAction,
				phase0Action,
				phase1Action,
				phase2Action,
				phase3Action);
		}
		
		public static ObjectAction SpawnBossEnemy(
			Dictionary<string, DTDanmakuImage> spriteNameToImageDictionary,
			Dictionary<string, EnemyObjectTemplate> enemyObjectTemplates,
			Dictionary<string, DTDanmakuSound> soundNameToSoundDictionary,
			GuidGenerator guidGenerator)
		{			
			string spriteName = guidGenerator.NextGuid();
			spriteNameToImageDictionary.Add(spriteName, DTDanmakuImage.Boss);

			List<ObjectBox> damageBoxes = new List<ObjectBox>();
			damageBoxes.Add(new ObjectBox(lowerXMillis: -96500, upperXMillis: 96500, lowerYMillis: -10000, upperYMillis: 30000));
			damageBoxes.Add(new ObjectBox(lowerXMillis: -71500, upperXMillis: 71500, lowerYMillis: -35000, upperYMillis: 30000));
			damageBoxes.Add(new ObjectBox(lowerXMillis: -50000, upperXMillis: 50000, lowerYMillis: -81000, upperYMillis: 48000));
			damageBoxes.Add(new ObjectBox(lowerXMillis: -35000, upperXMillis: 35000, lowerYMillis: 0, upperYMillis: 68000));

			List<ObjectBox> collisionBoxes = new List<ObjectBox>();
			collisionBoxes.Add(new ObjectBox(lowerXMillis: -20000, upperXMillis: 20000, lowerYMillis: -50000, upperYMillis: 50000));
			collisionBoxes.Add(new ObjectBox(lowerXMillis: -50000, upperXMillis: 50000, lowerYMillis: -20000, upperYMillis: 20000));

			EnemyObjectTemplate enemyObjectTemplate = EnemyObjectTemplate.Enemy(
				action: GetBossAction(
					spriteNameToImageDictionary: spriteNameToImageDictionary,
					enemyObjectTemplates: enemyObjectTemplates,
					soundNameToSoundDictionary: soundNameToSoundDictionary,
					guidGenerator: guidGenerator),
				initialMilliHP: MathExpression.Constant(50L * 1000L * 1000L * 1000L * 1000L),
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
