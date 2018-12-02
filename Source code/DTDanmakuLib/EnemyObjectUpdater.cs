
namespace DTDanmakuLib
{
	using DTLib;
	using System;
	using System.Collections.Generic;

	public class EnemyObjectUpdater
	{
		public class UpdateResult
		{
			public List<EnemyObject> NewEnemyObjects;
			public List<Tuple<long, long>> NewPowerUps;
			public bool ShouldEndLevel;
			public List<string> NewSoundEffectsToPlay;
			public long? BossHealthMeterNumber;
			public long? BossHealthMeterMilliPercentage;
		}

		private class EnemyObjectWrapper
		{
			public EnemyObjectWrapper(EnemyObject enemyObject, bool hasHandledAction)
			{
				this.EnemyObject = enemyObject;
				this.HasHandledAction = hasHandledAction;
			}

			public EnemyObject EnemyObject { get; set; }
			public bool HasHandledAction { get; set; }
		}

		public static UpdateResult Update(
				List<EnemyObject> currentEnemyObjects,
				long playerXMillis,
				long playerYMillis,
				long elapsedMillisecondsPerIteration,
				bool isPlayerDestroyed,
				Dictionary<string, EnemyObjectTemplate> enemyObjectTemplates,
				IDTDeterministicRandom rng)
        {
			List<EnemyObjectWrapper> wrapperList = new List<EnemyObjectWrapper>();
			foreach (EnemyObject enemyObj in currentEnemyObjects)
				wrapperList.Add(new EnemyObjectWrapper(enemyObj, hasHandledAction: false));

			var newPowerUps = new List<Tuple<long, long>>();
			var newSoundEffectsToPlay = new List<string>();

			long? bossHealthMeterNumber = null;
			long? bossHealthMeterMilliPercentage = null;

			bool shouldEndLevel = false;

			while (true)
			{
				bool hasHandledAllActions = true;
				foreach (EnemyObjectWrapper x in wrapperList)
				{
					if (!x.HasHandledAction)
					{
						hasHandledAllActions = false;
						break;
					}
				}

				if (hasHandledAllActions)
					break;

				var newEnemyObjects = new List<EnemyObject>();

				foreach (EnemyObjectWrapper enemyObjectWrapper in wrapperList)
				{
					if (enemyObjectWrapper.HasHandledAction)
						continue;

					enemyObjectWrapper.HasHandledAction = true;

					EnemyObject enemyObject = enemyObjectWrapper.EnemyObject;

					if (enemyObject.IsDestroyed)
						continue;

					ResultOfAction actionResult = HandleAction(
						action: enemyObject.Action,
						obj: enemyObject,
						playerXMillis: playerXMillis,
						playerYMillis: playerYMillis,
						elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
						isPlayerDestroyed: isPlayerDestroyed,
						enemyObjectTemplates: enemyObjectTemplates,
						rng: rng);

					enemyObject.Action = actionResult.NewObjectAction;

					if (actionResult.ShouldEndLevel)
						shouldEndLevel = true;

					foreach (EnemyObject newEnemy in actionResult.NewEnemyObjects)
						newEnemyObjects.Add(newEnemy);

					foreach (var newPowerUp in actionResult.NewPowerUps)
						newPowerUps.Add(newPowerUp);

					foreach (var newSoundEffect in actionResult.NewSoundEffectsToPlay)
						newSoundEffectsToPlay.Add(newSoundEffect);

					if (actionResult.BossHealthMeterNumber != null)
						bossHealthMeterNumber = actionResult.BossHealthMeterNumber;

					if (actionResult.BossHealthMeterMilliPercentage != null)
						bossHealthMeterMilliPercentage = actionResult.BossHealthMeterMilliPercentage;

					if (!enemyObject.IsDestroyed)
					{
						var offset = DTDanmakuMath.GetOffset(
							speedInMillipixelsPerMillisecond: enemyObject.SpeedInMillipixelsPerMillisecond,
							movementDirectionInMillidegrees: enemyObject.MovementDirectionInMillidegrees,
							elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration);

						enemyObject.XMillis += offset.DeltaXInMillipixels;
						enemyObject.YMillis += offset.DeltaYInMillipixels;
					}
				}

				foreach (EnemyObject newEnemyObject in newEnemyObjects)
					wrapperList.Add(new EnemyObjectWrapper(newEnemyObject, hasHandledAction: false));

			}

			var finalEnemyList = new List<EnemyObject>();

			foreach (EnemyObjectWrapper enemyObjectWrapper in wrapperList)
			{
				if (!enemyObjectWrapper.EnemyObject.IsDestroyed)
					finalEnemyList.Add(enemyObjectWrapper.EnemyObject);
			}

			return new UpdateResult
			{
				NewEnemyObjects = finalEnemyList,
				NewPowerUps = newPowerUps,
				ShouldEndLevel = shouldEndLevel,
				NewSoundEffectsToPlay = newSoundEffectsToPlay,
				BossHealthMeterNumber = bossHealthMeterNumber,
				BossHealthMeterMilliPercentage = bossHealthMeterMilliPercentage
			};
        }

		private class ResultOfAction
		{
			public ResultOfAction(
				ObjectAction newObjectAction,
				bool shouldEndLevel,
				List<EnemyObject> newEnemyObjects,
				List<Tuple<long, long>> newPowerUps,
				List<string> newSoundEffectsToPlay,
				long? bossHealthMeterNumber,
				long? bossHealthMeterMilliPercentage)
			{
				if (newObjectAction == null || newEnemyObjects == null || newPowerUps == null || newSoundEffectsToPlay == null)
					throw new Exception();

				this.NewObjectAction = newObjectAction;
				this.ShouldEndLevel = shouldEndLevel;
				this.NewEnemyObjects = newEnemyObjects;
				this.NewPowerUps = newPowerUps;
				this.NewSoundEffectsToPlay = newSoundEffectsToPlay;
				this.BossHealthMeterNumber = bossHealthMeterNumber;
				this.BossHealthMeterMilliPercentage = bossHealthMeterMilliPercentage;
			}

			public ObjectAction NewObjectAction;
			public bool ShouldEndLevel;
			public List<EnemyObject> NewEnemyObjects;
			public List<Tuple<long, long>> NewPowerUps;
			public List<string> NewSoundEffectsToPlay;
			public long? BossHealthMeterNumber;
			public long? BossHealthMeterMilliPercentage;
		}

		private static ResultOfAction HandleAction(
			ObjectAction action,
			EnemyObject obj,
			long playerXMillis,
			long playerYMillis,
			long elapsedMillisecondsPerIteration,
			bool isPlayerDestroyed,
			Dictionary<string, EnemyObjectTemplate> enemyObjectTemplates,
			IDTDeterministicRandom rng)
		{			
			bool? isParentDestroyed;
			if (obj.ParentObject == null)
				isParentDestroyed = null;
			else
				isParentDestroyed = obj.ParentObject.IsDestroyed;

			switch (action.ObjectActionType)
			{
				case ObjectAction.Type.Move:
				case ObjectAction.Type.StrafeMove:
					long desiredX = action.MoveToXMillis.Evaluate(
						obj.GetEnemyObjectExpressionInfo(),
						playerXMillis: playerXMillis,
						playerYMillis: playerYMillis,
						elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
						rng: rng);
					long desiredY = action.MoveToYMillis.Evaluate(
						obj.GetEnemyObjectExpressionInfo(),
						playerXMillis: playerXMillis,
						playerYMillis: playerYMillis,
						elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
						rng: rng);
					long? directionInMillidegrees = DTDanmakuMath.GetMovementDirectionInMillidegrees(currentX: obj.XMillis, currentY: obj.YMillis, desiredX: desiredX, desiredY: desiredY);

					if (directionInMillidegrees != null)
					{
						obj.MovementDirectionInMillidegrees = directionInMillidegrees.Value;
						if (action.ObjectActionType == ObjectAction.Type.Move)
							obj.FacingDirectionInMillidegrees = directionInMillidegrees.Value;
						else if (action.ObjectActionType == ObjectAction.Type.StrafeMove)
							obj.FacingDirectionInMillidegrees = 180L * 1000L;
						else
							throw new Exception();
					}

					return new ResultOfAction(
						newObjectAction: action,
						shouldEndLevel: false,
						newEnemyObjects: new List<EnemyObject>(),
						newPowerUps: new List<Tuple<long, long>>(),
						newSoundEffectsToPlay: new List<string>(),
						bossHealthMeterNumber: null,
						bossHealthMeterMilliPercentage: null);
				case ObjectAction.Type.SetSpeed:
				case ObjectAction.Type.IncreaseSpeed:
				case ObjectAction.Type.DecreaseSpeed:
					long speed = action.SpeedInMillipixelsPerMillisecond.Evaluate(
						obj.GetEnemyObjectExpressionInfo(),
						playerXMillis: playerXMillis,
						playerYMillis: playerYMillis,
						elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
						rng: rng);

					if (action.ObjectActionType == ObjectAction.Type.SetSpeed)
						obj.SpeedInMillipixelsPerMillisecond = speed;
					else if (action.ObjectActionType == ObjectAction.Type.IncreaseSpeed)
						obj.SpeedInMillipixelsPerMillisecond += speed;
					else if (action.ObjectActionType == ObjectAction.Type.DecreaseSpeed)
						obj.SpeedInMillipixelsPerMillisecond -= speed;
					else
						throw new Exception();

					if (obj.SpeedInMillipixelsPerMillisecond < 0)
						obj.SpeedInMillipixelsPerMillisecond = 0;

					return new ResultOfAction(
						newObjectAction: action,
						shouldEndLevel: false,
						newEnemyObjects: new List<EnemyObject>(),
						newPowerUps: new List<Tuple<long, long>>(),
						newSoundEffectsToPlay: new List<string>(),
						bossHealthMeterNumber: null,
						bossHealthMeterMilliPercentage: null);

				case ObjectAction.Type.SetPosition:
					long newXMillisPosition = action.SetXMillisPosition.Evaluate(
						obj.GetEnemyObjectExpressionInfo(),
						playerXMillis: playerXMillis,
						playerYMillis: playerYMillis,
						elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
						rng: rng);
					long newYMillisPosition = action.SetYMillisPosition.Evaluate(
						obj.GetEnemyObjectExpressionInfo(),
						playerXMillis: playerXMillis,
						playerYMillis: playerYMillis,
						elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
						rng: rng);
					obj.XMillis = newXMillisPosition;
					obj.YMillis = newYMillisPosition;

					return new ResultOfAction(
						newObjectAction: action,
						shouldEndLevel: false,
						newEnemyObjects: new List<EnemyObject>(),
						newPowerUps: new List<Tuple<long, long>>(),
						newSoundEffectsToPlay: new List<string>(),
						bossHealthMeterNumber: null,
						bossHealthMeterMilliPercentage: null);

				case ObjectAction.Type.SetFacingDirection:

					long newFacingDirection = action.SetFacingDirectionInMillidegrees.Evaluate(
						obj.GetEnemyObjectExpressionInfo(),
						playerXMillis: playerXMillis,
						playerYMillis: playerYMillis,
						elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
						rng: rng);

					obj.FacingDirectionInMillidegrees = newFacingDirection;

					return new ResultOfAction(
						newObjectAction: action,
						shouldEndLevel: false,
						newEnemyObjects: new List<EnemyObject>(),
						newPowerUps: new List<Tuple<long, long>>(),
						newSoundEffectsToPlay: new List<string>(),
						bossHealthMeterNumber: null,
						bossHealthMeterMilliPercentage: null);

				case ObjectAction.Type.Destroy:
					obj.IsDestroyed = true;

					return new ResultOfAction(
						newObjectAction: action,
						shouldEndLevel: false,
						newEnemyObjects: new List<EnemyObject>(),
						newPowerUps: new List<Tuple<long, long>>(),
						newSoundEffectsToPlay: new List<string>(),
						bossHealthMeterNumber: null,
						bossHealthMeterMilliPercentage: null);

				case ObjectAction.Type.DestroyParent:
					if (obj.ParentObject == null)
						throw new Exception();

					obj.ParentObject.IsDestroyed = true;

					return new ResultOfAction(
						newObjectAction: action,
						shouldEndLevel: false,
						newEnemyObjects: new List<EnemyObject>(),
						newPowerUps: new List<Tuple<long, long>>(),
						newSoundEffectsToPlay: new List<string>(),
						bossHealthMeterNumber: null,
						bossHealthMeterMilliPercentage: null);

				case ObjectAction.Type.SpawnChild:
					long childXMillis = action.SpawnChildXMillis.Evaluate(
						obj.GetEnemyObjectExpressionInfo(),
						playerXMillis: playerXMillis,
						playerYMillis: playerYMillis,
						elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
						rng: rng);
					long childYMillis = action.SpawnChildYMillis.Evaluate(
						obj.GetEnemyObjectExpressionInfo(),
						playerXMillis: playerXMillis,
						playerYMillis: playerYMillis,
						elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
						rng: rng);

					EnemyObjectTemplate childObjectTemplate = enemyObjectTemplates[action.SpawnChildObjectTemplateName];

					var childObjectNumericVariables = new Dictionary<string, IMathExpression>();
					var childObjectBooleanVariables = new Dictionary<string, BooleanExpression>();

					if (action.SpawnChildInitialChildNumericVariables != null)
					{
						for (int i = 0; i < action.SpawnChildInitialChildNumericVariables.Count; i++)
							childObjectNumericVariables.Add(action.SpawnChildInitialChildNumericVariables[i].Name, action.SpawnChildInitialChildNumericVariables[i].Value);
					}
					if (action.SpawnChildInitialChildBooleanVariables != null)
					{
						for (int i = 0; i < action.SpawnChildInitialChildBooleanVariables.Count; i++)
							childObjectBooleanVariables.Add(action.SpawnChildInitialChildBooleanVariables[i].Name, action.SpawnChildInitialChildBooleanVariables[i].Value);
					}

					var newEnemyObjects = new List<EnemyObject>();

					newEnemyObjects.Add(new EnemyObject(
						template: childObjectTemplate,
						initialXMillis: childXMillis,
						initialYMillis: childYMillis,
						playerXMillis: playerXMillis,
						playerYMillis: playerYMillis,
						elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
						isPlayerDestroyed: isPlayerDestroyed,
						parent: obj,
						initialNumericVariables: childObjectNumericVariables,
						initialBooleanVariables: childObjectBooleanVariables,
						rng: rng));

					return new ResultOfAction(
						newObjectAction: action,
						shouldEndLevel: false,
						newEnemyObjects: newEnemyObjects,
						newPowerUps: new List<Tuple<long, long>>(),
						newSoundEffectsToPlay: new List<string>(),
						bossHealthMeterNumber: null,
						bossHealthMeterMilliPercentage: null);

				case ObjectAction.Type.SpawnPowerUp:
					var powerUpList = new List<Tuple<long, long>>();
					powerUpList.Add(new Tuple<long, long>(obj.XMillis, obj.YMillis));

					return new ResultOfAction(
						newObjectAction: action,
						shouldEndLevel: false,
						newEnemyObjects: new List<EnemyObject>(),
						newPowerUps: powerUpList,
						newSoundEffectsToPlay: new List<string>(),
						bossHealthMeterNumber: null,
						bossHealthMeterMilliPercentage: null);

				case ObjectAction.Type.SetNumericVariable:
					
					obj.NumericVariables[action.SetVariableName] = action.SetNumericVariableValue.Evaluate(
						obj.GetEnemyObjectExpressionInfo(),
						playerXMillis: playerXMillis,
						playerYMillis: playerYMillis,
						elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
						rng: rng);
					
					return new ResultOfAction(
						newObjectAction: action,
						shouldEndLevel: false,
						newEnemyObjects: new List<EnemyObject>(),
						newPowerUps: new List<Tuple<long, long>>(),
						newSoundEffectsToPlay: new List<string>(),
						bossHealthMeterNumber: null,
						bossHealthMeterMilliPercentage: null);

				case ObjectAction.Type.SetBooleanVariable:
					obj.BooleanVariables[action.SetVariableName] = action.SetBooleanVariableValue.Evaluate(
						obj.GetEnemyObjectExpressionInfo(),
						isParentDestroyed: isParentDestroyed,
						isPlayerDestroyed: isPlayerDestroyed,
						playerXMillis: playerXMillis,
						playerYMillis: playerYMillis,
						elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
						rng: rng);

					return new ResultOfAction(
						newObjectAction: action,
						shouldEndLevel: false,
						newEnemyObjects: new List<EnemyObject>(),
						newPowerUps: new List<Tuple<long, long>>(),
						newSoundEffectsToPlay: new List<string>(),
						bossHealthMeterNumber: null,
						bossHealthMeterMilliPercentage: null);

				case ObjectAction.Type.SetParentNumericVariable:

					if (obj.ParentObject == null)
						throw new Exception();

					obj.ParentObject.NumericVariables[action.SetVariableName] = action.SetNumericVariableValue.Evaluate(
						obj.GetEnemyObjectExpressionInfo(),
						playerXMillis: playerXMillis,
						playerYMillis: playerYMillis,
						elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
						rng: rng);

					return new ResultOfAction(
						newObjectAction: action,
						shouldEndLevel: false,
						newEnemyObjects: new List<EnemyObject>(),
						newPowerUps: new List<Tuple<long, long>>(),
						newSoundEffectsToPlay: new List<string>(),
						bossHealthMeterNumber: null,
						bossHealthMeterMilliPercentage: null);

				case ObjectAction.Type.SetParentBooleanVariable:
				{
					if (obj.ParentObject == null)
						throw new Exception();

					obj.ParentObject.BooleanVariables[action.SetVariableName] = action.SetBooleanVariableValue.Evaluate(
						obj.GetEnemyObjectExpressionInfo(),
						isParentDestroyed: obj.ParentObject.IsDestroyed,
						isPlayerDestroyed: isPlayerDestroyed,
						playerXMillis: playerXMillis,
						playerYMillis: playerYMillis,
						elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
						rng: rng);

					return new ResultOfAction(
						newObjectAction: action,
						shouldEndLevel: false,
						newEnemyObjects: new List<EnemyObject>(),
						newPowerUps: new List<Tuple<long, long>>(),
						newSoundEffectsToPlay: new List<string>(),
						bossHealthMeterNumber: null,
						bossHealthMeterMilliPercentage: null);
				}
				case ObjectAction.Type.EndLevel:

					return new ResultOfAction(
						newObjectAction: action,
						shouldEndLevel: true,
						newEnemyObjects: new List<EnemyObject>(),
						newPowerUps: new List<Tuple<long, long>>(),
						newSoundEffectsToPlay: new List<string>(),
						bossHealthMeterNumber: null,
						bossHealthMeterMilliPercentage: null);

				case ObjectAction.Type.PlaySoundEffect:
				{
					var newSoundEffectsToPlay = new List<string>();
					newSoundEffectsToPlay.Add(action.SoundEffectName);

					return new ResultOfAction(
						newObjectAction: action,
						shouldEndLevel: false,
						newEnemyObjects: new List<EnemyObject>(),
						newPowerUps: new List<Tuple<long, long>>(),
						newSoundEffectsToPlay: newSoundEffectsToPlay,
						bossHealthMeterNumber: null,
						bossHealthMeterMilliPercentage: null);
				}
				case ObjectAction.Type.DisplayBossHealthBar:
				{
					long bossHealthMeterNumber = action.BossHealthBarMeterNumber.Evaluate(
						obj.GetEnemyObjectExpressionInfo(),
						playerXMillis: playerXMillis,
						playerYMillis: playerYMillis,
						elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
						rng: rng);
						
					long bossHealthMeterMilliPercentage = action.BossHealthBarMilliPercentage.Evaluate(
						obj.GetEnemyObjectExpressionInfo(),
						playerXMillis: playerXMillis,
						playerYMillis: playerYMillis,
						elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
						rng: rng);

					return new ResultOfAction(
						newObjectAction: action,
						shouldEndLevel: false,
						newEnemyObjects: new List<EnemyObject>(),
						newPowerUps: new List<Tuple<long, long>>(),
						newSoundEffectsToPlay: new List<string>(),
						bossHealthMeterNumber: bossHealthMeterNumber,
						bossHealthMeterMilliPercentage: bossHealthMeterMilliPercentage);
				}
				case ObjectAction.Type.SetSpriteName:

					obj.SpriteName = action.SpriteName;

					return new ResultOfAction(
						newObjectAction: action,
						shouldEndLevel: false,
						newEnemyObjects: new List<EnemyObject>(),
						newPowerUps: new List<Tuple<long, long>>(),
						newSoundEffectsToPlay: new List<string>(),
						bossHealthMeterNumber: null,
						bossHealthMeterMilliPercentage: null);

				case ObjectAction.Type.Conditional:
					bool shouldExecute = action.Conditional.Evaluate(
						obj.GetEnemyObjectExpressionInfo(),
						isParentDestroyed: isParentDestroyed,
						isPlayerDestroyed: isPlayerDestroyed,
						playerXMillis: playerXMillis,
						playerYMillis: playerYMillis,
						elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
						rng: rng);

					if (shouldExecute)
					{
						var a = HandleAction(
							action: action.ConditionalAction,
							obj: obj,
							playerXMillis: playerXMillis,
							playerYMillis: playerYMillis,
							elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
							isPlayerDestroyed: isPlayerDestroyed,
							enemyObjectTemplates: enemyObjectTemplates,
							rng: rng);

						return new ResultOfAction(
							newObjectAction: ObjectAction.Condition(action.Conditional, a.NewObjectAction),
							shouldEndLevel: a.ShouldEndLevel,
							newEnemyObjects: a.NewEnemyObjects,
							newPowerUps: a.NewPowerUps,
							newSoundEffectsToPlay: a.NewSoundEffectsToPlay,
							bossHealthMeterNumber: a.BossHealthMeterNumber,
							bossHealthMeterMilliPercentage: a.BossHealthMeterMilliPercentage);
					}
					else
					{
						return new ResultOfAction(
							newObjectAction: action,
							shouldEndLevel: false,
							newEnemyObjects: new List<EnemyObject>(),
							newPowerUps: new List<Tuple<long, long>>(),
							newSoundEffectsToPlay: new List<string>(),
							bossHealthMeterNumber: null,
							bossHealthMeterMilliPercentage: null);
					}
				
				case ObjectAction.Type.ConditionalNextAction:
					ResultOfAction actionResult = HandleAction(
							action: action.ConditionalNextActionCurrentAction,
							obj: obj,
							playerXMillis: playerXMillis,
							playerYMillis: playerYMillis,
							elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
							isPlayerDestroyed: isPlayerDestroyed,
							enemyObjectTemplates: enemyObjectTemplates,
							rng: rng);
					
					bool shouldMoveToNext = action.ConditionalNextActionConditional.Evaluate(
						obj.GetEnemyObjectExpressionInfo(),
						isParentDestroyed: isParentDestroyed,
						isPlayerDestroyed: isPlayerDestroyed,
						playerXMillis: playerXMillis,
						playerYMillis: playerYMillis,
						elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
						rng: rng);

					return new ResultOfAction(
						newObjectAction: shouldMoveToNext
							? action.ConditionalNextActionNextAction
							: ObjectAction.ConditionalNextAction(actionResult.NewObjectAction, action.ConditionalNextActionConditional, action.ConditionalNextActionNextAction),
						shouldEndLevel: actionResult.ShouldEndLevel,
						newEnemyObjects: actionResult.NewEnemyObjects,
						newPowerUps: actionResult.NewPowerUps,
						newSoundEffectsToPlay: actionResult.NewSoundEffectsToPlay,
						bossHealthMeterNumber: actionResult.BossHealthMeterNumber,
						bossHealthMeterMilliPercentage: actionResult.BossHealthMeterMilliPercentage);

				case ObjectAction.Type.Union:
				{
					bool shouldEndLevel = false;
					var newUnionActions = new List<ObjectAction>();
					var enemyObjects = new List<EnemyObject>();
					var newPowerUps = new List<Tuple<long, long>>();
					var newSoundEffectsToPlay = new List<string>();
					long? bossHealthMeterNumber = null;
					long? bossHealthMeterMilliPercentage = null;

					for (var i = 0; i < action.UnionActions.Count; i++)
					{
						ResultOfAction r = HandleAction(
								action: action.UnionActions[i],
								obj: obj,
								playerXMillis: playerXMillis,
								playerYMillis: playerYMillis,
								elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
								isPlayerDestroyed: isPlayerDestroyed,
								enemyObjectTemplates: enemyObjectTemplates,
								rng: rng);

						newUnionActions.Add(r.NewObjectAction);

						if (r.ShouldEndLevel)
							shouldEndLevel = true;

						foreach (EnemyObject newEnemyObj in r.NewEnemyObjects)
							enemyObjects.Add(newEnemyObj);

						foreach (var newPowerUp in r.NewPowerUps)
							newPowerUps.Add(newPowerUp);

						foreach (var newSoundEffect in r.NewSoundEffectsToPlay)
							newSoundEffectsToPlay.Add(newSoundEffect);

						if (r.BossHealthMeterNumber != null)
							bossHealthMeterNumber = r.BossHealthMeterNumber;

						if (r.BossHealthMeterMilliPercentage != null)
							bossHealthMeterMilliPercentage = r.BossHealthMeterMilliPercentage;
					}

					return new ResultOfAction(
						newObjectAction: ObjectAction.Union(newUnionActions),
						shouldEndLevel: shouldEndLevel,
						newEnemyObjects: enemyObjects,
						newPowerUps: newPowerUps,
						newSoundEffectsToPlay: newSoundEffectsToPlay,
						bossHealthMeterNumber: bossHealthMeterNumber,
						bossHealthMeterMilliPercentage: bossHealthMeterMilliPercentage);
				}
				default:
					throw new Exception();
			}
		}
	}
}
