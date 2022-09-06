// using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using Photon.Pun;
using UnityEngine.XR.ARSubsystems;
using Photon.Realtime;
using System;

public class ChangeColor : MonoBehaviourPunCallbacks
{
    [SerializeField] private Color colorSelected = Color.red;
    [SerializeField] private Camera ARcamera;
    private Vector3 touchPOS;
    private ARRaycastManager arRaycastManager;

    private List<ARRaycastHit> hitAR;
    [SerializeField] private GameObject placedObjectPrefab;
    [SerializeField] private Text notifyTxt;
    [SerializeField] private GameObject cameraOffset;
    [SerializeField] private PhotonView view;
    [SerializeField] private ARAnchorManager arAnchorManager;
    [SerializeField] private ARPlaneManager arPlaneManager;
    [SerializeField] private GameObject flagPrefab;


    public TrackableId anchorPosID;
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            //view.RPC(nameof(SetCameraOffset), newPlayer, BitConverter.GetBytes(anchorPosID.subId1), BitConverter.GetBytes(anchorPosID.subId2));
        }
    }
/*    [PunRPC]
    public void SetCameraOffset(byte[] id1, byte[] id2)
    {
        anchorPosID = new TrackableId(BitConverter.ToUInt64(id1), BitConverter.ToUInt64(id2));
        notifyTxt.text = "Set ID successfully";
        isHasID = true;

    }*/
    
    
    void Start()
    {
       
        hitAR = new List<ARRaycastHit>();
        arRaycastManager = GetComponent<ARRaycastManager>();
        colorSelected = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
        
    }
    bool isSetAnchor = false;
    [PunRPC] 
    public void SetNewTransform(Vector3 position, Vector3 rotation)
    {
        cameraOffset.transform.position = ARcamera.transform.localPosition - position;
        cameraOffset.transform.eulerAngles = ARcamera.transform.localEulerAngles - rotation;
        notifyTxt.text += "Set CamOffset successfully";
    }
    public void ResolveAnchor()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            var pos = ARcamera.transform.localPosition;
            var rot = ARcamera.transform.localEulerAngles;
            view.RPC(nameof(SetNewTransform), RpcTarget.Others, pos, rot);
            notifyTxt.text = "Send offset";
        }
    }
    bool checkAnchor = false;
    bool isHasID = false;
    private void Update()
    {
/*        if (!isSetAnchor && isHasID)
        {
            var anchorPos = arAnchorManager.GetAnchor(anchorPosID);
            if (anchorPos != null)
            {
                notifyTxt.text += " - O";
                cameraOffset.transform.position = anchorPos.transform.position;
                cameraOffset.transform.rotation = anchorPos.transform.rotation;
                Instantiate(flagPrefab, anchorPos.transform);
                notifyTxt.text += "K";
                isSetAnchor = true;
            }

        }
        //notifyTxt.text = PhotonNetwork.IsMasterClient+"--"+anchorPosID.subId1 +"--"+ anchorPosID.subId2;
        if (PhotonNetwork.IsMasterClient && !checkAnchor)
        {
            if (arRaycastManager.Raycast(Camera.main.ViewportToScreenPoint(new Vector2(0.5f, 0.5f)), hitAR, TrackableType.Planes))
            {

                var hitPose = hitAR[0].pose;
                var hitTrackableId = hitAR[0].trackableId;
                var hitPlane = arPlaneManager.GetPlane(hitTrackableId);
                var anchor = arAnchorManager.AttachAnchor(hitPlane, hitPose);

                if (anchor == null)
                {
                    notifyTxt.text = "Error creating anchor.";
                }
                else
                {
                    anchorPosID = anchor.trackableId;
                    notifyTxt.text = "Creating anchor.";
                    checkAnchor = true;
                    SetCameraOffset(BitConverter.GetBytes(anchorPosID.subId1), BitConverter.GetBytes(anchorPosID.subId2));
                }
            }*/
            
        //}
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
    public void GotoMenu()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel("Menu");
    }

}
