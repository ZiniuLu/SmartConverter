using NXOpen;
using NXOpen.Features;
using NXOpen.GeometricUtilities;

using System;
using System.Collections.Generic;
using System.IO;

namespace SmartConverter
{
    class NX
    {
        // Field
        public static Session theSession = Session.GetSession();
        public static Part workPart = theSession.Parts.Work;
        public static Part displayPart = theSession.Parts.Display;

        private Sketch[] sketches = new Sketch[] { };
        private Sketch mySketch;
        private Extrude previousExtrude = null;

        private string name = "";
        private string path = "";
        private static List<_Contour> gcodeLineInLayer = new List<_Contour> { };
        private static List<GcodeLine> gcodeInLayer = new List<GcodeLine> { };
        private double layerHeight;

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
        public static List<_Contour> GcodeLineInLayer
        {
            set
            {
                gcodeLineInLayer = value;
            }
        }
        public double LayerHeight
        {
            set
            {
                layerHeight = value;
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


        public NX(double myLayerHeight)
        {
            layerHeight = myLayerHeight;
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

        public void BuildCurrentLayer(int layerNr, _Contour myContour)
        {
            // 3.1 create a sketch
            Console.WriteLine("\t\t\t\t3.1 create sketch in layer " + layerNr.ToString());
            CreateSketchInLayer(layerNr);
            
            // 3.2 sketch curve
            Console.WriteLine("\t\t\t\t3.2 sketch curves in layer " + layerNr.ToString());
            AddCurves(myContour);
            theSession.ActiveSketch.Deactivate(Sketch.ViewReorient.True, Sketch.UpdateLevel.Model);
            // 3.3 build the extrude using splines in current layer
            Console.WriteLine("\t\t\t\t3.3 build extrude in layer " + layerNr.ToString());
            CreateExtrude(layerNr);
        }

        private void CreateSketchInLayer(int layerNr)
        {
            double _z = layerNr * layerHeight;

            // create an empty sketch
            Point3d nxOrigin = new Point3d(0.0, 0.0, 0.0);
            Point3d curaOrigin = new Point3d(-111.5, -111.5, 0);
            Matrix3x3 wcsMatrix = workPart.WCS.CoordinateSystem.Orientation.Element;
            DatumPlane myDatumPlane = workPart.Datums.CreateFixedDatumPlane(nxOrigin, wcsMatrix);
            DisplayableObject[] objects = { myDatumPlane };
            theSession.DisplayManager.BlankObjects(objects);

            SketchInPlaceBuilder sketchBuilder = workPart.Sketches.CreateSketchInPlaceBuilder2((Sketch)null);
            sketchBuilder.PlaneOrFace.Value = myDatumPlane;
            sketchBuilder.SketchOrigin = workPart.Points.CreatePoint(curaOrigin);
            sketchBuilder.PlaneOption = Sketch.PlaneOption.Inferred;

            mySketch = (Sketch)sketchBuilder.Commit();
            sketchBuilder.Destroy();

            objects[0] = mySketch;
            theSession.DisplayManager.BlankObjects(objects);

            mySketch.SetName("Sketch_Layer_" + layerNr.ToString());
            mySketch.Activate(Sketch.ViewReorient.False);

            
            workPart.ModelingViews.WorkView.FitAfterShowOrHide(View.ShowOrHideType.HideOnly);
        }

        private void AddCurves(_Contour myContour)
        {
            foreach(_Curve myCurve in myContour.Curves)
            {
                AddLines(myCurve);
            }

            theSession.ActiveSketch.Update();
        }

        private void AddLines(_Curve myCurve)
        {
            List<_Point> points = myCurve.Points;
            points.Add(points[0]);

            for(int i = 0; i < points.Count - 1; i++)
            {
                Point3d startPoint = CreatePoint3d(points[i]);
                Point3d endPoint = CreatePoint3d(points[i + 1]);
                Line myLine = workPart.Curves.CreateLine(startPoint, endPoint);
                theSession.ActiveSketch.AddGeometry(myLine, Sketch.InferConstraintsOption.InferNoConstraints);
            }
        }

        private Point3d CreatePoint3d(_Point myPoint)
        {
            return new Point3d(myPoint.X, myPoint.Y, myPoint.Z);
        }

        private void CreateExtrude(int layerNr)
        {
            // Create Section
            NXObject[] myObjcts = mySketch.GetAllGeometry();
            Section mySection = workPart.Sections.CreateSection(0.00095, 0.001, 0.05);
            foreach(NXObject obj in myObjcts)
            {
                if(obj.GetType().ToString() == "NXOpen.Line")
                {
                    Curve[] curves = { (Curve)obj };
                    CurveDumbRule dumbrule = workPart.ScRuleFactory.CreateRuleCurveDumb(curves);
                    SelectionIntentRule[] rules = { dumbrule };
                    mySection.AddToSection(rules, null, null, null, new Point3d(0, 0, 0), Section.Mode.Create, false);
                }
            }

            // Create extrude
            Point3d origin = new Point3d(0, 0, 0);
            Vector3d axisZ = new Vector3d(0, 0, 1);
            SmartObject.UpdateOption updateOption = SmartObject.UpdateOption.DontUpdate;

            ExtrudeBuilder extrudeBuilder = workPart.Features.CreateExtrudeBuilder(null);
            extrudeBuilder.Section = mySection;
            extrudeBuilder.Direction = workPart.Directions.CreateDirection(origin, axisZ, updateOption);
            extrudeBuilder.AllowSelfIntersectingSection(true);
            extrudeBuilder.DistanceTolerance = 0.001;
            extrudeBuilder.Limits.StartExtend.Value.RightHandSide = "0";
            extrudeBuilder.Limits.EndExtend.Value.RightHandSide = layerHeight.ToString();

            Extrude myExtrude;

            if(previousExtrude != null)
            {
                extrudeBuilder.BooleanOperation.Type = BooleanOperation.BooleanType.Unite;
                //extrudeBuilder.BooleanOperation.SetBooleanOperationAndBody(BooleanOperation.BooleanType.Unite, );
                myExtrude = (Extrude)extrudeBuilder.Commit();
                extrudeBuilder.Destroy();
            }
            else
            {
                extrudeBuilder.BooleanOperation.Type = BooleanOperation.BooleanType.Create;
                myExtrude = (Extrude)extrudeBuilder.Commit();
                extrudeBuilder.Destroy();
            }

            myExtrude.SetName("Extrude_Layer_" + layerNr.ToString());

            
        }

        public static int GetUnloadOption(string dummy) { return (int)NXOpen.Session.LibraryUnloadOption.Immediately; }
    }
}
