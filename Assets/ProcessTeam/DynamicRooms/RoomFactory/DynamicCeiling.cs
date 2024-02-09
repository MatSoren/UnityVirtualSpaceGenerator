using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public abstract class DynamicCeiling : MonoBehaviour{

    public abstract GameObject createInstanceCeiling(GameObject prefab,Transform pos,float xSize, float zSize);
}


public class mediaEsfera : DynamicCeiling{

    GameObject ceiling;

    public override GameObject createInstanceCeiling(GameObject prefab, Transform pos, float xSize, float zSize)
    {
        this.ceiling = new GameObject("Techo");
        this.ceiling.transform.Translate (pos.position);
        Vector3 posicion = new Vector3(pos.position.x,pos.position.y,pos.position.z);
        GameObject planoext = Instantiate(prefab,pos.position,Quaternion.identity);
        planoext.GetComponent<DynamicMesh>().setPlaneSize(xSize,zSize);
        planoext.GetComponent<DynamicMesh>().setRepeticiones(xSize,zSize);
        planoext.GetComponent<DynamicMesh>().setInvertirPlano(false);
        planoext.transform.name = "planoext";
        planoext.transform.SetParent(this.ceiling.transform);

        GameObject planoint = Instantiate(prefab, pos.position, Quaternion.identity);
        planoint.GetComponent<DynamicMesh>().setPlaneSize(xSize, zSize);
        planoint.GetComponent<DynamicMesh>().setRepeticiones(xSize, zSize);
        planoint.GetComponent<DynamicMesh>().setInvertirPlano(true);
        planoint.transform.name = "planoint";
        planoint.transform.SetParent(this.ceiling.transform);

        GameObject esfera = Instantiate(prefab,pos.position,Quaternion.identity);
        esfera.GetComponent<DynamicMesh>().setPlaneSize(xSize,zSize);
        esfera.GetComponent<DynamicMesh>().setRepeticiones(xSize,zSize);
        esfera.GetComponent<DynamicMesh>().setInvertirPlano(false);
        esfera.GetComponent<DynamicMesh>().planeSin("mediaesfera");
        esfera.transform.name = "mediaesfera";

        esfera.transform.SetParent(this.ceiling.transform);

        return this.ceiling;
    }


}

public class plano : DynamicCeiling
{
    public override GameObject createInstanceCeiling(GameObject prefab, Transform pos, float xSize, float zSize)
    {
        GameObject ceiling = Instantiate(prefab,pos.position,Quaternion.identity);
        ceiling.GetComponent<DynamicMesh>().setPlaneSize(xSize,zSize);
        ceiling.GetComponent<DynamicMesh>().setRepeticiones(xSize,zSize);
        ceiling.GetComponent<DynamicMesh>().setInvertirPlano(true);
        ceiling.transform.name = "Techo";
        return ceiling;
    }
}