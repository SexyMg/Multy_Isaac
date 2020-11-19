﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeftBullet : MonoBehaviour
{
    public int bulletCount = 0;
    public GameObject[] bullets;
    public GameObject[] siluettes;
    public float reLoadTime;
    public void SetBullet(int maxBullet)
    {
        bulletCount = maxBullet;
        for (int i = 0; i < bullets.Length; i++)
        {
            if (i < maxBullet)
            {
                siluettes[i].SetActive(true);
                bullets[i].SetActive(true);
            }
            else
            {
                siluettes[i].SetActive(false);
                bullets[i].SetActive(false);
            }
        }
    }

    public bool MinusBullet()
    {
        if (bulletCount <= 0)
        {
            return false;
        }
        else
        {
            bulletCount--;
            for (int i = 0; i < bullets.Length; i++)
            {
                if (i <bulletCount)
                {
                    bullets[i].SetActive(true);
                }
                else
                {
                    bullets[i].SetActive(false);
                }
            }

            return false;
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Z))
            SetBullet(3);
        if(Input.GetKeyDown(KeyCode.X))
            SetBullet(5);
        if(Input.GetKeyDown(KeyCode.C))
            SetBullet(7);
        if(Input.GetKeyDown(KeyCode.V))
            SetBullet(17);
    }
}
