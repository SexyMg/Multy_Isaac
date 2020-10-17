﻿using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using Random = Unity.Mathematics.Random;

public class PlayerItem : MonoBehaviour
{
    //아이템
    public float itemRadious;
    public LayerMask itemLayer;
    public List<tem> ItemList = new List<tem>();
    public Image[] ItemBoxes;
    public GameObject[] btns;
    private Player player;
    public Sprite NullSprite;
    public ItemSlot[] slots;
    
    private void Start()
    {
        player = GetComponent<Player>();
    }

    public void OtherBtnSetFalse(int index)
    {
        for (int i = 0; i < btns.Length; i++)
        {
            if (i != index)
            {
                btns[i].SetActive(false);
            }
        }
    }
    private void Update()
    {
        if (player.pv.IsMine)
        {
            for (int i = 0; i < ItemList.Count; i++)
            {
                ItemBoxes[i].sprite = ItemList[i].ItemSprite;
            }

            for (int i = ItemList.Count; i < 6; i++)
            {
                ItemBoxes[i].sprite = NullSprite;
            }

            if (player.canMove)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Collider2D item = Physics2D.OverlapCircle(transform.position, itemRadious, itemLayer);
                    if (item != null)
                    {
                        if (item.GetComponent<Item>().canGet())
                        {
                            if (ItemList.Count < 6)
                            {
                                GetItem(item.GetComponent<Item>().item);
                                item.GetComponent<Item>().Destroy();   
                            }
                            else
                            {
                                PopUpManager.instance.PopUp("더 이상 주울 수 없습니다!",Color.red);
                            }
                        }
                    }   
                }
            }
        }
    }

    public void DiscardItem(int index, int itemIndex)
    {
        if (ItemList[index] != null)
        {
            ItemList.RemoveAt(index);
            player.pv.RPC("discardRPC",RpcTarget.All,"item"+itemIndex);   
        }
    }

    public void DeadDiscard(int index)
    {
        if (ItemList[index] != null)
        {
            player.pv.RPC("DeadDiscardRPC",RpcTarget.All,"item"+ItemList[index].index);      
        }
    }
    public void Dead()
    {
        for (int i = 0; i < ItemList.Count; i++)
        {
            DeadDiscard(i);
        }

      ItemList.Clear();
    }
    [PunRPC]
    void discardRPC(string itemName)
    {
        PhotonNetwork.InstantiateRoomObject(itemName, transform.position, quaternion.identity);
    }
    
    [PunRPC]
    void DeadDiscardRPC(string itemName)
    {
        PhotonNetwork.InstantiateRoomObject(itemName, 
            new Vector3(transform.position.x+UnityEngine.Random.Range(-1f,1f),transform.position.y+UnityEngine.Random.Range(-1f,1f),transform.position.z), quaternion.identity);
    }
    public bool GetItem(tem item)
    {
        if (ItemList.Count < 6)
        {
            ItemList.Add(item);
            return true;
        }
        else
        {
            return false;
        }
    }
}
