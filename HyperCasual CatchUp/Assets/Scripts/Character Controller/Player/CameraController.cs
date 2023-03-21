using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    #region FIELDS

    [Header("FOLLOW SETTINGS:")]
    [SerializeField] private Transform target;
    [SerializeField] private float movementFollowSpeed = 5f;
    [SerializeField] private float rotationFollowSpeed = 5f;
    [SerializeField] private bool isFollowRotation;

    private Vector3 currentVelocity = Vector3.zero;

    #endregion

    private void LateUpdate()
    {
        FollowTarget();
    }

    private void FollowTarget()
    {
        //transform.position = Vector3.SmoothDamp(transform.position, target.position, 
        //    ref currentVelocity, movementFollowSpeed);
        transform.position = Vector3.Lerp(transform.position, target.position,
            Time.deltaTime * movementFollowSpeed);
        if (isFollowRotation) transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, 
            Time.deltaTime * rotationFollowSpeed);
    }
}
