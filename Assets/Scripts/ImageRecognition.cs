using GoogleARCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using ZXing;

// class used for QR code detection, place on a gameobject
public class ImageRecognition : MonoBehaviour
{
    public GameObject FitToScanOverlay; //screen overlay for scanning
    public GameObject scanOverlay2; //screen overlay for scanning (start phase)
    public GameObject calibrationLocations; // transforms with calibration positions
    public GameObject person; // person indicator
    public GameObject controller; // indoornavcontroller object

    public Button btn; // scan button

    private bool searchingForMarker = false; // bool to say if looking for marker
    private bool first = true; // bool to fix multiple scan findings
    private int counter = 0; // counter used to change button color
    private Color normColor; // standard button color set at start
    private Color pressedColor; // pressed button color set at start

    public Text textField; // information text

    //used to set button colors
    private void Start()
    {
        var colors = btn.colors;
        normColor = colors.normalColor;
        pressedColor = colors.pressedColor;
    }

    // Update is called once per frame
    void Update()
    {
        if (searchingForMarker)
        {
            Scan();
        }
    }

    /// <summary>
    /// Capture and scan the current frame 
    /// </summary>
    void Scan()
    {
        System.Action<byte[], int, int> callback = (bytes, width, height) =>
        {
            if (bytes == null)
            {
                // No image is available.
                return;
            }

            // Decode the image using ZXing parser
            IBarcodeReader barcodeReader = new BarcodeReader();
            var result = barcodeReader.Decode(bytes, width, height, RGBLuminanceSource.BitmapFormat.Gray8);
            var resultText = result.Text;

            // result action
            if (first)
            {
                Relocate(resultText);
                first = false;
            }
        };

        CaptureScreenAsync(callback);
    }

    // move to person indicator to the new spot
    private void Relocate(string text)
    {
        text = text.Trim(); //remove spaces
        //find the correct location scanned and move the person to its position
        foreach (Transform child in calibrationLocations.transform)
        {
            if(child.name.Equals(text))
            {
                person.transform.position = child.position;
                btn.gameObject.GetComponent<Image>().color = normColor;
                textField.text = "";
                break;
            }
        }
        searchingForMarker = false;
        FitToScanOverlay.SetActive(false);
    }

    /// <summary>
    /// Capture the screen using CameraImage.AcquireCameraImageBytes.
    /// </summary>
    /// <param name="callback"></param>
    void CaptureScreenAsync(Action<byte[], int, int> callback)
    {
        Task.Run(() =>
        {
            byte[] imageByteArray = null;
            int width;
            int height;

            using (var imageBytes = Frame.CameraImage.AcquireCameraImageBytes())
            {
                if (!imageBytes.IsAvailable)
                {
                    callback(null, 0, 0);
                    return;
                }

                int bufferSize = imageBytes.YRowStride * imageBytes.Height;

                imageByteArray = new byte[bufferSize];

                Marshal.Copy(imageBytes.Y, imageByteArray, 0, bufferSize);

                width = imageBytes.Width;
                height = imageBytes.Height;
            }

            callback(imageByteArray, width, height);
        });
    }

    // handle scanmarker button click
    public void onClick()
    {
        counter++;
        if(counter % 2 == 1)
        {
            searchingForMarker = true;
            FitToScanOverlay.SetActive(true);
            first = true;
            btn.gameObject.GetComponent<Image>().color = pressedColor;
        } else
        {
            searchingForMarker = false;
            FitToScanOverlay.SetActive(false);
            first = false;
            btn.gameObject.GetComponent<Image>().color = normColor;
        }

    }

    // is used at start of application to set initial position
    public bool StartPosition(WebCamTexture wt)
    {
        bool succeeded = false;
        try
        {
            IBarcodeReader barcodeReader = new BarcodeReader();
            // decode the current frame
            var result = barcodeReader.Decode(wt.GetPixels32(),
              wt.width, wt.height);
            if (result != null)
            {
                Debug.Log("found: " + result.Text);
                Relocate(result.Text);
                scanOverlay2.SetActive(false);
                succeeded = true;
            }
        }
        catch (Exception ex) { Debug.LogWarning(ex.Message); }
        return succeeded;
    }
}
