using System.Drawing;
using RatEye;
using RatEye.Processing;
using Xunit;

namespace RatEyeTest
{
	public class InventoryTest : TestEnvironment
	{
		[Fact]
		public void SegregateInventory()
		{
			var image = new Bitmap("TestData/FHD_Inventory2.png");
			var inventory = new Inventory(image);
			var (pos, size) = inventory.LocateIcon(new Vector2(1100, 650));
			Assert.Equal(new Vector2(4, 2), size);
		}
	}
}
