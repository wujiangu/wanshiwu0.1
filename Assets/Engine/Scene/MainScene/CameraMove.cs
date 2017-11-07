using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour {

    private Vector2 srcVec;
    private Vector2 dstVec;
    private float offSet = 0.014f;
	// Use this for initialization
	void Start () {
        //cs.SceneManager.Get().LoadScene(cs.SceneManager.LOGINSCENE);
	}
	
	// Update is called once per frame
	void Update () {

        if (MapBasicData.IsSelect) return;

        if (Input.GetMouseButtonDown(0))
        {
            srcVec = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            dstVec = Input.mousePosition;
            float dstX = dstVec.x - srcVec.x;
            float dstY = dstVec.y - srcVec.y;
            srcVec = dstVec;
            float xDir = -dstX * offSet;
            float yDir = -dstY * offSet;

            transform.position += new Vector3(xDir, yDir, 0);
           
             if (transform.position.x <= MapBasicData.CameraLeft)
             {
                 transform.position = new Vector3(MapBasicData.CameraLeft, transform.position.y, transform.position.z);
             }

             if (transform.position.x >= MapBasicData.CameraRight)
            {
                transform.position = new Vector3(MapBasicData.CameraRight, transform.position.y, transform.position.z);
            }

             if (transform.position.y >= MapBasicData.CameraUp)
             {
                 transform.position = new Vector3(transform.position.x, MapBasicData.CameraUp, transform.position.z);
             }

             if (transform.position.y <= MapBasicData.CameraDown)
             {
                 transform.position = new Vector3(transform.position.x, MapBasicData.CameraDown, transform.position.z);
             }
        }
	}
}
