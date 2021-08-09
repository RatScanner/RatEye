using System;
using System.Collections.Generic;
using System.Linq;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace RatEye.Processing
{
	public class Inventory
	{
		private readonly Config _config;
		private readonly Mat _image;
		private Mat _grid;
		private Mat _vertGrid;
		private List<Icon> _icons;

		private Config.Processing ProcessingConfig => _config.ProcessingConfig;

		/// <summary>
		/// All icons, contained inside the inventory
		/// </summary>
		public IEnumerable<Icon> Icons
		{
			get
			{
				SatisfyState(State.GridParsed);
				return _icons;
			}
		}

		/// <summary>
		/// Constructor for inventory view processing object
		/// </summary>
		/// <param name="image">Image of the inventory which will be processed</param>
		/// <param name="overrideConfig">When provided, will be used instead of <see cref="Config.GlobalConfig"/></param>
		public Inventory(System.Drawing.Bitmap image, Config overrideConfig = null)
		{
			_config = overrideConfig ?? Config.GlobalConfig;
			_image = image.ToMat();
		}

		private enum State
		{
			Default,
			GridDetected,
			GridParsed,
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
					case State.GridDetected:
						DetectInventoryGrid();
						break;
					case State.GridParsed:
						ParseInventoryGrid();
						break;
					default:
						throw new Exception("Cannot satisfy unknown state.");
				}

				_currentState++;
			}
		}

		private void DetectInventoryGrid()
		{
			var minGridColor = _config.ProcessingConfig.InventoryConfig.MinGridColor;
			var maxGridColor = _config.ProcessingConfig.InventoryConfig.MaxGridColor;
			var minGridScalar = Scalar.FromRgb(minGridColor.R, minGridColor.G, minGridColor.B);
			var maxGridScalar = Scalar.FromRgb(maxGridColor.R, maxGridColor.G, maxGridColor.B);
			using var colorFilter = _image.InRange(minGridScalar, maxGridScalar);

			var scaledSlotSize = ProcessingConfig.ScaledSlotSize;

			using var lineStructure = Mat.Ones(MatType.CV_8U, new[] { (int)(scaledSlotSize), 1 });

			var verticalLines = colorFilter.Erode(lineStructure);
			Cv2.Dilate(verticalLines, verticalLines, lineStructure);
			using var horizontalLines = colorFilter.Erode(lineStructure.T());
			Cv2.Dilate(horizontalLines, horizontalLines, lineStructure.T());

			var filteredLines = new Mat();
			Cv2.BitwiseOr(horizontalLines, verticalLines, filteredLines);
			_vertGrid = verticalLines;
			_grid = filteredLines;
		}

		private void ParseInventoryGrid()
		{
			_icons = new List<Icon>();

			var gridIndexer = _grid.GetGenericIndexer<byte>();
			var vertGrindIndexer = _vertGrid.GetGenericIndexer<byte>();

			var rowStep = (int)(_config.ProcessingConfig.ScaledSlotSize);
			for (var y = 0; y < _vertGrid.Rows; y += rowStep)
			{
				for (var x = 0; x < _vertGrid.Cols; x++)
				{
					if (!vertGrindIndexer[y, x].Equals(0xFF)) continue;

					TryAddIcon(gridIndexer, x, y);
				}
			}
		}

		/// <summary>
		/// If the position at the indexer is part of a rectangle, it will be added to <see cref="_icons"/>
		/// </summary>
		/// <param name="indexer">The indexer of the mat</param>
		/// <param name="x">X position of the assumed icon</param>
		/// <param name="y">Y position of the assumed icon</param>
		/// <returns><see langword="true"/> if it is a valid icon, else <see langword="false"/></returns>
		private bool TryAddIcon(Mat.Indexer<byte> indexer, int x, int y)
		{
			/*
			 * The idea is, that we walk along the most inner
			 * edge of the assumed rectangle. If we do not find
			 * a left turn before reaching the end of our image,
			 * we know that the assumed rectangle was indeed not
			 * a rectangle and we return false.
			 *
			 * There are some optimizations to reduce the amount
			 * of loop iterations down to a minimum
			 */

			var rows = _grid.Rows;
			var cols = _grid.Cols;

			Vector2 bottomLeft = null;
			Vector2 bottomRight = null;
			Vector2 topRight = null;
			Vector2 topLeft = null;

			int a, b, c, d, e;

			// Go south
			for (a = y; a < rows; a++)
			{
				if (indexer[a, x].Equals(0x00)) return false;
				if (indexer[a, x + 1].Equals(0xFF))
				{
					bottomLeft = new Vector2(x, a);
					break;
				}

				if (!(a + 1 < rows)) return false;
			}

			// Go east
			for (b = x + 1; b < cols; b++)
			{
				if (indexer[a, b].Equals(0x00)) return false;
				if (indexer[a - 1, b].Equals(0xFF))
				{
					bottomRight = new Vector2(b, a);
					break;
				}

				if (!(b + 1 < cols)) return false;
			}

			// Go north
			for (c = a - 1; c > 0; c--)
			{
				if (indexer[c, b].Equals(0x00)) return false;
				if (indexer[c, b - 1].Equals(0xFF))
				{
					topRight = new Vector2(b, c);
					break;
				}

				if (!(c - 1 > 0)) return false;
			}

			// Go west
			for (d = b - 1; d >= x; d--)
			{
				if (indexer[c, d].Equals(0x00)) return false;
				if (indexer[c + 1, d].Equals(0xFF))
				{
					topLeft = new Vector2(d, c);
					break;
				}

				if (!(d - 1 >= x)) return false;
			}

			// Go south to origin
			for (e = c + 1; e <= y; e++)
			{
				if (indexer[e, d].Equals(0x00)) return false;
				if (!(e == y && d == x)) continue;

				var size = bottomRight - topLeft + Vector2.One;

				var altWidth = topRight.X - bottomLeft.X + 1;
				var altHeight = bottomLeft.Y - topRight.Y + 1;

				if (size != new Vector2(altWidth, altHeight)) return false;
				if (size == new Vector2(2, 2)) return false;
				if (_icons.Any(i => i.Position == topLeft)) return false;

				var icon = _image.ToBitmap().Crop(topLeft.X, topLeft.Y, size.X, size.Y);
				_icons.Add(new Icon(icon, topLeft, size, _config));
				return true;
			}

			return false;
		}

		/// <summary>
		/// Try to locate a icon at a given position
		/// </summary>
		/// <param name="position">Position at which to locate the icon. <see langword="null"/> = center</param>
		/// <returns><see langword="null"/> if no icon was found</returns>
		/// <remarks><see cref="Vector2.Zero"/> corresponds top left corner of the image</remarks>
		public Icon LocateIcon(Vector2 position = null)
		{
			SatisfyState(State.GridParsed);

			if (position == null) position = new Vector2(_grid.Size()) / 2;

			foreach (var icon in _icons)
			{
				if (position.X <= icon.Position.X || position.Y <= icon.Position.Y) continue;

				var bottomRight = icon.Position + icon.Size;
				if (position.X < bottomRight.X && position.Y < bottomRight.Y) { return icon; }
			}

			return null;
		}

		~Inventory()
		{
			_image?.Dispose();
			_grid?.Dispose();
			_vertGrid?.Dispose();
		}
	}
}
