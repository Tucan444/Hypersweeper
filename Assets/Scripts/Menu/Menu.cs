using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Menu : MonoBehaviour
{
    public Toggle ndtoggle;

    public TMP_Dropdown difficultyy;
    public TMP_Dropdown dimensionn;

    public GameObject sizeSlider;
    public GameObject densitySlider;

    public Slider sslider;
    public Slider dslider;

    public TMP_Text sizeText;
    public TMP_Text densityText;

    public static int[][] size = new int[][] {
        new int[] {10, 20, 40, 60},
        new int[] {5, 7, 10, 12},
        new int[] {4, 5, 6, 7}
    };

    public static float[][] density = new float[][] {
        new float[] {0.1f, 0.15f, 0.2f, 0.25f},
        new float[] {0.08f, 0.1f, 0.115f, 0.12f},
        new float[] {0.05f, 0.065f, 0.075f, 0.095f}
    };

    public static int dimension = 2;
    public static int difficulty = 1;
    public static bool noDeath = false;

    public static int customSize;
    public static float customDensity;

    static float ssliderv = 0;
    static float dsliderv = 0;

    int[][] sizeMap = new int[][] {
        new int[] {5, 120},
        new int[] {4, 40},
        new int[] {3, 16}
    };

    float[][] densityMap = new float[][] {
        new float[] {0.05f, 0.6f},
        new float[] {0.02f, 0.3f},
        new float[] {0.015f, 0.2f}
    };

    void Start() {
        difficultyy.value = difficulty;
        dimensionn.value = dimension - 2;

        if (difficulty != 4) {
            sizeSlider.SetActive(false);
            densitySlider.SetActive(false);
        } else {
            sslider.value = ssliderv;
            dslider.value = dsliderv;
            UpdateCustom();
        }

        ndtoggle.isOn = noDeath;
    }

    void Update() {
        if (difficulty == 4) {
            UpdateCustom();
        }
    }

    public void Controls() {
        SceneManager.LoadScene(2);
    }

    public void Play() {
        SceneManager.LoadScene(1);
    }

    public void DifficultyChange(int i) {
        int prevD = difficulty;
        difficulty = i;
        if (difficulty != 4 && prevD == 4) {
            sizeSlider.SetActive(false);
            densitySlider.SetActive(false);
        } else if (difficulty == 4 && prevD != 4) {
            sizeSlider.SetActive(true);
            densitySlider.SetActive(true);
            UpdateCustom();
        }
    }

    public void DimensionChange(int i) {
        dimension = i+2;
    } 

    public void NoDeathChange(bool a) {
        noDeath = a;
    }

    public void UpdateCustom() {
        ssliderv = sslider.value;
        dsliderv = dslider.value;

        customSize = getSize();
        customDensity = getDensity();

        sizeText.text = $"size: {customSize}";
        //int d2r = (int)(customDensity * 100);
        densityText.text = $"density: {(int)(customDensity * 100)}%";
    }

    // getter
    public int getSize() {
        return (int)((sslider.value * (float)sizeMap[dimension - 2][1]) + ((1f - sslider.value) * (float)sizeMap[dimension - 2][0]));
    }

    public float getDensity() {
        return (dslider.value * densityMap[dimension - 2][1]) + ((1f - dslider.value) * densityMap[dimension - 2][0]);
    }
}
