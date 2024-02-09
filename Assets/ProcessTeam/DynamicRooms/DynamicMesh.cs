using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]

public class DynamicMesh : MonoBehaviour {
    Mesh myMesh;
    MeshFilter meshFilter;

    bool tieneMeshCollider = false;
    MeshCollider meshCollider = null;

    public Vector2 planeSize = new Vector2(1,1);
    Vector2 repeticionDeMaterial = new Vector2(1,1);
    int cantidadDePoligonos = 1;
    float resolucion;
    bool[,] validos;
    bool invertirPlano = false;

    private Dictionary<string,Type> _shapeByName;

    void Awake() {
        this.resolucion = 1 / cantidadDePoligonos;
        myMesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = myMesh;
        meshCollider = GetComponent<MeshCollider>();
        if (meshCollider != null)
            this.tieneMeshCollider = true;

        _shapeByName = new Dictionary<string,Type> {
                {"mediaesfera", typeof(Sphere)},
                {"terrain", typeof(terrain)},
            };

    }
    List<int> triangulos;
    List<Vector3> vertices;
    List<Vector2> uv;

    public void setCantDePoligonos(int cant) {
        this.cantidadDePoligonos = cant;
        setPlaneSize(planeSize.x * resolucion,planeSize.y * resolucion);
    }


    public void setPlaneSize(float x,float y) {
        planeSize.x = (float) (x / resolucion);
        planeSize.y = (float) (y / resolucion);
        validos = new bool[(int) planeSize.y,(int) planeSize.x];
        for (int i = 0; i < planeSize.y; i++)
            for (int j = 0; j < planeSize.x; j++)
                validos[i,j] = true;
        generatePlane();
        actualizarMesh();
    }


    private void actualizarMesh() {
        myMesh.Clear();
        myMesh.vertices = vertices.ToArray();
        myMesh.triangles = triangulos.ToArray();
        myMesh.uv = uv.ToArray();
        myMesh.normals = calcularNomrales();
        myMesh.RecalculateBounds();
        myMesh.RecalculateTangents();
        myMesh.UploadMeshData(false);
        if (tieneMeshCollider)
            this.meshCollider.sharedMesh = myMesh;

    }

    private Vector3[] calcularNomrales() {
        Vector3[] normales = new Vector3[this.vertices.Count];
        int triangulos = this.triangulos.Count / 3;
        for (int i = 0; i < triangulos; i++) {
            int indiceTriangulo = i * 3;
            int indiceVerticeA = this.triangulos[indiceTriangulo];
            int indiceVerticeB = this.triangulos[indiceTriangulo + 1];
            int indiceVerticeC = this.triangulos[indiceTriangulo + 2];

            Vector3 normalDeTriangulo = generaNormal(indiceVerticeA,indiceVerticeB,indiceVerticeC);

            normales[indiceVerticeA] += normalDeTriangulo;
            normales[indiceVerticeB] += normalDeTriangulo;
            normales[indiceVerticeC] += normalDeTriangulo;
        }

        for (int i = 0; i < normales.Length; i++) {
            normales[i].Normalize();
        }

        return normales;

    }

    private Vector3 generaNormal(int a,int b,int c) {
        Vector3 puntoA = this.vertices[a];
        Vector3 puntoB = this.vertices[b];
        Vector3 puntoC = this.vertices[c];

        Vector3 AB = puntoB - puntoA;
        Vector3 AC = puntoC - puntoA;
        return Vector3.Cross(AB,AC).normalized;
    }

    public void setInvertirPlano(bool set) {
        Vector3[] normales = new Vector3[myMesh.normals.Length];
        this.invertirPlano = set;
        generatePlane();
        actualizarMesh();
    }

    public void rotarMaterial()
    {
    uv = new List<Vector2>();
    for (int col = 0; col < (int)this.planeSize.y + 1; col++)
    {
        for (int fil = 0; fil < (int)this.planeSize.x + 1; fil++)
        {
            uv.Add(new Vector2((col / this.planeSize.y * this.repeticionDeMaterial.y), ((fil / this.planeSize.x * this.repeticionDeMaterial.x))));
        }
    }
    actualizarMesh();
    }

    public void setTriangulos() {

        triangulos = new List<int>();
        if (!this.invertirPlano) {
            for (int fil = 0; fil < (int) this.planeSize.y; fil++)
                for (int col = 0; col < (int) this.planeSize.x; col++)
                    if (validos[fil,col]) {
                        int i = (int) (fil * (this.planeSize.x)) + fil + col;
                        triangulos.Add(i);
                        triangulos.Add(i + (int) this.planeSize.x + 1);
                        triangulos.Add(i + (int) this.planeSize.x + 2);

                        triangulos.Add(i);
                        triangulos.Add(i + (int) this.planeSize.x + 2);
                        triangulos.Add(i + 1);
                    }
        }
        else {
            for (int fil = 0; fil < (int) this.planeSize.y; fil++)
                for (int col = 0; col < (int) this.planeSize.x; col++)
                    if (validos[fil,col]) {
                        int i = (int) (fil * (this.planeSize.x)) + fil + col;
                        triangulos.Add(i);
                        triangulos.Add(i + (int) this.planeSize.x + 2);
                        triangulos.Add(i + (int) this.planeSize.x + 1);

                        triangulos.Add(i);
                        triangulos.Add(i + 1);
                        triangulos.Add(i + (int) this.planeSize.x + 2);

                    }
        }
        actualizarMesh();
    }

    public void generatePlane() {
        vertices = new List<Vector3>();
        uv = new List<Vector2>();
        for (int col = 0; col < (int) this.planeSize.y + 1; col++) {
            for (int fil = 0; fil < (int) this.planeSize.x + 1; fil++) {
                vertices.Add(new Vector3(fil * resolucion,0,col * resolucion));
                uv.Add(new Vector2((fil / this.planeSize.x * this.repeticionDeMaterial.x),(col / this.planeSize.y * this.repeticionDeMaterial.y)));
            }
        }

        setTriangulos();
        actualizarMesh();
    }

    public void removerAreaMesh(float xPos,float zPos,float xSize,float zSize) {
        xPos *= (int) (1 / this.resolucion);
        zPos *= (int) (1 / this.resolucion);
        xSize *= (int) (1 / this.resolucion);
        zSize *= (int) (1 / this.resolucion);
        for (int i = 0; i < xSize; i++)
            for (int j = 0; j < zSize; j++) {
                if (validXY((int) zPos + j,(int) xPos + i))
                    validos[(int) xPos + i,(int) zPos + j] = false;
            }
        setTriangulos();
    }
    public void llenarAreaMesh(float xPos,float zPos,float xSize,float zSize) {
        xPos *= (int) (1 / this.resolucion);
        zPos *= (int) (1 / this.resolucion);
        xSize *= (int) (1 / this.resolucion);
        zSize *= (int) (1 / this.resolucion);
        for (int i = 0; i < xSize; i++)
            for (int j = 0; j < zSize; j++) {
                validos[(int) xPos + i,(int) zPos + j] = true;
            }
        setTriangulos();
    }

    public bool hayMesh(float xPos,float zPos) {
        xPos *= (int) (1 / this.resolucion);
        zPos *= (int) (1 / this.resolucion);
        return validos[(int) xPos,(int) zPos];
    }

    //Dibujos dinamicos en el plano
    public void planeSin(string shape) {
        if (shape != "plano") {
            var e = Activator.CreateInstance(_shapeByName[shape]) as DynamicShape;
            Vector3 size = new Vector3(this.planeSize.x,this.planeSize.y);
            this.vertices = e.calcular(this.vertices,size);
            actualizarMesh();
        }
    }

    public void terrain(float perlin,float perlinHigh,float mountain) {

    var e = Activator.CreateInstance(_shapeByName["terrain"]) as terrain;
    Vector3 size = new Vector3(this.planeSize.x,this.planeSize.y);
    e.setPerlinScale(perlin);
    e.setPerlinHighScale(perlinHigh);
    e.setMountainHeight(mountain);
    e.resolucion = this.resolucion;
    this.vertices = e.calcular(this.vertices,size);
    actualizarMesh();

}

    public Vector3[] getVerticesEnArea(float xPos,float zPos,float xSize,float zSize) {

        xPos *= (int) (1 / this.resolucion);
        zPos *= (int) (1 / this.resolucion);
        xSize *= (int) (1 / this.resolucion);
        zSize *= (int) (1 / this.resolucion);
        List<Vector3> verticesAux = new List<Vector3>();
        List<List<Vector3>> listaDeListas = new List<List<Vector3>>();
        for (int j = 0; j < zSize + 1; j++) {
            for (int i = 0; i < xSize + 1; i++) {
                verticesAux.Add(this.vertices[(int) (xPos + j) * (int) (this.planeSize.x + 1) + (int) (zPos + i)]);
            }
            //listaDeListas.Add(verticesAux);
            //verticesAux = new List<Vector3>();
        }
        //List<Vector3> auxListas = new List<Vector3>();
        //for (int i = 0; i < listaDeListas.Count; i++) {
        //    auxListas = listaDeListas[i];
        //    for (int j = auxListas.Count - 1; j >= 0; j--)
        //        verticesAux.Add(auxListas[j]);
        //}
        //generatePlane();
        return verticesAux.ToArray();
        
    }

    public Vector3[] getVerticesEnArea90(float xPos,float zPos,float xSize,float zSize) {

        xPos *= (int) (1 / this.resolucion);
        zPos *= (int) (1 / this.resolucion);
        xSize *= (int) (1 / this.resolucion);
        zSize *= (int) (1 / this.resolucion);
        List<Vector3> verticesAux = new List<Vector3>();
        List<List<Vector3>> listaDeListas = new List<List<Vector3>>();
        for (int j = 0; j < zSize + 1; j++) {
            for (int i = 0; i < xSize + 1; i++) {                
                verticesAux.Add(this.vertices[(int) (xPos + i) * (int) (this.planeSize.x + 1) + (int) (zPos + j)]);
            }
            listaDeListas.Add(verticesAux);
            verticesAux = new List<Vector3>();
        }
        List<Vector3> auxListas = new List<Vector3>();
        for (int i = 0;i<listaDeListas.Count;i++) {
            auxListas = listaDeListas[i];
            for (int j = auxListas.Count - 1; j >= 0; j--)
                verticesAux.Add(auxListas[j]);
        }
        
        return verticesAux.ToArray();

    }

    public void setAlturasVertices(Vector3[] v) {
        for(int i=0; i<this.vertices.Count; i++) {
            this.vertices[i] = new Vector3(this.vertices[i].x,v[i].y,this.vertices[i].z);
        }

        setTriangulos();
        actualizarMesh();
    }


    public void setRepeticiones(float x, float y)
    {
        this.repeticionDeMaterial.x = x;
        this.repeticionDeMaterial.y = y;
        generatePlane();
    }

    public void restaurarMesh()
    {
        for (int fil = 0; fil < (int)this.planeSize.y; fil++)
            for (int col = 0; col < (int)this.planeSize.x; col++)
                validos[fil, col] = true;
        setTriangulos();
    }

    public void AdaptarCalleAlTerreno()
    {

        //for (int i = 0; i < this.vertices.Count; i++)
        //{
        //    Vector3 calleVertice = this.vertices[i];
        //    Vector3 inicioRaycast = this.transform.position + this.transform.rotation * (calleVertice + Vector3.up * 50); //raycast por encima del v�rtice

        //    RaycastHit hit;

        //    if (Physics.Raycast(inicioRaycast, Vector3.down, out hit, 100f))
        //    {
        //        calleVertice.y = hit.point.y + 0.02f;
        //        this.vertices[i] = calleVertice;
        //    }
        //}
        //actualizarMesh();
    }
    private bool validXY(float x,float y) {
        return this.planeSize.x > x && this.planeSize.y > y;
    }
}

