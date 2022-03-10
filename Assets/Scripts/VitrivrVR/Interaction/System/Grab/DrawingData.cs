using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Plane = UnityEngine.Plane;
using Vector3 = UnityEngine.Vector3;

namespace VitrivrVR.Interaction.System.Grab
{
    public class DrawingData
    {
        private List<LineRenderer> _drawnLines;
        private List<Vector3> _points; // contains from every line first, last and middle point OR random number of points
        private Plane _plane;
        
        public DrawingData()
        {
            _drawnLines = new List<LineRenderer>();
        }

        public void AddLine(LineRenderer line)
        {
            _drawnLines.Add(line);
        }

        private void SetPoint()
        {
            int numberOfPoints = 0;
            foreach (var line in _drawnLines)
            {
                numberOfPoints = line.positionCount;
                if (numberOfPoints <= 2)
                {
                    Vector3[] tmp = new Vector3[numberOfPoints];
                    line.GetPositions(tmp);
                    foreach (var t in tmp)
                    {
                        _points.Add(t);
                    }
                }
                else
                {
                    _points.Add(line.GetPosition(0));
                    _points.Add(line.GetPosition(numberOfPoints / 2));
                    _points.Add(line.GetPosition(numberOfPoints - 1));
                }
            }
        }

        private void SetPlane()
        {
            int numberOfPoints = _points.Count;
            if (numberOfPoints < 3)
            {
                return;
            }
            else
            {
                // TODO Regressionsebene durch Punktwolke
                // https://www.matheboard.de/archive/550004/thread.html
                _plane = new Plane();
            }
        }
        
        
        // TODO get random points from lines
        // TODO getPlane create plane class
        // TODO get character recognition method
        
    }
}