using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartConverter
{
    public class _Main
    {
        public static int Main()
        {
            string gcodePath = @"D:\C#\Code\NX12\SmartConverter\SmartConverter\UM2_beam.gcode";

            Console.WriteLine("Start!\n");

            // 1. read gcode file
            Console.WriteLine("1. Read Gcode file:");
            GcodeFile myGcodeFile = new GcodeFile(gcodePath);
            myGcodeFile.CheckGcode(@"D:\C#\Code\NX12\SmartConverter\SmartConverter\CheckGcode.txt");

            Console.WriteLine("Line Count: " + myGcodeFile.LineCount);
            Console.WriteLine("Layer Count: " + myGcodeFile.LayerCount);

            // 2. Get PointSet in each layer
            Console.WriteLine("\n----------------------------------------------\n2. Get point set in each layer");
            for (int layerNr = 0; layerNr <= myGcodeFile.LayerCount; layerNr++)
            {
                List<string> gcodeInLayer = myGcodeFile.GetGcodeInLayer(layerNr);
                foreach(string currentLine in gcodeInLayer)
                {
                    GcodeLine thisLine = new GcodeLine(currentLine);
                }
            }



            // 3. sketch in NX
            Console.WriteLine("\n----------------------------------------------\n3. Sketch in NX");
            NX myPart = new NX
            {
                Name = "",
                Path = ""
            };


            myPart.Close(true);
            Console.ReadLine();

            return 0;
        }
    }
}