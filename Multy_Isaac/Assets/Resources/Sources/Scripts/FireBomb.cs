using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Photon.Pun;
using UnityEngine;

public class FireBomb : MonoBehaviour
{
    public GameObject fire;
    public Ease easeType;
    private Camera cam;
    public float rotateSpeed;
    private void Start()
    {
        cam = FindObjectOfType<Wheel_Cam>().GetComponent<Camera>();
        transform.DOMove((Vector2)cam.ScreenToWorldPoint(Input.mousePosition), 1.5f).SetEase(easeType).OnComplete(()=>
            {
                if(PhotonNetwork.OfflineMode) 
                    Instantiate(fire,transform.position,Quaternion.identity);
                else 
                    PhotonNetwork.InstantiateRoomObject(fire.name,transform.position,Quaternion.identity);
                Destroy(gameObject);
            });   
    }

    void Update()
    {
        transform.eulerAngles=new Vector3(0,0,transform.eulerAngles.z+rotateSpeed*Time.deltaTime);
    }
}
