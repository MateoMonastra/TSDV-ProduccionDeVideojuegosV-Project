using System;
using UnityEngine;

public class BlobShadow : MonoBehaviour
{
    [SerializeField] private GameObject shadowObject;
    [SerializeField] private float offset;
    private RaycastHit _hit;

    private void LateUpdate()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out _hit, LayerMask.GetMask("Environment")))
        {
            shadowObject.transform.position = _hit.point + Vector3.up * offset;
        }
    }
}