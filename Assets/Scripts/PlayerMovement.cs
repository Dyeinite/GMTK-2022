using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody2D rb;
    CapsuleCollider2D cc;
    PlayerController playerController;

    [Header("Movement")]
    [SerializeField] private float movementSpeed;
    [SerializeField] private float jumpForce;

    [Header("Ground Check")]
    [SerializeField] LayerMask whatIsGround;
    [SerializeField] Transform feetPos;
    [SerializeField] float checkRadius;

    [Header("Jumping")]
    [SerializeField] private float fallingGravity = 4f;
    [SerializeField] private float groundedGravity = 2f;
    [SerializeField] private int totalJumps;
    [SerializeField] private float jumpBufferTime = 0.2f;

    [Header("Slopes")]
    [SerializeField] private PhysicsMaterial2D noFriction;
    [SerializeField] private PhysicsMaterial2D fullFriction;
    [SerializeField] private float slopeCheckDistance;
    [SerializeField] private float maxSlopeAngle;

   


    private bool onSlope;
    private bool isFacingRight = true;
    private bool canWalkOnSlope;
    private bool isGrounded;
    private bool isAirborn;
    

    private Vector2 colliderSize;
    private Vector2 moveInput;
    private Vector2 moveDirection;
    private Vector2 slopeNormalPerp;

    private float slopeDownAngleOld;
    private float slopeDownAngle;
    private float slopeSideAngle;
    private float jumpBufferCounter;

    private int extraJumps;

    public Vector2 MoveInput { set => moveInput = value; }

    private void Awake()
    {
        playerController = new PlayerController();
        rb = GetComponent<Rigidbody2D>();
        cc = GetComponent<CapsuleCollider2D>();
        colliderSize = cc.size;
    }

    private void OnEnable()
    {
        playerController.Land.Move.performed += i => moveInput = i.ReadValue<Vector2>();
        playerController.Enable();
    }
    private void OnDisable()
    {
        playerController.Disable();
    }


    private void Start()
    {
        extraJumps = totalJumps;
    }



    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(feetPos.position, checkRadius, whatIsGround);

        if (!isGrounded)
        {
            if (!isAirborn) isAirborn = !isAirborn;
        }
        else
        {
            if (isAirborn)
            {
                extraJumps = totalJumps;
                
                isAirborn = false;
            }
        }
        //if (moveInput.x < 0 && isFacingRight) Flip();
        //else if (moveInput.x > 0 && !isFacingRight) Flip();
        if (playerController.Land.Jump.triggered)
        {
            jumpBufferCounter = jumpBufferTime;
            Jump();
            
        }
        else jumpBufferCounter -= Time.deltaTime;

    }


    private void FixedUpdate()
    {
        SlopeCheck();
        ApplyMovement();
        HandleGravity();
    }

    private void HandleGravity()
    {
        if (isAirborn && rb.velocity.y < 0)
        {
            rb.gravityScale = fallingGravity;
        }
        else
        {
            rb.gravityScale = groundedGravity;
        }
    }

    private void ApplyMovement()
    {

        if (isGrounded && onSlope && canWalkOnSlope)
        {

            rb.drag = 2.25f;
            moveDirection = -slopeNormalPerp * moveInput.x;
        }
        else
        {
            rb.drag = 5f;
            moveDirection = transform.right * moveInput.x;
        }
        rb.AddForce(moveDirection * movementSpeed, ForceMode2D.Force);
    }
    public void Jump()
    {
        if (slopeDownAngle <= maxSlopeAngle && extraJumps > 0 && jumpBufferCounter > 0f)
        {
            extraJumps--;
        
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
            jumpBufferCounter = 0f;
        }


    }



    private void SlopeCheck()
    {
        Vector2 checkPos = transform.position - new Vector3(0f, colliderSize.y / 2);
        SlopeCheckVertical(checkPos);
        SlopeCheckHorizontal(checkPos);

    }

    private void SlopeCheckHorizontal(Vector2 checkPos)
    {
        RaycastHit2D slopeHitFront = Physics2D.Raycast(checkPos, transform.right, slopeCheckDistance, whatIsGround);
        RaycastHit2D slopeHitBack = Physics2D.Raycast(checkPos, -transform.right, slopeCheckDistance, whatIsGround);

        if (slopeHitFront)
        {
            onSlope = true;
            slopeSideAngle = Vector2.Angle(slopeHitFront.normal, Vector2.up);

        }
        else if (slopeHitBack)
        {
            onSlope = true;
            slopeSideAngle = Vector2.Angle(slopeHitBack.normal, Vector2.up);
        }
        else
        {

            slopeSideAngle = 0.0f;
            onSlope = false;
        }
    }

    private void SlopeCheckVertical(Vector2 checkPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, slopeCheckDistance, whatIsGround);
        if (hit)
        {

            slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;

            slopeDownAngle = Vector2.Angle(hit.normal, Vector2.up);

            if (slopeDownAngle != slopeDownAngleOld)
            {

                onSlope = true;
            }

            slopeDownAngleOld = slopeDownAngle;
        }

        if (slopeDownAngle > maxSlopeAngle || slopeSideAngle > maxSlopeAngle)
        {
            canWalkOnSlope = false;
        }
        else
        {
            canWalkOnSlope = true;
        }

        if (onSlope && moveInput.x == 0 && canWalkOnSlope)
        {
            rb.sharedMaterial = fullFriction;
        }
        else
        {
            rb.sharedMaterial = noFriction;
        }
    }

    private void Flip()
    {
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
        isFacingRight = !isFacingRight;
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(feetPos.position, checkRadius);

    }




}
