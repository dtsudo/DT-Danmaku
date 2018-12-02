
namespace DTDanmakuLib
{
	using DTLib;
	using System.Collections.Generic;

	public class EnemyObject
	{
		public const long DAMAGE_TAKEN_IN_MILLIHP_WHEN_COLLIDE_WITH_PLAYER = 1000;
		public const long DAMAGE_TAKEN_IN_MILLIHP_WHEN_HIT_BY_PLAYER_BULLET = 1000;

		private EnemyObjectType _objectType;
		private DTImmutableList<ObjectBox> _damageBoxes;
		private DTImmutableList<ObjectBox> _collisionBoxes;
		private EnemyObject _parentObject;

		public EnemyObjectType ObjectType { get { return this._objectType; } }
		public ObjectAction Action { get; set; }
		public long? MilliHP { get; set; }
		public DTImmutableList<ObjectBox> DamageBoxes { get { return this._damageBoxes; } } // nullable; does not rotate (i.e. does not respect FacingDirectionInMillidegrees)
		public DTImmutableList<ObjectBox> CollisionBoxes { get { return this._collisionBoxes; } } // nullable; does not rotate (i.e. does not respect FacingDirectionInMillidegrees)
		public string SpriteName { get; set; } // nullable

		public long XMillis { get; set; }
		public long YMillis { get; set; }
		public long SpeedInMillipixelsPerMillisecond { get; set; }
		public long MovementDirectionInMillidegrees { get; set; }
		public long FacingDirectionInMillidegrees { get; set; }
		public bool IsDestroyed { get; set; }
		public EnemyObject ParentObject { get { return this._parentObject; } } // nullable
		public Dictionary<string, long> NumericVariables { get; set; }
		public Dictionary<string, bool> BooleanVariables { get; set; }

		public EnemyObjectExpressionInfo GetEnemyObjectExpressionInfo()
		{
			return new EnemyObjectExpressionInfo(
				numericVariables: this.NumericVariables,
				booleanVariables: this.BooleanVariables,
				xMillis: this.XMillis,
				yMillis: this.YMillis,
				milliHP: this.MilliHP,
				parent: this.ParentObject != null ? this.ParentObject.GetEnemyObjectExpressionInfo() : null);
		}

		public EnemyObject(
			EnemyObjectTemplate template,
			long initialXMillis,
			long initialYMillis,
			long playerXMillis,
			long playerYMillis,
			long elapsedMillisecondsPerIteration,
			bool isPlayerDestroyed,
			EnemyObject parent /* nullable */,
			Dictionary<string, IMathExpression> initialNumericVariables, /* MathExpression is with respect to the parent */ /* nullable */
			Dictionary<string, BooleanExpression> initialBooleanVariables /* BooleanExpression is with respect to the parent */ /* nullable */,
			IDTDeterministicRandom rng)
		{
			this._objectType = template.ObjectType;
			this._damageBoxes = template.DamageBoxes;
			this._collisionBoxes = template.CollisionBoxes;
			this.SpriteName = template.SpriteName;
			this.Action = template.Action;
			if (template.InitialMilliHP != null)
			{
				EnemyObjectExpressionInfo enemyObjectExpressionInfo;

				if (parent != null)
					enemyObjectExpressionInfo = parent.GetEnemyObjectExpressionInfo();
				else
					enemyObjectExpressionInfo = null;

				this.MilliHP = template.InitialMilliHP.Evaluate(
					enemyObjectExpressionInfo: enemyObjectExpressionInfo,
					playerXMillis: playerXMillis,
					playerYMillis: playerYMillis,
					elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
					rng: rng);
			}
			else
			{
				this.MilliHP = null;
			}
			this.XMillis = initialXMillis;
			this.YMillis = initialYMillis;
			this.SpeedInMillipixelsPerMillisecond = 0;
			this.MovementDirectionInMillidegrees = 180 * 1000;
			this.FacingDirectionInMillidegrees = 180 * 1000;
			this.IsDestroyed = false;
			this._parentObject = parent;
			this.NumericVariables = new Dictionary<string, long>();
			this.BooleanVariables = new Dictionary<string, bool>();
			if (initialNumericVariables != null)
			{
				foreach (var keyValuePair in initialNumericVariables)
				{
					string variableName = keyValuePair.Key;
					EnemyObjectExpressionInfo enemyObjectExpressionInfo;
					if (parent != null)
						enemyObjectExpressionInfo = parent.GetEnemyObjectExpressionInfo();
					else
						enemyObjectExpressionInfo = null;

					this.NumericVariables.Add(variableName,
						keyValuePair.Value.Evaluate(
							enemyObjectExpressionInfo: enemyObjectExpressionInfo,
							playerXMillis: playerXMillis,
							playerYMillis: playerYMillis,
							elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
							rng: rng));
				}
			}
			if (initialBooleanVariables != null)
			{
				foreach (var keyValuePair in initialBooleanVariables)
				{
					string variableName = keyValuePair.Key;
					EnemyObjectExpressionInfo enemyObjectExpressionInfo;
					if (parent != null)
						enemyObjectExpressionInfo = parent.GetEnemyObjectExpressionInfo();
					else
						enemyObjectExpressionInfo = null;

					bool? isParentDestroyed;
					if (parent != null)
						isParentDestroyed = parent.IsDestroyed;
					else
						isParentDestroyed = null;

					this.BooleanVariables.Add(variableName,
						keyValuePair.Value.Evaluate(
							enemyObjectExpressionInfo: enemyObjectExpressionInfo,
							isParentDestroyed: isParentDestroyed,
							isPlayerDestroyed: isPlayerDestroyed,
							playerXMillis: playerXMillis,
							playerYMillis: playerYMillis,
							elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
							rng: rng));
				}
			}
		}
	}
}
