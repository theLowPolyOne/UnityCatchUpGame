using Fallencake.Entities.Characters;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : StaticInstance<GameManager>
{
    #region FIELDS

    [Header("GUI:")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private TMP_Text bestScoresLabel;
    [SerializeField] private TMP_Text currentScoresPanelLabel;
    [SerializeField] private TMP_Text currentScoresInGameLabel;
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject touchCanvas;

    [Header("Spawners:")]
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private MineSpawner mineSpawner;

    [Header("GAMEPLAY CONFIG:")]
    [SerializeField] private Player player;
    [SerializeField] private float safeTime = 5f;
    [SerializeField] private int enemyMaxSpawned = 50;
    [SerializeField] private float enemyspawnTimestep = 5f;
    [SerializeField] private int mineMaxSpawned = 100;
    [SerializeField] private float minespawnTimestep = 10f;

    public Transform Player { get => player.transform; }

    private const string bestScoresKey = "BEST_SCORES";
    private int currentScores;
    private int bestScores;

    #endregion

    #region MONO AND INITIALIZATION

    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    private void Initialize()
    {
        startButton.onClick.AddListener(StartGame);
        restartButton.onClick.AddListener(RestartGame);
        player.Health.OnDeath += HandleGameOver;
        bestScores = PlayerPrefs.GetInt(bestScoresKey);
        startPanel.SetActive(true);
        gameOverPanel.SetActive(false);
        touchCanvas.SetActive(false);
        enemySpawner.MaxSpawned = enemyMaxSpawned;
        mineSpawner.MaxSpawned = mineMaxSpawned;
        enemySpawner.SpawnTimestep = enemyspawnTimestep;
        mineSpawner.SpawnTimestep = minespawnTimestep;
    }

    private void SaveScores()
    {
        if (currentScores > bestScores)
        {
            PlayerPrefs.SetInt(bestScoresKey, currentScores);
            bestScores = currentScores;
        }
    }

    private void StartGame()
    {
        currentScores = 0;
        startPanel.SetActive(false);
        touchCanvas.SetActive(true);
        player.Movement.ReturnControl();
        StartCoroutine(StartTimer());
        StartCoroutine(ActivateSpawners());
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    #endregion

    private void HandleGameOver()
    {
        StopAllCoroutines();
        SaveScores();
        bestScoresLabel.text = bestScores.ToString();
        currentScoresPanelLabel.text = currentScores.ToString();
        StartCoroutine(ShowGameOverPanel());
    }

    private IEnumerator ShowGameOverPanel()
    {
        yield return new WaitForSeconds(1);
        gameOverPanel.SetActive(true);
    }

    private IEnumerator StartTimer()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            currentScores++;
            currentScoresInGameLabel.text = $"SCORES: {currentScores}";
        }
    }

    private IEnumerator ActivateSpawners()
    {
        yield return new WaitForSeconds(safeTime);
        enemySpawner.StartSpawning();
        mineSpawner.StartSpawning();
    }
}
