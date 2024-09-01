using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Transform player;
    [SerializeField] private float attackRange;
    [SerializeField] private Transform[] wayPoints;
    [SerializeField] private LayerMask hurtLayer;
    [SerializeField] private Transform attackPosition;
    [SerializeField] private GameObject attackParts;

    private Rigidbody2D rb;
    private Animator animator;
    private AudioManager audioManager;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioManager = FindObjectOfType<AudioManager>();
        StartCoroutine(Patrol());
    }

   public void TakeDamage()
    {
        animator.SetTrigger("hit");
        StopAllCoroutines();
        StartCoroutine(RestartPatrol());
    }

    public void Attack()
    {

        // play the attack sound and parts effect
        audioManager.PlaySFX(audioManager.enemyAttack);
        Instantiate(attackParts, attackPosition.position, Quaternion.identity);
        // launch a overlap circle to check if the player is in range
        Collider2D[] hitPlayer = Physics2D.OverlapCircleAll(attackPosition.position, attackRange, hurtLayer);
        foreach (Collider2D other in hitPlayer)
        {
            if (other.CompareTag("Player"))
            {
                other.GetComponentInParent<Player>().TakeDamage();
            }
        }
    }

    public virtual IEnumerator Patrol()
    {
        while (true)
        {
            for (int i = 0; i < wayPoints.Length; i++)
            {
                yield return StartCoroutine(Move(wayPoints[i]));
            }
        }
    }

    public virtual IEnumerator Move(Transform wayPoint)
    {
        while (transform.position != wayPoint.position)
        {
            transform.position = Vector3.MoveTowards(transform.position, wayPoint.position, speed * Time.deltaTime);
            animator.SetBool("running", true);
            ViewToDirection(wayPoint);
            yield return null;
        }
    }

    public virtual void ViewToDirection(Transform direction)
    {
        if (direction.position.x < transform.position.x)
        {
            transform.eulerAngles = Vector3.zero;
        }
        else if (direction.position.x > transform.position.x)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player Hit");
            animator.SetTrigger("attack");
            animator.SetBool("running", false);
            StopAllCoroutines();
            StartCoroutine(RestartPatrol());
        }
    }

    IEnumerator RestartPatrol()
    {
        yield return new WaitForSeconds(2f);
        StartCoroutine(Patrol());
    }
}
