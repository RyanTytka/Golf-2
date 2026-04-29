using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine;

public class ImageButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //Attach this to an object to make it enlarge when the mouse hovers over it
    private Tween scaleTween;
    public Vector3 scaleVector;

    public void OnPointerEnter(PointerEventData eventData)
    {
        //scale this obj up on hover
        scaleTween?.Kill();
        scaleTween = transform.DOScale(scaleVector, 0.1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //stop scaling this up
        scaleTween?.Kill();
        scaleTween = transform.DOScale(Vector3.one, 0.1f);
    }
}
