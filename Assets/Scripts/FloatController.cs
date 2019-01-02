using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatController : MonoBehaviour {

    public float moveSpeed = 3f;
    public float turnSpeed = 100f;

    private Transform seaPlane;
    private Cloth seaCloth;
    [SerializeField]
    private int closestVerticeIndex = 0;
    [SerializeField]
    private Vector3 closetVertice;
    Rigidbody playerRB;




    private void Awake()
    {
        playerRB = GetComponent<Rigidbody>();
    }
    // Use this for initialization
    void Start () {
        seaPlane = GameObject.Find("SeaPlane").transform;
        seaCloth = seaPlane.GetComponent<Cloth>();

    }
	
	// Update is called once per frame
	void Update () {
        //FloatingBoat();
        closetVertice = seaCloth.vertices[closestVerticeIndex];
        Debug.DrawRay(closetVertice, closetVertice.normalized);
    }

    private void FixedUpdate()
    {
        Move();
        Turn();
    }

    float FloatingBoat()
    {
        float closetDistance = Vector3.Distance(seaCloth.vertices[0], this.transform.position);

        //TODO: check only the 8 nearest vertices
        for (int i = 0; i< seaCloth.vertices.Length; i++)
        {
            float distance = Vector3.Distance(seaCloth.vertices[i], this.transform.position);
            if(distance < closetDistance)
            {
                closetDistance = distance;
                closestVerticeIndex = i;
            }
        }

        return seaCloth.vertices[closestVerticeIndex].y / seaCloth.transform.lossyScale.y;

        //transform.localPosition = new Vector3(
        //    transform.localPosition.x, 
        //    seaCloth.vertices[closestVerticeIndex].y/seaCloth.transform.lossyScale.y, 
        //    transform.localPosition.z);

    }

    private void Move()
    {
        //可以试不同的运动机制，直接给移动或者给加速度（force）
        float moving = Input.GetAxisRaw("Vertical");
        Vector3 movement = transform.forward * moving * moveSpeed * Time.deltaTime;
        Vector3 floating = new Vector3(0, FloatingBoat(), 0);
        //playerRB.MovePosition(playerRB.position + movement+ floating);


        transform.localPosition = new Vector3(
            transform.localPosition.x + movement.x,
            seaCloth.vertices[closestVerticeIndex].y / seaCloth.transform.lossyScale.y,
            transform.localPosition.z + movement.z);
        //playerRB.AddForce(movement);
    }

    private void OnEnable()
    {
        playerRB.isKinematic = false;
    }

    private void OnDisable()
    {
        playerRB.isKinematic = true;
    }
    void Turn()
    {
        float turning = Input.GetAxisRaw("Horizontal") * turnSpeed * Time.deltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turning, 0f);
        playerRB.MoveRotation(playerRB.rotation * turnRotation);         //注意，转弯用的是*
    }
}
