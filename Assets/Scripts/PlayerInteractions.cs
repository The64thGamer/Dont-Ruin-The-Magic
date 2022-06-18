using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractions : MonoBehaviour
{
    [Header("InteractableInfo")]
    public float sphereCastRadius = 0.5f;
    public LayerMask layers;
    private Vector3 raycastPos;
    public GameObject lookObject;
    private PhysicsObject physicsObject;
    private Camera mainCamera;

    [Header("Pickup")]
    public Transform pickupParent;
    private GameObject pickupParentLook;
    public GameObject currentlyPickedUpObject;
    private Rigidbody pickupRB;
    public bool freeze;

    [Header("ObjectFollow")]
    [SerializeField] private float minSpeed = 0;
    [SerializeField] private float maxSpeed = 300f;
    [SerializeField] private float maxDistance = 10f;
    private float currentSpeed = 0f;
    private float currentDist = 0f;

    [Header("Rotation")]
    public float rotationSpeed = 100f;
    Quaternion lookRot;
    public Vector2 holdingRotation;
    RaycastHit hit;
    Vector3 fixedRot;

    private void Start()
    {
        mainCamera = Camera.main;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(hit.point, sphereCastRadius);
    }

    //Interactable Object detections and distance check
    void Update()
    {
        //Here we check if we're currently looking at an interactable object
        raycastPos = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        if (Physics.SphereCast(raycastPos, sphereCastRadius, mainCamera.transform.forward, out hit, maxDistance, layers))
        {
            lookObject = hit.collider.gameObject;
        }
        else
        {
            lookObject = null;
        }

        //if we press the button of choice
        if (Input.GetMouseButtonDown(0))
        {
            //and we're not holding anything
            if (currentlyPickedUpObject == null)
            {
                //and we are looking an interactable object
                if (lookObject != null)
                {
                    PickUpObject();
                }

            }
            //if we press the pickup button and have something, we drop it
            else
            {
                BreakConnection();
            }
        }


    }

    //Velocity movement toward pickup parent and rotation
    private void FixedUpdate()
    {
        if (currentlyPickedUpObject != null)
        {
            currentDist = Vector3.Distance(pickupParent.position, pickupRB.position);
            currentSpeed = Mathf.SmoothStep(minSpeed, maxSpeed, currentDist / maxDistance);
            currentSpeed *= Time.fixedDeltaTime;
            Vector3 direction = pickupParent.position - pickupRB.position;
            pickupRB.velocity = direction.normalized * currentSpeed;
            //Rotation
            //lookRot = Quaternion.LookRotation(pickupParentLook.transform.position - pickupRB.position);
            //lookRot = Quaternion.Slerp(mainCamera.transform.rotation, lookRot, rotationSpeed * Time.deltaTime);
            lookRot = Quaternion.Euler(fixedRot);
            pickupRB.MoveRotation(lookRot);
        }

    }

    private void OnDisable()
    {
        BreakConnection();
    }

    //Release the object
    public void BreakConnection()
    {
        if (pickupRB != null)
        {
            pickupRB.constraints = RigidbodyConstraints.None;
        }
        currentlyPickedUpObject = null;
        if (physicsObject != null)
        {
            physicsObject.pickedUp = false;
        }
        currentDist = 0;
        pickupRB = null;
    }

    private void PickUpObject()
    {
        if (lookObject.layer == 8)
        {
            physicsObject = lookObject.GetComponent<PhysicsObject>();
            currentlyPickedUpObject = lookObject;
            pickupRB = currentlyPickedUpObject.GetComponent<Rigidbody>();
            pickupRB.constraints = RigidbodyConstraints.FreezeRotation;
            if (physicsObject != null)
            {
                physicsObject.playerInteractions = this;
                StartCoroutine(physicsObject.PickUp());
            }
            pickupParent.transform.localEulerAngles = Vector3.zero;
            mainCamera.transform.parent.GetComponent<Player>().holdingRotation = new Vector2(0, 0);

        }
    }

    public void PickUpObject(GameObject item)
    {
        if (item.layer == 8)
        {
            lookObject = item;
            PickUpObject();
        }
    }

    public string ObjectTaken()
    {
        if (lookObject != null && lookObject.layer == 8 && currentlyPickedUpObject == null)
        {
            return lookObject.name;
        }
        else
        {
            return "";
        }
    }

    public bool PickupCheck()
    {
        //Pickups
        if (currentlyPickedUpObject != null)
        {
            fixedRot = new Vector3(holdingRotation.y, holdingRotation.x, 0);
            if (Input.GetMouseButton(1))
            {
                holdingRotation.x += Input.GetAxis("Mouse X") * 1.5f;
                holdingRotation.y += Input.GetAxis("Mouse Y") * -1.5f;
                return false;
            }
            else
            {
                return true;
            }
        }
        else
        {
            return true;
        }
    }

}