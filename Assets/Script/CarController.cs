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

    // Start is called before the first frame update
    void Start()
    {
        rb.transform.parent = null;

        dragOnGround = rb.drag;
    }

    // Update is called once per frame
    void Update()
    {
        speedInput = 0f;
        if(Input.GetAxis("Vertical") > 0)
        {
            speedInput = Input.GetAxis("Vertical") * forwardAccel;
        }else if(Input.GetAxis("Vertical") < 0)
        {
            speedInput = Input.GetAxis("Vertical") * reverseAccel;
        }



        turnInput = Input.GetAxis("Horizontal");

        if(grounded && Input.GetAxis("Vertical") != 0)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, turnInput * turnStrength * Time.deltaTime * Mathf.Sign(speedInput) * (rb.velocity.magnitude / maxSpeed), 0f)); 
        }



        //turning the wheel
        leftFrontWheel.localRotation = Quaternion.Euler(leftFrontWheel.localRotation.eulerAngles.x, (turnInput * maxWheelTurn) - 180, leftFrontWheel.localRotation.eulerAngles.z);
        rightFrontWheel.localRotation = Quaternion.Euler(rightFrontWheel.eulerAngles.x, (turnInput * maxWheelTurn), rightFrontWheel.localRotation.eulerAngles.z);



        transform.position = rb.position;
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
    }
}
