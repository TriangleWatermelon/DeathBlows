using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(LineRenderer))]
public class PhysicsRope : MonoBehaviour
{
    [BoxGroup("Components")]
    [SerializeField] Transform startPos, endPos;

    private LineRenderer lineRenderer;
    private List<RopeSegment> ropeSegments = new List<RopeSegment>();
    [BoxGroup("Control")]
    [SerializeField] float segmentLength = 0.25f;
    [BoxGroup("Control")]
    [SerializeField] int segmentCount = 20;
    [BoxGroup("Control")]
    [SerializeField] float lineWidth = 0.2f;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        Vector3 ropeStartPoint = startPos.position;

        for (int i = 0; i < segmentCount; i++)
        {
            ropeSegments.Add(new RopeSegment(ropeStartPoint));
            ropeStartPoint.y -= segmentLength;
        }

        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
    }

    void Update()
    {
        DrawRope();
    }

    private void FixedUpdate()
    {
        Simulate();
    }

    /// <summary>
    /// Adds gravity over time to the points.
    /// </summary>
    private void Simulate()
    {
        Vector2 forceGravity = new Vector2(0f, -1f);

        for (int i = 1; i < segmentCount; i++)
        {
            RopeSegment firstSegment = ropeSegments[i];
            Vector2 velocity = firstSegment.curPos - firstSegment.oldPos;
            firstSegment.oldPos = firstSegment.curPos;
            firstSegment.curPos += velocity;
            firstSegment.curPos += forceGravity * Time.fixedDeltaTime;
            ropeSegments[i] = firstSegment;
        }

        //Change the number here to check constraints more or less each frame.
        for (int i = 0; i < 50; i++)
            ApplyConstraints();
    }

    /// <summary>
    /// Keeps the points within an acceptable distance from one another.
    /// </summary>
    private void ApplyConstraints()
    {
        //Constrant to first Point 
        RopeSegment firstSegment = ropeSegments[0];
        firstSegment.curPos = startPos.position;
        ropeSegments[0] = firstSegment;

        //Constrant to last Point 
        RopeSegment lastSegment = ropeSegments[ropeSegments.Count - 1];
        lastSegment.curPos = endPos.position;
        ropeSegments[ropeSegments.Count - 1] = lastSegment;

        for (int i = 0; i < segmentCount - 1; i++)
        {
            RopeSegment firstTempSegment = ropeSegments[i];
            RopeSegment seconfTempSegment = ropeSegments[i + 1];

            float dist = (firstTempSegment.curPos - seconfTempSegment.curPos).magnitude;
            float error = Mathf.Abs(dist - segmentLength);
            Vector2 changeDir = Vector2.zero;

            if (dist > segmentLength)
            {
                changeDir = (firstTempSegment.curPos - seconfTempSegment.curPos).normalized;
            }
            else if (dist < segmentLength)
            {
                changeDir = (seconfTempSegment.curPos - firstTempSegment.curPos).normalized;
            }

            Vector2 changeAmount = changeDir * error;
            if (i != 0)
            {
                firstTempSegment.curPos -= changeAmount * 0.5f;
                ropeSegments[i] = firstTempSegment;
                seconfTempSegment.curPos += changeAmount * 0.5f;
                ropeSegments[i + 1] = seconfTempSegment;
            }
            else
            {
                seconfTempSegment.curPos += changeAmount;
                ropeSegments[i + 1] = seconfTempSegment;
            }
        }
    }

    private void DrawRope()
    {
        Vector3[] ropePositions = new Vector3[segmentCount];
        for (int i = 0; i < segmentCount; i++)
        {
            ropePositions[i] = ropeSegments[i].curPos;
        }

        lineRenderer.positionCount = ropePositions.Length;
        lineRenderer.SetPositions(ropePositions);
    }

    public struct RopeSegment
    {
        public Vector2 curPos;
        public Vector2 oldPos;

        public RopeSegment(Vector2 pos)
        {
            curPos = pos;
            oldPos = pos;
        }
    }
}