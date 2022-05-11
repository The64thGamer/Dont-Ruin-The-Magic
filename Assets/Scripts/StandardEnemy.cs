using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class StandardEnemy : MonoBehaviour
{
    // Start is called before the first frame update
    public float aggresionDistance = 20.0f;
    public Transform eyes;
    Player player;
    NavMeshAgent agent;
    float counter;

    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
        agent = this.GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        counter = Mathf.Max(0, counter - Time.deltaTime);
        if(counter == 0)
        {
            counter = 1;
            RecalculatePlayer();
        }
    }

    void RecalculatePlayer()
    {
        if (player.camsSpotted > 0 && Vector3.Distance(this.transform.position, player.PlayerCamScript.transform.position) < aggresionDistance)
        {
                    agent.SetDestination(player.transform.position);
        }
    }
}
