using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using UnityEngine.EventSystems;
using System;
using Rewired.Integration.UnityUI;
using Rewired.Demos;
using System.Reflection;
using Rewired.Utils.Libraries.TinyJson;
using UnityEngine.UI;

public class PlayerInput : MonoBehaviour
{
    public static Action<Controller> OnControllerChange;

    [Header("Pointer")]
    [SerializeField] private GameObject _pointerPrefab;
    [SerializeField] private bool _hideHardwarePointer = true;
    private RewiredStandaloneInputModule _inputModule;

    private Rewired.Player _player;
    private Camera _camera;
    private PlayerPointer _pointer;

    //[SerializeField]
    private Vector2 _pointerPosition;
    private Vector2 _lastPointerPosition;
    private bool _isPointerMovementDetected;
    private Coroutine _blockCoroutine;

    private static bool IsHPressBlocked;
    private static bool IsVPressBlocked;
    private static bool IsInputBlocked;

    private Controller _lastActiveController;

    private static PlayerInput Instance => GameManager.Input;
    public static Rewired.Player Player
    {
        get
        {
            if (Instance._player == null)
                Instance._player = ReInput.players.GetPlayer(0);
            return Instance._player;
        }
    }

    public static Controller LastActiveController => Instance._lastActiveController;
    public static Vector2 PointerPosition => Instance._pointerPosition;
    public static bool IsPointerMovementDetected => Instance._isPointerMovementDetected;
    public static bool WasPointerLastActiveController => LastActiveController == Controller.Mouse;
    public static bool IsUIInputActive;

    private void Awake()
    {
        _camera = Camera.main;
        _inputModule = GetComponentInChildren<RewiredStandaloneInputModule>();
        InitializePointer();
    }

    private void Update()
    {
        CheckLastActiveController();
    }

    private void CheckLastActiveController()
    {
        switch(Player.controllers.GetLastActiveController()?.type)
        {
            case ControllerType.Mouse:
                if (_lastActiveController != Controller.Mouse)
                    OnControllerChange?.Invoke(Controller.Mouse);
                _lastActiveController = Controller.Mouse;
                break;
            case ControllerType.Joystick:
                if (_lastActiveController != Controller.Joystick)
                    OnControllerChange?.Invoke(Controller.Joystick);
                _lastActiveController = Controller.Joystick;
                break;
            case ControllerType.Keyboard:
                if (_lastActiveController != Controller.Keyboard)
                    OnControllerChange?.Invoke(Controller.Keyboard);
                _lastActiveController = Controller.Keyboard;
                break;
        }
    }

    private void InitializePointer()
    {
        _pointer = Instantiate(_pointerPrefab).GetComponentInChildren<PlayerPointer>();
        OnControllerChange += (Controller controller) =>
        {
            if (controller == Controller.Mouse)
            {
                _pointer.gameObject.SetActive(true);
                _inputModule.allowMouseInput = true;
                EventSystem.current.SetSelectedGameObject(null);
            }
            else
            {
                _pointer.gameObject.SetActive(false);
                _inputModule.allowMouseInput = false;
            }
        };
    }

    public void OnScreenPositionChanged(Vector2 position)
    {
        _pointerPosition = _camera.ScreenToWorldPoint(position);
        _pointer.OnScreenPositionChanged(position);
        CheckPointerMovement();
    }

    private void CheckPointerMovement()
    {
        _inputModule.allowMouseInput = true;
        if (Mathf.Abs(_lastPointerPosition.x - _pointerPosition.x) > 0.2f || Mathf.Abs(_lastPointerPosition.y - _pointerPosition.y) > 0.2f)
        {
            _lastPointerPosition = _pointerPosition;
            _isPointerMovementDetected = true;
        }
        else
            _isPointerMovementDetected = false;
    }

    #region Axises
    public static float Horizontal()
    {
        return Player.GetAxisRaw(RewiredConsts.Action.Horizontal);
    }

    public static float Vertical()
    {
        return Player.GetAxisRaw(RewiredConsts.Action.Vertical);
    }

    public static float PointerHorizontal()
    {
        return Player.GetAxisRaw(RewiredConsts.Action.PointerHorizontal);
    }

    public static float PointerVertical()
    {
        return Player.GetAxisRaw(RewiredConsts.Action.PointerVertical);
    }

    public static float HorizontalS(bool ui = false)
    {
        return Player.GetAxisRaw(RewiredConsts.Action.HorizontalS);
    }

    public static float VerticalS(bool ui = false)
    {
        return Player.GetAxisRaw(RewiredConsts.Action.VerticalS);
    }
    #endregion Axises

    #region Buttons
    public static bool OnConfirm()
    {
        return Player.GetButtonDown(RewiredConsts.Action.Confirm);
    }

    public static bool OnCancel()
    {
        return Player.GetButtonDown(RewiredConsts.Action.Cancel);
	}

    public static bool OnPause()
    {
        return Player.GetButtonDown(RewiredConsts.Action.Pause);
	}

    public static bool OnRight()
    {
        
        return Player.GetButtonDown(RewiredConsts.Action.Right);
    }

    public static bool OnRightPress(float blockTime = 0.1f)
    {
        if (!IsHPressBlocked)
        {
            var temp = Player.GetButton(RewiredConsts.Action.Right);
            if (temp) Instance.StartCoroutine(Instance.BlockH(blockTime));
            return temp;
        }
        return false;
    }

    public static bool OnLeft()
    {
        
        return Player.GetButtonDown(RewiredConsts.Action.Left);
    }

    public static bool OnLeftPress(float blockTime = 0.1f)
    {
        if (!IsHPressBlocked)
        {
            var temp = Player.GetButton(RewiredConsts.Action.Left);
            if (temp) Instance.StartCoroutine(Instance.BlockH(blockTime));
            return temp;
        }
        return false;
    }

    public static bool OnUp()
    {
        return Player.GetButtonDown(RewiredConsts.Action.Up);
    }

    public static bool OnUpPress(float blockTime = 0.1f)
    {
        if (!IsVPressBlocked)
        {
            var temp = Player.GetButton(RewiredConsts.Action.Up);
            if (temp) Instance.StartCoroutine(Instance.BlockV(blockTime));
            return temp;
        }
        return false;
    }

    public static bool OnDown()
    {
        return Player.GetButtonDown(RewiredConsts.Action.Down);
    }

    public static bool OnDownPress(float blockTime = 0.1f)
    {
        if (!IsVPressBlocked)
        {
            var temp = Player.GetButton(RewiredConsts.Action.Down);
            if (temp) Instance.StartCoroutine(Instance.BlockV(blockTime));
            return temp;
        }
        return false;
    }

    public static bool OnButtonX()
    {
        return Player.GetButtonDown(RewiredConsts.Action.Optional);
    }

    public static bool OnButtonY()
    {
        return Player.GetButtonDown(RewiredConsts.Action.OptionalS);
    }

    public static bool OnNext()
    {
        return Player.GetButtonDown(RewiredConsts.Action.TabNext);
    }

    public static bool OnPrevious()
    {
        return Player.GetButtonDown(RewiredConsts.Action.TabPrevious);
    }

    public static bool SecondNext()
    {
        return Player.GetButtonDown(RewiredConsts.Action.TabNextS);
    }

    public static bool SecondPrevious()
    {
        return Player.GetButtonDown(RewiredConsts.Action.TabPreviousS);
    }

    public static bool OnPointerPrimary()
    {
        return Player.GetButtonDown(RewiredConsts.Action.PointerPrimary);
    }

    public static bool OnPointerSecondary()
    {
        return Player.GetButtonDown(RewiredConsts.Action.PointerSecondary);
    }

    public static bool OnPointerThrid()
    {
        return Player.GetButtonDown(RewiredConsts.Action.PointerThird);
    }

    public static bool OnPointerThirdPress()
    {
        return Player.GetButton(RewiredConsts.Action.PointerThird);
    }
    #endregion Buttons

    #region Specials
    public static bool AnyKey()
    {
        return Player.GetAnyButtonDown();
    }

    public static bool OnTouch()
    {
        //TODO:
        return false;
	}
    #endregion Specials

    #region Combinations
    public static bool OnForceRestart()
    {
        return Player.GetButtonLongPress(RewiredConsts.Action.TabNext) && Player.GetButtonLongPress(RewiredConsts.Action.TabPrevious);
    }
    
    public static bool OnForceCombatWin()
    {
        return Player.GetButtonLongPress(RewiredConsts.Action.OptionalS) && Player.GetButtonLongPress(RewiredConsts.Action.RightS);
    }
    #endregion Combinations

    #region Blockers
    private IEnumerator BlockH(float time = 0.1f)
    {
        IsHPressBlocked = true;
        yield return new WaitForSeconds(time);
        IsHPressBlocked = false;
    }

    private IEnumerator BlockV(float time = 0.1f)
    {
        IsVPressBlocked = true;
        yield return new WaitForSeconds(time);
        IsVPressBlocked = false;
    }

    private IEnumerator Block(float time = 0.1f)
    {
        yield return null;
        IsInputBlocked = true;
        if(time >= 0)
        {
            yield return new WaitForSeconds(time);
            IsInputBlocked = false;
        }
    }

    public static void BlockInput(float time)
    {
        Instance._blockCoroutine = Instance.StartCoroutine(Instance.Block(time));
    }

    public static void ReleaseInputBlock()
    {
        if(Instance._blockCoroutine != null)
            Instance.StopCoroutine(Instance._blockCoroutine);
        IsInputBlocked = false;
    }

    public static void LockEventSystem()
    {
        if (EventSystem.current)
            EventSystem.current.enabled = false;
    }

    public static void UnlockEventSystem()
    {
        if (EventSystem.current)
            EventSystem.current.enabled = true;
    }

    public static void EnablePointerInput()
    {
        Instance._inputModule.allowMouseInput = true;
    }

    public static void DisablePointerInput()
    {
        Instance._inputModule.allowMouseInput = false;
    }
    #endregion

    public enum Controller
    {
        Keyboard = 0,
        Mouse = 1,
        Joystick = 2,
        Handheld = 3,
        JoyCon = 4,
        LeftJoyCon = 5,
        RightJoyCon = 6,
    }
}