using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

// Manages...
// Wallsliding
// Player death
// Horizontal movement
// Jumps (cancel, walljump, coyote frames, buffer jump, double jump)
public class PlayerMovement : MonoBehaviour
{
    /* this references */
    private Rigidbody2D rb;
    private BoxCollider2D box2D;
    private SpriteRenderer sr;

    /* Cinemachine references */
    public CinemachineVirtualCamera vcam;

    /* Child: Double Jump UI */
    public GameObject doubleUI;

    /* Horizontal Speed */
    public float speed;//on land
    public float walljumpHSpeed;//on wall

    private Vector2 iDir;//input axis
    private Vector2 iRawDir;//raw input axis

    /* Jumps Available */
    public int maxJumpTimes;
    private int jumpTimes;

    /* Jump Height */
    public float jumpHeight;//on land
    public float walljumpHeight;//on wall

    /* Counter Jump Speed (release space) */
    public float cancelRate;

    /* Wall Slide Speed */
    public float wallslideNormalSpeed;
    public float wallslideFastSpeed;

    /* Modifications when midair */
    public float airModifier;
    public float airMaxSpeed;

    /* Storing Coyote Frames for mid air jump */
    private float coyoteTime;
    public float maxCoyoteTime;

    /* Storing Jump Buffers */
    private float jumpBufferHoldTime;
    private float jumpReleaseBufferTime;
    public float maxBufferTime;

    /* Jump States */
    private bool jumping;
    private bool jumpCancelled;
    private bool groundJump;

    /* Player States */
    private bool isGrounded;
    private bool isWallslide;
    private bool isLeftWall,isRightWall;
    private bool isAlive;
    private bool isDeathJump;

    /* Jump reset layer */
    private LayerMask groundMask;

    /* Death Rotation */
    public float rotationSpeed;
    public Vector3 targetRotation;

    /* Script wide variable */
    private float jumpForce = 0;

    /* Force from external sources (moving platforms) */
    public Vector2 inheritForce;

    void Start(){
        /* Initialize */
        isAlive = true;

        /* this references */
        rb = this.GetComponent<Rigidbody2D>();
        box2D = this.GetComponent<BoxCollider2D>();
        sr = this.GetComponent<SpriteRenderer>();

        /* References */
        groundMask = LayerMask.GetMask("Default");
    }

    void Update(){

        /* Update Coyote Frames */
        if(isGrounded){
            coyoteTime = maxCoyoteTime;
        }else{
            coyoteTime -= Time.deltaTime;
        }

        /* Update Jump Buffer */
        jumpBufferHoldTime -= Time.deltaTime;
        jumpReleaseBufferTime -= Time.deltaTime;

        /* Left Right Input */
        iDir = new Vector2(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical"));
        iRawDir = new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));

        /* Player dead */
        if(!isAlive){
            box2D.enabled = false;
            rb.velocity = new Vector2(0,rb.velocity.y);

            /* Death Boost (bounces player up) */
            if(!isDeathJump){
                vcam.LookAt = vcam.Follow = null;

                rb.velocity = new Vector2(rb.velocity.x,0);
                isDeathJump = true;
                rb.AddForce(new Vector2(0,15),ForceMode2D.Impulse);
            }


            /* Death Rotation */
            transform.rotation = Quaternion.RotateTowards(transform.rotation,Quaternion.Euler(targetRotation),rotationSpeed * Time.deltaTime);

            return;
        }

        /* JUMPS */
        if(Input.GetButtonDown("Jump") || jumpBufferHoldTime>0){
            // pressed jump or buffered a jump

            /* Ungrounded, buffered a jump */
            if(!isGrounded && Input.GetButtonDown("Jump")){
                jumpBufferHoldTime = maxBufferTime;
            }

            /* Normal Jump (midair or grounded) */
            if(!isWallslide && jumpTimes > 0){

                /* Reset jump buffer */
                jumpBufferHoldTime = 0;

                /* Calculate force needed to reach jumpHeight */
                jumpForce = Mathf.Sqrt(jumpHeight * -2 * (Physics2D.gravity.y * rb.gravityScale));

                /* Apply force */
                rb.velocity = new Vector2(rb.velocity.x,0);//Reset for doublejump
                rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);//ApplyForce

            }else if(isWallslide){/* Wall Jump */

                /* Reset jump buffer */
                jumpBufferHoldTime = 0;
                jumpForce = Mathf.Sqrt(walljumpHeight * -2 * (Physics2D.gravity.y * rb.gravityScale));//Calculate JumpForce

                /* Apply force */
                rb.velocity = new Vector2(rb.velocity.x,0);//Reset for doublejump

                //Apply vertical force and horizontal, as it's wall jump
                rb.AddForce(new Vector2((isLeftWall?1:-1)*walljumpHSpeed, 
                            /*(iRawDir.y<0?-0.25f:1)* */jumpForce), ForceMode2D.Impulse);

                /* Reset jump states (walljump refreshes jump) */
                jumpTimes = maxJumpTimes;
                doubleUI.SetActive(true);
                groundJump = true;
            }


            /* Set jump states */
            jumping = true;
            jumpCancelled = false;

            /* Check if jumped from ground */
            if(isGrounded||coyoteTime>0){//Jump within coyote frames count as groundJump
                groundJump = true;
                coyoteTime = 0;
            }

            /* Update jumps left */
            if(groundJump){
                jumpTimes --;
            }
            else{
                jumpTimes -=2;
                groundJump = true;
            }

            /* Double Jump UI */
            if(jumpTimes==0){
                doubleUI.SetActive(false);
            }

        }

        /* Cancel Jump */
        if(jumping){
            if(Input.GetButtonUp("Jump")){
                jumpReleaseBufferTime = maxBufferTime;
                jumpCancelled = true;
            }
        }

    }

    //Physics actions
    void FixedUpdate(){
        checkGround();
        checkWallslide();

        /* Player Dead */
        if(!isAlive){
            return;
        }

        moveCharacter();
        jumpCharacter();

        /* Reset jump and player states */
        if(isGrounded){
            jumpTimes = maxJumpTimes;
            doubleUI.SetActive(true);
            jumping = false;
            groundJump = false;
        }

        /* Tweak vertical velocity if wallslide */
        if(isWallslide){
            if(iRawDir.y>=0){//Normal wallslide
                rb.velocity = new Vector2(rb.velocity.x,Mathf.Clamp(rb.velocity.y,-wallslideNormalSpeed,float.MaxValue));
            }else{//Fast wall slide
                rb.velocity = new Vector2(rb.velocity.x,Mathf.Clamp(rb.velocity.y,-wallslideFastSpeed,float.MaxValue));
            }
        }

        rb.velocity += inheritForce;
        inheritForce = Vector2.zero;

    }

    //Move Player with Rigidbody
    void moveCharacter(){
        /* Ground Control */
        if(isGrounded){
            rb.velocity = new Vector2(iDir.x * speed * Time.fixedDeltaTime ,rb.velocity.y);
        }
        else{/* Air Control */

            /* Supposed horizontal change */
            float horizontalChange = (iDir.x * speed * Time.fixedDeltaTime )-rb.velocity.x;
            /* Clamp horizontal change for less sensitive controls in air */
            horizontalChange = Mathf.Clamp(horizontalChange,-airMaxSpeed,airMaxSpeed);

            /* Apply clamped horizontal change */
            /* If it will decrease current x velocity, only apply if user meant to through holding opposite movement keys */
            /* This enables long jump off walls, but also wallclimb */
            if((horizontalChange<0&&iRawDir.x<=0)||(horizontalChange>0&&iRawDir.x>=0)){
                rb.velocity = new Vector2(rb.velocity.x + horizontalChange ,rb.velocity.y);
            }
        }
        /* rb.AddForce(dir * speed, ForceMode2D.Impulse); */
        /* rb.velocity = dir * speed * Time.fixedDeltaTime; */

        /* rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxSpeed); */
    }

    /* Apply Cancel Jump velocity */
    void jumpCharacter(){
        if((jumpCancelled || jumpReleaseBufferTime>0) && jumping && rb.velocity.y >0){
            /* Jump is cancelled, or a jump cancel is buffered */
            /* Still jumping */
            /* Still going up */

            /* Apply cancel force */
            rb.AddForce(Vector2.down * cancelRate);
            /* Reset jump cancel buffer */
            jumpReleaseBufferTime = 0;
        }
        /* rb.velocity = new Vector2(rb.velocity.x,jumpSpeed); */
    }

    /* Check if player is grounded through boxcast */
    void checkGround(){
        RaycastHit2D hit = Physics2D.BoxCast(box2D.bounds.center, box2D.bounds.size, 0f, -Vector2.up, 0.1f,groundMask);
        if(hit.collider != null && rb.velocity.y ==0 ){
            isGrounded = true;
        }
        else{
            isGrounded = false;
        }

        /* if(col.gameObject.CompareTag("Ground")){ */
        /*   jumpTimes = maxJumpTimes; */
        /*   isGrounded = true; */
        /* } */
    }

    /* Check if player is wallsliding through boxcast */
    void checkWallslide(){
        RaycastHit2D rHit = Physics2D.BoxCast(box2D.bounds.center, box2D.bounds.size, 0f, Vector2.right, 0.15f,groundMask);
        RaycastHit2D lHit = Physics2D.BoxCast(box2D.bounds.center, box2D.bounds.size, 0f, -Vector2.right, 0.15f,groundMask);

        bool contact = (rHit.collider!=null&&rHit.collider.gameObject.CompareTag("Ground"))||(lHit.collider!=null&&lHit.collider.gameObject.CompareTag("Ground"));

        if(rb.velocity.y<0&&contact&&(iRawDir.x!=0||isWallslide)){
            isWallslide = true;
            if(rHit.collider!=null){
                isRightWall = true;
            }else{
                isLeftWall = true;
            }
            sr.color = new Color(0.53f,0.21f,0.26f,1f);
        }
        else{
            isWallslide = false;
            isRightWall = isLeftWall = false;
            sr.color = new Color(1f,0.38f,0.71f,1f);
        }
    }

    /* Check if player is dead by colliding with lava */
    void OnTriggerEnter2D(Collider2D col){
        if(col.gameObject.CompareTag("Kill")){
            isAlive = false;
        }
    }

}
