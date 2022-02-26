using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public string levelToLoad;
    public static UIManager Instance;

    public TMP_Text lapCounterText, bestLapTimeText, currentLapTimeText, posText, countdownText, goText, raceResultText;

    public GameObject ResultScreen;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ExitRace()
    {
        SceneManager.LoadScene(levelToLoad);
    }
}
