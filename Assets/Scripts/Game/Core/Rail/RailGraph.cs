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
            Debug.Log("[RailMap] Awake - Building rail network...");
            BuildRailNetwork();
        }
        
        private void BuildRailNetwork()
        {
            if (splineContainer == null)
            {
                Debug.LogError("[RailMap] SplineContainer is null! Please assign it in the Inspector.");
                return;
            }
    
            Debug.Log($"[RailMap] SplineContainer has {splineContainer.Splines.Count} splines");
            
            DetectStopPoints();
            CreateSegments();
    
            Debug.Log($"[RailMap] BuildRailNetwork complete. {_stopPoints.Count} stop points (links + ends), {_segments.Count} total segments.");
        }
        
        private void DetectStopPoints()
        {
            Debug.Log("[RailMap] Detecting links and spline ends using KnotLinkCollection...");
            
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
                    
                    // Get all linked knots using Unity's built-in system
                    IReadOnlyList<SplineKnotIndex> linkedKnots = linkCollection.GetKnotLinks(currentKnotIndex);
                    
                    bool isLink = linkedKnots != null && linkedKnots.Count > 1;
                    bool isSplineEnd = knotIndex == 0 || knotIndex == spline.Count - 1;
                    
                    // ONLY create stop point if it's a link OR a spline end
                    if (isLink || isSplineEnd)
                    {
                        // Get world position from actual knot data (not interpolated)
                        var knot = spline[knotIndex];
                        Vector3 worldPos = splineContainer.transform.TransformPoint(knot.Position);
                        
                        // Create list of all knots at this position
                        var knotsAtStop = new List<KnotInfo>();
                        
                        if (linkedKnots != null && linkedKnots.Count > 1)
                        {
                            // This is a link - add all linked knots
                            // Use average position of all linked knots
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
                                
                                Debug.Log($"[RailMap] Linked knot: Spline {linkedKnot.Spline}, Knot {linkedKnot.Knot} at {linkedWorldPos}");
                            }
                            
                            // Use average position for the link
                            worldPos = sumPos / count;
                            Debug.Log($"[RailMap] Link average position calculated: {worldPos}");
                        }
                        else
                        {
                            // Just a spline end
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
                        
                        string typeStr = stopPoint.IsLink ? "LINK (junction)" : "SPLINE END";
                        Debug.Log($"[RailMap] Stop point {stopPointId} at {stopPoint.WorldPosition}: {typeStr}, {knotsAtStop.Count} knot(s) linked");
                    }
                    else
                    {
                        processed.Add(knotKey);
                    }
                }
            }
            
            Debug.Log($"[RailMap] Found {_stopPoints.Values.Count(sp => sp.IsLink)} links and {_stopPoints.Values.Count(sp => sp.IsSplineEnd && !sp.IsLink)} spline ends");
        }
        
        private void CreateSegments()
        {
            Debug.Log("[RailMap] Creating rail segments...");
            
            foreach (var startStop in _stopPoints.Values)
            {
                foreach (var startKnot in startStop.Knots)
                {
                    var spline = splineContainer.Splines[startKnot.SplineIndex];
                    
                    // Try forward direction
                    if (startKnot.KnotIndex < spline.Count - 1)
                    {
                        WalkAndCreateSegment(startKnot, startStop.Id, 1);
                    }
                    
                    // Try backward direction
                    if (startKnot.KnotIndex > 0)
                    {
                        WalkAndCreateSegment(startKnot, startStop.Id, -1);
                    }
                    
                    // Handle closed splines
                    if (spline.Closed)
                    {
                        if (startKnot.KnotIndex == 0)
                            WalkAndCreateSegment(startKnot, startStop.Id, -1);
                        if (startKnot.KnotIndex == spline.Count - 1)
                            WalkAndCreateSegment(startKnot, startStop.Id, 1);
                    }
                }
            }
            
            Debug.Log($"[RailMap] Created {_segments.Count} segments");
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
                    
                    Debug.Log($"[RailMap] Created segment: stop {startStopId} -> {stopPoint.Id} (spline {startKnot.SplineIndex}, knots {startKnot.KnotIndex}->{currentKnot})");
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
            Debug.Log($"[RailMap] GetStopPointPosition called for stop {stopPointId}");
            
            if (!_stopPoints.ContainsKey(stopPointId))
            {
                Debug.LogError($"[RailMap] Stop point {stopPointId} not found!");
                return Vector3.zero;
            }
            
            var pos = _stopPoints[stopPointId].WorldPosition;
            Debug.Log($"[RailMap] Stop point {stopPointId} position: {pos}");
            return pos;
        }
        
        public Vector3 GetStopPointTangent(int stopPointId)
        {
            Debug.Log($"[RailMap] GetStopPointTangent called for stop {stopPointId}");
            
            if (!_segmentsAtStop.ContainsKey(stopPointId) || _segmentsAtStop[stopPointId].Count == 0)
            {
                Debug.LogWarning($"[RailMap] No segments at stop {stopPointId}");
                return Vector3.forward;
            }
            
            var firstSegment = _segmentsAtStop[stopPointId][0];
            var tangent = firstSegment.GetDirection();
            
            Debug.Log($"[RailMap] Stop point {stopPointId} tangent: {tangent}");
            return tangent;
        }
        
        public RailSegment GetNextSegment(int currentStopPointId, Vector2Int direction)
        {
            Debug.Log($"[RailMap] GetNextSegment called: stop={currentStopPointId}, direction={direction}");
            
            if (!_segmentsAtStop.ContainsKey(currentStopPointId))
            {
                Debug.LogWarning($"[RailMap] No segments at stop point {currentStopPointId}");
                return null;
            }
            
            var availableSegments = _segmentsAtStop[currentStopPointId];
            Debug.Log($"[RailMap] Found {availableSegments.Count} segments at stop {currentStopPointId}");
            
            RailSegment bestMatch = null;
            float bestDot = -1f;
            
            foreach (var segment in availableSegments)
            {
                Vector3 segmentDir = segment.GetDirection();
                Vector3 inputDir = new Vector3(direction.x, 0, direction.y);
                
                float dot = Vector3.Dot(segmentDir.normalized, inputDir.normalized);
                Debug.Log($"[RailMap] Segment {segment.StartStopPointId}->{segment.EndStopPointId}: direction={segmentDir}, dot={dot}");
                
                if (dot > bestDot)
                {
                    bestDot = dot;
                    bestMatch = segment;
                }
            }
            
            if (bestMatch != null)
            {
                Debug.Log($"[RailMap] Best match: {bestMatch.StartStopPointId}->{bestMatch.EndStopPointId} (dot={bestDot})");
            }
            
            return bestMatch;
        }
        
        public SplineContainer GetSplineContainer()
        {
            return splineContainer;
        }
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
            
            if (direction < 0)
                _direction = -_direction;
            
            Debug.Log($"[RailSegment] Segment {StartStopPointId}->{EndStopPointId} initialized with direction: {_direction}");
        }
        
        public Vector3 GetDirection()
        {
            return _direction;
        }
    }
}