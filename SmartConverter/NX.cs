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

        public void GetPointSet(int layerNr)
        {
            // Get all Poins in current layer from Gcode
        }

        public void BuildCurrentLayer()
        {
            // build the extrude using the sketch in current layer
        }
    }
}
