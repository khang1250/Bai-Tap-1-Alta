using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CpChecker : MonoBehaviour
{
    public CarController theCar;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "CheckPoint")
        {
            //Debug.Log("Hit Cp" + other.GetComponent<CheckPoints>().cpNumber);

            theCar.CheckpointHit(other.GetComponent<CheckPoints>().cpNumber);

        }
    }
}
