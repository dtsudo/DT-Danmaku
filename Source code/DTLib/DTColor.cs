
namespace DTLib
{
	using System;

	/// <summary>
	/// Represents a color, containing the standard r, g, b, and alpha values.
	/// </summary>
	public class DTColor
	{
		private int r;
		private int g;
		private int b;
		private int alpha;

        // r, g, b should all be within [0, 255]
		public DTColor(int r, int g, int b)
		{
			if (r < 0 || r > 255)
				throw new Exception();

			if (g < 0 || g > 255)
				throw new Exception();

			if (b < 0 || b > 255)
				throw new Exception();

			this.r = r;
			this.g = g;
			this.b = b;
			this.alpha = 255;
		}

        // r, g, b, alpha should all be within [0, 255]
		public DTColor(int r, int g, int b, int alpha)
		{
			if (r < 0 || r > 255)
				throw new Exception();

			if (g < 0 || g > 255)
				throw new Exception();

			if (b < 0 || b > 255)
				throw new Exception();

			if (alpha < 0 || alpha > 255)
				throw new Exception();

			this.r = r;
			this.g = g;
			this.b = b;
			this.alpha = alpha;
		}

		public int R { get { return this.r; } }
		public int G { get { return this.g; } }
		public int B { get { return this.b; } }
		public int Alpha { get { return this.alpha; } }
	}
}
