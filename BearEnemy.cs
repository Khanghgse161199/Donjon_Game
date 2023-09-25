using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearEnemy : MonoBehaviour, IDamge
{

    private float maxhealth = 20f;
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    public void ApplyDamage(float amount, bool isDamage)
    {
        //maxhealth -= amount;
    }

    void Update()
    {
        if (maxhealth <= 0) Destroy(gameObject);
        animator.SetInteger("stateb", 0);
    }
}
