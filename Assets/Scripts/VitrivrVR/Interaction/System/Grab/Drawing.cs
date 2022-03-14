using System.Collections.Generic;
using UnityEngine;

namespace VitrivrVR.Interaction.System.Grab
{
  public class Drawing : Grabable
  {
    public DrawingData collectData;
    public Material lineMaterial;
    public float lineWidth = 0.01f;
    public float maxSegmentDistance = 0.02f;
    public float minCornerAngle = 10;

    private LineRenderer _currentLine;
    private LineRenderer _lineGameObject;
    private float _sqrMaxSegmentDistance;

    private new void Awake()
    {
      base.Awake();
      _sqrMaxSegmentDistance = maxSegmentDistance * maxSegmentDistance;

      var go = new GameObject("DrawLine", typeof(LineRenderer));
      _lineGameObject = go.GetComponent<LineRenderer>();
      _lineGameObject.material = lineMaterial;
      _lineGameObject.widthMultiplier = lineWidth;
      _lineGameObject.numCapVertices = 4;
      _lineGameObject.numCornerVertices = 4;
      collectData = new DrawingData();
    }

    private new void Update()
    {
      base.Update();
      if (!_currentLine) return;

      // Update current position
      var numPositions = _currentLine.positionCount;
      var position = transform.position;
      _currentLine.SetPosition(numPositions - 1, position);
      // If the last position was far enough away, create new point
      var lastPosition = _currentLine.GetPosition(numPositions - 2);
      if ((lastPosition - position).sqrMagnitude > _sqrMaxSegmentDistance)
      {
        // Check if the last position can be removed
        if (numPositions > 2)
        {
          var rootPosition = _currentLine.GetPosition(numPositions - 3);
          var angle = Vector3.Angle(lastPosition - rootPosition, position - lastPosition);
          if (angle < minCornerAngle)
          {
            _currentLine.SetPosition(numPositions - 2, position);
          }
          else
          {
            _currentLine.positionCount++;
            _currentLine.SetPosition(numPositions, position);
          }
        }
        else
        {
          _currentLine.positionCount++;
          _currentLine.SetPosition(numPositions, position);
        }

        if (_currentLine.positionCount == 50)
        {
          Destroy(_currentLine);
        }
      }
    }

    private void OnDisable()
    {
      if (_currentLine)
      {
        collectData.AddLine(_currentLine);
      }
      _currentLine = null;
    }

    public override void OnInteraction(Transform interactor, bool start)
    {
      if (start)
      {
        _currentLine = Instantiate(_lineGameObject);
        var position = transform.position;
        _currentLine.SetPosition(0, position);
        _currentLine.SetPosition(1, position);
        // collectData.addLine(_currentLine);
      }
      else
      {
        _currentLine = null;
      }
    }

	private void OnDrawGizmos()
    {
        if (!collectData)
        {
            return;
        }
        Gizmos.color = Color.green;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(collectData.GetSupportVector(), new Vector3(0.1, 0.1, 0.1));
        Gizmos.DrawLine(collectData.GetSupportVector(), collectData.GetNormalVector());
    }
  }
}