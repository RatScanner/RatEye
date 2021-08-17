using System.Drawing;
using System.IO;
using RatEye;
using Xunit;
using Inventory = RatEye.Processing.Inventory;

namespace RatEyeTest
{
	[Collection("IconTest")]
	public class StaticIconTest : TestEnvironment
	{
		public override void Setup()
		{
			Config.GlobalConfig.PathConfig.StaticIcons = "Data/StaticIcons";
			Config.GlobalConfig.PathConfig.StaticCorrelationData = "Data/StaticIcons/correlation.json";
			Config.GlobalConfig.ProcessingConfig.IconConfig.UseStaticIcons = true;
			Config.GlobalConfig.Apply();
			base.Setup();
		}

		[Fact]
		public void ItemFHD()
		{
			var image = new Bitmap("TestData/FHD_Inventory2.png");
			var inventory = new Inventory(image);
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
			var image = new Bitmap("TestData/FHD_Inventory2.png");
			var inventory = new Inventory(image);
			var icon = inventory.LocateIcon(new Vector2(1080, 580));
			Assert.Equal("5448fee04bdc2dbc018b4567", icon.Item.Id);
			var expectedPath = Path.GetFullPath("Data/StaticIcons/item_water_bottle_loot.png");
			Assert.Equal(expectedPath, Path.GetFullPath(icon.IconPath));
			Assert.True(icon.Rotated);
		}
	}
}
