using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

//class that handles all navigation
public class NavigationController : MonoBehaviour
{
    public GameObject trigger; // trigger to spawn and despawn AR arrows
    public Transform[] destinations; // list of destination positions
    public Transform target; // current choosen destination
    public GameObject person; // person indicator
    public Text text;  // information text box

    private NavMeshPath path; // current calculated path
    private LineRenderer line; // linerenderer to display path
    
    public Dropdown dropdown; // dropdown of destinations
    private bool destinationSet; // bool to say if a destination is set

    //create initial path, get linerenderer and fill dropdown.
    void Start()
    {
        path = new NavMeshPath();
        line = transform.GetComponent<LineRenderer>();
        PopulateList();
        destinationSet = false;
    }

    void Update()
    {
        //if a target is set, calculate and update path
        if(target != null)
        {
            NavMesh.CalculatePath(person.transform.position, target.position, NavMesh.AllAreas, path);
            //lost path due to standing above obstacle (drift)
            if(path.corners.Length == 0)
            {
                text.text = "Try moving away for obstacles (optionally recalibrate)";
            } else
            {
                text.text = "";
            }
            line.positionCount = path.corners.Length;
            line.SetPositions(path.corners);
            line.enabled = true;

        } 
    }

    //set current destination and create a trigger for showing AR arrows
    public void setDestination(int index)
    {
        target = destinations[index];
        GameObject.Instantiate(trigger, person.transform.position, person.transform.rotation);
    }

    // fill dropdown
    private void PopulateList()
    {
        List<string> names = new List<string>();
        names.Add("Choose a destination..");
        foreach(Transform dest in destinations)
        {
            names.Add(dest.name);
        }
        dropdown.AddOptions(names);
    }

    // dropdown listener
    public void DropdownIndexChanged(int index)
    {
        if(index == 0 && !destinationSet)
        {
            target = null;
            line.positionCount = 0;
        } else
        {
            if (destinationSet)
            {
                RemoveArrowAndCollider();
                setDestination(index);
            } else
            {
                dropdown.options.RemoveAt(0);
                dropdown.SetValueWithoutNotify(index - 1);
                setDestination(index - 1);
            }
            destinationSet = true;
        }
    }

    // clear button listener, delete current destination and repopulate dropdown
    public void Clear()
    {
        target = null;
        line.positionCount = 0;
        dropdown.ClearOptions();
        PopulateList();   
        dropdown.SetValueWithoutNotify(0);
        RemoveArrowAndCollider();
        destinationSet = false;
    }

    // remove AR arrow when path is cleared
    private void RemoveArrowAndCollider()
    {
        Destroy(GameObject.Find("NavTrigger(Clone)"));
        Destroy(GameObject.Find("Anchor"));
    }
}
