using System;
using System.Drawing;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace RatEye.Processing
{
	public class Inventory
	{
		private Mat _image;
		private Mat _scaledImage;
		private Mat _grid;
		private int _slotSize;
		private float _scale;

		/// <summary>
		/// Constructor for inventory view processing object
		/// </summary>
		/// <param name="image">Image of the inventory which will be processed</param>
		/// <param name="scale">Scale of the image. 1 when the image is from a FullHD screen. 2 when 4k.</param>
		/// <param name="slotSize">Pixel size of a single slot in pixel</param>
		public Inventory(Bitmap image, float scale = 1, int slotSize = 63)
		{
			_image = image.ToMat();
			_slotSize = slotSize;
			_scale = scale;
		}

		private enum State
		{
			Default,
			Rescaled,
			GridDetected,
		}

		private State _currentState = State.Default;

		private void SatisfyState(State targetState)
		{
			while (_currentState < targetState)
			{
				try
				{
					switch (++_currentState)
					{
						case State.Default:
							break;
						case State.Rescaled:
							RescaleInventory();
							break;
						case State.GridDetected:
							DetectInventoryGrid();
							break;
						default:
							throw new Exception("Cannot satisfy unknown state.");
					}
				}
				catch (Exception e)
				{
					Logger.LogError(e);
					throw;
				}
			}
		}

		private void RescaleInventory()
		{
			_scaledImage = _image.ToBitmap().Rescale(_scale).ToMat();
		}

		private void DetectInventoryGrid()
		{
			SatisfyState(State.Rescaled);

			// TODO implement
		}

		/// <summary>
		/// Finds the position and size of the icon at a given position
		/// </summary>
		/// <param name="origin">Position at which to locate the icon. null = center</param>
		/// <returns>A tuple containing position and size in pixel</returns>
		public (Vector2 position, Vector2 size) LocateIcon(Vector2 origin = null)
		{
			SatisfyState(State.GridDetected);

			if (origin == null) origin = new Vector2(_image.Size()) / 2;

			// TODO re-implement

			// --- Below is non-working leftover ---
			var mat = _image.Clone();
			Logger.LogDebugMat(mat, "capture");
			_grid = mat.InRange(new Scalar(84, 81, 73), new Scalar(104, 101, 93));
			Logger.LogDebugMat(_grid, "in_range");
			var mask = mat.Canny(100, 100);
			Logger.LogDebugMat(mask, "canny_edge");
			mask = mask.Dilate(Mat.Ones(10, 3, MatType.CV_8UC1));
			Logger.LogDebugMat(mask, "canny_edge_dilate");
			Cv2.BitwiseAnd(_grid, mask, _grid);
			Logger.LogDebugMat(_grid, "in_range_and_canny_edge_dilate");

			var rightBorderDist = 30;
			var leftBorderDist = 30;
			var topBorderDist = 30;
			var bottomBorderDist = 30;

			var position = origin - new Vector2(leftBorderDist, topBorderDist);
			var size = new Vector2(rightBorderDist + leftBorderDist, topBorderDist + bottomBorderDist);
			//LocatedIcon = (position, size);
			//return LocatedIcon;
			return (null, null);
		}
	}
}
