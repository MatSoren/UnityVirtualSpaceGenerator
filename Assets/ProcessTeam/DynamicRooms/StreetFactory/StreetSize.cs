using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StreetSize : MonoBehaviour
{
    private float anchoDeCalle;
    private float largoDeCalle;
    private Vector2 p1;
    private Vector2 p2;
    private Vector2 p3;
    private Vector2 p4;
    private float vectorOffset1 = 0.1F;
    private float vectorOffset2 = 0.2F;

    public void setDimensiones(float x, float y) {
        this.anchoDeCalle = x;
        this.largoDeCalle = y;
        Vector2 proporcionXYv1 = new Vector2((float) Math.Sin(this.transform.localEulerAngles.y * Mathf.Deg2Rad),(float) Math.Cos(this.transform.localEulerAngles.y * Mathf.Deg2Rad));
        Vector2 proporcionXYv2 = new Vector2((float) Math.Cos(-this.transform.localEulerAngles.y * Mathf.Deg2Rad),(float) Math.Sin(-this.transform.localEulerAngles.y * Mathf.Deg2Rad));
        
        this.p1 = new Vector2(this.transform.position.x+vectorOffset1,this.transform.position.z+vectorOffset1);
        this.p2 = p1 + proporcionXYv1 * largoDeCalle - new Vector2(vectorOffset2,vectorOffset2);
        this.p3 = p1 + proporcionXYv2 * anchoDeCalle;
        this.p4 = p2 + proporcionXYv2 * anchoDeCalle;
    }

    public List<Vector2> getVectores() {
        List<Vector2> aux = new List<Vector2>();
        aux.Add(this.p1);
        aux.Add(this.p2);
        aux.Add(this.p3);
        aux.Add(this.p4);
        return aux;
    }

    public float getLargoCalle() {
        return this.largoDeCalle;
    }

    public float getAnchoCalle() {
        return this.anchoDeCalle;
    }

    public float GetVectorOffset1(){
        return this.vectorOffset1;
    }

    public float GetVectorOffset2(){
        return this.vectorOffset2;
    }

}

