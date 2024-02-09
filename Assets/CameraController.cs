using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Camera camera1;
    [SerializeField] Camera camera2;

    public void Start() {
        camera1.gameObject.SetActive(true);
        camera2.gameObject.SetActive(false);

    }
    [ContextMenu("test1")]
    public void changeCamera() {
        camera1.gameObject.SetActive(!camera1.gameObject.activeSelf);
        camera2.gameObject.SetActive(!camera2.gameObject.activeSelf);

    }
}
