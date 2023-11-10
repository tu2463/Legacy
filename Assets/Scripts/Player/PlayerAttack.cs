using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Rigidbody2D rb;

    private PlayerJump jump;
    private PlayerMovement move;
    private PlayerBlock block;

    public static bool isAttacking; // EnemyController.cs needs this bool so I changed it to public -- Cheyu
    private static bool isPowerAttacking;
    private bool canPowerAttack;

    [SerializeField] GameObject ground;
    private Collider2D groundCollider;

    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D> ();

        jump = GetComponent<PlayerJump> ();
        move = GetComponent<PlayerMovement> ();
        block = GetComponent<PlayerBlock> ();
        anim = GetComponent<Animator> ();
        groundCollider = ground.GetComponent<Collider2D>();

        isAttacking = false;
        isPowerAttacking = false;
        canPowerAttack = true;
    }

    // Update is called once per frame
    void Update()
    {     
        // basic attack   
        if (!jump.getIsJumping() && !move.getIsMoving() && !block.getIsBlocking() && !isPowerAttacking && Input.GetButtonDown("Fire1"))
        {
            //Debug.Log("Attack");
            isAttacking = true;
            anim.SetTrigger("canAttack");
            // I created a EnemyIsAttacked(int damage) function in EnemyHealth.cs. It probably can be used here. -- Cheyu
        }
        else
        {
            isAttacking = false; 
        }

        // power attack
        if (!jump.getIsJumping() && !move.getIsMoving() && !block.getIsBlocking() && !isAttacking && canPowerAttack && Input.GetKeyDown(KeyCode.E))
        {
            isPowerAttacking = true;
            StartCoroutine(PowerAttackRoutine());
        }

        if (!rb.IsTouching(groundCollider))
        {
            isPowerAttacking = true;
            anim.SetBool("isPowerAttackFalling", true);
        }
        else
        {
            isPowerAttacking = false;
            anim.SetBool("isPowerAttackFalling", false);
        }
    }

    private IEnumerator PowerAttackRoutine()
    {
        Debug.Log("Power Attack");
        anim.SetTrigger("canPowerAttack");
        canPowerAttack = false;
        yield return new WaitForSeconds(5f);
        canPowerAttack = true;
    }

    public void powerAttackAddVel()
    {
        rb.velocity = new Vector2(rb.velocity.x, 5f);
    }

    public bool getIsAttacking()
    {
        return isAttacking;
    }

    public bool getIsPowerAttacking()
    {
        return isPowerAttacking;
    }
}
