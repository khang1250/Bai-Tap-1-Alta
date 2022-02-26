using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RaceManager : MonoBehaviour
{
    public static RaceManager instance;

    public CheckPoints[] allCheckpoint;

    public int totalLaps;

    public CarController playerCar;
    public List<CarController> allAiCar = new List<CarController>();
    public int playerPosition;
    public float timeBetweenPosCheck = .2f;
    private float posCheckCounter;

    //public float aiDefaultSpeed = 30f, playerDefaultSpeed = 30f, rubberBandSpdMod = 3.5f, rubberBandAccel = .5f;

    public bool isStarting;
    public float timeBetweenStartCount = 1f;
    private float startCounter;
    public int countdownCurrent = 3;

    public int playerStartPos, aiNumberToSpawn;
    public Transform[] startPoints;
    public List<CarController> carToSpawn = new List<CarController>();

    public bool raceCompleted;

    public string raceCompleteScene;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update   
    void Start()
    {
        totalLaps = RaceInfoManager.instance.noOfLap;
       

        for ( int i = 0; i< allCheckpoint.Length; i++ )
        {
            allCheckpoint[i].cpNumber = i;
        }

        isStarting = true;
        startCounter = timeBetweenStartCount;

        UIManager.Instance.countdownText.text = countdownCurrent + "!";

        playerStartPos = Random.Range(0, aiNumberToSpawn + 1);

        playerCar = Instantiate(RaceInfoManager.instance.racerToUse, startPoints[playerStartPos].position, startPoints[playerStartPos].rotation);
        playerCar.isAI = false;
 
        CameraController.instance.SeTarget(playerCar);
        //playerCar.transform.position = startPoints[playerStartPos].position;
        //playerCar.rb.transform.position = startPoints[playerStartPos].position;

        for ( int i = 0; i < aiNumberToSpawn + 1; i++)
        {
            if(i != playerStartPos)
            {
                int selectedCar = Random.Range(0, carToSpawn.Count);

                allAiCar.Add(Instantiate(carToSpawn[selectedCar], startPoints[i].position, startPoints[i].rotation)); 

                carToSpawn.RemoveAt(selectedCar);
            }
        }

        UIManager.Instance.posText.text = playerStartPos + 1 + "/" + (allAiCar.Count + 1);

    }

    // Update is called once per frame
    void Update()
    {
        if (isStarting)
        {
            startCounter -= Time.deltaTime;
            if(startCounter <= 0)
            {
                countdownCurrent--;
                startCounter = timeBetweenStartCount;

                UIManager.Instance.countdownText.text = countdownCurrent + "!";

                if (countdownCurrent == 0)
                {
                    isStarting=false;

                    UIManager.Instance.countdownText.gameObject.SetActive(false);
                    UIManager.Instance.goText.gameObject.SetActive(true);
                }
            }
        }
        else
        {

            posCheckCounter -= Time.deltaTime;
            if(posCheckCounter <= 0)
            {
                playerPosition = 1;

                foreach(CarController aiCar in allAiCar)
                {
                    if(aiCar.currentLap > playerCar.currentLap)
                    {
                        playerPosition++;
                    }else if(aiCar.currentLap == playerCar.currentLap)
                    {
                        if(aiCar.nextCheckpoint > playerCar.nextCheckpoint)
                        {
                            playerPosition++;
                        }
                    } else if(aiCar.nextCheckpoint == playerCar.nextCheckpoint)
                    {
                        if (Vector3.Distance(aiCar.transform.position, allCheckpoint[aiCar.nextCheckpoint].transform.position) < Vector3.Distance(playerCar.transform.position, allCheckpoint[aiCar.nextCheckpoint].transform.position))
                        {
                            playerPosition++;
                        }
                    }
                }

                posCheckCounter = timeBetweenPosCheck;

                UIManager.Instance.posText.text = playerPosition + "/" + (allAiCar.Count + 1);
            }

        }
    }

    public void FinishRace()
    {
        raceCompleted = true;

        switch (playerPosition)
        {
            case 1:
                UIManager.Instance.raceResultText.text = "You Finished 1st";
                break;
            case 2:
                UIManager.Instance.raceResultText.text = "You Finished 2nd";
                break;
            case 3:
                UIManager.Instance.raceResultText.text = "You Finished 3rd";
                break;

            default:
                UIManager.Instance.raceResultText.text = "You Finished" + playerPosition + "th";
                break;
        }


        UIManager.Instance.ResultScreen.SetActive(true);      
    }
    public void ExitRace()
    {
        SceneManager.LoadScene(raceCompleteScene); 
    }
}
