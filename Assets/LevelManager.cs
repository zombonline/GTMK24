using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class LevelManager : MonoBehaviour
{
    [SerializeField] LevelScroll levelScroll;
    [SerializeField] LevelChunk[] levelChunks;
    [SerializeField] LevelChunk statBoostLevelChunk;
    [SerializeField] LevelChunk startLevelChunk;
    [SerializeField] LevelChunk winLevelChunk;
    [SerializeField] private float statBoostCooldownInMeters = 10, totalMetersToWin;
    private float metressSinceStatBoost = 0;
    private float totalMetersTravelled = 0;
    private static bool isPaused = false, isGameOver;

    GameObject lastSpawnedChunk;
    private float cameraTop;

    [SerializeField] UnityEvent onPause, onGameOver, onContinue, onGameComplete;

    private bool winSpawned = false;

    [SerializeField] TextMeshProUGUI distanceTravelledText;

    private void Start()
    {
        cameraTop = Camera.main.ScreenToWorldPoint(new Vector2(0, Camera.main.pixelHeight)).y;
        lastSpawnedChunk = SpawnNewLevelChunk(startLevelChunk, 0);
        ContinueGame();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (isGameOver) { return; }
            if(!isPaused) 
            { 
                FindObjectOfType<FMODController>().PlayPauseSnaphot();

                PauseGame();
            }
            else 
            {
                FindObjectOfType<FMODController>().StopPauseSnapshot();
                ContinueGame(); 
            }
        }

        if(winSpawned)
        {
            if(lastSpawnedChunk.GetComponent<Collider2D>().bounds.max.y < cameraTop)
            {
                levelScroll.StopScrolling();
            }
        }
        if (isPaused || winSpawned) { return; }
        totalMetersTravelled += levelScroll.GetCurrentScrollSpeed() * Time.deltaTime;
        metressSinceStatBoost += levelScroll.GetCurrentScrollSpeed() * Time.deltaTime;
        distanceTravelledText.text = "Distance Scaled: " + totalMetersTravelled.ToString("0.00") + "\nHighest: " + PlayerPrefs.GetFloat("HIGH SCORE", 0).ToString("0.00");
        if (lastSpawnedChunk.GetComponent<Collider2D>().bounds.max.y < cameraTop)
        {
            if(totalMetersTravelled >= totalMetersToWin)
            {
                winSpawned = true;
                lastSpawnedChunk = SpawnNewLevelChunk(winLevelChunk, lastSpawnedChunk.GetComponent<Collider2D>().bounds.max.y);
                return;
            }
            if (metressSinceStatBoost >= statBoostCooldownInMeters)
            {
                metressSinceStatBoost = 0;
                lastSpawnedChunk = SpawnNewLevelChunk(statBoostLevelChunk, lastSpawnedChunk.GetComponent<Collider2D>().bounds.max.y);
            }
            else
            {
                lastSpawnedChunk = SpawnNewLevelChunk(levelChunks[Random.Range(0, levelChunks.Length)], lastSpawnedChunk.GetComponent<Collider2D>().bounds.max.y);
            }
        }

    }
    private GameObject SpawnNewLevelChunk(LevelChunk levelChunk, float yPos)
    {
        GameObject newLevelChunk = 
            Instantiate(levelChunk.gameObject, new Vector2(0, yPos) , Quaternion.identity);
        newLevelChunk.transform.SetParent(levelScroll.transform);
        return newLevelChunk;
    }

    public static bool GetIsPaused()
    {
        return isPaused;
    }
    public static bool GetIsGameOver()
    {
        return isGameOver;
    }
    public void GameOver()
    {
        if(PlayerPrefs.GetFloat("HIGH SCORE", 0) < totalMetersTravelled)
        {
            PlayerPrefs.SetFloat("HIGH SCORE", totalMetersTravelled);
        }
        isGameOver = true;
        isPaused = true;
        onGameOver.Invoke();
    }
    public void PauseGame()
    {
        isPaused = true;
        onPause.Invoke();
    }
    public void ContinueGame()
    {
        isPaused = false;
        onContinue.Invoke();
    }
    public void GameComplete()
    {
        if (PlayerPrefs.GetFloat("HIGH SCORE", 0) < totalMetersTravelled)
        {
            PlayerPrefs.SetFloat("HIGH SCORE", totalMetersTravelled);
        }
        isGameOver = true;
        isPaused = true;
        onGameComplete.Invoke();
    }
}
