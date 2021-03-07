﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;
using DG.Tweening;
using Pathfinding;
using Photon.Pun;
using Random = UnityEngine.Random;

public class Zombie : MonoBehaviour
{
    private Animator anim;
    private IEnumerator poisonCor;
    private bool canPoison = false;
    private float poisonTime=0;
    public int zombieIndex = 1;
    private TimeManager time;
    public float nightDetecctRad;
    public float detectRad= 5;
    public List<Player> Players=new List<Player>();
    public List<Transform> PlayerTrs=new List<Transform>();
    private IEnumerator corr;
    private PhotonView pv;
    public float AttackTime;
    public float minIdleTime=0.5f;
    public float maxIdleTIme = 2f;
    public float minMove;
    public float maxMove;
    public float speed;
    private Rigidbody2D rigid;
    private Enemy enemy;

    public Path path;
    public float nextWaypointDistance = 3;
    private int currentWaypoint = 0;
    private bool reachedEndOfPath;
    private bool isMaster= false;
    private void Start()
    {
        anim = GetComponent<Animator>();
        pv = GetComponent<PhotonView>();
        rigid = GetComponent<Rigidbody2D>();
        corr = MoveCor();
        enemy = GetComponent<Enemy>();
        //플레이어들 넣어주기
        Player[] players;
        players = FindObjectsOfType<Player>();

        foreach (Player p in players)
        {
            Players.Add(p);
            PlayerTrs.Add(p.GetComponent<Transform>());
        }
        
        time = FindObjectOfType<TimeManager>();

        if (PhotonNetwork.OfflineMode)
        {
         StartCoroutine(corr);
         StartCoroutine(find());
         isMaster = true;
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(corr);
               StartCoroutine(find());
               isMaster = true;
            }
        }

        poisonCor = poisonAttack();
    }

    public void OnPathComplete (Path p) {
        if (isMaster)
        {
            //Debug.Log("A path was calculated. Did it fail with an error? " + p.error);

            if (!p.error) {
                path = p;
                // Reset the waypoint counter so that we start to move towards the first point in the path
                currentWaypoint = 0;
            }   
        }
    }
    public void stopCor()
    {
        StopCoroutine(corr);
    }

    public void nightDetect()
    {
        if (!enemy.isFinding && enemy.canMove)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (!Players[i].isDead)
                { 
                    Transform tr = PlayerTrs[i];
                    if (Vector3.Distance(transform.position, tr.position) < nightDetecctRad)
                    { 
                        enemy.setPlayer(tr);
                        break;
                    }
                }
            }
        }
    }

    void GoPath()
    {
        if (path == null) //경로가 비었으면 
        {
            return; //아무것도 안함
        }
        reachedEndOfPath = false;
        float distanceToWaypoint;
        while (true)
        {
            distanceToWaypoint = Vector3.Distance(transform.position,
                path.vectorPath[currentWaypoint]);
            if (distanceToWaypoint < nextWaypointDistance)
            {
                if (currentWaypoint + 1 < path.vectorPath.Count)
                {
                    currentWaypoint++;
                }
                else
                {
                    reachedEndOfPath = true;
                    break;
                }
            }
            else
            {
                break;
            }
        }
        var speedFactor =
            reachedEndOfPath ? Mathf.Sqrt(distanceToWaypoint / nextWaypointDistance) : 1f;
        Vector3 dir = (path.vectorPath[currentWaypoint] - transform.position).normalized;
        Vector3 velocity = dir * speed * speedFactor;
        rigid.velocity = velocity;

        enemy.setAnim("Walk");
        enemy.setLocalX(enemy.targetPosition.transform.position.x);
        canPoison = false;
    }
    public void Update () 
        {
            if (isMaster)
            {
                if (poisonTime < AttackTime)
                    poisonTime += Time.deltaTime;
              if (!enemy.isFinding && enemy.canMove) 
              {
                  switch (zombieIndex)
                  {
                      case 1:
                      case 2:
                      case 3:
                      case 4:
                          for (int i = 0; i < Players.Count; i++)
                          {
                              if (!Players[i].isDead)
                              {
                                  Transform tr = PlayerTrs[i];
                                  if (tr != null)
                                  {
                                      float rad = detectRad;
                                      if (time.isNight)
                                          rad = nightDetecctRad;
                                      if (Vector3.Distance(transform.position, tr.position) < rad)
                                      {
                                          enemy.setPlayer(tr);
                                          break;
                                      }
                                  }
                              }
                          }
                          break;
                  }
              }
              else
              {
                  if (enemy.isFinding)
                  {
                      if (zombieIndex == 1 || zombieIndex == 4 || zombieIndex == 3)
                      { 
                          GoPath();
                      }
                      else if (zombieIndex == 2)
                      {
                          bool isDetecting = false;
                          Transform tr = null;
                          for (int i = 0; i < Players.Count; i++)
                          {
                              if (!Players[i].isDead)
                              {
                                  tr = PlayerTrs[i];
                                  if (tr != null)
                                  {
                                      if (Vector3.Distance(transform.position, tr.position) < detectRad+1f)
                                      {
                                          isDetecting = true;
                                          break;
                                      }
                                  }
                              }
                          }

                          if (isDetecting)
                          {
                              rigid.velocity=Vector2.zero;
                              enemy.setLocalX(tr.position.x);
                              if (canPoison == false)
                              {
                                  canPoison = true;
                                  StopCoroutine(poisonCor);
                                  StartCoroutine(poisonCor);
                              }
                          }
                          else
                          {
                              StopCoroutine(poisonCor);
                              GoPath();
                          }
                      }
                  }
              }
            }
        }

    IEnumerator poisonAttack()
    {
        while (true)
        {
            if (poisonTime < AttackTime)
            {
                enemy.setAnim("Idle");
            }
            else
            {
                enemy.setAnim("Attack");
                poisonTime = 0;
            }
            
            yield return new WaitUntil(()=>isEndAnim());
            enemy.setAnim("Idle");
            yield return new WaitForSeconds(AttackTime);
        }
    }
    bool isEndAnim()
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f;
    }
    IEnumerator find()
    {
        while (true)
        {
            if (enemy.isFinding)
            {
                enemy.seeker.StartPath(transform.position, enemy.targetPosition.position, OnPathComplete);
            }
            yield return new WaitForSeconds(0.2f);
        }
    }
        
    IEnumerator MoveCor()
    {
        while (true)
        {
            Vector2 roamPos = GetRoamingPosition();
            
            //가려는 방향에 따라 플립
            enemy.setLocalX(roamPos.x);
            

            if (Vector2.Distance(transform.position, roamPos) < 2f)
            {
                enemy.setAnim("Idle");
                
                rigid.velocity=Vector2.zero;
                yield return new WaitForSeconds(Random.Range(minIdleTime,maxIdleTIme));
            }
            else
            {
                enemy.setAnim("Walk");
             
                Vector2 dir =roamPos -  (Vector2)transform.position;
                dir.Normalize();
                float reachedPositionDistance = 0.5f;
                rigid.velocity = dir * speed;
                yield return new WaitUntil(() =>Vector2.Distance( transform.position,roamPos)<reachedPositionDistance);      
            }
        }
    }

    public void OnDisable()
    {
        if(isMaster) 
            enemy.seeker.pathCallback -= OnPathComplete;
    }
    
   private Vector2 GetRoamingPosition()
   {
       float randomMove = Random.Range(minMove, maxMove);
       return transform.position + UtilsClass.GetRandomDir() * randomMove;
   }


   private void OnCollisionStay2D(Collision2D other)
   {
       if (isMaster)
       {
           if (enemy.canMove && !enemy.isFinding)
           {
               if (other.gameObject.CompareTag("Wall")) 
               { 
                   Restart(); 
               }
           }
       }
   }
   
   private void OnTriggerEnter2D(Collider2D other)
   {
       if (isMaster)
       {
           if (enemy.canMove)
           {
               if (other.CompareTag("Player") && enemy.canMove)
               {
                   switch (zombieIndex)
                   {
                       case 1:
                       case 3:
                       case 4:
                           StartCoroutine(Attack());
                           break;
                   }
               }
           }
       }
   }

   void Restart()
   {
       StopCoroutine(corr);
       StartCoroutine(corr);
   }
   
   IEnumerator Attack()
   {
       StopCoroutine(corr);
       enemy.isFinding = false;
       //enemy.canMove = false;
       rigid.velocity=Vector2.zero;

       enemy.setLocalX(enemy.targetPosition.position.x);
       enemy.setAnim("Attack");
       
       yield return new WaitForSeconds(AttackTime);
       enemy.setAnim("Walk");
       //enemy.canMove = true;

       if (enemy.targetPosition.GetComponent<Player>().isDead)
       {
           enemy.ExclamationClose();
           Restart();
       }
       
//       for (int i = 0; i < Players.Count; i++)
//       {
//           if (!Players[i].isDead)
//           {
//               Transform tr = PlayerTrs[i];
//               float rad = detectRad;
//               if (time.isNight)
//                   rad = nightDetecctRad;
//               if (Vector3.Distance(transform.position, tr.position) < rad)
//               {
//                   enemy.isFinding = true;
//                   enemy.targetPosition = tr;
//                   enemy.setAnim("Walk");
//                   break;
//               }
//           }
//       }   
   }

   public void Detect(float rad)
   {
       for (int i = 0; i < Players.Count; i++)
       {
           if (!Players[i].isDead)
           {
               Transform tr = PlayerTrs[i];
               if (tr != null)
               {
                   if (Vector3.Distance(transform.position, tr.position) < rad)
                   {
                       enemy.setPlayer(tr);
                       break;
                   }
               }
           }
       }   
   }
}
