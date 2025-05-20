using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour{
    private const string PLAYER_BINDINGS = "InputBindings";
    public static GameInput Instance {get; private set;}

    public event EventHandler TakeActionEvent; // Interact1
    public event EventHandler AlternateActionEvent;  // Interact2
    public event EventHandler ActivateFireballEvent;
    public event EventHandler DeactivateFireballEvent;
    public event EventHandler ActivateIceWaveEvent;
    public event EventHandler ActivateWindAttackEvent;

    public event EventHandler PauseGameEvent;

    public enum Keybinds{
        Move_Up,
        Move_Down,
        Move_Left,
        Move_Right,
        Interact,
        AltInteract,
        Pause,
        Ability1,
        Ability2,
        Ability3
    }
    private PlayerInputSystem playerInputSystem; // Unity Input System we use for player actions

    private void Awake(){
        Instance = this;

        playerInputSystem = new PlayerInputSystem();
        if(PlayerPrefs.HasKey(PLAYER_BINDINGS)){
            playerInputSystem.LoadBindingOverridesFromJson(PlayerPrefs.GetString(PLAYER_BINDINGS));
        }
        playerInputSystem.Player.Enable();

        playerInputSystem.Player.Take.performed += Take_performed;
        playerInputSystem.Player.AlternateInteraction.performed += AlternateInteraction_performed;

        playerInputSystem.Player.Fireball.performed += Fireball_performed;
        playerInputSystem.Player.Fireball.canceled += Fireball_released; 

        playerInputSystem.Player.IceWave.performed += IceWave_performed;
        playerInputSystem.Player.WindAttack.performed += WindAttack_performed;
        playerInputSystem.Player.Pause.performed += Pause_performed; 

    }
    private void OnDestroy(){
        playerInputSystem.Player.Take.performed -= Take_performed;
        playerInputSystem.Player.AlternateInteraction.performed -= AlternateInteraction_performed;

        playerInputSystem.Player.Fireball.performed -= Fireball_performed;
        playerInputSystem.Player.Fireball.canceled -= Fireball_released; 


        playerInputSystem.Player.IceWave.performed -= IceWave_performed;
        playerInputSystem.Player.WindAttack.performed += WindAttack_performed;
        playerInputSystem.Player.Pause.performed -= Pause_performed;

        playerInputSystem.Dispose();
    }
    private void Take_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj){
        TakeActionEvent?.Invoke(this, EventArgs.Empty);
    }
    private void AlternateInteraction_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj){
        AlternateActionEvent?.Invoke(this, EventArgs.Empty);
    }
    
    private void Fireball_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj){
        ActivateFireballEvent?.Invoke(this, EventArgs.Empty);
    }
    private void Fireball_released(UnityEngine.InputSystem.InputAction.CallbackContext obj){
        DeactivateFireballEvent?.Invoke(this, EventArgs.Empty);
    }
    
    private void IceWave_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj){
        ActivateIceWaveEvent?.Invoke(this, EventArgs.Empty);
    }
    private void WindAttack_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj){
        ActivateWindAttackEvent?.Invoke(this, EventArgs.Empty);
    }
    private void Pause_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj){
        PauseGameEvent?.Invoke(this, EventArgs.Empty);
    }
    public Vector2 GetPlayerMovementNormalized(){
        Vector2 inputVector = playerInputSystem.Player.Movement.ReadValue<Vector2>();
        inputVector = inputVector.normalized;
        return inputVector;
    }

    public string GetKeybindText(Keybinds keybinds){
        switch(keybinds){
            default:
            case Keybinds.Move_Up:
                return playerInputSystem.Player.Movement.bindings[1].ToDisplayString();
            case Keybinds.Move_Down:
                return playerInputSystem.Player.Movement.bindings[2].ToDisplayString();
            case Keybinds.Move_Left:
                return playerInputSystem.Player.Movement.bindings[3].ToDisplayString();
            case Keybinds.Move_Right:               
                return playerInputSystem.Player.Movement.bindings[4].ToDisplayString();
            case Keybinds.Interact:
                return playerInputSystem.Player.Take.bindings[0].ToDisplayString();
            case Keybinds.AltInteract:
                return playerInputSystem.Player.AlternateInteraction.bindings[0].ToDisplayString();
            case Keybinds.Pause:
                return playerInputSystem.Player.Pause.bindings[0].ToDisplayString();
            case Keybinds.Ability1:
                return playerInputSystem.Player.Fireball.bindings[0].ToDisplayString();
            case Keybinds.Ability2:
                return playerInputSystem.Player.IceWave.bindings[0].ToDisplayString();
            case Keybinds.Ability3:
                return playerInputSystem.Player.IceWave.bindings[0].ToDisplayString();

        }
    }
    public void RebindKeybind(Keybinds keybinds, Action onActionRebound){
        playerInputSystem.Player.Disable();

        InputAction inputAction;
        int bindingIndex;

        switch (keybinds){
            default:
            case Keybinds.Move_Up:
                inputAction = playerInputSystem.Player.Movement;
                bindingIndex = 1;
                break;
            case Keybinds.Move_Down:
                inputAction = playerInputSystem.Player.Movement;
                bindingIndex = 2;
                break;
            case Keybinds.Move_Left:
                inputAction = playerInputSystem.Player.Movement;
                bindingIndex = 3;
                break;
            case Keybinds.Move_Right:
                inputAction = playerInputSystem.Player.Movement;
                bindingIndex = 4;
                break;
            case Keybinds.Interact:
                inputAction = playerInputSystem.Player.Take;
                bindingIndex = 0;
                break;
            case Keybinds.AltInteract:
                inputAction = playerInputSystem.Player.AlternateInteraction;
                bindingIndex = 0;
                break;
            case Keybinds.Pause:
                inputAction = playerInputSystem.Player.Pause;
                bindingIndex = 0;
                break;
            case Keybinds.Ability1:
                inputAction = playerInputSystem.Player.Fireball;
                bindingIndex = 0;
                break;
            case Keybinds.Ability2:
                inputAction = playerInputSystem.Player.IceWave;
                bindingIndex = 0;
                break;
            case Keybinds.Ability3:
                inputAction = playerInputSystem.Player.WindAttack;
                bindingIndex = 0;
                break;
        }

        inputAction.PerformInteractiveRebinding(bindingIndex).OnComplete(callback => {
            callback.Dispose();
            playerInputSystem.Player.Enable();
            onActionRebound();

            PlayerPrefs.SetString(PLAYER_BINDINGS, playerInputSystem.SaveBindingOverridesAsJson());
            PlayerPrefs.Save();
        }).Start();
    }

    public void DisableInput(){
        playerInputSystem.Player.Disable();
    }
    public void EnableInput(){
        playerInputSystem.Player.Enable();
    }
}
