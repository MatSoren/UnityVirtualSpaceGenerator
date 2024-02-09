using System;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetection : MonoBehaviour {
    // hashtable que guarda el <id,gameObject>
    [SerializeField]
    private Dictionary<int, GameObject> _objectCollisioning = new Dictionary<int, GameObject>();

    private string[] _collidersToAvoid = new string[1] { "puertaWrapper" };

    // --- Methods

    protected virtual void OnTriggerEnter(Collider other) {
        // evitamos considerar colliders que no aportan información<
        if (Array.Exists(_collidersToAvoid, element => element == other.gameObject.name)) {
            return;
        }

        // si el gameObject no está en nuestra tabla la agregamos
        if (!_objectCollisioning.ContainsKey(other.gameObject.GetInstanceID())) {
            _objectCollisioning.Add(other.gameObject.GetInstanceID(), other.gameObject);
        }
    }

    // cuando el cuerpo rígido deja de estar en contacto con el collider
    protected virtual void OnTriggerExit(Collider other) {
        // evitamos considerar colliders que no aportan información
        if (Array.Exists(_collidersToAvoid, element => element == other.gameObject.name)) {
            return;
        }

        // si el GameObject está en nuestra tabla la borramos porque dejó de colisionar
        if (_objectCollisioning.ContainsKey(other.gameObject.GetInstanceID())) {
            _objectCollisioning.Remove(other.gameObject.GetInstanceID());
        }
    }

    public Dictionary<int, GameObject> getTableOfObjectsCollisioning() {
        //shallow copy, can access to the GameObjects!!
        return new Dictionary<int, GameObject>(_objectCollisioning);
    }

    // retorna si el objeto está colisionando o no
    public bool isObjectCollisioning(int idObject) {
        return _objectCollisioning.ContainsKey(idObject);
    }

}
