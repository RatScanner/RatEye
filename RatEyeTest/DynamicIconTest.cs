using System.Drawing;
using System.IO;
using RatEye;
using Xunit;
using Color = System.Drawing.Color;

namespace RatEyeTest
{
	public class DynamicIconTest : TestEnvironment
	{
		[Fact]
		public void ItemFHD()
		{
			var config = new Config()
			{
				PathConfig = new Config.Path()
				{
					DynamicIcons = "Data/DynamicIconsH",
					DynamicCorrelationData = "Data/DynamicIconsH/index.json",
				},
				ProcessingConfig = new Config.Processing()
				{
					IconConfig = new Config.Processing.Icon()
					{
						UseStaticIcons = false,
						UseDynamicIcons = true,
					},
				},
			};

			var ratEye = new RatEyeEngine(config, GetItemDatabase());

			var image = new Bitmap("TestData/FHD/InventoryH.png");
			var inventory = ratEye.NewInventory(image);
			var icon = inventory.LocateIcon(new Vector2(1800, 600));
			Assert.Equal("5ea034eb5aad6446a939737b", icon.Item.Id);
			var expectedPath = Path.GetFullPath("Data/DynamicIconsH/51.png");
			Assert.Equal(expectedPath, Path.GetFullPath(icon.IconPath));
			Assert.False(icon.Rotated);
		}

		[Fact]
		public void ItemFHDHighlighted()
		{
			var config = new Config()
			{
				PathConfig = new Config.Path()
				{
					DynamicIcons = "Data/DynamicIconsH",
					DynamicCorrelationData = "Data/DynamicIconsH/index.json",
				},
				ProcessingConfig = new Config.Processing()
				{
					IconConfig = new Config.Processing.Icon()
					{
						UseStaticIcons = false,
						UseDynamicIcons = true,
					},
					InventoryConfig = new Config.Processing.Inventory()
					{
						OptimizeHighlighted = true,
					},
				},
			};
			var ratEye = new RatEyeEngine(config, GetItemDatabase());

			var image = new Bitmap("TestData/FHD/InventoryHighlighted2H.png");
			var inventory = ratEye.NewInventory(image);
			var icon = inventory.LocateIcon();
			Assert.Equal("5c0505e00db834001b735073", icon.Item.Id);
			var expectedPath = Path.GetFullPath("Data/DynamicIconsH/44.png");
			Assert.Equal(expectedPath, Path.GetFullPath(icon.IconPath));
			Assert.True(icon.Rotated);
		}
	}
}
