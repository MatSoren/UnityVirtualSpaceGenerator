using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

using UnityEngine; 

namespace ProcessTeam.DynamicRooms.RoomFactory {
    public abstract class DynamicStairs : MonoBehaviour {
        public abstract string Name { get; }
        public abstract GameObject createInstanceStair(string stairName,GameObject roomOrigen,GameObject roomDestino,float xPos,float zPos);
        public float xSize;
        public float zSize;
        public int cantClones;
        public float getXSize() { return this.xSize; }
        public float getZSize() { return this.zSize; }
        public int getCantClones() { return this.cantClones; }

        public abstract void agujerear(GameObject origen, GameObject destino, float xPosAgujero, float zPosAgujero);
    }

    public class EscaleraCaracol : DynamicStairs {
        public override string Name => "escaleracaracol";
        public GameObject stair;

        public override GameObject createInstanceStair(string stairName,GameObject roomOrigen,GameObject roomDestino,float xPos,float zPos) {
            float height = roomDestino.transform.position.y - roomOrigen.transform.position.y;

            this.stair = new GameObject("Escalera");
            this.stair.transform.Translate(roomOrigen.transform.position);


            Vector3 pos = new Vector3(this.stair.transform.position.x + xPos,this.stair.transform.position.y,this.stair.transform.position.z + zPos);
            GameObject stairToSpawn = Resources.Load<GameObject>("Prefabs/PrefabsPivot/" + stairName);
            GameObject escalera = Instantiate(stairToSpawn,pos,Quaternion.identity);
            escalera.transform.SetParent(this.stair.transform);

            int cantRotaciones = 1;
            for (int i = 0; i < 2; i++) 
                for(int j = 0; j < 2; j++) 
                    if (i == 0 && roomOrigen.GetComponent<ObjectManager>().haySuelo(i,j))
                         cantRotaciones = j + 1;
                    else
                        if (roomOrigen.GetComponent<ObjectManager>().haySuelo(i,j)) 
                            cantRotaciones = (-1)*j ;
                
            escalera.transform.Rotate(new Vector3(0,-90 * cantRotaciones,0));

            this.xSize = escalera.GetComponent<PrefabInfo>().xSize;
            this.zSize = escalera.GetComponent<PrefabInfo>().zSize;
           
            float alturaUnitariaEscalera = escalera.GetComponent<PrefabInfo>().ySize;
            float divAux = (height / alturaUnitariaEscalera);
            int cantClones = Mathf.FloorToInt(divAux);
            float escalado = divAux / (float) cantClones;
            this.cantClones = cantClones;
            GameObject lastInstance = escalera;
            pos = new Vector3(pos.x,pos.y + alturaUnitariaEscalera,pos.z);
            for (int i = 1; i < cantClones; i++) {
                GameObject auxEscalera = Instantiate(escalera,pos,Quaternion.Euler(0,-90 * (i+cantRotaciones),0));
                lastInstance = auxEscalera;
                auxEscalera.transform.SetParent(this.stair.transform);
                pos = new Vector3(pos.x,pos.y + alturaUnitariaEscalera,pos.z);
            }
            this.stair.transform.localScale = new Vector3(this.stair.transform.localScale.x,this.stair.transform.localScale.y * escalado,this.stair.transform.localScale.z);

            this.stair.transform.SetParent(roomOrigen.transform);
            
            agujerear(roomOrigen, roomDestino, xPos, zPos);
            
            float xPosAgujero = xPos - getXSize() / 2;
            float zPosAgujero = zPos - getZSize() / 2;
            
            int cantDeRotaciones = Mathf.Abs((int) (lastInstance.transform.localEulerAngles.y / 90));
            switch (cantDeRotaciones) {
                case 0:
                    roomDestino.GetComponent<ObjectManager>().rellenarPiso(xPosAgujero + 1,zPosAgujero,1,1);
                    roomOrigen.GetComponent<ObjectManager>().rellenarTecho(xPosAgujero + 1,zPosAgujero,1,1);
                    break;
                case 3:
                    roomDestino.GetComponent<ObjectManager>().rellenarPiso(xPosAgujero + 1,zPosAgujero + 1,1,1);
                    roomOrigen.GetComponent<ObjectManager>().rellenarTecho(xPosAgujero + 1,zPosAgujero + 1,1,1);
                    break;
                case 2:
                    roomDestino.GetComponent<ObjectManager>().rellenarPiso(xPosAgujero,zPosAgujero + 1,1,1);
                    roomOrigen.GetComponent<ObjectManager>().rellenarTecho(xPosAgujero,zPosAgujero + 1,1,1);
                    break;
                case 1:
                    roomDestino.GetComponent<ObjectManager>().rellenarPiso(xPosAgujero,zPosAgujero,1,1);
                    roomOrigen.GetComponent<ObjectManager>().rellenarTecho(xPosAgujero,zPosAgujero,1,1);
                    break;
            }

            this.stair.transform.SetParent(roomOrigen.transform);
            return this.stair;
        }

        public override void agujerear(GameObject roomOrigen, GameObject roomDestino, float xPos, float zPos)
        {
            float xSize = (float) base.getXSize();
            float zSize = (float) base.getZSize();
            float xPosAgujero = xPos - xSize / 2;
            float zPosAgujero = zPos - zSize / 2;
            roomDestino.GetComponent<ObjectManager>().agujerearPiso(xPosAgujero,zPosAgujero,xSize,zSize);
            roomOrigen.GetComponent<ObjectManager>().agujerearTecho(xPosAgujero,zPosAgujero,xSize,zSize);
        }
    }
    public class EscaleraRecta : DynamicStairs
    {
        public override string Name => "escalerarecta";
        public GameObject stair;

        public override GameObject createInstanceStair(string stairName, GameObject roomOrigen, GameObject roomDestino, float xPos, float zPos)
        {
            float height = roomDestino.transform.position.y - roomOrigen.transform.position.y;
            this.stair = new GameObject("Escalera");
            this.stair.transform.Translate(roomOrigen.transform.position);

            Vector3 pos = new Vector3(this.stair.transform.position.x + xPos, this.stair.transform.position.y, this.stair.transform.position.z + zPos);
            GameObject stairToSpawn = Resources.Load<GameObject>("Prefabs/PrefabsPivot/" + stairName);
            GameObject escalera = Instantiate(stairToSpawn, pos, Quaternion.identity);
            escalera.transform.SetParent(this.stair.transform);

            this.xSize = escalera.GetComponent<PrefabInfo>().xSize;
            this.zSize = escalera.GetComponent<PrefabInfo>().zSize;


            float alturaUnitariaEscalera = escalera.GetComponent<PrefabInfo>().ySize;
            float desplazamientoUnitario = escalera.GetComponent<PrefabInfo>().zSize;

            float divAux = (height / alturaUnitariaEscalera);
            int cantClones = Mathf.FloorToInt(divAux);
            float escalado = divAux / (float)cantClones;
            this.cantClones = cantClones;
            GameObject lastInstance = escalera;
            pos = new Vector3(pos.x, pos.y + alturaUnitariaEscalera, pos.z + desplazamientoUnitario);

            for (int i = 1; i < cantClones; i++)
            {
                GameObject auxEscalera = Instantiate(escalera, pos, Quaternion.identity);
                lastInstance = auxEscalera;
                auxEscalera.transform.SetParent(this.stair.transform);
                pos = new Vector3(pos.x, pos.y + alturaUnitariaEscalera, pos.z + desplazamientoUnitario);
            }
            this.stair.transform.localScale = new Vector3(this.stair.transform.localScale.x, this.stair.transform.localScale.y * escalado, this.stair.transform.localScale.z);

            this.stair.transform.SetParent(roomOrigen.transform);

            agujerear(roomOrigen, roomDestino, xPos, zPos);
            
            this.stair.transform.localRotation = Quaternion.Euler(0,0,0);
            
            return this.stair;
        }

        public override void agujerear(GameObject roomOrigen, GameObject roomDestino, float xPos, float zPos)
        {
            float xSize = getXSize();
            float zSize = getZSize() * cantClones / 2;
            float xPosAgujero = xPos - xSize / 2;
            float zPosAgujero = xPos + getZSize() * cantClones / 2;
            roomDestino.GetComponent<ObjectManager>().agujerearPiso(xPosAgujero, zPosAgujero, xSize, zSize);
            roomOrigen.GetComponent<ObjectManager>().agujerearTecho(xPosAgujero, zPosAgujero, xSize, zSize);
        }
    }
}