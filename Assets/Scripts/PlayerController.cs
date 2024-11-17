using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Rigidbody2D rb;
    public float horizontal;
    public float speed = 5;
    public float jumpPower = 15f;


    public enum FacingDirection
    {
        left, right
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // The input from the player needs to be determined and
        // then passed in the to the MovementUpdate which should
        // manage the actual movement of the character.



        float horizontal = Input.GetAxis("Horizontal");
        Vector2 playerInput = new Vector2(0, 0);
        playerInput.x = horizontal;

        

        MovementUpdate(playerInput);
        
        if(!IsGrounded() && Input.GetKeyDown(KeyCode.Space))
        {
            jump();
        }
   
    }

    private void MovementUpdate(Vector2 playerInput)
    {
        rb.velocity = new Vector2(playerInput.x * speed, rb.velocity.y);

    }

    public bool IsWalking()
    {
        bool returning = false;
        if (rb.velocity.x < 0 || rb.velocity.x > 0)
        {

            returning = true;
            print(returning);
            return returning;
        }
        else
        {
            print(returning);
            return returning;
        }
        
    }
    public bool IsGrounded()
    {
        RaycastHit2D hit;
        hit = Physics2D.Raycast(transform.position, Vector2.down, 0.8f);
        Debug.DrawRay(transform.position, Vector2.down * 0.8f, Color.red);

        if(hit.collider==null)
        {
            return true;
        }
        return false;
    }
    public void jump()
    { 

        rb.AddForce(Vector2.up * (jumpPower),ForceMode2D.Impulse);
        
    }
    public FacingDirection GetFacingDirection()
    {
        if (rb.velocity.x <0)
        {
            return FacingDirection.left;
        }
        if (rb.velocity.x > 0)
        {
            return FacingDirection.right;
        }
        return FacingDirection.left; 
    }
}
