using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    [SerializeField] private UIButtonsPanel _parentPanel;
    [SerializeField] protected TextMeshProUGUI _textField;
    [Header("General")]
    [SerializeField] private bool _isDefaultButton;
    [Header("Events")]
    [SerializeField] private UnityEvent _onConfirm;
    [SerializeField] private UnityEvent _onNextValue;
    [SerializeField] private UnityEvent _onPrevValue;

    private int _index = -1;

    protected virtual void Awake()
    {
        PlayerInput.OnControllerChange += OnControllerChanedHandler;
    }

    protected virtual void Start()
    {
        if (_parentPanel.IsActive && PlayerInput.IsUIInputActive && _isDefaultButton && _parentPanel.CurrentSelectedButton == null)
            Select();
    }

    /// <summary>
    /// Subscribes index tu button, defining it's position in parent panel hierarchy.
    /// Only called by parent panel to establish hierarchy.
    /// </summary>
    /// <param name="index"></param>
    public void SubscribeIndex(int index)
    {
        _index = index;
    }

    private void OnControllerChanedHandler(PlayerInput.Controller controller)
    {
        if (!_parentPanel.IsActive || !PlayerInput.IsUIInputActive) return;
        if (controller == PlayerInput.Controller.Mouse)
            Deselect();
        else if (_isDefaultButton && _parentPanel.CurrentSelectedButton == null)
            Select();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_parentPanel.IsActive || !PlayerInput.IsUIInputActive) return;
        if (_parentPanel.CurrentSelectedButton == this)
            SetStatePressed();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_parentPanel.IsActive || !PlayerInput.IsUIInputActive) return;
        if (_parentPanel.CurrentSelectedButton == this)
        {
            SetStateSelected();
            _onConfirm?.Invoke();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_parentPanel.IsActive || !PlayerInput.IsUIInputActive) return;
        Select();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_parentPanel.IsActive || !PlayerInput.IsUIInputActive) return;
        Deselect();
    }

    public void Select()
    {
        _parentPanel.DeselectAll();
        SetStateSelected();
        _parentPanel.OnButtonSelection(this, _index);
    }

    public void Deselect()
    {
        SetStateNormal();
        _parentPanel.OnButtonDeselection(this, _index);
    }

    protected virtual void SetStateNormal()
    {

    }

    protected virtual void SetStateSelected()
    {

    }

    protected virtual void SetStatePressed()
    {

    }

    public void Confirm()
    {
        _onConfirm?.Invoke();
    }

    public void SelectNextValue()
    {
        _onNextValue?.Invoke();
    }

    public void SelectPreviousValue()
    {
        _onPrevValue?.Invoke();
    }

    private enum ButtonSelectionState
    {
        Normal,
        Selected,
        Pressed,
    }

    public void DebugTest()
    {
        Debug.Log("Button: " + _index);
    }
}