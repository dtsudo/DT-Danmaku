
namespace DTDanmakuLib
{
	using DTLib;
	using System;
	using System.Collections.Generic;
	
	public class BossHealthBar
	{
		private class DisplayedHealthBar : IComparable<DisplayedHealthBar>
		{
			public long MeterNumber;
			public long MilliPercentage;

			public int CompareTo(DisplayedHealthBar other)
			{
				if (this.MeterNumber < other.MeterNumber)
					return -1;
				if (this.MeterNumber > other.MeterNumber)
					return 1;
				return 0;
			}
		}

		private long? bossHealthMeterNumber;
		private long? bossHealthMeterMilliPercentage;
		
		private List<DisplayedHealthBar> displayedHealthBars;

		public BossHealthBar()
		{
			this.bossHealthMeterNumber = null;
			this.bossHealthMeterMilliPercentage = null;

			this.displayedHealthBars = new List<DisplayedHealthBar>();
		}

		public void ProcessBossHealthBar(
			long? bossHealthMeterNumber,
			long? bossHealthMeterMilliPercentage,
			long elapsedMillisPerFrame)
		{
			if (bossHealthMeterNumber == null && bossHealthMeterMilliPercentage != null)
				throw new Exception();
			if (bossHealthMeterNumber != null && bossHealthMeterMilliPercentage == null)
				throw new Exception();

			this.bossHealthMeterNumber = bossHealthMeterNumber;
			this.bossHealthMeterMilliPercentage = bossHealthMeterMilliPercentage;

			if (bossHealthMeterNumber == null)
			{
				this.displayedHealthBars = new List<DisplayedHealthBar>();
			}
			else
			{
				long numPercentPerSecond = 33;
				long numMilliPercentPerFrame = numPercentPerSecond * elapsedMillisPerFrame;

				List<DisplayedHealthBar> newList = new List<DisplayedHealthBar>();

				bool[] isAlreadyIncluded = new bool[bossHealthMeterNumber.Value];
				for (int i = 0; i < isAlreadyIncluded.Length; i++)
					isAlreadyIncluded[i] = false;

				foreach (DisplayedHealthBar existingHealthBar in this.displayedHealthBars)
				{
					if (existingHealthBar.MeterNumber - 1 < isAlreadyIncluded.Length)
						isAlreadyIncluded[existingHealthBar.MeterNumber - 1] = true;

					if (existingHealthBar.MeterNumber < bossHealthMeterNumber.Value)
						existingHealthBar.MilliPercentage = Math.Min(existingHealthBar.MilliPercentage + numMilliPercentPerFrame, 100 * 1000);
					else if (existingHealthBar.MeterNumber > bossHealthMeterNumber.Value)
						existingHealthBar.MilliPercentage = Math.Max(existingHealthBar.MilliPercentage - numMilliPercentPerFrame, 0);
					else
					{
						if (existingHealthBar.MilliPercentage < bossHealthMeterMilliPercentage.Value)
							existingHealthBar.MilliPercentage = Math.Min(existingHealthBar.MilliPercentage + numMilliPercentPerFrame, bossHealthMeterMilliPercentage.Value);
						else if (existingHealthBar.MilliPercentage > bossHealthMeterMilliPercentage.Value)
							existingHealthBar.MilliPercentage = Math.Max(existingHealthBar.MilliPercentage - numMilliPercentPerFrame, bossHealthMeterMilliPercentage.Value);
					}

					if (existingHealthBar.MeterNumber <= bossHealthMeterNumber.Value || existingHealthBar.MilliPercentage > 0)
						newList.Add(existingHealthBar);
				}

				for (int i = 0; i < isAlreadyIncluded.Length; i++)
				{
					if (!isAlreadyIncluded[i])
					{
						DisplayedHealthBar newHealthBar = new DisplayedHealthBar();
						newHealthBar.MeterNumber = i + 1;
						newHealthBar.MilliPercentage = 0;
						newList.Add(newHealthBar);
					}
				}
				
				this.displayedHealthBars = newList;
			}
		}
		
		public void RenderBossHealthBar(TranslatedDTDanmakuDisplay display)
		{
			List<DisplayedHealthBar> list = new List<DisplayedHealthBar>();
			foreach (DisplayedHealthBar healthBar in this.displayedHealthBars)
				list.Add(healthBar);

			list.Sort();

			for (int i = 0; i < list.Count; i++)
			{
				DisplayedHealthBar healthBar = list[i];

				long maxWidthInPixels = 800;

				long desiredWidth = maxWidthInPixels * healthBar.MilliPercentage / 100L / 1000L;

				int alpha;

				if (this.bossHealthMeterNumber == null)
					alpha = 0;
				else if (healthBar.MeterNumber >= this.bossHealthMeterNumber.Value)
					alpha = 255;
				else if (healthBar.MeterNumber == this.bossHealthMeterNumber.Value - 1)
					alpha = 40;
				else
					alpha = 0;

				display.DrawRectangle(
					x: 100,
					y: 40,
					width: (int)desiredWidth,
					height: 7,
					color: GetColor(bossHealthMeterNumber: healthBar.MeterNumber, alpha: alpha),
					fill: true);
			}
		}
		
		private static DTColor GetColor(long bossHealthMeterNumber, int alpha)
		{
			long barNum = (bossHealthMeterNumber - 1) % 3;

			if (barNum == 0)
				return new DTColor(r: 255, g: 67, b: 38, alpha: alpha);
			if (barNum == 1)
				return new DTColor(r: 255, g: 127, b: 39, alpha: alpha);
			
			return new DTColor(r: 255, g: 190, b: 38, alpha: alpha);
		}
	}
}
