using System;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace RatEye.Processing
{
	public class Inventory
	{
		private Mat _image;
		private Mat _grid;

		private enum Direction
		{
			North,
			East,
			South,
			West,
		}

		/// <summary>
		/// Constructor for inventory view processing object
		/// </summary>
		/// <param name="image">Image of the inventory which will be processed</param>
		public Inventory(System.Drawing.Bitmap image)
		{
			_image = image.ToMat();
		}

		private enum State
		{
			Default,

			//Rescaled,
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
						//case State.Rescaled:
						//	RescaleInventory();
						//	break;
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

		//private void RescaleInventory()
		//{
		//	_scaledImage = _image.ToBitmap().Rescale(Config.Processing.Scale).ToMat();
		//}

		private void DetectInventoryGrid()
		{
			SatisfyState(State.Default);

			var colorFilter = _image.InRange(new Scalar(84, 81, 73), new Scalar(108, 117, 112));

			var scaledSlotSize = Config.Processing.ScaledSlotSize;

			var lineStructure = Mat.Ones(MatType.CV_8U, new[] { (int)(scaledSlotSize), 1 });

			var verticalLines = colorFilter.Erode(lineStructure);
			verticalLines = verticalLines.Dilate(lineStructure);
			var horizontalLines = colorFilter.Erode(lineStructure.T());
			horizontalLines = horizontalLines.Dilate(lineStructure.T());

			var filteredLines = new Mat();
			Cv2.BitwiseOr(horizontalLines, verticalLines, filteredLines);
			_grid = filteredLines;
		}

		/// <summary>
		/// Finds the origin and size of the icon at a given origin
		/// </summary>
		/// <param name="origin">Position at which to locate the icon. <see langword="null"/> = center</param>
		/// <returns>A tuple containing the position and size in pixel</returns>
		/// <remarks><see cref="origin"/> of (0, 0) means top left corner of the image</remarks>
		public (Vector2 position, Vector2 size) LocateIcon(Vector2 origin = null)
		{
			SatisfyState(State.GridDetected);

			if (origin == null) origin = new Vector2(_grid.Size()) / 2;

			var topBorderDist = FindGridEdgeDistance(origin, Direction.North);
			var rightBorderDist = FindGridEdgeDistance(origin, Direction.East);
			var bottomBorderDist = FindGridEdgeDistance(origin, Direction.South);
			var leftBorderDist = FindGridEdgeDistance(origin, Direction.West);

			var position = origin - new Vector2(leftBorderDist, topBorderDist);
			var size = new Vector2(rightBorderDist + leftBorderDist, topBorderDist + bottomBorderDist);
			return (position, size);
		}

		/// <summary>
		/// Find the distance to the first white pixel in a give direction
		/// </summary>
		/// <param name="origin">Position at which to locate the icon.</param>
		/// <param name="direction"></param>
		/// <returns></returns>
		private int FindGridEdgeDistance(Vector2 origin, Direction direction)
		{
			var maxSteps = direction switch
			{
				Direction.North => origin.Y,
				Direction.East => _grid.Width - origin.X,
				Direction.South => _grid.Height - origin.Y,
				Direction.West => origin.X,
			};

			var (xDiff, yDiff) = direction switch
			{
				Direction.North => (0, -1),
				Direction.East => (1, 0),
				Direction.South => (0, 1),
				Direction.West => (-1, 0),
			};

			var x = origin.X;
			var y = origin.Y;

			var indexer = _grid.GetGenericIndexer<byte>();
			for (var i = 0; i < maxSteps; i++)
			{
				if (indexer[y, x].Equals(0xFF))
				{
					Logger.LogDebug($"Edge at: ({x}, {y}) | Distance from origin: {i}");
					return i;
				}

				x += xDiff;
				y += yDiff;
			}

			Logger.LogDebug($"No edge found! Origin: ({x}, {y}) | Direction: {direction}");
			return 0;
		}
	}
}
