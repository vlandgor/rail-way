using System;
using Game.Rail;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Game.Player.Behaviour
{
    public class LocalPlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float speed = 5f;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0.1f, 0f);

        private SplineContainer _splineContainer;
        private Transform _splineTransform;
        private Spline _spline;
        
        private bool _isMoving = false;
        private float _normalizedTime = 0f;
        private float _segmentLength;
        private float _speedDivLength;
        
        private RailSegment _currentSegment;
        private Action<int> _onReachedDestination;
        
        private float _startT;
        private float _endT;

        private void Update()
        {
            if (!_isMoving || _spline == null)
                return;

            MoveAlongSegment();
        }

        public void MoveAlongSegment(RailSegment segment, SplineContainer container, Action<int> onComplete)
        {
            _currentSegment = segment;
            _splineContainer = container;
            _spline = container.Splines[segment.SplineIndex];
            _splineTransform = container.transform;
            _onReachedDestination = onComplete;
            
            // Convert knot indices to normalized t values (0-1)
            _startT = _spline.ConvertIndexUnit(segment.StartKnotIndex, PathIndexUnit.Knot, PathIndexUnit.Normalized);
            _endT = _spline.ConvertIndexUnit(segment.EndKnotIndex, PathIndexUnit.Knot, PathIndexUnit.Normalized);
            
            // Calculate actual arc length between these knots
            float startDistance = _spline.ConvertIndexUnit(segment.StartKnotIndex, PathIndexUnit.Knot, PathIndexUnit.Distance);
            float endDistance = _spline.ConvertIndexUnit(segment.EndKnotIndex, PathIndexUnit.Knot, PathIndexUnit.Distance);
            _segmentLength = Mathf.Abs(endDistance - startDistance);
            
            _speedDivLength = speed / _segmentLength;
            
            _normalizedTime = 0f;
            _isMoving = true;
        }

        private void MoveAlongSegment()
        {
            _normalizedTime += _speedDivLength * Time.deltaTime;

            if (_normalizedTime >= 1f)
            {
                _normalizedTime = 1f;
                _isMoving = false;
                
                EvaluatePosition(_normalizedTime);
                
                _onReachedDestination?.Invoke(_currentSegment.EndStopPointId);
                return;
            }

            EvaluatePosition(_normalizedTime);
        }

        private void EvaluatePosition(float t)
        {
            float splineT = Mathf.Lerp(_startT, _endT, t);

            _spline.Evaluate(splineT, out float3 pos, out float3 tan, out float3 up);

            Vector3 worldPos = _splineTransform.TransformPoint(pos);
            
            // Apply offset relative to the spline's orientation
            Vector3 worldUp = _splineTransform.TransformDirection(up);
            Vector3 tangentDir = _splineTransform.TransformDirection(tan);
            Vector3 right = Vector3.Cross(tangentDir, worldUp).normalized;
            
            // Apply offset in spline-local space
            worldPos += worldUp * offset.y;
            worldPos += right * offset.x;
            worldPos += tangentDir * offset.z;
            
            transform.position = worldPos;

            if (math.lengthsq(tan) > 0.0001f)
            {
                // Flip direction if going backward
                if (_currentSegment.Direction < 0)
                    tangentDir = -tangentDir;
                transform.forward = tangentDir;
            }
        }

        public bool IsMoving => _isMoving;

        public void StopMovement()
        {
            _isMoving = false;
            _onReachedDestination = null;
        }
    }
}