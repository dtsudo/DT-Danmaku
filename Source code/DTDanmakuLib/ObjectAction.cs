
namespace DTDanmakuLib
{
	using DTLib;
	using System.Collections.Generic;

	public class ObjectAction
	{
		public enum Type
		{
			Move,
			StrafeMove,
			SetSpeed,
			IncreaseSpeed,
			DecreaseSpeed,
			SetPosition,
			SetFacingDirection,
			Destroy, // No-op if object is already destroyed (e.g. if you have a Union of two Destroy actions)
			DestroyParent, // No-op if object is already destroyed (e.g. if you have a Union of two Destroy actions)
			SpawnChild,
			SpawnPowerUp,
			SetNumericVariable,
			SetBooleanVariable,
			SetParentNumericVariable,
			SetParentBooleanVariable,
			EndLevel,
			PlaySoundEffect,
			DisplayBossHealthBar,
			SetSpriteName,

			Conditional,
			ConditionalNextAction, // guaranteed to run current action at least once
			Union
		}

		public Type ObjectActionType { get; private set; }

		// Needed for Move and StrafeMove
		public IMathExpression MoveToXMillis { get; private set; }
		public IMathExpression MoveToYMillis { get; private set; }

		// Needed for SetSpeed, IncreaseSpeed, and DecreaseSpeed
		public IMathExpression SpeedInMillipixelsPerMillisecond { get; private set; }

		// Needed for SetPosition
		public IMathExpression SetXMillisPosition { get; private set; }
		public IMathExpression SetYMillisPosition { get; private set; }

		// Needed for SetFacingDirection
		public IMathExpression SetFacingDirectionInMillidegrees { get; private set; }

		// Needed for SpawnChild
		public class InitialChildNumericVariableInfo
		{
			public string Name { get; private set; }
			public IMathExpression Value { get; private set; }
			public InitialChildNumericVariableInfo(string name, IMathExpression value)
			{
				this.Name = name;
				this.Value = value;
			}
		}
		public class InitialChildBooleanVariableInfo
		{
			public string Name { get; private set; }
			public BooleanExpression Value { get; private set; }
			public InitialChildBooleanVariableInfo(string name, BooleanExpression value)
			{
				this.Name = name;
				this.Value = value;
			}
		}
		public IMathExpression SpawnChildXMillis { get; private set; }
		public IMathExpression SpawnChildYMillis { get; private set; }
		public string SpawnChildObjectTemplateName { get; private set; }
		public DTImmutableList<InitialChildNumericVariableInfo> SpawnChildInitialChildNumericVariables { get; private set; } /* MathExpression is with respect to the parent */ /* This property is nullable */
		public DTImmutableList<InitialChildBooleanVariableInfo> SpawnChildInitialChildBooleanVariables { get; private set; } /* BooleanExpression is with respect to the parent */ /* This property is nullable */

		// Needed for SetNumericVariable, SetBooleanVariable, SetParentNumericVariable, and SetParentBooleanVariable
		public string SetVariableName { get; private set; }
		// Needed for SetNumericVariable and SetParentNumericVariable
		public IMathExpression SetNumericVariableValue { get; private set; } /* In both cases, MathExpression is with respect to the current object */
		// Needed for SetBooleanVariable and SetParentBooleanVariable
		public BooleanExpression SetBooleanVariableValue { get; private set; } /* In both cases, MathExpression is with respect to the current object */

		// Needed for PlaySoundEffect
		public string SoundEffectName { get; private set; }

		// Needed for DisplayBossHealthBar
		public IMathExpression BossHealthBarMilliPercentage { get; private set; } // between 0 and 100 * 1000
		public IMathExpression BossHealthBarMeterNumber { get; private set; } // 1 is the final health bar; 2 is the 2nd final health bar; etc

		// Needed for SetSpriteName
		public string SpriteName { get; private set; } // nullable

		// Needed for Conditional
		public BooleanExpression Conditional { get; private set; }
		public ObjectAction ConditionalAction { get; private set; }

		// Needed for ConditionalNextAction
		public ObjectAction ConditionalNextActionCurrentAction { get; private set; }
		public BooleanExpression ConditionalNextActionConditional { get; private set; }
		public ObjectAction ConditionalNextActionNextAction { get; private set; }

		// Needed for Union
		public DTImmutableList<ObjectAction> UnionActions { get; private set; }

		private ObjectAction() { }

		public static ObjectAction Move(IMathExpression moveToXMillis, IMathExpression moveToYMillis)
		{
			ObjectAction action = new ObjectAction();
			action.ObjectActionType = Type.Move;
			action.MoveToXMillis = moveToXMillis;
			action.MoveToYMillis = moveToYMillis;
			return action;
		}

		public static ObjectAction StrafeMove(IMathExpression moveToXMillis, IMathExpression moveToYMillis)
		{
			ObjectAction action = new ObjectAction();
			action.ObjectActionType = Type.StrafeMove;
			action.MoveToXMillis = moveToXMillis;
			action.MoveToYMillis = moveToYMillis;
			return action;
		}

		public static ObjectAction SetSpeed(IMathExpression speedInMillipixelsPerMillisecond)
		{
			ObjectAction action = new ObjectAction();
			action.ObjectActionType = Type.SetSpeed;
			action.SpeedInMillipixelsPerMillisecond = speedInMillipixelsPerMillisecond;
			return action;
		}

		public static ObjectAction IncreaseSpeed(IMathExpression speedInMillipixelsPerMillisecond)
		{
			ObjectAction action = new ObjectAction();
			action.ObjectActionType = Type.IncreaseSpeed;
			action.SpeedInMillipixelsPerMillisecond = speedInMillipixelsPerMillisecond;
			return action;
		}

		public static ObjectAction DecreaseSpeed(IMathExpression speedInMillipixelsPerMillisecond)
		{
			ObjectAction action = new ObjectAction();
			action.ObjectActionType = Type.DecreaseSpeed;
			action.SpeedInMillipixelsPerMillisecond = speedInMillipixelsPerMillisecond;
			return action;
		}

		public static ObjectAction SetPosition(IMathExpression xMillis, IMathExpression yMillis)
		{
			ObjectAction action = new ObjectAction();
			action.ObjectActionType = Type.SetPosition;
			action.SetXMillisPosition = xMillis;
			action.SetYMillisPosition = yMillis;
			return action;
		}

		public static ObjectAction SetFacingDirection(IMathExpression facingDirectionInMillidegrees)
		{
			ObjectAction action = new ObjectAction();
			action.ObjectActionType = Type.SetFacingDirection;
			action.SetFacingDirectionInMillidegrees = facingDirectionInMillidegrees;
			return action;
		}

		public static ObjectAction Destroy()
		{
			ObjectAction action = new ObjectAction();
			action.ObjectActionType = Type.Destroy;
			return action;
		}

		public static ObjectAction DestroyParent()
		{
			ObjectAction action = new ObjectAction();
			action.ObjectActionType = Type.DestroyParent;
			return action;
		}

		public static ObjectAction SpawnChild(
			IMathExpression childXMillis,
			IMathExpression childYMillis,
			string childObjectTemplateName,
			List<InitialChildNumericVariableInfo> childInitialNumericVariables,
			List<InitialChildBooleanVariableInfo> childInitialBooleanVariables)
		{
			ObjectAction action = new ObjectAction();
			action.ObjectActionType = Type.SpawnChild;
			action.SpawnChildXMillis = childXMillis;
			action.SpawnChildYMillis = childYMillis;
			action.SpawnChildObjectTemplateName = childObjectTemplateName;
			if (childInitialNumericVariables != null)
				action.SpawnChildInitialChildNumericVariables = new DTImmutableList<InitialChildNumericVariableInfo>(list: childInitialNumericVariables);
			if (childInitialBooleanVariables != null)
				action.SpawnChildInitialChildBooleanVariables = new DTImmutableList<InitialChildBooleanVariableInfo>(list: childInitialBooleanVariables);
			return action;
		}

		public static ObjectAction SpawnPowerUp()
		{
			ObjectAction action = new ObjectAction();
			action.ObjectActionType = Type.SpawnPowerUp;
			return action;
		}

		public static ObjectAction SetNumericVariable(string variableName, IMathExpression variableValue)
		{
			ObjectAction action = new ObjectAction();
			action.ObjectActionType = Type.SetNumericVariable;
			action.SetVariableName = variableName;
			action.SetNumericVariableValue = variableValue;
			return action;
		}

		public static ObjectAction SetBooleanVariable(string variableName, BooleanExpression variableValue)
		{
			ObjectAction action = new ObjectAction();
			action.ObjectActionType = Type.SetBooleanVariable;
			action.SetVariableName = variableName;
			action.SetBooleanVariableValue = variableValue;
			return action;
		}

		public static ObjectAction SetParentNumericVariable(string variableName, IMathExpression variableValue)
		{
			ObjectAction action = new ObjectAction();
			action.ObjectActionType = Type.SetParentNumericVariable;
			action.SetVariableName = variableName;
			action.SetNumericVariableValue = variableValue;
			return action;
		}

		public static ObjectAction SetParentBooleanVariable(string variableName, BooleanExpression variableValue)
		{
			ObjectAction action = new ObjectAction();
			action.ObjectActionType = Type.SetParentBooleanVariable;
			action.SetVariableName = variableName;
			action.SetBooleanVariableValue = variableValue;
			return action;
		}

		public static ObjectAction EndLevel()
		{
			ObjectAction action = new ObjectAction();
			action.ObjectActionType = Type.EndLevel;
			return action;
		}

		public static ObjectAction PlaySoundEffect(string soundEffectName)
		{
			ObjectAction action = new ObjectAction();
			action.ObjectActionType = Type.PlaySoundEffect;
			action.SoundEffectName = soundEffectName;
			return action;
		}

		public static ObjectAction DisplayBossHealthBar(IMathExpression healthBarMeterNumber, IMathExpression healthBarMilliPercentage)
		{
			ObjectAction action = new ObjectAction();
			action.ObjectActionType = Type.DisplayBossHealthBar;
			action.BossHealthBarMeterNumber = healthBarMeterNumber;
			action.BossHealthBarMilliPercentage = healthBarMilliPercentage;
			return action;
		}

		public static ObjectAction SetSpriteName(string spriteName)
		{
			ObjectAction action = new ObjectAction();
			action.ObjectActionType = Type.SetSpriteName;
			action.SpriteName = spriteName;
			return action;
		}

		public static ObjectAction Condition(BooleanExpression condition, ObjectAction action)
		{
			ObjectAction a = new ObjectAction();
			a.ObjectActionType = Type.Conditional;
			a.Conditional = condition;
			a.ConditionalAction = action;
			return a;
		}

		public static ObjectAction ConditionalNextAction(ObjectAction currentAction, BooleanExpression condition, ObjectAction nextAction)
		{
			ObjectAction action = new ObjectAction();
			action.ObjectActionType = Type.ConditionalNextAction;
			action.ConditionalNextActionCurrentAction = currentAction;
			action.ConditionalNextActionConditional = condition;
			action.ConditionalNextActionNextAction = nextAction;
			return action;
		}

		public static ObjectAction Union(List<ObjectAction> actions)
		{
			ObjectAction action = new ObjectAction();
			action.ObjectActionType = Type.Union;
			action.UnionActions = new DTImmutableList<ObjectAction>(list: actions);
			return action;
		}

		public static ObjectAction Union(ObjectAction action1, ObjectAction action2)
		{
			List<ObjectAction> list = new List<ObjectAction>();
			list.Add(action1);
			list.Add(action2);
			return ObjectAction.Union(list);
		}

		public static ObjectAction Union(ObjectAction action1, ObjectAction action2, ObjectAction action3)
		{
			List<ObjectAction> list = new List<ObjectAction>();
			list.Add(action1);
			list.Add(action2);
			list.Add(action3);
			return ObjectAction.Union(list);
		}

		public static ObjectAction Union(ObjectAction action1, ObjectAction action2, ObjectAction action3, ObjectAction action4)
		{
			List<ObjectAction> list = new List<ObjectAction>();
			list.Add(action1);
			list.Add(action2);
			list.Add(action3);
			list.Add(action4);
			return ObjectAction.Union(list);
		}

		public static ObjectAction Union(ObjectAction action1, ObjectAction action2, ObjectAction action3, ObjectAction action4, ObjectAction action5)
		{
			List<ObjectAction> list = new List<ObjectAction>();
			list.Add(action1);
			list.Add(action2);
			list.Add(action3);
			list.Add(action4);
			list.Add(action5);
			return ObjectAction.Union(list);
		}

		public static ObjectAction Union(ObjectAction action1, ObjectAction action2, ObjectAction action3, ObjectAction action4, ObjectAction action5, ObjectAction action6)
		{
			List<ObjectAction> list = new List<ObjectAction>();
			list.Add(action1);
			list.Add(action2);
			list.Add(action3);
			list.Add(action4);
			list.Add(action5);
			list.Add(action6);
			return ObjectAction.Union(list);
		}

		public static ObjectAction Union(ObjectAction action1, ObjectAction action2, ObjectAction action3, ObjectAction action4, ObjectAction action5, ObjectAction action6, ObjectAction action7)
		{
			List<ObjectAction> list = new List<ObjectAction>();
			list.Add(action1);
			list.Add(action2);
			list.Add(action3);
			list.Add(action4);
			list.Add(action5);
			list.Add(action6);
			list.Add(action7);
			return ObjectAction.Union(list);
		}

		public static ObjectAction Union(
			ObjectAction action1,
			ObjectAction action2,
			ObjectAction action3,
			ObjectAction action4,
			ObjectAction action5,
			ObjectAction action6,
			ObjectAction action7,
			ObjectAction action8)
		{
			List<ObjectAction> list = new List<ObjectAction>();
			list.Add(action1);
			list.Add(action2);
			list.Add(action3);
			list.Add(action4);
			list.Add(action5);
			list.Add(action6);
			list.Add(action7);
			list.Add(action8);
			return ObjectAction.Union(list);
		}
	}
}
