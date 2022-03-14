using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;
using Plane = UnityEngine.Plane;
using Vector3 = UnityEngine.Vector3;

namespace VitrivrVR.Interaction.System.Grab
{
    public class DrawingData
    {
        private List<LineRenderer> _drawnLines;
        private List<Vector3> _points; // contains from every line first, last and middle point OR random number of points
        private Vector3 _supportVector;
        private Vector3 _normalVector;
        public Plane plane;
        
        public DrawingData()
        {
            _drawnLines = new List<LineRenderer>();
        }

        public void AddLine(LineRenderer line)
        {
            _drawnLines.Add(line);
        }

        private void SetPoints()
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
                    // or add all points
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
                FindPlane();
            }
        }

        private void FindPlane()
        {
            // make a matrix out of all points which is a vector list
            var A = Matrix<float>.Build;
            int n = _points.Count; 
            float[,] vectorArray = new float[n,3];
            Vector3 center = new Vector3(0, 0, 0);
            for (int i = 0; i < n; i++)
            {
                vectorArray[i, 0] = _points[i].x; 
                vectorArray[i, 1] = _points[i].y; 
                vectorArray[i, 2] = _points[i].z;
                center.x += _points[i].x; 
                center.y += _points[i].y; 
                center.z += _points[i].z;
            }
            center /= n;
            _supportVector = center;
            var a = A.DenseOfArray(vectorArray);
            var decomp = a.Svd(true);
            var v = decomp.VT.Transpose(); 
            // returns 3x3 matrix where the first 2 colums are "Richtungvektoren" and the 3. is normal vector to plane.
            var normal = v.Column(2);
            _normalVector = new Vector3(normal[0], normal[1], normal[2]);
            plane.SetNormalAndPosition(_normalVector, _supportVector);
        }

        private Vector3 ProjectToPlane(Vector3 v)
        {
            Vector3 result;
            var distance = plane.GetDistanceToPoint(v);
            var factor = Vector3.Dot((v - _supportVector), _normalVector);
            var div = Vector3.Dot(_normalVector, _normalVector);
            result = v - (factor / div)*_normalVector; 
            // TODO find better source 
            // source:https://studyflix.de/mathematik/orthogonale-projektion-1468
            
            // TODO convert 3D vectors to 2D vectors
            return result;
        }
        // TODO use GIZMOS to test what you wrote so far!!!

        // TODO get character recognition method

    }
}