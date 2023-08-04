using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunDictionary : MonoBehaviour
{
    public GunVariables[] guns;
}

[System.Serializable]
public class GunVariables
{
    //Public
    [Header("Visuals")]
    public string name;
    public Sprite gunSprite;
    public Sprite bulletSprite;
    public Sprite bulletSpriteCrit;
    public float bulletWait;
    public float bulletFade;
    public Sprite muzzleSprite;
    public Sprite muzzleSpriteCrit;
    public float muzzleWait;
    public float muzzleFade;
    public AudioClip shoot;
    public AudioClip shootCrit;

    [Header("Shots")]
    public float shotTime;
    public int bulletsPerShot;
    public float bulletSpreadAngle;
    public bool randomBulletSpread;
    public float bulletDistance;
    [Header("Damage")]
    public float damage;
    public float minDamage;
    public bool damageFalloff;
    public float critChance;
    [Header("Pivots")]
    public Vector3 bulletSpawn;
}