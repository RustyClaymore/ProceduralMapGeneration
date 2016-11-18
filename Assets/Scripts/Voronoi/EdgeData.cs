using UnityEngine;
using System.Collections;

public class EdgeData
{
    private Vector3 m_initialPoint;
    private Vector3 m_finalPoint;

    private int m_segmentsCount;

    public EdgeData[] m_edgeSegments;

    public EdgeData()
    {
        m_initialPoint = Vector3.zero;
        m_finalPoint = Vector3.zero;
        m_segmentsCount = 1;
    }

    public EdgeData(Vector3 initialPoint, Vector3 finalPoint, int segmentsCount)
    {
        m_initialPoint = initialPoint;
        m_finalPoint = finalPoint;
        m_segmentsCount = segmentsCount;
    }

    public void DivideEdgeIntoSegments(int numberOfSegments)
    {
        m_segmentsCount = numberOfSegments;

        Vector3 edgeDirection = m_finalPoint - m_initialPoint;

        Vector3 edgeDirectionNormalized = edgeDirection.normalized;
        float edgeMagnitude = edgeDirection.magnitude;

        float segmentMagnitude = edgeMagnitude / (float)numberOfSegments;

        m_edgeSegments = new EdgeData[numberOfSegments];

        m_edgeSegments[0] = new EdgeData();

        m_edgeSegments[0].SetInitialPoint(m_initialPoint);
        m_edgeSegments[0].SetFinalPoint(m_initialPoint + edgeDirectionNormalized * segmentMagnitude);

        for (int i = 1; i < numberOfSegments; i++)
        {
            EdgeData segment = new EdgeData(m_edgeSegments[i - 1].m_finalPoint, m_edgeSegments[i - 1].m_finalPoint + edgeDirectionNormalized * segmentMagnitude, 1);
            m_edgeSegments[i] = new EdgeData();
            m_edgeSegments[i] = segment;
        }
    }

    #region Getters/Setters
    public int GetSegmentsCount()
    {
        return m_segmentsCount;
    }

    public void SetSegmentCount(int segCount)
    {
        m_segmentsCount = segCount;
    }

    public Vector3 GetInitialPoint()
    {
        return m_initialPoint;
    }

    public void SetInitialPoint(Vector3 initPoint)
    {
        m_initialPoint = initPoint;
    }

    public Vector3 GetFinalPoint()
    {
        return m_finalPoint;
    }

    public void SetFinalPoint(Vector3 finalPoint)
    {
        m_finalPoint = finalPoint;
    }

    #endregion
}
