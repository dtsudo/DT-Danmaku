
namespace DTDanmakuLib
{
	using System;
	using System.Collections.Generic;

	public class DestructionAnimationGenerator
	{
		public class GenerateDestructionAnimationResult
		{
			public ObjectAction objectAction;
			public Dictionary<string, DTDanmakuImage> spriteNameToImageDictionary;
			public Dictionary<string, EnemyObjectTemplate> enemyObjectTemplates;

			public GenerateDestructionAnimationResult(
				ObjectAction objectAction,
				Dictionary<string, DTDanmakuImage> spriteNameToImageDictionary,
				Dictionary<string, EnemyObjectTemplate> enemyObjectTemplates)
			{
				this.objectAction = objectAction;
				this.spriteNameToImageDictionary = spriteNameToImageDictionary;
				this.enemyObjectTemplates = enemyObjectTemplates;
			}
		}

		private const string elapsedTimeInMillisVariableName = "timeInMillis";

		private static Tuple<ObjectAction, EnemyObjectTemplate> SpawnOneDestructionSpriteImage(
			long startMilliseconds,
			long endMilliseconds,
			string childObjectTemplateName,
			string spriteName,
			bool isLast)
		{
			List<ObjectAction> destroyActions = new List<ObjectAction>();
			destroyActions.Add(ObjectAction.Destroy());
			destroyActions.Add(ObjectAction.DestroyParent());

			ObjectAction destroySelfAction = ObjectAction.Condition(
				condition: BooleanExpression.GreaterThanOrEqualTo(
					leftSide: MathExpression.ParentVariable(elapsedTimeInMillisVariableName),
					rightSide: MathExpression.Constant(endMilliseconds)),
				action: isLast ? ObjectAction.Union(destroyActions) : ObjectAction.Destroy());

			ObjectAction spawnChildAction = ObjectAction.SpawnChild(
				childXMillis: MathExpression.XMillis(),
				childYMillis: MathExpression.YMillis(),
				childObjectTemplateName: childObjectTemplateName,
				childInitialNumericVariables: null,
				childInitialBooleanVariables: null);

			ObjectAction delayedSpawnChildAction = 
				ObjectAction.Condition(
					condition: BooleanExpression.GreaterThanOrEqualTo(
						leftSide: MathExpression.Variable(elapsedTimeInMillisVariableName),
						rightSide: MathExpression.Constant(startMilliseconds)),
					action: ObjectActionGenerator.DoOnce(spawnChildAction));

			EnemyObjectTemplate template = EnemyObjectTemplate.Enemy(
				action: destroySelfAction,
				initialMilliHP: null,
				damageBoxes: null,
				collisionBoxes: null,
				spriteName: spriteName);

			return new Tuple<ObjectAction, EnemyObjectTemplate>(delayedSpawnChildAction, template);
		}

		public static GenerateDestructionAnimationResult GenerateDestructionAnimation(
			List<DTDanmakuImage> orderedSprites,
			long millisecondsPerSprite,
			GuidGenerator guidGenerator)
		{
			Dictionary<string, DTDanmakuImage> spriteNameToImageDictionary = new Dictionary<string, DTDanmakuImage>();
			Dictionary<string, EnemyObjectTemplate> enemyObjectTemplates = new Dictionary<string, EnemyObjectTemplate>();

			ObjectAction incrementElapsedTimeVariable = ObjectAction.SetNumericVariable(
				variableName: elapsedTimeInMillisVariableName,
				variableValue: MathExpression.Add(
					leftSide: MathExpression.Variable(variableName: elapsedTimeInMillisVariableName),
					rightSide: MathExpression.ElapsedMillisecondsPerIteration()));

			long startMilliseconds = 0;
			List<ObjectAction> actionsToUnionTogether = new List<ObjectAction>();
			actionsToUnionTogether.Add(incrementElapsedTimeVariable);
			for (int i = 0; i < orderedSprites.Count; i++)
			{
				DTDanmakuImage image = orderedSprites[i];
				string spriteName = guidGenerator.NextGuid();
				string childObjectTemplateName = guidGenerator.NextGuid();

				Tuple<ObjectAction, EnemyObjectTemplate> result = SpawnOneDestructionSpriteImage(
					startMilliseconds: startMilliseconds,
					endMilliseconds: startMilliseconds + millisecondsPerSprite,
					childObjectTemplateName: childObjectTemplateName,
					spriteName: spriteName,
					isLast: i == orderedSprites.Count - 1);
				startMilliseconds += millisecondsPerSprite;

				actionsToUnionTogether.Add(result.Item1);
				spriteNameToImageDictionary.Add(spriteName, image);
				enemyObjectTemplates.Add(childObjectTemplateName, result.Item2);
			}
			
			string placeholderObjectTemplateName = guidGenerator.NextGuid();

			List<ObjectAction.InitialChildNumericVariableInfo> initialChildNumericVariables = new List<ObjectAction.InitialChildNumericVariableInfo>();
			initialChildNumericVariables.Add(new ObjectAction.InitialChildNumericVariableInfo(elapsedTimeInMillisVariableName, MathExpression.Constant(0)));
			ObjectAction action = ObjectAction.SpawnChild(
				childXMillis: MathExpression.XMillis(),
				childYMillis: MathExpression.YMillis(),
				childObjectTemplateName: placeholderObjectTemplateName,
				childInitialNumericVariables: initialChildNumericVariables,
				childInitialBooleanVariables: null);

			EnemyObjectTemplate placeholderObjectTemplate = EnemyObjectTemplate.Enemy(
				action: ObjectAction.Union(actionsToUnionTogether),
				initialMilliHP: null,
				damageBoxes: null,
				collisionBoxes: null,
				spriteName: null);

			enemyObjectTemplates.Add(placeholderObjectTemplateName, placeholderObjectTemplate);

			return new GenerateDestructionAnimationResult(
				objectAction: action,
				spriteNameToImageDictionary: spriteNameToImageDictionary,
				enemyObjectTemplates: enemyObjectTemplates);
		}
	}
}
