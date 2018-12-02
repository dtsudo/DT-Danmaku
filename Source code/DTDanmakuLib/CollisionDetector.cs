
namespace DTDanmakuLib
{
	using DTLib;
	using System.Collections.Generic;

	public class CollisionDetector
	{
		public static List<PlayerBullet> HandleCollisionBetweenPlayerBulletsAndEnemyObjects(
			List<PlayerBullet> playerBullets,
			List<EnemyObject> enemyObjects)
		{
			List<PlayerBullet> newPlayerBullets = new List<PlayerBullet>();

			foreach (PlayerBullet playerBullet in playerBullets)
			{
				bool hasCollidedWithAnyEnemy = false;

				foreach (EnemyObject enemyObject in enemyObjects)
				{
					if (enemyObject.IsDestroyed)
						continue;

					bool hasCollided = HasCollided(
						object1XMillis: playerBullet.xMillis,
						object1YMillis: playerBullet.yMillis,
						object1Boxes: playerBullet.CollisionBoxes,
						object2XMillis: enemyObject.XMillis,
						object2YMillis: enemyObject.YMillis,
						object2Boxes: enemyObject.DamageBoxes);

					if (hasCollided)
					{
						enemyObject.MilliHP = enemyObject.MilliHP.Value - EnemyObject.DAMAGE_TAKEN_IN_MILLIHP_WHEN_HIT_BY_PLAYER_BULLET;
						hasCollidedWithAnyEnemy = true;
						break;
					}
				}

				if (!hasCollidedWithAnyEnemy)
					newPlayerBullets.Add(playerBullet);
			}

			return newPlayerBullets;
		}

		// Returns whether the player collided with any enemy objects
		public static bool HandleCollisionBetweenPlayerAndEnemyObjects(
			List<Player.PlayerSubFramePosition> playerSubFramePositions,
			bool isPlayerDead,
			bool isPlayerInvulnerable,
			List<EnemyObject> enemyObjects)
		{
			if (isPlayerDead)
				return false;

			if (isPlayerInvulnerable)
				return false;

			List<ObjectBox> playerCollisionBoxesList = new List<ObjectBox>();
			playerCollisionBoxesList.Add(new ObjectBox(lowerXMillis: -1 * 1000, upperXMillis: 1 * 1000, lowerYMillis: -1 * 1000, upperYMillis: 1 * 1000));
 			DTImmutableList<ObjectBox> playerCollisionBoxes = new DTImmutableList<ObjectBox>(playerCollisionBoxesList);

			foreach (EnemyObject enemyObj in enemyObjects)
			{
				if (enemyObj.IsDestroyed)
					continue;

				foreach (Player.PlayerSubFramePosition playerSubFramePosition in playerSubFramePositions)
				{
					bool hasCollided = HasCollided(
						object1XMillis: playerSubFramePosition.XMillis,
						object1YMillis: playerSubFramePosition.YMillis,
						object1Boxes: playerCollisionBoxes,
						object2XMillis: enemyObj.XMillis,
						object2YMillis: enemyObj.YMillis,
						object2Boxes: enemyObj.CollisionBoxes);

					if (hasCollided)
					{
						enemyObj.MilliHP = enemyObj.MilliHP - EnemyObject.DAMAGE_TAKEN_IN_MILLIHP_WHEN_COLLIDE_WITH_PLAYER;
						return true;
					}
				}
			}

			return false;
		}

		public static bool HasCollided(
			long object1XMillis,
			long object1YMillis,
			DTImmutableList<ObjectBox> object1Boxes, /* nullable */
			long object2XMillis,
			long object2YMillis,
			DTImmutableList<ObjectBox> object2Boxes /* nullable */)
		{
			if (object1Boxes == null || object2Boxes == null)
				return false;
			if (object1Boxes.Count == 0 || object2Boxes.Count == 0)
				return false;

			for (int i = 0; i < object1Boxes.Count; i++)
			{
				ObjectBox obj1Box = object1Boxes[i];
				for (int j = 0; j < object2Boxes.Count; j++)
				{
					ObjectBox obj2Box = object2Boxes[j];

					long obj1Left = object1XMillis + obj1Box.LowerXMillis;
					long obj1Right = object1XMillis + obj1Box.UpperXMillis;

					long obj2Left = object2XMillis + obj2Box.LowerXMillis;
					long obj2Right = object2XMillis + obj2Box.UpperXMillis;

					bool noXCollision = obj1Right < obj2Left || obj2Right < obj1Left;

					if (!noXCollision)
					{
						long obj1Bottom = object1YMillis + obj1Box.LowerYMillis;
						long obj1Top = object1YMillis + obj1Box.UpperYMillis;

						long obj2Bottom = object2YMillis + obj2Box.LowerYMillis;
						long obj2Top = object2YMillis + obj2Box.UpperYMillis;

						bool noYCollision = obj1Top < obj2Bottom || obj2Top < obj1Bottom;

						if (!noYCollision)
							return true;
					}
				}
			}

			return false;
		}
	}
}
