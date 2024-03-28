using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Lava's behaviour when player is alive and when player is dead */
public class LavaMovement : MonoBehaviour
{
    /* this references */
    private Rigidbody2D rb;

    /* Distance from pivot to side */
    public float offset;

    /* Speed of movement */
    public float speed;
    public float maxSpeed;

    /* Time to pause */
    public float pauseTime;

    /* Player dead conditions */
    public bool DONE;
    private Vector3 endPos;

    /* Is stopping */
    public bool isStop;
    public bool isPermStop;

    void Start(){
        /* Initialize */
        isStop = true;

        /* Get references */
        rb = this.GetComponent<Rigidbody2D>();
        /* StartCoroutine(waiter()); */
    }

    void Update(){

        /* Controls for other scripts */

        /* Stop & Restart */
        if(isStop){
            rb.velocity = new Vector2(0,0);
            isStop = false;
            StartCoroutine(waiter());
        }

        /* Stop until isStop called */
        if(isPermStop){
            rb.velocity = new Vector2(0,0);
            isPermStop = false;
        }

    }

    void LateUpdate(){
        /* Lock all movement after player dead */
        if(DONE){
            rb.velocity = new Vector2(0,0);
            transform.position = endPos;
        }
    }

    IEnumerator waiter(){
        /* Wait and start moving */
        yield return new WaitForSeconds(pauseTime);
        rb.velocity = new Vector2(0,speed);
    }

    void OnTriggerEnter2D(Collider2D col){
        /* Killed player */
        if(col.gameObject.CompareTag("Player")){
            DONE = true;
            endPos = gameObject.transform.position;
        }
    }
}
