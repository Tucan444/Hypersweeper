using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        /* Minefeels mf = new Minefeels(GetComponent<SquareMaker>(), 4, 4);
        mf.ChangeAnchor(new int[4] {1, 0, 2, 3});
        mf.ChangeAnchor(new int[4] {1, 1, 1, 2}); */
        TextMaker tm = GetComponent<TextMaker>();
        GameObject t1 = tm.GetText();
        tm.ChangeText(t1, 13);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
