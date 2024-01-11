using System.Drawing;
using System.IO;
using RatEye;
using Xunit;

namespace RatEyeTest
{
	public class StaticIconTest : TestEnvironment
	{
		[Fact]
		public void ItemFHD()
		{
			var image = new Bitmap("TestData/FHD/Inventory2.png");
			var inventory = GetRatEyeEngine().NewInventory(image);
			var icon = inventory.LocateIcon(new Vector2(735, 310));
			Assert.Equal("Peltor ComTac 2 headset", icon.Item.Name);
			Assert.Equal("5645bcc04bdc2d363b8b4572", icon.Item.Id);
			var expectedPath = Path.GetFullPath("Data/Icons/5645bcc04bdc2d363b8b4572.png");
			Assert.Equal(expectedPath, Path.GetFullPath(icon.IconPath));
			Assert.False(icon.Rotated);
		}

		[Fact]
		public void ItemFHDOCR()
		{
			var image = new Bitmap("TestData/FHD/Inventory2.png");
			var engine = GetRatEyeEngine();
			engine.Config.ProcessingConfig.IconConfig.ScanMode = Config.Processing.Icon.ScanModes.OCR;

			var inventory = engine.NewInventory(image);
			var icon = inventory.LocateIcon(new Vector2(735, 310));
			Assert.Equal("Peltor ComTac 2 headset", icon.Item.Name);
			Assert.Equal("5645bcc04bdc2d363b8b4572", icon.Item.Id);
			var expectedPath = Path.GetFullPath("Data/Icons/5645bcc04bdc2d363b8b4572.png");
			Assert.Equal(expectedPath, Path.GetFullPath(icon.IconPath));
			Assert.False(icon.Rotated);
		}

		[Fact]
		public void ItemFHDRotated()
		{
			var image = new Bitmap("TestData/FHD/Inventory2.png");
			var inventory = GetRatEyeEngine().NewInventory(image);
			var icon = inventory.LocateIcon(new Vector2(1080, 580));
			Assert.Equal("5448fee04bdc2dbc018b4567", icon.Item.Id);
			var expectedPath = Path.GetFullPath("Data/Icons/5448fee04bdc2dbc018b4567.png");
			Assert.Equal(expectedPath, Path.GetFullPath(icon.IconPath));
			Assert.True(icon.Rotated);
		}

		[Fact]
		public void ItemQHD()
		{
			var scale = Config.Processing.Resolution2Scale(2560, 1440);
			var ratEye = GetRatEyeEngine(scale);

			var image = new Bitmap("TestData/QHD/Container.png");
			var inventory = ratEye.NewInventory(image);
			var icon = inventory.LocateIcon(new Vector2(640, 840));
			Assert.Equal(new Vector2(9, 8), icon.ItemPosition);
			Assert.Equal("Wrench", icon.Item.Name);
			Assert.Equal("590c311186f77424d1667482", icon.Item.Id);
			var expectedPath = Path.GetFullPath("Data/Icons/590c311186f77424d1667482.png");
			Assert.Equal(expectedPath, Path.GetFullPath(icon.IconPath));
			Assert.False(icon.Rotated);
		}

		[Fact]
		public void ItemQHDOCR()
		{
			var image = new Bitmap("TestData/QHD/Container.png");
			var scale = Config.Processing.Resolution2Scale(2560, 1440);
			var engine = GetRatEyeEngine(scale);
			engine.Config.ProcessingConfig.IconConfig.ScanMode = Config.Processing.Icon.ScanModes.OCR;

			var inventory = engine.NewInventory(image);
			var icon = inventory.LocateIcon(new Vector2(735, 330));
			Assert.Equal("Bolts", icon.Item.Name);
			Assert.Equal("57347c5b245977448d35f6e1", icon.Item.Id);
			Assert.False(icon.Rotated);
		}

		[Fact]
		public void GridTest()
		{
			var scale = Config.Processing.Resolution2Scale(1920, 1080);
			var ratEye = GetRatEyeEngine(scale);

			var image = new Bitmap("TestData/FHD/InventoryHighlighted3H.png");
			var inventory = ratEye.NewInventory(image);
			var icon = inventory.LocateIcon();
			Assert.NotNull(icon.Item);
		}

		[Fact]
		public void FilteredDatabaseTest()
		{
			var itemDatabase = GetItemDatabase();
			itemDatabase = itemDatabase.Filter(item => item.Id != "5645bcc04bdc2d363b8b4572");

			var scale = Config.Processing.Resolution2Scale(1920, 1080);
			var ratEye = GetRatEyeEngine(scale, itemDatabase);

			var image = new Bitmap("TestData/FHD/Inventory2.png");
			var inventory = ratEye.NewInventory(image);
			var icon = inventory.LocateIcon(new Vector2(735, 310));
			Assert.NotEqual("5645bcc04bdc2d363b8b4572", icon.Item.Id);
		}

		private RatEyeEngine GetRatEyeEngine(float scale = 1f, RatStash.Database itemDatabase = null)
		{
			var config = new Config()
			{
				PathConfig = new Config.Path()
				{
					StaticIcons = "Data/Icons",
					StaticCorrelationData = "Data/Icons/correlation.json",
					TrainedData = "Data/traineddata/best",
				},
				ProcessingConfig = new Config.Processing()
				{
					Scale = scale,
					IconConfig = new Config.Processing.Icon()
					{
						UseStaticIcons = true,
						UseDynamicIcons = false,
					},
				},
			};
			return new RatEyeEngine(config, itemDatabase ?? GetItemDatabase());
		}
	}
}
