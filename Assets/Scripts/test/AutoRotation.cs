using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotation : MonoBehaviour
{
   
    // Update is called once per frame
    void Update()
    {
        transform.localEulerAngles = new Vector3(0, 0, Time.deltaTime*10 + transform.eulerAngles.z);
    }
}
