using System.Collections.Generic;
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
        private Vector3 _directVector1;
        private Vector3 _directVector2;
        private Plane _plane;
        
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
            _supportVector = CalcSupportVector();
            
            var v = CalcDecomp();
            var dirVec1 = v.Column(0); // => direction vectors
            var dirVec2 = v.Column(1);
            var normalVec = v.Column(2);

            _directVector1 = new Vector3(dirVec1[0], dirVec1[1], dirVec1[2]);
            _directVector2 = new Vector3(dirVec2[0], dirVec2[1], dirVec2[2]);
            _normalVector = new Vector3(normalVec[0], normalVec[1], normalVec[2]);
            
            _plane.SetNormalAndPosition(_normalVector, _supportVector);
        }

        private Vector3 ProjectToPlane(Vector3 v)
        {
            Vector3 result;
            var distance = _plane.GetDistanceToPoint(v);
            var factor = Vector3.Dot((v - _supportVector), _normalVector);
            var div = Vector3.Dot(_normalVector, _normalVector);
            result = v - (factor / div)*_normalVector; 
            // TODO find better source 
            // source:https://studyflix.de/mathematik/orthogonale-projektion-1468
            
            // TODO convert 3D vectors to 2D vectors
            return result;
        }

        private Vector3 CalcSupportVector()
        {
            Vector3 center = new Vector3(0, 0, 0);
            for (int i = 0; i < _points.Count; i++)
            {
                center.x += _points[i].x; 
                center.y += _points[i].y; 
                center.z += _points[i].z;
            }
            center /= _points.Count;
            return center;
        }

        private Matrix<float> CalcDecomp()
        {
            // make a matrix out of all points which is a vector list
            var A = Matrix<float>.Build;
            float[,] vectorArray = new float[_points.Count,3];
            for (int i = 0; i < _points.Count; i++)
            {
                vectorArray[i, 0] = _points[i].x; 
                vectorArray[i, 1] = _points[i].y; 
                vectorArray[i, 2] = _points[i].z;
            }
            var a = A.DenseOfArray(vectorArray);
            var decomp = a.Svd(true);
            var v = decomp.VT.Transpose(); // returns 3x3 matrix where the first 2 colums are "Richtungvektoren" and the 3. is normal vector to plane.

            return v;
        }

        public Vector3 GetSupportVector()
        {
            return _supportVector;
        }

        public Vector3 GetNormalVector()
        {
            return _normalVector;
        }

        public Vector3 GetDirectVector1()
        {
            return _directVector1;
        }

        public Vector3 GetDirectVector2()
        {
            return _directVector2;
        }

        public Plane GetPlane()
        {
            return _plane;
        }

        public int GetNumberOfPoints()
        {
            return _points.Count;
        }
        
        // TODO use GIZMOS to test what you wrote so far!!!

        // TODO get character recognition method

    }
}