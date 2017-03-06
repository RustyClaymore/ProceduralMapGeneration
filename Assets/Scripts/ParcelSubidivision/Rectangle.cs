using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rectangle
{

    public Vector3 leftBot;
    public Vector3 rightTop;
    public Vector3 leftTop;
    public Vector3 rightBot;

    public Segment topSegment, botSegment, leftSegment, rightSegment;

    public float area;
    public Vector3 center;

    public Rectangle()
    {
        leftBot = Vector3.zero;
        rightTop = Vector3.zero;
        leftTop = Vector3.zero;
        rightBot = Vector3.zero;

        topSegment = new Segment();
        botSegment = new Segment();
        leftSegment = new Segment();
        rightSegment = new Segment();

        area = 0;
        center = Vector3.zero;
    }

    public void UpdateSegments()
    {
        topSegment.startPos = rightTop;
        topSegment.finalPos = leftTop;

        leftSegment.startPos = leftTop;
        leftSegment.finalPos = leftBot;

        botSegment.startPos = leftBot;
        botSegment.finalPos = rightBot;

        rightSegment.startPos = rightBot;
        rightSegment.finalPos = rightTop;
    }

    public Rectangle(Vector3 leftBottom, Vector3 rightTop)
    {
        this.leftBot = leftBottom;
        this.rightTop = rightTop;
        leftTop = new Vector3(leftBottom.x, 0, rightTop.z);
        rightBot = new Vector3(rightTop.x, 0, leftBottom.z);

        topSegment = new Segment();
        botSegment = new Segment();
        leftSegment = new Segment();
        rightSegment = new Segment();

        UpdateSegments();

        float centerX = (leftBot.x + rightTop.x) / 2f;
        float centerY = (leftBot.y + rightTop.y) / 2f;
        float centerZ = (leftBot.z + rightTop.z) / 2f;

        center = new Vector3(centerX, centerY, centerZ);

        area = GetArea();
    }

    public float GetArea()
    {
        float length = rightTop.x - leftBot.x;
        float width = rightTop.z - leftBot.z;

        area = length * width;
        return area;
    }
}
