using System.Drawing;
using System.IO;
using RatEye;
using RatStash;
using Xunit;
using Inventory = RatEye.Processing.Inventory;

namespace RatEyeTest
{
	public class IconTest : TestEnvironment
	{
		public override void Setup()
		{
			Config.GlobalConfig.PathConfig.StaticIcons = "Data/StaticIcons";
			Config.GlobalConfig.PathConfig.DynamicIcons = "Data/DynamicIcons";

			Config.GlobalConfig.PathConfig.StaticCorrelationData = "Data/StaticIcons/correlation.json";
			Config.GlobalConfig.PathConfig.DynamicCorrelationData = "Data/DynamicIcons/index.json";

			Config.GlobalConfig.ProcessingConfig.IconConfig.UseStaticIcons = true;
			Config.GlobalConfig.ProcessingConfig.IconConfig.UseDynamicIcons = true;
			Config.GlobalConfig.Apply();
			base.Setup();
		}

		[Fact]
		public void ItemFHDStatic()
		{
			var image = new Bitmap("TestData/FHD_Inventory2.png");
			var inventory = new Inventory(image);
			var icon = inventory.LocateIcon(new Vector2(735, 310));
			Assert.Equal("Peltor ComTac 2 headset", icon.Item.Name);
			Assert.Equal("5645bcc04bdc2d363b8b4572", icon.Item.Id);
			var expectedPath = Path.GetFullPath("Data/StaticIcons/item_equipment_headset_comtacii.png");
			Assert.Equal(expectedPath, Path.GetFullPath(icon.Item.GetIconPath()));
			Assert.False(icon.Rotated);
		}

		[Fact]
		public void ItemFHDStaticRotated()
		{
			var image = new Bitmap("TestData/FHD_Inventory2.png");
			var inventory = new Inventory(image);
			var icon = inventory.LocateIcon(new Vector2(1080, 580));
			Assert.Equal("5448fee04bdc2dbc018b4567", icon.Item.Id);
			var expectedPath = Path.GetFullPath("Data/StaticIcons/item_water_bottle_loot.png");
			Assert.Equal(expectedPath, Path.GetFullPath(icon.Item.GetIconPath()));
			Assert.True(icon.Rotated);
		}

		[Fact]
		public void ItemFHDDynamic()
		{
			var image = new Bitmap("TestData/FHD_Inventory2.png");
			var inventory = new Inventory(image);
			var icon = inventory.LocateIcon(new Vector2(960, 525));
			Assert.Equal("OKP-7 reflex sight", icon.Item.Name);
			Assert.Equal("57486e672459770abd687134", icon.Item.Id);
			var expectedPath = Path.GetFullPath("Data/DynamicIcons/24.png");
			Assert.Equal(expectedPath, Path.GetFullPath(icon.Item.GetIconPath()));
			Assert.False(icon.Rotated);
		}

		[Fact]
		public void ItemFHDDynamicHighlighted()
		{
			var image = new Bitmap("TestData/FHD_InventoryHighlighted2.png");
			var inventory = new Inventory(image,
				new Config()
				{
					ProcessingConfig = new Config.Processing()
					{
						InventoryConfig = new Config.Processing.Inventory()
						{
							MaxGridColor = (89, 89, 89)
						}
					}
				});
			var icon = inventory.LocateIcon();
			Assert.Equal("MP-133 12ga shotgun", icon.Item.Name);
			Assert.Equal("54491c4f4bdc2db1078b4568", icon.Item.Id);
			var expectedPath = Path.GetFullPath("Data/DynamicIcons/121.png");
			Assert.Equal(expectedPath, Path.GetFullPath(icon.Item.GetIconPath(icon.ItemExtraInfo)));
			Assert.True(icon.Rotated);
		}

		[Fact]
		public void GetUnknownIconPath()
		{
			var path = new Item().GetIconPath();
			Assert.Null(path);
		}
	}
}
