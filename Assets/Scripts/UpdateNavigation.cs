using GoogleARCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//used to update AR stuff using colliders
public class UpdateNavigation : MonoBehaviour
{
    public GameObject trigger; // collider to change arrows
    public GameObject indicator; // person indicator moving in the map
    public GameObject arcoreDeviceCam; // ar camera
    public GameObject arrowHelper; // box facing the arrow of person indicator used to calculate spawned AR arrow direction
    public LineRenderer line; // line renderer used to calculate spawned AR arrow direction
    public GameObject pin; // what to span at destination

    private Anchor anchor; //spawned anchor when putting somthing AR on screen
    private bool hasEntered; //used for onenter collider
    private bool hasExited; //used for onexit collider

    public Text text; // information text

    private void Start()
    {
        hasEntered = false;
        hasExited = false;
    }

    private void Update()
    {
        hasEntered = false;
        hasExited = false;
    }

    //what to do when entering a collider
    private void OnTriggerEnter(Collider other)
    {
        //if it is a navTrigger then caculate angle and spawn a new AR arrow
        if (other.name.Equals("NavTrigger(Clone)") && line.positionCount > 0)
        {
            if (hasEntered)
            {
                return;
            }
            hasEntered = true;
            Debug.Log("Entered collider");
            //logic to calculate arrow angle
            Vector2 personPos = new Vector2(this.transform.position.x, this.transform.position.z);
            Vector2 personHelp = new Vector2(arrowHelper.transform.position.x, arrowHelper.transform.position.z);
            Vector3 node3D = line.GetPosition(1);
            Vector2 node2D = new Vector2(node3D.x, node3D.z);
            float angle = Mathf.Rad2Deg * (Mathf.Atan2(personHelp.y - personPos.y, personHelp.x - personPos.x) -
                    Mathf.Atan2(node2D.y - personPos.y, node2D.x - personPos.x));

            // position arrow a bit before the camera and a bit lower
            Vector3 pos = arcoreDeviceCam.transform.position + arcoreDeviceCam.transform.forward * 2 + arcoreDeviceCam.transform.up * -0.5f;
            // rotate arrow a bit
            Quaternion rot = arcoreDeviceCam.transform.rotation * Quaternion.Euler(20, 180, 0);
            // create new anchor
            anchor = Session.CreateAnchor(new Pose(pos, rot));
            //spawn arrow
            GameObject spawned = GameObject.Instantiate(indicator, anchor.transform.position, anchor.transform.rotation, anchor.transform);
            // use calculated angle on spawned arrow
            spawned.transform.Rotate(0, angle, 0, Space.Self);
        }
        // if it is a destination spawn a pin and delete current arrow + current path + delete current destination
        if (other.tag.Equals("destination") && line.positionCount > 0 && GameObject.Find("PathShower").GetComponent<NavigationController>().target.name.Equals(other.name))
        {
            Destroy(GameObject.Find("NavTrigger(Clone)"));
            Destroy(GameObject.Find("Anchor"));
            GameObject.Find("PathShower").GetComponent<NavigationController>().target = null;
            line.positionCount = 0;
            Vector3 pos = arcoreDeviceCam.transform.position + arcoreDeviceCam.transform.forward * 2 + arcoreDeviceCam.transform.up * -0.3f;
            Quaternion rot = arcoreDeviceCam.transform.rotation;
            anchor = Session.CreateAnchor(new Pose(pos, rot));
            GameObject.Instantiate(pin, anchor.transform.position, anchor.transform.rotation, anchor.transform);
            Debug.Log("Arrived at " + other.name);
            //GameObject.Instantiate(trigger, this.transform.position, this.transform.rotation);
        }
        // show calibration text nearby calibration points
        if (other.tag.Equals("calibration"))
        {
            text.text = "If your position seems of, try scanning a nearby marker.";
        }
    }

    //what to do when exiting a collider
    private void OnTriggerExit(Collider other)
    {
        //if it is a navTrigger then delete Anchor and arrow and create a new trigger
        if (other.name.Equals("NavTrigger(Clone)"))
        {
            if (hasExited)
            {
                return;
            }
            hasExited = true;
            Debug.Log("Exited collider");
            Destroy(GameObject.Find("NavTrigger(Clone)"));
            Destroy(GameObject.Find("Anchor"));
            GameObject.Instantiate(trigger, this.transform.position, this.transform.rotation);
        }
        // if destination than delete anchor
        if (other.tag.Equals("destination") && GameObject.Find("PathShower").GetComponent<NavigationController>().target.name.Equals(other.name))
        {
            Destroy(GameObject.Find("Anchor"));
        }
        //if calibration than reset text
        if (other.tag.Equals("calibration"))
        {
            text.text = "";
        }
    }
}
