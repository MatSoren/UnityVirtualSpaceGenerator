using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProcessTeam.DynamicRooms {
    public class ObjectManager : MonoBehaviour {
        [SerializeField] private GameObject padre;
        //[SerializeField] private GameObject generator;
        private Dictionary<string,GameObject> _objects;
        private Dictionary<string, Type> _ceilingByName;
        private float roomYSize;
        private GameObject[,] _grid;
        private Direction _displacementDirection;
        private float _xGridSize;
        private float _zGridSize;
        private int _addedRoomId;
        private string _shownRoomId;
        private static int _cantKanban;


        private void Awake() {
            _objects = new Dictionary<string,GameObject>();
            _ceilingByName = new Dictionary<string, Type> {
                {"plano", typeof(plano)},
                {"mediaEsfera" ,typeof(mediaEsfera)},
            };
        }

        #region funcionesAuxiliares
        private void InstantiateObject(Vector3 position,string objectType,float angle,float scaleX,float scaleY,float scaleZ,string objectName) {
            GameObject objectAux = Resources.Load<GameObject>("Prefabs/PrefabsPivot/" + objectType);
            GameObject objectInstance = Instantiate(objectAux,padre.transform.position + position,Quaternion.Euler(0,angle,0));
            Vector3 newScale = new Vector3(scaleX,scaleY,scaleZ);
            objectInstance.transform.localScale = newScale;
            objectInstance.transform.name = objectName;
            objectInstance.transform.SetParent(this.padre.transform);
            _objects.Add(objectName,objectInstance);
        }

        private static string VerifyName(string objectName) {
            objectName = objectName switch {
                "mesas" => "mesa",
                "televisores" => "televisor",
                "bibliotecas" => "biblioteca",
                "pizarras" => "pizarra",
                "kanbans" => "kanban",
                "plantas" => "planta",
                "waypoints" => "waypoint",
                "glorietas" => "glorieta",
                "botellas" => "botella",
                "sillones" => "sillon",
                "dispensers" => "dispenser",
                "canteros" => "cantero",
                "bancos" => "banco",
                "faroles" => "farol",
                "pisos" => "piso",
                "paredes" => "pared",
                "cuadros" => "cuadro",
                "escritorios" => "escritorio",
                "pupitres" => "pupitre",
                "sillas" => "silla",
                "arboles" => "arbol",
                "toboganes" => "tobogan",
                "pasamanos" => "pasamano",
                "piramides" => "piramide",
                "juego de plaza" => "juegos de plaza",
                "escalera de barras" => "escalera con barras",
                "escalera barra" => "escalera con barras",
                "escalera de barra" => "escalera con barras",
                "escalera barras" => "escalera con barras",
                "escalera de plaza" => "escalera con barras",
                "barra con argollas" => "barra con anillas",
                "barra de argollas" => "barra con anillas",
                "areneros" => "arenero",
                "sube y baja" => "subibaja",
                "subeybaja" => "subibaja",
                "subibajas" => "subibaja",
                "carrusel con asientos" => "calesita",
                "carrusel" => "calesita",
                "calesitas" => "calesita",
                "hamacas" => "hamaca",
                "trepador" => "trepador curvo",
                "tacho de basura" => "tacho",
                "tachos de basura" => "tacho",
                "basurero" => "tacho",
                "basureros" => "tacho",
                "tachos" => "tacho",
                _ => objectName
            };

            return objectName;
        }

        public void AddAmountKanbans(int amount) {
            _cantKanban += amount;
        }
        #endregion

        public void EmptyRoom() {

        }

        public void RemoveObject(string objectName) {

        }

        public void RemoveRoom() {

        }

        public void setRoomYSize(float roomYSize) { this.roomYSize = roomYSize; }

        public float getRoomYSize() { return roomYSize; }

        public GameObject spawnFloor(GameObject prefab,Transform pos,float xSize,float zSize) {
            GameObject floor = Instantiate(prefab,pos.position,Quaternion.identity);
            floor.GetComponent<DynamicMesh>().setPlaneSize(xSize,zSize);
            floor.GetComponent<DynamicMesh>().setRepeticiones(xSize,zSize);
            floor.transform.name = "Piso";
            return floor;
        }

        public GameObject spawnWall(GameObject prefab,Vector3 pos,float largo, float alto) {
            //Por una cuestion de reducir la cantidad de poligonos se redondea la cantidad por altura y por largo y luego se "Estira" la parde
            //  para obtener las dimensiones deseadas. 
            
            

            GameObject estructura = Instantiate(prefab, pos, Quaternion.identity);
            GameObject wall = estructura.transform.GetChild(0).gameObject;
            
            // Proporciones de cuanto estirar la pared
            int alturaRedondeada = Mathf.FloorToInt(alto);
            float escalado = (alto / (float) alturaRedondeada);

            wall.GetComponent<DynamicMesh>().setPlaneSize(largo,alturaRedondeada);
            wall.GetComponent<DynamicMesh>().setRepeticiones(largo,alturaRedondeada);
            wall = estructura.transform.GetChild(1).gameObject;
            wall.GetComponent<DynamicMesh>().setPlaneSize(largo,alturaRedondeada);
            wall.GetComponent<DynamicMesh>().setRepeticiones(largo,alturaRedondeada);
            estructura.transform.localScale = new Vector3 (1,wall.transform.localScale.y * escalado,1);
            wall.transform.localPosition += new Vector3(0,alturaRedondeada, 0);
            setRoomYSize(alto);
            return estructura;
        }

        public GameObject spawnCeiling(GameObject prefab,Transform pos,float xSize,float zSize,string tipo) {
            var e = Activator.CreateInstance(_ceilingByName[tipo]) as DynamicCeiling;
            GameObject ceiling = e.createInstanceCeiling(prefab, pos, xSize, zSize);
            
            return ceiling;
        }

        public void spawnWindow(string nombreEstructura,string prefabVentana,float xPos,float yPos,float xSize,float ySize) {
            agujerearPared(nombreEstructura,xPos,yPos,ySize,xSize);
            //Si se quiere spawnear algun vidrio q se pase prefab
        }

        public void agujerearTecho(float xPos,float zPos,float xSize,float zSize) {
            GameObject estructuraTecho = null;
            for (int i = 0; i < padre.transform.childCount; i++)
                if (padre.transform.GetChild(i).name.Contains("Techo")) {
                    estructuraTecho = padre.transform.GetChild(i).gameObject; break;
                }
            estructuraTecho.transform.GetChild(0).gameObject.GetComponent<DynamicMesh>().removerAreaMesh(zPos,xPos,zSize,xSize);
        }
        public void rellenarTecho(float xPos,float zPos,float xSize,float zSize) {
            GameObject estructuraTecho = null;
            for (int i = 0; i < padre.transform.childCount; i++)
                if (padre.transform.GetChild(i).name.Contains("Techo")) {
                    estructuraTecho = padre.transform.GetChild(i).gameObject; break;
                }
            estructuraTecho.transform.GetChild(0).gameObject.GetComponent<DynamicMesh>().llenarAreaMesh(zPos,xPos,zSize,xSize);
        }

        public void restaurarTecho() {
            GameObject estructuraTecho = null;
            for (int i = 0; i < padre.transform.childCount; i++)
                if (padre.transform.GetChild(i).name.Contains("Techo")) {
                    estructuraTecho = padre.transform.GetChild(i).gameObject; break;
                }
            estructuraTecho.transform.GetChild(0).gameObject.GetComponent<DynamicMesh>().restaurarMesh();
        }

        public bool haySuelo(float xPos,float zPos) {
            GameObject Piso = null;
            for (int i = 0; i < padre.transform.childCount; i++)
                if (padre.transform.GetChild(i).name.Contains("Piso")) {
                    Piso = padre.transform.GetChild(i).gameObject; break;
                }
            return Piso.GetComponent<DynamicMesh>().hayMesh(xPos,zPos);
        }

        public bool hayTecho(float xPos,float zPos) {
            GameObject estructuraTecho = null;
            for (int i = 0; i < padre.transform.childCount; i++)
                if (padre.transform.GetChild(i).name.Contains("Techo")) {
                    estructuraTecho = padre.transform.GetChild(i).gameObject; break;
                }
            return estructuraTecho.transform.GetChild(0).gameObject.GetComponent<DynamicMesh>().hayMesh(xPos,zPos);
        }

        public void agujerearPiso(float xPos,float zPos,float xSize,float zSize) {
            GameObject Piso = null;
            for (int i = 0; i < padre.transform.childCount; i++)
                if (padre.transform.GetChild(i).name.Contains("Piso")) {
                    Piso = padre.transform.GetChild(i).gameObject; break;
                }
            Piso.GetComponent<DynamicMesh>().removerAreaMesh(zPos,xPos,zSize,xSize);
        }

        public void rellenarPiso(float xPos,float zPos,float xSize,float zSize) {
            GameObject Piso = null;
            for (int i = 0; i < padre.transform.childCount; i++)
                if (padre.transform.GetChild(i).name.Contains("Piso")) {
                    Piso = padre.transform.GetChild(i).gameObject; break;
                }
            Piso.GetComponent<DynamicMesh>().llenarAreaMesh(zPos,xPos,zSize,xSize);
        }

        public void restaurarPiso() {
            GameObject Piso = null;
            for (int i = 0; i < padre.transform.childCount; i++)
                if (padre.transform.GetChild(i).name.Contains("Piso")) {
                    Piso = padre.transform.GetChild(i).gameObject; break;
                }
            Piso.GetComponent<DynamicMesh>().restaurarMesh();
        }

        public void agujerearPared(string NSEO,float xPos,float yPos,float ySize,float xSize) {
            GameObject estructuraPared = null;
            for (int i = 0; i < padre.transform.childCount; i++)
                if (padre.transform.GetChild(i).name.Contains(NSEO)) {
                    estructuraPared = padre.transform.GetChild(i).gameObject; break;
                }
            GameObject pared = null;
            for (int i = 0; i < estructuraPared.transform.childCount; i++) {
                if (estructuraPared.transform.GetChild(i).name.Contains("Pared")) {
                    pared = estructuraPared.transform.GetChild(i).gameObject;
                    pared.transform.GetChild(0).gameObject.GetComponent<DynamicMesh>().removerAreaMesh(yPos,xPos,ySize,xSize);
                    pared.transform.GetChild(1).gameObject.GetComponent<DynamicMesh>().removerAreaMesh((int) getRoomYSize() - 1 - yPos - (ySize - 1),xPos,ySize,xSize);
                    break;
                }
            }
        }

        public void addDoor(string estructura, string prefab, float xPos) {
            for (int i = 0; i < padre.transform.childCount; i++)
                if (padre.transform.GetChild(i).name.Contains(estructura)) {
                    GameObject estructuraPared = padre.transform.GetChild(i).gameObject; //NSEO
                    GameObject toSpawn = Resources.Load<GameObject>(prefab);
                    GameObject Puerta = Instantiate(toSpawn, estructuraPared.transform.position, Quaternion.identity);
                    Puerta.transform.SetParent(estructuraPared.transform);
                    Puerta.transform.localPosition += new Vector3(xPos, 0, 0);
                    Puerta.transform.Rotate(0,180,0);
                    //Deber�an ser agujeros dependientes de la altura de la puerta... Cambiar sistema de puerta
                    Puerta.transform.name = "Puerta";
                    agujerearPared(estructura, xPos, 0, 3, 1); //3 hardcodeado, altura de puerta, ahora mismo queda mal
                }
        }

        public void SpawnObject(string objectType, float xPos, float zPos, float angle, float scaleX, float scaleY, float scaleZ, string objectName) {
            objectType = VerifyName(objectType);
            var objectPosition = new Vector3(xPos, 0, zPos);
            InstantiateObject(objectPosition, objectType, angle, scaleX, scaleY, scaleZ, objectName);
        }

        public void removeObjects(){
            foreach (var par in _objects)
            {
                Destroy(par.Value);
            }
            _objects.Clear();
        }

        public void addComponentToObject(string idObject, string component) {
            GameObject objeto = _objects[idObject];
            if (objeto != null) {
                System.Type tipoComponente = System.Type.GetType(component + ", UnityEngine.dll");
                Component componenteAgregado = objeto.AddComponent(tipoComponente);
            }
        }

        public void ChangeMaterial(string objectType, string material) {
            Material rendMaterial = Resources.Load<Material>("Materials/" + material);
            switch (objectType.ToLower()) {
                case "piso":
                    for (int i = 0; i < padre.transform.childCount; i++)
                        if (padre.transform.GetChild(i).name.Contains("Piso")) {
                            GameObject piso = padre.transform.GetChild(i).gameObject;
                            piso.GetComponent<MeshRenderer>().material = rendMaterial;                            
                        }
                    break;
                case "pared externa":
                    for (int i = 0; i < padre.transform.childCount; i++)
                        if (!padre.transform.GetChild(i).name.Contains("Piso") && !padre.transform.GetChild(i).name.Contains("Techo")) {
                            GameObject estructura = padre.transform.GetChild(i).gameObject; //NSEO
                            for (int j = 0; j < estructura.transform.childCount; j++) {
                                GameObject subEstructura = estructura.transform.GetChild(j).gameObject;//Pared Ventana Puerta
                                for (int k = 0; k < subEstructura.transform.childCount; k++)
                                    if (subEstructura.transform.GetChild(k).name.Contains("Ext")) {
                                        GameObject externa = subEstructura.transform.GetChild(k).gameObject;
                                        externa.GetComponent<MeshRenderer>().material = rendMaterial;
                                    }
                            }
                        }
                    break;
                case "pared interna":
                    for (int i = 0; i < padre.transform.childCount; i++)
                        if (!padre.transform.GetChild(i).name.Contains("Piso") && !padre.transform.GetChild(i).name.Contains("Techo")) {
                            GameObject estructura = padre.transform.GetChild(i).gameObject; //NSEO
                            for (int j = 0; j < estructura.transform.childCount; j++) {
                                GameObject subEstructura = estructura.transform.GetChild(j).gameObject;//Pared Ventana Puerta
                                for (int k = 0; k < subEstructura.transform.childCount; k++)
                                    if (subEstructura.transform.GetChild(k).name.Contains("Int")) {
                                        GameObject interna = subEstructura.transform.GetChild(k).gameObject;
                                        interna.GetComponent<MeshRenderer>().material = rendMaterial;
                                    }
                            }
                        }
                    break;
                case "techo":
                    for (int i = 0; i < padre.transform.childCount; i++)
                        if (padre.transform.GetChild(i).name.Contains("Techo")) {
                            GameObject estructura = padre.transform.GetChild(i).gameObject;
                            for (int j = 0; j < estructura.transform.childCount; j++)
                                estructura.transform.GetChild(j).gameObject.GetComponent<MeshRenderer>().material = rendMaterial;
                        }
                    break;
                case "puerta":
                    for (int i = 0; i < padre.transform.childCount; i++)
                        if (!padre.transform.GetChild(i).name.Contains("Piso") && !padre.transform.GetChild(i).name.Contains("Techo")) {
                            GameObject estructura = padre.transform.GetChild(i).gameObject; //NSEO
                            for (int j = 0; j < estructura.transform.childCount; j++) {
                                GameObject estrPuerta = estructura.transform.GetChild(j).gameObject;//Pared Puerta
                                if (estrPuerta.name.Contains("Puerta"))
                                    for (int k = 0; k < estrPuerta.transform.childCount; k++)
                                        if (estrPuerta.transform.GetChild(k).name.Contains("puerta")) {
                                            GameObject puertaWrapper = estrPuerta.transform.GetChild(k).gameObject;
                                            for (int h = 0; h < puertaWrapper.transform.childCount; h++)
                                                puertaWrapper.transform.GetChild(h).GetComponent<MeshRenderer>().material = rendMaterial;
                                        }
                            }
                        }
                    break;
            }
        }

        private enum Direction {
            XPositives,
            XNegatives,
            ZPositives,
            ZNegatives,
            NoDisplacement
        }
    }
}