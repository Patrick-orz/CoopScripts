using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Move platform from original to targetPos and back*/
public class FloorMovement : MonoBehaviour
{
    /* References */
    private Rigidbody2D rb;
    private PlayerMovement pm;

    /* Speed of movement */
    public float speed;

    public float distanceStop;

    /* Where to go */
    public Vector2 targetPos;
    public Vector2 originalPos;

    /* Time until next movement */
    public float waitTime;

    /* isPlatform moving */
    private bool lerping;
    /* Is platform going to move towards target */
    private bool go;
    private bool isPlayer;

    void Start(){
        /* Get references */
        rb = gameObject.GetComponent<Rigidbody2D>();
        pm = GameObject.Find("Player").GetComponent<PlayerMovement>();

        /* Initialize */
        go = true;
        lerping = true;
        /* hotfix */
        targetPos.y = originalPos.y = transform.position.y;
    }

    void FixedUpdate(){
        Debug.Log(go);
        if(lerping){

            if(Vector2.Distance((go?targetPos:originalPos),transform.position)<=distanceStop){
                lerping = false;
                rb.velocity = Vector2.zero;
                StartCoroutine(switchSide());
            }else{
                Vector2 b = (go?targetPos:originalPos);
                Vector2 tmp = transform.position;

                Vector2 direction = b-tmp;
                rb.velocity = direction.normalized * speed ;

                if(isPlayer){
                    pm.inheritForce += rb.velocity;
                }
            }

        }
    }

    IEnumerator switchSide(){
        yield return new WaitForSeconds(waitTime);
        lerping = true;
        go = !go;
    }

    void OnCollisionEnter2D(Collision2D col){
        if(col.collider.gameObject.CompareTag("Player")){
            isPlayer = true;
        }
    }

    void OnCollisionExit2D(Collision2D col){
        if(col.collider.gameObject.CompareTag("Player")){
            isPlayer = false;
        }
    }
}
