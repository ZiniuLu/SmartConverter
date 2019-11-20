using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartConverter
{
    public static class CMD
    {
        private static Process cmdProcess = new Process();
        private static string name = "cmd.exe";
        private static string path = "";
        private static bool state = false;


        public static string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }
        public static string Path
        {
            get
            {
                return path;
            }
            set
            {
                path = value;
            }
        }
        public static bool State
        {
            get
            {
                return state;
            }
            set
            {
                state = value;
            }
        }


        private static void WriteCommand(string command)
        {
            if (!state)
                Create();

            cmdProcess.StandardInput.WriteLine(command);
            cmdProcess.StandardInput.Flush();
            cmdProcess.StandardInput.Close();

            Console.WriteLine(cmdProcess.StandardOutput.ReadToEnd());
        }

        public static void Create()
        {
            cmdProcess.StartInfo.FileName = name;
            cmdProcess.StartInfo.CreateNoWindow = true;
            cmdProcess.StartInfo.RedirectStandardInput = true;
            cmdProcess.StartInfo.RedirectStandardOutput = true;
            cmdProcess.Start();

            state = true;
        }

    }
}
