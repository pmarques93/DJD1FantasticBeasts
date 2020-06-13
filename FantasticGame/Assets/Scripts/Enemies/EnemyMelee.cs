﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMelee : MonoBehaviour
{
    public Stats Stats { get; private set; }

    [SerializeField] Transform          meleePosition;


    [SerializeField] LayerMask          playerLayer;
    [SerializeField] LayerMask          groundLayer;
    [SerializeField] LayerMask          boxesAndwalls;

    [SerializeField] Transform          groundRangeCheck, groundCheck, wallCheck, backStab;


    [SerializeField] GameObject         ammunitionHit;
    // Drops
    [SerializeField] GameObject         healthPickUp, manaPickUp;

    [SerializeField] float  speed;              // WALKING SPEED
    [SerializeField] float  limitRange;         // RANGE FOR WALKING
    [SerializeField] float  HP;                 // CURRENT HP
    [SerializeField] float  enemyDamage;        // ENEMY DAMAGE
    [SerializeField] float  attackPushForce;    // HOW MUCH WILL ENEMY PUSH THE PLAYER
    [SerializeField] int    lootChance;         // LOOT CHANCE 1 - 10


    public float    Damage { get; private set; }
    public float    PushForce { get; private set; }

    bool            meleeAttack;
    float           attackDelay;        // FIRST ATTACK
    float           nextAttacksDelay;   // AFTER FIRST ATTACK
    Collider2D      atackingCollider;

    float           originalSpeed;
    Vector2         startingPos;
    bool            limitWalkingRangeReached;
    Vector2         tempPosition;
    float           waitingTimeCounter;

    float           canMoveTimer;

    Player      p1;
    Animator    animator;
    private void Awake()
    {
        Stats = new Stats();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        Stats.IsAlive       = true;
        startingPos         = transform.position;
        Stats.CurrentHP     = HP;

        attackDelay                 = 0.75f;
        nextAttacksDelay            = 0.99f;
        Stats.CanMeleeAttack        = false;
        Stats.MeleeAttackDelay      = attackDelay;
        meleeAttack                 = false;


        Stats.MeleeDamage   = enemyDamage;
        Damage              = enemyDamage;
        PushForce           = attackPushForce;


        limitWalkingRangeReached    = false;
        waitingTimeCounter          = Random.Range(1f, 3f);


        originalSpeed       = speed;
    }

    private void Update()
    {
        p1 = FindObjectOfType<Player>();


        // OTHER ATTACKS ANIMATION DELAY
        if (meleeAttack == true)
        {
            Stats.MeleeAttackDelay -= Time.deltaTime;
        }
        if (Stats.MeleeAttackDelay < 0)
        {   // If timeDelay gets < 0, sets timer back to initial delay again and the character can attack
            Melee();
            Stats.MeleeAttackDelay = nextAttacksDelay;
        }
        if (meleeAttack == false)
            Movement();

        //  BACKSTAB ----------------------------------------------------------------------------------
        BackStabCheck();
        //  AIMING CHECK ------------------------------------------------------------------------------
        AimCheck();


        // ALIVE --------------------------------------------------------------------------------------
        if (!(Stats.IsAlive))
        {
            int chance = Random.Range(0, 10);
            if (chance > lootChance)
            {
                if (healthPickUp != null && chance >= 5) Instantiate(healthPickUp, transform.position + new Vector3(0f, 0.2f, 0f), transform.rotation);
                else if (manaPickUp != null) Instantiate(manaPickUp, transform.position + new Vector3(0f, 0.2f, 0f), transform.rotation);
            }
            Stats.Die(gameObject);
        }

        // ANIMATIONS --------------------------------------------------------------------------------
        animator.SetBool("attack", meleeAttack);
        animator.SetBool("limitWalkingRangeReached", limitWalkingRangeReached);
        animator.SetFloat("speed", speed);
        animator.SetFloat("canMoveTimer", canMoveTimer);
    }


    void BackStabCheck()
    {
        Collider2D checkSurround = Physics2D.OverlapCircle(backStab.position, 0.13f, playerLayer);

        if (checkSurround && meleeAttack == false)
        {
            transform.Rotate(0f, 180f, 0f);
            if (limitWalkingRangeReached == true) limitWalkingRangeReached = false;
        }
    }


    // Checks if the the player is in range and if there's an object between the enemy and player
    void AimCheck()
    {
        atackingCollider = Physics2D.OverlapBox(meleePosition.position, new Vector3(0.4f, 0.7f, 0f) , 0f, playerLayer);

        if (atackingCollider != null)
        {
            meleeAttack = true;
            speed = 0;
            // Sets canMoveTimer to attackDelay, to start counting in the beggining of the attack -- Check else condition --
            canMoveTimer = attackDelay;
        }
        else
        {
            // ONLY STARTS MOVING AGAIN ONCE THE CANMOVETIMER IS < 0
            canMoveTimer -= Time.deltaTime;
            if (canMoveTimer < 0)
            {
                // If player moves out of range, resets animation counter to initial value
                Stats.MeleeAttackDelay = attackDelay;

                if (limitWalkingRangeReached == false)          // If it's maximum range
                {
                    speed = originalSpeed;
                    meleeAttack = false;
                }
              
                if (limitWalkingRangeReached && meleeAttack)    // If it's maximum range and has not collider to attack
                {
                    speed = originalSpeed;
                    meleeAttack = false;
                }

                if (limitWalkingRangeReached)   // If the player leaves its reach while his ranged reached is true
                {
                    waitingTimeCounter -= Time.deltaTime;
                    transform.position = tempPosition;
                }

                if (p1 == null) // If it killed the player
                {
                    speed = originalSpeed;
                    meleeAttack = false;
                    limitWalkingRangeReached = false;
                }
            }         
        }
    }

    // Melee Attack
    void Melee()
    {
        meleeAttack = false;
        if (atackingCollider != null)
        {
            p1.Stats.TakeDamage(Damage);
            if (p1.Movement.CrouchGetter) Instantiate(ammunitionHit, p1.transform.position + new Vector3(0f, 0.3f, 0f), p1.transform.rotation);
            else Instantiate(ammunitionHit, p1.transform.position + new Vector3(0f, 0.5f, 0f), p1.transform.rotation);

            // Pushes the player
            if (p1.transform.position.x > transform.position.x)
                p1.Movement.Rb.AddForce(new Vector2(PushForce, 0f));
            else if (p1.transform.position.x < transform.position.x)
                p1.Movement.Rb.AddForce(new Vector2(-PushForce, 0f));
            StartCoroutine(p1.CameraShake.Shake(0.025f, 0.08f));
        }
    }


    // Movement, turns 180 if reaches max position || if collides against a wall || if doesn't detect ground
    // Uses a random timer to turn the enemy
    void Movement()
    {
        Collider2D isGroundedCheck = Physics2D.OverlapCircle(groundCheck.position, 0.02f, groundLayer);
        if (isGroundedCheck)
        {
            transform.position += transform.right * speed * Time.deltaTime;

            // FRONT WALLS
            Collider2D frontWall = Physics2D.OverlapCircle(wallCheck.position, 0.02f, boxesAndwalls);
            if (frontWall != null && limitWalkingRangeReached == false)
            {
                limitWalkingRangeReached = true;
                tempPosition = transform.position;
            }

            Collider2D goundRangeCheck = Physics2D.OverlapCircle(groundRangeCheck.position, 0.1f, groundLayer);
            // NO FLOOR
            if (goundRangeCheck == null && limitWalkingRangeReached == false)
            {
                limitWalkingRangeReached = true;
                tempPosition = transform.position;
            }

            // MAX RANGE
            if ((transform.position.x > startingPos.x + limitRange || transform.position.x < startingPos.x - limitRange) && limitWalkingRangeReached == false)
            {
                limitWalkingRangeReached = true;
                tempPosition = transform.position;
            }

            // WAITING TIME DELAY // If it reaches the limit distance, starts walking back
            if (limitWalkingRangeReached && meleeAttack == false)
            {
                waitingTimeCounter -= Time.deltaTime;
                transform.position = tempPosition;
            }
            if (waitingTimeCounter < 0)
            {
                transform.Rotate(0f, 180f, 0f);
                waitingTimeCounter = Random.Range(1f, 4f); // WAITING TIME <<<<<<<<<<< TA RANDOM NESTE ;
                limitWalkingRangeReached = false;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (transform.right.x > 0) Gizmos.DrawWireCube(meleePosition.position, new Vector3( 0.4f, 0.7f, 0f));
        if (transform.right.x < 0) Gizmos.DrawWireCube(meleePosition.position, new Vector3( -0.4f, 0.7f, 0f));
        Gizmos.DrawWireSphere(backStab.position, 0.13f);
    }
}





