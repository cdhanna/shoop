using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VersionLabelBehaviour : MonoBehaviour
{
    public TextMeshProUGUI Version;
    // Start is called before the first frame update
    void Start()
    {
        Version.text = Application.version;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
