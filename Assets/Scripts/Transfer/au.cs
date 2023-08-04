using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class au : MonoBehaviour
{
    [Header("Values")]
    public float baseSpeed;
    public float decelerationMult;
    public float maxSpeed;
    public float maxCrouchSpeed;

    [Header("Objects")]
    public Rigidbody2D charController;
    public BoxCollider2D boxCollider;
    public SpriteRenderer spriteRenderer;
    public spriteColorManager spriteColorManager;

    [Header("Debug")]
    public bool crouch;
    public bool jumping;
    public bool grounded;

    [Header("Guns")]
    public GameObject[] guns;
    public Gun[] gunComps;
    public Transform gunPivot;

    [Header("Sprites")]
    public Sprite redTeam;
    public Sprite blueTeam;
    public Sprite neutralTeam;

    Vector2 walkVelocity;
    Vector2 pushVelocity;

    //Damage
    bool isDamaged;
    int damageColorIndex;
    float timeDamaged;

    private void Start()
    {
        spriteRenderer = this.transform.GetChild(0).GetComponent<SpriteRenderer>();
        spriteColorManager = spriteRenderer.GetComponent<spriteColorManager>();
        QualitySettings.vSyncCount = 1;
        switch (LayerMask.LayerToName(this.gameObject.layer))
        {
            case "Red":
                spriteRenderer.sprite = redTeam;
                break;
            case "Blue":
                spriteRenderer.sprite = blueTeam;
                break;
            case "Neutral":
                spriteRenderer.sprite = neutralTeam;
                break;
            default:
                break;
        }
        gunComps = new Gun[guns.Length];
        for (int i = 0; i < guns.Length; i++)
        {
            gunComps[i] = guns[i].GetComponent<Gun>();
        }
    }
    private void Update()
    {
        if(isDamaged)
        {
            float T = (Time.time - timeDamaged) * 10f; //divisor being how long red fades
            spriteColorManager.UpdateColor(Color.Lerp(Color.red, Color.white, T), damageColorIndex);
            if(T > 1)
            {
                spriteColorManager.RemoveColor(damageColorIndex);
                isDamaged = false;
                damageColorIndex = 0;
                timeDamaged = 0;
            }
        }
    }

    public void UpdateMovement(GlobalManager.playerInputs input)
    {
        //Apply Multipliers
        float finalMultiplier = 1;
        
        if(input.movement.magnitude > 0.1)
        {
            finalMultiplier *= baseSpeed;
        }

        //Apply
        Vector2 movementBase = input.movement.normalized;
        movementBase *= finalMultiplier;
        walkVelocity *= decelerationMult;
        walkVelocity += movementBase;

        //Clamp
        if(crouch)
        {
            walkVelocity = Vector2.ClampMagnitude(walkVelocity, maxCrouchSpeed);
        }
        else
        {
            walkVelocity = Vector2.ClampMagnitude(walkVelocity, maxSpeed);
        } 

        //Finish
        charController.velocity = walkVelocity + pushVelocity;

        //Gun
        if(input.primaryFire == 1 || input.primaryFire == 2)
        {
            gunComps[0].ShootGun();
        }
        gunComps[0].AimGun(input.cam);
    }

    public void Damage(float damage)
    {
        if(!isDamaged)
        {
            isDamaged = true;
            damageColorIndex = spriteColorManager.AssignColor(0);
        }
        timeDamaged = Time.time;
    }

    public void Dead()
    {
        if(isDamaged)
        {
            isDamaged = false;
            spriteColorManager.RemoveColor(damageColorIndex);
        }
        for (int i = 0; i < guns.Length; i++)
        {
            guns[i].SetActive(false);
        }
        this.gameObject.SetActive(false);
    }
}
