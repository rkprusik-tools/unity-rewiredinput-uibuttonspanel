using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// UI Panel for multiplatform selectable buttons.
/// Remember to enable IsUIInputActive in PlayerInput and to activate this panel or no input will be read.
/// </summary>
public class UIButtonsPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private List<UIButton> _buttons;
    [Header("General")]
    [SerializeField] private bool _isLooping;//will button selection start from begining once we reach the end
    [SerializeField] private bool _isVerticalButtonPanel;//main panel layout - vertical or horizontal
    [SerializeField] private bool _isMultiValueButtonPanel;//options panel - where buttons have multiple values to chose from
    [Header("Events")]
    [SerializeField] private UnityEvent _cancelEvent;

    private bool _isActive;

    [SerializeField] private UIButton _currentSelectedButton;
    [SerializeField] private int _currentIndex;

    public UIButton CurrentSelectedButton => _currentSelectedButton;

    public bool IsActive => _isActive;

    protected virtual void Awake()
    {
        for (int i = 0; i < _buttons.Count; i++)
            _buttons[i].SubscribeIndex(i);
    }

    protected virtual void Update()
    {
        if (!_isActive || !PlayerInput.IsUIInputActive) return;
        CheckVerticalNavigation();
        CheckHorizontalNavigation();
        CheckConfirm();
        CheckCancel();
    }

    protected virtual void CheckConfirm()
    {
        if (PlayerInput.OnConfirm())
            _currentSelectedButton?.Confirm();
    }

    protected virtual void CheckCancel()
    {
        if(PlayerInput.OnCancel())
            _cancelEvent?.Invoke();
    }

    protected virtual void CheckHorizontalNavigation()
    {
        if(PlayerInput.OnLeftPress(0.2f))
        {
            if (!_isVerticalButtonPanel)
                SelectPrevious();
            else if (_isMultiValueButtonPanel)
                SelectPreviousValue();
        }
        else if(PlayerInput.OnRightPress(0.2f))
        {
            if (!_isVerticalButtonPanel)
                SelectNext();
            else
                SelectNextValue();
        }
    }

    protected virtual void CheckVerticalNavigation()
    {
        if (PlayerInput.OnUpPress(0.2f))
        {
            if (_isVerticalButtonPanel)
                SelectPrevious();
            else if (_isMultiValueButtonPanel)
                SelectPreviousValue();
        }
        else if (PlayerInput.OnDownPress(0.2f))
        {
            if (_isVerticalButtonPanel)
                SelectNext();
            else
                SelectNextValue();
        }
    }

    protected virtual void SelectPreviousValue()
    {
        _currentSelectedButton?.SelectPreviousValue();
    }

    protected virtual void SelectNextValue()
    {
        _currentSelectedButton?.SelectNextValue();
    }

    /// <summary>
    /// Remember that UIButton.Select will notify this class about being selected.
    /// There is no need to handle current button change here.
    /// </summary>
    protected virtual void SelectNext()
    {
        if(_currentIndex >= _buttons.Count - 1)
        {
            if (_isLooping)
                _currentIndex = 0;
            else
                return;
        }
        else
        {
            _currentIndex++;
        }
        _buttons[_currentIndex].Select();
    }

    /// <summary>
    /// Remember that UIButton.Select will notify this class about being selected.
    /// There is no need to handle current button change here.
    /// </summary>
    protected virtual void SelectPrevious()
    {
        if (_currentIndex <= 0)
        {
            if (_isLooping)
                _currentIndex = _buttons.Count - 1;
            else
                return;
        }
        else
        {
            _currentIndex--;
        }
        _buttons[_currentIndex].Select();
    }

    public virtual void Activate()
    {
        _isActive = true;
    }

    public virtual void Deactivate()
    {
        _isActive = false;
    }

    public void DeselectAll()
    {
        foreach (var b in _buttons)
            b.Deselect();
    }

    public virtual void OnButtonSelection(UIButton button, int index)
    {
        _currentSelectedButton = button;
        _currentIndex = index;
    }

    public virtual void OnButtonDeselection(UIButton button, int index)
    {
        _currentSelectedButton = null;
        _currentIndex = -1;
    }

    [ContextMenu("ActivatePanel")]
    public void DActivatePanel()
    {
        Activate();
        PlayerInput.IsUIInputActive = true;
    }
}
