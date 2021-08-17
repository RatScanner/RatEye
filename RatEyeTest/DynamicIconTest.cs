using System.Drawing;
using System.IO;
using RatEye;
using Xunit;
using Color = System.Drawing.Color;
using Inventory = RatEye.Processing.Inventory;

namespace RatEyeTest
{
	[Collection("SerialTest")]
	public class DynamicIconTest : TestEnvironment
	{
		protected override void Setup()
		{
			Config.GlobalConfig.PathConfig.DynamicIcons = "Data/DynamicIconsH";
			Config.GlobalConfig.PathConfig.DynamicCorrelationData = "Data/DynamicIconsH/index.json";
			Config.GlobalConfig.ProcessingConfig.IconConfig.UseDynamicIcons = true;
			Config.GlobalConfig.Apply();
			base.Setup();
		}

		[Fact]
		public void ItemFHD()
		{
			var image = new Bitmap("TestData/FHD_InventoryH.png");
			var inventory = new Inventory(image);
			var icon = inventory.LocateIcon(new Vector2(1800, 600));
			Assert.Equal("5ea034eb5aad6446a939737b", icon.Item.Id);
			var expectedPath = Path.GetFullPath("Data/DynamicIconsH/51.png");
			Assert.Equal(expectedPath, Path.GetFullPath(icon.IconPath));
			Assert.False(icon.Rotated);
		}

		[Fact]
		public void ItemFHDHighlighted()
		{
			var image = new Bitmap("TestData/FHD_InventoryHighlighted2H.png");
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
			Assert.Equal("5c0505e00db834001b735073", icon.Item.Id);
			var expectedPath = Path.GetFullPath("Data/DynamicIconsH/44.png");
			Assert.Equal(expectedPath, Path.GetFullPath(icon.IconPath));
			Assert.True(icon.Rotated);
		}
	}
}
