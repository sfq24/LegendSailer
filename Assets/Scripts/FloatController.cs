using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatController : MonoBehaviour {

    public float moveSpeed = 1.5f;
    public float turnSpeed = 30f;
    public float smooth = 5f;

    private Transform seaPlane;
    private Cloth seaCloth;

    [SerializeField]
    private int closestVerticeIndex = 0;
    [SerializeField]
    private int previousVerticeIndex = 0;

    [SerializeField]
    private Vector3 closetVertice;
    Rigidbody playerRB;

    private float distanceBetweenVertices = 0f;

    private void Awake()
    {
        playerRB = GetComponent<Rigidbody>();
    }
    // Use this for initialization
    void Start () {
        seaPlane = GameObject.Find("SeaPlane").transform;
        seaCloth = seaPlane.GetComponent<Cloth>();
        distanceBetweenVertices = seaCloth.transform.lossyScale.x;

    }
	
	// Update is called once per frame
	void Update () {
        //FloatingBoat();
        closetVertice = seaCloth.vertices[closestVerticeIndex];
        Debug.DrawRay(closetVertice, closetVertice.normalized);
        //distanceBetweenVertices = Vector3.Distance(seaCloth.vertices[0], seaCloth.vertices[1]);
        //Debug.Log("DistanceBetweenVertices:  " + distanceBetweenVertices);
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
            float distance = Vector3.Distance(
                new Vector3(seaCloth.vertices[i].x, 0f, seaCloth.vertices[i].z),
                new Vector3( this.transform.position.x, 0f, this.transform.position.z));

            if(distance < closetDistance)
            {
                closetDistance = distance;

                previousVerticeIndex = closestVerticeIndex;
                closestVerticeIndex = i;
                Debug.Log("Closest: " + closestVerticeIndex + "Previous: " + previousVerticeIndex);
            }
        }

        return seaCloth.vertices[closestVerticeIndex].y / seaCloth.transform.lossyScale.y;

    }

    private void Move()
    {
        //可以试不同的运动机制，直接给移动或者给加速度（force）
        float moving = Input.GetAxisRaw("Vertical");
        Vector3 movement = transform.forward * moving * moveSpeed * Time.deltaTime;
        Vector3 floating = new Vector3(0, FloatingBoat(), 0);
        //playerRB.MovePosition(playerRB.position + movement + floating);
        //float yPosition = Vector3.Lerp(
        //    seaCloth.vertices[previousVerticeIndex],
        //    seaCloth.vertices[closestVerticeIndex],
        //    Vector3.Distance(transform.localPosition, seaCloth.vertices[previousVerticeIndex]) / distanceBetweenVertices).y / seaCloth.transform.lossyScale.y;

        float yPosition = Mathf.Lerp(transform.localPosition.y, FloatingBoat(), smooth * Time.deltaTime);

        transform.localPosition = new Vector3(
            transform.localPosition.x + movement.x,
            yPosition,
            transform.localPosition.z + movement.z);
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
