using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BucDamge : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spirte;
    public Transform player;
    public Transform meleeAttackOrigin;
    public LayerMask FlashPlayer;

    private BoxCollider2D coll;

    private bool isAttack = false;
    private bool isFlipped = false;
    private bool IsFindPlayer = false;
    public float meleeAttackRadius;
    public float timeUntilMeleeReadied = 0;

    [SerializeField] private float timeAttackDelay;
    [SerializeField] public float timeDirectionDelay;
    [SerializeField] private float timeSlashDelay;

    private float timeAttackDelayTmp;
    private float timeDirectionDelayTmp;
    private float timeSlashDelayTmp;
    private enum MovementState { idle, die, mattack, run, slash }
    void Start()
    {
        animator = GetComponent<Animator>();
        spirte = GetComponent<SpriteRenderer>();
        coll = GetComponent<BoxCollider2D>();
    }
    void Update()
    {

        if (timeDirectionDelayTmp >= 0)
        {
            timeDirectionDelayTmp -= Time.deltaTime;
        }

        if(timeSlashDelayTmp >= 0)
        {
            timeSlashDelayTmp -= Time.deltaTime;
        }

        LookAtPlayer();

        HandleInRadius();
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
        if (timeUntilMeleeReadied <= 0 && timeDirectionDelayTmp < 0)
        {
            Collider2D[] overlappedColliders;
            if (!isFlipped)
            {
                //meleeAttackOrigin.position = new Vector3(transform.position.x - 0.65f, transform.position.y + 0.8f, transform.position.z);
                overlappedColliders = Physics2D.OverlapCircleAll(meleeAttackOrigin.position, meleeAttackRadius, FlashPlayer);
            }
            else
            {
                //meleeAttackOrigin.position = new Vector3(transform.position.x + 0.65f, transform.position.y + 0.8f, transform.position.z);
                overlappedColliders = Physics2D.OverlapCircleAll(meleeAttackOrigin.position, meleeAttackRadius, FlashPlayer);
            }
            if (overlappedColliders != null && overlappedColliders.Count() > 0)
            {
                for (int i = 0; i < overlappedColliders.Length; i++)
                {
                    IDamge palyerAttributes = overlappedColliders[i].GetComponent<IDamge>();
                    if (palyerAttributes != null)
                    {
                        palyerAttributes.ApplyDamage(1f, true);
                        timeSlashDelayTmp = timeSlashDelay;
                        timeUntilMeleeReadied = timeSlashDelay;
                    }
                    else IsFindPlayer = true;
                }
            }
            else IsFindPlayer = true;
        }
        else
        {
            timeUntilMeleeReadied -= Time.deltaTime;
        }
    }
}
