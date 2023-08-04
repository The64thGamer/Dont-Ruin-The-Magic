using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Gun : MonoBehaviour
{
    public GunVariables gunStats;
    public SpriteRenderer spriteRenderer;
    GameObject bulletPivot;
    public bool isHostGun;

    //Private-
    float currentShotTime;
    AudioSource audioSource;
    Vector2 gunPos;

    private void Start()
    {
        bulletPivot = new GameObject("Bullet Pivot");
        bulletPivot.transform.parent = this.transform;
        bulletPivot.transform.localPosition = gunStats.bulletSpawn;
        audioSource = this.gameObject.AddComponent<AudioSource>();
        spriteRenderer = this.gameObject.AddComponent<SpriteRenderer>();
        if (isHostGun)
        {
            audioSource.spatialBlend = 0;
        }
        else
        {
            audioSource.spatialBlend = 1;
        }
        audioSource.maxDistance = 400;
        audioSource.minDistance = 100;
        audioSource.dopplerLevel = 0;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        spriteRenderer.sprite = gunStats.gunSprite;
        spriteRenderer.sortingOrder = 101;
    }
    // Update is called once per frame
    public void AimGun(Vector2 mousePos)
    {
        // get direction you want to point at
        Vector2 direction = (mousePos - (Vector2)transform.position).normalized;

        // set vector of transform directly
        transform.right = -direction;

        if (this.transform.eulerAngles.z > 90 && this.transform.eulerAngles.z < 270)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            transform.right = -transform.right;
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        gunPos = direction;
    }

    public void ShootGun()
    {
        if (currentShotTime + gunStats.shotTime <= Time.time && this.isActiveAndEnabled)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        currentShotTime = Time.time;
        //Crit
        bool crit = false;
        if(Random.Range(0,100) <= gunStats.critChance)
        {
            crit = true;
        }
        //Sound
        if(!crit)
        {
            audioSource.PlayOneShot(gunStats.shoot);
        }
        else
        {
            audioSource.PlayOneShot(gunStats.shootCrit);
        }
        

        for (int i = 0; i < gunStats.bulletsPerShot; i++)
        {
            //Angle
            float angle = 0;
            if(gunStats.randomBulletSpread)
            {
                angle = Random.Range(-(gunStats.bulletSpreadAngle / 2), (gunStats.bulletSpreadAngle / 2));
            }
            else if(gunStats.bulletsPerShot > 1)
            {
                angle =( (gunStats.bulletSpreadAngle / (gunStats.bulletsPerShot -1)) * i) - gunStats.bulletSpreadAngle / 2;
            }

            //Muzzleflash
            GameObject muzzle = new GameObject("Bullet");
            muzzle.transform.position = bulletPivot.transform.position;
            muzzle.transform.localRotation = this.transform.localRotation;
            if(this.transform.localScale.x < 0)
            {
                muzzle.transform.eulerAngles -= new Vector3(0, 0, 90);
            }
            else
            {
                muzzle.transform.eulerAngles += new Vector3(0, 0, 90);
            }    
            SpriteRenderer rr = muzzle.AddComponent<SpriteRenderer>();
            FadeOutObject fo = muzzle.AddComponent<FadeOutObject>();
            if(crit)
            {
                rr.sprite = gunStats.muzzleSpriteCrit;
                fo.fadeTime = gunStats.muzzleFade * 3f;
                fo.waitTime = gunStats.muzzleFade * 3f;
            }
            else
            {
                rr.sprite = gunStats.muzzleSprite;
                fo.fadeTime = gunStats.muzzleFade;
                fo.waitTime = gunStats.muzzleFade;
            }
            fo.follow = bulletPivot.transform;

            //Create
            GameObject bullet = new GameObject("Bullet");
            bullet.transform.position = bulletPivot.transform.position;
            rr = bullet.AddComponent<SpriteRenderer>();
            fo = bullet.AddComponent<FadeOutObject>();
            if (crit)
            {
                rr.sprite = gunStats.bulletSpriteCrit;
                fo.fadeTime = gunStats.bulletFade * 3f;
                fo.waitTime = gunStats.bulletWait * 3f;
            }
            else
            {
                rr.sprite = gunStats.bulletSprite;
                fo.fadeTime = gunStats.bulletFade;
                fo.waitTime = gunStats.bulletWait;
            }
            

            //Calculates
            Vector2 fire = RotateGun(gunPos, angle);
            LayerMask layerMask = ~((1 << this.gameObject.layer) | (1 << LayerMask.NameToLayer("Default")));
            RaycastHit2D ray = Physics2D.Raycast(bulletPivot.transform.position, fire, gunStats.bulletDistance, layerMask);
            if(!ray.collider)
            {
                ray.point = new Vector2(bulletPivot.transform.position.x, bulletPivot.transform.position.y) + (fire * gunStats.bulletDistance);
            }
            else
            {
                //Apply Damage
                if(isEnemyTag(LayerMask.LayerToName(ray.collider.gameObject.layer)) == 1)
                {
                    if(!gunStats.damageFalloff)
                    {
                        if(crit)
                        {
                            ray.collider.SendMessage("Damage", gunStats.damage *1.5f);
                        }
                        else
                        {
                            ray.collider.SendMessage("Damage", gunStats.damage);
                        }
                        
                    }
                    else
                    {
                        if (crit)
                        {
                            ray.collider.SendMessage("Damage", (float)Mathf.Lerp(gunStats.damage * 1.5f, gunStats.minDamage * 1.5f, ray.distance / gunStats.bulletDistance));
                        }
                        else
                        {
                            ray.collider.SendMessage("Damage", (float)Mathf.Lerp(gunStats.damage, gunStats.minDamage, ray.distance / gunStats.bulletDistance));
                        }
                        
                    }
                }
            }
            Vector2 centerPos = (new Vector2(bulletPivot.transform.position.x, bulletPivot.transform.position.y) + ray.point) / 2f;
            bullet.transform.position = centerPos;
            Vector2 direction = ray.point - new Vector2(bulletPivot.transform.position.x, bulletPivot.transform.position.y);
            direction = Vector3.Normalize(direction);
            bullet.transform.right = direction;
            Vector2 scale = new Vector2(1, 1);
            scale.x = Vector2.Distance(bulletPivot.transform.position, ray.point) / (rr.sprite.rect.width / rr.sprite.pixelsPerUnit);
            bullet.transform.localScale = scale;
        }
    }
    Vector2 RotateGun(Vector2 v, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }

    int isEnemyTag(string layer)
    {
        if (layer == "Red" || layer == "Blue" || layer == "Neutral")
        {
            if(layer == LayerMask.LayerToName(gameObject.layer))
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }
        else
        {
            return -1;
        }
    }
}
