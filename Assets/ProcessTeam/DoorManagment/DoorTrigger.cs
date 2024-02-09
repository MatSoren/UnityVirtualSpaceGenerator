using UnityEngine;
namespace ProcessTeam.DynamicRooms.PrefabsScript {

    public class DoorTrigger : MonoBehaviour {
        // Atributos
        [SerializeField]
        private DoorController _door;

        // cuando hay una colisión se llama a este evento. 
        // El collider que no tenga isTrigger será recibido por parámetro
        protected virtual void OnTriggerEnter(Collider other) {
            // si el que colisiona es el jugador
            if (other.tag == "Player") {
                // Debug.Log("Colisionó el jugador");

                // si la puerta está cerrada la abrimos
                if (!_door.isOpen) {
                    _door.Open(other.transform.position);
                }
            }
        }

        // cuando elcuerpo rígido deja de estar en contacto con el collider
        protected virtual void OnTriggerExit(Collider other) {
            // si el que deja de estar en contacto es el jugador
            if (other.tag == "Player") {
                // Debug.Log("Se fue el jugador");

                // si la puerta está abierta la cerramos
                if (_door.isOpen) {
                    _door.Close();
                }
            }
        }

    }
}
