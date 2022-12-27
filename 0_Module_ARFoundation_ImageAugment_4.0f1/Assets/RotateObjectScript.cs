using UnityEngine;
using System.Collections;
using UnityEngine.XR.ARSubsystems;

namespace Vuforia
{
    public class RotateObjectScript : MonoBehaviour
    {

        //private Transform defaultTransform;

        private Vector3 defaultScale;
        private Quaternion defaultRotation;

        private float oldDistance = 0;
        private float newDistance = 0;

        private float startScale;
        void Start()
        {
            //	defaultTransform = transform;

            startScale = transform.localScale.x;
            defaultRotation = transform.rotation;
            defaultScale = transform.localScale;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                Debug.Log(Input.GetTouch(0).deltaPosition.x);
                if (Input.GetTouch(0).deltaPosition.x > 0)
                {
                    transform.rotation *= Quaternion.Euler(0, -3f, 0);
                }
                else
                {
                    transform.rotation *= Quaternion.Euler(0, 3f, 0);
                }
            }
            else if (Input.touchCount > 1)
            {
                Debug.Log(Input.touchCount);
                if ((Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved) && oldDistance > 0)
                {
                    Vector2 posi_touch0 = Input.GetTouch(0).position;
                    Vector2 posi_touch1 = Input.GetTouch(1).position;

                    float maxScale;
                    float minScale;
                    float deltaScale;

                    //	if(playContents.tag == "Cadalog"){
                    maxScale = startScale * 2f;
                    minScale = startScale * 0.5f;
                    deltaScale = startScale * 0.1f;
                    //	}else{ //nameCard
                    //		maxScale = 0.56f;
                    //		minScale = 0.15f;
                    //		deltaScale = 0.008f;
                    //	}


                    newDistance = Mathf.Abs(posi_touch1.x - posi_touch0.x);

                    if (oldDistance < newDistance)
                    {
                        if (transform.localScale.x < maxScale)
                        {
                            transform.localScale += new Vector3(deltaScale, deltaScale, deltaScale);
                        }
                    }
                    else
                    {
                        if (transform.localScale.x > minScale)
                        {
                            transform.localScale -= new Vector3(deltaScale, deltaScale, deltaScale);
                        }
                    }

                }
                else if (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(1).phase == TouchPhase.Ended)
                {
                    oldDistance = 0;
                    newDistance = 0;
                }
                else
                {
                    Vector2 posi_ftouch0 = Input.GetTouch(0).position;
                    Vector2 posi_ftouch1 = Input.GetTouch(1).position;

                    oldDistance = 0;
                    newDistance = 0;

                    oldDistance = Mathf.Abs(posi_ftouch0.x - posi_ftouch1.x);
                }
            }
        }
    }
}