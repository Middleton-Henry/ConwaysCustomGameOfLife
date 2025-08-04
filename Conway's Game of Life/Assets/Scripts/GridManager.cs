using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject[,] gridDisplay;
    public Sprite square;
    public float slotSize = 1f;
    public bool outlines = false;
    public float outlineThickness;

    public Camera cam;
 
    

    Ray ray;
	RaycastHit2D hit;

    private int width;
    private int height;


    private Vector2 topLeft, bottomRight;
    public float buffer;

    private float horizontalBuffer;
    private Vector2 adjustedTopLeft, adjustedBottomRight;

    public Color dead = new Color(0f,0f,0f);
    public Color alive = new Color(1f,1f,1f);

    

    
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        //verticalSpace = (int)Camera.main.orthographicSize;
        //horizontalSpace = verticalSpace * (Screen.width/Screen.height);
        

        width = gameObject.GetComponent<GameManager>().width;
        height = gameObject.GetComponent<GameManager>().height;
    }

    public void createGrid(bool[,] matrix)
    {
        for(int i = gameObject.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(gameObject.transform.GetChild(i).gameObject);
        }   

        
        height = gameObject.GetComponent<GameManager>().height;
        width = height;

        topLeft = (Vector2)cam.ScreenToWorldPoint(new Vector3(0, cam.pixelHeight, cam.farClipPlane));
        bottomRight = (Vector2)cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth, 0, cam.farClipPlane));

        float horizontalBuffer = buffer*(-1f)*((float)topLeft.x-bottomRight.x)/((float)topLeft.y-bottomRight.y)+(-1f)*((float)topLeft.x-bottomRight.x)/((float)topLeft.y-bottomRight.y);

        Debug.Log("horizontal Buffer: "+ horizontalBuffer);
        //float verticalBuffer = buffer;
        //float verticalBuffer = buffer*(-1f)/((topLeft.y-bottomRight.y)/(topLeft.x-bottomRight.x));

        

        adjustedTopLeft = new Vector2(topLeft.x + horizontalBuffer, topLeft.y - buffer);
        adjustedBottomRight = new Vector2(bottomRight.x - horizontalBuffer, bottomRight.y + buffer);

        slotSize = (float)((adjustedTopLeft.y-adjustedBottomRight.y)/width);

        //Debug.Log( "top left: "+(((float)topLeft.y) - verticalBuffer)+"\nbottom right"+adjustedBottomRight+"\nratio (x/y)"+((adjustedBottomRight.x-adjustedTopLeft.x)/(adjustedTopLeft.y-adjustedBottomRight.y))+"\noriginal ratio:"+((topLeft.y-bottomRight.y)/(topLeft.x-bottomRight.x)));


        gridDisplay = new GameObject[width,height];

        for (int col = 0; col < width; col++)
        {
            for (int row = 0; row < height; row++)
            {
                spawnSlot(col,row,matrix[col,row]);
                setSlotPosition(col,row);
                updateSlotColor(col,row,matrix[col,row]);
            }
        }
        resizeGridOutline(gameObject.GetComponent<MenuManager>().outlineSize.value);
    }


    public void updateGridColors(bool[,] matrix)
    {
        for (int col = 0; col < width; col++)
        {
            for (int row = 0; row < height; row++)
            {
                updateSlotColor(col,row,matrix[col,row]);
            }
        }
        
    }

    public void updateSlotColor(int col, int row, bool value)
    {
        if(value) gridDisplay[col,row].GetComponent<SpriteRenderer>().color = alive;
        else gridDisplay[col,row].GetComponent<SpriteRenderer>().color = dead;

    }

    public void setSlotPosition(int col, int row)
    {


        float positionX = Mathf.Lerp(adjustedTopLeft.x, adjustedBottomRight.x, (float) (col+.5f)/width);
        float positionY = Mathf.Lerp(adjustedTopLeft.y, adjustedBottomRight.y, (float) (row+.5f)/height);

                //grid[col,row] = Instantiate(slotPrefab, new Vector3 (positionX, positionY, positionZ), Quaternion.identity);
                //grid[col,row].transform.parent = transform;
                //grid[col,row].gameObject.name = "Slot_" + col + "," + row;
        
        gridDisplay[col,row].transform.position = new Vector2(positionX,positionY);
        //gridDisplay[col,row].transform.localScale = new Vector2(slotSize,slotSize);
    }

    public void spawnSlot(int col, int row, bool value)
    {
        GameObject newSlot = new GameObject ("Cell:"+col+"_"+row);

        //newSlot.transform.position = new Vector3(col-(horizontalSpace - slotSize*0.5f),row-(verticalSpace - slotSize*0.5f),0f);

        gridDisplay[col,row] = newSlot;
        newSlot.transform.parent = gameObject.transform;
        newSlot.AddComponent(typeof(BoxCollider2D));

        var defaultColor = newSlot.AddComponent<SpriteRenderer>();
        defaultColor.sprite = square;
        
    }
    public void resizeGridOutline(float input)//from 0 to 1
    {
        outlineThickness = input;
        float newSlotSize = slotSize-outlineThickness*slotSize;

        for (int col = 0; col < width; col++)
        {
            for (int row = 0; row < height; row++)
            {
                gridDisplay[col,row].transform.localScale = new Vector2(newSlotSize,newSlotSize);
            }
        }

    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            //ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            hit = Physics2D.Raycast(cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if(hit.collider != null)
            {
                
                string objectName = hit.collider.name;
                Debug.Log("object hit: " + objectName);
                if(objectName.Substring(0, 4) == "Cell")
                {
                    string[] SplitText = objectName.Split(':', '_');

                    int col = int.Parse(SplitText[1]);
                    int row = int.Parse(SplitText[2]);
                    
                    //Debug.Log("neighbours: " + gameObject.GetComponent<GameManager>().calcNeighbours(col, row));
                    Debug.Log("selected slot [" + col + "," + row +"]");
                    if(gameObject.GetComponent<GameManager>().currentStateMatrix[col,row]==false) gameObject.GetComponent<MenuManager>().incrementPopulationCounter(1);
                    updateSlotColor(col,row,true);
                    gameObject.GetComponent<GameManager>().currentStateMatrix[col,row]=true;
                }
                    
            }
        }
        if(Input.GetMouseButtonDown(1))
        {
            //ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            hit = Physics2D.Raycast(cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if(hit.collider != null)
            {
                
                string objectName = hit.collider.name;
                Debug.Log("object hit: " + objectName);
                if(objectName.Substring(0, 4) == "Cell")
                {
                    string[] SplitText = objectName.Split(':', '_');

                    int col = int.Parse(SplitText[1]);
                    int row = int.Parse(SplitText[2]);
                    
                    Debug.Log("selected slot [" + col + "," + row +"]");
                    if(gameObject.GetComponent<GameManager>().currentStateMatrix[col,row]) gameObject.GetComponent<MenuManager>().incrementPopulationCounter(-1);
                    updateSlotColor(col,row,false);
                    gameObject.GetComponent<GameManager>().currentStateMatrix[col,row]=false;
                }
                    
            }
        }
        

    }

}
