
namespace DTDanmaku
{
	using System;
	using System.IO;

	public class Util
	{
		/// <summary>
		/// Returns the path of where the executable is located; should not include the trailing
		/// slash or backslash.
		/// 
		/// Note that the location of where the executable is has no relationship with the current
		/// working directory (usually denoted as ".").  It is possible to execute a program from
		/// any current working directory.
		/// </summary>
		public static string GetExecutablePath()
		{
			var executablePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
			var executableDirectory = Directory.GetParent(executablePath);

			return executableDirectory.FullName;
		}
	}
}
