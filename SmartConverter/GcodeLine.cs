using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartConverter
{
    class GcodeLine
    {
        // Enum
        public enum LType
        {
            Gcode,
            Mcode,
            Comment,
            Unknown
        }
        public enum CType
        {
            LAYER_COUNT,
            LAYER,
            NONMESH,
            MESH,
            OUTERWALL,
            Unknown
        }

        // Field
        private string line = "";
        private double x = 0;
        private double y = 0;
        private double z = 0;
        private bool enableE = false; // Gcode with Extrude
        
        private static int lineNr = 0;

        private static double currentX = 0;
        private static double currentY = 0;
        private static double currentZ = 0;

        public LType lineType;


        public GcodeLine(string thisLine)
        {
            line = thisLine;
            lineType = GetLineType(line);
            if (lineType == LType.Gcode)
            {
                GetXYZ();
            }
        }

        // Method
        public static LType GetLineType(string gcodeLine)
        {
            char firstLetter = gcodeLine[0];
            LType myLType = LType.Unknown;
            switch (firstLetter)
            {
                case ';':
                    {
                        myLType = LType.Comment;
                        break;
                    }
                case 'G':
                    {
                        myLType = LType.Gcode;
                        break;
                    }
                case 'M':
                    {
                        myLType = LType.Mcode;
                        break;
                    }
                default:
                    {
                        myLType = LType.Unknown;
                        break;
                    }
            }

            return myLType;
        }
        public static CType GetCommentType(string commentLine)
        {
            CType myCType = CType.Unknown;
            if (commentLine[0] == ';')
            {
                switch(GetCommentKeyword(commentLine))
                {
                    case "LAYER_COUNT":
                        {
                            myCType = CType.LAYER_COUNT;
                            break;
                        }
                    case "LAYER":
                        {
                            myCType = CType.LAYER;
                            break;
                        }
                    case "NONMESH":
                        {
                            myCType = CType.NONMESH;
                            break;
                        }
                    case "MESH":
                        {
                            myCType = CType.MESH;
                            break;
                        }
                    case "OUTERWALL":
                        {
                            myCType = CType.OUTERWALL;
                            break;
                        }
                    default:
                        {
                            myCType = CType.Unknown;
                            break;
                        }
                }
            }

            return myCType;
        }   // done
        private static string GetCommentKeyword(string comment)
        {
            string keyword = "Unknown";

            if (comment.Contains("LAYER_COUNT:")) { keyword = "LAYER_COUNT"; }
            else if (comment.Contains("LAYER:")) { keyword = "LAYER"; }
            else if (comment.Contains("MESH:NONMESH")) { keyword = "NONMESH"; }
            else if (comment.Contains("MESH:")) { keyword = "MESH"; }
            else if (comment.Contains("TYPE:WALL-OUTER")) { keyword = "OUTERWALL"; }

            return keyword;
        }   // done

        private static void GetXYZ()
        {

        }

    } 
}
