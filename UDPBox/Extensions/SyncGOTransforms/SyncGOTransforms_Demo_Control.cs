using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hont.UDPBoxExtensions
{
    public class SyncGOTransforms_Demo_Control : MonoBehaviour
    {
        void Update()
        {
            if (Input.GetKey(KeyCode.Return))
            {
                transform.gameObject.SetActive(!transform.gameObject.activeSelf);
            }

            if (Input.GetKey(KeyCode.W))
            {
                transform.Translate(new Vector3(0f, 1f, 0f) * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.S))
            {
                transform.Translate(new Vector3(0f, -1f, 0f) * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.A))
            {
                transform.Translate(new Vector3(1f, 0f, 0f) * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.D))
            {
                transform.Translate(new Vector3(-1f, 0f, 0f) * Time.deltaTime);
            }
        }
    }
}
