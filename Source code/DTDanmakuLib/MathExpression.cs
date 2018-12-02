
namespace DTDanmakuLib
{
	using System.Collections.Generic;
	using System;
	using DTLib;

	public interface IMathExpression
	{
		long Evaluate(
			EnemyObjectExpressionInfo enemyObjectExpressionInfo, // nullable
			long playerXMillis,
			long playerYMillis,
			long elapsedMillisecondsPerIteration,
			IDTDeterministicRandom rng);
	}

	public class MathExpression
	{
		private class ConstantExpression : IMathExpression
		{
			private long value;

			public ConstantExpression(long value)
			{
				this.value = value;
			}

			public long Evaluate(
				EnemyObjectExpressionInfo enemyObjectExpressionInfo, // nullable
				long playerXMillis,
				long playerYMillis,
				long elapsedMillisecondsPerIteration,
				IDTDeterministicRandom rng)
			{
				return this.value;
			}
		}

		private class VariableExpression : IMathExpression
		{
			private string variableName;

			public VariableExpression(string variableName)
			{
				this.variableName = variableName;
			}

			public long Evaluate(
				EnemyObjectExpressionInfo enemyObjectExpressionInfo, // nullable
				long playerXMillis,
				long playerYMillis,
				long elapsedMillisecondsPerIteration,
				IDTDeterministicRandom rng)
			{
				if (enemyObjectExpressionInfo == null)
					throw new Exception();
				return enemyObjectExpressionInfo.NumericVariables[this.variableName];
			}
		}

		private class XMillisExpression : IMathExpression
		{
			public XMillisExpression() { }

			public long Evaluate(
				EnemyObjectExpressionInfo enemyObjectExpressionInfo, // nullable
				long playerXMillis,
				long playerYMillis,
				long elapsedMillisecondsPerIteration,
				IDTDeterministicRandom rng)
			{
				if (enemyObjectExpressionInfo == null)
					throw new Exception();
				return enemyObjectExpressionInfo.XMillis;
			}
		}

		private class YMillisExpression : IMathExpression
		{
			public YMillisExpression() { }

			public long Evaluate(
				EnemyObjectExpressionInfo enemyObjectExpressionInfo, // nullable
				long playerXMillis,
				long playerYMillis,
				long elapsedMillisecondsPerIteration,
				IDTDeterministicRandom rng)
			{
				if (enemyObjectExpressionInfo == null)
					throw new Exception();
				return enemyObjectExpressionInfo.YMillis;
			}
		}

		private class MilliHPExpression : IMathExpression
		{
			public MilliHPExpression() { }

			public long Evaluate(
				EnemyObjectExpressionInfo enemyObjectExpressionInfo, // nullable
				long playerXMillis,
				long playerYMillis,
				long elapsedMillisecondsPerIteration,
				IDTDeterministicRandom rng)
			{
				if (enemyObjectExpressionInfo == null)
					throw new Exception();
				if (enemyObjectExpressionInfo.MilliHP == null)
					throw new Exception();
				return enemyObjectExpressionInfo.MilliHP.Value;
			}
		}

		private class ParentVariableExpression : IMathExpression
		{
			private string variableName;

			public ParentVariableExpression(string variableName)
			{
				this.variableName = variableName;
			}

			public long Evaluate(
				EnemyObjectExpressionInfo enemyObjectExpressionInfo, // nullable
				long playerXMillis,
				long playerYMillis,
				long elapsedMillisecondsPerIteration,
				IDTDeterministicRandom rng)
			{
				if (enemyObjectExpressionInfo == null)
					throw new Exception();
				if (enemyObjectExpressionInfo.Parent == null)
					throw new Exception();
				return enemyObjectExpressionInfo.Parent.NumericVariables[this.variableName];
			}
		}

		private class ParentXMillisExpression : IMathExpression
		{
			public ParentXMillisExpression() { }

			public long Evaluate(
				EnemyObjectExpressionInfo enemyObjectExpressionInfo, // nullable
				long playerXMillis,
				long playerYMillis,
				long elapsedMillisecondsPerIteration,
				IDTDeterministicRandom rng)
			{
				if (enemyObjectExpressionInfo == null)
					throw new Exception();
				if (enemyObjectExpressionInfo.Parent == null)
					throw new Exception();
				return enemyObjectExpressionInfo.Parent.XMillis;
			}
		}

		private class ParentYMillisExpression : IMathExpression
		{
			public ParentYMillisExpression() { }

			public long Evaluate(
				EnemyObjectExpressionInfo enemyObjectExpressionInfo, // nullable
				long playerXMillis,
				long playerYMillis,
				long elapsedMillisecondsPerIteration,
				IDTDeterministicRandom rng)
			{
				if (enemyObjectExpressionInfo == null)
					throw new Exception();
				if (enemyObjectExpressionInfo.Parent == null)
					throw new Exception();
				return enemyObjectExpressionInfo.Parent.YMillis;
			}
		}

		private class PlayerXMillisExpression : IMathExpression
		{
			public PlayerXMillisExpression() { }

			public long Evaluate(
				EnemyObjectExpressionInfo enemyObjectExpressionInfo, // nullable
				long playerXMillis,
				long playerYMillis,
				long elapsedMillisecondsPerIteration,
				IDTDeterministicRandom rng)
			{
				return playerXMillis;
			}
		}

		private class PlayerYMillisExpression : IMathExpression
		{
			public PlayerYMillisExpression() { }

			public long Evaluate(
				EnemyObjectExpressionInfo enemyObjectExpressionInfo, // nullable
				long playerXMillis,
				long playerYMillis,
				long elapsedMillisecondsPerIteration,
				IDTDeterministicRandom rng)
			{
				return playerYMillis;
			}
		}

		private class ElapsedMillisecondsPerIterationExpression : IMathExpression
		{
			public ElapsedMillisecondsPerIterationExpression() { }

			public long Evaluate(
				EnemyObjectExpressionInfo enemyObjectExpressionInfo, // nullable
				long playerXMillis,
				long playerYMillis,
				long elapsedMillisecondsPerIteration,
				IDTDeterministicRandom rng)
			{
				return elapsedMillisecondsPerIteration;
			}
		}

		private class AddExpression : IMathExpression
		{
			private IMathExpression operand1;
			private IMathExpression operand2;

			public AddExpression(IMathExpression operand1, IMathExpression operand2)
			{
				this.operand1 = operand1;
				this.operand2 = operand2;
			}

			public long Evaluate(
				EnemyObjectExpressionInfo enemyObjectExpressionInfo, // nullable
				long playerXMillis,
				long playerYMillis,
				long elapsedMillisecondsPerIteration,
				IDTDeterministicRandom rng)
			{
				long operand1 = this.operand1.Evaluate(
					enemyObjectExpressionInfo: enemyObjectExpressionInfo,
					playerXMillis: playerXMillis,
					playerYMillis: playerYMillis,
					elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
					rng: rng);
				long operand2 = this.operand2.Evaluate(
					enemyObjectExpressionInfo: enemyObjectExpressionInfo,
					playerXMillis: playerXMillis,
					playerYMillis: playerYMillis,
					elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
					rng: rng);

				return unchecked(operand1 + operand2);
			}
		}

		private class SubtractExpression : IMathExpression
		{
			private IMathExpression operand1;
			private IMathExpression operand2;

			public SubtractExpression(IMathExpression operand1, IMathExpression operand2)
			{
				this.operand1 = operand1;
				this.operand2 = operand2;
			}

			public long Evaluate(
				EnemyObjectExpressionInfo enemyObjectExpressionInfo, // nullable
				long playerXMillis,
				long playerYMillis,
				long elapsedMillisecondsPerIteration,
				IDTDeterministicRandom rng)
			{
				long operand1 = this.operand1.Evaluate(
					enemyObjectExpressionInfo: enemyObjectExpressionInfo,
					playerXMillis: playerXMillis,
					playerYMillis: playerYMillis,
					elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
					rng: rng);
				long operand2 = this.operand2.Evaluate(
					enemyObjectExpressionInfo: enemyObjectExpressionInfo,
					playerXMillis: playerXMillis,
					playerYMillis: playerYMillis,
					elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
					rng: rng);

				return unchecked(operand1 - operand2);
			}
		}

		private class MultiplyExpression : IMathExpression
		{
			private IMathExpression operand1;
			private IMathExpression operand2;

			public MultiplyExpression(IMathExpression operand1, IMathExpression operand2)
			{
				this.operand1 = operand1;
				this.operand2 = operand2;
			}

			public long Evaluate(
				EnemyObjectExpressionInfo enemyObjectExpressionInfo, // nullable
				long playerXMillis,
				long playerYMillis,
				long elapsedMillisecondsPerIteration,
				IDTDeterministicRandom rng)
			{
				long operand1 = this.operand1.Evaluate(
					enemyObjectExpressionInfo: enemyObjectExpressionInfo,
					playerXMillis: playerXMillis,
					playerYMillis: playerYMillis,
					elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
					rng: rng);
				long operand2 = this.operand2.Evaluate(
					enemyObjectExpressionInfo: enemyObjectExpressionInfo,
					playerXMillis: playerXMillis,
					playerYMillis: playerYMillis,
					elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
					rng: rng);

				return unchecked(operand1 * operand2);
			}
		}

		private class DivideExpression : IMathExpression
		{
			private IMathExpression operand1;
			private IMathExpression operand2;

			public DivideExpression(IMathExpression operand1, IMathExpression operand2)
			{
				this.operand1 = operand1;
				this.operand2 = operand2;
			}

			public long Evaluate(
				EnemyObjectExpressionInfo enemyObjectExpressionInfo, // nullable
				long playerXMillis,
				long playerYMillis,
				long elapsedMillisecondsPerIteration,
				IDTDeterministicRandom rng)
			{
				long operand1 = this.operand1.Evaluate(
					enemyObjectExpressionInfo: enemyObjectExpressionInfo,
					playerXMillis: playerXMillis,
					playerYMillis: playerYMillis,
					elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
					rng: rng);
				long operand2 = this.operand2.Evaluate(
					enemyObjectExpressionInfo: enemyObjectExpressionInfo,
					playerXMillis: playerXMillis,
					playerYMillis: playerYMillis,
					elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
					rng: rng);

				if (operand2 == 0)
					throw new Exception();
				return unchecked(operand1 / operand2);
			}
		}

		private class MinExpression : IMathExpression
		{
			private IMathExpression operand1;
			private IMathExpression operand2;

			public MinExpression(IMathExpression operand1, IMathExpression operand2)
			{
				this.operand1 = operand1;
				this.operand2 = operand2;
			}

			public long Evaluate(
				EnemyObjectExpressionInfo enemyObjectExpressionInfo, // nullable
				long playerXMillis,
				long playerYMillis,
				long elapsedMillisecondsPerIteration,
				IDTDeterministicRandom rng)
			{
				long operand1 = this.operand1.Evaluate(
					enemyObjectExpressionInfo: enemyObjectExpressionInfo,
					playerXMillis: playerXMillis,
					playerYMillis: playerYMillis,
					elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
					rng: rng);
				long operand2 = this.operand2.Evaluate(
					enemyObjectExpressionInfo: enemyObjectExpressionInfo,
					playerXMillis: playerXMillis,
					playerYMillis: playerYMillis,
					elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
					rng: rng);

				return operand1 < operand2 ? operand1 : operand2;
			}
		}

		private class MaxExpression : IMathExpression
		{
			private IMathExpression operand1;
			private IMathExpression operand2;

			public MaxExpression(IMathExpression operand1, IMathExpression operand2)
			{
				this.operand1 = operand1;
				this.operand2 = operand2;
			}

			public long Evaluate(
				EnemyObjectExpressionInfo enemyObjectExpressionInfo, // nullable
				long playerXMillis,
				long playerYMillis,
				long elapsedMillisecondsPerIteration,
				IDTDeterministicRandom rng)
			{
				long operand1 = this.operand1.Evaluate(
					enemyObjectExpressionInfo: enemyObjectExpressionInfo,
					playerXMillis: playerXMillis,
					playerYMillis: playerYMillis,
					elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
					rng: rng);
				long operand2 = this.operand2.Evaluate(
					enemyObjectExpressionInfo: enemyObjectExpressionInfo,
					playerXMillis: playerXMillis,
					playerYMillis: playerYMillis,
					elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
					rng: rng);

				return operand1 > operand2 ? operand1 : operand2;
			}
		}
		
		// Scaled such that input is milliDegrees and output is sine of that value times 1000
		private class SineScaledExpression : IMathExpression
		{
			private IMathExpression operand;

			public SineScaledExpression(IMathExpression operand)
			{
				this.operand = operand;
			}

			public long Evaluate(
				EnemyObjectExpressionInfo enemyObjectExpressionInfo, // nullable
				long playerXMillis,
				long playerYMillis,
				long elapsedMillisecondsPerIteration,
				IDTDeterministicRandom rng)
			{
				long operand = this.operand.Evaluate(
					enemyObjectExpressionInfo: enemyObjectExpressionInfo,
					playerXMillis: playerXMillis,
					playerYMillis: playerYMillis,
					elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
					rng: rng);

				return DTMath.SineScaled(operand);
			}
		}

		// Scaled such that input is milliDegrees and output is cosine of that value times 1000
		private class CosineScaledExpression : IMathExpression
		{
			private IMathExpression operand;

			public CosineScaledExpression(IMathExpression operand)
			{
				this.operand = operand;
			}

			public long Evaluate(
				EnemyObjectExpressionInfo enemyObjectExpressionInfo, // nullable
				long playerXMillis,
				long playerYMillis,
				long elapsedMillisecondsPerIteration,
				IDTDeterministicRandom rng)
			{
				long operand = this.operand.Evaluate(
					enemyObjectExpressionInfo: enemyObjectExpressionInfo,
					playerXMillis: playerXMillis,
					playerYMillis: playerYMillis,
					elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
					rng: rng);

				return DTMath.CosineScaled(operand);
			}
		}

		// Scaled such that output is in millidegrees
		private class ArcTangentScaledExpression : IMathExpression
		{
			private IMathExpression operand1;
			private IMathExpression operand2;
			private bool changeUndefinedOutputToZero;

			/*
				When changeUndefinedOutputToZero is true, then when operand1 == operand2 == 0, the expression evaluates to 0.
				When changeUndefinedOutputToZero is false, then operand1 and operand2 must never both be zero.
			*/
			public ArcTangentScaledExpression(IMathExpression operand1, IMathExpression operand2, bool changeUndefinedOutputToZero)
			{
				this.operand1 = operand1;
				this.operand2 = operand2;
				this.changeUndefinedOutputToZero = changeUndefinedOutputToZero;
			}

			public long Evaluate(
				EnemyObjectExpressionInfo enemyObjectExpressionInfo, // nullable
				long playerXMillis,
				long playerYMillis,
				long elapsedMillisecondsPerIteration,
				IDTDeterministicRandom rng)
			{
				long operand1 = this.operand1.Evaluate(
					enemyObjectExpressionInfo: enemyObjectExpressionInfo,
					playerXMillis: playerXMillis,
					playerYMillis: playerYMillis,
					elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
					rng: rng);
				long operand2 = this.operand2.Evaluate(
					enemyObjectExpressionInfo: enemyObjectExpressionInfo,
					playerXMillis: playerXMillis,
					playerYMillis: playerYMillis,
					elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
					rng: rng);

				if (this.changeUndefinedOutputToZero && operand1 == 0L && operand2 == 0L)
					return 0;

				return DTMath.ArcTangentScaled(operand1, operand2);
			}
		}

		// yields a random integer in [0, RandomExclusiveBound)
		private class RandomIntegerExpression : IMathExpression
		{
			private IMathExpression operand;

			public RandomIntegerExpression(IMathExpression operand)
			{
				this.operand = operand;
			}

			public long Evaluate(
				EnemyObjectExpressionInfo enemyObjectExpressionInfo, // nullable
				long playerXMillis,
				long playerYMillis,
				long elapsedMillisecondsPerIteration,
				IDTDeterministicRandom rng)
			{
				long randomExclusiveBound = this.operand.Evaluate(
					enemyObjectExpressionInfo: enemyObjectExpressionInfo,
					playerXMillis: playerXMillis,
					playerYMillis: playerYMillis,
					elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
					rng: rng);

				if (randomExclusiveBound <= 0)
					throw new Exception();

				long randomByte1 = rng.NextInt(128);
				long randomByte2 = rng.NextInt(256);
				long randomByte3 = rng.NextInt(256);
				long randomByte4 = rng.NextInt(256);
				long randomByte5 = rng.NextInt(256);
				long randomByte6 = rng.NextInt(256);
				long randomByte7 = rng.NextInt(256);
				long randomByte8 = rng.NextInt(256);
				long randomLong =
					(randomByte1 << 56)
					| (randomByte2 << 48)
					| (randomByte3 << 40)
					| (randomByte4 << 32)
					| (randomByte5 << 24)
					| (randomByte6 << 16)
					| (randomByte7 << 8)
					| (randomByte8);

				if (randomLong < 0)
				{
					randomLong = unchecked(-randomLong);
				}

				if (randomLong < 0)
					randomLong = 0;

				if (randomExclusiveBound == 1)
					return 0;

				// Note that this isn't perfectly uniform for large values of randomExclusiveBound
				// but it'll decent enough for now.
				long randomVal = randomLong % randomExclusiveBound;

				return randomVal;
			}
		}

		private class AbsoluteValueExpression : IMathExpression
		{
			private IMathExpression operand;

			public AbsoluteValueExpression(IMathExpression operand)
			{
				this.operand = operand;
			}

			public long Evaluate(
				EnemyObjectExpressionInfo enemyObjectExpressionInfo, // nullable
				long playerXMillis,
				long playerYMillis,
				long elapsedMillisecondsPerIteration,
				IDTDeterministicRandom rng)
			{
				long value = this.operand.Evaluate(
					enemyObjectExpressionInfo: enemyObjectExpressionInfo,
					playerXMillis: playerXMillis,
					playerYMillis: playerYMillis,
					elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration,
					rng: rng);

				if (value == long.MinValue)
					return long.MinValue;

				if (value < 0)
					return -value;

				return value;
			}
		}

		public static IMathExpression Constant(long constantValue)
		{
			return new ConstantExpression(value: constantValue);
		}

		public static IMathExpression Variable(string variableName)
		{
			return new VariableExpression(variableName: variableName);
		}

		public static IMathExpression XMillis()
		{
			return new XMillisExpression();
		}

		public static IMathExpression YMillis()
		{
			return new YMillisExpression();
		}

		public static IMathExpression MilliHP()
		{
			return new MilliHPExpression();
		}

		public static IMathExpression ParentVariable(string variableName)
		{
			return new ParentVariableExpression(variableName: variableName);
		}

		public static IMathExpression ParentXMillis()
		{
			return new ParentXMillisExpression();
		}

		public static IMathExpression ParentYMillis()
		{
			return new ParentYMillisExpression();
		}

		public static IMathExpression PlayerXMillis()
		{
			return new PlayerXMillisExpression();
		}

		public static IMathExpression PlayerYMillis()
		{
			return new PlayerYMillisExpression();
		}

		public static IMathExpression ElapsedMillisecondsPerIteration()
		{
			return new ElapsedMillisecondsPerIterationExpression();
		}

		public static IMathExpression Add(IMathExpression leftSide, IMathExpression rightSide)
		{
			return new AddExpression(leftSide, rightSide);
		}

		public static IMathExpression Add(IMathExpression leftSide, long rightSide)
		{
			return MathExpression.Add(leftSide, MathExpression.Constant(rightSide));
		}

		public static IMathExpression Add(long leftSide, IMathExpression rightSide)
		{
			return MathExpression.Add(MathExpression.Constant(leftSide), rightSide);
		}

		public static IMathExpression Subtract(IMathExpression leftSide, IMathExpression rightSide)
		{
			return new SubtractExpression(leftSide, rightSide);
		}

		public static IMathExpression Multiply(IMathExpression leftSide, IMathExpression rightSide)
		{
			return new MultiplyExpression(leftSide, rightSide);
		}

		public static IMathExpression Multiply(IMathExpression x1, IMathExpression x2, IMathExpression x3)
		{
			return new MultiplyExpression(new MultiplyExpression(x1, x2), x3);
		}

		public static IMathExpression Divide(IMathExpression leftSide, IMathExpression rightSide)
		{
			return new DivideExpression(leftSide, rightSide);
		}

		public static IMathExpression Min(IMathExpression leftSide, IMathExpression rightSide)
		{
			return new MinExpression(leftSide, rightSide);
		}

		public static IMathExpression Min(long leftSide, IMathExpression rightSide)
		{
			return MathExpression.Min(MathExpression.Constant(leftSide), rightSide);
		}

		public static IMathExpression Min(IMathExpression leftSide, long rightSide)
		{
			return MathExpression.Min(leftSide, MathExpression.Constant(rightSide));
		}

		public static IMathExpression Max(IMathExpression leftSide, IMathExpression rightSide)
		{
			return new MaxExpression(leftSide, rightSide);
		}

		public static IMathExpression SineScaled(IMathExpression operand)
		{
			return new SineScaledExpression(operand);
		}

		public static IMathExpression CosineScaled(IMathExpression operand)
		{
			return new CosineScaledExpression(operand);
		}

		public static IMathExpression ArcTangentScaled(IMathExpression x, IMathExpression y, bool changeUndefinedOutputToZero)
		{
			return new ArcTangentScaledExpression(operand1: x, operand2: y, changeUndefinedOutputToZero: changeUndefinedOutputToZero);
		}

		public static IMathExpression RandomInteger(IMathExpression operand)
		{
			return new RandomIntegerExpression(operand);
		}

		public static IMathExpression RandomInteger(long value)
		{
			return MathExpression.RandomInteger(MathExpression.Constant(value));
		}

		public static IMathExpression AbsoluteValue(IMathExpression operand)
		{
			return new AbsoluteValueExpression(operand);
		}
	}
}
