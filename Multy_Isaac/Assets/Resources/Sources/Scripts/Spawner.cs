﻿using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    public string monster;
    public GameObject offlineMonster;
    public float minDelay, maxDelay;
    public BoxCollider2D bound;

    private void Start()
    {
        StartCoroutine(spawn());
    }

    IEnumerator spawn()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minDelay,maxDelay));
            if (PhotonNetwork.IsMasterClient)
            {
                if (PhotonNetwork.OfflineMode)
                    Instantiate(offlineMonster,
                        new Vector3(Random.Range(bound.bounds.min.x, bound.bounds.max.x),
                            Random.Range(bound.bounds.min.y, bound.bounds.max.y)), quaternion.identity);
                else
                    PhotonNetwork.InstantiateRoomObject(monster,
                        new Vector3(Random.Range(bound.bounds.min.x, bound.bounds.max.x),
                            Random.Range(bound.bounds.min.y, bound.bounds.max.y)), quaternion.identity);
            }
        }
    }
}
