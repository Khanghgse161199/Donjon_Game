using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using UnityEngine.UI;

public class MovingPlayer : MonoBehaviour, IDamge
{
    private Rigidbody2D rb;
    private BoxCollider2D coll;
    private Animator animator;
    private SpriteRenderer sprite;

    [Header("Attributes")]
    [SerializeField] public float healthPool = 20f;
    [SerializeField] public Slider healthPlayer;

    [SerializeField] private LayerMask jumpableGround;
    [SerializeField] private LayerMask ElevatorGround;
    [SerializeField] private LayerMask FlashEnemies;
    [SerializeField] private LayerMask FlashBats;
    [SerializeField] private LayerMask FlashBear;

    private float dirX;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float RollForce;
    [SerializeField] private float FlashForce;
    [SerializeField] private bool isBlocking = false;
    [SerializeField] private bool isSlash = false;
    [SerializeField] private bool isRoll = false;
    [SerializeField] private bool isFlash = false;
    [SerializeField] private bool isAttack = false;
    [SerializeField] private bool IsDie = false;
    [SerializeField] private bool IsElevator= false;
    [SerializeField] private bool blockAttack = false;
    [SerializeField] private float attackDelay;
    [SerializeField] private float attackDelay2;
    [SerializeField] private float attackDelay3;
    [SerializeField] private float rollDelay;
    [SerializeField] private float flashDelay;
    [SerializeField] private float maxDistance;
    [SerializeField] private float timeAttackDelay;
    [SerializeField] private float timeDieDelay;
    private float currentHealth;
    private float timeAttackDelayTmp;
    private float timeDieDelayTmp;
    private float attackDelayTmp;
    private float rollDelayTmp;
    private float flashDelayTmp;
    private int skill = 0;

    [Header("combat")]
    [SerializeField] private Transform meleeAttackOrigin;
    public float meleeAttackRadius = 0.6f;
    public float meleedamage = 2f;
    public float timeUntilMeleeReadied = 0;


    private enum MovementState { idle, running, jumping, falling, block, slash, slash2, slash3, roll, flash, attack, die, blockAttack }
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        coll = GetComponent<BoxCollider2D>();
        currentHealth = healthPool; 
        timeAttackDelayTmp = timeAttackDelay;
        timeDieDelayTmp = timeDieDelay;
        healthPlayer.maxValue = healthPool;
    }

    // Update is called once per frame
    void Update()
    {
        healthPlayer.value = currentHealth;

        // Left- Right
        if (!isBlocking && !isSlash && !isAttack)
        {
            dirX = Input.GetAxis("Horizontal");
        }

        if (dirX != 0)
        {
            rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);
        }
        else if (dirX != 0 && isBlocking)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            dirX = 0;
        }
        else if (dirX != 0 && isSlash)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }

        // Jump
        if (Input.GetButtonDown("Jump") && IsGround() || Input.GetButtonDown("Jump") && IsOnElevator())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        // Block
        if (Input.GetKeyDown(KeyCode.LeftShift) && IsGround())
        {
            isBlocking = true;
            rb.velocity = new Vector2(0, rb.velocity.y);
            dirX = 0;
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isBlocking = false;
        }

        // Slash
        if (Input.GetKey(KeyCode.LeftControl) && !isSlash && !isBlocking && !isAttack)
        {
            isSlash = true;
            skill++;
            if (skill == 1)
            {
                attackDelayTmp = attackDelay;
            }
            else if (skill == 2)
            {
                attackDelayTmp = attackDelay2;
            }
            else if (skill == 3)
            {
                attackDelayTmp = attackDelay3;
            }
            else skill = 0;

            HandleAttack();
            HandleBat();
            HandleBear();
        }
        

        if (attackDelayTmp <= 0)
        {
            isSlash = false;
        }
        else attackDelayTmp -= Time.deltaTime;

        if(timeUntilMeleeReadied > 0)
        {
            timeUntilMeleeReadied -= Time.deltaTime;
        }

        // Roll
        if (Input.GetKey(KeyCode.Z) && !isRoll && IsGround() && !isSlash && !isBlocking)
        {
            isRoll = true;
            rollDelayTmp = rollDelay;
        }
        else rollDelayTmp -= Time.deltaTime;

        if (rollDelayTmp <= 0) isRoll = false;

        //Flash
        if (Input.GetKey(KeyCode.X) && !isFlash && IsGround() && CheckEnemies() && !isSlash && !isBlocking && !isAttack)
        {
            isFlash = true;
            SetStatic();
            flashDelayTmp = flashDelay;

        }
        else flashDelayTmp -= Time.deltaTime;

        if (flashDelayTmp <= 0)
        {
            isFlash = false;
            SetStatic();
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        //IsAttack
        if (isAttack)
        {
            timeAttackDelayTmp -= Time.deltaTime;
        }

        if (timeAttackDelayTmp < 0) isAttack = false;

        //IsDie
        if (IsDie)
        {
            timeDieDelayTmp -= Time.deltaTime;

        }

        if (timeDieDelayTmp < 0) IsDie = false;

        // Animation
        UpdateAnimationState();
       
    }

    private void UpdateAnimationState()
    {
        MovementState state;

        if (dirX > 0f && !isBlocking)
        {
            state = MovementState.running;
            sprite.flipX = false;
        }
        else if (dirX < 0f && !isBlocking)
        {
            state = MovementState.running;
            sprite.flipX = true;
        }
        else
        {
            state = MovementState.idle;
        }

        if (rb.velocity.y > .1f)
        {
            state = MovementState.jumping;
        }
        else if (rb.velocity.y < -.1f)
        {
            state = MovementState.falling;
        }   

        if (isBlocking && !blockAttack)
        {
            state = MovementState.block;
        }
        else if (isBlocking && blockAttack)
        {
            state = MovementState.blockAttack;
        }


        if (isSlash && skill == 1)
        {
            state = MovementState.slash;
        }
        else if (isSlash && skill == 2)
        {
            state = MovementState.slash2;
        }
        else if (isSlash && skill == 3)
        {
            state = MovementState.slash3;
        }

        if (isRoll && !sprite.flipX)
        {
            state = MovementState.roll;
            rb.velocity = new Vector2(RollForce, rb.velocity.y);
        }
        else if (isRoll && sprite.flipX)
        {
            state = MovementState.roll;
            rb.velocity = new Vector2(RollForce * -1, rb.velocity.y);
        }

        if (isFlash && !sprite.flipX)
        {
            state = MovementState.flash;
            rb.velocity = new Vector2(FlashForce, 0);
        }
        else if (isFlash && sprite.flipX)
        {
            state = MovementState.flash;
            rb.velocity = new Vector2(FlashForce * -1, 0);
        }

        if (isAttack)
        {
            state = MovementState.attack;
        }

        if (IsDie)
        {
            state = MovementState.die;
        }

        animator.SetInteger("state", (int)state);

    }

    private void setActive()
    {
        Destroy(gameObject, 2.5f);
    }

    private bool IsGround()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .01f, jumpableGround);
    }

    private bool IsOnElevator()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .01f, ElevatorGround);
    }

    private bool CheckEnemies()
    {
        if (!sprite.flipX)
        {
            return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.right, maxDistance, FlashEnemies);
        }
        else if (sprite.flipX)
        {
            return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.left, maxDistance, FlashEnemies);
        }
        else return false;
    }

    private bool checkRadius()
    {
        Collider2D[] overlappedColliders;
        if (sprite.flipX)
        {
            meleeAttackOrigin.position = new Vector3(transform.position.x - 0.65f, transform.position.y + 0.8f, transform.position.z);
            overlappedColliders = Physics2D.OverlapCircleAll(meleeAttackOrigin.position, meleeAttackRadius, FlashEnemies);
        }
        else
        {
            meleeAttackOrigin.position = new Vector3(transform.position.x + 0.65f, transform.position.y + 0.8f, transform.position.z);
            overlappedColliders = Physics2D.OverlapCircleAll(meleeAttackOrigin.position, meleeAttackRadius, FlashEnemies);
        }
        if (overlappedColliders != null && overlappedColliders.Length > 0 && overlappedColliders.Count() > 0)
        {
            return true;
        }
        else return false;
    }

    private bool checkBulletRadius()
    {
        Collider2D[] overlappedColliders;
        if (sprite.flipX)
        {
            meleeAttackOrigin.position = new Vector3(transform.position.x - 0.65f, transform.position.y + 0.8f, transform.position.z);
            overlappedColliders = Physics2D.OverlapCircleAll(meleeAttackOrigin.position, meleeAttackRadius, FlashBats);
        }
        else
        {
            meleeAttackOrigin.position = new Vector3(transform.position.x + 0.65f, transform.position.y + 0.8f, transform.position.z);
            overlappedColliders = Physics2D.OverlapCircleAll(meleeAttackOrigin.position, meleeAttackRadius, FlashBats);
        }
        if (overlappedColliders != null && overlappedColliders.Length > 0 && overlappedColliders.Count() > 0)
        {
            return true;
        }
        else return false;
    }

    private void SetStatic()
    {
        Vector2 playerPosition = transform.position;
        Vector2 forwardDirection = transform.right;

        var hit = Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.right, 3f, FlashEnemies);
        var hit2 = Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.left, 3f, FlashEnemies);
        if (hit.collider != null && hit.collider.CompareTag("enemies"))
        {
            float distanceToEnemy = hit.distance;

            Rigidbody2D enemyRigidbody = hit.collider.GetComponent<Rigidbody2D>();
            if (distanceToEnemy > 0f && distanceToEnemy <= 2f && isFlash)
            {

                if (enemyRigidbody != null)
                {
                    enemyRigidbody.bodyType = RigidbodyType2D.Static;
                    rb.bodyType = RigidbodyType2D.Kinematic;
                }
                else enemyRigidbody.bodyType = RigidbodyType2D.Dynamic;
            }
            else enemyRigidbody.bodyType = RigidbodyType2D.Dynamic;
        }
        else if (hit2.collider != null && hit2.collider.CompareTag("enemies"))
        {
            float distanceToEnemy = hit2.distance;

            Rigidbody2D enemyRigidbody = hit2.collider.GetComponent<Rigidbody2D>();
            if (distanceToEnemy > 0f && distanceToEnemy <= 2f && isFlash)
            {

                if (enemyRigidbody != null)
                {
                    enemyRigidbody.bodyType = RigidbodyType2D.Static;
                    rb.bodyType = RigidbodyType2D.Kinematic;
                }
                else enemyRigidbody.bodyType = RigidbodyType2D.Dynamic;
            }
            else enemyRigidbody.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    private void HandleAttack()
    {
        if (isSlash)
        {
            Collider2D[] overlappedColliders;
            if (sprite.flipX)
            {
                meleeAttackOrigin.position = new Vector3(transform.position.x - 0.65f, transform.position.y + 0.8f, transform.position.z);
                overlappedColliders = Physics2D.OverlapCircleAll(meleeAttackOrigin.position, meleeAttackRadius, FlashEnemies);            
            }
            else
            {
                meleeAttackOrigin.position = new Vector3(transform.position.x + 0.65f, transform.position.y + 0.8f, transform.position.z);
                overlappedColliders = Physics2D.OverlapCircleAll(meleeAttackOrigin.position, meleeAttackRadius, FlashEnemies);
            }
            for (int i = 0; i < overlappedColliders.Length; i++)
            {
                IDamge enemyAttributes = overlappedColliders[i].GetComponent<IDamge>();
                if (enemyAttributes != null)
                {
                    enemyAttributes.ApplyDamage(meleedamage, true);
                }
            }

            //if (skill == 1)
            //{
            //    timeUntilMeleeReadied = attackDelay;
            //}
            //else if (skill == 2)
            //{
            //    timeUntilMeleeReadied = attackDelay2;
            //}
            //else if (skill == 3)
            //{
            //    timeUntilMeleeReadied = attackDelay3;
            //}
        }
    }

    private void HandleBat()
    {
        if (isSlash)
        {
            Collider2D[] overlappedColliders;
            if (sprite.flipX)
            {
                meleeAttackOrigin.position = new Vector3(transform.position.x - 0.75f, transform.position.y + 0.8f, transform.position.z);
                overlappedColliders = Physics2D.OverlapCircleAll(meleeAttackOrigin.position, meleeAttackRadius, FlashBats);
            }
            else
            {
                meleeAttackOrigin.position = new Vector3(transform.position.x + 0.75f, transform.position.y + 0.8f, transform.position.z);
                overlappedColliders = Physics2D.OverlapCircleAll(meleeAttackOrigin.position, meleeAttackRadius, FlashBats);
            }
            for (int i = 0; i < overlappedColliders.Length; i++)
            {
                IDamge enemyAttributes = overlappedColliders[i].GetComponent<IDamge>();
                if (enemyAttributes != null)
                {
                    enemyAttributes.ApplyDamage(2, true);
                }
            }
        }
    }
    private void HandleBear()
    {
        if (isSlash)
        {
            Collider2D[] overlappedColliders;
            if (sprite.flipX)
            {
                meleeAttackOrigin.position = new Vector3(transform.position.x - 0.75f, transform.position.y + 0.8f, transform.position.z);
                overlappedColliders = Physics2D.OverlapCircleAll(meleeAttackOrigin.position, meleeAttackRadius, FlashBear);
            }
            else
            {
                meleeAttackOrigin.position = new Vector3(transform.position.x + 0.75f, transform.position.y + 0.8f, transform.position.z);
                overlappedColliders = Physics2D.OverlapCircleAll(meleeAttackOrigin.position, meleeAttackRadius, FlashBear);
            }
            for (int i = 0; i < overlappedColliders.Length; i++)
            {
                IDamge enemyAttributes = overlappedColliders[i].GetComponent<IDamge>();
                if (enemyAttributes != null)
                {
                    enemyAttributes.ApplyDamage(2, true);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Debug.DrawRay(transform.position, -Vector2.up * FlashEnemies, Color.green);
        if (meleeAttackOrigin != null)
        {
            Gizmos.DrawWireSphere(meleeAttackOrigin.position, meleeAttackRadius);
        }
    }

    public void ApplyDamage(float amount, bool isDamage)
    {
        if (!isAttack && !IsDie && !isFlash)
        {
            if (isBlocking && checkRadius() || isBlocking && checkBulletRadius())
            {
                blockAttack = true;
            }
            else
            {
                blockAttack = false;
                isAttack = true;
                currentHealth -= amount;
                timeAttackDelayTmp = timeAttackDelay;
                if (currentHealth <= 0)
                {
                    Debug.Log("player die");
                    Die(true);
                }
            }
        }
    }

    private void Die(bool die)
    {
        IsDie = die;
        timeDieDelayTmp = timeDieDelay;
    }

    private void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("bullet"))
        {
            ApplyDamage(1, true);
            Debug.Log("Dinh");
            Destroy(collision.gameObject);
        }
    }
}
