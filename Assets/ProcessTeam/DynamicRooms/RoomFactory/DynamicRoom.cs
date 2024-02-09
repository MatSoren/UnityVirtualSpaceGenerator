
using System.Runtime.Serialization;
using UnityEngine;

namespace ProcessTeam.DynamicRooms.RoomFactory {
    public abstract class DynamicRoom : MonoBehaviour {
        public abstract string Name { get; }
        public abstract GameObject CreateInstanceRoom(GameObject parent, int roomId, GameObject position, float xSize, float zSize, float ySize);

        public string piso = "Prefabs/EstructurasRoom/DynMesh";
        public string pared = "Prefabs/EstructurasRoom/DynMeshWall";
        public string techo = "Prefabs/EstructurasRoom/DynMesh";
    }

    public class RoomVacio : DynamicRoom {
        protected GameObject _room;
        public override string Name => "RoomVacio";
        protected GameObject prefabPiso => Resources.Load<GameObject>(piso);
        protected GameObject prefabPared => Resources.Load<GameObject>(pared);
        protected GameObject prefavTecho => Resources.Load<GameObject>(techo);

        public override GameObject CreateInstanceRoom(GameObject parent, int roomId, GameObject position, float xSize, float zSize, float ySize) {

            GameObject roomAux = Resources.Load<GameObject>("Prefabs/Rooms/RoomVacio");

            _room = Instantiate(roomAux,position.transform.position,Quaternion.identity);
            GameObject floor = _room.GetComponent<ObjectManager>().spawnFloor(prefabPiso, position.transform, xSize, zSize);
            floor.transform.SetParent(_room.transform);
            setParedes(_room, position, xSize, zSize, ySize);
            GameObject newPos = position;
            newPos.transform.Translate(new Vector3(0, ySize, 0));
            _room.GetComponent<ObjectManager>().setRoomYSize(ySize);
            GameObject ceiling = _room.GetComponent<ObjectManager>().spawnCeiling(prefavTecho, newPos.transform, xSize, zSize,"plano");
            GameObject techos = new GameObject();
            techos.transform.name = "Techo";
            techos.transform.SetParent(_room.transform);
            ceiling.transform.SetParent(techos.transform);
            _room.transform.localRotation = position.transform.localRotation;
            
            return _room;
        }

        protected void setParedes(GameObject room, GameObject pos, float xSize, float zSize, float ySize) {
            //Eventualemente se pensara un sistema de spawneo basado en pivotes (posIni y fin)
            //Tambien estaria bueno q haga q las paredes se almacenen en un gameObject estructura NSEO
            Vector3 posIni = new Vector3(pos.transform.position.x, pos.transform.position.y, pos.transform.position.z);
            float rotacion = -360 / 4;
            GameObject wall = null;
            for (int i = 0; i < 4; i++) {
                float largo = 0;
                string nombre = "";
                switch (i) {
                    case 0:
                        largo = xSize;
                        nombre = "Sur";
                        break;
                    case 1:
                        largo = zSize;
                        nombre = "Este";
                        posIni = posIni + new Vector3(xSize, 0, 0);
                        break;
                    case 2:
                        largo = xSize;
                        nombre = "Norte";
                        posIni = posIni + new Vector3(0, 0, zSize);
                        break;
                    case 3:
                        largo = zSize;
                        nombre = "Oeste";
                        posIni = posIni - new Vector3(xSize, 0, 0);
                        break;
                }
                wall = room.GetComponent<ObjectManager>().spawnWall(prefabPared, posIni,largo, ySize);
                GameObject estructura = new GameObject();
                estructura.transform.SetParent(room.transform);
                estructura.transform.position = posIni;
                estructura.transform.name = nombre;
                estructura.transform.Rotate(new Vector3(0, i * rotacion, 0));
                wall.transform.SetParent(estructura.transform);
                string idPared = "Pared";
                wall.transform.name = idPared;
                wall.transform.Rotate(new Vector3(0, i * rotacion, 0));
            }
        }
    }

    public class RoomAbierto : DynamicRoom {
        private GameObject _room;
        public override string Name => "RoomAbierto";
        private GameObject prefabPiso => Resources.Load<GameObject>(piso);

        public override GameObject CreateInstanceRoom(GameObject parent, int roomId, GameObject position, float xSize, float zSize, float ySize) {
            GameObject roomAux = Resources.Load<GameObject>("Prefabs/Rooms/RoomAbierto");
            _room = Instantiate(roomAux,position.transform.position,Quaternion.identity);
            GameObject floor = _room.GetComponent<ObjectManager>().spawnFloor(prefabPiso, position.transform, xSize, zSize);
            floor.transform.SetParent(_room.transform);
            _room.transform.localRotation = position.transform.localRotation;
            return _room;
        }
    }

    public class RoomVacioTecho : RoomVacio
    {
        public override string Name => "RoomVacioTecho";
        public override GameObject CreateInstanceRoom(GameObject parent, int roomId, GameObject position, float xSize, float zSize, float ySize)
        {

            GameObject roomAux = Resources.Load<GameObject>("Prefabs/Rooms/RoomVacio");
            _room = Instantiate(roomAux, position.transform.position, Quaternion.identity);
            GameObject floor = _room.GetComponent<ObjectManager>().spawnFloor(prefabPiso, position.transform, xSize, zSize);
            floor.transform.SetParent(_room.transform);
            base.setParedes(_room, position, xSize, zSize, ySize);
            GameObject newPos = position;
            newPos.transform.Translate(new Vector3(0, ySize, 0));
            _room.GetComponent<ObjectManager>().setRoomYSize(ySize);
            GameObject ceiling = _room.GetComponent<ObjectManager>().spawnCeiling(prefavTecho, newPos.transform, xSize, zSize, "mediaEsfera");
            GameObject techos = new GameObject();
            techos.transform.name = "Techo";
            techos.transform.SetParent(_room.transform);
            ceiling.transform.SetParent(techos.transform);
            _room.transform.localRotation = position.transform.localRotation;
            return _room;
        }

    }
}
