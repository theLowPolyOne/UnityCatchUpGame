using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwayingController : MonoBehaviour
{
    #region FIELDS

    [Header("COMPONENTS:")]
    [SerializeField] private Transform _targetTransform;
    [SerializeField] private Transform _targetBody;

    [Header("MOVEMENT PARAMETERS:")]
    [SerializeField] private Vector3 scaleDown = new Vector3(1.1f, 0.9f, 1.1f);
    [SerializeField] private Vector3 scaleUp = new Vector3(0.9f, 1.1f, 0.9f);

    [SerializeField] private float scaleFactor = 2f;
    [SerializeField] private float rotationFactor = 50f;

    #endregion

    private void Update()
    {
        DeformTransform();
    }

    //Applies Squash and Stretch like effect deforming the transform component
    private void DeformTransform()
    {
        Vector3 relativePosition = _targetTransform.InverseTransformPoint(transform.position);
        float interpolant = relativePosition.y * scaleFactor;
        Vector3 scale = Lerp3(scaleDown, Vector3.one, scaleUp, interpolant);
        _targetBody.localScale = scale;
        _targetBody.localEulerAngles = new Vector3(relativePosition.z, 0, -relativePosition.x) * rotationFactor;
    }

    //Calculates a linear interpolation between three vectors
    //representing the 3 scale states of the transform component
    private Vector3 Lerp3(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        if (t < 0)
        {
            return Vector3.LerpUnclamped(a, b, t + 1f);
        }
        else
        {
            return Vector3.LerpUnclamped(b, c, t);
        }
    }
}