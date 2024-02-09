using ProcessTeam.DynamicRooms.RoomFactory;
using System.Collections.Generic;
using UnityEngine;

namespace ProcessTeam.DynamicRooms.BuildingManager {
    public abstract class DynamicBuilding : MonoBehaviour {
        public abstract string name { get; }
        private List<GameObject> floors = new List<GameObject>();
        private List<GameObject> undergroundFloors = new List<GameObject>(); // el valor en [0] corresponde al subsuelo 1
        public string id;
        private protected abstract void addElevationMethod(int floor,float xPos,float zPos, string tipo);
        public abstract DynamicBuilding CreateInstanceBuilding(string id,GameObject pos,float xSize,float zSize);
        public abstract void addFloorAt(string roomType,int floor,float ySize, float posXEscalera = 5.0f, float posZEscalera = 5.0f, string tipoEscalera = "escalerarecta");
        public abstract void addFloorAtTop(string roomType,float ySize, float posXEscalera = 5.0f, float posZEscalera = 5.0f, string tipoEscalera = "escalerarecta");

        public abstract void createFirstFloor(string roomType,float ySize);

        public int getCantPisos() { return floors.Count; }

        public int getCantPisosBajoTierra() { return undergroundFloors.Count; }

        public GameObject getFloorAt(int floorNumber)
        {
            return floorNumber < 0 ? getUndergroundFloorAt(-1+floorNumber * -1) : floors[floorNumber];
        }

        public GameObject getUndergroundFloorAt(int floorNumber)
        {
            return undergroundFloors[floorNumber];
        }
        public void addFloorListAt(GameObject room,int floor) {
            floors.Insert(floor,room);
        }
        public void removeFloorListAt(int floor) {
            floors.RemoveAt(floor);
        }
        public void addUndergroundFloorListAt(GameObject room, int floor)
        {
            undergroundFloors.Insert(floor, room);
        }
        public void removeUndergroundFloorListAt(int floor) {
            undergroundFloors.RemoveAt(-floor-1);
        }

        public int getUndergroundFloorsCount()
        {
            return undergroundFloors.Count;
        }
        public abstract void print();
        public abstract void changeMaterialAt(int floor,string objectType,string material);
        public abstract void addObjectToRoom(string objectType,float xPos,float zPos,float angle,float scaleX,float scaleY,float scaleZ,string objectName,int floor);
        public abstract void removeObjectsFromRoom(int floor);
        public abstract void addComponentToObject(string idObject,int floor,string component);
        public abstract void addWindowAt(int floor,string nombreEstructura,string prefabVentana,float xPos,float yPos,float xSize,float ySize);
        public abstract void addDoorAt(int floor,string nombreEstructura,string prefabPuerta,float xPos);
        public string getId() {
            return id;
        }

        public abstract void removeFloorAt(int floor);

        public abstract void addUndergroundFloorAtBottom(string roomType, float ySize, float posXEscalera = 5.0f, float posZEscalera = 5.0f, string tipoEscalera = "escalerarecta");
        public abstract void addUndergroundFloorAt(string roomType, int floor, float ysize, float posXEscalera = 5.0f, float posZEscalera = 5.0f, string tipoEscalera = "escalerarecta");

        public abstract void acomodarAgujerosDeEscaleras(int floor);
    }

    public class BuildingConstArea : DynamicBuilding {
        public override string name => "BuildingConstArea";
        private GameObject building;
        private float xSize;
        private float zSize;
        private readonly RoomsManager rm = GameObject.FindGameObjectWithTag("RMBuilding").GetComponent<RoomsManager>();

        public override DynamicBuilding CreateInstanceBuilding(string id,GameObject pos,float xSize,float zSize) {
            this.xSize = xSize;
            building = pos;
            this.id = id;
            this.zSize = zSize;
            return this;
        }


        public override void changeMaterialAt(int floor,string objectType,string material) {
            rm.ChangeMaterialAt(this.getFloorAt(floor),objectType,material);
        }

        public override void createFirstFloor(string roomType, float ySize) {
            GameObject room = rm.setAndCreateRoom(building,roomType,xSize,zSize,ySize,this.id + ", Piso 0");
            addFloorListAt(room,getCantPisos());
        }

        private protected override void addElevationMethod(int floor,float xPos,float zPos, string tipo) {
            if (floor + 1 < getCantPisos() && floor >= -getUndergroundFloorsCount())
            {
                rm.setAndAddStair(getFloorAt(floor), getFloorAt(floor + 1), xPos, zPos, tipo);
            } else {
                Debug.Log("No existe piso de destino");
            }
        }

        public override void addDoorAt(int floor,string nombreEstructura,string prefabPuerta,float xPos) {
            GameObject room = getFloorAt(floor);
            room.GetComponent<ObjectManager>().addDoor(nombreEstructura,prefabPuerta,xPos);

        }

        public override void addFloorAtTop(string roomType,float ySize, float posXEscalera = 5.0f, float posZEscalera = 5.0f, string tipoEscalera = "escalerarecta") {
            GameObject newPosition;
            if (getCantPisos() > 0) {

                float alturaRoom = getFloorAt(getCantPisos() - 1).GetComponent<ObjectManager>().getRoomYSize();

                Vector3 auxPos = getFloorAt(getCantPisos() - 1).transform.position;
                newPosition = new GameObject();
                newPosition.transform.Translate(auxPos);
                newPosition.transform.Translate(new Vector3(0,alturaRoom,0));
                newPosition.transform.localRotation = building.transform.localRotation;
            }
            else {
                newPosition = Instantiate(this.building ,building.transform.position,building.transform.localRotation);
            }
            var room = rm.setAndCreateRoom(newPosition,roomType,xSize,zSize,ySize,"Piso " + getCantPisos());
            room.transform.SetParent(building.transform);
            if (getCantPisos() > 0)
                if (xSize > posXEscalera && zSize > posZEscalera)
                    rm.setAndAddStair(getFloorAt(getCantPisos() - 1),room,posXEscalera,posZEscalera, tipoEscalera);
            Destroy(newPosition);
            addFloorListAt(room,getCantPisos());
        }

        public override void addObjectToRoom(string objectType,float xPos,float zPos,float angle,float scaleX,float scaleY,float scaleZ,string objectName,int floor) {
            //Debug.Log("El piso es " + floor);
            GameObject room = getFloorAt(floor);
            if (room != null) {
                //Debug.Log("El piso es valido. A�adiendo " + objectType + " en el piso " + floor + " del building.");
                rm.setAndAddObject(room,objectType,xPos,zPos,angle,scaleX,scaleY,scaleZ,objectName);
            }
            else
                Debug.Log("El piso no es valido.");
        }

        public override void removeObjectsFromRoom(int floor){
            GameObject room = getFloorAt(floor);
            if (room != null) {
                rm.removeObjects(room);
            }
            else
                Debug.Log("El piso no es valido.");
        }

        public override void addComponentToObject(string idObject,int floor,string component) {
            GameObject room = getFloorAt(floor);
            if (room != null) {
                rm.setAndAddComponentToObject(room,idObject,component);
            }

        }

        public override void addWindowAt(int floor,string nombreEstructura,string prefabVentana,float xPos,float yPos,float xSize,float ySize) {
            if (floor < base.getCantPisos()) {
                base.getFloorAt(floor).GetComponent<ObjectManager>().spawnWindow(nombreEstructura,prefabVentana,xPos,yPos,xSize,ySize);
            }
        }

        public override void addFloorAt(string roomType,int floor,float ySize, float posXEscalera = 5.0f, float posZEscalera = 5.0f, string tipoEscalera = "escalerarecta") {
            if (floor == getCantPisos())
            {
                addFloorAtTop(roomType,ySize,posXEscalera,posZEscalera,tipoEscalera);
            }
            else
            {
                if (floor < 0)
                {
                    if (getUndergroundFloorsCount() == 0 || getUndergroundFloorsCount() == -floor-1)
                        addUndergroundFloorAtBottom(roomType, ySize, posXEscalera, posZEscalera, tipoEscalera);
                    else
                        addUndergroundFloorAt(roomType, floor, ySize, posXEscalera, posZEscalera, tipoEscalera);
                }
                else
                {
                    GameObject piso;
                    GameObject newPosition;
                    piso = getFloorAt(floor);
                    var oldPosition = piso.transform.position;
                    for (int i = getCantPisos() - 1; i >= floor; i--)
                    {
                        piso = getFloorAt(i);
                        piso.name = "Piso " + (i + 1);
                        Vector3 newPos = piso.transform.position + new Vector3(0, ySize, 0);
                        piso.transform.position = newPos;
                    }

                    newPosition = new GameObject();
                    newPosition.transform.Translate(oldPosition);
        
                    var room = rm.setAndCreateRoom(newPosition,roomType,xSize,zSize,ySize,"Piso " + floor);
                    room.transform.SetParent(building.transform);
                
                
                    getFloorAt(floor).GetComponent<ObjectManager>().restaurarPiso();
                
                    if (getCantPisos() > 0) 
                        rm.setAndAddStair(room,getFloorAt(floor),posXEscalera,posZEscalera,tipoEscalera);
                    Destroy(newPosition);
                    addFloorListAt(room,floor);
                    acomodarAgujerosDeEscaleras(floor-1);
                }
            }
        }

        public override void addUndergroundFloorAt(string roomType, int floor, float ySize, float posXEscalera = 5.0f, float posZEscalera = 5.0f, string tipoEscalera = "escalerarecta") {
            GameObject piso;
            GameObject newPosition;
            piso = getUndergroundFloorAt(-floor-1);
            var oldPosition = piso.transform.position;
            var yTranslation = new Vector3(0, -ySize, 0);
            for (int i = getUndergroundFloorsCount()-1; i >= -floor-1; i--)
            {
                piso = getUndergroundFloorAt(i);
                piso.transform.Translate(yTranslation);
                piso.name = "Subsuelo " + (i+2);
            }

            newPosition = new GameObject();
            newPosition.transform.Translate(oldPosition);
            var room = rm.setAndCreateRoom(newPosition,roomType,xSize,zSize,ySize,"Subsuelo " + -floor);
            room.transform.SetParent(building.transform);
            addUndergroundFloorListAt(room, -floor-1);
            Destroy(newPosition);
            getFloorAt(floor+1).GetComponent<ObjectManager>().restaurarPiso();
            if (xSize > posXEscalera && zSize > posZEscalera)
                rm.setAndAddStair(room,
                    floor == -1 ? getFloorAt(0) : getUndergroundFloorAt(-floor-2), posXEscalera, posZEscalera, tipoEscalera);
            acomodarAgujerosDeEscaleras(floor-1);
        }

        public override void print() {
            Debug.Log("Soy un BuildingConstArea");
        }

        public override void removeFloorAt(int floor) {
            GameObject floorToMove;
            GameObject floorToDelete;
            if (floor >= getCantPisos() || floor < -getUndergroundFloorsCount()) {
                Debug.Log("El piso no es valido");
            } else
            {
                floorToDelete = floor < 0 ? getUndergroundFloorAt(-floor-1) : getFloorAt(floor);
                var yTranslation = floorToDelete.GetComponent<ObjectManager>().getRoomYSize();
                Destroy(floorToDelete);
                if (floor < 0) {
                    for (int i = floor - 1; i >= -getUndergroundFloorsCount(); i--)
                    {
                        floorToMove = getFloorAt(i);
                        floorToMove.transform.Translate(new Vector3(0, yTranslation, 0));
                        floorToMove.name = "Subsuelo " + (-i - 1);
                    }
                    getFloorAt(floor + 1).GetComponent<ObjectManager>().restaurarPiso();
                    removeUndergroundFloorListAt(floor);
                    acomodarAgujerosDeEscaleras(floor);
                } else 
                {
                    for (int i = floor+1; i < getCantPisos() ; i++) {
                        floorToMove = getFloorAt(i);
                        floorToMove.transform.Translate(new Vector3(0,-yTranslation,0));
                        floorToMove.name = "Piso " + (i-1);
                    }
                    removeFloorListAt(floor);
                    if (floor < getCantPisos())
                        getFloorAt(floor).GetComponent<ObjectManager>().restaurarPiso();
                    acomodarAgujerosDeEscaleras(floor-1);
                }
            }
        }

        public override void addUndergroundFloorAtBottom(string roomType, float ySize, float posXEscalera = 5.0f, float posZEscalera = 5.0f, string tipoEscalera = "escalerarecta")
        {
            var newPosition = new GameObject();
            if (getUndergroundFloorsCount() > 0)
            {
                var lastUnderRoom = getUndergroundFloorAt(getUndergroundFloorsCount() - 1);
                var auxPos = lastUnderRoom.transform.position;
                newPosition.transform.Translate(auxPos);
                newPosition.transform.Translate(new Vector3(0,-ySize,0));
            } else
            {
                newPosition.transform.Translate(getFloorAt(0).transform.position);
                newPosition.transform.Translate(new Vector3(0,-ySize,0));
            }
            var room = rm.setAndCreateRoom(newPosition,roomType,xSize,zSize,ySize,"Subsuelo " + (getUndergroundFloorsCount()+1));
            room.transform.SetParent(building.transform);
            Destroy(newPosition);
            addUndergroundFloorListAt(room,getUndergroundFloorsCount());
            if (xSize > posXEscalera && zSize > posZEscalera)
                rm.setAndAddStair(room,
                    getUndergroundFloorsCount() == 1 ? getFloorAt(0) : getUndergroundFloorAt(getUndergroundFloorsCount() - 2), posXEscalera, posZEscalera, tipoEscalera);
        }
        
        public override void acomodarAgujerosDeEscaleras(int floor)
        {
            {
                if (floor < getCantPisos() - 1 && floor >= -getUndergroundFloorsCount())
                {
                    var roomOrigen = getFloorAt(floor);
                    var roomDestino = getFloorAt(floor + 1);
                    
                    for (var x = 0; x < xSize; x++)
                    {
                        for (var z = 0; z < zSize; z++)
                        {
                            if (!roomOrigen.GetComponent<ObjectManager>().hayTecho(x, z))
                            {
                                roomDestino.GetComponent<ObjectManager>().agujerearPiso(z,x,1,1);
                            }
                        }
                    }
                }
                else
                {
                    Debug.Log("No existe piso de destino o de origen de la escalera");
                }
            }
        }
    }

}