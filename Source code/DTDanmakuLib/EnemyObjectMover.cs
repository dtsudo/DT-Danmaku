
namespace DTDanmakuLib
{
	using System.Collections.Generic;

	public class EnemyObjectMover
	{
		public static void UpdateEnemyPositions(
			List<EnemyObject> enemyObjects,
			long elapsedMillisecondsPerIteration)
		{
			foreach (EnemyObject enemyObject in enemyObjects)
			{
				if (enemyObject.IsDestroyed)
					continue;

				DTDanmakuMath.Offset offset = DTDanmakuMath.GetOffset(
					speedInMillipixelsPerMillisecond: enemyObject.SpeedInMillipixelsPerMillisecond,
					movementDirectionInMillidegrees: enemyObject.MovementDirectionInMillidegrees,
					elapsedMillisecondsPerIteration: elapsedMillisecondsPerIteration);

				enemyObject.XMillis += offset.DeltaXInMillipixels;
				enemyObject.YMillis += offset.DeltaYInMillipixels;
			}
		}
	}
}
