using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartConverter
{
    class NX
    {
        // Field
        private string name = "";
        private string path = "";
        private static List<string> gcodeLineInLayer = new List<string> { };
        private static List<GcodeLine> gcodeInLayer = new List<GcodeLine> { };

        // Property
        public string Name
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
        public string Path
        {
            get
            {
                return path;
            }
            set
            {
                if (File.Exists(value))
                {
                    path = value;
                }
            }
        }
        public static List<string> GcodeLineInLayer
        {
            set
            {
                gcodeLineInLayer = value;
            }
        }


        // Method
        public int Create(string dir)
        {
            if (!Directory.Exists(dir))
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
        public int Create()
        {
            if (Directory.Exists(path))
            {
                return Create(path);
            }
            else
            {
                return -1;
            }
        }
        public int Close(bool save)
        {
            if (save)
            {
                return 0;
            }
            else
            {
                return 0;
            }
        }

        


        private void ConvertGcode(string gcodeInString, int layerNr)
        {
            GcodeLine thisGcode = new GcodeLine(gcodeInString)
            {
                LayerNr = layerNr
            };
            if (thisGcode.LineType == GcodeLine.LType.Gcode)
            {
                gcodeInLayer.Add(thisGcode);
            }
        }

        public void BuildCurrentLayer()
        {
            // build the extrude using the sketch in current layer
        }
    }
}
