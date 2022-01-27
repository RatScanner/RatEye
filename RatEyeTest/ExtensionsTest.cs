using System.Drawing;
using RatEye;
using Xunit;

namespace RatEyeTest
{
	public class ExtensionsTest : TestEnvironment
	{
		[Fact]
		public void ImageCropOutOfBound()
		{
			var image = new Bitmap("TestData/FHD/Grid1.png");
			var cropped = image.Crop(-50, -100, 500, 1000);
			cropped.Save("Debug/Cropped.png");
			Assert.True(true);
		}
	}
}
