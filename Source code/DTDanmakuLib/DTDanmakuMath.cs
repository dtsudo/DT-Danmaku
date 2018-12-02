
namespace DTDanmakuLib
{
	using DTLib;
	using System;

	public class DTDanmakuMath
	{
		// 0 degrees is going straight up
		// 90 degrees is going right
		// 180 degrees is straight down
		// 270 degrees is going left
		// (Note function returns value in millidegrees)	
		public static long? GetMovementDirectionInMillidegrees(long currentX, long currentY, long desiredX, long desiredY)
		{
			long deltaX = desiredX - currentX;
			long deltaY = desiredY - currentY;

			if (deltaX == 0 && deltaY == 0)
				return null;

			long angleInMillidegrees = DTMath.ArcTangentScaled(x: deltaX, y: deltaY);
			long angle2 = -angleInMillidegrees + 90L * 1000L;
			return DTMath.NormalizeAngleInMillidegrees(angle2);
		}

		/*
			When (currentX, currentY) == (desiredX, desiredY), the resulting math expression is arbitrary 
			but is guaranteed not to error.
		*/
		public static IMathExpression GetMovementDirectionInMillidegrees(
			IMathExpression currentX,
			IMathExpression currentY,
			IMathExpression desiredX,
			IMathExpression desiredY)
		{
			IMathExpression deltaX = MathExpression.Subtract(desiredX, currentX);
			IMathExpression deltaY = MathExpression.Subtract(desiredY, currentY);

			IMathExpression angleInMillidegrees = MathExpression.ArcTangentScaled(x: deltaX, y: deltaY, changeUndefinedOutputToZero: true);
			IMathExpression angle2 = MathExpression.Add(
				MathExpression.Multiply(angleInMillidegrees, MathExpression.Constant(-1)),
				MathExpression.Constant(90 * 1000));

			return angle2;
		}

		public class Offset
		{
			public long DeltaXInMillipixels;
			public long DeltaYInMillipixels;
		}

		public static Offset GetOffset(
			long speedInMillipixelsPerMillisecond,
			long movementDirectionInMillidegrees,
			long elapsedMillisecondsPerIteration)
		{
			if (speedInMillipixelsPerMillisecond < 0)
				throw new Exception();

			if (speedInMillipixelsPerMillisecond == 0)
				return new Offset { DeltaXInMillipixels = 0, DeltaYInMillipixels = 0 };

			movementDirectionInMillidegrees = DTMath.NormalizeAngleInMillidegrees(movementDirectionInMillidegrees);

			long numberOfMillipixels = speedInMillipixelsPerMillisecond * elapsedMillisecondsPerIteration;
			
			return new Offset
			{
				DeltaXInMillipixels = numberOfMillipixels * DTMath.SineScaled(milliDegrees: movementDirectionInMillidegrees) / 1000L,
				DeltaYInMillipixels = numberOfMillipixels * DTMath.CosineScaled(milliDegrees: movementDirectionInMillidegrees) / 1000L 
			};
		}
		
		public class MathExpressionOffset
		{
			public IMathExpression DeltaXInMillipixels;
			public IMathExpression DeltaYInMillipixels;
		}

		public static MathExpressionOffset GetOffset(
			IMathExpression millipixels,
			IMathExpression movementDirectionInMillidegrees)
		{
			return new MathExpressionOffset
			{
				DeltaXInMillipixels = MathExpression.Divide(MathExpression.Multiply(millipixels, MathExpression.SineScaled(movementDirectionInMillidegrees)), MathExpression.Constant(1000L)),
				DeltaYInMillipixels = MathExpression.Divide(MathExpression.Multiply(millipixels, MathExpression.CosineScaled(movementDirectionInMillidegrees)), MathExpression.Constant(1000L))
			};
		}
	}
}
