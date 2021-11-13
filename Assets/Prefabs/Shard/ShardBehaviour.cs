using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShardBehaviour : MonoBehaviour
{
    public GamePieceShaderControls Controls;
    private bool _blowingUp;
    public float BlowUpAt { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!_blowingUp && BlowUpAt > 0 && Time.realtimeSinceStartup > BlowUpAt)
        {
            Blowup();
        }
    }

    [ContextMenu("Blow up")]
    public void Blowup()
    {
        _blowingUp = true;
        var scale = transform.localScale;
        var endScale = transform.localScale + new Vector3(.4f, 1f, 0);
        
        Controls.SetColors(new Color(0, 0, 0, 0), Color.black, .4f);
        Controls.SetSDFProperties(0f, 0f, .5f, (r) =>
        {

            transform.localScale = Vector3.Lerp(scale, endScale, r);
            if (r > .99f)
            {
                Destroy(gameObject);
            }
        });
        
    }
}
