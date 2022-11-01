using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Mines : MonoBehaviour
{
    public TMP_Text mines;
    public TMP_Text deaths;
    public GameObject deaths_;
    public GameObject win;
    public GameObject lose;

    public GameObject z;
    public GameObject w;

    SquareMaker sm;
    TextMaker tm;
    Minefeels mf;
    TouchHandler th;

    float lossInterval = 0;
    float lossIntervalDuration = 1;
    bool lossIntervalStarted = false;
    // Start is called before the first frame update
    void Start()
    {
        sm = GetComponent<SquareMaker>();
        tm = GetComponent<TextMaker>();
        if (Menu.difficulty < 4) {
            mf = new Minefeels(sm, tm, Menu.dimension, Menu.size[Menu.dimension-2][Menu.difficulty], Menu.density[Menu.dimension-2][Menu.difficulty]);
        } else {
            mf = new Minefeels(sm, tm, Menu.dimension, Menu.customSize, Menu.customDensity);
        }
        th = GetComponent<TouchHandler>();
        th.mf = mf;
        th.ConfigureCollider();

        win.SetActive(false);
        lose.SetActive(false);
        deaths_.SetActive(Menu.noDeath);

        float halfTile = mf.trueTileSize * 1.5f;
        switch (Menu.dimension) {
            case 3:
                z.transform.position = new Vector3((-mf.chunkSize * 0.5f) - halfTile, (-mf.height * 0.5f) + halfTile, 0);
                w.transform.position = new Vector3(100000, 0, 0);
                break;
            case 4:
                z.transform.position = new Vector3((-mf.width * 0.5f) - halfTile, (-mf.height * 0.5f) + halfTile, 0);
                w.transform.position = new Vector3((-mf.width * 0.5f) + halfTile, (-mf.height * 0.5f) - halfTile, 0);
                break;
            default:
                z.transform.position = new Vector3(100000, 0, 0);
                w.transform.position = new Vector3(100000, 0, 0);
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        mf.Update();
        mines.text = $"mines: {mf.minefield.mines - mf.marked}";
        if (Menu.noDeath) {
            deaths.text = $"deaths: {mf.deaths}";
        }

        if (mf.minefield.tilesAmount - mf.totalVisited == mf.minefield.mines) {
            Win();
        }

        if (mf.deaths > 0 && !Menu.noDeath) {
            Loss();
        }

        if (lossIntervalStarted) {
            lossInterval += Time.deltaTime;
        }

        if (lossInterval > lossIntervalDuration) {
            lose.SetActive(true);
        }
    }

    void Win() {
        mf.inability = true;
        win.SetActive(true);
    }

    void Loss() {
        mf.inability = true;
        mf.RevealMines();
        lossIntervalStarted = true;
    }

    public void GoBack() {
        SceneManager.LoadScene(0);
    }
}
