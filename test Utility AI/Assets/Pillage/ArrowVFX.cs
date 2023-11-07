using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class ArrowVFX : MonoBehaviour
{

    public List<Transform> arrows;
    public Transform destination;
    public Transform sourcePoistion;
    public List<float> durations;

    //private void Start()
    //{
    //    FireArrow();
    //}

    public Sequence FireArrow(UnityAction callback)
    {
        gameObject.SetActive(true);
        ResetArrow();

        Sequence tweenSeq = DOTween.Sequence();

        int index = 0;
        
        
        foreach (var item in arrows)
        {
            tweenSeq.Join(item.DOMove(destination.position, 1f + durations[Random.Range(0, durations.Count)])
                .SetEase(Ease.Linear).OnComplete(() => item.gameObject.SetActive(false)));
            index++;
        }


        return tweenSeq.Play().OnComplete(() => 
        { 
            callback?.Invoke();
            gameObject.SetActive(false);
        }); 
    }

    public void ResetArrow()
    {
        foreach (var item in arrows)
        {
            item.gameObject.SetActive(true);
            item.transform.position = new Vector3(item.transform.position.x, item.transform.position.y, sourcePoistion.position.z);
        }
    }
}
