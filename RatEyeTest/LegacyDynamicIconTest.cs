using System.Drawing;
using System.IO;
using RatEye;
using Xunit;
using Color = System.Drawing.Color;

namespace RatEyeTest
{
	public class LegacyDynamicIconTest : TestEnvironment
	{
		[Fact]
		public void ItemFHD()
		{
			var config = new Config()
			{
				PathConfig = new Config.Path()
				{
					DynamicIcons = "Data/DynamicIcons",
					DynamicCorrelationData = "Data/DynamicIcons/index.json",
				},
				ProcessingConfig = new Config.Processing()
				{
					IconConfig = new Config.Processing.Icon()
					{
						UseStaticIcons = false,
						UseDynamicIcons = true,
						UseLegacyCacheIndex = true,
					},
				},
			};
			var ratEye = new RatEyeEngine(config);

			var image = new Bitmap("TestData/FHD_Inventory2.png");
			var inventory = ratEye.NewInventory(image);
			var icon = inventory.LocateIcon(new Vector2(960, 525));
			Assert.Equal("OKP-7 reflex sight (dovetail)", icon.Item.Name);
			Assert.Equal("57486e672459770abd687134", icon.Item.Id);
			var expectedPath = Path.GetFullPath("Data/DynamicIcons/24.png");
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
					DynamicIcons = "Data/DynamicIcons",
					DynamicCorrelationData = "Data/DynamicIcons/index.json",
				},
				ProcessingConfig = new Config.Processing()
				{
					IconConfig = new Config.Processing.Icon()
					{
						UseStaticIcons = false,
						UseDynamicIcons = true,
						UseLegacyCacheIndex = true,
					},
					InventoryConfig = new Config.Processing.Inventory()
					{
						MaxGridColor = Color.FromArgb(89, 89, 89),
						OptimizeHighlighted = true,
					},
				},
			};
			var ratEye = new RatEyeEngine(config);

			var image = new Bitmap("TestData/FHD_InventoryHighlighted2.png");
			var inventory = ratEye.NewInventory(image);
			var icon = inventory.LocateIcon();
			Assert.Equal("MP-133 12ga pump-action shotgun", icon.Item.Name);
			Assert.Equal("54491c4f4bdc2db1078b4568", icon.Item.Id);
			var expectedPath = Path.GetFullPath("Data/DynamicIcons/121.png");
			Assert.Equal(expectedPath, Path.GetFullPath(icon.IconPath));
			Assert.True(icon.Rotated);
		}
	}
}
