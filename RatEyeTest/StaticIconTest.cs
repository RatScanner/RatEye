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
			var expectedPath = Path.GetFullPath("Data/StaticIcons/item_equipment_headset_comtacii.png");
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
			var expectedPath = Path.GetFullPath("Data/StaticIcons/item_water_bottle_loot.png");
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
			Assert.Equal("Wrench", icon.Item.Name);
			Assert.Equal("590c311186f77424d1667482", icon.Item.Id);
			var expectedPath = Path.GetFullPath("Data/StaticIcons/item_tools_wrench.png");
			Assert.Equal(expectedPath, Path.GetFullPath(icon.IconPath));
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
					StaticIcons = "Data/StaticIcons",
					StaticCorrelationData = "Data/StaticIcons/correlation.json",
				},
				ProcessingConfig = new Config.Processing()
				{
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
