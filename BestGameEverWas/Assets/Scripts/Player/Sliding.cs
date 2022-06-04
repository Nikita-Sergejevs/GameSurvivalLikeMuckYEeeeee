using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sliding : MonoBehaviour
{
    [Header("Reference")]
    private Rigidbody rb;
    private PlayerMovement pm;
    public Transform orientation;
    public Transform playerObj;

    [Header("Sliding")]
    private float slideTimer;
    public float maxSlideTime;
    public float slideForce;

    public float slideYScale;
    private float startYScale;

    [Header("Input")]
    private float horizontalInput;
    private float verticalInput;
    public KeyCode slideKey = KeyCode.LeftControl;

    private bool sliding;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();

        startYScale = playerObj.localScale.y;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if(Input.GetKeyDown(slideKey) && (horizontalInput != 0 || verticalInput != 0))
        {
            StartSlide();
        }
        if(Input.GetKeyUp(slideKey) && sliding)
        {
            StopSlide();
        }
    }

    private void FixedUpdate()
    {
        if(sliding)
        {
            SlidingMovement();
        }
    }

    private void StartSlide()
    {
        sliding = true;

        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;
    }

    private void SlidingMovement()
    {
        Vector3 inputDirection = orientation.forward * verticalInput;

        rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

        slideTimer -= Time.deltaTime;
        if(slideTimer <= 0)
        {
            StopSlide();
        }
    }

    private void StopSlide()
    {
        sliding = false;
        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
    }
}
