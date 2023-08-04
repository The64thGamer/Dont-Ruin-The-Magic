using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecurityCam : MonoBehaviour
{
    // Start is called before the first frame update
    float maxPlayerLight = 0.45f;
    Player player;
    Renderer rend;
    bool isTracking;
    bool isVisible;
    public LayerMask mask;
    Vector3[] directions = new Vector3[]
        {
            new Vector3(0.0f, 1.0f, 0.0f),
            new Vector3(0.0f, -1.0f, 0.0f),
            new Vector3(0.0f, 0.0f, 1.0f),
            new Vector3(0.0f, 0.0f, -1.0f),
            new Vector3(1.0f, 0.0f, 0.0f),
            new Vector3(-1.0f, 0.0f, 0.0f),
        };
    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
        rend = player.GetComponent<Renderer>();
        StartCoroutine(TabPlayer());
    }

    IEnumerator TabPlayer()
    {
        while (true)
        {
            RaycastHit check;

            if (isTracking)
            {
                if (Physics.Raycast(this.transform.position, (player.PlayerCamScript.transform.position - new Vector3(0, 0.2f, 0)) - this.transform.position, out check, 50.0f, mask))
                {
                    if (check.collider.gameObject != player.gameObject)
                    {
                        isTracking = false;
                        isVisible = false;
                        player.UnPingCamera(true);
                        player.UnPingCamera(false);
                    }
                    else
                    {
                        //Check Light of player
                        float finalColor = CheckPlayerColor();

                        //If True
                        if (finalColor < maxPlayerLight)
                        {
                            isTracking = false;
                            player.UnPingCamera(true);
                        }
                    }
                }
                else
                {
                    isTracking = false;
                    isVisible = false;
                    player.UnPingCamera(true);
                    player.UnPingCamera(false);
                }
                yield return new WaitForSeconds(1 + Random.Range(0, 1));
            }
            else
            {
                if (Physics.Raycast(this.transform.position, (player.PlayerCamScript.transform.position - new Vector3(0,0.2f,0)) - this.transform.position, out check, 50.0f, mask))
                {
                    if (check.collider.gameObject == player.gameObject)
                    {
                        if (!isVisible)
                        {
                            player.PingCamera(this.transform, false);
                            isVisible = true;
                        }

                        //Check Light of player
                        float finalColor = CheckPlayerColor();

                        //If True
                        if (finalColor > maxPlayerLight)
                        {
                            isTracking = true;
                            player.PingCamera(this.transform,true);
                        }
                    }
                    else if(isVisible)
                    {
                        isVisible = false;
                        player.UnPingCamera(false);
                    }
                }
                yield return new WaitForSeconds(2 + Random.Range(0, 1));
            }

        }
    }

    float CheckPlayerColor()
    {
        UnityEngine.Rendering.SphericalHarmonicsL2 sphere;
        LightProbes.GetInterpolatedProbe(player.PlayerCamScript.transform.position, rend, out sphere);
        Color[] results = new Color[6];
        sphere.Evaluate(directions, results);
        Color finalColor = (results[0] + results[1] + results[2] + results[3] + results[4] + results[5]) / 6.0f;
        return (finalColor.r + finalColor.g + finalColor.b) / 3.0f;
    }

    private void OnDrawGizmos()
    {
        if (Application.IsPlaying(this))
        {
            RaycastHit check;
            if (Physics.Raycast(this.transform.position, (player.PlayerCamScript.transform.position - new Vector3(0, 0.2f, 0)) - this.transform.position, out check, 50.0f, mask))
            {
                if (check.collider.gameObject == player.gameObject)
                {
                    Gizmos.DrawLine(this.transform.position, (player.PlayerCamScript.transform.position - new Vector3(0, 0.2f, 0)));
                }
            }
        }
    }
}
