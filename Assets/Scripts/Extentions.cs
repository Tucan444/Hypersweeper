using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extentions
{
    public static Vector3 TP( this Touch touch ) {
        Vector3 touchPosition = Camera.main.ScreenToWorldPoint(touch.position);
        touchPosition.z = 0;
        return touchPosition;
    }

    public static Vector3 GetTP(int index=0) {
        return Input.GetTouch(index).TP();
    }

    public static Vector3 GetSTP(int index=0) {
        return Input.GetTouch(index).position;
    }

    public static Vector3 GetTPTraced(Camera cam, int index=0) {
        Ray spear = cam.ScreenPointToRay(Input.GetTouch(index).position);
        float faraway = spear.origin.z;
        return spear.GetPoint(-(faraway/spear.direction.z));
    }
}
