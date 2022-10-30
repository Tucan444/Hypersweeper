using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class clicki : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        Vector3 touchPosition = Extentions.GetTP();

        transform.position = touchPosition;

    }
}
