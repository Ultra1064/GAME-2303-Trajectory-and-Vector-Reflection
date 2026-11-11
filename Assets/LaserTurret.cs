using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LaserTurret : MonoBehaviour
{
    [SerializeField] LayerMask targetLayer;
    [SerializeField] GameObject crosshair;
    [SerializeField] float baseTurnSpeed = 3;
    [SerializeField] GameObject gun;
    [SerializeField] Transform turretBase;
    [SerializeField] Transform barrelEnd;
    [SerializeField] LineRenderer line;

    List<Vector3> laserPoints = new List<Vector3>();

    public int reflectMax = 5;
    int reflectCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        TrackMouse();
        TurnBase();

        laserPoints.Clear();
        laserPoints.Add(barrelEnd.position);

        if(Physics.Raycast(barrelEnd.position, barrelEnd.forward, out RaycastHit hit, 1000.0f, targetLayer))
        {
            laserPoints.Add(hit.point);
            Reflect(hit.point.normalized, hit.normal.normalized);
        }

        line.positionCount = laserPoints.Count;
        for(int i = 0; i < line.positionCount; i++)
        {
            line.SetPosition(i, laserPoints[i]);
        }
    }

    void Reflect(Vector3 hitpoint, Vector3 hitnormal)
    {
        reflectCount++; //Needed reflect count because a stack overflow happened
        Vector3 math = MathTime(hitpoint, hitnormal);
        //laserPoints.Add(hitpoint - 2f * Vector3.Dot(hitpoint, hitnormal) * hitnormal); //Need to figure out how to extend laser when it no longer reflects.
        if(reflectCount < reflectMax && Physics.Raycast(hitpoint, math, out RaycastHit hit, 1000.0f, targetLayer)) 
        {
            laserPoints.Add(hit.point);
            Reflect(hit.point.normalized, hit.normal.normalized);
        }
        else
        {
            laserPoints.Add(math * 1000f);
            reflectCount = 0;
        }
    }

    Vector3 MathTime(Vector3 hitpoint, Vector3 hitnormal)
    {
        return hitpoint - 2f * Vector3.Dot(hitpoint, hitnormal) * hitnormal; //Need to research this formula further.
    }

    void TrackMouse()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if(Physics.Raycast(cameraRay, out hit, 1000, targetLayer ))
        {
            crosshair.transform.forward = hit.normal;
            crosshair.transform.position = hit.point + hit.normal * 0.1f;
        }
    }

    void TurnBase()
    {
        Vector3 directionToTarget = (crosshair.transform.position - turretBase.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, directionToTarget.y, directionToTarget.z));
        turretBase.transform.rotation = Quaternion.Slerp(turretBase.transform.rotation, lookRotation, Time.deltaTime * baseTurnSpeed);
    }
}
