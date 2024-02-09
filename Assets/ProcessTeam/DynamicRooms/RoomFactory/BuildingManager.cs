using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


namespace ProcessTeam.DynamicRooms.BuildingManager {
    public class BuildingManager : MonoBehaviour {
        [SerializeField] private GameObject position;
        private Dictionary<string,Type> _buildingsByName;
        private Dictionary<string,DynamicBuilding> _buildingInstances;  //Eventual mapa donde se accede a un building dependiendo de la direccion q tenga
        private Dictionary<string,GameObject> _streetInstances;
        private List<GameObject> listaDeCalles;
        private Dictionary<string,Type> _shapeByName;
        private DynamicBuilding lastCreatedBuilding;
        

        private GameObject terreno;
        private int xSizeTerreno = 200;
        private int zSizeTerreno = 200;
        private int xPosTerreno;
        private int zPosTerreno;

        public int GetXSizeTerreno() {
            return xSizeTerreno;
        }

        public int GetZSizeTerreno() {
            return zSizeTerreno;
        }

        [ContextMenu("Toma1")]
        public void toma1() {
            createStreet("1",-99,0,-80,90,10,40);
            createStreet("2",-80,0,-99,0,10,40);
            createStreet("3",0,0,56,90,10,40);
            createStreet("4",0,0,-56,0,10,40);

        }

        [ContextMenu("borrarObjetos")]
        public void borrar() {
            removeObjectsFromBuildStreet("1",30,1);
            changeMaterialAtStreet("2",31,2,"pared interna","SimpleRedBricks");

        }

        [ContextMenu("borrarTodosLosObjetos")]
        public void borrarTodos() {
            removeObjectsFromAllBuildsStreet("1",30);
        }

        public void Start() {

            _buildingInstances = new Dictionary<string,DynamicBuilding>();
            _streetInstances = new Dictionary<string,GameObject>();
            listaDeCalles = new List<GameObject>();
            lastCreatedBuilding = null;
            this.xPosTerreno = -1 * xSizeTerreno / 2;
            this.zPosTerreno = -1 * zSizeTerreno / 2;

            GameObject prefav = Resources.Load<GameObject>("Prefabs/EstructurasRoom/terreno");
            terreno = Instantiate(prefav,new Vector3(xPosTerreno,0,zPosTerreno),Quaternion.identity);
            terreno.GetComponent<DynamicMesh>().setCantDePoligonos(1);
            terreno.GetComponent<DynamicMesh>().setPlaneSize(xSizeTerreno,zSizeTerreno);
            terreno.GetComponent<DynamicMesh>().setRepeticiones(xSizeTerreno,zSizeTerreno);
            terreno.GetComponent<DynamicMesh>().terrain(0.1f,2,5);
            terreno.transform.SetParent(this.transform);
            terreno.name = "Terreno Principal";


            _buildingsByName = new Dictionary<string,Type> {
                {"BuildingConstArea", typeof(BuildingConstArea)}
            };

            createStreet("Calle 0",-50,0,45,90,10,80);
            createStreet("Calle 1",0,0,-20,0,10,90);

            createBuildingOnStreet("Calle 1",25,"BuildingConstArea",20,15);
            addFloorToBuildOnStreet("Calle 1",25,0,"RoomVacio",3);
            addFloorToBuildOnStreet("Calle 1",25,1,"RoomVacio",4);

            createBuildingOnStreet("Calle 0",62,"BuildingConstArea",15,25);
            addFloorToBuildOnStreet("Calle 0",62,0,"RoomVacio",3);
            addFloorToBuildOnStreet("Calle 0",62,1,"RoomVacio",4);

        }

        public static bool isBuildingOperation(string data) {
            return data.StartsWith("Crear building") ||
                   data.StartsWith("Crear objeto") ||
                   data.StartsWith("Eliminar building") ||
                   data.StartsWith("Add floor") ||
                   data.StartsWith("Remove floor") ||
                   data.StartsWith("Vaciar building");
        }

        public void createTerrain(string tipo,float xPos,float yPos,float zPos,float ancho,float largo,float perlinScale,float PerlinHighScale,float mountain) {
            GameObject prefav = Resources.Load<GameObject>("Prefabs/EstructurasRoom/terreno");
            GameObject terreno = Instantiate(prefav,new Vector3(xPos,yPos,zPos),Quaternion.identity);
            terreno.GetComponent<DynamicMesh>().setPlaneSize(ancho,largo);
            terreno.GetComponent<DynamicMesh>().setRepeticiones(ancho,largo);
            terreno.GetComponent<DynamicMesh>().terrain(perlinScale,PerlinHighScale,mountain);
            terreno.transform.SetParent(this.transform);
            terreno.name = tipo;
        }
        
        public void createStreet(string nombreCalle,float xPos,float yPos,float zPos,float angle,float ancho,float largo) {
            if (nombreCalle == "General Paz") yPos = 0.01f;
            GameObject calleAux = Resources.Load<GameObject>("Prefabs/EstructurasRoom/DynMeshStreet");
            GameObject calle = Instantiate(calleAux,new Vector3(xPos,yPos,zPos),Quaternion.identity);
            calle.transform.Rotate(new Vector3(0,angle,0));
            calle.transform.SetParent(this.transform);
            calle.name = nombreCalle;
            this._streetInstances.Add(nombreCalle,calle);
            this.listaDeCalles.Add(calle);
            calle.GetComponent<StreetBehavior>().crearCalle(ancho,largo);
            foreach (GameObject calleAComparar in this.listaDeCalles) {
                if (calle.GetInstanceID() != calleAComparar.GetInstanceID())
                    calle.GetComponent<StreetBehavior>().reaccionarInterseccion(calleAComparar);
            }
            float xSizeCalle = (float) Math.Floor(ancho * Math.Cos(angle * Mathf.Deg2Rad) + largo * Math.Sin(angle * Mathf.Deg2Rad));
            float zSizeCalle = (float) Math.Floor(largo * Math.Cos(angle * Mathf.Deg2Rad) + ancho * Math.Sin(angle * Mathf.Deg2Rad));

            adaptarCalleTerreno(calle,xPos,zPos,xSizeCalle,zSizeCalle,angle);
            removerTerreno(xPos,(float) (zPos - Math.Sin(angle * Mathf.Deg2Rad) * ancho),xSizeCalle,zSizeCalle);
        }
        
        public void agrandarCalle(string nombreCalle,float largoExtra) {
            if (!this._streetInstances.ContainsKey(nombreCalle)) return;
            GameObject calle = this._streetInstances[nombreCalle];
            float largo = calle.GetComponent<StreetSize>().getLargoCalle();
            float ancho = calle.GetComponent<StreetSize>().getAnchoCalle();
            float angle = calle.transform.rotation.eulerAngles.y;
            calle.GetComponent<StreetBehavior>().agrandarCalle(ancho,largo + largoExtra);
            float xPos = calle.transform.localPosition.x;
            float zPos = calle.transform.localPosition.z;

            foreach (GameObject calleAComparar in this.listaDeCalles) {
                if (calle.GetInstanceID() != calleAComparar.GetInstanceID())
                    calle.GetComponent<StreetBehavior>().reaccionarInterseccion(calleAComparar);
            }
            largo = calle.GetComponent<StreetSize>().getLargoCalle();
            ancho = calle.GetComponent<StreetSize>().getAnchoCalle();
            float xSizeCalle = (float) Math.Floor(ancho * Math.Cos(angle * Mathf.Deg2Rad) + largo * Math.Sin(angle * Mathf.Deg2Rad));
            float zSizeCalle = (float) Math.Floor(largo * Math.Cos(angle * Mathf.Deg2Rad) + ancho * Math.Sin(angle * Mathf.Deg2Rad));
            adaptarCalleTerreno(calle, xPos, zPos, xSizeCalle, zSizeCalle, angle);
            removerTerreno(xPos,(float) (zPos - Math.Sin(angle * Mathf.Deg2Rad) * ancho),xSizeCalle,zSizeCalle);
        }
        public void createBuildingOnStreet(string nombreCalle,int direccion,string prefab,float tamFrente,float tamProfundidad) {
            if (!_buildingsByName.ContainsKey(prefab)) return;
            if (!this._streetInstances[nombreCalle].GetComponent<StreetBehavior>().isValidDir(direccion,tamFrente)) return;

            Type type = _buildingsByName[prefab];
            var building = Activator.CreateInstance(type) as DynamicBuilding;

            GameObject calle = this._streetInstances[nombreCalle];

            Vector2 agujero = new Vector2(calle.transform.localPosition.x,calle.transform.localPosition.z);
            agujero += new Vector2(this._streetInstances[nombreCalle].GetComponent<StreetBehavior>().getPosAtDir(direccion,tamFrente,tamProfundidad).x,this._streetInstances[nombreCalle].GetComponent<StreetBehavior>().getPosAtDir(direccion,tamFrente,tamProfundidad).z);
            int parImpar = direccion % 2;
            float angle = calle.transform.rotation.eulerAngles.y;
            float anchoCalle = calle.GetComponent<StreetSize>().getAnchoCalle();
            float angleCos = (float) (Math.Cos(angle * Mathf.Deg2Rad));
            float angleSin = (float) (Math.Sin(angle * Mathf.Deg2Rad));
            float tamX = angleCos * tamProfundidad + angleSin * tamFrente;
            float tamZ = angleSin * tamProfundidad + angleCos * tamFrente;
            float posEnY = angleCos * agujero.y + angleSin * (calle.transform.localPosition.z - ((1 - parImpar) * (anchoCalle + tamProfundidad)));
            float posEnX = angleCos * agujero.x + angleSin * (calle.transform.localPosition.x + (direccion));

            if (angleSin == 0) {
                if (!hayTerreno(posEnX,posEnY,tamX,tamZ,(int) tamFrente,(int) tamProfundidad)) return;
            }
            else
                if (!hayTerreno(posEnX,posEnY,tamX,tamZ,(int) tamProfundidad,(int) tamFrente)) return;

            GameObject newPos = new GameObject(nombreCalle + " " + direccion);
            newPos.transform.SetParent(calle.transform);
            string id = "id" + nombreCalle + direccion;
            newPos.transform.localPosition = (this._streetInstances[nombreCalle].GetComponent<StreetBehavior>().getPosAtDir(direccion,tamFrente,tamProfundidad));
            newPos.transform.localRotation = calle.transform.localRotation;
            lastCreatedBuilding = building.CreateInstanceBuilding(nombreCalle + direccion,newPos,tamProfundidad,tamFrente);
            calle.GetComponent<StreetBehavior>().addBuilding(direccion,lastCreatedBuilding);
            this._buildingInstances.Add(id,lastCreatedBuilding);

            removerTerreno(posEnX,posEnY,tamX,tamZ);
            if (calle.transform.localRotation.y == 0) {
                generateButton(id,"Window",new Vector3(1.5f,1.5f,-0.1f),newPos,angle);
                generateButton(id,"Color",new Vector3(3f,1.5f,-0.1f),newPos,angle);
                generateButton(id,"Floor",new Vector3(1.5f,2.7f,-0.1f),newPos,angle);
            }
            else {
                
                generateButton(id,"Window",new Vector3(0.1f,1.5f,1.5f),newPos,angle);
                generateButton(id,"Color",new Vector3(0.1f,1.5f,3f),newPos,angle);
                generateButton(id,"Floor",new Vector3(0.1f,2.7f,1.5f),newPos,angle);
            }
        }
        private void generateButton(string id,string type,Vector3 pos,GameObject padre,float angle) {
            GameObject buildMenu = Resources.Load<GameObject>("Prefabs/Interfaces/BuildingButton" + type);
            GameObject menuInstance = Instantiate(buildMenu);
            menuInstance.GetComponent<buildingButton>().bm = this;
            menuInstance.GetComponent<buildingButton>().id = id;
            menuInstance.transform.SetParent(padre.transform);
            menuInstance.transform.localPosition = pos;
            menuInstance.transform.localRotation = Quaternion.Euler(0,angle,0);
        }

        public void addFloorToBuildOnStreet(string calle,int dir,int floor,string roomType,float ySize,float posXEscalera = 5F,float posZEscalera = 5F,string tipoEscalera = "escalerarecta") {
            if (this._streetInstances.ContainsKey(calle))
                if (this._streetInstances[calle].GetComponent<StreetBehavior>().hayBuildEnDir(dir)) {
                    this._streetInstances[calle].GetComponent<StreetBehavior>().getBuilding(dir).addFloorAt(roomType,floor,ySize,posXEscalera,posZEscalera,tipoEscalera);
                }
        }
        
        public void addObjectToBuildStreet(string calle,int dir,string objectType,float xPos,float zPos,float angle,float scaleX,float scaleY,float scaleZ,string objectName,int floor) {
            if (this._streetInstances.ContainsKey(calle))
                if (this._streetInstances[calle].GetComponent<StreetBehavior>().hayBuildEnDir(dir)) {
                    this._streetInstances[calle].GetComponent<StreetBehavior>().getBuilding(dir).addObjectToRoom(objectType,xPos,zPos,angle,scaleX,scaleY,scaleZ,objectName,floor); ;
                }
        }

        public void removeObjectsFromBuildStreet(string calle, int dir, int floor){
            if (this._streetInstances.ContainsKey(calle))
                if (this._streetInstances[calle].GetComponent<StreetBehavior>().hayBuildEnDir(dir)) {
                    this._streetInstances[calle].GetComponent<StreetBehavior>().getBuilding(dir).removeObjectsFromRoom(floor);
                }
        }

        public void removeObjectsFromAllBuildsStreet(string calle, int dir){
            if (this._streetInstances.ContainsKey(calle))
                if (this._streetInstances[calle].GetComponent<StreetBehavior>().hayBuildEnDir(dir)) {
                    int pisos = this._streetInstances[calle].GetComponent<StreetBehavior>().getBuilding(dir).getCantPisos();
                    int pisosNeg = this._streetInstances[calle].GetComponent<StreetBehavior>().getBuilding(dir).getCantPisosBajoTierra();
                    for (int i = 0; i < pisos; i++)
                        this._streetInstances[calle].GetComponent<StreetBehavior>().getBuilding(dir).removeObjectsFromRoom(i);
                    for (int j = -1; j > -pisosNeg; j--)
                        this._streetInstances[calle].GetComponent<StreetBehavior>().getBuilding(dir).removeObjectsFromRoom(j);
                }
        }

        
        public void addComponentToObjectStreet(string calle,int dir,int floor,string idObject,string component) {
            if (this._streetInstances.ContainsKey(calle))
                if (this._streetInstances[calle].GetComponent<StreetBehavior>().hayBuildEnDir(dir)) {
                    this._streetInstances[calle].GetComponent<StreetBehavior>().getBuilding(dir).addComponentToObject(idObject,floor,component);
                }
        }
        
        public void changeMaterialAtStreet(string calle,int dir,int floor,string objectType,string material) {
            if (this._streetInstances.ContainsKey(calle))
                if (this._streetInstances[calle].GetComponent<StreetBehavior>().hayBuildEnDir(dir)) {
                    this._streetInstances[calle].GetComponent<StreetBehavior>().getBuilding(dir).changeMaterialAt(floor,objectType,material);
                }
        }

        
        public void addWindowAtStreet(string calle,int dir,int floor,string nombreEstructura,string prefabVentana,float xPos,float yPos,float xSize,float ySize) {
            if (this._streetInstances.ContainsKey(calle))
                if (this._streetInstances[calle].GetComponent<StreetBehavior>().hayBuildEnDir(dir)) {
                    this._streetInstances[calle].GetComponent<StreetBehavior>().getBuilding(dir).addWindowAt(floor,nombreEstructura,prefabVentana,xPos,yPos,xSize,ySize);
                }
        }
        
        public void addDoorAtStreet(string calle,int dir,int floor,string nombreEstructura,string prefabPuerta,float xPos) {
            if (this._streetInstances.ContainsKey(calle))
                if (this._streetInstances[calle].GetComponent<StreetBehavior>().hayBuildEnDir(dir)) {
                    this._streetInstances[calle].GetComponent<StreetBehavior>().getBuilding(dir).addDoorAt(floor,nombreEstructura,prefabPuerta,xPos);
                }
        }

        public void removeFloorAtStreet(string calle,int dir,int floor) {
            if (this._streetInstances.ContainsKey(calle))
                if (this._streetInstances[calle].GetComponent<StreetBehavior>().hayBuildEnDir(dir)) {
                    this._streetInstances[calle].GetComponent<StreetBehavior>().getBuilding(dir).removeFloorAt(floor);
                }
        }
        
        public void createBuilding(Vector3 posRelativa,string id,string prefab,float xSize,float zSize) {
            if (!_buildingsByName.ContainsKey(prefab)) return;

            Type type = _buildingsByName[prefab];
            var building = Activator.CreateInstance(type) as DynamicBuilding;

            GameObject newPos = new GameObject("B" + id);
            newPos.transform.SetParent(transform);
            newPos.transform.Translate(position.transform.position + posRelativa);
            lastCreatedBuilding = building.CreateInstanceBuilding(id,newPos,xSize,zSize);

            _buildingInstances.Add(id,lastCreatedBuilding);
        }
        
        public void addFloorAt(string id,int floor,string roomType,float ySize,float posXEscalera = 5.0F,float posZEscalera = 5.0F,string tipoEscalera = "escalerarecta") {
            DynamicBuilding buildingInstance = _buildingInstances[id];
            if (buildingInstance.getCantPisos() == floor)
                buildingInstance.addFloorAtTop(roomType,ySize,posXEscalera,posZEscalera,tipoEscalera);
            else
                buildingInstance.addFloorAt(roomType,floor,ySize,posXEscalera,posZEscalera,tipoEscalera);
        }

        public void addObjectToBuilding(string id,string objectType,float xPos,float zPos,float angle,float scaleX,float scaleY,float scaleZ,string objectName,int floor) {
            DynamicBuilding buildingInstance = _buildingInstances[id];
            if (buildingInstance.getId() != null)
                buildingInstance.addObjectToRoom(objectType,xPos,zPos,angle,scaleX,scaleY,scaleZ,objectName,floor);
            else
                Debug.Log("No existe building con id" + id);
        }

        public void removeObjectsFromBuildings( string id, int floor){
            DynamicBuilding buildingInstance = _buildingInstances[id];
            if (buildingInstance.getId() != null)
                buildingInstance.removeObjectsFromRoom(floor);
            else
                Debug.Log("No existe building con id" + id);
        }

        public void removeObjectsFromAllBuildings( string id){
            DynamicBuilding buildingInstance = _buildingInstances[id];
            if (buildingInstance.getId() != null){
                for (int i = 0; i < buildingInstance.getCantPisos(); i++)
                    buildingInstance.removeObjectsFromRoom(i);
                for (int j = -1; j > -(buildingInstance.getCantPisosBajoTierra()); j--)
                    buildingInstance.removeObjectsFromRoom(j);
            }
            else
                Debug.Log("No existe building con id" + id);
        }

        public void addComponentToObject(string idBuilding,int floor,string idObject,string component) {
            DynamicBuilding buildingInstance = _buildingInstances[idBuilding];
            if (buildingInstance.getId() != null)
                buildingInstance.addComponentToObject(idObject,floor,component);
            else
                Debug.Log("No existe building con id" + idBuilding);
        }

        public void addWindowAt(string idBuilding,int floor,string nombreEstructura,string prefabVentana,float xPos,float yPos,float xSize,float ySize) {
            DynamicBuilding buildingInstance = _buildingInstances[idBuilding];
            if (buildingInstance.getId() != null)
                buildingInstance.addWindowAt(floor,nombreEstructura,prefabVentana,xPos,yPos,xSize,ySize);
            else
                Debug.Log("No existe building con id" + idBuilding);
        }

        public void addDoorAt(string idBuilding,int floor,string nombreEstructura,string prefabPuerta,float xPos) {
            DynamicBuilding buildingInstance = _buildingInstances[idBuilding];
            if (buildingInstance.getId() != null)
                buildingInstance.addDoorAt(floor,nombreEstructura,prefabPuerta,xPos);
            else
                Debug.Log("No existe building con id" + idBuilding);
        }

        public void changeMaterialAt(string id,int floor,string objectType,string material) {
            DynamicBuilding buildingInstance = _buildingInstances[id];
            if (buildingInstance.getCantPisos() > floor) {
                buildingInstance.changeMaterialAt(floor,objectType,material);
            }
        }
        public void changeWholeMaterial(string id,string objectType,string material) {
            DynamicBuilding buildingInstance = _buildingInstances[id];
            for (int i=0; i<buildingInstance.getCantPisos();i++) {
                buildingInstance.changeMaterialAt(i,objectType,material);
            }
        }

        public int countFloorsBuilding(string id) {
            if (!_buildingInstances.ContainsKey(id)) return -1;
            return _buildingInstances[id].getCantPisos();
        }

        public void removeFloorAt(string idBuilding,int floor) {
            DynamicBuilding buildingInstance = _buildingInstances[idBuilding];
            if (buildingInstance.getId() != null)
                buildingInstance.removeFloorAt(floor);
        }

        private void removerTerreno(float xPos,float zPos,float xSize,float zSize) {
            this.terreno.GetComponent<DynamicMesh>().removerAreaMesh(zPos + this.zSizeTerreno / 2,xPos + this.xSizeTerreno / 2,zSize,xSize);
        }

        private bool hayTerreno(float xPos,float zPos,float xSize,float zSize,int tamProfundidad,int tamFrente) {
            bool hayTerreno = true;
            for (int i = 0; i < tamProfundidad; i++)
                for (int j = 0; j < tamFrente; j++) {
                    hayTerreno = this.terreno.GetComponent<DynamicMesh>().hayMesh(zPos + this.zSizeTerreno / 2 + i,xPos + this.xSizeTerreno / 2 + j);
                    if (!hayTerreno) return false;
                }
            return true;
        }

        public void addUndergroudFloorAtBottom(string idBuilding,string roomtype,float ySize,float posXEscalera,float posZEscalera,string tipoEscalera) {
            DynamicBuilding buildingInstance = _buildingInstances[idBuilding];
            if (buildingInstance.getId() != null)
                buildingInstance.addUndergroundFloorAtBottom(roomtype,ySize,posXEscalera,posZEscalera,tipoEscalera);
        }

        private void adaptarCalleTerreno(GameObject calle,float xPos,float zPos,float xSize,float zSize,float angle) {
            float anchoCalle = calle.GetComponent<StreetSize>().getAnchoCalle();

            if (angle == 90)
                calle.GetComponent<StreetBehavior>().adaptarCalleAlTerreno90(zPos + this.zSizeTerreno / 2 - anchoCalle,xPos + this.xSizeTerreno / 2,zSize,xSize,this.terreno);
            else {
                calle.GetComponent<StreetBehavior>().adaptarCalleAlTerreno(zPos + this.zSizeTerreno / 2,xPos + this.xSizeTerreno / 2,xSize,zSize,this.terreno);
            }
        }
    }
}
