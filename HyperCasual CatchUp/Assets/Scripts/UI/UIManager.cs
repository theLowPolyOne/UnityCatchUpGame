using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject uiObject;
    public List<Vector2> points;
    private float offset = 20f;

    void SetUIObjectPosition()
    {
        // Отримуємо позицію для розміщення UI об'єкта
        Vector2 position = GetPositionToPlaceUI();
        uiObject.transform.position = position;
    }

    Vector2 GetPositionToPlaceUI()
    {
        float closestDistance = Mathf.Infinity;
        Vector2 closestPoint = Vector2.zero;
        float distanceToLine;
        float distanceToPoint;

        // Ітеруємося по всіх точках та лініях, щоб знайти найближчу точку для розміщення UI об'єкта
        foreach (Vector2 point in points)
        {
            // Обчислюємо відстань між точкою та UI об'єктом
            distanceToPoint = Vector2.Distance(transform.position, point);

            // Перевіряємо, чи перекривається UI об'єкт з будь-якою з точок
            if (distanceToPoint < uiObject.GetComponent<RectTransform>().rect.width / 2)
            {
                // Якщо він перекривається, повертаємо позицію точки з деякою зсувом
                return point + Vector2.one * offset;
            }

            // Ітеруємося по всіх лініях, щоб знайти найближчу відстань між UI об'єктом та лінією
            for (int i = 0; i < points.Count; i++)
            {
                int j = (i + 1) % points.Count;
                distanceToLine = DistancePointLine(point, points[i], points[j]);

                // Якщо відстань менша за найближчу відстань до цього моменту, оновлюємо найближчу відстань та точку
                if (distanceToLine < closestDistance)
                {
                    closestDistance = distanceToLine;
                    closestPoint = ClosestPointOnLineSegment(point, points[i], points[j]);
                }
            }
        }

        // Якщо немає перекриття з будь-якою з точок, повертаємо найближчу точку на лінії з деяким зсувом
        return closestPoint + (closestPoint - (Vector2)transform.position).normalized * offset;
    }

    // Обчислюємо відстань між точкою та відрізком ліні
    float DistancePointLine(Vector2 p, Vector2 v, Vector2 w)
    {
        float l2 = (v - w).sqrMagnitude;
        if (l2 == 0f) return Vector2.Distance(p, v);
        float t = Mathf.Clamp01(Vector2.Dot(p - v, w - v) / l2);
        Vector2 projection = v + t * (w - v);
        return Vector2.Distance(p, projection);
    }

    // Обчислюємо найближчу точку на відрізку лінії до точки
    Vector2 ClosestPointOnLineSegment(Vector2 p, Vector2 v, Vector2 w)
    {
        float l2 = (v - w).sqrMagnitude;
        if (l2 == 0f) return v;
        float t = Mathf.Clamp01(Vector2.Dot(p - v, w - v) / l2);
        return v + t * (w - v);
    }
}