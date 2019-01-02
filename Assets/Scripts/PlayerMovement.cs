using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatMovement : MonoBehaviour {

    public float speed = 6f;
    public float turningSpeed = 3f;
    Vector3 movement;
    Animator anim;
    Rigidbody playerRB;

    private void Awake()
    {
        playerRB = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Move();
        Turning();
        
    }

    private void Move()
    {
        //可以试不同的运动机制，直接给移动或者给加速度（force）
        float moving = Input.GetAxisRaw("Horizontal");
        movement = transform.forward * moving * speed * Time.deltaTime;
        playerRB.MovePosition(playerRB.position + movement);
    }

    private void OnEnable()
    {
        playerRB.isKinematic = false;
    }

    private void OnDisable()
    {
        playerRB.isKinematic = true;
    }
    void Turning()
    {
        float turning = Input.GetAxisRaw("Vertical") * turningSpeed *Time.deltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turning, 0f); 
        playerRB.MoveRotation(playerRB.rotation * turnRotation);         //注意，转弯用的是*
    }
}
