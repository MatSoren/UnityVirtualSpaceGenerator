using System.Collections;
using UnityEngine;
namespace ProcessTeam.DynamicRooms.PrefabsScript {
    public class DoorController : MonoBehaviour {
        // --- Atributos
        public bool isOpen = false;
        [Header("Velocity of the open door animation")]
        [SerializeField]
        private float _speed = 1f;

        [Header("Degrees ° of the open door")]
        [SerializeField]
        private float _rotationAmount = 90f;

        private float _forwardDirection = 0;

        private Vector3 _startRotation;
        private Vector3 _forward;

        private Coroutine animationCoroutine;

        // --- Métodos

        private void Awake() {
            // definimos las posición inicial de la puerta para cuando tenga que cerrarse 
            _startRotation = transform.rotation.eulerAngles;

            // definimos cual es la dirección de la puerta con la que nos compararemos
            _forward = transform.forward * -1;
        }

        public void Open(Vector3 userPosition) {
            // si está cerrada
            if (!isOpen) {
                // si se está ejecutando una animación se la para (para que no haya overlap)
                if (animationCoroutine != null) {
                    StopCoroutine(animationCoroutine);
                }

                // calculamos de que lado de la puerta estamos parados para ver a donde la debemos de abrir
                float dot = Vector3.Dot(_forward, (userPosition - transform.position).normalized);
                // Debug.Log($"Dot:{dot.ToString("N3")}");

                // llamamos a la corutina y la guardamos (para poder pararla si es necesario)
                animationCoroutine = StartCoroutine(DoRotationOpen(dot));
            }
        }

        private IEnumerator DoRotationOpen(float forwardAmount) {
            Quaternion startRotation = transform.rotation;
            Quaternion endRotation;

            if (forwardAmount >= _forwardDirection) {
                endRotation = Quaternion.Euler(new Vector3(0, _startRotation.y - _rotationAmount, 0));
                // Debug.Log($"Rotate To:{endRotation}");
            } else {
                endRotation = Quaternion.Euler(new Vector3(0, _startRotation.y + _rotationAmount, 0));
                // Debug.Log($"Rotate To:{endRotation}");
            }

            isOpen = true;
            float time = 0;
            while (time < 1) {
                transform.rotation = Quaternion.Slerp(startRotation, endRotation, time);
                yield return null;
                time += Time.deltaTime * _speed;
            }
        }

        public void Close() {
            // si está abierto
            if (isOpen) {
                // si se está ejecutando una animación se la para (para que no haya overlap)
                if (animationCoroutine != null) {
                    StopCoroutine(animationCoroutine);
                }
                // cerramos la puerta
                animationCoroutine = StartCoroutine(DoRotationClose());
            }
        }

        private IEnumerator DoRotationClose() {
            Quaternion startRotation = transform.rotation;
            Quaternion endRotation = Quaternion.Euler(_startRotation);

            isOpen = false;

            float time = 0;
            while (time < 1) {
                transform.rotation = Quaternion.Slerp(startRotation, endRotation, time);
                yield return null;
                time += Time.deltaTime * _speed;
            }
        }
    }
}