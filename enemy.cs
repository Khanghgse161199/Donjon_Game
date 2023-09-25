using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

public class enemy : MonoBehaviour, IDamge
{
    private Animator animator;
    //private SpriteRenderer spirte;
    public Transform player;
    public Transform meleeAttackOrigin;
    public LayerMask FlashGround;
    public LayerMask FlashPlayer;
    public float meleeAttackRadius;
    public float timeUntilMeleeReadied = 0;
    //private Rigidbody2D rb;
    private BoxCollider2D coll;
    public float meleedamage = 2f;

    [Header("Attributes")]
    public float healthPool = 20f;

    [Header("Movement")]
    //public float speed = 5f;
    //public float jumpForce = 6f;
    //public float groundedLeeway = 0.1f;
    private float currentHealth = 20f;
    private bool isAttack = false;
    private bool IsDie = false;
    private bool IsFindPlayer = false;
    private bool isFlipped = false;
    private bool isSlash = false;

    [SerializeField] private float timeAttackDelay;
    [SerializeField] private float timeDieDelay;
    [SerializeField] private float timeSlashDelay;
    [SerializeField] public float timeDirectionDelay;
    private float timeAttackDelayTmp;
    private float timeDieDelayTmp;
    private float timeSlashDelayTmp;
    private float timeDirectionDelayTmp;
   
    private enum MovementState { idle, die, mattack, run, slash }
    void Start()
    {
        currentHealth = healthPool;
        animator = GetComponent<Animator>();
        //spirte = GetComponent<SpriteRenderer>();
        //rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isAttack)
        {
            timeAttackDelayTmp -= Time.deltaTime;
        }

        if (IsDie)
        {
            timeDieDelayTmp -= Time.deltaTime;
            if (timeDieDelayTmp < (-0.2f)) CompleteLevel();
        }

        if (isSlash)
        {
            IsFindPlayer = false;
            timeSlashDelayTmp -= Time.deltaTime;
        }

        if (timeDirectionDelayTmp >= 0)
        {
            timeDirectionDelayTmp -= Time.deltaTime;
        }

        if (timeAttackDelayTmp < 0) isAttack = false;

        if (timeDieDelayTmp < 0) setActive();

        if (timeSlashDelayTmp < 0) isSlash = false;

        LookAtPlayer();

        HandleInRadius();

        UpdateAnimationEstate();
    }

    private void UpdateAnimationEstate()
    {
        MovementState state;

        if (isAttack)
        {
            state = MovementState.mattack;
        }
        else if (IsDie)
        {
            state = MovementState.die;
        }
        else if (isSlash)
        {
            state = MovementState.slash;
        }
        else if (IsFindPlayer)
        {
            state = MovementState.run;
        }
        else
        {
            state = MovementState.slash;
        }


        animator.SetInteger("estate", (int)state);
    }

    public virtual void ApplyDamage(float amount, bool isDamage)
    {
        if (!isAttack && !IsDie)
        {
            isAttack = true;
            currentHealth -= (amount - 1);
            Debug.Log(currentHealth);
            timeAttackDelayTmp = timeAttackDelay;
            if (currentHealth <= 0 && !IsDie)
            {
                Die(true);
            }
        }
    }

    private void Die(bool isDie)
    {
        IsDie = isDie;
        timeDieDelayTmp = timeDieDelay;
    }

    private void setActive()
    {
        Destroy(gameObject, 2.5f);
    }

    public void LookAtPlayer()
    {
        Vector3 flipped = transform.localScale;
        flipped.z *= -1f;

        if (transform.position.x > player.position.x && isFlipped)
        {
            transform.localScale = flipped;
            transform.Rotate(0f, 180f, 0f);
            isFlipped = false;
            timeDirectionDelayTmp = timeDirectionDelay;

        }
        else if (transform.position.x < player.position.x && !isFlipped)
        {
            transform.localScale = flipped;
            transform.Rotate(0f, 180f, 0f);
            isFlipped = true;
            timeDirectionDelayTmp = timeDirectionDelay;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Debug.DrawRay(transform.position, -Vector2.up * FlashPlayer, Color.green);
        if (meleeAttackOrigin != null)
        {
            Gizmos.DrawWireSphere(meleeAttackOrigin.position, meleeAttackRadius);
        }
    }

    private bool IsGround()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .01f, FlashGround);
    }

    private void HandleInRadius()
    {
        if (!isSlash && !IsDie && timeUntilMeleeReadied <= 0 && timeDirectionDelayTmp < 0)
        {
            Collider2D[] overlappedColliders;
            if (!isFlipped)
            {
                meleeAttackOrigin.position = new Vector3(transform.position.x - 0.65f, transform.position.y + 0.8f, transform.position.z);
                overlappedColliders = Physics2D.OverlapCircleAll(meleeAttackOrigin.position, meleeAttackRadius, FlashPlayer);
            }
            else
            {
                meleeAttackOrigin.position = new Vector3(transform.position.x + 0.65f, transform.position.y + 0.8f, transform.position.z);
                overlappedColliders = Physics2D.OverlapCircleAll(meleeAttackOrigin.position, meleeAttackRadius, FlashPlayer);
            }
            if (overlappedColliders != null && overlappedColliders.Count() > 0)
            {
                for (int i = 0; i < overlappedColliders.Length; i++)
                {
                    IDamge palyerAttributes = overlappedColliders[i].GetComponent<IDamge>();
                    if (palyerAttributes != null && IsGround() && !isAttack)
                    {
                        isSlash = true;
                        IsFindPlayer = false;
                        palyerAttributes.ApplyDamage(meleedamage, true);
                    }
                    else IsFindPlayer = true;
                }
            }
            else IsFindPlayer = true;

            timeUntilMeleeReadied = timeSlashDelay;
        }
        else
        {
            timeUntilMeleeReadied -= Time.deltaTime;
        }
    }
    private void CompleteLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

}
