using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public Rigidbody rb;

    public float maxSpeed;

    public float forwardAccel = 8f, reverseAccel = 4f;

    private float speedInput;

    public float turnStrength = 180f;
    private float turnInput;

    private bool grounded;

    public Transform groundRay, groundRay2;
    public LayerMask isGround;
    public float groundRayLength = 2.25f;

    private float dragOnGround;
    public float gravity = 10f;
    
    public Transform leftFrontWheel, rightFrontWheel;
    public float maxWheelTurn = 25f;

    public ParticleSystem[] dustTrail;
    public float maxEmission = 25f, emissionFadeSpeed = 20f;
    private float emissionRate;

    public int nextCheckpoint;
    public int currentLap;

    public float lapTime, bestLapTime;

    public bool isAI;

    public int currentTarget;

    private Vector3 targetPoint;

    public float aiAccelerateSpeed = 1f, aiTurnSpeed = .8f, aiReachPointRange = 5f, aiPointVariant = 3f, aiMaxTurn = 15f;

    private float aiSpeedInput, aiSpeedMod;

    // Start is called before the first frame update
    void Start()
    {
        rb.transform.parent = null;

        dragOnGround = rb.drag;

        if (isAI)
        {
            targetPoint = RaceManager.instance.allCheckpoint[currentTarget].transform.position;
            RandomAITarget();

            aiSpeedMod = Random.Range(1f, 2f);
        }

        emissionRate = 100f;

        UIManager.Instance.lapCounterText.text = currentLap + "/" + RaceManager.instance.totalLaps;
    }

    // Update is called once per frame
    void Update()
    {
        if (!RaceManager.instance.isStarting)
        {
            lapTime += Time.deltaTime;

            if (!isAI)
            {
                var ts = System.TimeSpan.FromSeconds(lapTime);
                UIManager.Instance.currentLapTimeText.text = string.Format("{0:00}m{1:00}.{2:000}s", ts.Minutes, ts.Seconds, ts.Milliseconds);

                speedInput = 0f;
                if (Input.GetAxis("Vertical") > 0)
                {
                    speedInput = Input.GetAxis("Vertical") * forwardAccel;
                }
                else if (Input.GetAxis("Vertical") < 0)
                {
                    speedInput = Input.GetAxis("Vertical") * reverseAccel;
                }



                turnInput = Input.GetAxis("Horizontal");

                /*if(grounded && Input.GetAxis("Vertical") != 0)
                {
                    transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, turnInput * turnStrength * Time.deltaTime * Mathf.Sign(speedInput) * (rb.velocity.magnitude / maxSpeed), 0f)); 
                }*/

                if (Input.GetKeyDown(KeyCode.R))
                {
                    ResetToTrack(); 
                }
            }
            else
            {
                targetPoint.y = transform.position.y;

                if (Vector3.Distance(transform.position, targetPoint) < aiReachPointRange)
                {
                    SetNextAiTarget();

                }

                Vector3 targetDire = targetPoint - transform.position;
                float angle = Vector3.Angle(targetDire, transform.forward);

                Vector3 localPos = transform.InverseTransformPoint(targetPoint);
                if (localPos.x < 0f)
                {
                    angle = -angle;
                }

                turnInput = Mathf.Clamp(angle / aiMaxTurn, -1f, 1f);

                if (Mathf.Abs(angle) < aiMaxTurn)
                {
                    aiSpeedInput = Mathf.MoveTowards(aiSpeedInput, 1f, aiAccelerateSpeed);
                }
                else
                {
                    aiSpeedInput = Mathf.MoveTowards(aiSpeedInput, aiTurnSpeed, aiAccelerateSpeed);
                }

                aiSpeedInput = 1f;
                speedInput = aiSpeedInput * forwardAccel * aiSpeedMod;
            }




            //turning the wheel
            leftFrontWheel.localRotation = Quaternion.Euler(leftFrontWheel.localRotation.eulerAngles.x, (turnInput * maxWheelTurn) - 180, leftFrontWheel.localRotation.eulerAngles.z);
            rightFrontWheel.localRotation = Quaternion.Euler(rightFrontWheel.eulerAngles.x, (turnInput * maxWheelTurn), rightFrontWheel.localRotation.eulerAngles.z);



            //transform.position = rb.position;

            if (grounded && (Mathf.Abs(turnInput) > .5f || (rb.velocity.magnitude < maxSpeed * .5f && rb.velocity.magnitude != 0)))
            {
                emissionRate = maxEmission;
            }

            if (rb.velocity.magnitude <= .5f)
            {
                emissionRate = 0;
            }

            //control particle emissions
            emissionRate = Mathf.MoveTowards(emissionRate, 0f, emissionFadeSpeed * Time.deltaTime);

            for (int i = 0; i < dustTrail.Length; i++)
            {
                var emissionModule = dustTrail[i].emission;

                emissionModule.rateOverTime = emissionRate;
            }
        } 
    }

    private void FixedUpdate()
    {
        grounded = false;

        RaycastHit hit;
        Vector3 normalTarget = Vector3.zero;

        if(Physics.Raycast(groundRay.position, -transform.up, out hit, groundRayLength, isGround))
        {
            grounded = true; 

            normalTarget = hit.normal;
        }

        if(Physics.Raycast(groundRay2.position, -transform.up, out hit, groundRayLength, isGround))
        {
            grounded = true;

            normalTarget = (normalTarget + hit.normal) / 2f;
        }

        //when on ground rotate to match normal
        if (grounded)
        {
            transform.rotation = Quaternion.FromToRotation(transform.up, normalTarget) *  transform.rotation;
        }


        //accelerate
        if (grounded)
        {
            rb.AddForce(transform.forward * speedInput * 1000f);

            rb.drag = dragOnGround;
        }
        else
        {
            rb.drag = .1f;

            rb.AddForce(Vector3.up * -gravity * 100f);
        }

        if(rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }

        transform.position = rb.position;

        if (grounded && speedInput != 0)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, turnInput * turnStrength * Time.deltaTime * Mathf.Sign(speedInput) * (rb.velocity.magnitude / maxSpeed), 0f));
        }  
    }

    public void CheckpointHit(int cpNumber)
    {
        if(cpNumber == nextCheckpoint)
        {
            nextCheckpoint++;

            if(nextCheckpoint == 15 || nextCheckpoint == 27)
            {
                nextCheckpoint = 0;
                LapCompleted();
            }
        }

        if (isAI)
        {
            if(cpNumber == currentTarget)
            {
                SetNextAiTarget();
            }
        }
    }

    public void SetNextAiTarget()
    {
        currentTarget++;
        if (currentTarget >= RaceManager.instance.allCheckpoint.Length)
        {
            currentTarget = 0;
        }

        targetPoint = RaceManager.instance.allCheckpoint[currentTarget].transform.position;
        RandomAITarget();
    }

    public void LapCompleted()
    {
        currentLap++;

        if(lapTime < bestLapTime || bestLapTime == 0)
        {
            bestLapTime = lapTime;
        }

        if(currentLap <= RaceManager.instance.totalLaps)
        {
            lapTime = 0f;

            if (!isAI)
            {
                var ts = System.TimeSpan.FromSeconds(bestLapTime);
                UIManager.Instance.bestLapTimeText.text = string.Format("{0:00}m{1:00}.{2:000}s", ts.Minutes, ts.Seconds, ts.Milliseconds); 
                UIManager.Instance.lapCounterText.text = currentLap + "/" + RaceManager.instance.totalLaps;
            }
        }
        else
        {
            if (!isAI)
            {
                isAI = true;
                aiSpeedMod = 1f;

                targetPoint = RaceManager.instance.allCheckpoint[currentTarget].transform.position;
                RandomAITarget();

                var ts = System.TimeSpan.FromSeconds(bestLapTime);
                UIManager.Instance.bestLapTimeText.text = string.Format("{0:00}m{1:00}.{2:000}s", ts.Minutes, ts.Seconds, ts.Milliseconds);

                RaceManager.instance.FinishRace(); 
            }
        }
    }

    public void RandomAITarget()
    {
        targetPoint += new Vector3(Random.Range(-aiPointVariant, aiPointVariant), 0f, Random.Range(-aiPointVariant, aiPointVariant));
    }

    void ResetToTrack()
    {
        int pointToGoTo = nextCheckpoint - 1;
        if (pointToGoTo < 0)
        {
            pointToGoTo = RaceManager.instance.allCheckpoint.Length - 1;   
        }

        transform.position = RaceManager.instance.allCheckpoint[pointToGoTo].transform.position;
        rb.transform.position = transform.position;
        rb.velocity = Vector3.zero;

        speedInput = 0f;
        turnInput = 0f;
    }
}
 