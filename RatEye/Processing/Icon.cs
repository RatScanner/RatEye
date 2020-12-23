using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenCvSharp;

namespace RatEye.Processing
{
	public class Icon
	{
		private Mat _scaledMat;
		private System.Drawing.Size _locatedIconSlotSize;

		private readonly object _matchLock = new object();

		/// <summary>
		/// Do template matching over the last located icon <see cref="LocatedIcon"/>
		/// </summary>
		/// <param name="useDynamicIcons">Also use dynamic icons for template matching</param>
		/// <returns>icon key, confidence and position of the matched icon</returns>
		public (string iconKey, float conf, Vector2 pos) MatchIcon(bool useDynamicIcons)
		{
			var source = _scaledMat.Clone();

			(string iconKey, float conf, Vector2 pos) matchResult = (null, 0f, Vector2.Zero());

			// Load icons
			var iconTemplates = new Dictionary<string, Mat>();
			// TODO set LocatedIconSlotSize
			var staticIcons = IconManager.GetStaticIcons(_locatedIconSlotSize);
			var dynamicIcons = IconManager.GetDynamicIcons(_locatedIconSlotSize);

			staticIcons.ToList().ForEach(x => iconTemplates[x.Key] = x.Value);
			if (useDynamicIcons) dynamicIcons.ToList().ForEach(x => iconTemplates[x.Key] = x.Value);

			// 
			var firstIcon = iconTemplates.First().Value;
			if (source.Width < firstIcon.Width || source.Height < firstIcon.Height)
			{
				var infoText = "\nsW: " + source.Width + " | sH: " + source.Height;
				infoText += "\ntW: " + firstIcon.Width + " | tH: " + firstIcon.Height;
				Logger.LogWarning("Source dimensions smaller than template dimensions!" + infoText);
				Logger.LogDebugMat(source, "source");
				Logger.LogDebugMat(firstIcon, "template");
				return matchResult;
			}

			// hm = highest match
			var hmConf = 0f;
			var hmKey = "";
			var hmPos = Vector2.Zero();

			Parallel.ForEach(iconTemplates, icon =>
			{
				// TODO Prepare masks when loading icons
				var mask = icon.Value.InRange(new Scalar(0, 0, 0, 128), new Scalar(255, 255, 255, 255));
				mask = mask.CvtColor(ColorConversionCodes.GRAY2BGR, 3);
				var iconNoAlpha = icon.Value.CvtColor(ColorConversionCodes.BGRA2BGR, 3);

				var matches = source.MatchTemplate(iconNoAlpha, TemplateMatchModes.CCorrNormed, mask);
				matches.MinMaxLoc(out _, out var maxVal, out _, out var maxLoc);

				lock (_matchLock)
				{
					if (maxVal > hmConf)
					{
						hmConf = (float)maxVal;
						hmKey = icon.Key;
						hmPos = new Vector2(maxLoc);
					}
				}
			});

			return (hmKey, hmConf, hmPos);
		}
	}
}
