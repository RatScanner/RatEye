﻿using System.Drawing;
using System.Linq;
using RatEye;
using RatEye.Processing;
using Xunit;

namespace RatEyeTest
{
	[Collection("SerialTest")]
	public class InventoryTest : TestEnvironment
	{
		[Fact]
		public void LocateSingleIcon()
		{
			var image = new Bitmap("TestData/FHD_Inventory2.png");
			var inventory = new Inventory(image);
			var icon = inventory.LocateIcon(new Vector2(1100, 650));
			Assert.Equal(new Vector2(4, 2), icon.Size / 63);
		}

		[Fact]
		public void LocateCenterIcon()
		{
			var image = new Bitmap("TestData/FHD_Inventory2Centered.png");
			var inventory = new Inventory(image);
			var icon = inventory.LocateIcon();
			Assert.Equal(new Vector2(2, 2), icon.Size / 63);
		}

		[Fact]
		public void LocateAllIcons()
		{
			var image = new Bitmap("TestData/FHD_Inventory2.png");
			var inventory = new Inventory(image);
			var icons = inventory.Icons;
			Assert.NotNull(icons);
			Assert.Equal(89, icons.Count());
			Assert.DoesNotContain(null, icons);
		}
	}
}
