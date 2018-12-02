
namespace DTDanmakuLib
{
	public class ObjectBox
	{
		// relative to the object's center; i.e. object's center is (0,0)
		public long LowerXMillis { get; private set; }
		public long UpperXMillis { get; private set; }
		public long LowerYMillis { get; private set; }
		public long UpperYMillis { get; private set; }

		public ObjectBox(long lowerXMillis, long upperXMillis, long lowerYMillis, long upperYMillis)
		{
			this.LowerXMillis = lowerXMillis;
			this.UpperXMillis = upperXMillis;
			this.LowerYMillis = lowerYMillis;
			this.UpperYMillis = upperYMillis;
		}
	}
}
