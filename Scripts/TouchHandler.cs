using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchHandler : MonoBehaviour
{
    // specific use properties
    public Minefeels mf;


    public Camera cam;
    public float maxCamSize = 30;
    public float moveSpeed = 6;
    [Range(0.0001f, 0.9f)]public float endurance_ = 0.8f;
    [Range(0.1f, 0.999f)] public float screenSpeed_ = 0.3f;
    [Range(0.8f, 1)] public float eveid = 0.9f;

    float defCamSize;

    int prevT = 0; // n of touches in previous update
    int tcount = 0; // touch count

    float ttime = 0; // time touch 0 is touched
    float tthershold = 0.2f;
    float dthreshold = 0.1f;

    float scalent = 1; // initial distance between 2 fingers
    Vector3 center = new Vector3(); // center of 2 fingers
    Vector3 prevC = new Vector3();
    bool reinitiate = false;

    Vector3 pos1 = new Vector3();
    Vector3 pos2 = new Vector3();

    Vector3 prev1 = new Vector3();
    Vector3 prev2 = new Vector3();

    Vector3 cameraPos;
    Vector3 virtualPos;
    Vector3 endurance = new Vector3();

    Vector3 movePoint = new Vector3(); // distance traveled
    bool moveMode = false;
    bool ignoreMode = false;

    Touch t1; Touch t2;

    void Start() {
        cameraPos = cam.transform.position;
        virtualPos = cam.transform.position;
        defCamSize = cam.orthographicSize;
    }

    // Update is called once per frame
    void Update()
    {
        tcount = Mathf.Min(Input.touchCount, 2);

        if (tcount > 0){
            pos1 = Extentions.GetTP(0);
            t1 = Input.GetTouch(0);
        }
        if (tcount > 1) {
            pos2 = Extentions.GetTP(1);
            t2 = Input.GetTouch(1);
        }

        // upline
        if (prevT == 0 && tcount != prevT) {
            endurance = new Vector3();
            virtualPos = cam.transform.position;
        }

        if (prevT == 0 && tcount == 1) {  // 0 > 1
            prev1 = pos1;
        }

        if ((prevT < tcount && tcount == 2) || (reinitiate && tcount == 2)) {  // 0 > 2, 1 > 2
            // initiate 2 fingers
            ttime = 0;
            scalent = Vector2.Distance(Extentions.GetSTP(), Extentions.GetSTP(1));
            center = (pos1 + pos2) / 2;
            moveMode = true;
            reinitiate = false;


        } else if (prevT > tcount && tcount == 0) {  // downline 2 > 0, 1 > 0
            Reset();
        } else if (prevT == 2 && tcount == 1) {  // downline 2 > 1
            prev1 = pos1;

        } else if (tcount == 2) { // 2 lifespan
            center = (pos1 + pos2) / 2;
            movePoint -= (center - prevC) * moveSpeed;
            virtualPos += movePoint;
            movePoint = new Vector3();

            cam.orthographicSize = Mathf.Min(maxCamSize, defCamSize * (scalent / Vector2.Distance(Extentions.GetSTP(), Extentions.GetSTP(1))));

            // checking death
            if ((Input.GetTouch(0).phase == TouchPhase.Ended) || (Input.GetTouch(1).phase == TouchPhase.Ended)) {
                defCamSize = cam.orthographicSize;
                reinitiate = true;
            }

        } else if (tcount == 1) { // 1 lifespan
            if (!ignoreMode) {
                ttime += Time.deltaTime;
                movePoint -= (pos1 - prev1) * moveSpeed;
                if (movePoint.magnitude > dthreshold || moveMode) { // move
                    virtualPos += movePoint;
                    movePoint = new Vector3();
                    moveMode = true;
                } else {
                    if (ttime > tthershold) { // hold
                        Hold();
                        ignoreMode = true;
                    } else {
                        if (Input.GetTouch(0).phase == TouchPhase.Ended) { // tap
                            Tap();
                            Reset();
                            endurance = new Vector3();
                            virtualPos = cam.transform.position;
                        }
                    }
                }
            }
        }
        

        // backline for updating camera pos
        float ddd = 1f - ((1 - endurance_) * Time.deltaTime * 15);
        float ttt = 1f - (screenSpeed_) * Time.deltaTime * 15;
        float eee = 0.9f * Time.deltaTime * 15;
        if (tcount == 0) {
            cameraPos += endurance * Time.deltaTime;
            endurance *= ddd;
            if (PointOut()) {
                endurance *= ddd;
                Vector2 cp = GetComponent<BoxCollider2D>().ClosestPoint(cameraPos);
                cameraPos = Vector3.Lerp(cameraPos, new Vector3(cp.x, cp.y, cameraPos.z), eee);
            }
        } else {
            cameraPos = Vector3.Lerp(virtualPos, cameraPos, ttt);
            if (PointOut()) {
                Vector2 cp = GetComponent<BoxCollider2D>().ClosestPoint(virtualPos);
                virtualPos = Vector3.Lerp(virtualPos, new Vector3(cp.x, cp.y, virtualPos.z), eee);
            }
        }

        cam.transform.position = cameraPos;


        prevT = tcount;
        prev1 = pos1;
        prev2 = pos2;
        prevC = center;
    }

    public bool PointOut() {
        if (!GetComponent<BoxCollider2D>().OverlapPoint(new Vector2(cam.transform.position.x, cam.transform.position.y))) {
            return true;
        }
        return false;
    }

    public void Reset() {
        ttime = 0;
        movePoint = new Vector3();
        endurance = virtualPos - cameraPos;
        moveMode = false;
        ignoreMode = false;
        prev1 = new Vector3();
        prev2 = new Vector3();
    }

    public void Tap() {Debug.Log("tap");}

    public void Hold() {
        mf.Hold((Vector2)Extentions.GetTP());
    }
}
