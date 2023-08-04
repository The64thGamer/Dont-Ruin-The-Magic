using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class botAI : MonoBehaviour
{
    GlobalManager manager;

    [Header("Bot")]
    public GameObject currentBot;
    public DamagableEntity botDI;
    public Player botPly;

    [Header("States")]
    public botState currentBotState;
    public botMoveState currentBotMoveState;

    [Header("Targets")]
    public Transform movementTarget;
    public float moveTargetDist;
    public Transform attackTarget;
    public float attackTargetDist;
    public Vector3 pivotTarget;

    [Header("Values")]
    [Range(0, 1)]
    public float currentStateAttentiveness;
    [Range(0, 1)]
    public float currentSubStateAttentiveness;
    [Range(0, 1)]
    public float currentMoveStateAttentiveness;
    public float minStateAttentiveness = 1.0f;
    public float averageStateAttentiveness;
    public float maxStateAttentiveness;

    [Header("Priorities")]
    [Range(0, 1)]
    public float enemies_PR = 1;
    [Range(0, 1)]
    public float teamMates_PR = 1;
    [Range(0, 1)]
    public float objective_PR = 1;
    [Range(0, 1)]
    public float openness_PR = 1;
    [Range(0, 1)]
    public float dying_PR = 1;
    [Range(0, 1)]
    public float noAmmo_PR = 1;
    [Range(0, 1)]
    public float pickupsNearby_PR = 1;

    [Header("Personality")]
    [Range(0, 1)]
    public float revenge; //K/D ratio affects ignoring objective to kill enemies
    [Range(0, 1)]
    public float pacifist; //ignores enemies
    [Range(0, 1)]
    public float helper; //prioritizes teammates
    [Range(0, 1)]
    public float easyKills; //avoids groups of enemies
    [Range(0, 1)]
    public float badAim; //how well can aim
    [Range(0, 1)]
    public float awareness; //how aware of the situation
    [Range(0, 1)]
    public float range; //how close the bot wants to be when attacking enemies

    [Header("Situation")]
    [Range(0, 1)]
    public float enemies_SU; //amount of enemies and how close
    [Range(0, 1)]
    public float teamMates_SU; //amount of teammates and how close
    [Range(0, 1)]
    public float objective_SU; //proximity to objective
    [Range(0, 1)]
    public float openness_SU; //how open the current area is
    [Range(0, 1)]
    public float dying_SU; //how close bot is to dying
    [Range(0, 1)]
    public float noAmmo_SU; //how close bot is to no ammo
    [Range(0, 1)]
    public float pickupsNearby_SU; //pickups are nearby

    [Header("Debug")]
    public bool forceState;
    public botState forceBotState;
    public bool forceMoveState;
    public botMoveState forceBotMoveState;

    //private
    float currentStateTime;
    float currentSubStateTime;
    float currentMoveStateTime;
    Vector3 mouseAim;
    Vector3 movementAim;
    List<Vector2> gizmoVectors = new List<Vector2>();
    Collider2D[] hit;
    List<Transform> objectiveTransforms = new List<Transform>();
    List<Transform> teamMateTransforms = new List<Transform>();
    List<Transform> enemyTransforms = new List<Transform>();

    public enum botState
    {
        none,
        attack
    }
    public enum botMoveState
    {
        none,
        moveToEntity,
        randomMovement,
    }

    private void Start()
    {
        movementAim = currentBot.transform.position;
        manager = GameObject.Find("Global Manager").GetComponent<GlobalManager>();
        botPly = currentBot.GetComponent<Player>();
        botDI = currentBot.GetComponent<DamagableEntity>();
    }

    public GlobalManager.playerInputs BotUpdate(GlobalManager.playerInputs previousInputs)
    {
        gizmoVectors.Clear();
        GlobalManager.playerInputs newInputs = new GlobalManager.playerInputs();

        //Visualize
        VisualizeSituation();

        //Assess
        AssessSituation();

        //Act
        switch (currentBotState)
        {
            case botState.none:
                if(movementTarget != null)
                {
                    newInputs.cam = new Vector2(movementTarget.position.x - currentBot.transform.position.x, movementTarget.position.y - currentBot.transform.position.y).normalized;
                }
                break;
            case botState.attack:
                if (attackTarget != null)
                {
                    mouseAim = Vector3.Lerp(mouseAim, attackTarget.position + (new Vector3(Random.Range(-300.0f, 300.0f), Random.Range(-300.0f, 300.0f), 0) * badAim * badAim), Random.Range(0.2f, 10) * Time.deltaTime);
                    newInputs.cam = mouseAim;
                    switch (newInputs.primaryFire)
                    {
                        case 0:
                            newInputs.primaryFire = 1;
                            break;
                        case 1:
                            newInputs.primaryFire = 2;
                            break;
                        default:
                            break;
                    }
                }
                break;
            default:
                break;
        }
        switch (currentBotMoveState)
        {
            case botMoveState.none:
                break;
            case botMoveState.moveToEntity:
                if(objective_SU > 0)
                {

                }
                else
                {

                }

                if(objective_SU > teamMates_SU && objectiveTransforms.Count > 0)
                {
                    movementTarget = objectiveTransforms[0];
                }
                else if(teamMateTransforms.Count > 0)
                {
                    movementTarget = teamMateTransforms[0];
                }
                if (movementTarget != null)
                {
                    Vector3 move = Vector3.Lerp(movementTarget.position, pivotTarget, Mathf.Sin((awareness * 420.0f) + Time.time * badAim * awareness) - Mathf.Cos(badAim + (Time.time*2) * badAim * awareness)); 
                    move = Vector3.Lerp(move, mouseAim, Mathf.Sin((revenge + Time.time * awareness) * 3));
                    move = Vector3.Lerp(move, movementAim, Mathf.Tan((revenge*53.2f) + (badAim * 124.8f) + Time.time / 5.0f));

                    newInputs.movement = new Vector2(move.x - currentBot.transform.position.x, move.y - currentBot.transform.position.y).normalized;
                }
                break;
            case botMoveState.randomMovement:
                newInputs.movement = (movementAim - currentBot.transform.position).normalized;
                break;
            default:
                break;
        }

        //Debug
        currentStateAttentiveness = Time.time - currentStateTime / minStateAttentiveness;
        currentSubStateAttentiveness = Time.time - currentSubStateTime / minStateAttentiveness;
        currentMoveStateAttentiveness = Time.time - currentMoveStateTime / minStateAttentiveness;

        //Return
        return newInputs;
    }

    void VisualizeSituation()
    {
        enemies_SU = 0;
        teamMates_SU = -0.1f;
        objective_SU = 0;
        openness_SU = 1;
        dying_SU = 0;
        noAmmo_SU = 0;
        pickupsNearby_SU = 0;
        enemyTransforms.Clear();
        teamMateTransforms.Clear();
        objectiveTransforms.Clear();
        pivotTarget = Vector3.zero;

        //Dying
        dying_SU = 1 - (Mathf.Pow(botDI.health / botDI.totalHealth, 3) + (revenge * revenge)); //Fix revenge to be based on KD ratio

        //Objects, Enemies, and Teammates
        hit = Physics2D.OverlapCircleAll(currentBot.transform.position, 200);
        bool applyCSTA = false;

        for (int e = 0; e < hit.Length; e++)
        {
            //Gizmos
            gizmoVectors.Add(currentBot.transform.position);
            gizmoVectors.Add(hit[e].transform.position);
            pivotTarget += hit[e].gameObject.transform.position;

            if(hit[e].gameObject.layer == LayerMask.NameToLayer("Objective"))
            {
                objectiveTransforms.Add(hit[e].gameObject.transform);
                objective_SU = 1 - Mathf.Min((Vector3.Distance(hit[e].gameObject.transform.position, currentBot.transform.position) / 200.0f) - 0.1f,1);
            }

            switch (isEnemyTag(LayerMask.LayerToName(hit[e].gameObject.layer)))
            {
                case -1:
                    //Object
                    openness_SU -= 0.002f;
                    break;
                case 0:
                    //Teammate
                    teamMateTransforms.Add(hit[e].gameObject.transform);
                    teamMates_SU += 0.1f;
                    break;
                case 1:
                    //Enemy
                    enemyTransforms.Add(hit[e].gameObject.transform);
                    enemies_SU += 0.1f;
                    if (attackTarget == null)
                    {
                        attackTarget = hit[e].transform;//Immediate Attack when no other enemies
                        applyCSTA = true;
                    }
                    else
                    {
                        //Enemy Check
                        if (!attackTarget.gameObject.activeSelf)
                        {
                            attackTarget = null;
                            attackTargetDist = 0;
                        }
                        //Attentiveness check
                        if (Time.time - currentStateTime >= minStateAttentiveness && Time.time - currentSubStateTime >= minStateAttentiveness)
                        {
                            if (Vector2.Distance(currentBot.transform.position, hit[e].transform.position) < attackTargetDist)
                            {
                                applyCSTA = true;
                                attackTarget = hit[e].transform;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        if (hit.Length != 0)
        {
            pivotTarget /= hit.Length;
        }
        else
        {
            pivotTarget = currentBot.transform.position;
        }
        if (applyCSTA)
        {
            currentSubStateTime = Time.time;
        }

        //Rerange
        enemies_SU = Mathf.Clamp01(enemies_SU * enemies_PR);
        teamMates_SU = Mathf.Clamp01(teamMates_SU * teamMates_PR);
        objective_SU = Mathf.Clamp01(objective_SU * objective_PR);
        openness_SU = Mathf.Clamp01(openness_SU * openness_PR);
        dying_SU = Mathf.Clamp01(dying_SU * dying_PR);
        noAmmo_SU = Mathf.Clamp01(noAmmo_SU * noAmmo_PR);
        pickupsNearby_SU = Mathf.Clamp01(pickupsNearby_SU * pickupsNearby_PR);

        //Distances
        if (attackTarget != null)
        {
            attackTargetDist = Vector2.Distance(currentBot.transform.position, attackTarget.position);
        }
        if (movementTarget != null)
        {
            moveTargetDist = Vector2.Distance(currentBot.transform.position, movementTarget.position);
        }
    }

    void AssessSituation()
    {
        //Olds
        botState oldState = currentBotState;
        botMoveState oldMove = currentBotMoveState;

        //Assess
        if (forceState)
        {
            currentBotState = forceBotState;
        }
        else
        {
            //Assess
            //Is the bot allowed to change state
            if (Time.time - currentStateTime >= minStateAttentiveness)
            {
                if (enemies_SU > 0)
                {
                    currentBotState = botState.attack;
                }
                else
                {
                    currentBotState = botState.none;
                }
            }
        }
        if (forceMoveState)
        {
            currentBotMoveState = forceBotMoveState;
        }
        else
        {
            //Assess
            //Is the bot allowed to change state
            if (Time.time - currentMoveStateTime >= minStateAttentiveness)
            {
                movementAim += new Vector3(Random.Range(-6.0f, 6.0f), Random.Range(-6.0f, 6.0f));
                if (teamMates_SU > 0 || objective_SU > 0)
                {
                    //Move Towards
                    currentBotMoveState = botMoveState.moveToEntity;
                }
                else
                {
                    //Random
                    currentBotMoveState = botMoveState.randomMovement;
                }

                if((int)currentMoveStateAttentiveness % 5 == 0)
                {
                    movementAim = currentBot.transform.position;
                }
            }
            else
            {
                movementAim = currentBot.transform.position;
            }
        }

        //Old Check
        if(oldState != currentBotState)
        {
            currentStateTime = Time.time;
            currentSubStateTime = Time.time;
        }
        if(oldMove != currentBotMoveState)
        {
            currentMoveStateTime = Time.time;
        }
    }

    int isEnemyTag(string layer)
    {
        if (layer == "Red" || layer == "Blue" || layer == "Neutral")
        {
            if (layer == LayerMask.LayerToName(currentBot.gameObject.layer))
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }
        else
        {
            return -1;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(mouseAim, Vector3.one * 10);

        Gizmos.color = Color.green;
        Gizmos.DrawCube(movementAim, Vector3.one * 10);

        Gizmos.color = Color.blue;
        Gizmos.DrawCube(pivotTarget, Vector3.one * 10);

        Gizmos.color = Color.white;
        for (int i = 0; i < gizmoVectors.Count; i += 2)
        {
            Gizmos.DrawLine(gizmoVectors[i], gizmoVectors[i + 1]);
        }
        gizmoVectors.Clear();
    }

}
