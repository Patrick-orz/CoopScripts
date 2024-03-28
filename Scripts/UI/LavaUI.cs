using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* Controls UI on top of screen hinting on lava's distance from player */
public class LavaUI : MonoBehaviour
{
    /* References */
    public GameObject player;
    public GameObject lava;
    private LavaMovement lavaMovement;
    private RawImage ri;

    void Start(){
        /* Get references */
        ri = gameObject.GetComponent<RawImage>();
        lavaMovement = lava.GetComponent<LavaMovement>();
    }

    void Update(){
        /* Distance of player from lava */
        float dist = player.transform.position.y - (lava.transform.position.y + lavaMovement.offset/2);

        /* Use color lerp to have red show close green show far */
        ri.color = NicerColorLerp(new Color(1,0,0,1),new Color(0,1,0,1),Mathf.Clamp(dist/75f,0,1));
    }

    Color NicerColorLerp(Color A, Color B, float t) {
        /* Lerp for color */
        return new Color(Mathf.Sqrt(A.r * A.r * (1 - t) + t * B.r * B.r), Mathf.Sqrt(A.g * A.g * (1 - t) + t * B.g * B.g), Mathf.Sqrt(A.b * A.b * (1 - t) + t * B.b * B.b));
    }
    // Taken from 
    // https://forum.unity.com/threads/a-nicer-color-lerp-function.772595/
}
