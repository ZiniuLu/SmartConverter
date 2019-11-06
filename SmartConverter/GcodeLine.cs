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
            Unknown,
            Error
        }

        // Field
        private string line;
        private LType lineType = LType.Unknown;
        private CType commentType = CType.Unknown;
        private int layerNr = -1;

        private readonly char[] element = new char[] { 'X', 'Y', 'Z', 'E' };
        private bool[] elementExist = new bool[] { false, false, false, false}; // element: X Y Z E
        private int[] elementIndex = new int[] { -1, -1, -1, -1 };
        private double[] elementValue = new double[] { 0, 0, 0, 0 };

        public static double currentX = 0;
        public static double currentY = 0;

        // Property
        public LType LineType
        {
            get
            {
                return lineType;
            }
        }
        public CType CommentType
        {
            get
            {
                return commentType;
            }
        }
        public int LayerNr
        {
            get
            {
                return layerNr;
            }
            set
            {
                layerNr = value;
            }
        }
        public double X
        {
            get
            {
                return elementValue[0];
            }
        }
        public double Y
        {
            get
            {
                return elementValue[1];
            }
        }
        public bool EnableE
        {
            get
            {
                return elementExist[3];
            }
        }

        public GcodeLine(string thisLine)
        {
            line = thisLine;
            lineType = GetLineType(line);

            if (lineType == LType.Gcode)
            {
                CheckElement();
                GetPointValue();
            }
            else if (lineType == LType.Comment)
            {
                commentType = GetCommentType(line);
            }
        }

        // Method
        public static LType GetLineType(string gcodeLine)
        {
            char firstLetter = gcodeLine[0];
            LType myLType;
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
                switch (GetCommentKeyword(commentLine))
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
            else
            {
                myCType = CType.Error;
            }

            return myCType;
        }
        private static string GetCommentKeyword(string comment)
        {
            string keyword = "Unknown";

            if (comment.Contains("LAYER_COUNT:")) { keyword = "LAYER_COUNT"; }
            else if (comment.Contains("LAYER:")) { keyword = "LAYER"; }
            else if (comment.Contains("MESH:NONMESH")) { keyword = "NONMESH"; }
            else if (comment.Contains("MESH:")) { keyword = "MESH"; }
            else if (comment.Contains("TYPE:WALL-OUTER")) { keyword = "OUTERWALL"; }

            return keyword;
        }
        private void GetLayerNr()
        {
            if((lineType == LType.Comment) && (commentType == CType.LAYER))
            {
                int firstIndex = line.IndexOf(":") + 1;
                int length = line.Length - firstIndex;
                layerNr = Int32.Parse(line.Substring(firstIndex, length));
            }
        }

        private void CheckElement()
        {
            for(int i = 0; i <= element.Length - 1; i++)
            {
                if(line.Contains(element[i]))
                {
                    elementExist[i] = true;
                    elementIndex[i] = line.IndexOf(element[i]);
                }
                else
                {
                    elementExist[i] = false;
                    elementIndex[i] = -1;
                }
            }
        }   // check if current gcode line contains element of X/Y/Z/E
        private void GetPointValue()
        {
            for (int i = 0; i <= element.Length - 1; i++)
            {
                if (elementExist[i])
                {
                    for (int j = i + 1; j <= element.Length - 1; j++)
                    {
                        string valStr = "";
                        if (elementExist[j])    // exists next element
                        {
                            valStr = line.Substring(elementIndex[i] + 1, elementIndex[j] - elementIndex[i] - 1);
                            elementValue[i] = Double.Parse(valStr);
                            break;
                        }
                        else if (j == element.Length - 1)
                        {
                            valStr = line.Substring(elementIndex[i] + 1, line.Length - elementIndex[i] - 1);
                            elementValue[i] = Double.Parse(valStr);
                            break;
                        }
                    }
                }
            }

            if ((elementValue[0] != 0) && (elementValue[1] == 0))
            {
                Console.WriteLine(line);
                Console.WriteLine("elementValue[0] = " + elementValue[0]);
                Console.WriteLine("elementValue[1] = " + elementValue[1]);
                Console.WriteLine("This is ERROR\n");
            }
        }   // get the value of coordinate X and Y in current gcode line

        public static List<GcodeLine> ConvertFormart(int layerNr, List<string> gcodeStringSet)
        {
            List<GcodeLine> gcodeConverted = new List<GcodeLine> { };

            foreach (string thisLine in gcodeStringSet)
            {
                GcodeLine thisGcode = new GcodeLine(thisLine)
                {
                    LayerNr = layerNr
                };

                if (thisGcode.LineType == GcodeLine.LType.Gcode)
                {
                    gcodeConverted.Add(thisGcode);
                }
            }

            return gcodeConverted;
        }
    } 
}
