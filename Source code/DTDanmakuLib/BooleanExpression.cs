
namespace DTDanmakuLib
{
	using DTLib;
	using System;

	public class BooleanExpression
	{
		public enum Type
		{
			// Leaf nodes
			True,
			False,
			Variable,
			ParentVariable,
			IsParentDestroyed,
			IsPlayerDestroyed,
			Equal,
			NotEqual,
			GreaterThan,
			GreaterThanOrEqualTo,
			LessThan,
			LessThanOrEqualTo,

			// Non-leaf nodes
			And,
			Or,
			Not
		}

		public Type BooleanExpressionType { get; private set; }

		// Required for Type = Variable or ParentVariable
		public string VariableName { get; private set; }

		// Required for Type = Equal, NotEqual, GreaterThan, GreaterThanOrEqualTo, LessThan, LessThanOrEqualTo
		public IMathExpression MathLeftSide { get; private set; }
		public IMathExpression MathRightSide { get; private set; }

		// Required for Type = And, Or, Not
		public BooleanExpression BooleanOperand1 { get; private set; }
		// Required for Type = And, Or
		public BooleanExpression BooleanOperand2 { get; private set; }

		private BooleanExpression() { }

		public static BooleanExpression True()
		{
			BooleanExpression expression = new BooleanExpression();
			expression.BooleanExpressionType = Type.True;
			return expression;
		}

		public static BooleanExpression False()
		{
			BooleanExpression expression = new BooleanExpression();
			expression.BooleanExpressionType = Type.False;
			return expression;
		}

		public static BooleanExpression Variable(string variableName)
		{
			BooleanExpression expression = new BooleanExpression();
			expression.BooleanExpressionType = Type.Variable;
			expression.VariableName = variableName;
			return expression;
		}

		public static BooleanExpression ParentVariable(string variableName)
		{
			BooleanExpression expression = new BooleanExpression();
			expression.BooleanExpressionType = Type.ParentVariable;
			expression.VariableName = variableName;
			return expression;
		}

		public static BooleanExpression IsParentDestroyed()
		{
			BooleanExpression expression = new BooleanExpression();
			expression.BooleanExpressionType = Type.IsParentDestroyed;
			return expression;
		}

		public static BooleanExpression IsPlayerDestroyed()
		{
			BooleanExpression expression = new BooleanExpression();
			expression.BooleanExpressionType = Type.IsPlayerDestroyed;
			return expression;
		}

		public static BooleanExpression Equal(IMathExpression leftSide, IMathExpression rightSide)
		{
			BooleanExpression expression = new BooleanExpression();
			expression.BooleanExpressionType = Type.Equal;
			expression.MathLeftSide = leftSide;
			expression.MathRightSide = rightSide;
			return expression;
		}

		public static BooleanExpression NotEqual(IMathExpression leftSide, IMathExpression rightSide)
		{
			BooleanExpression expression = new BooleanExpression();
			expression.BooleanExpressionType = Type.NotEqual;
			expression.MathLeftSide = leftSide;
			expression.MathRightSide = rightSide;
			return expression;
		}

		public static BooleanExpression GreaterThan(IMathExpression leftSide, IMathExpression rightSide)
		{
			BooleanExpression expression = new BooleanExpression();
			expression.BooleanExpressionType = Type.GreaterThan;
			expression.MathLeftSide = leftSide;
			expression.MathRightSide = rightSide;
			return expression;
		}

		public static BooleanExpression GreaterThanOrEqualTo(IMathExpression leftSide, IMathExpression rightSide)
		{
			BooleanExpression expression = new BooleanExpression();
			expression.BooleanExpressionType = Type.GreaterThanOrEqualTo;
			expression.MathLeftSide = leftSide;
			expression.MathRightSide = rightSide;
			return expression;
		}

		public static BooleanExpression GreaterThanOrEqualTo(IMathExpression leftSide, long rightSide)
		{
			return BooleanExpression.GreaterThanOrEqualTo(leftSide, MathExpression.Constant(rightSide));
		}

		public static BooleanExpression GreaterThanOrEqualTo(long leftSide, IMathExpression rightSide)
		{
			return BooleanExpression.GreaterThanOrEqualTo(MathExpression.Constant(leftSide), rightSide);
		}

		public static BooleanExpression LessThan(IMathExpression leftSide, IMathExpression rightSide)
		{
			BooleanExpression expression = new BooleanExpression();
			expression.BooleanExpressionType = Type.LessThan;
			expression.MathLeftSide = leftSide;
			expression.MathRightSide = rightSide;
			return expression;
		}

		public static BooleanExpression LessThanOrEqualTo(IMathExpression leftSide, IMathExpression rightSide)
		{
			BooleanExpression expression = new BooleanExpression();
			expression.BooleanExpressionType = Type.LessThanOrEqualTo;
			expression.MathLeftSide = leftSide;
			expression.MathRightSide = rightSide;
			return expression;
		}

		public static BooleanExpression LessThanOrEqualTo(IMathExpression leftSide, long rightSide)
		{
			return BooleanExpression.LessThanOrEqualTo(leftSide, MathExpression.Constant(rightSide));
		}

		public static BooleanExpression LessThanOrEqualTo(long leftSide, IMathExpression rightSide)
		{
			return BooleanExpression.LessThanOrEqualTo(MathExpression.Constant(leftSide), rightSide);
		}

		public static BooleanExpression And(BooleanExpression leftSide, BooleanExpression rightSide)
		{
			BooleanExpression expression = new BooleanExpression();
			expression.BooleanExpressionType = Type.And;
			expression.BooleanOperand1 = leftSide;
			expression.BooleanOperand2 = rightSide;
			return expression;
		}

		public static BooleanExpression Or(BooleanExpression leftSide, BooleanExpression rightSide)
		{
			BooleanExpression expression = new BooleanExpression();
			expression.BooleanExpressionType = Type.Or;
			expression.BooleanOperand1 = leftSide;
			expression.BooleanOperand2 = rightSide;
			return expression;
		}

		public static BooleanExpression Or(BooleanExpression b1, BooleanExpression b2, BooleanExpression b3)
		{
			// Note the careful attention to short-circuiting behavior
			return BooleanExpression.Or(BooleanExpression.Or(b1, b2), b3);
		}

		public static BooleanExpression Or(BooleanExpression b1, BooleanExpression b2, BooleanExpression b3, BooleanExpression b4)
		{
			// Note the careful attention to short-circuiting behavior
			return BooleanExpression.Or(BooleanExpression.Or(b1, b2, b3), b4);
		}


		public static BooleanExpression Not(BooleanExpression boolean)
		{
			BooleanExpression expression = new BooleanExpression();
			expression.BooleanExpressionType = Type.Not;
			expression.BooleanOperand1 = boolean;
			return expression;
		}




		public bool Evaluate(
			EnemyObjectExpressionInfo enemyObjectExpressionInfo, // nullable
			bool? isParentDestroyed, // null if parent doesn't exist
			bool isPlayerDestroyed,
			long playerXMillis,
			long playerYMillis,
			long elapsedMillisecondsPerIteration,
			IDTDeterministicRandom rng)
		{
			switch (this.BooleanExpressionType)
			{
				case Type.True:
					return true;
				case Type.False:
					return false;
				case Type.Variable:
					if (enemyObjectExpressionInfo == null)
						throw new Exception();
					return enemyObjectExpressionInfo.BooleanVariables[this.VariableName];
				case Type.ParentVariable:
					if (enemyObjectExpressionInfo == null)
						throw new Exception();
					if (enemyObjectExpressionInfo.Parent == null)
						throw new Exception();
					return enemyObjectExpressionInfo.Parent.BooleanVariables[this.VariableName];
				case Type.IsParentDestroyed:
					if (isParentDestroyed == null)
						throw new Exception();
					return isParentDestroyed.Value;
				case Type.IsPlayerDestroyed:
					return isPlayerDestroyed;
				case Type.Equal:
				case Type.NotEqual:
				case Type.GreaterThan:
				case Type.GreaterThanOrEqualTo:
				case Type.LessThan:
				case Type.LessThanOrEqualTo:
					long leftSide = this.MathLeftSide.Evaluate(
						enemyObjectExpressionInfo: enemyObjectExpressionInfo,
						playerXMillis: playerXMillis,
						playerYMillis: playerYMillis,
						elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
						rng: rng);
					long rightSide = this.MathRightSide.Evaluate(
						enemyObjectExpressionInfo: enemyObjectExpressionInfo,
						playerXMillis: playerXMillis,
						playerYMillis: playerYMillis,
						elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
						rng: rng);

					if (this.BooleanExpressionType == Type.Equal)
						return leftSide == rightSide;
					else if (this.BooleanExpressionType == Type.NotEqual)
						return leftSide != rightSide;
					else if (this.BooleanExpressionType == Type.GreaterThan)
						return leftSide > rightSide;
					else if (this.BooleanExpressionType == Type.GreaterThanOrEqualTo)
						return leftSide >= rightSide;
					else if (this.BooleanExpressionType == Type.LessThan)
						return leftSide < rightSide;
					else if (this.BooleanExpressionType == Type.LessThanOrEqualTo)
						return leftSide <= rightSide;
					else
						throw new Exception();
				case Type.And:
				case Type.Or:
				case Type.Not:
					bool operand1 = this.BooleanOperand1.Evaluate(
						enemyObjectExpressionInfo: enemyObjectExpressionInfo,
						isParentDestroyed: isParentDestroyed,
						isPlayerDestroyed: isPlayerDestroyed,
						playerXMillis: playerXMillis,
						playerYMillis: playerYMillis,
						elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
						rng: rng);

					if (this.BooleanExpressionType == Type.Not)
						return !operand1;

					if (this.BooleanExpressionType == Type.And && operand1 == false)
						return false;
					if (this.BooleanExpressionType == Type.Or && operand1 == true)
						return true;

					bool operand2 = this.BooleanOperand2.Evaluate(
						enemyObjectExpressionInfo: enemyObjectExpressionInfo,
						isParentDestroyed: isParentDestroyed,
						isPlayerDestroyed: isPlayerDestroyed,
						playerXMillis: playerXMillis,
						playerYMillis: playerYMillis,
						elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
						rng: rng);

					if (this.BooleanExpressionType == Type.And)
						return operand1 && operand2;
					else if (this.BooleanExpressionType == Type.Or)
						return operand1 || operand2;
					else
						throw new Exception();
				default:
					throw new Exception();
			}
		}
	}
}

