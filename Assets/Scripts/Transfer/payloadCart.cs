using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class payloadCart : MonoBehaviour
{
    [Header("Values")]
    public float baseSpeed;
    public float deceleration;
    public float acceleration;
    public int maxCartPushers;
    public float muliplierPerPusher;
    public float regenDelay;
    public float regenAmount;

    [Header("Debug")]
    [Range(0, 1)]
    public float cartPosPercentage;
    public float totalTrackLength;
    public float currentCartSpeed;
    public int peopleOnCart;
    public int otherTeamPause;

    public Transform startPoint;
    public cartTrack[] tracks;
    Vector2 objectPosition;
    public float cartPosOnTrack;

    [System.Serializable]
    public class cartTrack
    {
        public Transform p1;
        public Transform p2;
        public Transform p3;
        public float totalDistance;
        public Vector2[] distanceAndT;
    }

    private void Start()
    {
        FindTrackLength(0);
    }

    // Update is called once per frame
    void Update()
    {
        //Apply Speed
        float currentNeededSpeed;
        if (otherTeamPause <= 0)
        {
            currentNeededSpeed = (Mathf.Min(maxCartPushers, peopleOnCart) * muliplierPerPusher) * baseSpeed;
        }
        else
        {
            currentNeededSpeed = 0;
        }
        
        if (currentNeededSpeed < currentCartSpeed)
        {
            currentCartSpeed = Mathf.Max(currentNeededSpeed, currentCartSpeed - (deceleration * Time.deltaTime));
        }
        else if (currentNeededSpeed > currentCartSpeed)
        {
            currentCartSpeed = Mathf.Min(currentNeededSpeed, currentCartSpeed + (acceleration * Time.deltaTime));
        }

        //Apply Cart Position
        cartPosOnTrack += currentCartSpeed * Time.deltaTime;
        if (cartPosPercentage > 1 || cartPosOnTrack > tracks[0].totalDistance)
        {
            cartPosOnTrack = 0;
        }
        cartPosPercentage = FindCartPos();
        Vector2 p0 = startPoint.position;
        Vector2 p1 = tracks[0].p1.position;
        Vector2 p2 = tracks[0].p2.position;
        Vector2 p3 = tracks[0].p3.position;
        objectPosition = Mathf.Pow(1 - cartPosPercentage, 3) * p0 + 3 * Mathf.Pow(1 - cartPosPercentage, 2) * cartPosPercentage * p1 + 3 * (1 - cartPosPercentage) * Mathf.Pow(cartPosPercentage, 2) * p2 + Mathf.Pow(cartPosPercentage, 3) * p3;
        transform.position = objectPosition;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Blue"))
        {
            peopleOnCart++;
        }
        if (collision.gameObject.layer == LayerMask.NameToLayer("Red"))
        {
            otherTeamPause++;
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Blue"))
        {
            peopleOnCart--;
        }
        if (collision.gameObject.layer == LayerMask.NameToLayer("Red"))
        {
            otherTeamPause--;
        }
    }

    float FindCartPos()
    {
        float T = 0;
        int index = 0;
        float cartSubPos = cartPosOnTrack;
        //Found Track, find subtrack piece
        float currentLength = 0;
        for (int i = 0; i < tracks[index].distanceAndT.Length; i++)
        {
            float newlength = currentLength + tracks[index].distanceAndT[i].x;

            if(cartSubPos < newlength)
            {
                //Find T between points
                if(i < tracks[index].distanceAndT.Length-1)
                {
                    T = remap(cartSubPos, currentLength, newlength, tracks[index].distanceAndT[i].y, tracks[index].distanceAndT[i + 1].y);
                }
                else
                {
                    T = remap(cartSubPos, currentLength, newlength, tracks[index].distanceAndT[i].y, 1);
                }
                break;
            }

            currentLength = newlength;
        }
        return T;
    }

    float remap(float val, float in1, float in2, float out1, float out2)
    {
        return out1 + (val - in1) * (out2 - out1) / (in2 - in1);
    }

    void FindTrackLength(int index)
    {
        Vector2 p0 = startPoint.position;
        Vector2 p1 = tracks[0].p1.position;
        Vector2 p2 = tracks[0].p2.position;
        Vector2 p3 = tracks[0].p3.position;
        List<Vector2> distanceTNew = new List<Vector2>();
        List<float> trackT = new List<float>();
        List<Vector2> trackDots = new List<Vector2>();
        for (float t = 0; t < 1.1; t += 0.1f)
        {
            trackT.Add(t);
            trackDots.Add(Mathf.Pow(1 - t, 3) * p0 + 3 * Mathf.Pow(1 - t, 2) * t * p1 + 3 * (1 - t) * Mathf.Pow(t, 2) * p2 + Mathf.Pow(t, 3) * p3);
        }

        //Return
        float total = 0;
        for (int i = 0; i < trackDots.Count-1; i++)
        {
            float distance = Vector2.Distance(trackDots[i], trackDots[i + 1]);
            total += distance;
            distanceTNew.Add(new Vector2(distance, trackT[i]));
        }

        tracks[index].totalDistance = total;
        tracks[index].distanceAndT = distanceTNew.ToArray();
    }
}
