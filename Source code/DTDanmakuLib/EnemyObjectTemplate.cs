
namespace DTDanmakuLib
{
	using DTLib;
	using System.Collections.Generic;

	public class EnemyObjectTemplate
	{
		public EnemyObjectType ObjectType { get; private set; }

		public ObjectAction Action { get; private set; }

		public IMathExpression InitialMilliHP { get; private set; } // nullable (based on parent object)

		public DTImmutableList<ObjectBox> DamageBoxes { get; private set; } // nullable
		public DTImmutableList<ObjectBox> CollisionBoxes { get; private set; } // nullable

		public string SpriteName { get; private set; } // nullable

		private EnemyObjectTemplate() { }

		public static EnemyObjectTemplate Enemy(ObjectAction action, IMathExpression initialMilliHP, List<ObjectBox> damageBoxes, List<ObjectBox> collisionBoxes, string spriteName)
		{
			EnemyObjectTemplate template = new EnemyObjectTemplate();
			template.ObjectType = EnemyObjectType.Enemy;
			template.Action = action;
			template.InitialMilliHP = initialMilliHP;
			if (damageBoxes != null)
				template.DamageBoxes = new DTImmutableList<ObjectBox>(list: damageBoxes);
			if (collisionBoxes != null)
				template.CollisionBoxes = new DTImmutableList<ObjectBox>(list: collisionBoxes);
			template.SpriteName = spriteName;
			return template;
		}

		public static EnemyObjectTemplate EnemyBullet(ObjectAction action, IMathExpression initialMilliHP /* nullable */, List<ObjectBox> damageBoxes, List<ObjectBox> collisionBoxes, string spriteName)
		{
			EnemyObjectTemplate template = new EnemyObjectTemplate();
			template.ObjectType = EnemyObjectType.EnemyBullet;
			template.Action = action;
			if (initialMilliHP != null)
				template.InitialMilliHP = initialMilliHP;
			if (damageBoxes != null)
				template.DamageBoxes = new DTImmutableList<ObjectBox>(list: damageBoxes);
			if (collisionBoxes != null)
				template.CollisionBoxes = new DTImmutableList<ObjectBox>(list: collisionBoxes);
			template.SpriteName = spriteName;
			return template;
		}

		public static EnemyObjectTemplate Placeholder(ObjectAction action)
		{
			EnemyObjectTemplate template = new EnemyObjectTemplate();
			template.ObjectType = EnemyObjectType.Placeholder;
			template.Action = action;
			return template;
		}
	}
}
