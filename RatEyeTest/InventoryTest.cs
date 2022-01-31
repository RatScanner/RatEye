using System.Drawing;
using System.Linq;
using RatEye;
using Xunit;

namespace RatEyeTest
{
	public class InventoryTest : TestEnvironment
	{
		[Fact]
		public void LocateSingleIcon()
		{
			var image = new Bitmap("TestData/FHD/Inventory2.png");
			var inventory = GetDefaultRatEyeEngine().NewInventory(image);
			var icon = inventory.LocateIcon(new Vector2(1200, 700));
			Assert.Equal((2, 2), icon.Item.GetSlotSize());
		}

		[Fact]
		public void LocateCenterIcon()
		{
			var image = new Bitmap("TestData/FHD/Inventory2Centered.png");
			var inventory = GetDefaultRatEyeEngine().NewInventory(image);
			var icon = inventory.LocateIcon();
			Assert.Equal((2, 2), icon.Item.GetSlotSize());
		}

		[Fact]
		public void LocateAllIcons()
		{
			var image = new Bitmap("TestData/FHD/Inventory2.png");
			var inventory = GetDefaultRatEyeEngine().NewInventory(image);
			var icons = inventory.Icons;
			Assert.NotNull(icons);
			Assert.Equal(89, icons.Count());
			Assert.DoesNotContain(null, icons);
		}

		[Fact]
		public void LocateIconInFullBackpack()
		{
			var image = new Bitmap("TestData/FHD/Grid1.png");
			var inventory = GetDefaultRatEyeEngine().NewInventory(image);
			var icon = inventory.LocateIcon(new Vector2(260, 130));
			Assert.Equal((1, 1), icon.Item.GetSlotSize());
			Assert.Equal("Propital", icon.Item.ShortName);
		}

		[Fact]
		public void LocateIconInFullBackpackHighlighted()
		{
			var image = new Bitmap("TestData/FHD/Grid1.png");
			var inventory = GetDefaultRatEyeEngine(true).NewInventory(image);
			var icon = inventory.LocateIcon(new Vector2(130, 130));
			Assert.Equal((1, 1), icon.Item.GetSlotSize());
			Assert.Equal("Morphine", icon.Item.ShortName);
		}

		[Fact]
		public void LocateIconInContainer()
		{
			var image = new Bitmap("TestData/FHD/Grid2.png");
			var inventory = GetDefaultRatEyeEngine().NewInventory(image);
			var icon = inventory.LocateIcon(new Vector2(200, 200));
			Assert.Equal((1, 1), icon.Item.GetSlotSize());
			Assert.NotNull(icon.Item);
		}

		[Fact]
		public void LocateIconInContainerHighlighted()
		{
			var image = new Bitmap("TestData/FHD/GridHighlighted2.png");
			var inventory = GetDefaultRatEyeEngine(true).NewInventory(image);
			var icon = inventory.LocateIcon(new Vector2(200, 200));
			Assert.Equal((1, 1), icon.Item.GetSlotSize());
			Assert.NotNull(icon.Item);
		}

		[Fact]
		public void LocateIconInSecureContainer()
		{
			var image = new Bitmap("TestData/FHD/Grid3.png");
			var inventory = GetDefaultRatEyeEngine().NewInventory(image);
			var icon = inventory.LocateIcon(new Vector2(50, 100));
			Assert.Equal((1, 2), icon.Item.GetSlotSize());
			Assert.Equal("Water", icon.Item.ShortName);
		}

		[Fact]
		public void LocateIconInSecureContainerHighlighted()
		{
			var image = new Bitmap("TestData/FHD/GridHighlighted3.png");
			var inventory = GetDefaultRatEyeEngine(true).NewInventory(image);
			var icon = inventory.LocateIcon(new Vector2(50, 100));
			Assert.Equal((1, 2), icon.Item.GetSlotSize());
			Assert.Equal("Water", icon.Item.ShortName);
		}
	}
}
