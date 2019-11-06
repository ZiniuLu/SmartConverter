using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartConverter
{
    public struct Point
    {
        public int PointID;
        public double X;
        public double Y;
        public double Z;
    }
    public struct Curve
    {
        // Field
        public int CurveID;
        public List<Point> Points;
    }
    public struct Contour
    {
        public int LayerID;
        public List<Curve> Curves;
    }



    public class LayerContour
    {
        private int layerNr;
        private static double layerHeight = 0.2;
        private double z;
        private List<string> gcodeStringInLayer;

        private List<Point> points = new List<Point>();
        private List<Curve> curves = new List<Curve>();
        private Contour myContour;

        private int pointCounter = 0;
        private int curveCounter = 0;


        public int LayerNr
        {
            get
            {
                return layerNr;
            }
            set
            {
                if (value >= 0)
                    layerNr = value;
            }
        }
        public double Z
        {
            get
            {
                return z;
            }
        }
        public Contour MyContour
        {
            get
            {
                return myContour;
            }
            set
            {
                myContour = value;
            }
        }


        public LayerContour(int layerNumber, List<string> gcodeInCurrentLayer)
        {
            layerNr = layerNumber;
            z = layerNr * layerHeight;
            gcodeStringInLayer = gcodeInCurrentLayer;
            GetLayerContour();
        }

        private void GetLayerContour()
        {
            List<GcodeLine> gcodeInLayer = GcodeLine.ConvertFormart(layerNr, gcodeStringInLayer);

            int gcodeLineNr = gcodeInLayer.Count - 1;

            for(int c = 0; c < gcodeLineNr; c++)
            {
                //  current line contains E         ->  need current point
                //      next line contains E        ->  current point is not the last point of current curve
                //      next line doesn't contain E ->  current point is the last point of current curve
                
                //  current line doesn't contains E ->  must check the next point
                //      next line contains E        ->  current point is the first point of current curve
                //      next line doesn't contain E ->  current point can be skipped.


                if (gcodeInLayer[c].EnableE)    // exists extrude(E) in current gcode line
                {
                    Point currentPoint = new Point
                    {
                        PointID = pointCounter,
                        X = gcodeInLayer[c].X,
                        Y = gcodeInLayer[c].Y,
                        Z = z
                    };
                    points.Add(currentPoint);

                    if (pointCounter == 0)
                    {
                        Console.WriteLine("\n\tCurve " + curveCounter + "\t--------------------------------------------------");
                    }
                    //Console.WriteLine("\t\tP" + pointCounter + "\tX = " + points[pointCounter].X + "\t Y = " + points[pointCounter].Y + "\t Z = " + points[pointCounter].Z);

                    if (gcodeInLayer[c+1].EnableE)  // check the next line
                    {
                        pointCounter++;
                    }
                    else
                    {
                        Curve currentCurve = new Curve
                        {
                            CurveID = curveCounter,
                            Points = points
                        };
                        curves.Add(currentCurve);

                        pointCounter = 0;
                        points = new List<Point>();

                        curveCounter++;
                    }
                }
                else if (gcodeInLayer[c+1].EnableE)
                {
                    Point currentPoint = new Point
                    {
                        PointID = pointCounter,
                        X = gcodeInLayer[c].X,
                        Y = gcodeInLayer[c].Y,
                        Z = z
                    };
                    points.Add(currentPoint);

                    if (pointCounter == 0)
                    {
                        Console.WriteLine("\n\tCurve " + curveCounter + "\t--------------------------------------------------");
                    }
                    //Console.WriteLine("\t\tP" + pointCounter + "\tX = " + points[pointCounter].X + "\t Y = " + points[pointCounter].Y + "\t Z = " + points[pointCounter].Z);
                    pointCounter++;
                }
            }

            if(gcodeInLayer[gcodeLineNr].EnableE)   // check the last line
            {
                Point currentPoint = new Point
                {
                    PointID = pointCounter,
                    X = gcodeInLayer[gcodeLineNr].X,
                    Y = gcodeInLayer[gcodeLineNr].Y,
                    Z = z
                };
                points.Add(currentPoint);

                if (pointCounter == 0)
                {
                    Console.WriteLine("\n\tCurve " + curveCounter + "\t--------------------------------------------------");
                }
                //Console.WriteLine("\t\tP" + pointCounter + "\tX = " + points[pointCounter].X + "\t Y = " + points[pointCounter].Y + "\t Z = " + points[pointCounter].Z);

                Curve currentCurve = new Curve
                {
                    CurveID = curveCounter,
                    Points = points
                };
                curves.Add(currentCurve);
            }

            myContour.LayerID = layerNr;
            myContour.Curves = curves;
        }
    }
}
