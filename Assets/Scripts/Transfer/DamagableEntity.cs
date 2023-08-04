using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagableEntity : MonoBehaviour
{
    GlobalManager globalManager;
    public float totalHealth;
    public float health;
    public float damageMultiplier = 1;
    float totalDamage;
    float shakeAggression;
    float shakeTime;
    public GameObject entitySprite;
    public bool invincible;

    private void Awake()
    {
        globalManager = GameObject.Find("Global Manager").GetComponent<GlobalManager>();
    }
    private void LateUpdate()
    {
        if (totalDamage > 0)
        {
            shakeTime = 0.3f;
            shakeAggression = totalDamage / totalHealth * 30;
            Vector3 damageSend = new Vector3(this.transform.position.x, this.transform.position.y, totalDamage);
            globalManager.SendMessage("DamageUI", damageSend);
            if(!invincible)
            {
                health -= totalDamage;
                this.gameObject.SendMessage("Damage",totalDamage, SendMessageOptions.DontRequireReceiver);
            }
            if(health <= 0)
            {
                globalManager.AddCorpse(this.gameObject);
                this.gameObject.SendMessage("Dead",SendMessageOptions.DontRequireReceiver);
                health = 0;
                shakeAggression = 0;
                shakeTime = 0;
            }
            totalDamage = 0;
        }
        ShakeShot();
    }
    // Update is called once per frame
    public void Damage(float damage)
    {
        totalDamage += damage * damageMultiplier;
    }
    void ShakeShot()
    {
        if (shakeTime > 0)
        {
            Vector3 shake = new Vector2(Random.Range(-shakeAggression * shakeTime, shakeAggression * shakeTime), Random.Range(-shakeAggression * shakeTime, shakeAggression * shakeTime));
            shakeTime -= Time.deltaTime;
            entitySprite.transform.localPosition = shake;
            if (shakeTime <= 0)
            {
                shakeTime = 0;
                entitySprite.transform.localPosition = Vector3.zero;
            }
        }
    }
}
