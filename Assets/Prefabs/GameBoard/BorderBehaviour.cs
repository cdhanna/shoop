using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BorderSide { LEFT, RIGHT, TOP, LOW }

public class BorderBehaviour : MonoBehaviour
{
    public Camera Cam;

    public BorderSide Side;
    
    
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (!Cam)
        {
            Cam = FindObjectOfType<Camera>();

        }
        float OrthoWidth = Cam.orthographicSize * Cam.aspect;
        float OrthoHeight = Cam.orthographicSize;
        switch (Side)
        {
            case BorderSide.LEFT:
                transform.position = new Vector3 (Cam.transform.position.x - OrthoWidth, 0.0F, 0.0F);
                break;
            
            case BorderSide.RIGHT:
                transform.position = new Vector3 (Cam.transform.position.x + OrthoWidth, 0.0F, 0.0F);
                break;
            
            case BorderSide.TOP:
                transform.position = new Vector3 (0f, Cam.transform.position.y + OrthoHeight, 0.0F);
                break;
            
            case BorderSide.LOW:
                transform.position = new Vector3 (0f, Cam.transform.position.y - OrthoHeight, 0.0F);
                break;
        }
        // RightArrow.transform.position = new Vector3 (transform.localPosition.x + OrthoWidth, 0.0F, 0.0F);
    }
}
