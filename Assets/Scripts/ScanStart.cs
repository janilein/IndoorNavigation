using GoogleARCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//class used at start of app to initialize position
public class ScanStart : MonoBehaviour
{
    public GameObject ardevice; //ARCore device gameobject

    private bool camAvailable; //bool used for seeing if rendering with camera is possible
    private WebCamTexture backCam; //used to obtain video from device camera
    private Texture defaultBackground; 

    public RawImage background; // where to render to
    public AspectRatioFitter fit; //fit rendered view to screen

    public ImageRecognition imgRec; //object used to access method for setting location
    public GameObject text; // start text
    public GameObject scanOverlay; //start overlay

    //setup logic to capture camera video
    private void Start()
    {
        defaultBackground = background.texture;
        WebCamDevice[] devices = WebCamTexture.devices;

        if(devices.Length == 0)
        {
            Debug.Log("No camera detected");
            camAvailable = false;
            return;
        }

        for(int i = 0; i < devices.Length; i++)
        {
            if (!devices[i].isFrontFacing)
            {
                backCam = new WebCamTexture(devices[i].name, Screen.width, Screen.height);
            }
        }

        if(backCam == null)
        {
            Debug.Log("unable to find backcam");
            return;
        }

        backCam.Play();
        background.texture = backCam;

        camAvailable = true;
    }

    //if camera setup render each frame the obtained images
    private void Update()
    {
        if (!camAvailable)
        {
            return;
        }

        float ratio = (float)backCam.width / (float)backCam.height;
        fit.aspectRatio = ratio;

        float scaleY = backCam.videoVerticallyMirrored ? -1f: 1f;
        background.rectTransform.localScale = new Vector3(1f, scaleY, 1f);

        int orient = -backCam.videoRotationAngle;
        background.rectTransform.localEulerAngles = new Vector3(0, 0, orient);

        bool result = imgRec.StartPosition(backCam);
        //if result found that close this view and start ar application
        if (result)
        {
            ardevice.GetComponent<ARCoreSession>().enabled = true;
            background.gameObject.SetActive(false);
            text.SetActive(false);
            scanOverlay.SetActive(false);
            this.gameObject.SetActive(false);
        } 
    }

    //used for testing
    public void OnClick()
    {
        ardevice.GetComponent<ARCoreSession>().enabled = true;
        background.gameObject.SetActive(false);
        text.SetActive(false);
        scanOverlay.SetActive(false);
        this.gameObject.SetActive(false);
    }

    
}
