using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartConverter
{
    class GcodeFile
    {
        // Field
        private string path = "";
        private string name = "";
        private StreamReader file;

        private int lineCount = 0;
        private int layerCount = 0;
        private List<string>  gcode = new List<string> { };

        // Property
        public string Name
        {
            get
            {
                return name;
            }
        }
        public string Path
        {
            get
            {
                return path;
            }
            set
            {
                if (File.Exists(value) && (value.EndsWith(".gcode") || value.EndsWith (".txt")))
                {
                    path = value;
                    GetFileName();
                }
            }
        }
        public int LineCount
        {
            get
            {
                return lineCount;
            }
        }
        public int LayerCount
        {
            get
            {
                return layerCount;
            }
        }
        public List<string> Gcode
        {
            get
            {
                return gcode;
            }
        }

        public GcodeFile(string gcodePath)
        {
            path = gcodePath;
            if (Open())
            {
                GetGcode();
                GetLayerCount();
            }

        }
        ~GcodeFile()
        {
            Close();
        }

        // Method
        private bool Open()
        {
            try
            {
                file = new StreamReader(path);
                Console.WriteLine("Gcode File is opened!\n");
                return true;
            }
            catch (Exception)
            {
                Console.WriteLine("Open Gcode File failed!\n");
                Console.ReadLine();
                return false;
            }
        }
        private bool Close()
        {
            try
            {
                file.Close();
                Console.WriteLine("Gcode file is closed!");
            }
            catch (Exception)
            {
                Console.WriteLine("Close Gcode file failed!");
            }
            return true;
        }
        private void GetFileName()
        {
            int firstIndex = path.LastIndexOf("\\");
            int lastIndex = path.LastIndexOf(".");
            name = path.Substring(firstIndex + 1, lastIndex - firstIndex - 1);
            Console.WriteLine("file name = {0}\n", name);
        }
        private void GetLayerCount()
        {
            if (gcode.Count > 0)
            {
                string line = gcode[0];
                int firstIndex = line.IndexOf(":") + 1;
                int length = line.Length - firstIndex;

                layerCount = Int32.Parse(line.Substring(firstIndex, length));
            }
        }

        private List<string> GetGcode()
        {
            gcode.Clear();
            string currentLine;
            bool switch_keep_discard = true;

            while ((currentLine = file.ReadLine()) != null)
            {
                if (GcodeLine.GetLineType(currentLine) == GcodeLine.LType.Comment)
                {
                    switch (GcodeLine.GetCommentType(currentLine))
                    {
                        case GcodeLine.CType.Unknown:
                            {
                                switch_keep_discard = false;
                                break;
                            }
                        default:
                            {
                                switch_keep_discard = true;
                                gcode.Add(currentLine);
                                lineCount++;
                                break;
                            }
                    }
                }
                else if ((GcodeLine.GetLineType(currentLine) == GcodeLine.LType.Gcode) && switch_keep_discard)
                {
                    gcode.Add(currentLine);
                    lineCount++;
                    //Console.WriteLine("L{0}: {1}\n", lineCount, line);
                }
            }
            //Console.WriteLine("line count: {0} lines", lineCount);

            return gcode;
        }
        public bool CheckGcode(string path)
        {
            File.WriteAllText(path, String.Empty);
            File.WriteAllLines(path, gcode);

            return true;
        }

        public List<string> GetGcodeInLayer(int layerNr)
        {
            List<string> gcodeInLayer;

            string currentLayerKeyword = ";LAYER:" + layerNr.ToString();
            string nextLayerKeyword = ";LAYER:" + (layerNr + 1).ToString();

            int firstIndex = gcode.IndexOf(currentLayerKeyword);
            int lastIndex = gcode.IndexOf(nextLayerKeyword) - 1;

            if (firstIndex < 0)
                firstIndex = 0;
            if (lastIndex < firstIndex)
                lastIndex = gcode.Count - 1;

            int length = lastIndex - firstIndex + 1;

            gcodeInLayer = gcode.GetRange(firstIndex, length);

            return gcodeInLayer;
        }
    }
}
