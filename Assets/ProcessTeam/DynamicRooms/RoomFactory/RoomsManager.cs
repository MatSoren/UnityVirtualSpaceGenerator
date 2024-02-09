using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProcessTeam.DynamicRooms.RoomFactory {
    public class RoomsManager : MonoBehaviour {
        
        private Dictionary<string,Type> _roomsByName;
        private Dictionary<string,Type> _strairByName;

        public void Start() {
            _roomsByName = new Dictionary<string,Type> {
                {"RoomVacio", typeof(RoomVacio)},
                {"RoomAbierto", typeof(RoomAbierto)},
                {"RoomVacioTecho",typeof(RoomVacioTecho) },
            };
            _strairByName = new Dictionary<string,Type> {
                {"escaleracaracol", typeof(EscaleraCaracol)},
                {"escalerarecta", typeof(EscaleraRecta)}
            };
        }

        public static bool IsRoomOperation(string data) {
            return data.StartsWith("Crear room") ||
                   data.StartsWith("Crear objeto") ||
                   data.StartsWith("Room microlearning") ||
                   data.StartsWith("Eliminar room") ||
                   data.StartsWith("Vaciar room") ||
                   data.StartsWith("Eliminar objeto") ||
                   data.StartsWith("Cambiar material");
        }

        public GameObject setAndCreateRoom(GameObject position,string roomType,float xSize,float zSize,float ySize,string roomName,
                            string piso = "",string pared = "",string techo = "") {
            if (!_roomsByName.ContainsKey(roomType)) return null;

            Type type = _roomsByName[roomType];
            var room = Activator.CreateInstance(type) as DynamicRoom;
            if (piso != "") {
                room.piso = piso;
            }
            if (pared != "") {
                room.pared = pared;
            }
            if (techo != "") {
                room.techo = techo;
            }

            GameObject createdRoom = room.CreateInstanceRoom(gameObject,0,position,xSize,zSize,ySize);
            createdRoom.name = roomName;

            return createdRoom;
        }

        public void setAndAddObject(GameObject room,string objectType,float xPos,float zPos,float angle,float scaleX,float scaleY,float scaleZ,string objectName) {
            if (room != null)
                room.GetComponent<ObjectManager>().SpawnObject(objectType,xPos,zPos,angle,scaleX,scaleY,scaleZ,objectName);
            else {
                Debug.Log("La room no es valida");
            }
        }

        public void removeObjects(GameObject room){
            if (room != null){
                room.GetComponent<ObjectManager>().removeObjects();
            }
            else {
                Debug.Log("La room no es valida");
            }

        }

        public void setAndAddComponentToObject(GameObject room,string idObject,string component) {
            if (room != null)
                room.GetComponent<ObjectManager>().addComponentToObject(idObject,component);
            else
                Debug.Log("La room no es valida");
        }

        public GameObject setAndAddStair(GameObject roomOrigen,GameObject roomDestino,float xPos,float zPos,string stairType) {
            if (roomOrigen == null) return null;
            var e = Activator.CreateInstance(_strairByName[stairType]) as DynamicStairs;
            GameObject escalera = e.createInstanceStair(stairType,roomOrigen,roomDestino,xPos,zPos);
            
            return escalera;
        }

        //Eventualmente podría haber mas de una forma de movilizarse, este sera el gestor dependiendo de las entradas
        public void addElevationMethod(GameObject room,float xPos,float zPos,float height,string mode = "1") {
            if (room != null) {
                switch (mode) {
                    case "1":

                        break;/*
                    case "2":
                        obMa.addOjbectStair();
                        break;
                    case "3":
                        obMa.addStairRoomOutside();
                        break;*/

                }

            }
        }

        public void ChangeMaterialAt(GameObject roomToUpdate,string objectType,string material) {
            roomToUpdate.GetComponent<ObjectManager>().ChangeMaterial(objectType,material);
        }
    }
}
