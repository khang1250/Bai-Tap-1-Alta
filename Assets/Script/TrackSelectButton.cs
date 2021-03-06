 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrackSelectButton : MonoBehaviour
{
    public string trackSceneName;

    public Image trackImage;

    public int raceLap = 3;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SelectTrack()
    {
        RaceInfoManager.instance.trackToLoad = trackSceneName;
        RaceInfoManager.instance.noOfLap = raceLap;
        RaceInfoManager.instance.trackSprite = trackImage.sprite;

        MainMenu.instance.trackSelectImage.sprite = trackImage.sprite;

        MainMenu.instance.CloseTrackSelect();
    }
}
