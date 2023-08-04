using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crafting : MonoBehaviour
{
    public List<Recipe> recipies;
    Inventory inv;

    private void Awake()
    {
        inv = this.GetComponent<Inventory>();
    }

    public void AttemptCraft(int index)
    {
        bool check = true;
        //Check if items are in inventory
        for (int i = 0; i < recipies[index].ingredients.Count; i++)
        {
            if (inv.SearchForItem(recipies[index].ingredients[i].itemName) < recipies[index].ingredients[i].amount)
            {
                check = false;
            }
        }

        //If craftble
        if(check)
        {
            for (int i = 0; i < recipies[index].amount; i++)
            {
                inv.InsertItem(Resources.Load<GameObject>("Prefabs/" + recipies[index].itemName));
            }
        }
    }

    public void Redraw(int menu)
    {

    }

}

[System.Serializable]
public class Recipe
{
    public string itemName;
    public uint amount;
    public List<Ingredient> ingredients;
}
[System.Serializable]
public class Ingredient
{
    public string itemName;
    public uint amount;
}