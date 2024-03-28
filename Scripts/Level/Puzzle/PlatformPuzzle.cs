using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformPuzzle : MonoBehaviour
{
    /* Structs */
    public struct TARGETDATA{// Info for platform that should be stepped
        public int index;
        public Color c;

        /* Constructor */
        public TARGETDATA(int index,Color c){
            this.index = index;
            this.c = c;
        }
    }

    /* Reference */
    private LevelManager levelManager;
    private LayerMask playerMask;

    public GameObject gate;
    private SpriteRenderer gateRenderer;
    private BoxCollider2D gateBox2D;

    /* Puzzle info */
    public GameObject allPlatforms;

    public List<GameObject> platforms;
    private List<SpriteRenderer> platformsRenderer = new List<SpriteRenderer>();
    private List<BoxCollider2D> platformsBox2D = new List<BoxCollider2D>();

    public GameObject confirmationPlatform;
    private SpriteRenderer confirmationPlatformRenderer;
    private BoxCollider2D confirmationPlatformBox2D;

    public List<Color> colors;

    /* States & Count */
    public int maxCycles;
    private int cycles=0;

    private bool generated;
    private bool confirmed;
    private bool pressed;

    private bool confirmState;

    /* Generated */
    private List<Color> platformsData = new List<Color>();

    /* Check */
    private TARGETDATA targetData;
    private TARGETDATA pressedData;

    /* Answers */
    private List<string> rawAnswers;
    private TARGETDATA[,,] answers = new TARGETDATA[5,5,5];

    // Start is called before the first frame update
    void Start(){
        /* Get references */
        if(GameObject.Find("LevelManager")!=null)
            levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        for(int i=0;i<platforms.Count;i++){
            platformsRenderer.Add( platforms[i].GetComponent<SpriteRenderer>() );
            platformsBox2D.Add( platforms[i].GetComponent<BoxCollider2D>() );

            platformsData.Add( Color.white );
        }
        confirmationPlatformRenderer = confirmationPlatform.GetComponent<SpriteRenderer>();
        confirmationPlatformBox2D = confirmationPlatform.GetComponent<BoxCollider2D>();

        gateRenderer = gate.GetComponent<SpriteRenderer>();
        gateBox2D = gate.GetComponent<BoxCollider2D>();

        /* Initialize answer */
        rawAns();
        sortAns();
    }

    // Update is called once per frame
    void Update(){
        /* Puzzle still running */
        if(cycles < maxCycles){
            /* Gen new cycle */
            if(!generated){
                customizePlatforms();
                findTarget();
                Debug.Log(targetData.index);
                Debug.Log(targetData.c);
                generated = true;

                confirmState = false;
            }
        }else{
            /* Puzzle finished */
            reset();
            StartCoroutine(lerpOut(1,gate,gateRenderer));
        }
    }

    void FixedUpdate(){
        checkPlatforms();
        if(pressed){
            confirmState = true;

            confirmationPlatformRenderer.color = Color.gray;
        }
        if(confirmed){
            confirmState = false;

            nextCycle();
        }
    }

    void customizePlatforms(){
        for(int i=0;i<3;i++){
            /* Generate c */
            Color tempC = colors[Random.Range(0,colors.Count)];

            /* Assign */
            platformsData[i]=tempC;
            platformsRenderer[i].color = toneDown(tempC);
        }
        confirmationPlatformRenderer.color = Color.white;
    }

    void findTarget(){
        /* targetData = new TARGETDATA(0,Color.red); */
        List<int> sortData = new List<int>{toInt(platformsData[0]),toInt(platformsData[1]),toInt(platformsData[2])};
        sortData.Sort();
        /* Debug.Log(platformsData[0]); */
        /* Debug.Log(platformsData[1]); */
        /* Debug.Log(platformsData[2]); */

        /* Debug.Log(sortData[0]); */
        /* Debug.Log(sortData[1]); */
        /* Debug.Log(sortData[2]); */
        targetData = answers[sortData[0],sortData[1],sortData[2]];
    }

    void checkPlatforms(){
        /* Check if any platforms are stepped */
        pressed = false;
        RaycastHit2D hit;
        for(int i=0;i<platforms.Count;i++){

            /* Boxcast */
            playerMask = LayerMask.GetMask("Player");
            hit = Physics2D.BoxCast(platformsBox2D[i].bounds.center, platformsBox2D[i].bounds.size, 0f, Vector2.up, 0.1f,playerMask);

            /* Platform Step */
            if(hit.collider != null){
                pressed = true;

                /* c */
                for(int o=0;o<platforms.Count;o++){
                    platformsRenderer[o].color = toneDown(platformsData[o]);
                }
                platformsRenderer[i].color = platformsData[i];

                /* Store */
                pressedData = new TARGETDATA(i,platformsData[i]);
            }

        }

        /* Check if player confirmed */
        confirmed = false;
        playerMask = LayerMask.GetMask("Player");
        hit = Physics2D.BoxCast(confirmationPlatformBox2D.bounds.center, confirmationPlatformBox2D.bounds.size, 0f, Vector2.up, 0.1f,playerMask);
        if(hit.collider != null){
            if(confirmState){
                confirmed = true;
            }
        }
    }

    void nextCycle(){
        /* Check platform press */
        if(pressedData.index == targetData.index || pressedData.c == targetData.c){
            generated = false;
            cycles++;
        }else{
            levelManager.killing = true;
            kill();
        }
    }

    void kill(){
        StartCoroutine(lerpOut(1,allPlatforms));
    }

    Color toneDown(Color c){
        return new Color(c.r,c.g,c.b,0.5f);
    }

    void reset(){
        for(int i=0;i<platforms.Count;i++){
            platformsRenderer[i].color = Color.white;
        }
        confirmationPlatformRenderer.color = Color.white;
    }

    IEnumerator lerpOut(float maxLerpTime,GameObject go,SpriteRenderer sr = null,BoxCollider2D bc2d = null){
        if(bc2d!=null)
            bc2d.enabled = false;
        foreach (Transform child in go.transform){
            child.GetComponent<BoxCollider2D>().enabled = false;
        }

        float t = 0;
        Color startL = (sr!=null?sr.color:Color.white);
        Color endL = new Color(startL.r,startL.g,startL.b,0);

        while(t<maxLerpTime){
            float lt = t/maxLerpTime;

            if(sr!=null)
                sr.color = Color.Lerp(startL,endL,lt);

            foreach (Transform child in go.transform){
                child.gameObject.GetComponent<SpriteRenderer>().color = Color.Lerp(startL,endL,lt);
            }

            t += Time.deltaTime;
            yield return null;
        }
        if(sr!=null)
            sr.color = endL;
        foreach (Transform child in go.transform){
            child.gameObject.GetComponent<SpriteRenderer>().color = endL;
        }

        go.SetActive(false);
    }

    void rawAns(){
        //c = color, p = pos
        rawAnswers = new List<string>{
            "p0left",
            "c1green",
            "p1center",
            "c3yellow",
            "p2right",
            "c0red",
            "p2right",
            "c3yellow",
            "p2right",
            "c0red",
            "c0red",
            "c4cyan",
            "p1center",
            "c4cyan",
            "p2center ",
            "p2right",
            "p1center",
            "c3yellow",
            "p0left",
            "c1green",
            "c3yellow",
            "c2blue",
            "c1green",
            "p2right",
            "c1green",
            "p1center",
            "p1center",
            "p2right ",
            "p2left ",
            "c4cyan",
            "c2blue",
            "p1center",
            "c4cyan",
            "p2left ",
            "p0left"
        };
    }

    void sortAns(){
        int cnt = 0;
        for(int i=0;i<5;i++){
            for(int o=i;o<5;o++){
                for(int p=o;p<5;p++){

                    /* Debug.Log(rawAnswers[cnt][1]-'0'); */
                    if(rawAnswers[cnt][0]=='p'){
                        answers[i,o,p] = new TARGETDATA(rawAnswers[cnt][1]-'0',Color.black);
                    }else{
                        answers[i,o,p] = new TARGETDATA(-1,colors[rawAnswers[cnt][1]-'0']);
                    }

                    /* Debug.Log(answers[i,o,p].index); */
                    /* Debug.Log(answers[i,o,p].c); */

                    cnt++;
                }
            }
        }
    }

    int toInt(Color a){
        /* Manual sequence */
        if(a == Color.red)
            return 0;
        if(a == Color.green)
            return 1;
        if(a == Color.blue)
            return 2;
        if(a == new Color(1,1,0,1))
            return 3;
        return 4;
    }
}
