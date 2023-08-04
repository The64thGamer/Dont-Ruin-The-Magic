using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    //NOTES

    //You can spam sprint at maximum sprint to keep the bar max while also having max speed. Add a penalty for that.


    [Header("Initial")]
    //Attatched Objects
    public Camera PlayerCamScript;
    public Transform camTransform;
    public Image sprintBar;
    public Image sprintBar2;
    public Image sprintBar3;
    public Image healthBar;
    public Image healthBar2;
    public Image cameraIcon;
    public Image cameraSpottedIcon;
    public GameObject camTracer;
    PlayerInteractions playerInteractions;
    public GameObject takeItem;
    public TMP_Text takeItemText;
    Inventory inventory;
    public CanvasGroup inventoryScreen;
    AudioSource audsrc;


    //Position, Movement, Buttons
    [Tooltip("Initial Camera X position")]
    public float camXRotation;
    [Tooltip("Initial Camera Y position")]
    public float camYRotation;

    [Header("Splines")]
    public AnimationCurve healthSpline;
    public AnimationCurve sprintSpline;

    //Speeds and Base attributes
    [Header("Afflictions")]
    public float health;
    public bool inWater = false;
    public float fallMovementHinderance = 0;
    public int camsSpotted = 0;
    public int camsVisible = 0;
    public float camAccelSetting = 1;

    [Header("Speed")]
    float baseSpeed = 3f;
    float crouchSpeed = 1.5f;
    float sprintSpeed = 6f;
    float sprintTime = 4.0f;
    public float currentSprintTime;

    [Header("Jump")]
    public bool enableJump;
    float gravity = 15;
    float jumpSpeed = 5f;
    float crouchJumpSpeed = 6f;
    float airControl = 0.7f;
    float airTurnSpeed = 1.5f;
    public float coyoteTime = 0;
    bool firstFrameFall;
    public float initialFallHeight = 0;

    [Header("Crouch")]
    public bool enableCrouch;
    float camInitialHeight = 0.657f;
    float camCrouchHeight = -0.2f;
    public GameObject feet;
    public GameObject unCrouch;

    [Range(0, 1)]
    float flashLightSpam = 0;

    [Header("CameraZoom")]
    public bool enableCamZoom;
    float maxFov = 100;
    float minFov = 50;
    public float currentFOV;
    float sprintCrouchFOVAdder;
    float sprintCrouchFOVFader;
    float camBobAdder;
    float camBobFader;

    [Header("CameraSmooth")]
    public bool enableCamSmooth;
    public float smoothSpeed;
    public float maxVeclocity;
    Vector2 camAcceleration;

    [Header("Guns")]
    public GameObject[] guns;

    [Header("PlayerState")]
    public PlayerState playerState;

    public enum PlayerState
    {
        normal,
        frozenBody,
        frozenCam,
        frozenAll,
        frozenCamUnlock,
        frozenAllUnlock,
        noInput,
        noInputUnlock,
    }

    //Other
    CharacterController CharCont;
    Vector3 moveDirection = Vector3.zero;
    Vector2 CStick;
    Vector2 JoyStick;
    public float JumpBool;
    float smoothScroll;
    float timedelta;
    bool crouchBool;
    float camHeight;
    [HideInInspector]
    public Vector2 holdingRotation;

    //New Input
    [SerializeField]
    private Controller gamepad;
    [Header("Gamepad")]
    private bool clickGamepad = false;
    private bool jumpGamepad = false;
    private bool crouchGamepad = false;
    private bool flashGamepad = false;
    private bool runGamepad = false;
    private bool holdGamepad = false;
    public Vector2 GPJoy;
    public Vector2 GPCam;
    public Vector2 GPZoom;

    void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        gamepad.Gamepad.Enable();
    }

    void OnDisable()
    {
        gamepad.Gamepad.Disable();
    }
    void Awake()
    {
        audsrc = this.GetComponent<AudioSource>();
        inventory = this.GetComponent<Inventory>();
        playerInteractions = this.GetComponent<PlayerInteractions>();
        currentSprintTime = sprintTime;
        initialFallHeight = this.transform.position.y;
        DontDestroyOnLoad(this.gameObject);
        camHeight = camInitialHeight;
        if (PlayerCamScript != null)
        {
            smoothScroll = PlayerCamScript.fieldOfView;
        }
        Cursor.lockState = CursorLockMode.Locked;
        inventoryScreen.alpha = 0;
        playerState = PlayerState.normal;
        //Initialize Variables
        CharCont = GetComponent<CharacterController>();
        gamepad = new Controller();
        gamepad.Gamepad.Click.canceled += ctx => clickGamepad = false;
        gamepad.Gamepad.Click.performed += ctx => clickGamepad = true;
        gamepad.Gamepad.Jump.canceled += ctx => jumpGamepad = false;
        gamepad.Gamepad.Jump.performed += ctx => jumpGamepad = true;
        gamepad.Gamepad.Run.canceled += ctx => runGamepad = false;
        gamepad.Gamepad.Run.performed += ctx => runGamepad = true;
        gamepad.Gamepad.Crouch.canceled += ctx => crouchGamepad = false;
        gamepad.Gamepad.Crouch.performed += ctx => crouchGamepad = true;
        gamepad.Gamepad.Hold.canceled += ctx => holdGamepad = false;
        gamepad.Gamepad.Hold.performed += ctx => holdGamepad = true;
        gamepad.Gamepad.Horizontal.performed += ctx => GPJoy.x = ctx.ReadValue<float>();
        gamepad.Gamepad.Vertical.performed += ctx => GPJoy.y = ctx.ReadValue<float>();
        gamepad.Gamepad.Horizontal.canceled += ctx => GPJoy.x = 0;
        gamepad.Gamepad.Vertical.canceled += ctx => GPJoy.y = 0;
        gamepad.Gamepad.CamHorizontal.performed += ctx => GPCam.x = ctx.ReadValue<float>();
        gamepad.Gamepad.CamVertical.performed += ctx => GPCam.y = ctx.ReadValue<float>();
        gamepad.Gamepad.Zoom.performed += ctx => GPZoom.x = ctx.ReadValue<float>();
        gamepad.Gamepad.FlashZoom.performed += ctx => GPZoom.y = ctx.ReadValue<float>();
        gamepad.Gamepad.Zoom.canceled += ctx => GPZoom.x = 0;
        gamepad.Gamepad.FlashZoom.canceled += ctx => GPZoom.y = 0;
        gamepad.Gamepad.CamHorizontal.canceled += ctx => GPCam.x = 0;
        gamepad.Gamepad.CamVertical.canceled += ctx => GPCam.y = 0;
    }

    public void UpdateMovement(GlobalManager.playerInputs input)
    {
        //Joystick
        JoyStickCheck();

        //No Input
        if (playerState == PlayerState.noInput || playerState == PlayerState.noInputUnlock)
        {
            CStick = Vector2.zero;
            JoyStick = Vector2.zero;
        }

        //Cam Crouch Code
        PlayerCamScript.transform.localPosition = new Vector3(PlayerCamScript.transform.localPosition.x, Mathf.Lerp(PlayerCamScript.transform.localPosition.y, camHeight, Time.deltaTime * 5), PlayerCamScript.transform.localPosition.z);

        //Camera Code
        if (playerState != PlayerState.frozenCam && playerState != PlayerState.frozenAll && playerState != PlayerState.frozenAllUnlock && playerState != PlayerState.frozenCamUnlock && playerInteractions.PickupCheck())
        {
            //Cam Zoom
            if (enableCamZoom)
            {
                CamZoomCheck();
            }

            bool camMove = true;
            //Items

            //Camera
            if (camMove)
            {
                CameraMove(input.cam);
            }
        }

        //Body Code
        if (playerState != PlayerState.frozenBody && playerState != PlayerState.frozenAll && playerState != PlayerState.frozenAllUnlock)
        {
            //Jump
            if (!inWater)
            {
                if (enableJump && (jumpGamepad))
                {
                    JumpBool++;
                    UncrouchCheck();
                }
                else
                {
                    JumpBool = 0;
                }
            }
            //Crouch
            if (enableCrouch)
            {
                CrouchCheck(input);
            }


            //Move
            MovePlayer(input.movement);
        }

        switch (Cursor.lockState)
        {
            case CursorLockMode.None:
                if (playerState != PlayerState.frozenAllUnlock && playerState != PlayerState.frozenCamUnlock && playerState != PlayerState.noInputUnlock)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
                break;
            case CursorLockMode.Locked:
                if (playerState == PlayerState.frozenAllUnlock || playerState == PlayerState.frozenCamUnlock || playerState == PlayerState.noInputUnlock)
                {
                    Cursor.lockState = CursorLockMode.None;
                }
                break;
            default:
                break;
        }

        //FOV
        PlayerCamScript.fieldOfView = currentFOV + (sprintCrouchFOVAdder * sprintCrouchFOVFader);

        //Health
        float hlth = healthSpline.Evaluate(health);
        healthBar.fillAmount = hlth;
        healthBar2.color = new Color(1, 1, 1, hlth);

        //Sprint
        if (currentSprintTime == sprintTime)
        {
            sprintBar.color = new Color(1, 1, 1, Mathf.Max(0, sprintBar.color.a - Time.deltaTime * 10));
            sprintBar2.color = new Color(1, 1, 1, Mathf.Max(0, sprintBar2.color.a - Time.deltaTime * 10));
            sprintBar3.color = new Color(1, 1, 1, Mathf.Max(0, sprintBar3.color.a - Time.deltaTime * 10));
        }
        else
        {
            sprintBar.color = new Color(1, 1, 1, Mathf.Min(1, sprintBar.color.a + Time.deltaTime * 10));
            sprintBar3.color = new Color(1, 1, 1, Mathf.Min(0.32f, sprintBar3.color.a + Time.deltaTime * 10));

            float spr = sprintSpline.Evaluate(currentSprintTime / sprintTime);
            sprintBar.fillAmount = spr;
            sprintBar2.color = new Color(1, 1, 1, spr);
        }


        //Cam Icons
        if (camsVisible > 0)
        {
            cameraIcon.color = new Color(1, 1, 1, Mathf.Lerp(cameraIcon.color.a, 1, Time.deltaTime * 5));
        }
        else
        {
            cameraIcon.color = new Color(1, 1, 1, Mathf.Lerp(cameraIcon.color.a, 0, Time.deltaTime * 10));
        }
        if (camsSpotted > 0)
        {
            cameraSpottedIcon.color = new Color(1, 1, 1, Mathf.Lerp(cameraSpottedIcon.color.a, 1, Time.deltaTime * 5));
        }
        else
        {
            cameraSpottedIcon.color = new Color(1, 1, 1, Mathf.Lerp(cameraSpottedIcon.color.a, 0, Time.deltaTime * 10));
        }

        if (health <= 0)
        {
            playerState = PlayerState.frozenAll;
        }

        //Take Item
        string item = playerInteractions.ObjectTaken();
        if (item != "" && inventoryScreen.alpha == 0)
        {
            takeItem.SetActive(true);
            takeItemText.text = item;
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (inventory.InsertItem(playerInteractions.lookObject))
                {
                    Destroy(playerInteractions.lookObject);
                }
            }
        }
        else
        {
            takeItem.SetActive(false);
        }

        //Inventory
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (inventoryScreen.alpha == 0)
            {
                inventoryScreen.alpha = 1;
                playerState = PlayerState.noInputUnlock;
                inventory.DropDraggingItem(true);
            }
            else
            {
                inventoryScreen.alpha = 0;
                playerState = PlayerState.normal;
                inventory.DropDraggingItem(true);
            }
        }
    }
    void CameraMove(Vector2 axis)
    {

        if (enableCamSmooth)
        {
            camAcceleration += axis;
            if (camAcceleration.x > 0)
            {
                camAcceleration.x -= smoothSpeed;
            }
            else if (camAcceleration.x < 0)
            {
                camAcceleration.x += smoothSpeed;
            }
            if (camAcceleration.x < .1f && camAcceleration.x > -.1f)
            {
                camAcceleration.x = 0;
            }
            if (camAcceleration.y > 0)
            {
                camAcceleration.y -= smoothSpeed;
            }
            else if (camAcceleration.y < 0)
            {
                camAcceleration.y += smoothSpeed;
            }
            if (camAcceleration.y < .5f && camAcceleration.y > -.5f)
            {
                camAcceleration.y = 0;
            }
            camAcceleration.x = Mathf.Max(Mathf.Min(camAcceleration.x, maxVeclocity), -maxVeclocity);
            camAcceleration.y = Mathf.Max(Mathf.Min(camAcceleration.y, maxVeclocity), -maxVeclocity);
            camXRotation += camAcceleration.x / 100f * camAccelSetting;
            camYRotation += camAcceleration.y / 100f * camAccelSetting;
        }
        else
        {
            camXRotation += axis.x * camAccelSetting;
            camYRotation -= axis.y * camAccelSetting;
        }
        camBobFader = Mathf.Min(camBobFader + (Time.deltaTime * 25), 1);
        camBobAdder = Mathf.Max(camBobAdder - (Time.deltaTime * 10), 0);
        camYRotation = Mathf.Clamp(camYRotation, -85, 85);

        transform.eulerAngles = new Vector3(transform.eulerAngles.x, camXRotation, transform.eulerAngles.z);
        PlayerCamScript.transform.position = camTransform.position;
        PlayerCamScript.transform.eulerAngles = new Vector3(Mathf.Clamp(camYRotation + (camBobAdder * camBobFader), -85, 85), transform.eulerAngles.y, PlayerCamScript.transform.eulerAngles.z);

    }

    public void MovePlayer(Vector2 axis)
    {
        float newy = PlayerCamScript.transform.position.y - this.transform.position.y;
        CharCont.height = Mathf.Max(0.9f, (((newy * (1 / camInitialHeight)) / 2f) + .5f) * 1.3f);
        CharCont.center = new Vector3(0f, Mathf.Max(-0.21f, Remap(newy, feet.transform.localPosition.y, camInitialHeight, feet.transform.localPosition.y, 0)), 0);
        //Void Bounce
        if (this.transform.position.y < -20)
        {
            this.transform.position = new Vector3(this.transform.position.x, 100f, this.transform.position.z);
            moveDirection = this.transform.position;
        }
        float nowSpeed = baseSpeed;

        //Crouch
        if (crouchBool)
        {
            nowSpeed = crouchSpeed;
        }
        //Sprint
        if (runGamepad)
        {
            if (currentSprintTime > 0)
            {
                if (crouchBool)
                {
                    nowSpeed += sprintSpeed * (currentSprintTime / sprintTime) / 2.0f;
                }
                else
                {
                    nowSpeed += sprintSpeed * (currentSprintTime / sprintTime);
                }

                sprintCrouchFOVAdder = Mathf.Min(currentSprintTime / sprintTime * 2, 1) * 12;
                if (CharCont.velocity.magnitude > 0.1f)
                {
                    sprintCrouchFOVFader = Mathf.Min(sprintCrouchFOVFader + (Time.deltaTime * 10), 1);
                    currentSprintTime = Mathf.Max(currentSprintTime - Time.deltaTime, 0);
                }
                else
                {
                    sprintCrouchFOVFader = Mathf.Max(sprintCrouchFOVFader - (Time.deltaTime * 10), 0);
                    currentSprintTime = Mathf.Min(currentSprintTime + Time.deltaTime, sprintTime);
                }
            }
        }
        else
        {
            currentSprintTime = Mathf.Min(currentSprintTime + Time.deltaTime, sprintTime);
            sprintCrouchFOVFader = Mathf.Max(sprintCrouchFOVFader - (Time.deltaTime * 10), 0);
        }


        Vector3 transForward;
        Vector3 transRight;

        transForward = transform.forward;
        transRight = transform.right;

        if (CharCont.isGrounded)
        {
            coyoteTime = 0.3f;
            moveDirection = ((transForward * axis.y * nowSpeed) + (transRight * (axis.x * nowSpeed)));
            if (!firstFrameFall)
            {
                firstFrameFall = true;
                float fallHeight = this.transform.position.y - initialFallHeight;
                if (fallHeight < -0.6f)
                {
                    fallMovementHinderance = Mathf.Min(-fallHeight / 2.0f, 1);
                    camBobAdder = -fallHeight * 6.0f;
                    camBobFader = 0;
                }
                if (fallHeight < -3.0)
                {
                    health += fallHeight * 0.04f;
                }
                if (fallHeight < -6.5)
                {
                    health = 0;
                }
            }
        }
        else
        {
            if (firstFrameFall)
            {
                firstFrameFall = false;
                initialFallHeight = this.transform.position.y;
            }
            coyoteTime = Mathf.Max(0, coyoteTime - Time.deltaTime);
            moveDirection = (((transForward * axis.y * nowSpeed) + (transRight * (axis.x * nowSpeed * airTurnSpeed))) * airControl) + new Vector3(0, moveDirection.y, 0);
        }
        if (coyoteTime > 0)
        {
            //Jumping
            if (JumpBool == 1)
            {
                if (crouchBool)
                {
                    moveDirection.y = crouchJumpSpeed;
                }
                else
                {
                    moveDirection.y = jumpSpeed;
                }
                coyoteTime = 0;
            }
        }

        moveDirection.y -= gravity * Time.deltaTime;

        //Water
        if (inWater)
        {
            float water = (Mathf.Sin(Time.time * 4) / 3.0f) + 0.6f;
            moveDirection.x *= water;
            moveDirection.y *= 0.5f;
            moveDirection.z *= water;
        }

        //Fall Damage Hinderance
        moveDirection.x *= 1 - fallMovementHinderance;
        moveDirection.z *= 1 - fallMovementHinderance;
        fallMovementHinderance = Mathf.Max(0, fallMovementHinderance - Time.deltaTime);

        CharCont.Move(moveDirection * Time.deltaTime);
    }

    float Remap(float val, float in1, float in2, float out1, float out2)
    {
        return out1 + (val - in1) * (out2 - out1) / (in2 - in1);
    }

    float realModulo(float a, float b)
    {
        return a - b * Mathf.Floor(a / b);
    }

    void UncrouchCheck()
    {
        RaycastHit hit;
        if (Physics.Raycast(unCrouch.transform.position, transform.TransformDirection(Vector3.up), out hit, Mathf.Infinity))
        {
            if (hit.point.y > PlayerCamScript.transform.position.y + camInitialHeight)
            {
                crouchBool = false;
            }
        }
        else
        {
            crouchBool = false;
        }
    }


    bool mouseCheck()
    {
        if (clickGamepad)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    void CrouchCheck(GlobalManager.playerInputs input)
    {
        if (CharCont.isGrounded)
        {
            if (!inWater)
            {
                if (Convert.ToBoolean(input.crouch) != crouchBool)
                {
                    crouchBool = !crouchBool;
                    if (!crouchBool)
                    {
                        crouchBool = true;
                        UncrouchCheck();
                    }
                    timedelta = 0;

                }
            }
            else
            {
                crouchBool = false;
            }
        }
        else
        {
            timedelta += Time.deltaTime;
        }
        //Crouch height
        if (!crouchBool)
        {
            camHeight = camInitialHeight;
        }
        else
        {
            camHeight = camCrouchHeight;
        }

    }

    void JoyStickCheck()
    {
        JoyStick = GPJoy;
        CStick = GPCam * 2;
        //Anims
        JoyStick = JoyStick.normalized;
    }


    void CamZoomCheck()
    {
        smoothScroll += GPZoom.x;
        smoothScroll = Mathf.Clamp(smoothScroll, minFov, maxFov);
        currentFOV = Mathf.Lerp(currentFOV, smoothScroll, Time.deltaTime * 5);
    }

    public void PingCamera(Transform origin, bool spotted)
    {
        if (spotted)
        {
            GameObject tracer = GameObject.Instantiate(camTracer, camTracer.transform.parent);
            tracer.SetActive(true);
            tracer.GetComponent<CameraTracer>().cam = origin;
            camsSpotted++;
        }
        else
        {
            camsVisible++;
        }
    }

    public void UnPingCamera(bool spotted)
    {
        if (spotted)
        {
            camsSpotted--;
        }
        else
        {
            camsVisible--;
        }
    }
}


[System.Serializable]
public class FootstepType
{
    public string meshName;
    public AudioClip[] clips;
    [Range(0.0f, 1.0f)]
    public float volume;
}

[System.Serializable]
public class CharacterItems
{
    public string itemName;
    public Sprite icon;
    public string unlockString;
}