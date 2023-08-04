using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GlobalManager : MonoBehaviour
{
    [Header("Settings")]
    public DamageStyle damageStyle;
    public enum DamageStyle
    {
        rounded,
        floatingPoint,
        raw,
    }

    [Header("Players")]
    public List<activePlayers> playerList = new List<activePlayers>();

    [Header("Objects")]
    public Sprite[] alphabet;
    public GameObject playerPrefab;
    public PlayerInput controls;
    public Camera mainCam;

    [Header("Debug")]
    public bool assignHost;
    public bool assignBot;
    public Team assignTeam;
    public enum Team
    {
        Red,
        Blue,
        Neutral,
    }
    public bool randomPersonality;

    public struct playerInputs
    {
        public Vector2 cam;
        public Vector2 movement;
        public int primaryFire;
        public int secondaryFire;
        public int crouch;
    }

    [System.Serializable]
    public class activePlayers
    {
        public bool dead;
        public GameObject player;
        public Player playerComp;
        public float timeTillRespawn;
        public int assignedPlayer = 0;
        public playerInputs inputs;
        public botAI ai;
    }

    private void Update()
    {
        //Assign Input
        AssignInputs();

        //Debug
        if (assignHost)
        {
            assignHost = false;
            AssignHostPlayer(assignTeam);
        }
        if (assignBot)
        {
            assignBot = false;
            AssignBotPlayer(assignTeam);
        }

        //Respawn
        for (int i = 0; i < playerList.Count; i++)
        {
            if (playerList[i].dead)
            {
                if (playerList[i].timeTillRespawn <= Time.time)
                {
                    Respawn(i);
                }
            }
        }
    }

    ///<summary> 
    ///Assigns the host with the player ID of 1;
    ///</summary>
    public void AssignHostPlayer(Team team)
    {
        for (int i = 0; i < playerList.Count; i++)
        {
            if (playerList[i].assignedPlayer == 1)
            {
                Destroy(playerList[i].player);
                playerList.RemoveAt(i);
            }
        }
        activePlayers host = AssignPlayer(1, team);
        host.playerComp.PlayerCamScript = mainCam;
    }

    ///<summary> 
    ///Assigns a client with the player ID above 1;
    ///</summary>
    public void AssignClientPlayer(int client, Team team)
    {
        for (int i = 0; i < playerList.Count; i++)
        {
            if (playerList[i].assignedPlayer == client)
            {
                Destroy(playerList[i].player);
                playerList.RemoveAt(i);
                Debug.LogError("Duplicate Client Player Assign");
            }
        }
        AssignPlayer(client, team);
    }

    ///<summary> 
    ///Assigns a bot with the player ID of 0;
    ///</summary>
    public void AssignBotPlayer(Team team)
    {
        activePlayers bot = AssignPlayer(0, team);
        botAI ai = this.gameObject.AddComponent<botAI>();
        ai.currentBot = bot.player;
        bot.ai = ai;
        if (randomPersonality)
        {
            ai.revenge = Random.Range(0.0f, 1.0f);
            ai.revenge *= ai.revenge;
            ai.pacifist = Random.Range(0.0f, 1.0f);
            ai.pacifist *= ai.pacifist;
            ai.helper = Random.Range(0.0f, 1.0f);
            ai.helper *= ai.helper;
            ai.easyKills = Random.Range(0.0f, 1.0f);
            ai.easyKills *= ai.easyKills;
            ai.badAim = Random.Range(0.0f, 1.0f);
            ai.badAim *= ai.badAim;
            ai.awareness = Random.Range(0.0f, 1.0f);
            ai.awareness *= ai.awareness;
            ai.range = Random.Range(0.0f, 1.0f);
            ai.range *= ai.range;
        }
    }

    activePlayers AssignPlayer(int ID, Team team)
    {
        //Create Player
        activePlayers playerNew = new activePlayers();
        playerNew.player = GameObject.Instantiate(playerPrefab);
        Player playerComp = playerNew.player.GetComponent<Player>();
        playerNew.player.transform.position = new Vector3(Random.Range(-50, 50),5, Random.Range(-50, 50));

        //Assign Guns
        GameObject[] playerGuns = new GameObject[1];
        playerGuns[0] = new GameObject("Gun");
        playerGuns[0].transform.localPosition = Vector3.zero;
        Gun playerGunComp = playerGuns[0].AddComponent<Gun>();
        if (ID == 1)
        {
            playerGunComp.isHostGun = true;
        }
        else
        {
            playerGunComp.isHostGun = false;
        }
        playerGunComp.gunStats = this.GetComponent<GunDictionary>().guns[Random.Range(0, this.GetComponent<GunDictionary>().guns.Length)];
        playerComp.guns = playerGuns;

        //Team
        LayerMask teamLayer = new LayerMask();
        switch (team)
        {
            case Team.Red:
                teamLayer = LayerMask.NameToLayer("Red");
                Debug.Log("Assigned ID- " + ID + " to Red Team.");
                break;
            case Team.Blue:
                teamLayer = LayerMask.NameToLayer("Blue");
                Debug.Log("Assigned ID- " + ID + " to Blue Team.");
                break;
            case Team.Neutral:
                teamLayer = LayerMask.NameToLayer("Neutral");
                Debug.Log("Assigned ID- " + ID + " to Neutral Team.");
                break;
            default:
                break;
        }
        playerNew.player.layer = teamLayer;
        playerGuns[0].gameObject.layer = teamLayer;

        //Other Variables
        playerNew.dead = false;
        playerNew.timeTillRespawn = 0;
        playerNew.assignedPlayer = ID;
        playerNew.playerComp = playerComp;
        playerList.Add(playerNew);
        return playerNew;
    }

    public void AddCorpse(GameObject corpse)
    {
        for (int i = 0; i < playerList.Count; i++)
        {
            if (corpse == playerList[i].player)
            {
                playerList[i].dead = true;
                playerList[i].timeTillRespawn = Time.time + 5.0f;
            }
        }
    }

    public void Respawn(int index)
    {
        playerList[index].dead = false;
        playerList[index].timeTillRespawn = 0;
        GameObject player = playerList[index].player;
        player.SetActive(true);
        for (int i = 0; i < player.GetComponent<Player>().guns.Length; i++)
        {
            player.GetComponent<Player>().guns[i].SetActive(true);
        }
        DamagableEntity de = player.GetComponent<DamagableEntity>();
        de.health = de.totalHealth;
        player.transform.position += new Vector3(Random.Range(-200, 200), Random.Range(-200, 200));
    }

    public void DamageUI(Vector3 damage)
    {
        switch (damageStyle)
        {
            case DamageStyle.rounded:
                damage.z = (int)damage.z;
                break;
            case DamageStyle.floatingPoint:
                damage.z = Mathf.Round(damage.z * 10f) / 10f;
                break;
            default:
                break;
        }
        string text = damage.z.ToString();
        for (int i = 0; i < text.Length; i++)
        {
            GameObject bullet = new GameObject("Damage UI");
            SpriteRenderer rr = bullet.AddComponent<SpriteRenderer>();
            FadeOutObject fo = bullet.AddComponent<FadeOutObject>();
            rr.sortingLayerName = "UI";
            fo.fadeTime = 0.2f;
            fo.waitTime = 0.3f;
            fo.gravity = new Vector3(0, 20.4f, 0);
            switch (text[i])
            {
                case '0':
                    {
                        rr.sprite = alphabet[0];
                        break;
                    }
                case '1':
                    {
                        rr.sprite = alphabet[1];
                        break;
                    }
                case '2':
                    {
                        rr.sprite = alphabet[2];
                        break;
                    }
                case '3':
                    {
                        rr.sprite = alphabet[3];
                        break;
                    }
                case '4':
                    {
                        rr.sprite = alphabet[4];
                        break;
                    }
                case '5':
                    {
                        rr.sprite = alphabet[5];
                        break;
                    }
                case '6':
                    {
                        rr.sprite = alphabet[6];
                        break;
                    }
                case '7':
                    {
                        rr.sprite = alphabet[7];
                        break;
                    }
                case '8':
                    {
                        rr.sprite = alphabet[8];
                        break;
                    }
                case '9':
                    {
                        rr.sprite = alphabet[9];
                        break;
                    }
                case '.':
                    {
                        rr.sprite = alphabet[10];
                        break;
                    }
                case '-':
                    {
                        rr.sprite = alphabet[11];
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            //Transform
            bullet.transform.position = new Vector2((damage.x + (((float)i / (Mathf.Max(text.Length - 1.0f, 1))) * rr.sprite.texture.width * (text.Length - 1)) - (text.Length / 2.0f) * rr.sprite.texture.width), damage.y);
        }
    }

    void AssignInputs()
    {
        for (int i = 0; i < playerList.Count; i++)
        {
            if (!playerList[i].dead)
            {
                if (playerList[i].assignedPlayer == 0)
                {
                    //Bot Input
                    playerList[i].inputs = playerList[i].ai.BotUpdate(playerList[i].inputs);
                }
                else if (playerList[i].assignedPlayer == 1)
                {
                    //Host Input
                    //Gather Controls
                    playerList[i].inputs.crouch = GetActionState("Crouch", playerList[i].inputs.crouch);
                    playerList[i].inputs.primaryFire = GetActionState("PrimaryFire", playerList[i].inputs.primaryFire);
                    playerList[i].inputs.secondaryFire = GetActionState("SecondaryFire", playerList[i].inputs.secondaryFire);
                    playerList[i].inputs.movement = controls.actions["Move"].ReadValue<Vector2>().normalized;
                    playerList[i].inputs.cam = controls.actions["Camera"].ReadValue<Vector2>();
                }
                else
                {
                    //Client Input

                }
                //Update Character
                playerList[i].playerComp.UpdateMovement(playerList[i].inputs);
            }
        }
    }

    int GetActionState(string actionString, int previousPress)
    {
        if (controls.actions[actionString].WasPressedThisFrame() && previousPress == 0)
        {
            return 1;
        }
        else if (!controls.actions[actionString].WasPressedThisFrame() && previousPress == 1)
        {
            return 2;
        }
        else if (controls.actions[actionString].WasReleasedThisFrame() && previousPress == 2)
        {
            return 3;
        }
        else if (!controls.actions[actionString].WasReleasedThisFrame() && previousPress == 3)
        {
            return 0;
        }
        return previousPress;
    }
}
