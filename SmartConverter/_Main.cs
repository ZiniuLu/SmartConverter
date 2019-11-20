using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartConverter
{
    public class _Main
    {
        public static void Main()
        {
            string gcodePath = @"D:\C#\Code\NX12\SmartConverter\SmartConverter\2xUM2_beam.gcode";

            //string pathToCheck = @"D:\C#\Code\NX12\SmartConverter\SmartConverter\CheckGcode.txt";

            try
            {
                GeneratePRT(gcodePath);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private static int GeneratePRT(string pathGcode, string pathToCheck = "")
        {
            Console.WriteLine("Start!\n");

            // 1. read gcode file
            Console.WriteLine("1. Read Gcode file:");
            GcodeFile myGcodeFile = new GcodeFile(pathGcode);
            
            if(pathToCheck != "")
                myGcodeFile.CheckGcode(pathToCheck);

            Console.WriteLine("Line Count: " + myGcodeFile.LineCount);
            Console.WriteLine("Layer Count: " + myGcodeFile.LayerCount);

            // 2. get PointSet in each layer
            Console.WriteLine("\n----------------------------------------------\n2. Get point set in each layer");

            NX model = new NX();

            for (int layerNr = 0; layerNr < myGcodeFile.LayerCount; layerNr++)
            {
                //Console.WriteLine("\n\nLayer " + layerNr + "..........................................................");

                List<string> gcodeStringInLayer = myGcodeFile.GetGcodeInLayer(layerNr);   // get all gcode lines in one module-layer (formate: string)
                LayerContour myLayerContour = new LayerContour(layerNr, gcodeStringInLayer); // convert all gcode lines (string) to target format (LayerContour)

                // 3. sketch in NX
                Console.WriteLine("\n----------------------------------------------\n3. Sketch in NX");
                model.BuildCurrentLayer(layerNr, myLayerContour.MyContour);
            }

            //Console.ReadLine();

            return 0;
        }
    }
}