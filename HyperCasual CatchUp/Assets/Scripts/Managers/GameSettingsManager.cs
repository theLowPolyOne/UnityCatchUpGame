using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettingsManager : MonoBehaviour
{
    #region FIELDS

    [Header("PERFORMANCE:")]
    [SerializeField] private int targetFPS = 60;

    #endregion

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        Application.targetFrameRate = targetFPS;
    }
}
