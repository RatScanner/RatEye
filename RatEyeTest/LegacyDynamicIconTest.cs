using System.Drawing;
using System.IO;
using RatEye;
using Xunit;
using Color = System.Drawing.Color;
using Inventory = RatEye.Processing.Inventory;

namespace RatEyeTest
{
	[Collection("SerialTest")]
	public class LegacyDynamicIconTest : TestEnvironment
	{
		protected override void Setup()
		{
			Config.GlobalConfig.PathConfig.DynamicIcons = "Data/DynamicIcons";
			Config.GlobalConfig.PathConfig.DynamicCorrelationData = "Data/DynamicIcons/index.json";
			Config.GlobalConfig.ProcessingConfig.IconConfig.UseDynamicIcons = true;

			Config.GlobalConfig.ProcessingConfig.IconConfig.UseLegacyCacheIndex = true;

			Config.GlobalConfig.Apply();
			base.Setup();
		}

		[Fact]
		public void ItemFHD()
		{
			var image = new Bitmap("TestData/FHD_Inventory2.png");
			var inventory = new Inventory(image);
			var icon = inventory.LocateIcon(new Vector2(960, 525));
			Assert.Equal("OKP-7 reflex sight", icon.Item.Name);
			Assert.Equal("57486e672459770abd687134", icon.Item.Id);
			var expectedPath = Path.GetFullPath("Data/DynamicIcons/24.png");
			Assert.Equal(expectedPath, Path.GetFullPath(icon.IconPath));
			Assert.False(icon.Rotated);
		}

		[Fact]
		public void ItemFHDHighlighted()
		{
			var image = new Bitmap("TestData/FHD_InventoryHighlighted2.png");
			var inventory = new Inventory(image,
				new Config()
				{
					ProcessingConfig = new Config.Processing()
					{
						InventoryConfig = new Config.Processing.Inventory()
						{
							MaxGridColor = Color.FromArgb(89, 89, 89),
							OptimizeHighlighted = true,
						}
					}
				}.Apply());
			var icon = inventory.LocateIcon();
			Assert.Equal("MP-133 12ga shotgun", icon.Item.Name);
			Assert.Equal("54491c4f4bdc2db1078b4568", icon.Item.Id);
			var expectedPath = Path.GetFullPath("Data/DynamicIcons/121.png");
			Assert.Equal(expectedPath, Path.GetFullPath(icon.IconPath));
			Assert.True(icon.Rotated);
		}
	}
}
