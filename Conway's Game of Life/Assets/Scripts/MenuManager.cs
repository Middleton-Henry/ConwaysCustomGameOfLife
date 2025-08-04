using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    private Camera cam;
    public Color deadColor = new Color(0f,0f,0f);
    public Color aliveColor = new Color(1f,1f,1f);
    public Color outlineColor = new Color(0f,0f,1f);
    public Color additionalColor = new Color(1f,0f,0f);

    public List<GameObject> additionalColorObjects = new List<GameObject>();
    public List<GameObject> aliveColorObjects = new List<GameObject>();

    public List<GameObject> outlineColorObjects = new List<GameObject>();
    public List<GameObject> deadColorObjects = new List<GameObject>();

    public List<GameObject> buttonList = new List<GameObject>();
    public List<GameObject> manualInputList = new List<GameObject>();
    public List<GameObject> dropdownList = new List<GameObject>();
    public List<GameObject> toggleList = new List<GameObject>();

    public List<GameObject> RulesTextList = new List<GameObject>();
    public List<GameObject> RulesDropdownList = new List<GameObject>();
    public List<GameObject> RulesToggleList = new List<GameObject>();

    [SerializeField] private TMP_Dropdown checkNeighboursDropdown;
    [SerializeField] private Slider randomValueSlider;
    [SerializeField] private TMP_InputField randPercentInput;
    [SerializeField] private TMP_InputField gridLengthInput;
    [SerializeField] private Slider updateSpeed;
    [SerializeField] private Toggle automaticUpdateToggle;
    [SerializeField] private Image updateToggleCheckmark;
    [SerializeField] private TMP_Text hertzText;
    [SerializeField] public Slider outlineSize;
    [SerializeField] private TMP_Text generationCounterText;
    [SerializeField] private TMP_Text populationCounterText;
    [SerializeField] private TMP_InputField saveLoadInput;

    [SerializeField] private TMP_InputField startGenerationExporter;
    [SerializeField] private TMP_InputField finalGenerationExporter;

    [SerializeField] private RawImage imageVisualizer;

    public List<TMP_InputField> livingRGBInputs = new List<TMP_InputField>();
    public List<TMP_InputField> deadRGBInputs = new List<TMP_InputField>();
    public List<TMP_InputField> outlineRGBInputs = new List<TMP_InputField>();
    public List<TMP_InputField> additionalRGBInputs = new List<TMP_InputField>();

    private int hertzSpeed = 0;
    private bool maxHertz = false;
    private float timer=0f;

    public Texture2D[] exportArray;

    /*
    #if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void downloadToFile(string content, string filename);
    #endif
    */


    // Start is called before the first frame update
    void Start()
    {
        for(int rulesTextIndex = 0; rulesTextIndex < RulesTextList.Count; rulesTextIndex++)
        {
            RulesDropdownList.Add(RulesTextList[rulesTextIndex].transform.GetChild(0).GetChild(0).gameObject);
        }

        for(int rulesTextIndex = 0; rulesTextIndex < RulesTextList.Count; rulesTextIndex++)
        {
            RulesToggleList.Add(RulesTextList[rulesTextIndex].transform.GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0).gameObject);
        }


        cam = Camera.main;
        
        randomValueSlider.onValueChanged.AddListener((v) =>
        {
            randPercentInput.text = (v.ToString("0.000"));

            gameObject.GetComponent<GameManager>().randomPercentage = v;
        });

        updateSpeed.onValueChanged.AddListener((v) =>
        {
            
            if(v!=40f) 
            {
                hertzSpeed = (int)v;
                maxHertz = false;
            }
            if(v==40f)
            {
                hertzSpeed = (int)(1f/Time.deltaTime);
                maxHertz = true;
            }
             
            hertzText.text = (hertzSpeed.ToString("000"));

            gameObject.GetComponent<GameManager>().automaticUpdateSpeed = v;
        });

        automaticUpdateToggle.onValueChanged.AddListener((v) =>
        {
            if(!v) updateToggleCheckmark.enabled=false;
            if(v) 
            {
                updateToggleCheckmark.enabled=true;
                updateToggleCheckmark.color = aliveColor;
            }
            gameObject.GetComponent<GameManager>().automaticUpdateBool = v;
        });

        randPercentInput.onValueChanged.AddListener((v) =>
        {
            float inputNumber = float.Parse(randPercentInput.text);

            if(inputNumber>1f) inputNumber=1f;
            if(inputNumber<0f) inputNumber=0f;
            randPercentInput.text = (inputNumber.ToString("0.000"));

            randomValueSlider.value = inputNumber;
            gameObject.GetComponent<GameManager>().randomPercentage = inputNumber;
        });

        checkNeighboursDropdown.onValueChanged.AddListener((v) =>
        {
            setRulesColorsAndEnabled();
        });

        outlineSize.onValueChanged.AddListener((v) =>
        {
    
            gameObject.GetComponent<GridManager>().resizeGridOutline(v);
    

        });

        checkNeighboursRulesStatesList();

        checkRGBInputUpdate();

        setDefaultColors();

        setDefaultSettings();
    }



    public void parseStringLoad()
    {
        string[] LoadKey = saveLoadInput.text.Split('_');
        for(int i = 0; i < LoadKey.Length; i++)
        {
            Debug.Log(i+": "+LoadKey[i]);
        }

        int checkNeigboursIndex = int.Parse(LoadKey[0]);

        int[] neighbourRulesIndexArray = new int[37];
        for(int i = 0; i < 37; i++)
        {
            neighbourRulesIndexArray[i] = int.Parse(LoadKey[i+1]);
        }

        int gridWidth = int.Parse(LoadKey[38]);

        int slotInputCount = ((LoadKey.Length - 39)/2);
        int[] columns = new int[slotInputCount];
        int[] rows = new int[slotInputCount];
        int indexCounter = 0;
        for(int i = 39; i < LoadKey.Length; i=i+2)
        {
            columns[indexCounter]=int.Parse(LoadKey[i]);
            rows[indexCounter]=int.Parse(LoadKey[i+1]);
            indexCounter++;
        }

        
        //int checkNeigboursIndex, int[] neighbourRulesIndexArray, int gridWidth, int[] columns, int[] rows
        setCustomSettingAndGrid(checkNeigboursIndex, neighbourRulesIndexArray, gridWidth, columns, rows);
    }

    public void parseStringSave()
    {
        string saveKey="";
        
        //neighbours checked dropdown
        saveKey+=(""+checkNeighboursDropdown.value);

        saveKey+="_";

        //neighbours rules

        for(int i = 0; i < 37; i++)
        {
            saveKey+=(RulesDropdownList[i].GetComponent<TMP_Dropdown>().value+"_");
        }

        //grid width

        saveKey+=gameObject.GetComponent<GameManager>().width;


        //input grid values
        int widthIndex = gameObject.GetComponent<GameManager>().width;

        for(int col = 0; col < widthIndex; col++)
        {
            for(int row = 0; row < widthIndex; row++)
            {
                if(gameObject.GetComponent<GameManager>().currentStateMatrix[col,row]) saveKey+=("_"+col+"_"+row);
            }
        }



        saveLoadInput.text = saveKey;
    }

    public void setCustomSettingAndGrid(int checkNeigboursIndex, int[] neighbourRulesIndexArray, int gridWidth, int[] columns, int[] rows)
    {
        


        //neighbours checked dropdown
        checkNeighboursDropdown.value = checkNeigboursIndex;
        setRulesColorsAndEnabled();

        //neighbours rules
        for(int i = 0; i < 37; i++)
        {
            RulesDropdownList[i].GetComponent<TMP_Dropdown>().value = neighbourRulesIndexArray[i];
        }

        //set grid
        gridLengthInput.text = (""+gridWidth);
        rebuildGrid();
        gameObject.GetComponent<GameManager>().clearInitialMatrix();
        gameObject.GetComponent<GameManager>().resetMatrix();

        //input grid values
        gameObject.GetComponent<GameManager>().clearInitialMatrix();

        for(int i = 0; i < columns.Length; i++)
        {
            gameObject.GetComponent<GameManager>().initialStateMatrix[columns[i],rows[i]]=true;
        }

        gameObject.GetComponent<GameManager>().resetMatrix();

    }

    public void resetMatrixButton()
    {
        gameObject.GetComponent<GameManager>().resetMatrix();
    }

    public void updateGenerationCounter(int currentGeneration)
    {
        generationCounterText.text = (currentGeneration.ToString("0000"));
    }

    public void updatePopulationCounter(int currentPopulation)
    {
        populationCounterText.text = (currentPopulation.ToString("0000"));
    }

    public void incrementPopulationCounter(int changeValue)
    {
        int newPopulation = int.Parse(populationCounterText.text) + changeValue;
        populationCounterText.text = (newPopulation.ToString("0000"));
    }

    

    void Update()
    {
        timer += Time.deltaTime;

        if(timer >= .25f)
        {
            updateHertzDisplay();
            timer = 0f;
        }


        if (Input.GetKeyDown(KeyCode.R))
        {
            gameObject.GetComponent<GameManager>().clearGridValues();
            updateGenerationCounter(1);
            updatePopulationCounter(0);
        }

        
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            updateStateButton();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            resetMatrixButton();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            parseStringSave();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            parseStringLoad();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            distributeRandomValuesButtonPressed();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            if(automaticUpdateToggle.isOn==false) automaticUpdateToggle.isOn=true;
            else automaticUpdateToggle.isOn=false;
        }
    }
    
    public void updateHertzDisplay()
    {
        if(maxHertz)
        {
            hertzSpeed = (int)(1f/Time.deltaTime);
            hertzText.text = (hertzSpeed.ToString("000"));
        }
    }

    public void checkNeighboursRulesStatesList()
    {
        foreach (GameObject rulesDropdown in RulesDropdownList)
        {
            rulesDropdown.GetComponent<TMP_Dropdown>().onValueChanged.AddListener((v) =>
            {
                gameObject.GetComponent<GameManager>().changeNeighbourRuleState(RulesDropdownList.IndexOf(rulesDropdown), rulesDropdown.GetComponent<TMP_Dropdown>().value);
                
            });

        }
    }

    public void updateRGBInputValues()
    {

        int[] livingRGB = new int[3];
        int[] deadRGB = new int[3];
        int[] outlineRGB = new int[3];
        int[] additionalRGB = new int[3];

        livingRGB = colorToRGB(aliveColor);
        deadRGB = colorToRGB(deadColor);
        outlineRGB = colorToRGB(outlineColor);
        additionalRGB = colorToRGB(additionalColor);


        foreach (TMP_InputField livingInputField in livingRGBInputs)
        {
            switch(livingRGBInputs.IndexOf(livingInputField))
            {
                case 0:
                livingInputField.text = (""+livingRGB[0]);
                break;

                case 1:
                livingInputField.text = (""+livingRGB[1]);
                break;

                case 2:
                livingInputField.text = (""+livingRGB[2]);
                break;
            }
        }

        foreach (TMP_InputField deadInputField in deadRGBInputs)
        {
            switch(deadRGBInputs.IndexOf(deadInputField))
            {
                case 0:
                deadInputField.text = (""+deadRGB[0]);
                break;

                case 1:
                deadInputField.text = (""+deadRGB[1]);
                break;

                case 2:
                deadInputField.text = (""+deadRGB[2]);
                break;
            }
        }

        foreach (TMP_InputField outlineInputField in outlineRGBInputs)
        {
            switch(outlineRGBInputs.IndexOf(outlineInputField))
            {
                case 0:
                outlineInputField.text = (""+outlineRGB[0]);
                break;

                case 1:
                outlineInputField.text = (""+outlineRGB[1]);
                break;

                case 2:
                outlineInputField.text = (""+outlineRGB[2]);
                break;
            }
        }

        foreach (TMP_InputField additionalInputField in additionalRGBInputs)
        {
            switch(additionalRGBInputs.IndexOf(additionalInputField))
            {
                case 0:
                additionalInputField.text = (""+additionalRGB[0]);
                break;

                case 1:
                additionalInputField.text = (""+additionalRGB[1]);
                break;

                case 2:
                additionalInputField.text = (""+additionalRGB[2]);
                break;
            }
        }
    }

    public void checkRGBInputUpdate()
    {
        int[] livingRGB = new int[3];
        int[] deadRGB = new int[3];
        int[] outlineRGB = new int[3];
        int[] additionalRGB = new int[3];

        int fieldInteger = 0;


        foreach (TMP_InputField livingInputField in livingRGBInputs)
        {            
                switch(livingRGBInputs.IndexOf(livingInputField))
                {
                    case 0:
                    if(livingInputField.text==null) fieldInteger = 0;
                    else fieldInteger = int.Parse(livingInputField.text);
                    if(fieldInteger<0) fieldInteger = 0;
                    if(fieldInteger>255) fieldInteger = 255;
                    livingInputField.text = (""+fieldInteger);
                    livingRGB[0] = fieldInteger;
                    break;

                    case 1:
                    if(livingInputField.text==null) fieldInteger = 0;
                    else fieldInteger = int.Parse(livingInputField.text);
                    if(fieldInteger<0) fieldInteger = 0;
                    if(fieldInteger>255) fieldInteger = 255;
                    livingInputField.text = (""+fieldInteger);
                    livingRGB[1] = fieldInteger;
                    break;

                    case 2:
                    if(livingInputField.text==null) fieldInteger = 0;
                    else fieldInteger = int.Parse(livingInputField.text);
                    if(fieldInteger<0) fieldInteger = 0;
                    if(fieldInteger>255) fieldInteger = 255;
                    livingInputField.text = (""+fieldInteger);
                    livingRGB[2] = fieldInteger;
                    break;
                }
                aliveColor = RGBToColor(livingRGB);
        }

        foreach (TMP_InputField deadInputField in deadRGBInputs)
        {            
                switch(deadRGBInputs.IndexOf(deadInputField))
                {
                    case 0:
                    if(deadInputField.text==null) fieldInteger = 0;
                    else fieldInteger = int.Parse(deadInputField.text);
                    if(fieldInteger<0) fieldInteger = 0;
                    if(fieldInteger>255) fieldInteger = 255;
                    deadInputField.text = (""+fieldInteger);
                    deadRGB[0] = fieldInteger;
                    break;

                    case 1:
                    if(deadInputField.text==null) fieldInteger = 0;
                    else fieldInteger = int.Parse(deadInputField.text);
                    if(fieldInteger<0) fieldInteger = 0;
                    if(fieldInteger>255) fieldInteger = 255;
                    deadInputField.text = (""+fieldInteger);
                    deadRGB[1] = fieldInteger;
                    break;

                    case 2:
                    if(deadInputField.text==null) fieldInteger = 0;
                    else fieldInteger = int.Parse(deadInputField.text);
                    if(fieldInteger<0) fieldInteger = 0;
                    if(fieldInteger>255) fieldInteger = 255;
                    deadInputField.text = (""+fieldInteger);
                    deadRGB[2] = fieldInteger;
                    break;
                }
                deadColor = RGBToColor(deadRGB);
        }

        foreach (TMP_InputField outlineFieldInput in outlineRGBInputs)
        {            
                switch(outlineRGBInputs.IndexOf(outlineFieldInput))
                {
                    case 0:
                    if(outlineFieldInput.text==null) fieldInteger = 0;
                    else fieldInteger = int.Parse(outlineFieldInput.text);
                    if(fieldInteger<0) fieldInteger = 0;
                    if(fieldInteger>255) fieldInteger = 255;
                    outlineFieldInput.text = (""+fieldInteger);
                    outlineRGB[0] = fieldInteger;
                    break;

                    case 1:
                    if(outlineFieldInput.text==null) fieldInteger = 0;
                    else fieldInteger = int.Parse(outlineFieldInput.text);
                    if(fieldInteger<0) fieldInteger = 0;
                    if(fieldInteger>255) fieldInteger = 255;
                    outlineFieldInput.text = (""+fieldInteger);
                    outlineRGB[1] = fieldInteger;
                    break;

                    case 2:
                    if(outlineFieldInput.text==null) fieldInteger = 0;
                    else fieldInteger = int.Parse(outlineFieldInput.text);
                    if(fieldInteger<0) fieldInteger = 0;
                    if(fieldInteger>255) fieldInteger = 255;
                    outlineFieldInput.text = (""+fieldInteger);
                    outlineRGB[2] = fieldInteger;
                    break;
                }
                outlineColor = RGBToColor(outlineRGB);
        }

        foreach (TMP_InputField additionalFieldInput in additionalRGBInputs)
        {            
                switch(additionalRGBInputs.IndexOf(additionalFieldInput))
                {
                    case 0:
                    if(additionalFieldInput.text==null) fieldInteger = 0;
                    else fieldInteger = int.Parse(additionalFieldInput.text);
                    if(fieldInteger<0) fieldInteger = 0;
                    if(fieldInteger>255) fieldInteger = 255;
                    additionalFieldInput.text = (""+fieldInteger);
                    additionalRGB[0] = fieldInteger;
                    break;

                    case 1:
                    if(additionalFieldInput.text==null) fieldInteger = 0;
                    else fieldInteger = int.Parse(additionalFieldInput.text);
                    if(fieldInteger<0) fieldInteger = 0;
                    if(fieldInteger>255) fieldInteger = 255;
                    additionalFieldInput.text = (""+fieldInteger);
                    additionalRGB[1] = fieldInteger;
                    break;

                    case 2:
                    if(additionalFieldInput.text==null) fieldInteger = 0;
                    else fieldInteger = int.Parse(additionalFieldInput.text);
                    if(fieldInteger<0) fieldInteger = 0;
                    if(fieldInteger>255) fieldInteger = 255;
                    additionalFieldInput.text = (""+fieldInteger);
                    additionalRGB[2] = fieldInteger;
                    break;
                }
                additionalColor = RGBToColor(additionalRGB);
        }

        setColors();
    }

    

    public void updateDropdowns()
    {
        int indexDropdowns = 0;
        foreach (GameObject rulesDropdown in RulesDropdownList)
        {
            switch(gameObject.GetComponent<GameManager>().stateRules[indexDropdowns].ToString())
            {
                case "alive":
                rulesDropdown.GetComponent<TMP_Dropdown>().value = 0;
                break;

                case "same":
                rulesDropdown.GetComponent<TMP_Dropdown>().value = 1;
                break;

                case "dead":
                rulesDropdown.GetComponent<TMP_Dropdown>().value = 2;
                break;
            }
            indexDropdowns++;
        }

        checkNeighboursDropdown.value = (int)gameObject.GetComponent<GameManager>().neighbourRangeSelection;
    }

    public void setDefaultSettings()
    {
        for(int i = 0; i < 37; i++)
        {
            if(i==2) 
            {
                RulesDropdownList[i].GetComponent<TMP_Dropdown>().value = 1;
            }
            else
            {
                if(i==3) 
                {
                    RulesDropdownList[i].GetComponent<TMP_Dropdown>().value = 0;
                }
                else
                {
                    RulesDropdownList[i].GetComponent<TMP_Dropdown>().value = 2;
                }
            }


            
        }

        


        gameObject.GetComponent<GameManager>().setDefaultSettings();
        updateDropdowns();
    }

    public void rebuildGrid()
    {
        
        int inputNumber = int.Parse(gridLengthInput.text);
        if(inputNumber<1) inputNumber=1;
        gridLengthInput.text = (""+inputNumber);

        gameObject.GetComponent<GameManager>().rebuildGrid(inputNumber);
    }

    public void distributeRandomValuesButtonPressed()
    {
        
        gameObject.GetComponent<GameManager>().distributeRandomValues();
    }
    
    public void updateStateButton()
    {
        gameObject.GetComponent<GameManager>().forwardOneState();
    }

    public void setDefaultColors()
    {

       defaultColors();

        setColors();
        
    }

    public void defaultColors()
    {
        aliveColor = new Color(1f,1f,1f);
        outlineColor = new Color(.35f,.35f,.35f);
        additionalColor = new Color(.55f,.55f,.55f);
        deadColor = new Color(0f,0f,0f);
    }

    

    public void setColors()
    {
        gameObject.GetComponent<GridManager>().dead = deadColor;
        gameObject.GetComponent<GridManager>().alive = aliveColor;
        cam.backgroundColor = outlineColor;

        foreach(GameObject additionalColorSelection in additionalColorObjects)
        {
            if(additionalColorSelection!=null) setCorrectColorComponent(additionalColor, additionalColorSelection);
        }

        foreach(GameObject aliveColorSelection in aliveColorObjects)
        {
            if(aliveColorSelection!=null) setCorrectColorComponent(aliveColor, aliveColorSelection);
        }

        foreach(GameObject outlineColorSelection in outlineColorObjects)
        {
            if(outlineColorSelection!=null) setCorrectColorComponent(outlineColor, outlineColorSelection);
        }

        foreach(GameObject deadobjects in deadColorObjects)
        {
            if(deadobjects!=null) setCorrectColorComponent(deadColor, deadobjects);
        }

        foreach(GameObject button in buttonList)
        {
            if(button!=null) setButtonColors(button);
        }

        foreach(GameObject manualInput in manualInputList)
        {
            if(manualInput!=null) setManualInputColors(manualInput);
        }

        foreach(GameObject dropdown in dropdownList)
        {
            if(dropdown!=null) setDropdownColors(dropdown);
        }

        foreach(GameObject toggle in toggleList)
        {
            if(toggle!=null) setToggleColors(toggle);
        }
        setToggleColors(automaticUpdateToggle);

        setRulesColorsAndEnabled();

        gameObject.GetComponent<GameManager>().replaceColorPalette();
        updateRGBInputValues();
        
    }

    public void setRulesColorsAndEnabled() //run whenever drop down is changed
    {
        int neighboursChecked=0;

        switch(checkNeighboursDropdown.value) //4,8,12,20,24,36
        {
            case 0: 
            neighboursChecked = 4;
            break;

            case 1:
            neighboursChecked = 8;
            break;

            case 2:
            neighboursChecked = 12;
            break;

            case 3:
            neighboursChecked = 20;
            break;

            case 4:
            neighboursChecked = 24;
            break;

            case 5:
            neighboursChecked = 36;
            break;
        }





        for(int rulesTextIndex = 0; rulesTextIndex < RulesTextList.Count; rulesTextIndex++)
        {
                
            GameObject toggleObject = RulesToggleList[rulesTextIndex];
                ColorBlock theColors = toggleObject.GetComponent<Toggle>().colors;
                theColors.normalColor = deadColor;
                theColors.highlightedColor = outlineColor;
                theColors.pressedColor = outlineColor;
                theColors.selectedColor = deadColor;
                toggleObject.GetComponent<Toggle>().colors = theColors;
                toggleObject.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().color = aliveColor;

                toggleObject.GetComponent<Toggle>().enabled = false;
                toggleObject.GetComponent<Toggle>().enabled = true;

            if(rulesTextIndex <= neighboursChecked)
            {
                RulesTextList[rulesTextIndex].GetComponent<TMP_Text>().color = aliveColor;
                RulesTextList[rulesTextIndex].transform.GetChild(0).gameObject.GetComponent<RawImage>().color = outlineColor;

                GameObject dropdown = RulesDropdownList[rulesTextIndex];
                theColors = dropdown.GetComponent<TMP_Dropdown>().colors;
                theColors.normalColor = deadColor;
                theColors.highlightedColor = outlineColor;
                theColors.pressedColor = outlineColor;
                theColors.selectedColor = deadColor;
                dropdown.GetComponent<TMP_Dropdown>().colors = theColors;
                dropdown.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().color = aliveColor;

                //dropdown.GetComponent<TMP_Dropdown>().enabled = false;
                dropdown.GetComponent<TMP_Dropdown>().interactable = true;   

                dropdown.transform.GetChild(2).gameObject.GetComponent<Image>().color = deadColor;
            }
            else
            {
                RulesTextList[rulesTextIndex].GetComponent<TMP_Text>().color = outlineColor;
                RulesTextList[rulesTextIndex].transform.GetChild(0).gameObject.GetComponent<RawImage>().color = deadColor;

                GameObject dropdown = RulesDropdownList[rulesTextIndex];
                theColors = dropdown.GetComponent<TMP_Dropdown>().colors;
                theColors.normalColor = additionalColor;
                theColors.highlightedColor = additionalColor;
                theColors.pressedColor = additionalColor;
                theColors.selectedColor = additionalColor;
                theColors.disabledColor = additionalColor;
                dropdown.GetComponent<TMP_Dropdown>().colors = theColors;
                dropdown.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().color = deadColor;

                dropdown.GetComponent<TMP_Dropdown>().interactable = false;
            }

            
        }
        
        
        
        //foreach(GameObject RulesText in RulesTextList)

        gameObject.GetComponent<GameManager>().setNeighbourRangeSelection(checkNeighboursDropdown.value);
    }

    public void setCorrectColorComponent(Color setColor, GameObject objectToSet)
    {
        if(objectToSet.GetComponent<RawImage>()!=null) 
        {
            objectToSet.GetComponent<RawImage>().color = setColor;
            return;
        }


        if(objectToSet.GetComponent<TMP_Text>()!=null) 
        {
            objectToSet.GetComponent<TMP_Text>().color = setColor;
            return;
        }

        if(objectToSet.GetComponent<Image>()!=null) 
        {
            objectToSet.GetComponent<Image>().color = setColor;
            return;
        }

        
    }

    public void setButtonColors(GameObject button)
    {
        ColorBlock theColors = button.GetComponent<Button>().colors;
        theColors.normalColor = deadColor;
        theColors.highlightedColor = outlineColor;
        theColors.pressedColor = outlineColor;
        theColors.selectedColor = deadColor;
        //button.GetComponent<Image>().color = outlineColor;
        button.GetComponent<Button>().colors = theColors;
        button.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().color = aliveColor;

        button.GetComponent<Button>().enabled = false;
        button.GetComponent<Button>().enabled = true;
    }

    public void setToggleColors(Toggle toggle)
    {
        ColorBlock theColors = toggle.colors;
        theColors.normalColor = deadColor;
        theColors.highlightedColor = outlineColor;
        theColors.pressedColor = outlineColor;
        theColors.selectedColor = deadColor;
        //button.GetComponent<Image>().color = outlineColor;
        toggle.colors = theColors;

        toggle.enabled = false;
        toggle.enabled = true;
    }

    public void setManualInputColors(GameObject manualInput)
    {
        ColorBlock theColors = manualInput.GetComponent<TMP_InputField>().colors;
        theColors.normalColor = deadColor;
        theColors.highlightedColor = outlineColor;
        theColors.pressedColor = outlineColor;
        theColors.selectedColor = deadColor;
        //button.GetComponent<Image>().color = outlineColor;    
        manualInput.GetComponent<TMP_InputField>().colors = theColors;
        manualInput.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>().color = aliveColor;

        manualInput.GetComponent<TMP_InputField>().enabled = false;
        manualInput.GetComponent<TMP_InputField>().enabled = true;
    }

    public void setDropdownColors(GameObject dropdown)
    {
        ColorBlock theColors = dropdown.GetComponent<TMP_Dropdown>().colors;
        theColors.normalColor = deadColor;
        theColors.highlightedColor = outlineColor;
        theColors.pressedColor = outlineColor;
        theColors.selectedColor = deadColor;
        //button.GetComponent<Image>().color = outlineColor;
        dropdown.GetComponent<TMP_Dropdown>().colors = theColors;
        dropdown.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().color = aliveColor;

        dropdown.GetComponent<TMP_Dropdown>().enabled = false;
        dropdown.GetComponent<TMP_Dropdown>().enabled = true;
    }

    public void setToggleColors(GameObject toggleObject)
    {
        ColorBlock theColors = toggleObject.GetComponent<Toggle>().colors;
        theColors.normalColor = deadColor;
        theColors.highlightedColor = outlineColor;
        theColors.pressedColor = outlineColor;
        theColors.selectedColor = deadColor;
        //button.GetComponent<Image>().color = outlineColor;
        toggleObject.GetComponent<Toggle>().colors = theColors;
        toggleObject.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().color = aliveColor;

        toggleObject.GetComponent<Toggle>().enabled = false;
        toggleObject.GetComponent<Toggle>().enabled = true;
    }

    public void swapToRandomPalette()
    {
        generateRandomColorPalette();
        setColors();
    }

    public void generateRandomColorPalette()
    {
        
        float[] HSV1 = new float[3];
        float[] HSV2 = new float[3];
        float[] HSV3 = new float[3];
        float[] HSV4 = new float[3];

        if(Random.Range(0f,1f)>.5f) 
        {
            float hueShift = Random.Range(0f,.25f);

            HSV1[0]=Random.Range(.50f,1f);
            HSV2[0]=HSV1[0] - hueShift;
            HSV3[0]=HSV2[0] - hueShift;
            HSV4[0]=HSV3[0] - hueShift;

            if(HSV1[0]<0f) HSV1[0] = 0f;
            if(HSV2[0]<0f) HSV2[0] = 0f;
            if(HSV3[0]<0f) HSV3[0] = 0f;
            if(HSV4[0]<0f) HSV4[0] = 0f;
        }
        else
        {
            float hueShift = Random.Range(0f,.25f);
            
            HSV1[0]=Random.Range(0f,.50f);
            HSV2[0]=HSV1[0] + hueShift;
            HSV3[0]=HSV2[0] + hueShift;
            HSV4[0]=HSV3[0] + hueShift;

            if(HSV1[0]>1f) HSV1[0] = 1f;
            if(HSV2[0]>1f) HSV2[0] = 1f;
            if(HSV3[0]>1f) HSV3[0] = 1f;
            if(HSV4[0]>1f) HSV4[0] = 1f;
        }

        if(Random.Range(0f,1f)>.5f) 
        {
            float saturationShift = Random.Range(0f,.3f);

            HSV1[1]=Random.Range(.50f,1f);
            HSV2[1]=HSV1[1] - saturationShift;
            HSV3[1]=HSV2[1] - saturationShift;
            HSV4[1]=HSV3[1] - saturationShift;

            if(HSV1[1]<0f) HSV1[1] = 0f;
            if(HSV2[1]<0f) HSV2[1] = 0f;
            if(HSV3[1]<0f) HSV3[1] = 0f;
            if(HSV4[1]<0f) HSV4[1] = 0f;
        }
        else
        {
            float saturationShift = Random.Range(0f,.3f);
            
            HSV1[1]=Random.Range(0f,.50f);
            HSV2[1]=HSV1[1] + saturationShift;
            HSV3[1]=HSV2[1] + saturationShift;
            HSV4[1]=HSV3[1] + saturationShift;

            if(HSV1[1]>1f) HSV1[1] = 1f;
            if(HSV2[1]>1f) HSV2[1] = 1f;
            if(HSV3[1]>1f) HSV3[1] = 1f;
            if(HSV4[1]>1f) HSV4[1] = 1f;
        }

        if(Random.Range(0f,1f)>.5f) 
        {
            float valueShift = Random.Range(.10f,.4f);

            HSV1[2]=Random.Range(.75f,1f);
            HSV2[2]=HSV1[2] - valueShift*.90f;
            HSV3[2]=HSV2[2] - valueShift*1.10f;
            HSV4[2]=HSV3[2] - valueShift;

            if(HSV1[2]<0f) HSV1[2] = 0f;
            if(HSV2[2]<0f) HSV2[2] = 0f;
            if(HSV3[2]<0f) HSV3[2] = 0f;
            if(HSV4[2]<0f) HSV4[2] = 0f;
        }
        else
        {
            float valueShift = Random.Range(.10f,.4f);
            
            HSV1[2]=Random.Range(0f,.25f);
            HSV2[2]=HSV1[2] + valueShift*.90f;
            HSV3[2]=HSV2[2] + valueShift*1.10f;
            HSV4[2]=HSV3[2] + valueShift;

            if(HSV1[2]>1f) HSV1[2] = 1f;
            if(HSV2[2]>1f) HSV2[2] = 1f;
            if(HSV3[2]>1f) HSV3[2] = 1f;
            if(HSV4[2]>1f) HSV4[2] = 1f;
        }


        aliveColor = Color.HSVToRGB(HSV1[0],HSV1[1],HSV1[2]);
        outlineColor = Color.HSVToRGB(HSV2[0],HSV2[1],HSV2[2]);
        additionalColor = Color.HSVToRGB(HSV3[0],HSV3[1],HSV3[2]);
        deadColor = Color.HSVToRGB(HSV4[0],HSV4[1],HSV4[2]);


        /*
            Chromaticity diagram reprset hue and saturation
            H,S,L hue saturation luminence
            Color.HSVToRGB   each hsv value is 0-1

            1) for color1 generate random HSV (make S and V lower)
            2) generate random value A and B
            3) for color2 S2 = S1 + A   V2 = V1 + B (hue remains if monochromatic)
            4) repeat for all colors using the same values of A and B
            4) convert all colors to RGB

            make deadColor darkest and aliveColor brightest
        */
    }

    

    public Color RGBToColor(int[] RGB)
    {
        for(int i = 0; i < 3; i++)
        {
            if(RGB[i]==null) RGB[i] = 0;
            if(RGB[i]<0) RGB[i]=0;
            if(RGB[i]>255) RGB[i]=255;
        }
        Color floatColor = new Color(RGB[0]/255f,RGB[1]/255f,RGB[2]/255f);
        return floatColor;

    }

    public int[] colorToRGB(Color colorValue)
    {
        int[] RGB = new int[3];

        RGB[0] = (int)(colorValue.r*255);
        RGB[1] = (int)(colorValue.g*255);
        RGB[2] = (int)(colorValue.b*255);

        return(RGB);
    }

    public void createGif()
    {
        int startGeneration = int.Parse(startGenerationExporter.text);
        int finalGeneration = int.Parse(finalGenerationExporter.text);

        if (finalGeneration < startGeneration) finalGeneration = startGeneration;
        exportArray = new Texture2D[finalGeneration-startGeneration+1];

        


        int width = gameObject.GetComponent<GameManager>().width;
        Texture2D tex = new Texture2D(width, width, TextureFormat.RGB24, false);
        //byte[] bytes = ImageConversion.EncodeToPNG(tex);
        //Object.Destroy(tex);
        Color[] pixelArray = tex.GetPixels();
        Color[] pixelArrayCopy = tex.GetPixels();


        if(startGeneration>gameObject.GetComponent<GameManager>().generation)
        {
            while(true)
            {
                updateStateButton();
                if(startGeneration==gameObject.GetComponent<GameManager>().generation)
                {
                    break;
                }
            }
        }
        else
        {
            if(startGeneration<gameObject.GetComponent<GameManager>().generation)
            {
                resetMatrixButton();
                while(true)
                {
                    if(startGeneration==gameObject.GetComponent<GameManager>().generation)
                    {
                        break;
                    }
                    updateStateButton();
                }
            }
        }


        for(int gen = startGeneration; gen < finalGeneration+1; gen++)
        {
            for(int col = 0; col < width; col++)
            {
                for(int row = 0; row < width; row++)
                {
                    if(gameObject.GetComponent<GameManager>().currentStateMatrix[col,row]) pixelArrayCopy[col+row*width] = aliveColor;//new Color(1f,1f,1f,1f);
                    if(!gameObject.GetComponent<GameManager>().currentStateMatrix[col,row]) pixelArrayCopy[col+row*width] = deadColor;//new Color(0f,0f,0f,0f);

                }
            }

            
            for (int i = 0; i < width; ++i) {
                for (int j = 0; j < width; ++j) {
                    pixelArray[i*width+j] = pixelArrayCopy[(width-i-1)*width+j];
                }
            }
            

            for(int h=0; h<width; h++)
            {
                for(int w=0; w<width; w++)
                {
                    tex.SetPixel(w, h, pixelArray[h*width + w]);
                }
            }


            tex.Apply();
            string dateFormat = "yyyy-MM-dd-HH-mm-ss";
            //string filename = width.ToString() + "x" + width.ToString() + "px_" + System.DateTime.Now.ToString(dateFormat);
            tex.filterMode = FilterMode.Point;
            byte[] texture = tex.EncodeToPNG();

            InitiateDownload(("gen_"+gameObject.GetComponent<GameManager>().generation), texture);

            if(finalGeneration != gameObject.GetComponent<GameManager>().generation)updateStateButton();
            exportArray[gen-startGeneration] = tex;
        }


        


        
        imageVisualizer.texture = tex;

    }


    

    static string scriptTemplate = @"
            var link = document.createElement(""a"");
            link.download = '{0}';
            link.href = 'data:application/octet-stream;charset=utf-8;base64,{1}';
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            delete link;
        ";

    public static void InitiateDownload(string aName, byte[] aData)
    {
        string base64 = System.Convert.ToBase64String(aData);
        string script = string.Format(scriptTemplate, aName, base64);
        Application.ExternalEval(script);
    }
    public static void InitiateDownload(string aName, string aData)
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(aData);
        InitiateDownload(aName, data);
    }

    

    
    
    
}
