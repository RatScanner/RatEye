using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;

namespace RatEye
{
	/// <summary>
	/// Class for logging events and control flow of RatEye
	/// </summary>
	public static class Logger
	{
		private static List<string> backlog = new List<string>();

		internal static void LogDebug(string message)
		{
			if (Config.LogDebug) AppendToLog("[Debug] " + message);
		}

		internal static void LogDebugBitmap(Bitmap bitmap, string fileName = "bitmap")
		{
			if (Config.LogDebug)
			{
				bitmap.Save(GetUniquePath(Config.Path.Debug, fileName, ".png"));
			}
		}

		internal static void LogDebugMat(OpenCvSharp.Mat mat, string fileName = "mat")
		{
			if (Config.LogDebug)
			{
				mat.SaveImage(GetUniquePath(Config.Path.Debug, fileName, ".png"));
			}
		}

		internal static void LogInfo(string message)
		{
			AppendToLog("[Info]  " + message);
		}

		internal static void LogWarning(string message, Exception e = null)
		{
			AppendToLog("[Warning] " + message);
			AppendToLog(e == null ? Environment.StackTrace : e.ToString());
		}

		/// <summary>
		/// Logs a error
		/// </summary>
		/// <param name="e">Exception which gets written into the log</param>
		internal static void LogError(Exception e)
		{
			LogError(e.Message, e);
		}

		/// <summary>
		/// Logs a error
		/// </summary>
		/// <param name="message">Message which describes the error</param>
		/// <param name="e">Exception which gets written into the log</param>
		internal static void LogError(string message, Exception e = null)
		{
			// Log the error
			var logMessage = "[Error] " + message;
			var divider = new string('-', 20);
			if (e != null) logMessage += $"\n {divider} \n {e}";
			else logMessage += $"\n {divider} \n {Environment.StackTrace}";
			AppendToLog(logMessage);
		}

		/// <summary>
		/// Deletes all images from the configured debug folder
		/// </summary>
		public static void ClearDebugImages()
		{
			if (!Directory.Exists(Config.Path.Debug)) return;

			var files = Directory.GetFiles(Config.Path.Debug, "*.png");
			foreach (var file in files)
			{
				File.Delete(file);
			}
		}

		private static string GetUniquePath(string basePath, string fileName, string extension)
		{
			fileName = fileName.Replace(' ', '_');

			var index = 0;
			var uniquePath = Path.Combine(basePath, fileName + index + extension);

			while (File.Exists(uniquePath))
			{
				index += 1;
				uniquePath = Path.Combine(basePath, fileName + index + extension);
			}

			Directory.CreateDirectory(Path.GetDirectoryName(uniquePath));
			return uniquePath;
		}

		/// <summary>
		/// Deletes the log file
		/// </summary>
		public static void Clear()
		{
			File.Delete(Config.Path.LogFile);
		}

		private static void AppendToLog(string content)
		{
			ProcessBacklog();

			var prefix = "[" + DateTime.UtcNow.ToUniversalTime().TimeOfDay + "] > ";

			try
			{
				AppendToLogRaw(prefix + content + "\n");
			}
			catch (Exception e)
			{
				backlog.Add(prefix + "Could not write to log file\n" + e + "\n");
				backlog.Add(prefix + content + "\n");
				Thread.Sleep(250);
				ProcessBacklog();
			}
		}

		private static void AppendToLogRaw(string text)
		{
			System.Diagnostics.Debug.WriteLine(text);
			File.AppendAllText(Config.Path.LogFile, text, Encoding.UTF8);
		}

		private static void ProcessBacklog()
		{
			var newBacklog = new List<string>();

			foreach (var text in backlog)
			{
				try
				{
					AppendToLogRaw(text);
				}
				catch
				{
					newBacklog.Add(text);
				}
			}

			backlog = newBacklog;
		}
	}
}
