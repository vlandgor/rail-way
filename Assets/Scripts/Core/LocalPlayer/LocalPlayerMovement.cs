using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Core.LocalPlayer
{
    public class LocalPlayerMovement : MonoBehaviour
    {
        [Header("Spline Settings")]
        [SerializeField] private SplineContainer splineContainer;
        [SerializeField] private float speed = 5f;
        [SerializeField] private bool loop = false;

        private float _normalizedTime = 0f;
        private float _splineLength;
        private float _speedDivLength; // Cached division
        private Spline _spline;
        private Transform _splineTransform;

        private void Start()
        {
            CacheSplineData();
        }

        private void Update()
        {
            if (_spline == null)
                return;

            MoveAlongSpline();
        }

        private void CacheSplineData()
        {
            if (splineContainer == null)
                return;

            _spline = splineContainer.Spline;
            _splineTransform = splineContainer.transform;
            _splineLength = _spline.GetLength();
            _speedDivLength = speed / _splineLength;
        }

        private void MoveAlongSpline()
        {
            _normalizedTime += _speedDivLength * Time.deltaTime;

            if (_normalizedTime >= 1f)
            {
                if (loop)
                    _normalizedTime -= 1f;
                else
                {
                    _normalizedTime = 1f;
                    return; // Stop evaluating once complete
                }
            }

            // Single evaluation call
            _spline.Evaluate(_normalizedTime, out float3 pos, out float3 tan, out float3 _);

            // Transform to world space
            transform.position = _splineTransform.TransformPoint(pos);

            // Only update rotation if tangent is meaningful
            if (math.lengthsq(tan) > 0.0001f)
                transform.forward = _splineTransform.TransformDirection(tan);
        }

        // Call this if you modify the spline at runtime
        public void RefreshSplineData()
        {
            CacheSplineData();
        }
    }
}