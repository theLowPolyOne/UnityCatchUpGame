using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject uiObject;
    public List<Vector2> points;
    private float offset = 20f;

    void SetUIObjectPosition()
    {
        // �������� ������� ��� ��������� UI ��'����
        Vector2 position = GetPositionToPlaceUI();
        uiObject.transform.position = position;
    }

    Vector2 GetPositionToPlaceUI()
    {
        float closestDistance = Mathf.Infinity;
        Vector2 closestPoint = Vector2.zero;
        float distanceToLine;
        float distanceToPoint;

        // ��������� �� ��� ������ �� �����, ��� ������ ��������� ����� ��� ��������� UI ��'����
        foreach (Vector2 point in points)
        {
            // ���������� ������� �� ������ �� UI ��'�����
            distanceToPoint = Vector2.Distance(transform.position, point);

            // ����������, �� ������������� UI ��'��� � ����-���� � �����
            if (distanceToPoint < uiObject.GetComponent<RectTransform>().rect.width / 2)
            {
                // ���� �� �������������, ��������� ������� ����� � ������ ������
                return point + Vector2.one * offset;
            }

            // ��������� �� ��� �����, ��� ������ ��������� ������� �� UI ��'����� �� �����
            for (int i = 0; i < points.Count; i++)
            {
                int j = (i + 1) % points.Count;
                distanceToLine = DistancePointLine(point, points[i], points[j]);

                // ���� ������� ����� �� ��������� ������� �� ����� �������, ��������� ��������� ������� �� �����
                if (distanceToLine < closestDistance)
                {
                    closestDistance = distanceToLine;
                    closestPoint = ClosestPointOnLineSegment(point, points[i], points[j]);
                }
            }
        }

        // ���� ���� ���������� � ����-���� � �����, ��������� ��������� ����� �� ���� � ������ ������
        return closestPoint + (closestPoint - (Vector2)transform.position).normalized * offset;
    }

    // ���������� ������� �� ������ �� ������� ���
    float DistancePointLine(Vector2 p, Vector2 v, Vector2 w)
    {
        float l2 = (v - w).sqrMagnitude;
        if (l2 == 0f) return Vector2.Distance(p, v);
        float t = Mathf.Clamp01(Vector2.Dot(p - v, w - v) / l2);
        Vector2 projection = v + t * (w - v);
        return Vector2.Distance(p, projection);
    }

    // ���������� ��������� ����� �� ������ ���� �� �����
    Vector2 ClosestPointOnLineSegment(Vector2 p, Vector2 v, Vector2 w)
    {
        float l2 = (v - w).sqrMagnitude;
        if (l2 == 0f) return v;
        float t = Mathf.Clamp01(Vector2.Dot(p - v, w - v) / l2);
        return v + t * (w - v);
    }
}