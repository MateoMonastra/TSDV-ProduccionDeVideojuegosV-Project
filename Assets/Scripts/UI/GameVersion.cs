using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameVersion : MonoBehaviour
{
    [SerializeField] private TMP_Text version;

    private void Start()
    {
        version.text = Application.version;
    }
}
