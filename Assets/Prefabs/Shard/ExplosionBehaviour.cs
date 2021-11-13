using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionBehaviour : MonoBehaviour
{

    public ShardBehaviour ShardBehaviour;

    private Color Color;
    private bool _blowUp = false;
    public float Intensity = 1;
    
    private bool _flip;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetColor(Color color)
    {
        Color = color;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (_blowUp && transform.childCount == 0)
        {
            Destroy(gameObject);
        }
    }

    public void Blowup()
    {
        _blowUp = true;
        
        // TODO: replace with pool
        var shardCount = Random.Range(3, 5);
        var angleRange = 360f / shardCount;
        for (var i = 0; i < shardCount; i++)
        {
            var a = 360 * (i / (float) shardCount);
            a += Random.Range(-angleRange * .25f, angleRange * .25f);
            var aRad = Mathf.Deg2Rad * a;

            var d = Random.Range(1, 2f) * Intensity;
            var scaleRandom = Random.Range(0, 2);
            var scale = (2+scaleRandom) * Intensity;
            d += scaleRandom * .5f;
            
            var instance = Instantiate(ShardBehaviour, transform);
            instance.Controls.SetColors(Color, Color.black, .01f);
            instance.transform.eulerAngles = new Vector3(0, 0, a + Random.Range(-5, 5)*d + 90 - (_flip ? 180 : 0));
            instance.transform.localPosition = d * new Vector3(Mathf.Cos(aRad), Mathf.Sin(aRad), 0);
            instance.transform.localScale = new Vector3(.5f+ Random.Range(-.2f, .3f), scale , 1);
            instance.BlowUpAt = Time.realtimeSinceStartup + Random.Range(.4f, .6f);
        }
    }

    public void SetFlip(bool b)
    {
        _flip = b;
    }
}
