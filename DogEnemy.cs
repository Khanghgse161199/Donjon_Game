using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class DogEnemy : MonoBehaviour, IDamge
{
    [SerializeField] private float speed = 2.2f;
    [SerializeField] private bool checkRadius = false;
    [SerializeField] private bool isFlipped = false;
    [SerializeField] private LayerMask FlashPlayer;
    [SerializeField] private GameObject[] waypoints;
    [SerializeField] private SpriteRenderer sprite;

    [SerializeField] UnityEngine.Transform player;
    [SerializeField] private float timeDirectionDelay;
    public UnityEngine.Transform meleeAttackOrigin;
    private Animator animator;
    private BoxCollider2D coll;
    private bool isFinding = false;
    private float timeDirectionDelayTmp;
    public float meleeAttackRadius;
    public float timeUntilMeleeReadied = 0;
    public float timeSlashDelay;
    private int currentWaypointIndex = 0;
    private float health = 14f;

    private enum MovementState { idle, running}
    void Start()
    {
        coll = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        timeUntilMeleeReadied = timeSlashDelay;
    }

    
    void Update()
    {
        Finding();
        HandleInRadius();
        if (timeUntilMeleeReadied > 0)
        {
            timeUntilMeleeReadied -= Time.deltaTime;
        }
        if(health <= 0) Destroy(gameObject);
    }

    private void Finding()
    {
        MovementState state;

        var hit = Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.left, 9f, FlashPlayer);
        var hit2 = Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.right, 9f, FlashPlayer);
        if (hit.collider != null && hit.collider.tag == "Player" && checkStarEndPoint())
        {
            var distance = new Vector2(hit.collider.transform.position.x, (hit.collider.transform.position.y + 1f));
            transform.position = Vector2.MoveTowards(transform.position, distance, Time.deltaTime * speed);
            state = MovementState.running;
            isFinding = true;
            LookAtPlayer();
        }else if (hit2.collider != null && hit2.collider.tag == "Player" && checkStarEndPoint())
        {
            var distance = new Vector2(hit2.collider.transform.position.x, (hit2.collider.transform.position.y + 1f));
            transform.position = Vector2.MoveTowards(transform.position, distance, Time.deltaTime * speed);
            state = MovementState.running;
            isFinding = true;

            LookAtPlayer();
        }
        else
        {
            state = MovementState.idle; 
            isFinding = false;
        }

        animator.SetInteger("stated", (int)state);
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


    private void HandleInRadius()
    {
        if (timeUntilMeleeReadied <= 0 && timeDirectionDelayTmp <= 0 && isFinding)
        {
            Collider2D[] overlappedColliders;
            if (!isFlipped)
            {
                meleeAttackOrigin.position = new Vector3(transform.position.x - 0.65f, transform.position.y + 0f, transform.position.z);
                overlappedColliders = Physics2D.OverlapCircleAll(meleeAttackOrigin.position, meleeAttackRadius, FlashPlayer);
            }
            else
            {
                meleeAttackOrigin.position = new Vector3(transform.position.x + 0.65f, transform.position.y + 0f, transform.position.z);
                overlappedColliders = Physics2D.OverlapCircleAll(meleeAttackOrigin.position, meleeAttackRadius, FlashPlayer);
            }
            if (overlappedColliders != null && overlappedColliders.Count() > 0)
            {
                for (int i = 0; i < overlappedColliders.Length; i++)
                {
                    IDamge palyerAttributes = overlappedColliders[i].GetComponent<IDamge>();
                    if (palyerAttributes != null)
                    {
                        palyerAttributes.ApplyDamage(1, true);
                        timeUntilMeleeReadied = timeSlashDelay;
                    }
                }
            }
        }
    }

    private bool checkStarEndPoint()
    {
        if (Vector2.Distance(waypoints[currentWaypointIndex].transform.position, transform.position) < 3f)
        {
            return false;
        }
        else if (Vector2.Distance(waypoints[currentWaypointIndex + 1].transform.position, transform.position) < 3f)
        {
            return false;
        }
        else return true;
    }

    public void ApplyDamage(float amount, bool isDamage)
    {
        health -= amount;
    }
}
