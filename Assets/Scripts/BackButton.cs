using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButton : MonoBehaviour
{
    float duration;

    void Start() {
        duration = 0;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (duration > 0.4f) {
            if (Input.GetKey(KeyCode.Escape)) {
                if (SceneManager.GetActiveScene().buildIndex == 0) {
                    Application.Quit();
                } else {
                    SceneManager.LoadScene(0);
                }
            }
        } else {
            duration += Time.deltaTime;
        }
    }
}
