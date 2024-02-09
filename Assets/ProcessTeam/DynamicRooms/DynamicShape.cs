using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



public abstract class DynamicShape : MonoBehaviour{


    public abstract List<Vector3> calcular (List<Vector3> vectors , Vector3 origin);
}


public class Sphere : DynamicShape{

    public Vector3 centro;
    public float radio;

    public override List<Vector3> calcular(List<Vector3> puntosDelPlano, Vector3 size)
    {
        //Calcula el centro del plano como el promedio de los puntos.
        this.centro = CalcularCentroDelPlano(puntosDelPlano);

        Vector3 puntoPlano = new Vector3(this.centro.x,this.centro.y);
        // Paso 2: Calcula el radio como la distancia desde el centro a cualquiera de los puntos.
        this.radio = CalcularRadio(centro, puntoPlano);

        // Paso 3: Genera puntos en la media esfera utilizando coordenadas esféricas.
        List<Vector3> puntosEnLaMediaEsfera = new List<Vector3>();

        int numPasosPhi = (int)size.y; // Número de pasos para Phi (ángulo vertical).
        int numPasosTheta = (int)size.x; // Número de pasos para Theta (ángulo horizontal).

       for (int i = 0; i <= numPasosPhi; i++)
        {
            float phi = Mathf.PI  * i / numPasosPhi; // Ángulo vertical de 0 a π/2.

            for (int j = 0; j <= numPasosTheta; j++)
            {
                float theta = Mathf.PI * j / numPasosTheta; // Ángulo horizontal de 0 a 2π.
                float x = centro.x + radio * Mathf.Sin(phi) * Mathf.Cos(theta);
                float y = centro.y + radio * Mathf.Sin(phi) * Mathf.Sin(theta);
                float z = centro.z + radio * Mathf.Cos(phi);

                puntosEnLaMediaEsfera.Add(new Vector3(x, y, z));
            }
        }

        return puntosEnLaMediaEsfera;
    }


    private Vector3 CalcularCentroDelPlano(List<Vector3> puntosDelPlano)
    {
        Vector3 centro = Vector3.zero;

        foreach (Vector3 punto in puntosDelPlano)
        {
            centro += punto;
        }
        centro /= puntosDelPlano.Count;
        return centro;
    }

    private float CalcularRadio(Vector3 centro, Vector3 puntoEnElPlano)
    {
        return Vector3.Distance(centro, puntoEnElPlano);
    }
}

public class terrain : DynamicShape
{
    public float mountainHeight = 1.0f; // Altura de la montaña.
    public float perlinScale = 0.8f; // Escala para el ruido de Perlin.
    public float perlinHeightScale = 24.0f; // Escala para la altura del terreno basada en el ruido de Perlin.
    public float smoothDistance = 20.0f; // Distancia a partir de los bordes para suavizar
    public float resolucion = 1.0f;
    public override List<Vector3> calcular(List<Vector3> vertices, Vector3 size)
{
    List<Vector3> modifiedVertices = new List<Vector3>();
    for (int i = 0; i < vertices.Count; i++)
    {

        Vector3 vertex = vertices[i];

        float xCoord = vertex.x * perlinScale;
        float zCoord = vertex.z * perlinScale;
        float perlinValue = Mathf.PerlinNoise(xCoord, zCoord);

        float elevation = perlinValue * perlinHeightScale;

        // Aplica la elevación al vértice
        Vector3 modifiedVertex;

        
        modifiedVertex = vertex + Vector3.up * (elevation + UnityEngine.Random.Range(-0.001f,0.001f));

        //hardcode de montaña
        float anchoMontain = (size.x - (size.x / 10)) - (size.x - (size.x / 4));
        float mountainRadius = anchoMontain / 2;
        float centerMontain = ((size.x - (size.x / 4)) + (size.x - (size.x / 10)))/ 2;
        float distanceToCenter = Math.Abs(centerMontain - vertex.x);

        if (distanceToCenter<=mountainRadius && mountainHeight >1)
        {
            
            float aux = elevation * mountainHeight*(1.0f-(distanceToCenter/mountainRadius));
            modifiedVertex = vertex + (Vector3.up * aux);
        }

        // Ajusta la altura de los vértices cerca de los bordes para que tiendan a ser planos.
        float distanceToLeftEdge = (float)vertex.x;
        float distanceToRightEdge = Math.Abs((float)vertex.x - (size.x*resolucion));
        float distanceToTopEdge = (size.y*resolucion) - (float)vertex.z;
        float distanceToBottomEdge = (float)vertex.z;
        float minDistance = Mathf.Min(distanceToLeftEdge, distanceToRightEdge, distanceToTopEdge, distanceToBottomEdge);
        
        

        if (minDistance < smoothDistance)
        {
                if (minDistance != 0f)
                {
                    float borderElevation = elevation * (1.0f - (1 - (float)Math.Log((minDistance / smoothDistance) + 1)));
                    modifiedVertex.y = borderElevation;
                }else modifiedVertex.y = 0f;                                                           
        }

        modifiedVertices.Add(modifiedVertex);
    }

    return modifiedVertices; 
}

    public void setPerlinScale(float perlinScale)
    {
        this.perlinScale = perlinScale;
    }

    public void setPerlinHighScale (float perlinHighScale)
    {
        this.perlinHeightScale = perlinHighScale;
    }

    public void setMountainHeight(float height)
{
    this.mountainHeight = height;
}
}