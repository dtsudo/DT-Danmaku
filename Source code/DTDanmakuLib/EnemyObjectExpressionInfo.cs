
namespace DTDanmakuLib
{
	using System.Collections.Generic;

	public class EnemyObjectExpressionInfo
	{
		public Dictionary<string, long> NumericVariables;
		public Dictionary<string, bool> BooleanVariables;
		public long XMillis;
		public long YMillis;
		public long? MilliHP;
		public EnemyObjectExpressionInfo Parent; // nullable

		public EnemyObjectExpressionInfo(
			Dictionary<string, long> numericVariables,
			Dictionary<string, bool> booleanVariables,
			long xMillis,
			long yMillis,
			long? milliHP,
			EnemyObjectExpressionInfo parent)
		{
			this.NumericVariables = numericVariables;
			this.BooleanVariables = booleanVariables;
			this.XMillis = xMillis;
			this.YMillis = yMillis;
			this.MilliHP = milliHP;
			this.Parent = parent;
		}
	}
}
