using ProcessTeam.DynamicRooms.BuildingManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class buildingButton : MonoBehaviour
{

    [SerializeField] public BuildingManager bm;
    [SerializeField] public string id;
    public bool changeColor = true;
    public bool addWindow = false;
    public bool addFloor = false;

    private int alternator = 0;
    private int pisoVentana = 0;
    private void OnMouseUp() {
        if (changeColor) {
            switch (alternator) {
                case 0:
                    bm.changeWholeMaterial(id,"pared externa","Concrete textures pack/pattern 08/Concrete pattern 08");
                    alternator = 1;
                    break;
                case 1:
                    bm.changeWholeMaterial(id,"pared externa","Ground textures pack/Ground 01/Ground pattern 01");
                    alternator = 0;
                    break;
                default:
                    break;
            }
        }
        if (addWindow) {
            if (bm.countFloorsBuilding(id) > pisoVentana) { 
                bm.addWindowAt(id,pisoVentana,"Sur","",5,1,5,1.5f);

                pisoVentana++;
            }
        }
        if (addFloor) {
            bm.addFloorAt(id,bm.countFloorsBuilding(id),"RoomVacio",4);
        }
    }
}
