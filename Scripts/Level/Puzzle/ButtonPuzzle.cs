using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class ButtonPuzzle : MonoBehaviour
{
    /* Button data types */
    public struct BUTTONDATA{// Info for all generated buttons
        public int pos;

        public Color c;

        /* Constructor */
        public BUTTONDATA(int pos, Color c){
            this.pos = pos;
            this.c = c;
        }
    }
    public struct TARGETDATA{// Info for button that should be pressed
        public int index;
        public int pos;
        public Color c;

        /* Constructor */
        public TARGETDATA(int index,int pos){
            this.index = index;
            this.pos = pos;
            this.c = Color.black;
        }
        /* Overload constructor */
        public TARGETDATA(int index,int pos,Color c){
            this.index = index;
            this.pos = pos;
            this.c = c;
        }
    }
    public enum COLOR{red=0,blue=1,green=2,yellow=3,cyan=4};

    /* Level manager reference */
    private LevelManager levelManager;

    /* Puzzle info */
    public int maxNum,minNum;
    public List<Color> colors;
    private List<int> colorNum = new List<int>();
    private Color lMostC,rMostC;
    public List<GameObject> buttons;

    public GameObject gate;
    private SpriteRenderer gateRenderer;
    private BoxCollider2D gateBox2D;

    private List<int> lOffset = new List<int>(){-3,0,3};
    private List<int> rOffset = new List<int>(){-3,0,3};
    public int pressed;//which button is pressed

    /* Button gameobjects */
    private int num;
    private List<GameObject> chosenButtons;
    private List<int> discrete = new List<int>();

    private List<GameObject> buttonsTop=new List<GameObject>();
    private List<BoxCollider2D> buttonsTopBox2D = new List<BoxCollider2D>();

    private LayerMask playerMask;
    public float pressOffset;//how much the button goes down after press

    /* Button puzzle data */
    private List<BUTTONDATA> buttonsData=new List<BUTTONDATA>();
    private TARGETDATA targetData;

    void Start(){
        /* Initialization */
        pressed=-1;
        for(int i=0;i<colors.Count;i++)
            colorNum.Add(0);
        if(GameObject.Find("LevelManager")!=null)
            levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        gateRenderer = gate.GetComponent<SpriteRenderer>();
        gateBox2D = gate.GetComponent<BoxCollider2D>();

        /* Randomly generate a button puzzle */
        /* Choose the butons to use */
        num = Random.Range(minNum,maxNum+1);
        // num = 4;

        chosenButtons = new List<GameObject>(buttons);
        chosenButtons = shuffle(chosenButtons);

        /* Customize buttons */
        customizeButton();

        /* Make pos variable discrete */
        discrete.Sort();
        for(int i=0;i<buttonsData.Count;i++){
            int nPos = bs(discrete,0,discrete.Count-1,buttonsData[i].pos);
            buttonsData[i] = new BUTTONDATA(nPos,buttonsData[i].c);
            /* Debug.Log(buttonsData[i].pos); */

            /* Leftmost and rightmost color */
            if(nPos==0)
                lMostC = buttonsData[i].c;
            if(nPos==discrete.Count-1)
                rMostC = buttonsData[i].c;
        }

        /* Find target button for this generation */
        findTarget();
        Debug.Log(targetData.index);
        Debug.Log(targetData.pos);
        Debug.Log(targetData.c);

    }

    void Update(){
        /* Check if pressed button is correct */
        if(pressed!=-1){

            bool correctIndex = (pressed == targetData.index||targetData.index==-1);
            bool correctPos = (buttonsData[pressed].pos == targetData.pos||targetData.pos==-1);
            bool correctColor = (buttonsData[pressed].c == targetData.c||targetData.c==Color.black);
            if(correctIndex && correctPos && correctColor){
                /* Correct button */
                StartCoroutine(gateOpen(0.5f));
                Debug.Log("correct");
            }else{
                /* Incorrect button */
                levelManager.killing = true;
                Debug.Log("incorrect");
            }
        }
    }

    void FixedUpdate(){
        checkButton();
    }

    void checkButton(){
        /* Check if any button has been pressed */
        for(int i=0;i<num;i++){

            /* Boxcast */
            playerMask = LayerMask.GetMask("Player");
            RaycastHit2D hit = Physics2D.BoxCast(buttonsTopBox2D[i].bounds.center, buttonsTopBox2D[i].bounds.size, 0f, Vector2.up, 0.1f,playerMask);

            /* Button Press */
            if(hit.collider != null && pressed==-1){
                pressed = i;
                StartCoroutine(buttonDown(i,0.25f));
                /* buttonsTop[i].SetActive(false); */
            }

        }
    }

    void customizeButton(){
        for(int i=0;i<num;i++){
            int posTmp;

            /* Show */
            chosenButtons[i].SetActive(true);

            /* Get button top reference */
            GameObject buttonTop = chosenButtons[i].transform.GetChild(0).gameObject;
            buttonsTop.Add(buttonTop);
            buttonsTopBox2D.Add(buttonsTop[i].GetComponent<BoxCollider2D>());

            /* Color */
            int randomColor = Random.Range(0,colors.Count);
            colorNum[randomColor]++;
            Color colorTmp = colors[randomColor];
            buttonsTop[i].GetComponent<SpriteRenderer>().color = colorTmp;

            /* Position (Left Mid Right) */
            if(chosenButtons[i].transform.position.x < 0){//Left Button

                /* Apply random offset */
                int tmp = Random.Range(0,lOffset.Count);
                chosenButtons[i].transform.position = new 
                    Vector2(chosenButtons[i].transform.position.x + lOffset[tmp],chosenButtons[i].transform.position.y);

                /* Calculate order in buttons */
                posTmp = 2;
                if(lOffset[tmp]<0){
                    posTmp--;
                }else if(lOffset[tmp]>0){
                    posTmp++;
                }

                /* Ensure each offset is only applied once */
                lOffset.RemoveAt(tmp);

            }else{//Right Button

                /* Apply random offset */
                int tmp = Random.Range(0,rOffset.Count);
                chosenButtons[i].transform.position = new 
                    Vector2(chosenButtons[i].transform.position.x + rOffset[tmp],chosenButtons[i].transform.position.y);

                /* Calculate order in buttons */
                posTmp = 5;
                if(rOffset[tmp]<0){
                    posTmp--;
                }else if(rOffset[tmp]>0){
                    posTmp++;
                }

                /* Ensure each offset is only applied once */
                rOffset.RemoveAt(tmp);
            }

            /* Store button data into struct */
            buttonsData.Add(new BUTTONDATA(posTmp,colorTmp));
            discrete.Add(posTmp);

        }
    }

    void findTarget(){
        /* 3 Cases */
        if(num == 3){
            if(colorNum[(int)COLOR.red]==num||colorNum[(int)COLOR.green]==num||colorNum[(int)COLOR.blue]==num||colorNum[(int)COLOR.yellow]==num||colorNum[(int)COLOR.cyan]==num){
                /* All same color */
                targetData = new TARGETDATA(-1,1);
            }
            else if(colorNum[(int)COLOR.red]==2||colorNum[(int)COLOR.green]==2||colorNum[(int)COLOR.blue]==2||colorNum[(int)COLOR.yellow]==2||colorNum[(int)COLOR.cyan]==2){
                /* Two same color */
                targetData = new TARGETDATA(-1,0);
            }
            else if(colorNum[(int)COLOR.green]==1){
                /* One green */
                targetData = new TARGETDATA(-1,-1,Color.green);
            }else if(rMostC==Color.cyan||rMostC==Color.blue){
                /* Right most blue/cyan */
                targetData = new TARGETDATA(-1,0);
            }else{
                /* Default */
                targetData = new TARGETDATA(-1,2);
            }
        }

        /* 4 Cases */
        if(num == 4){
            if(colorNum[(int)COLOR.yellow]>=1&&colorNum[(int)COLOR.red]>=1){
                /* At least one yellow & at least one red */
                targetData = new TARGETDATA(-1,2);
            }else if(colorNum[(int)COLOR.green]>=1&&colorNum[(int)COLOR.cyan]>=1){
                /* At least one green & at least one cyan */
                targetData = new TARGETDATA(-1,3);
            }else if(colorNum[(int)COLOR.red]>=3||colorNum[(int)COLOR.green]>=3||colorNum[(int)COLOR.blue]>=3||colorNum[(int)COLOR.yellow]>=3||colorNum[(int)COLOR.cyan]>=3){
                /* 3 or 3+ same color */
                targetData = new TARGETDATA(-1,2);
            }else if(lMostC==Color.red||lMostC==Color.blue){
                /* Blue or red leftmost */
                targetData = new TARGETDATA(-1,3);
            }else if(colorNum[(int)COLOR.red]==1){
                /* One and only one red */
                targetData = new TARGETDATA(-1,-1,Color.red);
            }else if(colorNum[(int)COLOR.blue]==2){
                /* Two and only two blue */
                targetData = new TARGETDATA(-1,-1,Color.blue);
            }else{
                /* Default */
                targetData = new TARGETDATA(-1,0);
            }
        }

        /* 5 Cases */
        if(num == 5){
            if(colorNum[(int)COLOR.yellow]==1&&colorNum[(int)COLOR.red]==1){
                /* 1 yellow & 1 red */
                targetData = new TARGETDATA(-1,1);
            }else if(colorNum[(int)COLOR.blue]==1&&colorNum[(int)COLOR.cyan]==1){
                /* 1 blue & 1 cyan */
                targetData = new TARGETDATA(-1,0);
            }else if(colorNum[(int)COLOR.green]==1){
                /* One green */
                targetData = new TARGETDATA(-1,-1,Color.green);
            }else if(colorNum[(int)COLOR.red]>=4||colorNum[(int)COLOR.green]>=4||colorNum[(int)COLOR.blue]>=4||colorNum[(int)COLOR.yellow]>=4||colorNum[(int)COLOR.cyan]>=4){
                /* 4 or 4+ same */
                targetData = new TARGETDATA(-1,2);
            }else if(colorNum[(int)COLOR.yellow]==1){
                /* One yellow */
                targetData = new TARGETDATA(-1,4);
            }else{
                /* Default */
                targetData = new TARGETDATA(-1,2);
            }
        }

        /* 6 Cases */
        if(num == 6){
            if(colorNum[(int)COLOR.yellow]==1&&colorNum[(int)COLOR.green]==1){
                /* 1 yellow and 1 green */
                targetData = new TARGETDATA(-1,5);
            }else if(rMostC == Color.red){
                /* Right most red */
                targetData = new TARGETDATA(-1,5);
            }else if(colorNum[(int)COLOR.blue]>=1&&colorNum[(int)COLOR.red]>=1&&colorNum[(int)COLOR.cyan]>=1){
                /* Blue red cyan exists */
                targetData = new TARGETDATA(-1,3);
            }else if(colorNum[(int)COLOR.red]==1){
                /* One red */
                targetData = new TARGETDATA(-1,-1,Color.red);
            }else if(colorNum[(int)COLOR.cyan]==1){
                /* One cyan */
                targetData = new TARGETDATA(-1,1);
            }else{
                /* Default */
                targetData = new TARGETDATA(-1,2);
            }
        }
    }

    IEnumerator gateOpen(float openTime){
        float t=0;
        Color startL = gateRenderer.color;
        Color endL = new Color(startL.r,startL.g,startL.b,0);

        gateBox2D.enabled = false;
        while(t<openTime){
            float lt = t/openTime;
            gateRenderer.color = Color.Lerp(startL,endL,lt);
            t += Time.deltaTime;
            yield return null;
        }
        gateRenderer.color = endL;
    }

    IEnumerator buttonDown(int buttonIndex,float downTime){
        float t=0;
        Vector3 startL = buttonsTop[buttonIndex].transform.position;
        Vector3 endL = new Vector3(startL.x,startL.y-pressOffset,1);

        while(t<downTime){
            float lt = t/downTime;
            buttonsTop[buttonIndex].transform.position = Vector3.Lerp(startL,endL,lt);
            t += Time.deltaTime;
            yield return null;
        }
        buttonsTop[buttonIndex].transform.position = endL;
    }

    int bs(List<int> A,int l,int r,int target){
        /* Binary search algorithm for discreting buttonsData.pos */

        if(l>r)
            return 0;

        int mid = (l+r)/2;

        if(A[mid] == target)
            return mid;
        if(A[mid]>target)
            return bs(A,l,mid-1,target);
        return bs(A,mid+1,r,target);
    }

    List<GameObject> shuffle(List<GameObject> a){
        /* Fisher-Yates Shuffle */
        for(int i=a.Count-1;i>0;i--){
            int target = Random.Range(0,i);

            GameObject tmp = a[i];

            a[i]=a[target];
            a[target]=tmp;
        }
        return a;
    }

}
