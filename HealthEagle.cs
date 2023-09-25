using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthEagle : MonoBehaviour, IDamge
{
    private float health;
    private Animator animator;
    private bool isAttack = false;
    public float timeAttackDelay;
    private float timeDecayDelayTmp;

    void Start()
    {
        health = 8f;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (health <= 0) Destroy(gameObject);

        if (isAttack)
        {
            animator.SetInteger("statee", 1);
        }
        else animator.SetInteger("statee", 0);

        if (timeDecayDelayTmp < 0)
        {
            isAttack = false;
        }else timeDecayDelayTmp -= Time.deltaTime;

       
    }

    public void ApplyDamage(float amount, bool isDamage)
    {
        if (!isAttack)
        {
            health -= amount;
            isAttack = true;
            timeDecayDelayTmp = timeAttackDelay;
        }
    }
}
