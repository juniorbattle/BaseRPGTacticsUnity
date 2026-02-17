using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticsCamera : MonoBehaviour
{
    private static TacticsCamera _instance;

    public static TacticsCamera Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<TacticsCamera>();

                if (_instance == null)
                {
                    GameObject go = new GameObject("TacticsCamera");
                    _instance = go.AddComponent<TacticsCamera>();
                }
            }
            return _instance;
        }
    }
    
    private Transform target;
    private float followSpeed = 5f;

    public Vector3 offset;
    private bool offsetFocused = false;

    private float rotationSpeed = 180f; // Vitesse de rotation en degrés par seconde
    private float targetRotationAngle = 0f; // L'angle de rotation cible
    private float currentRotationAngle = 0f; // L'angle de rotation actuel
    private bool isRotating = false; // Est-ce que la caméra est en train de tourner ?

    private float targetRotationX = 0f; 

    void Update()
    {
        if (target != null)
        {
            Vector3 targetPosition = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(targetRotationX, transform.rotation.eulerAngles.y, 0);

            //transform.LookAt(target);
        }

        if (isRotating)
        {
            float step = rotationSpeed * Time.deltaTime;

            if (Mathf.Abs(currentRotationAngle - targetRotationAngle) > step)
            {
                float rotationStep = Mathf.MoveTowards(currentRotationAngle, targetRotationAngle, step);
                transform.Rotate(Vector3.up, rotationStep - currentRotationAngle, Space.Self);
                currentRotationAngle = rotationStep;
            }
            else
            {
                transform.Rotate(Vector3.up, targetRotationAngle - currentRotationAngle, Space.Self);
                currentRotationAngle = targetRotationAngle;
                isRotating = false;
            }
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void ChangeOffset()
    {
        offsetFocused = !offsetFocused;
        
        if (offsetFocused)
        {
            offset = new Vector3(0f, 3f, 0f);
            targetRotationX = -30f; // Rotation sur l'axe X à 0°
        }
        else 
        {
            offset = new Vector3(0f, 20f, 0f);
            targetRotationX = 0f; // Rotation sur l'axe X à 0°
        }
    }

    public void RotateLeft()
    {
        targetRotationAngle = currentRotationAngle + 90f;
        isRotating = true;
    }

    public void RotateRight()
    {
        targetRotationAngle = currentRotationAngle - 90f;
        isRotating = true;
    }
}
