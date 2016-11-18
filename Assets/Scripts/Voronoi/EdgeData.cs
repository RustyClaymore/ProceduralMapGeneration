using UnityEngine;
using System.Collections;

public class EdgeData
{
    public Vector3 m_initialPoint;
    public Vector3 m_finalPoint;

    public int m_segmentsCount;

    public EdgeData[] m_edgeSegments;

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
        Vector3 edgeDirectionNormalized = (m_finalPoint - m_initialPoint).normalized;
        float edgeMagnitude = edgeDirection.magnitude;

        float segmentMagnitude = edgeMagnitude / (float)numberOfSegments;

        for (int i = 0; i < numberOfSegments; i++)
        {
            EdgeData segment = new EdgeData(m_edgeSegments[i].m_initialPoint, m_edgeSegments[i].m_initialPoint + edgeDirectionNormalized * segmentMagnitude, 1);
            m_edgeSegments[i] = segment;
        }
    }

}
