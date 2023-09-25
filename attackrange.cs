using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class attackrange : MonoBehaviour
{
    [SerializeField] private Transform parentTransform;
    private SpriteRenderer spriteRenderer;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }


    void Update()
    {
        parentTransform = transform.parent;

        if (parentTransform.localScale.x < 0)
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
        }
    }
}
