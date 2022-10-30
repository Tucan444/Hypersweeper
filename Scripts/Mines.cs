using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mines : MonoBehaviour
{
    SquareMaker sm;
    Minefeels mf;
    TouchHandler th;
    // Start is called before the first frame update
    void Start()
    {
        sm = GetComponent<SquareMaker>();
        mf = new Minefeels(sm, tm, 4, 3);
        th = GetComponent<TouchHandler>();
        th.mf = mf;

        mf.ChangeAnchor(new int[4] {1, 2, 2, 0});
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
