using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolvingController : MonoBehaviour
{
    public SkinnedMeshRenderer SkinnedMesh;
    public float dissolveRate = 0.0125f;
    public float refreshRate = 0.025f;

    private Material[] skinnedMaterials;
    void Start()

    {
        if (SkinnedMesh == null)
            skinnedMaterials = SkinnedMesh.sharedMaterials;

    }

    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.Escape))
    //     {
    //         StartCoroutine(Dissolveco());
    //     }
    // }

    IEnumerator Dissolveco ()

        {
            if (skinnedMaterials.Length >0)
            {
                float counter = 0;

                while (skinnedMaterials[0].GetFloat("_DissolveAmonunt") < 1)
                {

                    counter += dissolveRate;
                    for(int i = 0; i < skinnedMaterials.Length; i++)
                    {
                        skinnedMaterials[i].SetFloat("_DissolveAmonunt",counter);
                    }
                    yield return new WaitForSeconds(refreshRate);

                }
            }
        }
}