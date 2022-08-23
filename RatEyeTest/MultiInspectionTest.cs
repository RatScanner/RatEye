using System.Drawing;
using RatEye;
using RatStash;
using Xunit;

namespace RatEyeTest
{
	public class MultiInspectionTest : TestEnvironment
	{
		[Fact]
		public void MultiInspectionsFHD()
		{
			var image = new Bitmap("TestData/FHD/MultiInspection.png");
			var ratEye = GetRatEyeEngine(1.0f, Language.English, "fast");
			var multiInspection = ratEye.NewMultiInspection(image);
			Assert.Equal(4, multiInspection.Inspections.Count);
			foreach (var inspection in multiInspection.Inspections)
			{
				Assert.True(inspection.ContainsMarker);
				Assert.NotNull(inspection.Item);
				Assert.NotNull(inspection.MarkerPosition);
			}
		}

		[Fact]
		public void EmptyMultiInspectionsFHD()
		{
			var image = new Bitmap("TestData/FHD/InventoryH.png");
			var ratEye = GetRatEyeEngine(1.0f, Language.English, "fast");
			var multiInspection = ratEye.NewMultiInspection(image);
			Assert.Empty(multiInspection.Inspections);
		}

		private static RatEyeEngine GetRatEyeEngine(float scale, Language language, string modelType)
		{
			var config = new Config()
			{
				PathConfig = new Config.Path()
				{
					TrainedData = $"Data/traineddata/{modelType}",
				},
				ProcessingConfig = new Config.Processing()
				{
					Scale = scale,
					Language = language,
					InspectionConfig = new Config.Processing.Inspection()
					{
						MarkerThreshold = 0.9f,
					}
				}
			};
			return new RatEyeEngine(config, GetItemDatabase());
		}
	}
}
