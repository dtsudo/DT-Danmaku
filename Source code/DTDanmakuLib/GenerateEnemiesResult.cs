

namespace DTDanmakuLib
{
	using System.Collections.Generic;

	public class GenerateEnemiesResult
	{
		public List<EnemyObject> enemyObjects;
		public Dictionary<string, DTDanmakuImage> spriteNameToImageDictionary;
		public Dictionary<string, EnemyObjectTemplate> enemyObjectTemplates;
		public Dictionary<string, DTDanmakuSound> soundNameToSoundDictionary;

		public GenerateEnemiesResult(
			List<EnemyObject> enemyObjects,
			Dictionary<string, DTDanmakuImage> spriteNameToImageDictionary,
			Dictionary<string, EnemyObjectTemplate> enemyObjectTemplates,
			Dictionary<string, DTDanmakuSound> soundNameToSoundDictionary)
		{
			this.enemyObjects = enemyObjects;
			this.spriteNameToImageDictionary = spriteNameToImageDictionary;
			this.enemyObjectTemplates = enemyObjectTemplates;
			this.soundNameToSoundDictionary = soundNameToSoundDictionary;
		}
	}
}
