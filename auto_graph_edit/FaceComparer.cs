using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DrawingEngine.Drawing3D;

namespace DrawingEngine.Sort
{
    class FaceComparer:IComparer<Face>
    {
        public int Compare(Face face1, Face face2)
        {
            Point3D camPos = SortObjects.cameraPosition;
            
            double avgDist1 = ((new Vector3D(camPos, face1.point1).Length() + new Vector3D(camPos, face1.point2).Length() + new Vector3D(camPos, face1.point3).Length()) / 3);
            double avgDist2 = ((new Vector3D(camPos, face2.point1).Length() + new Vector3D(camPos, face2.point2).Length() + new Vector3D(camPos, face2.point3).Length()) / 3);
            return (avgDist1 > avgDist2) ? -1 : (avgDist1 < avgDist2) ? 1 : 0; 

        }
    }
}
