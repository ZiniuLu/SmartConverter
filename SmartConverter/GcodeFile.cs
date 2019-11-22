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

        private int lineCount = 0;      // number of closed lines/curves in a layer
        private int layerCount = 0;     // number of layers
        private List<string>  gcodeText = new List<string> { };

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
                    GetGcodeFileName();
                }
            }
        }
        public int LayerCount
        {
            get
            {
                return layerCount;
            }
        }
        public List<string> GcodeText
        {
            get
            {
                return gcodeText;
            }
        }

        public GcodeFile(string pathGcodeFile)
        {
            path = pathGcodeFile;
            if (Open())
            {
                GetGcodeText();
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
                return true;
            }
            catch (Exception)
            {
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
        private void GetGcodeFileName()
        {
            int firstIndex = path.LastIndexOf("\\");
            int lastIndex = path.LastIndexOf(".");
            name = path.Substring(firstIndex + 1, lastIndex - firstIndex - 1);
            Console.WriteLine("file name = {0}\n", name);
        }
        private void GetLayerCount()
        {
            if (gcodeText.Count > 0)
            {
                string line = gcodeText[0];
                int firstIndex = line.IndexOf(":") + 1;
                int length = line.Length - firstIndex;

                layerCount = Int32.Parse(line.Substring(firstIndex, length));
            }
        }

        private List<string> GetGcodeText()
        {
            gcodeText.Clear();
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
                                gcodeText.Add(currentLine);
                                lineCount++;
                                break;
                            }
                    }
                }
                else if ((GcodeLine.GetLineType(currentLine) == GcodeLine.LType.Gcode) && switch_keep_discard)
                {
                    gcodeText.Add(currentLine);
                    lineCount++;
                    //Console.WriteLine("L{0}: {1}\n", lineCount, line);
                }
            }
            //Console.WriteLine("line count: {0} lines", lineCount);

            return gcodeText;
        }
        public bool PrintFilteredGcode(string path)
        {
            File.WriteAllText(path, String.Empty);
            File.WriteAllLines(path, gcodeText);

            return true;
        }

        public List<string> GetGcodeTextInLayer(int layerNr)
        {
            List<string> gcodeInLayer;

            string currentLayerKeyword = ";LAYER:" + layerNr.ToString();
            string nextLayerKeyword = ";LAYER:" + (layerNr + 1).ToString();

            int firstIndex = gcodeText.IndexOf(currentLayerKeyword);
            int lastIndex = gcodeText.IndexOf(nextLayerKeyword) - 1;

            if (firstIndex < 0)
                firstIndex = 0;
            if (lastIndex < firstIndex)
                lastIndex = gcodeText.Count - 1;

            int length = lastIndex - firstIndex + 1;

            gcodeInLayer = gcodeText.GetRange(firstIndex, length);

            return gcodeInLayer;
        }
    }
}
