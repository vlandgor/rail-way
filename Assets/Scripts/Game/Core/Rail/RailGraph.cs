using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Game.Core.Rail
{
    public class RailGraph : MonoBehaviour
    {
        [SerializeField] private SplineContainer splineContainer;
        
        private Dictionary<int, StopPoint> _stopPoints = new();
        public  Dictionary<int, StopPoint> StopPoints => _stopPoints;
        
        private List<RailSegment> _segments = new();
        private Dictionary<int, List<RailSegment>> _segmentsAtStop = new();
        
        private int _nextStopPointId = 0;
        
        private void Awake()
        {
            BuildRailNetwork();
        }
        
        private void BuildRailNetwork()
        {
            if (splineContainer == null) return;
            
            DetectStopPoints();
            CreateSegments();
        }
        
        private void DetectStopPoints()
        {
            var linkCollection = splineContainer.KnotLinkCollection;
            var processed = new HashSet<string>();
            
            for (int splineIndex = 0; splineIndex < splineContainer.Splines.Count; splineIndex++)
            {
                var spline = splineContainer.Splines[splineIndex];
                
                for (int knotIndex = 0; knotIndex < spline.Count; knotIndex++)
                {
                    string knotKey = $"{splineIndex}_{knotIndex}";
                    if (processed.Contains(knotKey)) continue;
                    
                    var currentKnotIndex = new SplineKnotIndex(splineIndex, knotIndex);
                    IReadOnlyList<SplineKnotIndex> linkedKnots = linkCollection.GetKnotLinks(currentKnotIndex);
                    
                    bool isLink = linkedKnots != null && linkedKnots.Count > 1;
                    bool isSplineEnd = knotIndex == 0 || knotIndex == spline.Count - 1;
                    
                    if (isLink || isSplineEnd)
                    {
                        var knot = spline[knotIndex];
                        Vector3 worldPos = splineContainer.transform.TransformPoint(knot.Position);
                        var knotsAtStop = new List<KnotInfo>();
                        
                        if (linkedKnots != null && linkedKnots.Count > 1)
                        {
                            Vector3 sumPos = Vector3.zero;
                            int count = 0;
                            
                            foreach (var linkedKnot in linkedKnots)
                            {
                                var linkedSpline = splineContainer.Splines[linkedKnot.Spline];
                                var linkedKnotData = linkedSpline[linkedKnot.Knot];
                                Vector3 linkedWorldPos = splineContainer.transform.TransformPoint(linkedKnotData.Position);
                                
                                sumPos += linkedWorldPos;
                                count++;
                                
                                knotsAtStop.Add(new KnotInfo
                                {
                                    SplineIndex = linkedKnot.Spline,
                                    KnotIndex = linkedKnot.Knot,
                                    WorldPosition = linkedWorldPos
                                });
                                
                                processed.Add($"{linkedKnot.Spline}_{linkedKnot.Knot}");
                            }
                            worldPos = sumPos / count;
                        }
                        else
                        {
                            knotsAtStop.Add(new KnotInfo
                            {
                                SplineIndex = splineIndex,
                                KnotIndex = knotIndex,
                                WorldPosition = worldPos
                            });
                            processed.Add(knotKey);
                        }
                        
                        int stopPointId = _nextStopPointId++;
                        var stopPoint = new StopPoint
                        {
                            Id = stopPointId,
                            WorldPosition = worldPos,
                            IsLink = isLink,
                            IsSplineEnd = isSplineEnd,
                            Knots = knotsAtStop
                        };
                        
                        _stopPoints[stopPointId] = stopPoint;
                    }
                    else
                    {
                        processed.Add(knotKey);
                    }
                }
            }
        }
        
        private void CreateSegments()
        {
            foreach (var startStop in _stopPoints.Values)
            {
                foreach (var startKnot in startStop.Knots)
                {
                    var spline = splineContainer.Splines[startKnot.SplineIndex];
                    
                    if (startKnot.KnotIndex < spline.Count - 1)
                    {
                        WalkAndCreateSegment(startKnot, startStop.Id, 1);
                    }
                    
                    if (startKnot.KnotIndex > 0)
                    {
                        WalkAndCreateSegment(startKnot, startStop.Id, -1);
                    }
                    
                    if (spline.Closed)
                    {
                        if (startKnot.KnotIndex == 0)
                            WalkAndCreateSegment(startKnot, startStop.Id, -1);
                        if (startKnot.KnotIndex == spline.Count - 1)
                            WalkAndCreateSegment(startKnot, startStop.Id, 1);
                    }
                }
            }
        }
        
        private void WalkAndCreateSegment(KnotInfo startKnot, int startStopId, int direction)
        {
            var spline = splineContainer.Splines[startKnot.SplineIndex];
            int currentKnot = startKnot.KnotIndex + direction;
            
            while (currentKnot >= 0 && currentKnot < spline.Count)
            {
                var stopPoint = FindStopPointAtKnot(startKnot.SplineIndex, currentKnot);
                
                if (stopPoint != null)
                {
                    var segment = new RailSegment
                    {
                        SplineIndex = startKnot.SplineIndex,
                        StartKnotIndex = startKnot.KnotIndex,
                        EndKnotIndex = currentKnot,
                        StartStopPointId = startStopId,
                        EndStopPointId = stopPoint.Id,
                        Direction = direction
                    };
                    
                    segment.InitializeDirection(splineContainer, startKnot.SplineIndex, direction);
                    _segments.Add(segment);
                    
                    if (!_segmentsAtStop.ContainsKey(startStopId))
                        _segmentsAtStop[startStopId] = new List<RailSegment>();
                    _segmentsAtStop[startStopId].Add(segment);
                    return;
                }
                
                currentKnot += direction;
            }
        }
        
        private StopPoint FindStopPointAtKnot(int splineIndex, int knotIndex)
        {
            foreach (var stop in _stopPoints.Values)
            {
                if (stop.Knots.Any(k => k.SplineIndex == splineIndex && k.KnotIndex == knotIndex))
                    return stop;
            }
            return null;
        }
        
        public Vector3 GetStopPointPosition(int stopPointId)
        {
            if (!_stopPoints.ContainsKey(stopPointId)) return Vector3.zero;
            return _stopPoints[stopPointId].WorldPosition;
        }

        public RailSegment GetNextSegment(int currentStopPointId, Vector2Int inputDirection, Vector3 playerForward)
        {
            if (!_segmentsAtStop.ContainsKey(currentStopPointId)) return null;
            
            var availableSegments = _segmentsAtStop[currentStopPointId];
            
            Quaternion referenceRotation = Quaternion.LookRotation(playerForward, Vector3.up);
            Vector3 worldSpaceInput = referenceRotation * new Vector3(inputDirection.x, 0, inputDirection.y);

            RailSegment bestMatch = null;
            float bestDot = -1f;
            
            foreach (var segment in availableSegments)
            {
                Vector3 segmentDir = segment.GetDirection();
                float dot = Vector3.Dot(segmentDir.normalized, worldSpaceInput.normalized);
                
                if (dot > bestDot)
                {
                    bestDot = dot;
                    bestMatch = segment;
                }
            }
            
            return bestMatch;
        }
        
        public SplineContainer GetSplineContainer() => splineContainer;
    }

    public class KnotInfo
    {
        public int SplineIndex;
        public int KnotIndex;
        public Vector3 WorldPosition;
    }
    
    public class StopPoint
    {
        public int Id;
        public Vector3 WorldPosition;
        public bool IsLink;
        public bool IsSplineEnd;
        public List<KnotInfo> Knots;
    }
    
    [System.Serializable]
    public class RailSegment
    {
        public int SplineIndex;
        public int StartKnotIndex;
        public int EndKnotIndex;
        public int StartStopPointId;
        public int EndStopPointId;
        public int Direction;
        
        private Vector3 _direction;
        
        public void InitializeDirection(SplineContainer container, int splineIndex, int direction)
        {
            var spline = container.Splines[splineIndex];
            
            float startT = (float)StartKnotIndex / (spline.Count - 1);
            float endT = (float)EndKnotIndex / (spline.Count - 1);
            float sampleT = Mathf.Lerp(startT, endT, 0.1f);
            
            spline.Evaluate(sampleT, out float3 _, out float3 tangent, out float3 _);
            _direction = container.transform.TransformDirection(tangent);
            
            if (direction < 0) _direction = -_direction;
        }
        
        public Vector3 GetDirection() => _direction;
    }
}