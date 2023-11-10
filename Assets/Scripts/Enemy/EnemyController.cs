using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// possible resource: https://www.youtube.com/watch?v=AD4JIXQDw0s
// next step:
// 1. search for how to make a good combat enemy AI / gameplay strategies in street fighter
// 2. separate the scripts to make them organized

public class EnemyController : MonoBehaviour
{
    public string sceneToLoad;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private EnemyHealth hp;
    public Animator anim;


    public bool EnemyIsAlive = true;
    public bool EnemyIsJumping = false;
    public bool EnemyIsMoving = false;
    public bool EnemyIsAttacking = false;
    public bool EnemyIsBlocking = false;
    public bool EnemyIsEnraged = false; // to do

    GameObject player;
    private PlayerMovement pm;
    private PlayerAttack pa;
    private PlayerHurt ph;

    private float jumpForce; // should it be public? should enemy use the same parameters as player?
    private float EnemyMoveSpeed;
    public float moveDirection;
    private float EnemyWidth;
    public float horizontalDistanceBtw;

    public float attackRange;
    public int enemyDamage;

    [SerializeField] ParticleSystem dust;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        hp = GetComponent<EnemyHealth>();

        sr.flipX = true; // assuming that the enemy is facing right by default (x>0),
                         // then it should be flipped as the game start to face left (facing the player)

        player = GameObject.FindGameObjectWithTag("Player");
        pm = player.GetComponent<PlayerMovement>();
        pa = player.GetComponent<PlayerAttack>();
        ph = player.GetComponent<PlayerHurt>();

        EnemyMoveSpeed = (float)(pm.moveSpeed * 0.5);
        EnemyWidth = GetComponent<SpriteRenderer>().bounds.size.x;
        attackRange = EnemyWidth / 2;
        enemyDamage = (int)Mathf.Ceil((float)(pa.playerDamage * 1.5)); // slightly higher than playerDamage
    }

    // Update is called once per frame

    bool EnemyShouldJump()
    {
        // to do
        //Debug.Log("--");
        //Debug.Log(Input.GetKeyDown(KeyCode.L));
        //Debug.Log(EnemyIsJumping);
        return Input.GetButtonDown("Jump") && !EnemyIsJumping;
    }

    bool EnemyShouldMove()
    {
        return horizontalDistanceBtw >= attackRange * 0.75;
    }

    bool EnemyShouldAttack()
    {
        // to do
        // if player is within attack scope, and player is not attacking, and is not blocking/about to end blocking
        return horizontalDistanceBtw < attackRange && !pa.getIsAttacking();
    }

    bool EnemyShouldBlock()
    {
        return horizontalDistanceBtw <= attackRange * 0.75 // if enemy is withink player's attack range - what's player's attack range?
            && pa.getIsAttacking(); // and player is attacking
    }

    IEnumerator blockRoutine()
    {
        Debug.Log("blockRoutine");
        anim.SetBool("EnemyIsBlocking", true);
        yield return new WaitForSeconds(0.2f);
        EnemyIsBlocking = false;
        anim.SetBool("EnemyIsBlocking", false);
    }

    void EnemyFlip()
    {
        if (moveDirection > 0f) // md>0, E is on P's right, should flip
        {
            sr.flipX = true; // assuming that the enemy is facing right by default (x>0)
        }
        else 
        {
            sr.flipX = false;
        }
    }

    void EnemyMove()
    {
        EnemyFlip();
        Vector2 target = new Vector2(player.transform.position.x, rb.position.y); //always moving towards the player
        Vector2 newPos = Vector2.MoveTowards(rb.position, target, EnemyMoveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);
        dust.Play();
    }

    IEnumerator attackRoutine()
    {
        Debug.Log("attack r");
        anim.SetTrigger("EnemyIsAttacking");
        yield return new WaitForSeconds(0.2f);
        EnemyIsAttacking = false;
        yield return new WaitForSeconds(10f);
    }

    void EnemyAttack()
    {
        // to do
        // if player is withink enemy's attack scope, and player is not blocking, then player's health goes down
        // make sure that player's health only decreases once, either in this script or PlayerHurt.cs
        // should enemy's damage on the player > player's damage on the enemy?
        ph.playerIsAttacked(enemyDamage);
    }

    void Update()
    {
        if (EnemyIsAlive) {
            horizontalDistanceBtw = Mathf.Abs(player.transform.position.x - rb.position.x);
            moveDirection = rb.position.x - player.transform.position.x; // if enemy is on player's right, md > 0

            if (EnemyShouldMove() && !EnemyIsJumping
                && !EnemyIsBlocking) // enemy should be allowed to move while attacking?
            {
                EnemyIsMoving = true;
                anim.SetBool("EnemyIsMoving", true);
                EnemyMove();
            }
            else
            {
                EnemyIsMoving = false;
                anim.SetBool("EnemyIsMoving", false);

            }

            if (EnemyShouldJump() && !EnemyIsAttacking && !EnemyIsBlocking) // allowed to jump while moving
            {
                Debug.Log("is jumping");
                //rb.velocity = Vector2.up * jumpForce;
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }
            
            if (EnemyShouldAttack() && !EnemyIsJumping
                && !EnemyIsMoving && !EnemyIsBlocking)
            {
                EnemyIsAttacking = true;
                EnemyAttack();
                StartCoroutine(attackRoutine());
            }

            if (EnemyShouldBlock() && !EnemyIsJumping && !EnemyIsAttacking) // allowed to block while moving
            {
                Debug.Log("is blocking");
                EnemyIsBlocking = true;
                StartCoroutine(blockRoutine());
            }
            
        }
        else {
            PlayerPrefs.SetInt("Win", 1);
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        { // if the obj that player is colliding with has the tag 'ground'
            EnemyIsJumping = false;
            anim.SetBool("EnemyIsJumping", false);
        }
        if (other.gameObject.CompareTag("Player")) {
            StartCoroutine(FlashRoutine());
            //default damage is 5, maybe change it for normal attack and power attack?
            hp.EnemyIsAttacked(5);
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            EnemyIsJumping = true;
            anim.SetBool("EnemyIsJumping", true);
        }
    }

    private IEnumerator FlashRoutine() {
        sr.color = Color.black;
        yield return new WaitForSeconds(0.2f);
        sr.color = Color.red;
    }
}
