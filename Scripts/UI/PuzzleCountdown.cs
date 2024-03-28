using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PuzzleCountdown : MonoBehaviour
{
    /* Gameobject Reference */
    private TextMeshProUGUI tmp;

    /* LevelManager Reference */
    public LevelManager levelManager;
    private TextMeshPro lavaCountdown;

    /* Color */
    public Color targetColor;

    // Start is called before the first frame update
    void Start(){
        /* Initialization */
        tmp = this.GetComponent<TextMeshProUGUI>();
        if(GameObject.Find("LevelManager")!=null)
            levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        lavaCountdown = levelManager.lava.transform.GetChild(0).gameObject.GetComponent<TextMeshPro>();

    }

    // Update is called once per frame
    void Update(){
        tmp.text = lavaCountdown.text;
        float t = (levelManager.maxTimeLeft-levelManager.timeLeft)/levelManager.maxTimeLeft;
        tmp.color = NicerColorLerp(new Color(1,1,1,1), targetColor, (Mathf.Sin(t*Mathf.PI*0.5f)));
    }

    Color NicerColorLerp(Color A, Color B, float t) {
        return new Color(Mathf.Sqrt(A.r * A.r * (1 - t) + t * B.r * B.r), Mathf.Sqrt(A.g * A.g * (1 - t) + t * B.g * B.g), Mathf.Sqrt(A.b * A.b * (1 - t) + t * B.b * B.b));
    }
}
