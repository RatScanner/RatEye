using System.Linq;

namespace RatEye
{
	/// <summary>
	/// Minimal item class used primarily for icon related code
	/// </summary>
	public class ItemInfo
	{
		/// <summary>
		/// Uid of the item
		/// </summary>
		public readonly string Uid;

		/// <summary>
		/// Mod uid's of the item
		/// </summary>
		public readonly string[] Mods;

		/// <summary>
		/// Metadata to distinguish items with folded stocks or other states
		/// </summary>
		public readonly string Meta;

		public bool HasMods => Mods?.Length > 0;

		public ItemInfo(string uid, string[] mods = null, string meta = null)
		{
			Uid = uid;
			Mods = mods;
			Meta = meta;
		}

		public override string ToString()
		{
			if (HasMods) return "Uid: " + Uid + string.Join("\nMod: ", Mods);
			else return "Uid: " + Uid;
		}

		public override int GetHashCode()
		{
			if (!HasMods) return Uid.GetHashCode();
			var modUidNotNullOrEmpty = Mods.Where(mod => !string.IsNullOrEmpty(mod));
			return (Uid + string.Join("", modUidNotNullOrEmpty)).GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ItemInfo icon)) return false;

			if (icon.Meta != Meta) return false;
			if (icon.Uid != Uid) return false;
			if ((icon.HasMods) != (HasMods)) return false;
			if (icon.Mods != null && Mods != null)
			{
				return icon.Mods.SequenceEqual(Mods);
			}

			return true;
		}
	}
}
