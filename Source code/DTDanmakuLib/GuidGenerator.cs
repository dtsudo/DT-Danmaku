
namespace DTDanmakuLib
{
	using System.Globalization;

	public class GuidGenerator
	{
		private long currentValue1;
		private long currentValue2;
		private string guidString;

		public GuidGenerator(string guidString)
		{
			this.currentValue1 = 0;
			this.currentValue2 = 0;
			this.guidString = guidString;
		}

		public string NextGuid()
		{
			this.currentValue1 = this.currentValue1 + 1;

			if (this.currentValue1 == long.MaxValue)
			{
				this.currentValue1 = 0;
				this.currentValue2 = this.currentValue2 + 1;
			}

			string currentValue1AsString = this.currentValue1.ToString(CultureInfo.InvariantCulture);
			string currentValue2AsString = this.currentValue2.ToString(CultureInfo.InvariantCulture);
			return "guidGenerator: guidString=" + this.guidString + " value1=" + currentValue1AsString + " value2=" + currentValue2AsString;
		}
	}
}
