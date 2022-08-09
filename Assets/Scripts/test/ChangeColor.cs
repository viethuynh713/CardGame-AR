// using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using Photon.Pun;
using System;

public class ChangeColor : MonoBehaviour
{
    [SerializeField] private Color colorSelected = Color.red;
    [SerializeField] private Camera ARcamera;
    private Vector3 touchPOS;
    private ARRaycastManager arRaycastManager;

    private List<ARRaycastHit> hitAR;
    [SerializeField] private GameObject placedObjectPrefab;
    [SerializeField] private Text notifyTxt;

    void Start()
    {
        // notifyTxt = GameObject.Find("NotifyTxt").GetComponent<Text>();
        hitAR = new List<ARRaycastHit>();
        arRaycastManager = GetComponent<ARRaycastManager>();
        colorSelected = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
    }
    /// <summary>
    /// Sets the color of the selected object to the color selected.
    /// </summary>
    /// <param name="p"></param>
    /// 

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            touchPOS = touch.position;
            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = ARcamera.ScreenPointToRay(touchPOS);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    CubeObject objSelected = hit.collider.GetComponent<CubeObject>();
                    if (objSelected != null)
                    {
                        objSelected.ChangeColor(colorSelected);
                        notifyTxt.text = "Color Changed";
                    }
                    else
                    {
                        SpawnPlacedObject();
                    }
                }
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                Ray ray = ARcamera.ScreenPointToRay(touchPOS);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    CubeObject objSelected = hit.collider.GetComponent<CubeObject>();
                    if (objSelected != null)
                    {
                        if (arRaycastManager.Raycast(touchPOS, hitAR, UnityEngine.XR.ARSubsystems.TrackableType.Planes))
                        {
                            notifyTxt.text = "Move to: " + hitAR[0].pose.position;
                            objSelected.ChangePosition(hitAR[0].pose.position);
                            

                        }
                    }

                }
            }
        }
/*
        if(Input.GetMouseButton(0))
        {
            touchPOS =Input.mousePosition;
            Debug.Log(touchPOS);
            Ray ray = ARcamera.ScreenPointToRay(touchPOS);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && Input.GetMouseButtonDown(0))
            {
                CubeObject objSelected = hit.collider.GetComponent<CubeObject>();
                if (objSelected != null)
                {
                    objSelected.ChangeColor(colorSelected);
                    notifyTxt.text = "Color Changed";
                    
                }
                else
                {
                    SpawnPlacedObject();
                }
            }
            if(!Input.GetMouseButtonUp(0))
            {
            if (Physics.Raycast(ray, out hit))
            {
                CubeObject objSelected = hit.collider.GetComponent<CubeObject>();
                if (objSelected != null)
                {
                
                    if (arRaycastManager.Raycast(touchPOS, hitAR, UnityEngine.XR.ARSubsystems.TrackableType.Planes))
                        {
                            notifyTxt.text = "Move to: " + hitAR[0].pose.position;
                            objSelected.ChangePosition(hitAR[0].pose.position);
                            

                        }
                }

            }
            }
        }
        
        */



    }

    private void SpawnPlacedObject()
    {
        if (arRaycastManager.Raycast(touchPOS, hitAR, UnityEngine.XR.ARSubsystems.TrackableType.Planes))
        {
            notifyTxt.text = "Spawning: " + placedObjectPrefab.name + " at " + hitAR[0].pose.position;
            GameObject obj = PhotonNetwork.Instantiate(placedObjectPrefab.name, hitAR[0].pose.position, Quaternion.identity);
            //obj.transform.SetParent(Camera.main.transform);

        }
    }

}
