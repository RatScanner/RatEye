using System.Drawing;
using RatEye;
using RatEye.Processing;
using Xunit;

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
		}

		[Fact]
		public void ItemFHDDynamic()
		{
			var image = new Bitmap("TestData/FHD_Inventory2.png");
			var inventory = new Inventory(image);
			var icon = inventory.LocateIcon(new Vector2(960, 525));
			Assert.Equal("OKP-7 reflex sight", icon.Item.Name);
			Assert.Equal("57486e672459770abd687134", icon.Item.Id);
		}
	}
}
