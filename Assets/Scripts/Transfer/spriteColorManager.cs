using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spriteColorManager : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    [SerializeField]
    List<Color> appliedColors;
    [SerializeField]
    List<int> priority;
    [SerializeField]
    List<bool> ignore;

    private void Awake()
    {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
    }

    public int AssignColor(int priorityValue)
    {
        //Check if available slots
        for (int i = 0; i < ignore.Count; i++)
        {
            if(ignore[i])
            {
                priority[i] = priorityValue;
                appliedColors[i] = new Color();
                ignore[i] = false;
                return i;
            }
        }
        priority.Add(priorityValue);
        appliedColors.Add(new Color());
        ignore.Add(false);
        return priority.Count - 1;
    }
    public void UpdateColor(Color color, int index)
    {
        appliedColors[index] = color;
    }
    public void RemoveColor(int index)
    {
        ignore[index] = true;
    }

    private void Update()
    {
        spriteRenderer.color = Color.white;
        int finalCount = appliedColors.Count;
        int currentPriority = 0;

        //The larger the priority number, the longer this takes to complete.
        if (finalCount == 1)
        {
            if (!ignore[0])
            {
                spriteRenderer.color -= Color.white - appliedColors[0];
            }
        }
        else
        {
            while (finalCount != 0)
            {
                for (int i = 0; i < appliedColors.Count; i++)
                {
                    if (priority[i] == currentPriority)
                    {
                        if (!ignore[i])
                        {
                            spriteRenderer.color -= Color.white - appliedColors[i];
                        }
                        finalCount--;
                    }
                }
                currentPriority++;
            }
        }
    }
}
