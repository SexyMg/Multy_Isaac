using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using DG.Tweening;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Experimental.Rendering.LWRP;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Light2D = UnityEngine.Experimental.Rendering.Universal.Light2D;

public class TimeManager : MonoBehaviour
{
    public GameObject[] gases;
    public GameObject[] gases_M;
    public ZombieSpawner spawner;
    public Text dayText;
    public int day = 1;
    private float maxTime;
    public RectTransform ClockRect;
    public bool isNight = false;
    private bool isDay = true;
    private Light2D globalLight;
    private PhotonView pv;
    public int time = 0;
    public int dayTime;
    public int dayToNightTime;
    public int nightTime;
    public int nightToDayTime;

    void Start()
    {
        pv = GetComponent<PhotonView>();
        globalLight = GetComponent<Light2D>();
        
        maxTime = dayTime + dayToNightTime + nightTime + nightToDayTime;
        if (SceneManager.GetActiveScene().name != "Tutorial")
        {
            StartCoroutine(cor());
            spawner.DaybyDay(day);
        }
    }

    private void Update()
    {
        ClockRect.eulerAngles = new Vector3(ClockRect.eulerAngles.x, ClockRect.eulerAngles.y, time / maxTime * -360);
        dayText.text = "Day " + day;
    }

    IEnumerator cor()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            bool canGo = false;
            if (PhotonNetwork.OfflineMode)
                canGo = true;
            else
            {
                if (PhotonNetwork.IsMasterClient)
                    canGo = true;
            }

            if (canGo)
            {
                         if (PhotonNetwork.OfflineMode)
            {
                time++;
            }
            else
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    time++;
                    pv.RPC("timeRPC", RpcTarget.All, time,day);
                }
            }

            if (time > dayTime)
            {
                if (isDay)
                {
                    isDay = false;
                    //낮에서 밤으로 전환

                    if (PhotonNetwork.OfflineMode)
                    {
                        nightRPC();
                    }
                    else
                    {
                        if(PhotonNetwork.IsMasterClient)
                            pv.RPC("nightRPC",RpcTarget.All);
                    }
                }
            }

            if (time > dayTime + dayToNightTime)
            {
                if (!isNight)
                {
                    if (PhotonNetwork.OfflineMode)
                    {
                        setIsNight(true);
                    }
                    else
                    {
                        if (PhotonNetwork.IsMasterClient)
                        {
                            pv.RPC("setIsNight", RpcTarget.All, true);
                        }
                    }
                }
            }

            if (time > dayTime + dayToNightTime + nightTime)
            {
                if (!isDay)
                {
                    isDay = true;
                    //밤에서 낮으로 전환

                    if (PhotonNetwork.OfflineMode)
                    {
                       dayRPC();
                    }
                    else
                    {
                        if(PhotonNetwork.IsMasterClient)
                            pv.RPC("dayRPC",RpcTarget.All);
                    }

                    yield return new WaitForSeconds(nightToDayTime);
                    if (PhotonNetwork.OfflineMode)
                    {
                        time = 0;
                        day++;
                        FindObjectOfType<ShopTem>().Change();
                       dbdRPC();
                       //gas(day);
                    }
                    else
                    {
                        if (PhotonNetwork.IsMasterClient)
                        {
                            time = 0;
                            day++;
                            FindObjectOfType<ShopTem>().Change();
                            pv.RPC("dbdRPC", RpcTarget.All);
                            pv.RPC("timeRPC", RpcTarget.All, time,day);
                            pv.RPC("gas",RpcTarget.All,day);
                        }
                    }
                    if(day>=4)
                        SteamAchieveemnt.instance.SetAchievement("survivor");
                }
            }

            if (time == 0)
            {
                if (isNight)
                {
                    if (PhotonNetwork.OfflineMode)
                    {
                        setIsNight(false);
                    }
                    else
                    {
                        if (PhotonNetwork.IsMasterClient)
                        {
                            pv.RPC("setIsNight", RpcTarget.All, false);
                        }
                    }
                }
            }   
            }
        }
    }

    [PunRPC]
    void gas(int daay)
    {
        gases[daay-2].SetActive(true);
        gases_M[daay-2].SetActive(true);
    }
    [PunRPC]
    void dbdRPC()
    {
        spawner.DaybyDay(day);
    }
    [PunRPC]
    void dayRPC()
    {
        DOTween.To(() => globalLight.intensity, x => globalLight.intensity = x, 1, nightToDayTime)
            .SetEase(Ease.InQuad);
    }
    [PunRPC]
    void nightRPC()
    {
        DOTween.To(() => globalLight.intensity, x => globalLight.intensity = x, 0, dayToNightTime)
            .SetEase(Ease.InQuad);
    }
    [PunRPC]
    void timeRPC(int value, int value2)
    {
        day = value2;
        time = value;
    }

    [PunRPC]
    void setIsNight(bool istrue)
    {
        isNight = istrue;
    }
}
