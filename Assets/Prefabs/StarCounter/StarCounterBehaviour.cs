using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class StarCounterBehaviour : MonoBehaviour
{

    public List<StarBehaviour> Stars;
    public GamePieceShaderControls Dest;
    public MultiDigitBehaviour Text;

    public GameBoardBehaviour BoardBehaviour;
    
    public SoundManifestObject SoundManifestObject;
    public StarStateProvider StarStateProvider;
    public AudioSource DialSource;

    private bool _isAnimating;
    
    // Start is called before the first frame update
    void Start()
    {
        Text.Number = StarStateProvider.GetState().Stars;

        BoardBehaviour = FindObjectOfType<GameBoardBehaviour>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_isAnimating) return;
        
        var diff = StarStateProvider.GetState().Stars - Text.Number;
        if (diff != 0)
        {
            var signDiff = (int) Mathf.Sign(diff);
            Text.SetNumber(Text.Number + signDiff);
            _isAnimating = true;
            DialSource.PlayOneShot(SoundManifestObject.SpendStarSounds.GetRandom());
            transform.DOPunchScale(transform.localScale * .1f, .17f).OnComplete(() => _isAnimating = false);
        }

    }


    [ContextMenu("Clear")]
    public void Clear()
    {
        
        StarStateProvider.GetState().ResetStars();
    }
    
    public IEnumerator TakeNewStars(int newStarsToCollect)
    {
        // identify the stars that are valid. 
        yield return new WaitForSecondsRealtime(.1f);

        if (BoardBehaviour.Flags.DisableStarCounter) yield break;

        var winners = Stars.Where(s => s._isWin).ToList();


        _isAnimating = true;
        var velocity = (Vector3.right + Vector3.down) * 34;
        var scaleVel = Vector3.one;
        
        foreach (var winner in winners)
        {
            newStarsToCollect--;
            if (newStarsToCollect < 0)
            {
                yield return new WaitForSeconds(.1f);

                winner.transform.DOPunchScale(Vector3.one * .4f, .2f);
                winner.DialSource.PlayOneShot(SoundManifestObject.ShowStarGain);

                yield return new WaitForSeconds(.3f);
                continue;
            }
            // animate this towards the dest, and when it gets there, make the desk shroom up. 

            velocity = (Vector3.right * 4 + Vector3.down * 6);
            var isCloseEnough = false;
            winner.transform.DOScale(Dest.transform.localScale, .1f);
            winner.transform.DOBlendableLocalRotateBy(new Vector3(0, 0, 180), .1f);
            
            winner.DialSource.PlayOneShot(SoundManifestObject.AcceptSound);

            while (!isCloseEnough)
            {
                winner.transform.position =
                    Vector3.SmoothDamp(winner.transform.position, Dest.transform.position, ref velocity, .1f);
                var diff = Dest.transform.position - winner.transform.position;
                var dist = diff.magnitude;
                
                // winner.transform.localScale = 
                //     Vector3.SmoothDamp(winner.transform.localScale, Dest.transform.localScale, )

                isCloseEnough = dist < .2f;
                yield return null;
            }

            // do the big boom

            winner.Controls.Renderer.DOFade(0, .1f);

            var origin = Dest.Renderer.color;
            Dest.Renderer.DOColor(Color.yellow, .2f).OnComplete(() =>
            {
                Dest.Renderer.DOColor(origin, .2f);
            });
            Dest.transform.DOShakeRotation(.3f, 15);
            Text.Number++;
            StarStateProvider.GetState().Stars++;
            Text.transform.DOPunchScale(Vector3.one * 1.1f, .2f);
            winner.DialSource.PlayOneShot(SoundManifestObject.ShowStarGain);

            yield return Dest.transform.DOPunchScale(Vector3.one * 1.5f, .2f).WaitForCompletion();

        }
    }
    
}
