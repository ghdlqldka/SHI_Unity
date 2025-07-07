using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace _Base_Framework
{

    public class _ExternalProcess
    {
        public virtual Process StartProcess(string fileName)
        {
            using (Process myProcess = new Process())
            {
                myProcess.StartInfo.UseShellExecute = false;
                myProcess.StartInfo.FileName = fileName;
                myProcess.StartInfo.CreateNoWindow = true;

                myProcess.Start();

                // This code assumes the process you are starting will terminate itself.
                // Given that it is started without a window so you cannot terminate it
                // on the desktop, it must terminate itself or you can do it programmatically
                // from this application using the Kill method.

                return myProcess;
            }
        }

        public virtual Process StartProcess(string fileName, string workingDirectory, bool useShellExecute, ProcessWindowStyle windowStyle = ProcessWindowStyle.Normal)
        {
            using (Process myProcess = new Process())
            {
                myProcess.StartInfo.FileName = fileName;
                myProcess.StartInfo.WorkingDirectory = workingDirectory;
                myProcess.StartInfo.UseShellExecute = useShellExecute; //ensure that it uses shell execute otherwise the application wont work
                myProcess.StartInfo.WindowStyle = windowStyle; //sets whether the shell window will display or not, this can be controlled in the Unity editor.

                return myProcess;
            }
        }

        // Start Internet Explorer. Defaults to the home page.
        // Process.Start("IExplore.exe");
        public static void OpenApplication(string fileName)
        {
            Process.Start(fileName);
        }

        // url's are not considered documents. They can only be opened by passing them as arguments.
        // Process.Start("IExplore.exe", "www.northwindtraders.com");

        // Start a Web page using a browser associated with .html and .asp files.
        // Process.Start("IExplore.exe", "C:\\myPath\\myFile.htm");
        // Process.Start("IExplore.exe", "C:\\myPath\\myFile.asp");
        public static void OpenWithArguments(string fileName, string arguments)
        {
            Process.Start(fileName, arguments);
        }

        // Uses the ProcessStartInfo class to start new processes, both in a minimized mode.
        /*
        ProcessStartInfo startInfo = new ProcessStartInfo("IExplore.exe");
        startInfo.WindowStyle = ProcessWindowStyle.Minimized;

        Process.Start(startInfo);

        startInfo.Arguments = "www.northwindtraders.com";

        Process.Start(startInfo);
        */
        public static void OpenWithStartInfo(ProcessStartInfo startInfo)
        {
            Process.Start(startInfo);
        }
    }
}