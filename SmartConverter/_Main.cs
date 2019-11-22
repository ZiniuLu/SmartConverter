using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartConverter
{
    public class _Main
    {
        public static void Main(string[] args)
        {

            string pathGcodeFile = args[0];
            double layerHeight = Double.Parse(args[1]);

            //string pathGcodeFile = @"D:\C#\Code\NX12\SmartConverter\SmartConverter\GcodeFile_generatedFromCura4.3\2xUM2_beam.gcode";
            //string pathToCheck = @"D:\C#\Code\NX12\SmartConverter\SmartConverter\CheckGcode.txt";

            try
            {
                BuildModelInNX(pathGcodeFile, layerHeight);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private static int BuildModelInNX(string pathGcodeFile, double layerHeight, string pathFilteredGcode = "")
        {
            // 1. read gcode file
            GcodeFile myGcodeFile = new GcodeFile(pathGcodeFile);

            if(pathFilteredGcode != "")
                myGcodeFile.PrintFilteredGcode(pathFilteredGcode);

            // 2. get PointSet in each layer
            NX model = new NX(layerHeight);

            for (int layerNr = 0; layerNr < myGcodeFile.LayerCount; layerNr++)
            {
                List<string> gcodeStringInLayer = myGcodeFile.GetGcodeTextInLayer(layerNr);   // get all gcode lines in one module-layer (formate: string)
                LayerContour myLayerContour = new LayerContour(layerNr, layerHeight, gcodeStringInLayer); // convert all gcode lines (string) to target format (LayerContour)

                // 3. sketch in NX
                model.BuildCurrentLayer(layerNr, myLayerContour.MyContour);
            }

            return 0;
        }
    }
}