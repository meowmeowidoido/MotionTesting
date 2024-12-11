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
        if (!playerStunned == true)
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

        //If the player presses LeftShift andthe player inputx is facing to the right or is positive, 
        //Dash to the right
        if (Input.GetKey(KeyCode.LeftShift) && playerInput.x > 0)
        {
            if (dashCooldown <= 0 && dashTime < dashDuration)
            {
                velocity = new Vector2(playerInput.x * dashSpeed, playerInput.y);
                dashTime += Time.deltaTime;

            }
        }

        //Same as the last bit but for the left.
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


        if (dashTime > dashDuration)//if the dashTime is greater than the duration, cooldown begins.
        {

            dashCooldown += Time.deltaTime;
            if (dashCooldown > dashCoolDownLimit)
            //when the dashCooldown is greater than the Limit it will reset the dashTime and coolDown (player can dash again)
            {
                dashTime = 0;
                dashCooldown = 0;
            }
          
        }
        if (isGrounded)//If the player is grounded and the dash cooldown is less than or equal to 0 the dashTime resets to 0 and dashCooldown is decremented by dashTime and deltaTime
                       //Cooldown only begins when player is on the ground.
        {
            if (dashTime <= 0) //If the dash is less than 0 reset to 0 and dashCooldown is decremented by dashTime * deltaTime.
            {
                dashTime = 0;
                dashCooldown -= dashTime * Time.deltaTime;
            }
        }

    }


    private void JumpUpdate()
    {
        //if the player is grounded 
        if (isGrounded)
        {//turn doubleJump to false. So that the player cannot glide with the first JUMP.
            doubleJump = false;
            //if the player jumps and is in the walking or idle state
            if (Input.GetButton("Jump") && (currentState.Equals(PlayerState.walking) || currentState.Equals(PlayerState.idle)))
            {
                //Increase velocity and make IsGrounded false.
                velocity.y = initialJumpSpeed;
                isGrounded = false;
                currentState = PlayerState.jumping;

            }


        }
        //WHEN the player lets go turn doubleJump to TRUE, this will allow the player to double JUMP.
        if (Input.GetButtonUp("Jump"))
        {
            doubleJump = true;
        }

        if (!isGrounded)//If the player is not grounded, 

        {
            //if they jump and && double jump is true, and jump Strength is GREATER than 1, allow player to jump
            if (Input.GetButton("Jump") && doubleJump == true && jumpStrength > 1f)
            {
                firstJump = true;
                //First jump becomes true making it known the first jump has been done
                if (firstJump == true)
                {
                    //decrement jump strenght by itself and deltaTime.
                    jumpStrength -= jumpStrength * Time.deltaTime;
                    //JUMP!
                    velocity.y = initialJumpSpeed;

                    isGrounded = false;

                }



            }




        }
        //If the jumpStrength is lower than 1.2 and the player lets go of the key they cannot jump
        //This is to ensure the player cannot perform multiple jumps by tapping the button.
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
        if (isGrounded == false && Input.GetKey(KeyCode.S))//when the player is not grounded and presses S, begin groundpound
        {
            transform.position += Vector3.down * 50 * Time.deltaTime;
            playerStunned = true;

        }
        if (Input.GetKeyUp(KeyCode.S) && !isGrounded)
        { //if the player lets go of S and are not grounded they are not STUNNED.
            playerStunned = false;
        }
        if (playerStunned == true && isGrounded == true)
        {
            stunTimer += Time.deltaTime; //If the player Hits the ground and is stunned (playerStunned is true they are stunned).
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

