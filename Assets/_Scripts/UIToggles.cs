using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIToggles : MonoBehaviour {

    [SerializeField] private GameObject _canvas;
    [SerializeField] private BlazePoseSample _blaze;

    public void OnButtonToggleCameraHandler () {
        _canvas.SetActive(!_canvas.activeSelf);
    }

    public void OnButtonToggleOutlineHandler() {
        _blaze.DrawStickFigure = !_blaze.DrawStickFigure;
    }
}
