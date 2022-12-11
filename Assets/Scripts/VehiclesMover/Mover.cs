using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class Mover : MonoBehaviour
{
    [SerializeField] private Transform _parent;

    private void Start()
    {
        Vector3[] _wayPoints = new Vector3[_parent.childCount];

        for (int i = 0; i < _parent.childCount; i++)
        {
            _wayPoints[i] = _parent.GetChild(i).transform.position;
        }

        Tween tween = transform.DOPath(_wayPoints, 20, PathType.CatmullRom).SetOptions(true).SetLookAt(0.01f);

        tween.SetLoops(-1);
    }
}
