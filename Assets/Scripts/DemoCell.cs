using UnityEngine;
using UnityEngine.UI;
using PolyAndCode.UI;
using TMPro;
using System.Collections.Generic;

//Cell class for demo. A cell in Recyclable Scroll Rect must have a cell class inheriting from ICell.
//The class is required to configure the cell(updating UI elements etc) according to the data during recycling of cells.
//The configuration of a cell is done through the DataSource SetCellData method.
//Check RecyclableScrollerDemo class
public class DemoCell : MonoBehaviour, ICell
{
    //UI
    public TMP_Text nameLabel;
    public Image icon;
    public Inventory inventory;

    //Model
    private Inventory.Item tagHold;
    private int _cellIndex;

    private void Start()
    {
        //Can also be done in the inspector
        GetComponent<Button>().onClick.AddListener(ButtonListener);
    }

    //This is called from the SetCell method in DataSource
    public void ConfigureCell(Inventory.Item tags,int cellIndex)
    {
        _cellIndex = cellIndex;
        tagHold = tags;

        for (int i = 0; i < tags.tags.Count; i++)
        {
            switch (tags.tags[i].name)
            {
                case "Name":
                    nameLabel.text = tags.tags[i].attribute + " (" + tags.amount + "x)";
                    break;
                case "Icon":
                    icon.sprite = Resources.Load<Sprite>("Icons/" + tags.tags[i].attribute);
                    break;
                case "Color":
                   
                    Color thecolor = Color.red;
                    ColorUtility.TryParseHtmlString("#"+tags.tags[i].attribute, out thecolor);
                    Debug.Log(thecolor);
                    icon.color = thecolor;
                    break;
                default:
                    break;
            }
        }
    }

    
    private void ButtonListener()
    {
        inventory.RemoveItem(_cellIndex, false);
    }
}
