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
			var image = new Bitmap("TestData/FHD/Item.png");
			var title = "GSSh-01 active headset";
			ConductTest(1f, image, 17, 13, title, "5b432b965acfc47a8774094e");
		}

		[Fact]
		public void ItemFHD2()
		{
			var image = new Bitmap("TestData/FHD/Inspection.png");
			var title = "Can of beef stew (Large)";
			ConductTest(1f, image, 25, 24, title, "57347da92459774491567cf5");
		}

		[Fact]
		public void ItemFHDRussian()
		{
			var image = new Bitmap("TestData/FHD/Item_Russian.png");
			var title = "Дульный тормоз-компенсатор Зенит \"ДТК-1\" 7.62x39 и 5.45x39 для АК";
			ConductTest(1f, image, 14, 12, title, "5649ab884bdc2ded0b8b457f", Language.Russian, 0.7f);
		}

		[Fact]
		public void ItemFHDRussianMixed()
		{
			var image = new Bitmap("TestData/FHD/Item_Russian_Mixed.png");
			var title = "Бронежилет PACA Soft Armor";
			ConductTest(1f, image, 16, 17, title, "5648a7494bdc2d9d488b4583", Language.Russian, 0.7f);
		}

		[Fact]
		public void ItemFHDChineseMixed()
		{
			var image = new Bitmap("TestData/FHD/Item_Chinese_Mixed.png");
			var title = "6B23-1护甲（数码丛林迷彩）";
			ConductTest(1f, image, 25, 17, title, "5c0e5bab86f77461f55ed1f3", Language.Chinese, 0.5f);
		}

		[Fact]
		public void ItemUHD()
		{
			var image = new Bitmap("TestData/UHD/Item.png");
			var title = "TerraGroup Labs access keycard";
			ConductTest(2f, image, 79, 50, title, "5c94bbff86f7747ee735c08f");
		}

		private static void ConductTest(
			float scale,
			Bitmap image,
			int posX,
			int posY,
			string title,
			string id,
			Language language = Language.English,
			float confidenceMul = 1f)
		{
			var bestRatEye = GetRatEyeEngine(scale, language, "best");
			ConductTestSub(bestRatEye, image, posX, posY, title, id, 0.9f * confidenceMul);

			var fastRatEye = GetRatEyeEngine(scale, language, "fast");
			ConductTestSub(fastRatEye, image, posX, posY, title, id, 0.7f * confidenceMul);
		}

		private static void ConductTestSub(
			RatEyeEngine ratEye,
			Bitmap image,
			int posX,
			int posY,
			string title,
			string id,
			float minTitleDistance)
		{
			var inspection = ratEye.NewInspection(image);
			Assert.True(inspection.ContainsMarker);
			Assert.InRange(inspection.MarkerConfidence, 0.99f, 1.0f);
			Assert.Equal(posX, inspection.MarkerPosition.X);
			Assert.Equal(posY, inspection.MarkerPosition.Y);
			Assert.InRange(inspection.Title.NormedLevenshteinDistance(title), minTitleDistance, 1f);
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
						MarkerThreshold = 0.9f,
					}
				}
			};
			return new RatEyeEngine(config, GetItemDatabase(language));
		}
	}
}
