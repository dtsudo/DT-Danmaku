
namespace DTDanmakuLib
{
	using DTLib;

	public class DTDanmaku
	{
		public static IFrame<IDTDanmakuAssets> GetFirstFrame(int fps, IDTDeterministicRandom rng, GuidGenerator guidGenerator, bool debugMode)
		{
			var frame = new InitialLoadingScreenFrame(fps: fps, rng: rng, guidGenerator: guidGenerator, debugMode: debugMode);
			return frame;
		}
	}
}
