using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    [Header("General Information")]
    public int width;
    public int height;

    public bool[,] currentStateMatrix;
    private bool[,] previousStateMatrix;
    public bool[,] initialStateMatrix;

    private string trueReturn;
    private string falseReturn;

    public int population = 0;

    private Camera cam;

    public int generation = 1;

    [Header("Automatic State Update")]
    public bool automaticUpdateBool = false;

    [Range(0.01f, 40.0f)]
    public float automaticUpdateSpeed;

    public float currentTime=0f;

    [Header("Custom Rules")]
    public bool customRulsetBool = false;
    //public List<string> rules = new List<string>();

    public enum neighbourRange {checkFour, checkEight, checkTwelve, checkTwenty, checkTwentyFour, checkThirtySix}; //4,8,12,20,24,36
    public neighbourRange neighbourRangeSelection = new neighbourRange();
    private neighbourRange prevRange;

    
    public enum neighbourRules {dead, same, alive};
    //public neighbourRules neighbourRulesSelection = new neighbourRules();
    public neighbourRules[] stateRules = new neighbourRules[37];

    public float randomPercentage;

    private bool isRunningUpdate;

    

    

    void Start()
    {
        //resizeRulesList();
        prevRange = neighbourRangeSelection;
        cam = Camera.main;

        calcWidth();


       currentStateMatrix = new bool[width,height];
       currentStateMatrix = initialValues(currentStateMatrix);

       previousStateMatrix = new bool[width,height];
       previousStateMatrix = initialValues(previousStateMatrix);

       initialStateMatrix = new bool[width,height];
       initialStateMatrix = currentStateMatrix.Clone() as bool[,];
        
        printGuide();

        gameObject.GetComponent<GridManager>().createGrid(currentStateMatrix);
    }

    public void setNeighbourRangeSelection(int index)
    {

        neighbourRangeSelection = (neighbourRange)index;
        
        /*switch(index)
        {
            case 0:
            neighbourRangeSelection = 
            break;
        }
        */
    }



    public void rebuildGrid(int length)
    {
        generation = 1;
        population = 0;
        width = length;
        height = length;

        currentStateMatrix = new bool[width,height];
        currentStateMatrix = initialValues(currentStateMatrix);

        previousStateMatrix = new bool[width,height];
        previousStateMatrix = initialValues(previousStateMatrix);
        

        gameObject.GetComponent<GridManager>().createGrid(currentStateMatrix);
    }

    public void printGuide()
    {
        Debug.Log("\n"
        +"Q: Toggle custom rule set\n"
        +"W: Distribute random values\n"
        +"E: Toggle automatic state updater\n"
        +"R: Reinitialize grid display\n"
        +"T: \n"
        +"Y: \n"
        +"U: Update state by 1 and update grid value visualization\n"
        +"I: Print state number"
        +"O: Clear all slots"
        );

    }

    public void setDefaultSettings()
    {
        neighbourRangeSelection = neighbourRange.checkEight;

        for(int i = 0; i < 37; i++)
        {
            if(i==2) 
            {
                changeNeighbourRuleState(i,1);
            }
            else
            {
                if(i==3) 
                {
                    changeNeighbourRuleState(i,0);
                }
                else
                {
                    changeNeighbourRuleState(i,2);
                }
            }

        }
    }

    public void changeNeighbourRuleState(int index, int caseIndex)
    {
        switch(caseIndex)
        {
            case 0:
            stateRules[index] = neighbourRules.alive;
            break;

            case 1:
            stateRules[index] = neighbourRules.same;
            break;

            case 2:
            stateRules[index] = neighbourRules.dead;
            break;
        }
        
    }

    public void calcWidth()
    {
        width = height; 
        Debug.Log("Dimensions: " + width +","+height);
    }

    public bool[,] randomDistribution(bool[,] matrix, float percentAlive)
    {
        population = 0;

        for(int col = 0; col < width; col++)
        {
            for(int row = 0; row < height; row++)
            {
                matrix[col,row] = true;

                if(Random.Range(0f,1f) > percentAlive) matrix[col,row] = false;
                if(matrix[col,row]) population++;
            }
        }

        gameObject.GetComponent<MenuManager>().updatePopulationCounter(population);
        
        return matrix;

        
    }

    public bool[,] initialValues(bool[,] matrix)
    {
        for(int col = 0; col < width; col++)
        {
            for(int row = 0; row < height; row++)
            {
                matrix[col,row] = false;
            }
        }
        return matrix;
    }

    public void clearInitialMatrix()
    {
        for(int col = 0; col < width; col++)
        {
            for(int row = 0; row < height; row++)
            {
                initialStateMatrix[col,row] = false;
            }
        }
    }

    /*
    public void resizeRulesList() {
	        switch(neighbourRangeSelection) //4,8,12,20,24,36
            {
                case neighbourRange.checkFour:
                    stateRules = new neighbourRules[5];
                    break;

                case neighbourRange.checkEight:
                    stateRules = new neighbourRules[9];
                    break;

                case neighbourRange.checkTwelve:
                    stateRules = new neighbourRules[13];
                    break;

                case neighbourRange.checkTwenty:
                    stateRules = new neighbourRules[21];
                    break;

                case neighbourRange.checkTwentyFour:
                    stateRules = new neighbourRules[25];
                    break;

                case neighbourRange.checkThirtySix:
                    stateRules = new neighbourRules[37];
                    break;
            }
    }
    */

    public void distributeRandomValues()
    {
        generation = 1;
        gameObject.GetComponent<MenuManager>().updateGenerationCounter(generation);
        currentStateMatrix = randomDistribution(currentStateMatrix,randomPercentage);
        gameObject.GetComponent<GridManager>().updateGridColors(currentStateMatrix);
        initialStateMatrix = currentStateMatrix.Clone() as bool[,];
    }

    void Update()
    {


        if(automaticUpdateBool && !isRunningUpdate)
        {
            StartCoroutine(automaticStateUpdater());
        }
        if(!automaticUpdateBool && isRunningUpdate)
        {
            StopCoroutine(automaticStateUpdater());

        }

        /*
        if (Input.GetKeyDown(KeyCode.O))
        {
            initialValues(currentStateMatrix);
            initialValues(previousStateMatrix);
            gameObject.GetComponent<GridManager>().updateGridColors(currentStateMatrix);
            generation = 1;
        }
        */
        
        
    }

    public void clearGridValues()
    {
            initialValues(currentStateMatrix);
            initialValues(previousStateMatrix);
            gameObject.GetComponent<GridManager>().updateGridColors(currentStateMatrix);
            generation = 1;
            population = 0;
    }

    public void replaceColorPalette()
    {
        gameObject.GetComponent<GridManager>().updateGridColors(currentStateMatrix);
    }

    public void forwardOneState()
    {
        stateUpdate();
        gameObject.GetComponent<GridManager>().updateGridColors(currentStateMatrix);
    }


    IEnumerator automaticStateUpdater()
    {
        isRunningUpdate = true;
        //while(true)
        //{
            stateUpdate();
            gameObject.GetComponent<GridManager>().updateGridColors(currentStateMatrix);
            


            if(!automaticUpdateBool) 
            {
                yield return null;
                isRunningUpdate = false;
            }
            if(automaticUpdateSpeed != 40.0f)
            {
                yield return new WaitForSeconds(1f/automaticUpdateSpeed);
                isRunningUpdate = false;
            }
            else 
            {
                yield return null;
                isRunningUpdate = false;
            }
        //}
    }

    public string printMatrix (bool[,] matrix)
    {
        string print = "\n";
        for(int col = 0; col < width; col++)
        {
            for(int row = 0; row < height; row++)
            {
                print += returnType(matrix[col,row]);
            }

            print += "\n";
        }

        return print;

    }

    public string returnType (bool value)
    {
        if(value) return trueReturn;

        return falseReturn;

    }

    public void resetMatrix()
    {
        generation = 1;
        gameObject.GetComponent<MenuManager>().updateGenerationCounter(generation);
        currentStateMatrix = initialStateMatrix.Clone() as bool[,];
        gameObject.GetComponent<GridManager>().updateGridColors(currentStateMatrix);
        calcPopulation();
        gameObject.GetComponent<MenuManager>().updatePopulationCounter(population);
    }

    public void calcPopulation()
    {
        population = 0;
        for(int col = 0; col < width; col++)
            {
                for(int row = 0; row < height; row++)
                {
                    if (currentStateMatrix[col,row]) population++;
                }
            }
    }

    public void stateUpdate()
    {
        if(generation ==1)
        {
            initialStateMatrix = new bool[width,width];
            initialStateMatrix = currentStateMatrix.Clone() as bool[,];
        }
        generation++;
        gameObject.GetComponent<MenuManager>().updateGenerationCounter(generation);
        population = 0;

        previousStateMatrix = currentStateMatrix.Clone() as bool[,];


        for(int col = 0; col < width; col++)
        {
            for(int row = 0; row < height; row++)
            {
                currentStateMatrix[col,row] = customSlotUpdate(col,row);
                if (currentStateMatrix[col,row]) population++;
            }
        }

        gameObject.GetComponent<MenuManager>().updatePopulationCounter(population);

    }

    public bool slotUpdate (int col, int row)
    {
        

        int neighbours = calcEightNeighbours(col,row);

        switch (neighbours)
        {
            case 1://if 1 neighbour, the cell dies
                return(false);
            case 2://if 2 neighbours, the cell stays the same
                if(previousStateMatrix[col,row]) return(true);
                return(false);
            case 3://if 3 neighbours, the cell is alive
                return(true);
            default://if 4 - 8 neighbours, the cell dies
                return (false);
        }
    }

    public bool customSlotUpdate (int col, int row)
    {
        int neighbours=0;

        switch(neighbourRangeSelection) //4,8,12,20,24,36
        {
            case neighbourRange.checkFour://A
                neighbours = calcFourNeighbours(col,row);
                break;

            case neighbourRange.checkEight://B
                neighbours = calcEightNeighbours(col,row);
                break;

            case neighbourRange.checkTwelve://C
                neighbours = calcTwelveNeighbours(col,row);
                neighbours += calcEightNeighbours (col,row);
                break;

            case neighbourRange.checkTwenty://D
                neighbours = calcTwentyNeighbours(col,row);
                neighbours += calcEightNeighbours(col,row);
                break;

            case neighbourRange.checkTwentyFour://E
                neighbours = calcTwentyFourNeighbours(col,row);
                neighbours += calcEightNeighbours(col,row);
                break;

            case neighbourRange.checkThirtySix://F
                neighbours = calcThirtySixNeighbours(col,row);
                neighbours = calcTwentyFourNeighbours(col,row);
                neighbours += calcEightNeighbours(col,row);
                break;
        }

        switch (stateRules[neighbours])
        {
            case neighbourRules.alive:
                return (true);
            case neighbourRules.dead:
                return (false);
            case neighbourRules.same:
                if(previousStateMatrix[col,row]) return(true);
                return(false);
        }
        return false;
    }



    public int calcFourNeighbours(int col, int row)
    {
        int neighbours = 0;

        int columnMinusOne = col-1;
        int columnPlusOne = col+1;

        int rowMinusOne = row-1;
        int rowPlusOne = row+1;



        int hashValue = 0;

        //behind column limit (ignore check 2)
        if(columnMinusOne < 0) hashValue = 1; 
        else if(columnPlusOne == width) hashValue = 2;
        //beyond column limit
        

        //behind row limit (ignore check 2)
        if(rowMinusOne < 0) hashValue += 10;
        else if(rowPlusOne == height) hashValue += 20;
        //beyond row limit
       



        switch (hashValue)
        {
            case 1: //cannot check col-1
                if(previousStateMatrix[col,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[col,rowPlusOne]) neighbours+=1;

                if(previousStateMatrix[columnPlusOne,row]) neighbours+=1;
                return neighbours;

            case 2: //cannot check col+1
                if(previousStateMatrix[columnMinusOne,row]) neighbours+=1;

                if(previousStateMatrix[col,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[col,rowPlusOne]) neighbours+=1;
                return neighbours;

            case 10: //cannot check row-1
                if(previousStateMatrix[columnMinusOne,row]) neighbours+=1;

                if(previousStateMatrix[col,rowPlusOne]) neighbours+=1;

                if(previousStateMatrix[columnPlusOne,row]) neighbours+=1;
                return neighbours;

            case 20: //cannot check row+1
                if(previousStateMatrix[columnMinusOne,row]) neighbours+=1;

                if(previousStateMatrix[col,rowMinusOne]) neighbours+=1;

                if(previousStateMatrix[columnPlusOne,row]) neighbours+=1;
                return neighbours;

            case 11: //cannot check col-1 or row-1
                if(previousStateMatrix[col,rowPlusOne]) neighbours+=1;
                
                if(previousStateMatrix[columnPlusOne,row]) neighbours+=1;
                return neighbours;

            case 12: //cannot check col+1 or row-1
                if(previousStateMatrix[columnMinusOne,row]) neighbours+=1;

                
                if(previousStateMatrix[col,rowPlusOne]) neighbours+=1;
                return neighbours;

            case 21: //cannot check col-1 or row+1
                
                if(previousStateMatrix[col,rowMinusOne]) neighbours+=1;

                if(previousStateMatrix[columnPlusOne,row]) neighbours+=1;
                return neighbours;

            case 22: //cannot check col+1 or row+1
                if(previousStateMatrix[columnMinusOne,row]) neighbours+=1;

                
                if(previousStateMatrix[col,rowMinusOne]) neighbours+=1;
                return neighbours;

            default: //check all neighbours
                if(previousStateMatrix[columnMinusOne,row]) neighbours+=1;

                
                if(previousStateMatrix[col,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[col,rowPlusOne]) neighbours+=1;

                if(previousStateMatrix[columnPlusOne,row]) neighbours+=1;
                return neighbours;

        }
    }

    public int calcEightNeighbours(int col, int row)
    {
        int neighbours = 0;

        int columnMinusOne = col-1;
        int columnPlusOne = col+1;

        int rowMinusOne = row-1;
        int rowPlusOne = row+1;



        int hashValue = 0;

        //behind column limit (ignore check 2)
        if(columnMinusOne < 0) hashValue = 1; 
        else if(columnPlusOne == width) hashValue = 2;
        //beyond column limit
        

        //behind row limit (ignore check 2)
        if(rowMinusOne < 0) hashValue += 10;
        else if(rowPlusOne == height) hashValue += 20;
        //beyond row limit
       



        switch (hashValue)
        {
            case 1: //cannot check col-1
                if(previousStateMatrix[col,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[col,rowPlusOne]) neighbours+=1;

                if(previousStateMatrix[columnPlusOne,row]) neighbours+=1;
                if(previousStateMatrix[columnPlusOne,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[columnPlusOne,rowPlusOne]) neighbours+=1;
                return neighbours;

            case 2: //cannot check col+1
                if(previousStateMatrix[columnMinusOne,row]) neighbours+=1;
                if(previousStateMatrix[columnMinusOne,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[columnMinusOne,rowPlusOne]) neighbours+=1;

                if(previousStateMatrix[col,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[col,rowPlusOne]) neighbours+=1;
                return neighbours;

            case 10: //cannot check row-1
                if(previousStateMatrix[columnMinusOne,row]) neighbours+=1;
                if(previousStateMatrix[columnMinusOne,rowPlusOne]) neighbours+=1;

                if(previousStateMatrix[col,rowPlusOne]) neighbours+=1;

                if(previousStateMatrix[columnPlusOne,row]) neighbours+=1;
                if(previousStateMatrix[columnPlusOne,rowPlusOne]) neighbours+=1;
                return neighbours;

            case 20: //cannot check row+1
                if(previousStateMatrix[columnMinusOne,row]) neighbours+=1;
                if(previousStateMatrix[columnMinusOne,rowMinusOne]) neighbours+=1;

                if(previousStateMatrix[col,rowMinusOne]) neighbours+=1;

                if(previousStateMatrix[columnPlusOne,row]) neighbours+=1;
                if(previousStateMatrix[columnPlusOne,rowMinusOne]) neighbours+=1;
                return neighbours;

            case 11: //cannot check col-1 or row-1
                if(previousStateMatrix[col,rowPlusOne]) neighbours+=1;
                
                if(previousStateMatrix[columnPlusOne,row]) neighbours+=1;
                if(previousStateMatrix[columnPlusOne,rowPlusOne]) neighbours+=1;
                return neighbours;

            case 12: //cannot check col+1 or row-1
                if(previousStateMatrix[columnMinusOne,row]) neighbours+=1;
                if(previousStateMatrix[columnMinusOne,rowPlusOne]) neighbours+=1;

                
                if(previousStateMatrix[col,rowPlusOne]) neighbours+=1;
                return neighbours;

            case 21: //cannot check col-1 or row+1
                
                if(previousStateMatrix[col,rowMinusOne]) neighbours+=1;

                if(previousStateMatrix[columnPlusOne,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[columnPlusOne,row]) neighbours+=1;
                return neighbours;

            case 22: //cannot check col+1 or row+1
                if(previousStateMatrix[columnMinusOne,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[columnMinusOne,row]) neighbours+=1;

                
                if(previousStateMatrix[col,rowMinusOne]) neighbours+=1;
                return neighbours;

            default: //check all neighbours
                if(previousStateMatrix[columnMinusOne,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[columnMinusOne,row]) neighbours+=1;
                if(previousStateMatrix[columnMinusOne,rowPlusOne]) neighbours+=1;

                
                if(previousStateMatrix[col,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[col,rowPlusOne]) neighbours+=1;

                if(previousStateMatrix[columnPlusOne,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[columnPlusOne,row]) neighbours+=1;
                if(previousStateMatrix[columnPlusOne,rowPlusOne]) neighbours+=1;
                return neighbours;

        }

        /*
                if(previousStateMatrix[columnMinusOne,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[columnMinusOne,row]) neighbours+=1;
                if(previousStateMatrix[columnMinusOne,rowPlusOne]) neighbours+=1;

                
                if(previousStateMatrix[col,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[col,rowPlusOne]) neighbours+=1;

                if(previousStateMatrix[columnPlusOne,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[columnPlusOne,row]) neighbours+=1;
                if(previousStateMatrix[columnPlusOne,rowPlusOne]) neighbours+=1;
                

        */
    }

    public int calcTwelveNeighbours(int col, int row)
    {
        int neighbours = 0;

        int columnMinusTwo = col-2;
        int columnPlusTwo = col+2;

        int rowMinusTwo = row-2;
        int rowPlusTwo = row+2;

        int columnMinusOne = col-1;
        int columnPlusOne = col+1;

        int rowMinusOne = row-1;
        int rowPlusOne = row+1;



        int hashValue = 0;

        //behind column limit (ignore check 2)
        if(columnMinusTwo < 0) hashValue = 1; 
        else if(columnPlusTwo >= width) hashValue = 2;
        //beyond column limit
        

        //behind row limit (ignore check 2)
        if(rowMinusTwo < 0) hashValue += 10;
        else if(rowPlusTwo >= height) hashValue += 20;
        //beyond row limit
       



        switch (hashValue)
        {
            case 1: //cannot check col-2
                if(previousStateMatrix[col,rowMinusTwo]) neighbours+=1;
                if(previousStateMatrix[col,rowPlusTwo]) neighbours+=1;

                if(previousStateMatrix[columnPlusTwo,row]) neighbours+=1;

                return neighbours;

            case 2: //cannot check col+2
                if(previousStateMatrix[columnMinusTwo,row]) neighbours+=1;

                if(previousStateMatrix[col,rowMinusTwo]) neighbours+=1;
                if(previousStateMatrix[col,rowPlusTwo]) neighbours+=1;

                return neighbours;

            case 10: //cannot check row-2
                if(previousStateMatrix[columnMinusTwo,row]) neighbours+=1;

                if(previousStateMatrix[col,rowPlusTwo]) neighbours+=1;

                if(previousStateMatrix[columnPlusTwo,row]) neighbours+=1;


                return neighbours;

            case 20: //cannot check row+2
                if(previousStateMatrix[columnMinusTwo,row]) neighbours+=1;

                if(previousStateMatrix[col,rowMinusTwo]) neighbours+=1;

                if(previousStateMatrix[columnPlusTwo,row]) neighbours+=1;

                return neighbours;

            case 11: //cannot check col-2 or row-2
                if(previousStateMatrix[col,rowPlusTwo]) neighbours+=1;
                
                if(previousStateMatrix[columnPlusTwo,row]) neighbours+=1;

                return neighbours;

            case 12: //cannot check col+2 or row-2
                if(previousStateMatrix[columnMinusTwo,row]) neighbours+=1;

                
                if(previousStateMatrix[col,rowPlusTwo]) neighbours+=1;


                return neighbours;

            case 21: //cannot check col-2 or row+2
                
                if(previousStateMatrix[col,rowMinusTwo]) neighbours+=1;

                if(previousStateMatrix[columnPlusTwo,row]) neighbours+=1;

                return neighbours;

            case 22: //cannot check col+2 or row+2
                if(previousStateMatrix[columnMinusTwo,row]) neighbours+=1;
                
                if(previousStateMatrix[col,rowMinusTwo]) neighbours+=1;


                return neighbours;

            default: //check all neighbours
                if(previousStateMatrix[columnMinusTwo,row]) neighbours+=1;

                
                if(previousStateMatrix[col,rowMinusTwo]) neighbours+=1;
                if(previousStateMatrix[col,rowPlusTwo]) neighbours+=1;

                if(previousStateMatrix[columnPlusTwo,row]) neighbours+=1;

                return neighbours;

        }
    }

    public int calcTwentyNeighbours(int col, int row)
    {
        int neighbours = 0;

        int columnMinusTwo = col-2;
        int columnPlusTwo = col+2;

        int rowMinusTwo = row-2;
        int rowPlusTwo = row+2;

        int columnMinusOne = col-1;
        int columnPlusOne = col+1;

        int rowMinusOne = row-1;
        int rowPlusOne = row+1;



        int hashValue = 0;

        //behind column limit (ignore check 2)
        if(columnMinusTwo < 0) hashValue = 1; 
        else if(columnPlusTwo >= width) hashValue = 2;
        //beyond column limit
        

        //behind row limit (ignore check 2)
        if(rowMinusTwo < 0) hashValue += 10;
        else if(rowPlusTwo >= height) hashValue += 20;
        //beyond row limit
       



        switch (hashValue)
        {
            case 1: //cannot check col-2
                if(previousStateMatrix[col,rowMinusTwo]) neighbours+=1;
                if(previousStateMatrix[col,rowPlusTwo]) neighbours+=1;

                if(previousStateMatrix[columnPlusTwo,row]) neighbours+=1;

                if(columnMinusOne>=0) if(previousStateMatrix[columnMinusOne,rowMinusTwo]) neighbours+=1;
                if(columnMinusOne>=0) if(previousStateMatrix[columnMinusOne,rowPlusTwo]) neighbours+=1;
                if(previousStateMatrix[columnPlusOne,rowMinusTwo]) neighbours+=1;
                if(previousStateMatrix[columnPlusTwo,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[columnPlusOne,rowPlusTwo]) neighbours+=1;
                if(previousStateMatrix[columnPlusTwo,rowPlusOne]) neighbours+=1;

                return neighbours;

            case 2: //cannot check col+2
                if(previousStateMatrix[columnMinusTwo,row]) neighbours+=1;

                if(previousStateMatrix[col,rowMinusTwo]) neighbours+=1;
                if(previousStateMatrix[col,rowPlusTwo]) neighbours+=1;

                if(previousStateMatrix[columnMinusOne,rowMinusTwo]) neighbours+=1;
                if(previousStateMatrix[columnMinusTwo,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[columnMinusOne,rowPlusTwo]) neighbours+=1;
                if(previousStateMatrix[columnMinusTwo,rowPlusOne]) neighbours+=1;
                if(columnPlusOne < width) if(previousStateMatrix[columnPlusOne,rowMinusTwo]) neighbours+=1;
                if(columnPlusOne < width) if(previousStateMatrix[columnPlusOne,rowPlusTwo]) neighbours+=1;
                return neighbours;

            case 10: //cannot check row-2
                if(previousStateMatrix[columnMinusTwo,row]) neighbours+=1;

                if(previousStateMatrix[col,rowPlusTwo]) neighbours+=1;

                if(previousStateMatrix[columnPlusTwo,row]) neighbours+=1;

                if(rowMinusOne >= 0) if(previousStateMatrix[columnMinusTwo,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[columnMinusOne,rowPlusTwo]) neighbours+=1;
                if(previousStateMatrix[columnMinusTwo,rowPlusOne]) neighbours+=1;
                if(rowMinusOne >= 0) if(previousStateMatrix[columnPlusTwo,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[columnPlusOne,rowPlusTwo]) neighbours+=1;
                if(previousStateMatrix[columnPlusTwo,rowPlusOne]) neighbours+=1;
                return neighbours;

            case 20: //cannot check row+2
                if(previousStateMatrix[columnMinusTwo,row]) neighbours+=1;

                if(previousStateMatrix[col,rowMinusTwo]) neighbours+=1;

                if(previousStateMatrix[columnPlusTwo,row]) neighbours+=1;

                if(previousStateMatrix[columnMinusOne,rowMinusTwo]) neighbours+=1;
                if(previousStateMatrix[columnMinusTwo,rowMinusOne]) neighbours+=1;
                if(rowPlusOne < height) if(previousStateMatrix[columnMinusTwo,rowPlusOne]) neighbours+=1;
                if(previousStateMatrix[columnPlusOne,rowMinusTwo]) neighbours+=1;
                if(previousStateMatrix[columnPlusTwo,rowMinusOne]) neighbours+=1;
                if(rowPlusOne < height) if(previousStateMatrix[columnPlusTwo,rowPlusOne]) neighbours+=1;
                return neighbours;

            case 11: //cannot check col-2 or row-2
                if(previousStateMatrix[col,rowPlusTwo]) neighbours+=1;
                
                if(previousStateMatrix[columnPlusTwo,row]) neighbours+=1;

                if(columnMinusOne >= 0) if(previousStateMatrix[columnMinusOne,rowPlusTwo]) neighbours+=1;
                if(rowMinusOne >= 0) if(previousStateMatrix[columnPlusTwo,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[columnPlusOne,rowPlusTwo]) neighbours+=1;
                if(previousStateMatrix[columnPlusTwo,rowPlusOne]) neighbours+=1;
                return neighbours;

            case 12: //cannot check col+2 or row-2
                if(previousStateMatrix[columnMinusTwo,row]) neighbours+=1;

                
                if(previousStateMatrix[col,rowPlusTwo]) neighbours+=1;

                if(rowMinusOne >= 0) if(previousStateMatrix[columnMinusTwo,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[columnMinusOne,rowPlusTwo]) neighbours+=1;
                if(previousStateMatrix[columnMinusTwo,rowPlusOne]) neighbours+=1;
                if(columnPlusOne < width) if(previousStateMatrix[columnPlusOne,rowPlusTwo]) neighbours+=1;
                return neighbours;

            case 21: //cannot check col-2 or row+2
                
                if(previousStateMatrix[col,rowMinusTwo]) neighbours+=1;

                if(previousStateMatrix[columnPlusTwo,row]) neighbours+=1;

                if(columnMinusOne >= 0) if(previousStateMatrix[columnMinusOne,rowMinusTwo]) neighbours+=1;
                if(previousStateMatrix[columnPlusOne,rowMinusTwo]) neighbours+=1;
                if(previousStateMatrix[columnPlusTwo,rowMinusOne]) neighbours+=1;
                if(rowPlusOne < height) if(previousStateMatrix[columnPlusTwo,rowPlusOne]) neighbours+=1;
                return neighbours;

            case 22: //cannot check col+2 or row+2
                if(previousStateMatrix[columnMinusTwo,row]) neighbours+=1;
                
                if(previousStateMatrix[col,rowMinusTwo]) neighbours+=1;

                if(previousStateMatrix[columnMinusOne,rowMinusTwo]) neighbours+=1;
                if(previousStateMatrix[columnMinusTwo,rowMinusOne]) neighbours+=1;
                if(rowPlusOne < height) if(previousStateMatrix[columnMinusTwo,rowPlusOne]) neighbours+=1;
                if(columnPlusOne < width) if(previousStateMatrix[columnPlusOne,rowMinusTwo]) neighbours+=1;
                return neighbours;

            default: //check all neighbours
                if(previousStateMatrix[columnMinusTwo,row]) neighbours+=1;

                
                if(previousStateMatrix[col,rowMinusTwo]) neighbours+=1;
                if(previousStateMatrix[col,rowPlusTwo]) neighbours+=1;

                if(previousStateMatrix[columnPlusTwo,row]) neighbours+=1;

                if(previousStateMatrix[columnMinusOne,rowMinusTwo]) neighbours+=1;
                if(previousStateMatrix[columnMinusTwo,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[columnMinusOne,rowPlusTwo]) neighbours+=1;
                if(previousStateMatrix[columnMinusTwo,rowPlusOne]) neighbours+=1;
                if(previousStateMatrix[columnPlusOne,rowMinusTwo]) neighbours+=1;
                if(previousStateMatrix[columnPlusTwo,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[columnPlusOne,rowPlusTwo]) neighbours+=1;
                if(previousStateMatrix[columnPlusTwo,rowPlusOne]) neighbours+=1;
                return neighbours;

        }
    }

    public int calcTwentyFourNeighbours(int col, int row)
    {
        int neighbours = 0;

        int columnMinusTwo = col-2;
        int columnPlusTwo = col+2;

        int rowMinusTwo = row-2;
        int rowPlusTwo = row+2;

        int columnMinusOne = col-1;
        int columnPlusOne = col+1;

        int rowMinusOne = row-1;
        int rowPlusOne = row+1;



        int hashValue = 0;

        //behind column limit (ignore check 2)
        if(columnMinusTwo < 0) hashValue = 1; 
        else if(columnPlusTwo >= width) hashValue = 2;
        //beyond column limit
        

        //behind row limit (ignore check 2)
        if(rowMinusTwo < 0) hashValue += 10;
        else if(rowPlusTwo >= height) hashValue += 20;
        //beyond row limit
       



        switch (hashValue)
        {
            case 1: //cannot check col-2
                if(previousStateMatrix[col,rowMinusTwo]) neighbours+=1;
                if(previousStateMatrix[col,rowPlusTwo]) neighbours+=1;

                if(previousStateMatrix[columnPlusTwo,row]) neighbours+=1;
                if(previousStateMatrix[columnPlusTwo,rowMinusTwo]) neighbours+=1;
                if(previousStateMatrix[columnPlusTwo,rowPlusTwo]) neighbours+=1;

                if(columnMinusOne>=0) if(previousStateMatrix[columnMinusOne,rowMinusTwo]) neighbours+=1;
                if(columnMinusOne>=0) if(previousStateMatrix[columnMinusOne,rowPlusTwo]) neighbours+=1;
                if(previousStateMatrix[columnPlusOne,rowMinusTwo]) neighbours+=1;
                if(previousStateMatrix[columnPlusTwo,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[columnPlusOne,rowPlusTwo]) neighbours+=1;
                if(previousStateMatrix[columnPlusTwo,rowPlusOne]) neighbours+=1;

                return neighbours;

            case 2: //cannot check col+2
                if(previousStateMatrix[columnMinusTwo,row]) neighbours+=1;
                if(previousStateMatrix[columnMinusTwo,rowMinusTwo]) neighbours+=1;
                if(previousStateMatrix[columnMinusTwo,rowPlusTwo]) neighbours+=1;

                if(previousStateMatrix[col,rowMinusTwo]) neighbours+=1;
                if(previousStateMatrix[col,rowPlusTwo]) neighbours+=1;

                if(previousStateMatrix[columnMinusOne,rowMinusTwo]) neighbours+=1;
                if(previousStateMatrix[columnMinusTwo,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[columnMinusOne,rowPlusTwo]) neighbours+=1;
                if(previousStateMatrix[columnMinusTwo,rowPlusOne]) neighbours+=1;
                if(columnPlusOne < width) if(previousStateMatrix[columnPlusOne,rowMinusTwo]) neighbours+=1;
                if(columnPlusOne < width) if(previousStateMatrix[columnPlusOne,rowPlusTwo]) neighbours+=1;
                return neighbours;

            case 10: //cannot check row-2
                if(previousStateMatrix[columnMinusTwo,row]) neighbours+=1;
                if(previousStateMatrix[columnMinusTwo,rowPlusTwo]) neighbours+=1;

                if(previousStateMatrix[col,rowPlusTwo]) neighbours+=1;

                if(previousStateMatrix[columnPlusTwo,row]) neighbours+=1;
                if(previousStateMatrix[columnPlusTwo,rowPlusTwo]) neighbours+=1;

                if(rowMinusOne >= 0) if(previousStateMatrix[columnMinusTwo,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[columnMinusOne,rowPlusTwo]) neighbours+=1;
                if(previousStateMatrix[columnMinusTwo,rowPlusOne]) neighbours+=1;
                if(rowMinusOne >= 0) if(previousStateMatrix[columnPlusTwo,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[columnPlusOne,rowPlusTwo]) neighbours+=1;
                if(previousStateMatrix[columnPlusTwo,rowPlusOne]) neighbours+=1;
                return neighbours;

            case 20: //cannot check row+2
                if(previousStateMatrix[columnMinusTwo,row]) neighbours+=1;
                if(previousStateMatrix[columnMinusTwo,rowMinusTwo]) neighbours+=1;

                if(previousStateMatrix[col,rowMinusTwo]) neighbours+=1;

                if(previousStateMatrix[columnPlusTwo,row]) neighbours+=1;
                if(previousStateMatrix[columnPlusTwo,rowMinusTwo]) neighbours+=1;

                if(previousStateMatrix[columnMinusOne,rowMinusTwo]) neighbours+=1;
                if(previousStateMatrix[columnMinusTwo,rowMinusOne]) neighbours+=1;
                if(rowPlusOne < height) if(previousStateMatrix[columnMinusTwo,rowPlusOne]) neighbours+=1;
                if(previousStateMatrix[columnPlusOne,rowMinusTwo]) neighbours+=1;
                if(previousStateMatrix[columnPlusTwo,rowMinusOne]) neighbours+=1;
                if(rowPlusOne < height) if(previousStateMatrix[columnPlusTwo,rowPlusOne]) neighbours+=1;
                return neighbours;

            case 11: //cannot check col-2 or row-2
                if(previousStateMatrix[col,rowPlusTwo]) neighbours+=1;
                
                if(previousStateMatrix[columnPlusTwo,row]) neighbours+=1;
                if(previousStateMatrix[columnPlusTwo,rowPlusTwo]) neighbours+=1;

                if(columnMinusOne >= 0) if(previousStateMatrix[columnMinusOne,rowPlusTwo]) neighbours+=1;
                if(rowMinusOne >= 0) if(previousStateMatrix[columnPlusTwo,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[columnPlusOne,rowPlusTwo]) neighbours+=1;
                if(previousStateMatrix[columnPlusTwo,rowPlusOne]) neighbours+=1;
                return neighbours;

            case 12: //cannot check col+2 or row-2
                if(previousStateMatrix[columnMinusTwo,row]) neighbours+=1;
                if(previousStateMatrix[columnMinusTwo,rowPlusTwo]) neighbours+=1;

                
                if(previousStateMatrix[col,rowPlusTwo]) neighbours+=1;

                if(rowMinusOne >= 0) if(previousStateMatrix[columnMinusTwo,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[columnMinusOne,rowPlusTwo]) neighbours+=1;
                if(previousStateMatrix[columnMinusTwo,rowPlusOne]) neighbours+=1;
                if(columnPlusOne < width) if(previousStateMatrix[columnPlusOne,rowPlusTwo]) neighbours+=1;
                return neighbours;

            case 21: //cannot check col-2 or row+2
                
                if(previousStateMatrix[col,rowMinusTwo]) neighbours+=1;

                if(previousStateMatrix[columnPlusTwo,rowMinusTwo]) neighbours+=1;
                if(previousStateMatrix[columnPlusTwo,row]) neighbours+=1;

                if(columnMinusOne >= 0) if(previousStateMatrix[columnMinusOne,rowMinusTwo]) neighbours+=1;
                if(previousStateMatrix[columnPlusOne,rowMinusTwo]) neighbours+=1;
                if(previousStateMatrix[columnPlusTwo,rowMinusOne]) neighbours+=1;
                if(rowPlusOne < height) if(previousStateMatrix[columnPlusTwo,rowPlusOne]) neighbours+=1;
                return neighbours;

            case 22: //cannot check col+2 or row+2
                if(previousStateMatrix[columnMinusTwo,rowMinusTwo]) neighbours+=1;
                if(previousStateMatrix[columnMinusTwo,row]) neighbours+=1;
                
                if(previousStateMatrix[col,rowMinusTwo]) neighbours+=1;

                if(previousStateMatrix[columnMinusOne,rowMinusTwo]) neighbours+=1;
                if(previousStateMatrix[columnMinusTwo,rowMinusOne]) neighbours+=1;
                if(rowPlusOne < height) if(previousStateMatrix[columnMinusTwo,rowPlusOne]) neighbours+=1;
                if(columnPlusOne < width) if(previousStateMatrix[columnPlusOne,rowMinusTwo]) neighbours+=1;
                return neighbours;

            default: //check all neighbours
                if(previousStateMatrix[columnMinusTwo,rowMinusTwo]) neighbours+=1;
                if(previousStateMatrix[columnMinusTwo,row]) neighbours+=1;
                if(previousStateMatrix[columnMinusTwo,rowPlusTwo]) neighbours+=1;

                
                if(previousStateMatrix[col,rowMinusTwo]) neighbours+=1;
                if(previousStateMatrix[col,rowPlusTwo]) neighbours+=1;

                if(previousStateMatrix[columnPlusTwo,rowMinusTwo]) neighbours+=1;
                if(previousStateMatrix[columnPlusTwo,row]) neighbours+=1;
                if(previousStateMatrix[columnPlusTwo,rowPlusTwo]) neighbours+=1;

                if(previousStateMatrix[columnMinusOne,rowMinusTwo]) neighbours+=1;
                if(previousStateMatrix[columnMinusTwo,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[columnMinusOne,rowPlusTwo]) neighbours+=1;
                if(previousStateMatrix[columnMinusTwo,rowPlusOne]) neighbours+=1;
                if(previousStateMatrix[columnPlusOne,rowMinusTwo]) neighbours+=1;
                if(previousStateMatrix[columnPlusTwo,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[columnPlusOne,rowPlusTwo]) neighbours+=1;
                if(previousStateMatrix[columnPlusTwo,rowPlusOne]) neighbours+=1;
                return neighbours;

        }
    }

    public int calcThirtySixNeighbours(int col, int row)
    {
        int neighbours = 0;

        int columnMinusThree = col-3;
        int columnPlusThree = col+3;

        int rowMinusThree = row-3;
        int rowPlusThree = row+3;

        int columnMinusOne = col-1;
        int columnPlusOne = col+1;

        int rowMinusOne = row-1;
        int rowPlusOne = row+1;



        int hashValue = 0;

        //behind column limit (ignore check 2)
        if(columnMinusThree < 0) hashValue = 1; 
        else if(columnPlusThree >= width) hashValue = 2;
        //beyond column limit
        

        //behind row limit (ignore check 2)
        if(rowMinusThree < 0) hashValue += 10;
        else if(rowPlusThree >= height) hashValue += 20;
        //beyond row limit
       



        switch (hashValue)
        {
            case 1: //cannot check col-3
                if(previousStateMatrix[col,rowMinusThree]) neighbours+=1;
                if(previousStateMatrix[col,rowPlusThree]) neighbours+=1;

                if(previousStateMatrix[columnPlusThree,row]) neighbours+=1;

                if(columnMinusOne>=0) if(previousStateMatrix[columnMinusOne,rowMinusThree]) neighbours+=1;
                if(columnMinusOne>=0) if(previousStateMatrix[columnMinusOne,rowPlusThree]) neighbours+=1;
                if(previousStateMatrix[columnPlusOne,rowMinusThree]) neighbours+=1;
                if(previousStateMatrix[columnPlusThree,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[columnPlusOne,rowPlusThree]) neighbours+=1;
                if(previousStateMatrix[columnPlusThree,rowPlusOne]) neighbours+=1;

                return neighbours;

            case 2: //cannot check col+3
                if(previousStateMatrix[columnMinusThree,row]) neighbours+=1;

                if(previousStateMatrix[col,rowMinusThree]) neighbours+=1;
                if(previousStateMatrix[col,rowPlusThree]) neighbours+=1;

                if(previousStateMatrix[columnMinusOne,rowMinusThree]) neighbours+=1;
                if(previousStateMatrix[columnMinusThree,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[columnMinusOne,rowPlusThree]) neighbours+=1;
                if(previousStateMatrix[columnMinusThree,rowPlusOne]) neighbours+=1;
                if(columnPlusOne < width) if(previousStateMatrix[columnPlusOne,rowMinusThree]) neighbours+=1;
                if(columnPlusOne < width) if(previousStateMatrix[columnPlusOne,rowPlusThree]) neighbours+=1;
                return neighbours;

            case 10: //cannot check row-3
                if(previousStateMatrix[columnMinusThree,row]) neighbours+=1;

                if(previousStateMatrix[col,rowPlusThree]) neighbours+=1;

                if(previousStateMatrix[columnPlusThree,row]) neighbours+=1;

                if(rowMinusOne >= 0) if(previousStateMatrix[columnMinusThree,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[columnMinusOne,rowPlusThree]) neighbours+=1;
                if(previousStateMatrix[columnMinusThree,rowPlusOne]) neighbours+=1;
                if(rowMinusOne >= 0) if(previousStateMatrix[columnPlusThree,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[columnPlusOne,rowPlusThree]) neighbours+=1;
                if(previousStateMatrix[columnPlusThree,rowPlusOne]) neighbours+=1;
                return neighbours;

            case 20: //cannot check row+3
                if(previousStateMatrix[columnMinusThree,row]) neighbours+=1;

                if(previousStateMatrix[col,rowMinusThree]) neighbours+=1;

                if(previousStateMatrix[columnPlusThree,row]) neighbours+=1;

                if(previousStateMatrix[columnMinusOne,rowMinusThree]) neighbours+=1;
                if(previousStateMatrix[columnMinusThree,rowMinusOne]) neighbours+=1;
                if(rowPlusOne < height) if(previousStateMatrix[columnMinusThree,rowPlusOne]) neighbours+=1;
                if(previousStateMatrix[columnPlusOne,rowMinusThree]) neighbours+=1;
                if(previousStateMatrix[columnPlusThree,rowMinusOne]) neighbours+=1;
                if(rowPlusOne < height) if(previousStateMatrix[columnPlusThree,rowPlusOne]) neighbours+=1;
                return neighbours;

            case 11: //cannot check col-3 or row-3
                if(previousStateMatrix[col,rowPlusThree]) neighbours+=1;
                
                if(previousStateMatrix[columnPlusThree,row]) neighbours+=1;

                if(columnMinusOne >= 0) if(previousStateMatrix[columnMinusOne,rowPlusThree]) neighbours+=1;
                if(rowMinusOne >= 0) if(previousStateMatrix[columnPlusThree,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[columnPlusOne,rowPlusThree]) neighbours+=1;
                if(previousStateMatrix[columnPlusThree,rowPlusOne]) neighbours+=1;
                return neighbours;

            case 12: //cannot check col+3 or row-3
                if(previousStateMatrix[columnMinusThree,row]) neighbours+=1;

                
                if(previousStateMatrix[col,rowPlusThree]) neighbours+=1;

                if(rowMinusOne >= 0) if(previousStateMatrix[columnMinusThree,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[columnMinusOne,rowPlusThree]) neighbours+=1;
                if(previousStateMatrix[columnMinusThree,rowPlusOne]) neighbours+=1;
                if(columnPlusOne < width) if(previousStateMatrix[columnPlusOne,rowPlusThree]) neighbours+=1;
                return neighbours;

            case 21: //cannot check col-3 or row+3
                
                if(previousStateMatrix[col,rowMinusThree]) neighbours+=1;

                if(previousStateMatrix[columnPlusThree,row]) neighbours+=1;

                if(columnMinusOne >= 0) if(previousStateMatrix[columnMinusOne,rowMinusThree]) neighbours+=1;
                if(previousStateMatrix[columnPlusOne,rowMinusThree]) neighbours+=1;
                if(previousStateMatrix[columnPlusThree,rowMinusOne]) neighbours+=1;
                if(rowPlusOne < height) if(previousStateMatrix[columnPlusThree,rowPlusOne]) neighbours+=1;
                return neighbours;

            case 22: //cannot check col+3 or row+3
                if(previousStateMatrix[columnMinusThree,row]) neighbours+=1;
                
                if(previousStateMatrix[col,rowMinusThree]) neighbours+=1;

                if(previousStateMatrix[columnMinusOne,rowMinusThree]) neighbours+=1;
                if(previousStateMatrix[columnMinusThree,rowMinusOne]) neighbours+=1;
                if(rowPlusOne < height) if(previousStateMatrix[columnMinusThree,rowPlusOne]) neighbours+=1;
                if(columnPlusOne < width) if(previousStateMatrix[columnPlusOne,rowMinusThree]) neighbours+=1;
                return neighbours;

            default: //check all neighbours
                if(previousStateMatrix[columnMinusThree,row]) neighbours+=1;

                
                if(previousStateMatrix[col,rowMinusThree]) neighbours+=1;
                if(previousStateMatrix[col,rowPlusThree]) neighbours+=1;

                if(previousStateMatrix[columnPlusThree,row]) neighbours+=1;

                if(previousStateMatrix[columnMinusOne,rowMinusThree]) neighbours+=1;
                if(previousStateMatrix[columnMinusThree,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[columnMinusOne,rowPlusThree]) neighbours+=1;
                if(previousStateMatrix[columnMinusThree,rowPlusOne]) neighbours+=1;
                if(previousStateMatrix[columnPlusOne,rowMinusThree]) neighbours+=1;
                if(previousStateMatrix[columnPlusThree,rowMinusOne]) neighbours+=1;
                if(previousStateMatrix[columnPlusOne,rowPlusThree]) neighbours+=1;
                if(previousStateMatrix[columnPlusThree,rowPlusOne]) neighbours+=1;
                return neighbours;

        }
    }
    

    public void convertSlot(int col, int row, bool input)
    {
        currentStateMatrix[col,row] = input;
    }



    /*switch case based on # of neighbours
    1) return false
    2) if(true) return true
    3) return true
    4-8) return false
    */
}
