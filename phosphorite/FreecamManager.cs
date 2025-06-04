using GorillaLocomotion;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace phosphorite
{
    //someone fix this ok tysm

    /*public class FreecamManager : MonoBehaviour
    {
        public float speed = 2f;
        GameObject cameraObject;

        float pitch = 0f;
        float yaw = 0f;

        public void Awake()
        {
            if(cameraObject != null)
            {
                cameraObject = 
            }
        }

        public void FixedUpdate()
        {
            if (Mouse.current.rightButton.isPressed)
            {
                Vector2 vector = Mouse.current.delta.ReadValue();
                yaw += vector.x * 2f;
                pitch -= vector.y * 2f;
                pitch = Mathf.Clamp(pitch, -89f, 89f);
                cameraObject.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
            }

            if (Keyboard.current.wKey.isPressed)
                cameraObject.transform.position += cameraObject.transform.forward * speed * Time.deltaTime;
            if (Keyboard.current.sKey.isPressed)
                cameraObject.transform.position -= cameraObject.transform.forward * speed * Time.deltaTime;
            if (Keyboard.current.aKey.isPressed)
                cameraObject.transform.position -= cameraObject.transform.right * speed * Time.deltaTime;
            if (Keyboard.current.dKey.isPressed)
                cameraObject.transform.position += cameraObject.transform.right * speed * Time.deltaTime;

            if (Keyboard.current.spaceKey.isPressed)
                cameraObject.transform.position += cameraObject.transform.up * speed * Time.deltaTime;
            if (Keyboard.current.leftCtrlKey.isPressed)
                cameraObject.transform.position -= cameraObject.transform.up * speed * Time.deltaTime;
        }
    }*/
}
