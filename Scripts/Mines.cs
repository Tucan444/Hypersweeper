using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mines : MonoBehaviour
{
    SquareMaker sm;
    TextMaker tm;
    Minefeels mf;
    TouchHandler th;
    // Start is called before the first frame update
    void Start()
    {
        sm = GetComponent<SquareMaker>();
        tm = GetComponent<TextMaker>();
        mf = new Minefeels(sm, tm, 4, 4, 0.05f);
        th = GetComponent<TouchHandler>();
        th.mf = mf;
    }

    // Update is called once per frame
    void Update()
    {
        mf.Update();
    }
}
