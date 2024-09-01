using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.U2D.Animation;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{



    [Header("Movement")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpStartTime;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheckPosition;

    [Header("Attack")]
    [SerializeField] private Transform attackPosition;
    [SerializeField] private GameObject attackParts;

    [Header("References")]
    [SerializeField] private SpriteLibraryAsset characterRedSprites;
    [SerializeField] private SpriteLibraryAsset characterYellowSprites;
    [SerializeField] private SpriteLibrary spriteLibrary;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject changeHoodParticles;
    [SerializeField] private Light2D globalLight;
    [SerializeField] private GameObject mainTileMap;
    [SerializeField] private GameObject altTileMap;
    [SerializeField] private GameObject mainBackground;
    [SerializeField] private GameObject altBackground;

    private Rigidbody2D rb;
    private Animator animator;
    private AudioManager audioManager;
    private Light2D playerLight;
    private bool isJumping = false;
    private float jumpTime = 0;
    private float moveHorizontal;
    private float movementSpeed;
    private bool changingHood;
    private bool isRedHood = true;
    private bool isGrounded = false;
    private bool canDoubleJump = false;
    private bool canMove = true;
    private Vector3 lastGroundPosition;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioManager = FindObjectOfType<AudioManager>();
        movementSpeed = walkSpeed;
        playerLight = GetComponentInChildren<Light2D>();
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
        GroundCheck();
        ChangeHood();
    }

    void FixedUpdate()
    {
        rb.velocity = new Vector2(moveHorizontal * movementSpeed, rb.velocity.y);
    }

    private void ChangeHood()
    {
        if (Input.GetKeyDown(KeyCode.F) && !changingHood)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeAll;

            changingHood = true;
           animator.SetTrigger("changeHood");
        }
    }

    public void ChangeSprite()
    {
        // Freeze the player during the animation
        if (isRedHood)
        {

            spriteLibrary.spriteLibraryAsset = characterYellowSprites;
            Instantiate(changeHoodParticles, transform.position, Quaternion.identity);
            // when Yellow is active, we need to change the global light color to D9D700
            globalLight.color = Color.white;
            globalLight.intensity = 1f;
            isRedHood = false;
            mainTileMap.SetActive(false);
            mainBackground.SetActive(false);
            altTileMap.SetActive(true);
            altBackground.SetActive(true);
            playerLight.intensity = 0f;

        }
        else
        {
            canDoubleJump = false;
            spriteLibrary.spriteLibraryAsset = characterRedSprites;
            Instantiate(changeHoodParticles, transform.position, Quaternion.identity);
            // when Red is active, we need to change the global light color to #3A3A3A
            globalLight.color = new Color(0.227f, 0.227f, 0.227f);
            globalLight.intensity = 0.8f;
            isRedHood = true;
            mainTileMap.SetActive(true);
            mainBackground.SetActive(true);
            altTileMap.SetActive(false);
            altBackground.SetActive(false);
            playerLight.intensity = 1f;
        }
        // Unfreeze the player after the animation is done
        // we only need to constraint the Z axis
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        // Add a little force in case that the player is stuck in mid air
        if(!isGrounded) rb.AddForce(Vector2.down * 0.1f, ForceMode2D.Impulse);
        // Play the sound
        audioManager.PlaySFX(audioManager.changeHood);
        // Toggle the main theme
        audioManager.ToggleMainTheme();
        changingHood = false;
    }

    
    public void TakeDamage()
    {
       Debug.Log("Player took damage");
        StartCoroutine(PlayerKnockback());
       
    }

    private void Movement()
    {
        if (!canMove) return;
        moveHorizontal = Input.GetAxis("Horizontal");
        // Flip the player sprite depending on the direction
        transform.localScale = moveHorizontal > 0 ? new Vector3(1, 1, 1) : moveHorizontal < 0 ? new Vector3(-1, 1, 1) : transform.localScale;
       
        // Sprint default on Yellow mode
       // Can double jump on Yellow mode
       if(Input.GetMouseButtonDown(0))
        {
            animator.SetTrigger("attack");
        }
        if(isRedHood)
        {
            movementSpeed = walkSpeed;
            animator.SetBool("isRunning", false);
            animator.SetBool("isWalking", moveHorizontal != 0);
        }
        else
        {
            movementSpeed = runSpeed;
            animator.SetBool("isRunning", moveHorizontal != 0);
            animator.SetBool("isWalking", false);

        }
        Jump();
    }

    public void Attack()
    {
        // play the attack sound and parts effect
        audioManager.PlaySFX(audioManager.playerAttack);
        // launch a overlap circle to check if the player is in range
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPosition.position, 1f, LayerMask.GetMask("Hurt"));
        foreach (Collider2D enemy in hitEnemies)
        {
            if(enemy.CompareTag("Enemy") )
            {
            Instantiate(attackParts, attackPosition.position, Quaternion.identity);
            enemy.GetComponent<Enemy>().TakeDamage();
            }
        }
    }
    private void Jump()
    {
       
        if (Input.GetKeyDown(KeyCode.Space))
        {
            
            if (isGrounded || canDoubleJump)
            {
                isJumping = true;
                jumpTime = jumpStartTime;
                rb.velocity = Vector2.up * jumpForce;
                PlayJump();
            }
            if (!isGrounded && !isRedHood)
            {
                canDoubleJump = false;
            }
        }
       


        if (Input.GetKey(KeyCode.Space) && isJumping)
        {
            if (jumpTime > 0)
            {
                rb.velocity = Vector2.up * jumpForce;
                jumpTime -= Time.deltaTime;
            }
            else
            {
                isJumping = false;
                
            }
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            isJumping = false;
        }
        
    }

    private void GroundCheck()
        {
        // Check if the player is grounded with a overlap circle
        RaycastHit2D hit = Physics2D.Raycast(groundCheckPosition.position, Vector2.down, 0.1f, groundLayer);
        if (hit.collider != null)
        {
            isGrounded = true;
            animator.SetBool("isGrounded", true);
            if(!isRedHood) canDoubleJump = true;
        }
        else
        {
            isGrounded = false;
            animator.SetBool("isGrounded", false);
        }
    }

    public void Respawn()
    {
        transform.position = lastGroundPosition;
    }

    public void PlayFootstep()
    {
        audioManager.PlaySFXRandomPitch(audioManager.footsteps);
    }

    public void PlayJump()
    {
        animator.SetTrigger("jumping");
        audioManager.PlaySFX(audioManager.jump);
    }

    IEnumerator PlayerKnockback()
    {
        canMove = false;
        rb.velocity = Vector2.zero;
        // push the player back
        if (transform.localScale.x == 1)
        {
            rb.velocity = new Vector2(-1, 1) * 10;
            //rb.AddForce(Vector2.up * 10, ForceMode2D.Impulse); 
        }
        else
        {
            rb.velocity = new Vector2(1, 1) * 10;
            //rb.AddForce(Vector2.up * 10, ForceMode2D.Impulse);

        }
        yield return new WaitForSeconds(0.5f);
        canMove = true;

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(groundCheckPosition.position, groundCheckPosition.position + Vector3.down * 0.1f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Deadzone"))
        {
            Respawn();
        }
        if(collision.CompareTag("Checkpoint"))
        {
            lastGroundPosition = collision.transform.position;
        }
    }

}
