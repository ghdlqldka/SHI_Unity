using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using System.Runtime.InteropServices;

namespace _Magenta_WebGL //ZenFulcrum.EmbeddedBrowser
{
	/**
	 * Getting CEF running on a build result requires some fiddling to get all the files in the right place.
	 */
	class _Magenta_WebGL_PostProcessBuild //PostBuildStandalone
    {
		private static string LOG_FORMAT = "<color=magenta><b>[_Magenta_WebGL_PostProcessBuild]</b></color> {0}";

		// #if false

		/*
		static readonly List<string> byBinFiles = new List<string>(){"natives_blob.bin",
			"snapshot_blob.bin",
			"v8_context_snapshot.bin",
			"icudtl.dat",
		};
		*/
		static readonly List<string> __copyFileList__ = new List<string>(){"Magenta_WebGL_AppConfig.ini"};

		// [PostProcessBuild(10)]
		public static void PostprocessBuild(BuildTarget target, string buildFile)
		{
			try
			{
				if (_Magenta_WebGL_Config.Product != _Base_Framework._Base_Framework_Config._Product.Magenta_Framework)
				{
					Debug.LogWarningFormat(LOG_FORMAT, "This is NOT <b><color=red>Magenta_WebGL</color></b>!!!!!");
					return;
				}

				// PostprocessBuild(), target : StandaloneWindows64, buildFile : D:/Builds/Base_Framework/Base_Framework.exe
				Debug.LogWarningFormat(LOG_FORMAT, "PostprocessBuild(), target : <b><color=yellow>" + target + "</color></b>, buildFile : <b><color=yellow>" + buildFile + "</color></b>");

				PostprocessBuild_LinuxOrWindows(target, buildFile);
				// PostprocessBuild_Mac(target, buildFile);
			}
			catch (Exception ex)
			{
				EditorUtility.DisplayDialog("Magenta_WebGL build processing failed", ex.Message, "OK");
				throw;
			}
		}

		public static void PostprocessBuild_LinuxOrWindows(BuildTarget target, string buildFile)
		{
			// prereq
			bool windows = target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64;
			bool linux = target == BuildTarget.StandaloneLinux64;

			// PostprocessBuild_LinuxOrWindows(), windows : True, linux : False
			Debug.LogFormat(LOG_FORMAT, "PostprocessBuild_LinuxOrWindows(), windows : <b><color=yellow>" + windows + "</color></b>, linux : <b><color=yellow>" + linux + "</color></b>");

#if !UNITY_2019_2_OR_NEWER
			if (target == BuildTarget.StandaloneLinux || target == BuildTarget.StandaloneLinuxUniversal)
			{
				throw new Exception("ZFBrowser on Linux requires building for x86_64, not 32 bit or universal");
			}
#endif

			if (windows == false && linux == false)
			{
				return;
			}

			if (windows == true && buildFile.Contains(";"))
			{
				//Because Windows magically can't load our .dlls if it does. What can be done about it? ¯\_(ツ)_/¯
				throw new Exception("The build target (" + buildFile + ") may not contain a semicolon (;).");
			}

			//base info
			string buildType;
			if (windows == true)
			{
				buildType = "w" + (target == BuildTarget.StandaloneWindows64 ? "64" : "32");
			}
			else // Linux
			{
				buildType = "l64";
			}

			// Post processing D:/Builds/Base_Framework/Base_Framework.exe as w64
			Debug.LogFormat(LOG_FORMAT, "Post processing " + buildFile + " as " + buildType);

			string buildName;
			if (windows == true)
			{
				buildName = Regex.Match(buildFile, @"/([^/]+)\.exe$").Groups[1].Value;
			}
			else // Linux
			{
				buildName = Regex.Match(buildFile, @"\/([^\/]+?)(\.x86(_64)?)?$").Groups[1].Value;
			}
			// buildName : Base_Framework
			Debug.LogFormat(LOG_FORMAT, "buildName : " + buildName);

			// buildFile : D:/Builds/Base_Framework/Base_Framework.exe
			DirectoryInfo buildPath = Directory.GetParent(buildFile); // D:/Builds/Base_Framework
			string destDataPath = buildPath + "/" + buildName + "_Data"; // D:/Builds/Base_Framework/Base_Framework_Data
			string destPostProcessBuild_DataPath = destDataPath + "/PostProcessBuild_Data/";
			// destDataPath : D:\Builds\Base_Framework/Base_Framework_Data, destPostProcessBuild_DataPath : D:\Builds\Base_Framework/Base_Framework_Data/PostProcessBuild_Data/
			Debug.LogFormat(LOG_FORMAT, "destDataPath : " + destDataPath + ", destPostProcessBuild_DataPath : " + destPostProcessBuild_DataPath);

			if (Directory.Exists(destPostProcessBuild_DataPath) == false)
			{
				Directory.CreateDirectory(destPostProcessBuild_DataPath);
			}

			//can't use FileLocations because we may not be building the same type as the editor
			// sourceProcessBuild_DataPath : D:\Works\Base_Framework\Assets\_Framework\Base_Framework/PostProcessBuild_Data
			string sourceProcessBuild_DataPath = PostProcessBuild_Folder + "/PostProcessBuild_Data";

			//start copying

			//(Unity will copy the .dll and .so files for us)

			string sourceFileName;
			string destFileName;
			bool overwrite;

			foreach (string file in __copyFileList__)
			{
				sourceFileName = sourceProcessBuild_DataPath + "/" + file;
				destFileName = destPostProcessBuild_DataPath + file;
				overwrite = true;
				Debug.LogFormat(LOG_FORMAT, "File Copy => sourceFileName : " + sourceFileName + ", destFileName : " + destFileName);
				File.Copy(sourceFileName, destFileName, overwrite);
			}

#if false //
			sourceFileName = PostProcessBuild_Folder + "/ThirdPartyNotices.txt";
			destFileName = destPostProcessBuild_DataPath + "/ThirdPartyNotices.txt";
			overwrite = true;
			File.Copy(sourceFileName, destFileName, overwrite);

			//Copy the needed resources
			string resSrcDir = sourceProcessBuild_DataPath + "/CEFResources";
			foreach (string filePath in Directory.GetFiles(resSrcDir))
			{
				string fileName = new FileInfo(filePath).Name;
				if (fileName.EndsWith(".meta"))
				{
					continue;
				}

				File.Copy(filePath, destPostProcessBuild_DataPath + fileName, true);
			}

			//Slave process (doesn't get automatically copied by Unity like the shared libs)
			string exeExt = windows ? ".exe" : "";

#if false //
			sourceFileName = sourceProcessBuild_DataPath + "/" + FileLocations.SlaveExecutable + exeExt;
			destFileName = destPostProcessBuild_DataPath + FileLocations.SlaveExecutable + exeExt;
			overwrite = true;
			File.Copy(sourceFileName, destFileName, overwrite);

			if (linux == true)
			{
				MakeExecutable(destPostProcessBuild_DataPath + FileLocations.SlaveExecutable + exeExt);
			}
#endif

			//Locales
			string localesSrcDir = sourceProcessBuild_DataPath + "/CEFResources/locales";
			string localesDestDir = destDataPath + "/Plugins/locales";
			Directory.CreateDirectory(localesDestDir);
			foreach (string filePath in Directory.GetFiles(localesSrcDir))
			{
				string fileName = new FileInfo(filePath).Name;
				if (fileName.EndsWith(".meta"))
				{
					continue;
				}

				File.Copy(filePath, localesDestDir + "/" + fileName, true);
			}

			//Newer versions of Unity put the shared libs in the wrong place. Move them to where we expect them.
			if (linux == true && File.Exists(destPostProcessBuild_DataPath + "x86_64/zf_cef.so"))
			{
				foreach (string libFile in new[] { "zf_cef.so", "libEGL.so", "libGLESv2.so", "libZFProxyWeb.so" })
				{
					ForceMove(destPostProcessBuild_DataPath + "x86_64/" + libFile, destPostProcessBuild_DataPath + libFile);
				}
			}
#endif

#if false
			WriteBrowserAssets(dataPath + "/" + StandaloneWebResources.DefaultPath);
#endif
		}

		public static void PostprocessBuild_Mac(BuildTarget target, string buildFile)
		{
#if UNITY_2017_3_OR_NEWER
			if (target != BuildTarget.StandaloneOSX)
            {
				return;
			}
#else
		if (target == BuildTarget.StandaloneOSXUniversal || target == BuildTarget.StandaloneOSXIntel) {
			throw new Exception("ZFBrowser: Only OS X builds for x86_64 are supported.");
		}
		if (target != BuildTarget.StandaloneOSXIntel64) return;
#endif

			Debug.LogFormat(LOG_FORMAT, "Post processing " + buildFile);

			//var buildName = Regex.Match(buildFile, @"\/([^\/]+?)\.app$").Groups[1].Value;
			var buildPath = buildFile;
			var platformPluginsSrc = PostProcessBuild_Folder + "/Plugins/m64";

			//Copy app bits
			CopyDirectory(
				platformPluginsSrc + "/BrowserLib.app/Contents/Frameworks/Chromium Embedded Framework.framework",
				buildPath + "/Contents/Frameworks/Chromium Embedded Framework.framework"
			);
			CopyDirectory(
				platformPluginsSrc + "/BrowserLib.app/Contents/Frameworks/ZFGameBrowser.app",
				buildPath + "/Contents/Frameworks/ZFGameBrowser.app"
			);

			MakeExecutable(buildPath + "/BrowserLib.app/Contents/Frameworks/ZFGameBrowser.app/Contents/MacOS/ZFGameBrowser");

			if (Directory.Exists(buildPath + "/Contents/Plugins") == false)
			{
				Directory.CreateDirectory(buildPath + "/Contents/Plugins");
			}

			File.Copy(platformPluginsSrc + "/libZFProxyWeb.dylib", buildPath + "/Contents/Plugins/libZFProxyWeb.dylib", true);

			File.Copy(PostProcessBuild_Folder + "/ThirdPartyNotices.txt", buildPath + "/ThirdPartyNotices.txt", true);

#if false
			//BrowserAssets
			WriteBrowserAssets(buildPath + "/Contents/" + StandaloneWebResources.DefaultPath);
#endif
		}

#if false //
		private static void WriteBrowserAssets(string path)
		{
			// Debug.LogFormat(LOG_FORMAT, "Writing browser assets to " + path);

			string htmlDir = Application.dataPath + "/../BrowserAssets";
			Dictionary<string, byte[]> allData = new Dictionary<string, byte[]>();
			if (Directory.Exists(htmlDir))
			{
				foreach (string file in Directory.GetFiles(htmlDir, "*", SearchOption.AllDirectories))
				{
					string localPath = file.Substring(htmlDir.Length).Replace("\\", "/");
					allData[localPath] = File.ReadAllBytes(file);
				}
			}

#if false
			var wr = new StandaloneWebResources(path);
			wr.WriteData(allData);
#endif
		}

		private static void ForceMove(string src, string dest)
		{
			if (File.Exists(dest))
			{
				File.Delete(dest);
			}

			File.Move(src, dest);
		}
#endif //

		private static string PostProcessBuild_Folder
		{
			get
			{
				// path : D:\Works\Base_Framework\Assets\_Framework\Base_Framework\_PostProcessBuild\Editor\_Magenta_Framework_PostProcessBuild.cs
				string path = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
				Debug.LogFormat(LOG_FORMAT, "path : " + path);
				path = Directory.GetParent(path).Parent.FullName;
				// PostProcessBuild_Folder : D:\Works\Base_Framework\Assets\_Framework\Base_Framework\_PostProcessBuild
				Debug.LogFormat(LOG_FORMAT, "PostProcessBuild_Folder : " + path);
				return path;
			}
		}

		private static void CopyDirectory(string src, string dest)
		{
			string path = src;
			string searchPattern = "*";
			SearchOption searchOption = SearchOption.AllDirectories;
			foreach (string dir in Directory.GetDirectories(path, searchPattern, searchOption))
			{
				Directory.CreateDirectory(dir.Replace(src, dest));
			}

			foreach (string file in Directory.GetFiles(src, "*", SearchOption.AllDirectories))
			{
				if (file.EndsWith(".meta"))
					continue;

				File.Copy(file, file.Replace(src, dest), true);
			}
		}

		private static void MakeExecutable(string fileName)
		{
#if UNITY_EDITOR_WIN
			Debug.LogWarning("ZFBrowser: Be sure to mark the file \"" + fileName + "\" as executable (chmod +x) when you distribute it. If it's not executable the browser won't work.");
#else
		//dec 493 = oct 755 = -rwxr-xr-x
		chmod(fileName, 493);
#endif
		}

		[DllImport("__Internal")] static extern int symlink(string destStr, string symFile);
		[DllImport("__Internal")] static extern int chmod(string file, int mode);

		// #endif

	}

}
