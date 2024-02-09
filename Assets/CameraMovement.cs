using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] float speed = 5.0f;
    private Vector2 turn = new Vector2(0,0);
    void Update()
    {   
        move();
        rotate();
    }

    private void rotate() {
        if (Input.GetMouseButton(1)) { 
            Cursor.lockState = CursorLockMode.Locked;  
            turn.x += Input.GetAxis("Mouse X");
            turn.y += Input.GetAxis("Mouse Y");
            transform.localRotation = Quaternion.Euler(-turn.y+transform.localRotation.x,turn.x,0); 
        }
        else Cursor.lockState = CursorLockMode.None;
    }
    private void move() { 
        Vector3 direction = new Vector3(Input.GetAxisRaw("Horizontal"),0,Input.GetAxisRaw("Vertical"));
        direction = transform.localRotation * direction;
        direction.y = 0;
        direction.Normalize();
        this.transform.position += direction * speed * Time.deltaTime;
    }
}
