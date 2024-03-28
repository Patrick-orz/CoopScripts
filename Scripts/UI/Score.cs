using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Score : MonoBehaviour
{
    /* Gameobject Reference */
    private TextMeshProUGUI tmp;

    /* Score */
    public float score;
    public float add;

    /* Pop animation */
    public float originalSize;
    public float bigSize;
    public float popDuration;

    /* Value animation */
    public float valueDuration;

    // Start is called before the first frame update
    void Start(){
        tmp = this.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update(){
        if(add!=0){
            StartCoroutine(sizeLerp());
            StartCoroutine(valueLerp());
            add = 0;
        }
    }

    IEnumerator sizeLerp(){
        /* Out */
        float startL = originalSize;
        float endL = bigSize;

        float t= 0;
        while(t<popDuration){
            float lt = t/popDuration;
            tmp.fontSize = Mathf.Lerp(startL,endL,lt);
            t += Time.deltaTime;
            yield return null;
        }
        tmp.fontSize = endL;

        yield return new WaitForSeconds(0.1f);
        /* In */
        startL = bigSize;
        endL = originalSize;

        t= 0;
        while(t<popDuration){
            float lt = t/popDuration;
            tmp.fontSize = Mathf.Lerp(startL,endL,lt);
            t += Time.deltaTime;
            yield return null;
        }
        tmp.fontSize = endL;
    }

    IEnumerator valueLerp(){
        float startL = score;
        float endL = score + add;

        float t = 0;
        while(t<valueDuration){
            float lt = t/valueDuration;
            tmp.text = Mathf.Floor(Mathf.Lerp(startL,endL,lt)).ToString();
            t+=Time.deltaTime;
            yield return null;
        }
        tmp.text = endL.ToString();
        score = endL;
    }
}
