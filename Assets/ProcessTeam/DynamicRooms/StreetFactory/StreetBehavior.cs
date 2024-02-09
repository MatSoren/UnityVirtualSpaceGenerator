using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProcessTeam.DynamicRooms.BuildingManager;
using System;

public class StreetBehavior : MonoBehaviour
{

    float anchoVeredas;    
    private GameObject dynSidewalkR;
    private GameObject dynSidewalkL;
    List<Vector2> vectores;
    Dictionary<int,DynamicBuilding> buildings = new Dictionary<int, DynamicBuilding>();
    
    public int reaccionarInterseccion(GameObject calleIntersecada) {
        //vi y vj son los vectores correspondientes a la calle instanciada y con la q se quiera comparar respectivamente.
        //  recordar q vectores en unity son enrealidad puntos.
        float anchoVeredasInter = calleIntersecada.GetComponent<StreetBehavior>().getAnchoVeredas();
        float largo = calleIntersecada.GetComponent<StreetSize>().getLargoCalle();
        float anchoCalleInter = calleIntersecada.GetComponent<StreetSize>().getAnchoCalle();
        float anchoMio = gameObject.GetComponent<StreetSize>().getAnchoCalle();
        //Debug.Log("Soy "+gameObject.name+" La calle: "+calleIntersecada+" es: "+largo+" de largo y "+anchoCalleInter+" de ancho");
        GameObject calleInterSideR = calleIntersecada.GetComponent<StreetBehavior>().GetSidewalkR();
        GameObject calleInterSideL = calleIntersecada.GetComponent<StreetBehavior>().GetSidewalkL();
        float offset1 = gameObject.GetComponent<StreetSize>().GetVectorOffset1();
        float offset2 = gameObject.GetComponent<StreetSize>().GetVectorOffset2();
        List<Vector2> vi = this.vectores;
        List<Vector2> vj = calleIntersecada.GetComponent<StreetSize>().getVectores();
        if(IntersecaHaciaDerecha(vi,vj)) // si a mi calle la cortan hacia la derecha entonces su StreetBehavior hace los cambios
            return calleIntersecada.GetComponent<StreetBehavior>().reaccionarInterseccion(gameObject);
        else{
            Vector2 inicio;
            Vector2 fin;
            bool inter0101 = intersecan(vi[0],vi[1],vj[0],vj[1]);
            bool inter0123 = intersecan(vi[0],vi[1],vj[2],vj[3]);
            bool inter2301 = intersecan(vi[2],vi[3],vj[0],vj[1]);
            bool inter2323 = intersecan(vi[2],vi[3],vj[2],vj[3]);

            //opero izq-izq y luego izq-der
            inicio=new Vector2(0F,0F);
            if(!inter0101 && inter0123) //T INF IZQ
            {
                //Debug.Log("!inter0101 && inter0123");
                inicio = new Vector2(0F,0F);
                fin = definirPuntoLocal(vi[0],vi[1],vj[2],vj[3],anchoVeredasInter+offset1,0);
                borrarVeredas(gameObject,this.dynSidewalkL,inicio,fin,true);
                borrarInterseccion(gameObject,inicio,fin,0,anchoMio,true);
            }
            if(inter0101 && !inter0123) //T SUP IZQ
            {
                //Debug.Log("inter0101 && !inter0123");
                inicio = definirPuntoLocal(vi[0],vi[1],vj[0],vj[1],-offset2-anchoVeredasInter,0);
                fin = vi[1] - vi[0];
                borrarVeredas(gameObject,this.dynSidewalkL,inicio,fin,true);
                borrarInterseccion(gameObject,inicio,fin,0,anchoMio,true);
            }
            
            if(!inter2301 && inter2323) //T INF DER
            {
                //Debug.Log("!inter2301 && inter2323");
                inicio = new Vector2(0F,0F);
                fin = definirPuntoLocal(vi[2],vi[3],vj[2],vj[3],anchoVeredasInter+offset1,0);
                borrarVeredas(gameObject,this.dynSidewalkR,inicio,fin,true);
                borrarInterseccion(gameObject,inicio,fin,0,anchoMio,true);
            }
            if(inter2301 && !inter2323) //T SUP DER
            {
                //Debug.Log("inter2301 && !inter2323");
                inicio = definirPuntoLocal(vi[2],vi[3],vj[0],vj[1],-offset2-anchoVeredasInter,0);
                fin = vi[3] - vi[2];
                borrarVeredas(gameObject,this.dynSidewalkR,inicio,fin,true);
                borrarInterseccion(gameObject,inicio,fin,0,anchoMio,true);
            }

            if(inter2301 && inter2323 && inter0101 && inter0123) //DER-DER & DER-IZQ & IZQ-IZQ & IZQ-DER
            {
                //Debug.Log("inter2301 && inter2323");
                inicio = definirPuntoLocal(vi[2],vi[3],vj[0],vj[1],-offset2-anchoVeredasInter,0);
                fin = definirPuntoLocal(vi[2],vi[3],vj[2],vj[3],anchoVeredasInter+offset1,0);
                borrarVeredas(gameObject,this.dynSidewalkR,inicio,fin,true);
                borrarInterseccion(gameObject,inicio,fin,0,anchoMio,true);

                //Debug.Log("inter0101 && inter0123");
                inicio = definirPuntoLocal(vi[0],vi[1],vj[0],vj[1],-offset2-anchoVeredasInter,0);
                fin = definirPuntoLocal(vi[0],vi[1],vj[2],vj[3],anchoVeredasInter+offset1,0);
                borrarVeredas(gameObject,this.dynSidewalkL,inicio,fin,true);
                borrarInterseccion(gameObject,inicio,fin,0,anchoMio,true);
            }

            //ESQUINAS que tocan solo una
            if(inter0101 && !inter0123 && !inter2301 && !inter2323){ 
                inicio = new Vector2(0F,0F);
                fin = definirPuntoLocal(vj[0],vj[1],vi[0],vi[1],0,offset1+this.anchoVeredas);
                borrarVeredas(calleIntersecada,calleInterSideL,inicio,fin,false);
                borrarInterseccion(calleIntersecada, inicio, fin, 0, anchoVeredasInter,false);
                return 0;
            }

            if(inter0123 && !inter0101 && !inter2301 && !inter2323){ 
                inicio = new Vector2(0F,0F);
                fin = definirPuntoLocal(vj[2],vj[3],vi[0],vi[1],0,offset1+this.anchoVeredas);
                borrarVeredas(calleIntersecada,calleInterSideR,inicio,fin,false);
                borrarInterseccion(calleIntersecada, inicio, fin, anchoCalleInter - anchoVeredasInter, anchoCalleInter,false);
                return 0;
            }

            if(inter2323 && !inter0101 && !inter0123 && !inter2301){ 
                inicio = definirPuntoLocal(vj[2],vj[3],vi[2],vi[3],0,-offset2-this.anchoVeredas);
                fin = vj[3] - vj[2];
                borrarVeredas(calleIntersecada,calleInterSideR,inicio,fin,false);
                borrarInterseccion(calleIntersecada, inicio, fin, anchoCalleInter - anchoVeredasInter, anchoCalleInter,false);
                return 0;
            }

            if(inter2301 && !inter0101 && !inter0123 && !inter2323){ 
                fin = vj[1] - vj[0];
                inicio = definirPuntoLocal(vj[0],vj[1],vi[2],vi[3],0,-offset2-this.anchoVeredas);
                borrarVeredas(calleIntersecada,calleInterSideL,inicio,fin,false);
                borrarInterseccion(calleIntersecada, inicio, fin, 0, anchoVeredasInter,false);
                return 0;
            }

            // T der es cuando hay IZQ-DER, IZQ-IZQ PERO NO DER-DER NI DER-IZQ es una T inf de la otra calle
            if(inter0101 && !inter2301 && inter0123 && !inter2323){ 
                //Debug.Log("T der");
                inicio = new Vector2(0F,0F);
                fin = definirPuntoLocal(vj[2],vj[3],vi[0],vi[1],0,offset1+this.anchoVeredas);
                borrarVeredas(calleIntersecada,calleInterSideR,inicio,fin,false);

                fin = definirPuntoLocal(vj[0],vj[1],vi[0],vi[1],0,offset1+this.anchoVeredas);
                borrarVeredas(calleIntersecada,calleInterSideL,inicio,fin,false);
                borrarInterseccion(calleIntersecada, inicio, fin, 0, anchoCalleInter,false);

                inicio = definirPuntoLocal(vi[2],vi[3],vj[0],vj[1],-offset2-anchoVeredasInter,0);
                fin = definirPuntoLocal(vi[2],vi[3],vj[2],vj[3],anchoVeredasInter+offset1,0);
                borrarVeredas(gameObject,this.dynSidewalkL,inicio,fin,true);
                borrarInterseccion(gameObject,inicio,fin,0, this.anchoVeredas,true);
                return 0;
            }
            
            // T izq es cuando hay DER-DER, DER-IZQ pero no IZQ-IZQ ni IZQ-DER
            if(inter2301 && !inter0101 && inter2323 && !inter0123){
                //Debug.Log("T izq");

                inicio = definirPuntoLocal(vj[2],vj[3],vi[2],vi[3],0,-offset2-this.anchoVeredas);
                fin = vj[3] - vj[2];
                borrarVeredas(calleIntersecada,calleInterSideR,inicio,fin,false);

                inicio = definirPuntoLocal(vj[0],vj[1],vi[2],vi[3],0,-offset2-this.anchoVeredas);
                fin = vj[1] - vj[0];
                borrarVeredas(calleIntersecada,calleInterSideL,inicio,fin,false);
                borrarInterseccion(calleIntersecada, inicio, fin, 0, anchoCalleInter,false);

                inicio = definirPuntoLocal(vi[2],vi[3],vj[0],vj[1],-offset2-anchoVeredasInter,0);
                fin = definirPuntoLocal(vi[2],vi[3],vj[2],vj[3],anchoVeredasInter+offset1,0);
                borrarVeredas(gameObject,this.dynSidewalkR,inicio,fin,true);
                borrarInterseccion(gameObject,inicio,fin,anchoMio - this.anchoVeredas,anchoMio,true);
                return 0;
            }
            
            if(inter0123 && inter2323){ //Hay medio ID-DD (sup)
                //Debug.Log("Medio superior");
                inicio = definirPuntoLocal(vj[2],vj[3],vi[2],vi[3],0,-offset2-this.anchoVeredas);
                fin = definirPuntoLocal(vj[2],vj[3],vi[0],vi[1],0,offset1+this.anchoVeredas);
                borrarVeredas(calleIntersecada,calleInterSideR,inicio,fin,false);
                borrarInterseccion(calleIntersecada, inicio, fin, anchoCalleInter - anchoVeredasInter, anchoCalleInter,false);
            }
                
            if( inter0101 && inter2301 ) //Hay medio II-DI (inf)
            {
                //Debug.Log("Medio inferior");
                inicio = definirPuntoLocal(vj[0],vj[1],vi[2],vi[3],0,-offset2-this.anchoVeredas);
                fin = definirPuntoLocal(vj[0],vj[1],vi[0],vi[1],0,offset1+this.anchoVeredas);
                borrarVeredas(calleIntersecada,calleInterSideL,inicio,fin,false);
                borrarInterseccion(calleIntersecada, inicio, fin, 0, anchoVeredasInter,false);
            }
        } 
        return 0;
    }

    public void borrarInterseccion(GameObject calle, Vector2 inicio, Vector2 fin, float Zi, float Zj,  bool vertical){
        //la idea es que mas adelante se adapte a las diagonales
        float desde;
        float hasta;

        if(vertical){
            desde=inicio.x;
            hasta=fin.x;
        } else {
            desde=inicio.y;
            hasta=fin.y;
        }

        for(float i = desde; i<=hasta; i++){
            for(float j=Zi; j<Zj; j++){
                removerMesh(i, j, 1, 1, calle );
            }   
        }
        calle.GetComponent<DynamicMesh>().rotarMaterial();
        calle.GetComponent<DynamicMesh>().AdaptarCalleAlTerreno();
    }

    private Vector2 calcularPuntoInterseccion(Vector2 vi0,Vector2 vi1,Vector2 vj0,Vector2 vj1){
        Vector2 VectorA = vi1 - vi0;
        Vector2 VectorB = vj1 - vj0;

        float det = VectorA.x * VectorB.y - VectorA.y * VectorB.x;
        // cuando se llama ya sabemos que intersecan pero tal vez en otra situacion nos sirva
        if (det != 0)
        {
            float t = ((vj0.x - vi0.x) * VectorB.y - (vj0.y - vi0.y) * VectorB.x) / det;
            Vector2 interseccion = vi0 + t * VectorA;
            return interseccion;
        }
        else
        {
            return new Vector2(0,0);
        }
    }

    public Vector2 definirPuntoLocal(Vector2 vi0,Vector2 vi1,Vector2 vj0,Vector2 vj1, float offsetX,float offsetY){
        Vector2 interseccion = calcularPuntoInterseccion(vi0, vi1, vj0, vj1);
        //lo muevo a donde está el primer punto
        interseccion.x -= vi0.x + offsetX;
        interseccion.y -= vi0.y + offsetY;
        return interseccion;
    }

    /*public float DefIncremento(Vector2 vi0,Vector2 vi1){ para cuando quiera borrar diagonales
        float m = (vi1.y - vi0.y) / (vi1.x - vi0.x);

    }*/

    public void borrarVeredas(GameObject calleInter, GameObject sideWalk, Vector2 inicio, Vector2 fin, bool vertical) 
    {
        float anchoVeredas = calleInter.GetComponent<StreetBehavior>().getAnchoVeredas();
        if(vertical){

            for(float i = inicio.x; i<=fin.x; i++)
                for(float j=0; j<anchoVeredas; j++)
                    removerMesh(i, j, 1, 1, sideWalk);
        }else{
            for(float i = inicio.y; i<=fin.y; i++)
                for(float j=0; j<anchoVeredas; j++)
                    removerMesh(i, j, 1, 1, sideWalk);
        }
        
    }

    public float getAnchoVeredas(){
        return this.anchoVeredas;
    }

    public void crearCalle(float xSize, float zSize) {
        this.GetComponent<DynamicMesh>().setPlaneSize(xSize,zSize);
        this.GetComponent<DynamicMesh>().setRepeticiones(1,4);
        this.GetComponent<DynamicMesh>().rotarMaterial();
        this.GetComponent<StreetSize>().setDimensiones(xSize,zSize);
        this.vectores = GetComponent<StreetSize>().getVectores();

        this.anchoVeredas = xSize /5;
        setVeredas(xSize,zSize);
        
    }

    public void adaptarCalleAlTerreno(float xPos, float zPos, float xSize,float zSize ,GameObject terreno) {
    Vector3[] vertices = terreno.GetComponent<DynamicMesh>().getVerticesEnArea(xPos,zPos,xSize,zSize);
    this.GetComponent<DynamicMesh>().setAlturasVertices(vertices);
    Vector3[] verticesDynSidewalkL = this.GetComponent<DynamicMesh>().getVerticesEnArea(this.dynSidewalkL.transform.localPosition.z, this.dynSidewalkL.transform.localPosition.x, this.anchoVeredas,zSize);
    Vector3[] verticesDynSidewalkR = this.GetComponent<DynamicMesh>().getVerticesEnArea(this.dynSidewalkR.transform.localPosition.z,this.dynSidewalkR.transform.localPosition.x, this.anchoVeredas, zSize);
    this.dynSidewalkL.GetComponent<DynamicMesh>().setAlturasVertices(verticesDynSidewalkL);
    this.dynSidewalkR.GetComponent<DynamicMesh>().setAlturasVertices(verticesDynSidewalkR);
    }
    public void adaptarCalleAlTerreno90(float xPos,float zPos,float xSize,float zSize,GameObject terreno) {
    Vector3[] vertices = terreno.GetComponent<DynamicMesh>().getVerticesEnArea90(xPos,zPos,xSize,zSize);
    this.GetComponent<DynamicMesh>().setAlturasVertices(vertices);
    Vector3[] verticesDynSidewalkL = this.GetComponent<DynamicMesh>().getVerticesEnArea(this.dynSidewalkL.transform.localPosition.z, this.dynSidewalkL.transform.localPosition.x, this.anchoVeredas, zSize);
    Vector3[] verticesDynSidewalkR = this.GetComponent<DynamicMesh>().getVerticesEnArea(this.dynSidewalkR.transform.localPosition.z, this.dynSidewalkR.transform.localPosition.x, this.anchoVeredas, zSize);
    this.dynSidewalkL.GetComponent<DynamicMesh>().setAlturasVertices(verticesDynSidewalkL);
    this.dynSidewalkR.GetComponent<DynamicMesh>().setAlturasVertices(verticesDynSidewalkR);
    
    }

    public void agrandarCalle(float xSize,float zSize) {
        this.GetComponent<DynamicMesh>().setPlaneSize(xSize,zSize);
        this.GetComponent<DynamicMesh>().setRepeticiones(1,4);
        this.GetComponent<DynamicMesh>().rotarMaterial();
        this.GetComponent<StreetSize>().setDimensiones(xSize,zSize);
        this.vectores = GetComponent<StreetSize>().getVectores();
        updateVeredas(xSize,zSize);
        this.GetComponent<DynamicMesh>().AdaptarCalleAlTerreno();
    }

    private void updateVeredas(float anchoCalle,float largoCalle) {

        //Debug.Log("largoCalle: " + largoCalle + " ancho veredas: "+this.anchoVeredas);
        this.dynSidewalkL.GetComponent<DynamicMesh>().setPlaneSize(this.anchoVeredas,largoCalle);
        this.dynSidewalkL.GetComponent<DynamicMesh>().setRepeticiones(this.anchoVeredas,largoCalle);
        this.dynSidewalkL.GetComponent<DynamicMesh>().AdaptarCalleAlTerreno();
        this.dynSidewalkL.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Concrete textures pack/pattern 08/Concrete pattern 08");

        this.dynSidewalkR.GetComponent<DynamicMesh>().setPlaneSize(this.anchoVeredas,largoCalle);
        this.dynSidewalkR.GetComponent<DynamicMesh>().setRepeticiones(this.anchoVeredas,largoCalle);
        this.dynSidewalkR.GetComponent<DynamicMesh>().AdaptarCalleAlTerreno();
        this.dynSidewalkR.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Concrete textures pack/pattern 08/Concrete pattern 08");
        //gameObject.GetComponent<DynamicMesh>().removerAreaMesh(dynSidewalkL.transform.localPosition.z, dynSidewalkL.transform.localPosition.x, largoCalle, anchoVeredas);
        //gameObject.GetComponent<DynamicMesh>().removerAreaMesh(dynSidewalkR.transform.localPosition.z, dynSidewalkR.transform.localPosition.x, largoCalle, anchoVeredas);
    }

    private void setVeredas(float anchoCalle,float largoCalle) {
        //para ver para que lado se crean las cosas

        GameObject sidewlaToInstantiate = Resources.Load<GameObject>("Prefabs/EstructurasRoom/DynMesh");
        this.dynSidewalkL = Instantiate(sidewlaToInstantiate,this.transform.position,this.transform.localRotation);
        this.dynSidewalkL.transform.Translate(new Vector3(0,0.1F,0));

        this.dynSidewalkR = Instantiate(sidewlaToInstantiate,this.dynSidewalkL.transform.position,this.transform.localRotation);
        this.dynSidewalkR.transform.Translate(new Vector3(anchoCalle - anchoVeredas,0,0));

        this.dynSidewalkR.transform.SetParent(this.transform);
        this.dynSidewalkL.transform.SetParent(this.transform);
        this.dynSidewalkL.name = "SidewalkL";
        this.dynSidewalkR.name = "SidewalkR";

        //Debug.Log("largoCalle: " + largoCalle + " ancho veredas: "+this.anchoVeredas);
        this.dynSidewalkL.GetComponent<DynamicMesh>().setPlaneSize(this.anchoVeredas,largoCalle);
        this.dynSidewalkL.GetComponent<DynamicMesh>().setRepeticiones(this.anchoVeredas,largoCalle);
        this.dynSidewalkL.GetComponent<DynamicMesh>().AdaptarCalleAlTerreno();
        this.dynSidewalkL.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Concrete textures pack/pattern 08/Concrete pattern 08");

        this.dynSidewalkR.GetComponent<DynamicMesh>().setPlaneSize(this.anchoVeredas,largoCalle);
        this.dynSidewalkR.GetComponent<DynamicMesh>().setRepeticiones(this.anchoVeredas,largoCalle);
        this.dynSidewalkR.GetComponent<DynamicMesh>().AdaptarCalleAlTerreno();
        this.dynSidewalkR.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Concrete textures pack/pattern 08/Concrete pattern 08");
        //gameObject.GetComponent<DynamicMesh>().removerAreaMesh(dynSidewalkL.transform.localPosition.z, dynSidewalkL.transform.localPosition.x, largoCalle, anchoVeredas);
        //gameObject.GetComponent<DynamicMesh>().removerAreaMesh(dynSidewalkR.transform.localPosition.z, dynSidewalkR.transform.localPosition.x, largoCalle, anchoVeredas);
    }

    bool IntersecaHaciaDerecha(List<Vector2> vi, List<Vector2> vj){
        return ( (productoCruzado(vi[0],vi[1],vj[0])>0 && productoCruzado(vi[0],vi[1],vj[1])<0)
                || ( productoCruzado(vi[0],vi[1],vj[2])>0 && productoCruzado(vi[0],vi[1],vj[3])<0)
                || ( productoCruzado(vi[2],vi[3],vj[0])>0 && productoCruzado(vi[2],vi[3],vj[1])<0)
                || ( productoCruzado(vi[2],vi[3],vj[2])>0 && productoCruzado(vi[2],vi[3],vj[3])<0) );
    }

    GameObject GetSidewalkR(){
        return this.dynSidewalkR;
    }

    GameObject GetSidewalkL(){
        return this.dynSidewalkL;
    }

    private void removerMesh(float xPos,float zPos,float xSize,float zSize, GameObject sidewalk){
        sidewalk.GetComponent<DynamicMesh>().removerAreaMesh(xPos,zPos,xSize,zSize);
    }

    /*private void restaurarMesh(float xPos,float zPos,float xSize,float zSize, GameObject sidewalk){
        sidewalk.GetComponent<DynamicMesh>().restaurarMesh(xPos,zPos,xSize,zSize);
    }*/

    public void addBuilding(int dir, DynamicBuilding building) {
        this.buildings.Add(dir,building);
    }

    public DynamicBuilding getBuilding(int dir) {
        if (this.buildings.ContainsKey(dir)) {
            return this.buildings[dir];
        }
        return null;   
    }

    public bool isValidDir(int dir,float xSize) {
        return this.GetComponent<StreetSize>().getLargoCalle()>= dir+(int)xSize + 1;
    }

    public Vector3 getPosAtDir(int direccion,float xSize,float zSize) {
        float anchoCalle = GetComponent<StreetSize>().getAnchoCalle();
        if ((direccion % 2) == 0)
            return new Vector3(anchoCalle,0,direccion);
        else
            return new Vector3(-zSize,0,direccion );
    }

    public bool hayBuildEnDir(int dir) {
        return this.buildings.ContainsKey(dir);
    }

    private bool intersecan(Vector2 v1,Vector2 v2,Vector2 v3,Vector2 v4) {
        //Cada vector es un punto, por lo q 2-1 y 4-3 generan los 2 vectores con los q se espera trabajar
        float d1 = productoCruzado(v3,v4,v1);
        float d2 = productoCruzado(v3,v4,v2);
        float d3 = productoCruzado(v1,v2,v3);
        float d4 = productoCruzado(v1,v2,v4);
        if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) && ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0))) return true;

        if (d1 == 0 && onSegment(v3,v4,v1)) return true;
        if (d2 == 0 && onSegment(v3,v4,v2)) return true;
        if (d3 == 0 && onSegment(v1,v2,v3)) return true;
        if (d4 == 0 && onSegment(v1,v2,v4)) return true;
        return false;

    }

    private float productoCruzado(Vector2 punto0,Vector2 punto1,Vector2 punto2) {
        return (punto1.x - punto0.x) * (punto2.y - punto0.y) - (punto2.x - punto0.x) * (punto1.y - punto0.y);
    }
    private bool onSegment(Vector2 punto0,Vector2 punto1,Vector2 punto2) {
        return Mathf.Min(punto0.x,punto1.x) <= punto2.x && punto2.x <= Mathf.Max(punto0.x,punto1.x) &&
               Mathf.Min(punto0.y,punto1.y) <= punto2.y && punto2.y <= Mathf.Max(punto0.y,punto1.y);
    }
}
