using UnityEngine;
using UnityEngine.Assertions.Must;

public enum PlayerDirection
{
    left, right
}

public enum PlayerState
{
    idle, walking, jumping, dead
}

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D body;
    private PlayerDirection currentDirection = PlayerDirection.right;
    public PlayerState currentState = PlayerState.idle;
    public PlayerState previousState = PlayerState.idle;
    [Header("Ground Pound")]
    public bool playerStunned = false;
    public float stunTimer = 0;
    public float stunDuration = 1;

    [Header("Horizontal")]
    public float maxSpeed = 5f;
    public float accelerationTime = 0.25f;
    public float decelerationTime = 0.15f;

    [Header("Dashing")]
    public float dashSpeed = 15f;
    float dashCooldown = 0f;
    public float dashCoolDownLimit = 1f;
    public float dashTime = 0.5f;
    public float dashDuration = 0.75f;

    [Header("Vertical")]
    public float apexHeight = 3f;
    public float apexTime = 0.5f;
    public bool doubleJump = true;
    public float jumpStrength = 1.2f;
    float playerJumpStrength;
    bool firstJump = false;
    [Header("Ground Checking")]
    public float groundCheckOffset = 0.5f;
    public Vector2 groundCheckSize = new(0.4f, 0.1f);
    public LayerMask groundCheckMask;



    private float accelerationRate;
    private float decelerationRate;

    private float gravity;
    private float initialJumpSpeed;

    private bool isGrounded = false;
    public bool isDead = false;

    private Vector2 velocity;

    public void Start()
    {
        body.gravityScale = 0;

        accelerationRate = maxSpeed / accelerationTime;
        decelerationRate = maxSpeed / decelerationTime;

        gravity = -2 * apexHeight / (apexTime * apexTime);
        initialJumpSpeed = 2 * apexHeight / apexTime;


    }

    public void Update()
    {
        previousState = currentState;

        CheckForGround();
        
        Vector2 playerInput = new Vector2();
        playerInput.x = Input.GetAxisRaw("Horizontal");
        GroundPound();
        if (!playerStunned==true)
        {
           
            MovementUpdate(playerInput);
            DashUpdate(playerInput);
            JumpUpdate();
        }
            if (isDead)
        {
            currentState = PlayerState.dead;
        }

        switch (currentState)
        {
            case PlayerState.dead:
                // do nothing - we deqd.
                break;
            case PlayerState.idle:
                if (!isGrounded) currentState = PlayerState.jumping;
                else if (velocity.x != 0) currentState = PlayerState.walking;
                break;
            case PlayerState.walking:
                if (!isGrounded) currentState = PlayerState.jumping;
                else if (velocity.x == 0) currentState = PlayerState.idle;
                break;
            case PlayerState.jumping:
                if (isGrounded)
                {
                    if (velocity.x != 0) currentState = PlayerState.walking;
                    else currentState = PlayerState.idle;
                }
                break;
        }

      

        if (!isGrounded)
            velocity.y += gravity * Time.deltaTime;
        else
            velocity.y = 0;

        body.velocity = velocity;
    }

    private void MovementUpdate(Vector2 playerInput)
    {
        if (playerInput.x < 0)
            currentDirection = PlayerDirection.left;
        else if (playerInput.x > 0)
            currentDirection = PlayerDirection.right;

        if (playerInput.x != 0)
        {
            velocity.x += accelerationRate * playerInput.x * Time.deltaTime;
            velocity.x = Mathf.Clamp(velocity.x, -maxSpeed, maxSpeed);
        }
        else
        {
            if (velocity.x > 0)
            {
                velocity.x -= decelerationRate * Time.deltaTime;
                velocity.x = Mathf.Max(velocity.x, 0);
            }
            else if (velocity.x < 0)
            {
                velocity.x += decelerationRate * Time.deltaTime;
                velocity.x = Mathf.Min(velocity.x, 0);
            }
        }


    }
    private void DashUpdate(Vector2 playerInput)
    {
        print(dashCooldown);


        if (Input.GetKey(KeyCode.LeftShift) && playerInput.x > 0)
        {
            if (dashCooldown <= 0 && dashTime < dashDuration)
            {
                velocity = new Vector2(playerInput.x * dashSpeed, playerInput.y);
                dashTime += Time.deltaTime;

            }
        }
        if (Input.GetKey(KeyCode.LeftShift) && playerInput.x < 0)
        {
            if (dashCooldown <= 0)
            {
                if (dashCooldown == 0 && dashTime < dashDuration)
                {
                    velocity = new Vector2((playerInput.x * dashSpeed), playerInput.y);
                    dashTime += Time.deltaTime;
                }

            }

        }


        if (dashTime > dashDuration)
        {

            dashCooldown += Time.deltaTime;
            if (dashCooldown > dashCoolDownLimit)
            {
                dashTime = 0;
                dashCooldown = 0;
            }
            if (isGrounded)
            {
                if (dashCooldown <= 0)
                {
                    dashTime = 0;
                    dashCooldown -= dashTime * Time.deltaTime;
                }
            }
        }

    }


    private void JumpUpdate()
    {

        if (isGrounded)
        {
            doubleJump = false;
            if (Input.GetButton("Jump") && (currentState.Equals(PlayerState.walking) || currentState.Equals(PlayerState.idle)))
            {
                velocity.y = initialJumpSpeed;
                isGrounded = false;
                currentState = PlayerState.jumping;

            }


        }
        if (Input.GetButtonUp("Jump"))
        {
            doubleJump = true;
        }

        if (!isGrounded)

        {

            if (Input.GetButton("Jump") && doubleJump == true && jumpStrength > 1f)
            {
                firstJump = true;

                if (firstJump == true)
                {

                    jumpStrength -= jumpStrength * Time.deltaTime;
                    velocity.y = initialJumpSpeed;

                    isGrounded = false;

                }



            }




        }
        if (jumpStrength < 1.2 && Input.GetButtonUp("Jump"))
        {
            doubleJump = false;
        }


        if (isGrounded)
        {

            jumpStrength = 1.2f;


        }



    }

    private void CheckForGround()
    {
        if (isGrounded == true)
        {
            doubleJump = true;
        }
        isGrounded = Physics2D.OverlapBox(
            transform.position + Vector3.down * groundCheckOffset,
            groundCheckSize,
            0,
            groundCheckMask);
    }

    private void GroundPound()
    {
        if (isGrounded == false && Input.GetKey(KeyCode.S))
        {
            transform.position += Vector3.down * 50 * Time.deltaTime;
            playerStunned = true;
   
        }
        if(Input.GetKeyUp(KeyCode.S)&& !isGrounded ) {
            playerStunned = false;
        }
        if (playerStunned==true && isGrounded==true)
        {
            stunTimer += Time.deltaTime;
            transform.position += Vector3.zero;
            if (stunTimer > stunDuration)
            {
                playerStunned = false;
                stunTimer = 0;
            }
        }
      
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position + Vector3.down * groundCheckOffset, groundCheckSize);
    }

    public bool IsWalking()
    {
        return velocity.x != 0;
    }
    public bool IsGrounded()
    {
        return isGrounded;
    }

    public PlayerDirection GetFacingDirection()
    {
        return currentDirection;
    }

}
