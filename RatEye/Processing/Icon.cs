using System;
using System.Drawing;

namespace RatEye.Processing
{
	public class Icon
	{
		private readonly Config _config;
		private readonly Bitmap _image;
		private Bitmap _icon;
		private Bitmap _scaledIcon;

		/// <summary>
		/// Position of the icon inside the reference image
		/// </summary>
		public Vector2 Position { get; }

		/// <summary>
		/// Size of the icon, measured in pixel
		/// </summary>
		public Vector2 Size { get; }

		internal Icon(Bitmap image, Vector2 position, Vector2 size, Config config)
		{
			_config = config;
			_image = image;
			Position = position;
			Size = size;
		}

		private enum State
		{
			Default,
			Cropped,
			Rescaled,
		}

		private State _currentState = State.Default;

		private void SatisfyState(State targetState)
		{
			while (_currentState < targetState)
			{
				switch (_currentState + 1)
				{
					case State.Default:
						break;
					case State.Cropped:
						CropIcon();
						break;
					case State.Rescaled:
						RescaleIcon();
						break;
					default:
						throw new Exception("Cannot satisfy unknown state.");
				}

				_currentState++;
			}
		}

		private void CropIcon()
		{
			_icon = _image.Crop(Position.X, Position.Y, Size.X, Size.Y);
		}

		private void RescaleIcon()
		{
			_scaledIcon = _icon.Rescale(_config.ProcessingConfig.InverseScale);
		}
	}
}
