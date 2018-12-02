
namespace DTDanmakuLib
{
	using System.Collections.Generic;

	public class EnemyObjectRenderer
	{
		public static void Render(
			List<EnemyObject> enemyObjects,
			EnemyObjectType enemyObjectType,
			TranslatedDTDanmakuDisplay display,
			Dictionary<string, DTDanmakuImage> spriteNameToImageDictionary)
		{
			foreach (EnemyObject enemyObj in enemyObjects)
			{
				if (enemyObj.IsDestroyed)
					continue;

				if (enemyObj.ObjectType != enemyObjectType)
					continue;

				if (enemyObj.SpriteName == null)
					continue;

				DTDanmakuImage image = spriteNameToImageDictionary[enemyObj.SpriteName];

				long width = display.GetWidth(image);
				long height = display.GetHeight(image);

				int x = (int)(enemyObj.XMillis / 1000 - width / 2);
				int y = (int)(700 - enemyObj.YMillis / 1000 - height / 2);
				display.DrawImageRotatedClockwise(image, x, y, (int)enemyObj.FacingDirectionInMillidegrees);
			}
		}
	}
}
