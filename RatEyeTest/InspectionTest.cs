using System.Diagnostics;
using System.Drawing;
using RatEye;
using RatEye.Processing;
using RatStash;
using Xunit;

namespace RatEyeTest
{
	public class InspectionTest : TestEnvironment
	{
		[Fact]
		public void ItemFHD()
		{
			var image = new Bitmap("TestData/FHD_Item.png");
			var inspType = Inspection.InspectionType.Item;
			var title = "GSSh-01 active headset";
			ConductTest(1f, image, inspType, 17, 13, title, "5b432b965acfc47a8774094e");
		}

		[Fact]
		public void ItemFHDRussian()
		{
			var image = new Bitmap("TestData/FHD_Item_Russian.png");
			var inspType = Inspection.InspectionType.Item;
			var title = "Дульный тормоз-компенсатор Зенит \"ДТК-1\" 7.62x39 и 5.45x39 для АК";
			ConductTest(1f, image, inspType, 14, 12, title, "5649ab884bdc2ded0b8b457f", Language.Russian);
		}


		[Fact]
		public void ItemUHD()
		{
			var image = new Bitmap("TestData/UHD_Item.png");
			var inspType = Inspection.InspectionType.Item;
			var title = "TerraGroup Labs access keycard";
			ConductTest(2f, image, inspType, 79, 50, title, "5c94bbff86f7747ee735c08f");
		}

		[Fact]
		public void ContainerFHD()
		{
			var image = new Bitmap("TestData/FHD_Container.png");
			var inspType = Inspection.InspectionType.Container;
			var title = "Mechanism";
			ConductTest(1f, image, inspType, 23, 25, title, "5d5d940f86f7742797262046");
		}

		[Fact]
		public void ContainerUHD()
		{
			var image = new Bitmap("TestData/UHD_Container.png");
			var inspType = Inspection.InspectionType.Container;
			var title = "Holodilnick";
			ConductTest(2f, image, inspType, 51, 38, title, "5c093db286f7740a1b2617e3");
		}

		private static void ConductTest(
			float scale,
			Bitmap image,
			Inspection.InspectionType inspectionType,
			int posX,
			int posY,
			string title,
			string id,
			Language language = Language.English)
		{
			var fastRatEye = GetRatEyeEngine(scale, language, "fast");
			var bestRatEye = GetRatEyeEngine(scale, language, "best");

			ConductTestSub(fastRatEye, image, inspectionType, posX, posY, title, id, 0.7f);
			ConductTestSub(bestRatEye, image, inspectionType, posX, posY, title, id, 0.9f);
		}

		private static void ConductTestSub(
			RatEyeEngine ratEye,
			Bitmap image,
			Inspection.InspectionType
			inspectionType,
			int posX,
			int posY,
			string title,
			string id,
			float minTitleDistance)
		{
			var inspection = ratEye.NewInspection(image);
			Assert.True(inspection.ContainsMarker);
			Assert.InRange(inspection.MarkerConfidence, 0.99f, 1.0f);
			Assert.Equal(inspectionType, inspection.DetectedInspectionType);
			Assert.Equal(posX, inspection.MarkerPosition.X);
			Assert.Equal(posY, inspection.MarkerPosition.Y);
			Assert.InRange(inspection.Title.NormedLevenshteinDistance(title), minTitleDistance, 1f);
			Debug.WriteLine("ZASDA " + inspection.Title + ": " + inspection.Title.NormedLevenshteinDistance(title));
			Assert.Equal(id, inspection.Item.Id);
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
						MarkerThreshold = 0.99f,
						EnableContainers = true,
					}
				}
			};
			return new RatEyeEngine(config);
		}
	}
}
