using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Manages...
// Level generation
// Puzzle generation
// Puzzle countdown
// Syncing lava with levels through lerp

// The mastermind of the game
public class LevelManager : MonoBehaviour
{
    //References
        /* level objects */
    public List<GameObject> levels;
    private List<LevelData> levelsData = new List<LevelData>();
    public List<GameObject> puzzles;
    public List<LevelData> puzzlesData;

        /* player objects */
    public GameObject player;

        /* lava objects */
    public GameObject lava;
    private LavaMovement lavaMovement;
    private TextMeshPro lavaCountdown;

        /* UI */
    public Score score;
    private bool scored;

    /* Level Spawn Position */
    private Vector2 spawn=new Vector2(0,0);
    private Vector2 doSpawn;

    /* States */
    private int curSide = 1;//which side does current level exit on
    private bool isLevel;//is the next generated level a level or puzzle

    /* Syncing lava */
    public float lavaRiseDuration;//how long lava lerp takes
    private Vector3 startL,endL;//Used for lava lerp
    private bool lerping;//Whether lava lerping

    /* Puzzle countdown */
    private bool timing;
    public float timeLeft;
    public float maxTimeLeft;

    /* Puzzle kill (Countdown ended, puzzle failed, player passed) */
    public bool killing;
    private bool killLerped;
    private Vector3 killPos;

    /* References to Spawned Levels */
    private GameObject ld,ud;//ld lower level, upper level
    private LevelData lds,uds;

    void Start(){
        /* Initialize */
        isLevel = true;

        /* References */
        for(int i=0;i<levels.Count;i++){
            levelsData.Add( levels[i].GetComponent<LevelData>() );
        }
        for(int i=0;i<puzzles.Count;i++){
            puzzlesData.Add( puzzles[i].GetComponent<LevelData>() );
        }
        lavaMovement = lava.GetComponent<LavaMovement>();

        /* Get Lava Countdown Text Object */
        lavaCountdown = lava.transform.GetChild(0).gameObject.GetComponent<TextMeshPro>();

        /* Initial Spawn */
        /* Manually spawned due to its uniqueness */
        ud = Instantiate(levels[0],spawn,Quaternion.identity);
        ud.SetActive(true);
        uds = levelsData[0];
        curSide = 1;

        /* Spawn level above initial */
        spawnLevel(curSide);
    }

    void Update(){
        /* Spawn next level when past spawning point */
        if(player.transform.position.y > doSpawn.y){

            if(!scored){
                scored = true;
                /* Add to score */
                if(isLevel){//Passed parkour
                    score.add += Mathf.Floor((100f+(player.transform.position.y - (lava.transform.position.y + lavaMovement.offset/2))/2)*lds.multiplier);
                }else{//Passed puzzle
                    score.add += Mathf.Floor((200f+(timeLeft))*lds.multiplier);
                }
            }

            //Going to spawn puzzle
            if(!isLevel){

                if(lava.transform.position.y < doSpawn.y-uds.offset/2){//Lava needs syncing
                    if(!lerping){

                        //Initialize for lerp
                        lerping = true;
                        startL = lava.transform.position;
                        endL = new Vector3(doSpawn.x,doSpawn.y - uds.offset/2, -5);

                        //Stop lavamovement script
                        lavaMovement.isPermStop = true;

                        StartCoroutine(lavaLerp());

                    }
                }
                if(!lerping){//Finished syncing

                    //Increase speed
                    lavaMovement.speed = Mathf.Clamp(lavaMovement.speed+1,1,lavaMovement.maxSpeed);
                    //Decrease pause time
                    lavaMovement.pauseTime = Mathf.Clamp(lavaMovement.pauseTime-1,0,lavaMovement.pauseTime);
                    //Restart lava
                    lavaMovement.isStop = true;
                }

            }

            /* Despawn Lower Level and Spawn New Higher Level */
            if(!lerping){
                Debug.Log("Spawned");
                spawnLevel(curSide);
                scored = false;
            }


        }else if(player.transform.position.y > doSpawn.y - uds.offset/2 && isLevel){
            /* Entered Puzzle */

            /* Lerp Lava to Bottom of Puzzle */
            if(lava.transform.position.y < doSpawn.y - uds.offset/2){
                if(!lerping){

                    lerping = true;
                    startL = lava.transform.position;
                    endL = new Vector3(doSpawn.x,doSpawn.y - uds.offset/2 - lavaMovement.offset/2, -5);

                    lavaMovement.isPermStop = true;

                    StartCoroutine(lavaLerp());

                }
            }

            /* Start puzzle timer */
            if(!timing && !killing){
                timing = true;
                timeLeft = uds.time;
                maxTimeLeft = uds.time;
                StartCoroutine(timer());
            }

        }else if(player.transform.position.y > killPos.y+lavaMovement.offset/2 && lds.time != 0 && !killLerped){
            /* Player passed puzzle, execute kill to progress  */
            killing = true;
        }

        /* Execute kill */
        if(killing){
            if(!lerping){
                if(killLerped){
                    /* Restart lava after kill */
                    Debug.Log("alr WTF");
                    lavaMovement.isStop = true;
                    killing = false;
                }else{
                    /* Lerp lava */
                    killLerped = true;
                    lerping = true;
                    startL = lava.transform.position;
                    endL = killPos;
                    lavaCountdown.text = "";

                    lavaMovement.isPermStop = true;
                    StartCoroutine(lavaLerp());
                }

            }
        }
    }

    void spawnLevel(int side){
        if(isLevel){
            /* Spawn Platforms/level */

            /* Pick level to spawn which doesn't make game impossible */
            int i = Random.Range(1,levels.Count);
            while(Mathf.Abs(levelsData[i].inSide - side)!=1){
                i = Random.Range(1,levels.Count);
            }

            // Despawn lower
            if(ld != null)
                Destroy(ld);

            /* Current upper lvl becomes lower */
            ld = ud;
            lds = uds;

            // Spawn
            /* Find position to spawn on */
            spawn = new Vector2(0,spawn.y+uds.offset/2+levelsData[i].offset/2);

            /* Spawn */
            ud = Instantiate(levels[i],spawn,Quaternion.identity);
            ud.SetActive(true);
            uds = levelsData[i];

            /* Update spawning position and states */
            doSpawn = spawn;
            curSide = levelsData[i].outSide;

        }else{
            /* Spawn Puzzle */

            /* Pick random puzzle to spawn, all is possible */
            int i = Random.Range(0,puzzles.Count);

            // Despawn lower
            if(ld != null)
                Destroy(ld);

            // Spawn & Swap
            ld = ud;
            lds = uds;
            spawn = new Vector2(0,spawn.y+uds.offset/2+puzzlesData[i].offset/2);
            ud = Instantiate(puzzles[i],spawn,Quaternion.identity);
            ud.SetActive(true);
            uds = puzzlesData[i];

            /* Update position for kill execution */
            killPos = new Vector3(0,spawn.y+puzzlesData[i].offset/2-lavaMovement.offset/2,-5);
            killLerped = false;

            /* Reset states */
            doSpawn = spawn;
            curSide = puzzlesData[i].outSide;

        }

        /* Switch level type to spawn */
        isLevel = !isLevel;
    }

    /* Simple Vector3.Lerp */
    IEnumerator lavaLerp(){
        float t=0;
        while(t<lavaRiseDuration&&!lavaMovement.DONE){//End any lava movement if player dead
            float lt = t/lavaRiseDuration;
            lt = lt*lt*lt* (lt*(6f*lt-15f)+10f);//SmootherStep function
            lava.transform.position = Vector3.Lerp(startL,endL,lt);
            t += Time.deltaTime;
            yield return null;
        }
        lava.transform.position = endL;
        lerping = false;
    }

    IEnumerator timer(){
        /* Start timer when lava finishes lerping */
        while(lerping){
            yield return null;
        }

        /* Continue timer if player hasn't finished puzzle or time hasn't ran out */
        while(timing&&!killing){
            if(timeLeft>0){
                lavaCountdown.text = Mathf.Floor(timeLeft).ToString();
                timeLeft -= Time.deltaTime;
            }else{
                timeLeft = 0;
                timing = false;
                killing = true;
                Debug.Log("So killed");
            }

            yield return null;
        }
        timing = false;
    }
}
