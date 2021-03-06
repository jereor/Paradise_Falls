// GENERATED AUTOMATICALLY FROM 'Assets/Packages/UnityInputSystem/InputActions.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @InputActions : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @InputActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""InputActions"",
    ""maps"": [
        {
            ""name"": ""Player"",
            ""id"": ""28c5c67e-9625-449a-8712-07a82113f8b2"",
            ""actions"": [
                {
                    ""name"": ""Move"",
                    ""type"": ""PassThrough"",
                    ""id"": ""0bd01cc2-f21b-45b7-a892-990efeda9a9e"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Jump"",
                    ""type"": ""Button"",
                    ""id"": ""a8c30c08-4c52-4749-82c8-b943ab156f08"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Interact"",
                    ""type"": ""Button"",
                    ""id"": ""0d9c82d3-6aa6-4c98-8370-10938d6b9c0c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Shield"",
                    ""type"": ""Button"",
                    ""id"": ""26d35881-2fb2-444c-8cce-ec135f39e1e1"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Melee"",
                    ""type"": ""Button"",
                    ""id"": ""dcd7c718-e069-44d7-8830-f1481723994d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""ThrowAim"",
                    ""type"": ""Button"",
                    ""id"": ""948e6069-45ff-4c78-b6f5-d1c5b423cdc4"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""HeavyMelee"",
                    ""type"": ""Button"",
                    ""id"": ""1bf54e5f-a07a-4906-8ed7-95e291e94468"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""ShockwaveAttack"",
                    ""type"": ""Button"",
                    ""id"": ""b838cc22-8c8f-49c6-b10d-194504f72a64"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Dash"",
                    ""type"": ""Button"",
                    ""id"": ""83bd5f33-bc2d-4d93-bce7-256c464c19ae"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""AdvanceText"",
                    ""type"": ""Button"",
                    ""id"": ""de2c38ef-f5a0-4216-a7cf-c5c1787e7d18"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""TargetPosition"",
                    ""type"": ""PassThrough"",
                    ""id"": ""2fac39e7-5ef2-4917-befe-06f5441c1b8c"",
                    ""expectedControlType"": ""Stick"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""WASD"",
                    ""id"": ""fef811f2-fb98-4394-a75d-1aa10bc03ea9"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""6bc24320-a7d9-49c7-bde3-0a63bd42d8cb"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""up"",
                    ""id"": ""7104be52-49b9-4518-a387-59787d02e97b"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""ad6df111-7f4c-4c65-8cd0-39c03667ebb9"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""ea11ce27-3220-4bb4-ac96-c707ebc59d69"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""205c1943-05e3-48e4-ad4f-7a002e2c387a"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""148c5b14-bbba-4e56-8590-ab7fcf2bb5f1"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""8f7fcae9-6e74-481e-9893-f600636588c5"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""e2354d6c-d05e-4881-949a-bf79cf731b36"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""a887d257-62dc-4a14-ab9a-ae3b5f78c9de"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""5474105a-729c-4026-a7ba-8383f0573886"",
                    ""path"": ""<Joystick>/stick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""288afea5-a169-49e1-8629-99163a59f3f9"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""46c5e624-e78f-4385-b931-305d13e3eead"",
                    ""path"": ""<Keyboard>/f"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Interact"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b16e0e1c-c4b8-4000-8caf-df3762d7e80e"",
                    ""path"": ""<Gamepad>/buttonNorth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Interact"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""53d98ce8-3f1a-4074-8638-1e616915ca4c"",
                    ""path"": ""<Keyboard>/ctrl"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Shield"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0ed290a1-55a7-4a49-9813-8f3ef17a4f3a"",
                    ""path"": ""<Gamepad>/leftTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Shield"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8c8f460f-be9c-46b3-8228-8dbcf8d208ea"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Melee"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""bcd447bd-89c4-49d7-b63e-40150f84853e"",
                    ""path"": ""<Gamepad>/rightShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Melee"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""867f330b-b1c1-4ef5-9dbc-6f41956f057d"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ThrowAim"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""9d793c12-3c49-499c-a918-3dbdcfc1f282"",
                    ""path"": ""<Gamepad>/leftShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ThrowAim"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3d7d8884-30e0-4358-9275-1649e2e5f5b1"",
                    ""path"": ""<Keyboard>/leftAlt"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""HeavyMelee"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c88d6764-57a3-483e-bdc7-22f49828ed92"",
                    ""path"": ""<Gamepad>/buttonWest"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""HeavyMelee"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""cbc4b4ff-7e2e-412e-9eaf-512e943381b3"",
                    ""path"": ""<Keyboard>/q"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ShockwaveAttack"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""683e2ddc-4781-43ee-ac0f-2adfb875d285"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ShockwaveAttack"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""285856cc-bd0d-4a84-b011-9ecc357e9254"",
                    ""path"": ""<Keyboard>/leftShift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Dash"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b43ad399-185f-4d78-a1ee-904a9eee6766"",
                    ""path"": ""<Gamepad>/rightTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Dash"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d313c016-4e0c-4161-9d2b-be0b667db074"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""AdvanceText"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e22b2e81-34b6-47b9-8a68-4a4019df4b87"",
                    ""path"": ""<Gamepad>/rightShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""AdvanceText"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ada6ed52-5afa-4220-8e0d-5fb2c4d77e61"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""55ce7538-e9de-4a81-b050-600557ad1acf"",
                    ""path"": ""<Gamepad>/rightStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""TargetPosition"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""GameUI"",
            ""id"": ""b7df2b10-b9ea-439f-ae20-8904a15f26c0"",
            ""actions"": [
                {
                    ""name"": ""Pause"",
                    ""type"": ""Button"",
                    ""id"": ""be8516e8-0f09-4a32-a0ec-3579cac9dc6f"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Map"",
                    ""type"": ""Button"",
                    ""id"": ""8f0f6181-6114-4dcd-a121-0ccd035b4762"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Navigation"",
                    ""type"": ""PassThrough"",
                    ""id"": ""a7c0ba3c-2428-413f-8800-6de7baddeb0f"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Zoom"",
                    ""type"": ""PassThrough"",
                    ""id"": ""c383cbc1-98d7-4580-9ed4-5af80bd342ba"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""MapNavigation"",
                    ""type"": ""PassThrough"",
                    ""id"": ""00d15c47-64f9-4726-a50d-932cab1c05df"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Inventory"",
                    ""type"": ""Button"",
                    ""id"": ""df4f2461-098e-4030-9049-d9791f89dee9"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""6a51360f-1e3a-4010-a437-f8de733e55fb"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pause"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""9a83413e-6010-471a-b1c3-9ab745e18a1a"",
                    ""path"": ""<Gamepad>/start"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pause"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""9aaca787-e5d7-4506-819f-6b9d6125a963"",
                    ""path"": ""<Keyboard>/m"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Map"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8d64633b-d3a0-4d0c-b4e7-8377cb0fa677"",
                    ""path"": ""<Gamepad>/select"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Map"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""715fa20b-7a63-451d-9e1b-ab6011b2cc4d"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Navigation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""6701f247-125c-4ee6-92f2-91497091351a"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Navigation"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""d6b9dd23-568f-4fd6-96ea-7c84f491f56b"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Navigation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""707611a4-8760-42a0-9927-bf7f965c0007"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Navigation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""8fca91b7-9ba8-4ae0-b858-1f552c1c515c"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Navigation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""03968c15-6198-4f02-bd96-bf40183cde94"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Navigation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""88ee59d0-459f-4ab5-b722-d32cdd73c3ba"",
                    ""path"": ""<Mouse>/scroll"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Zoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8ebe86d6-22d5-4460-88cd-f5691c6b08c1"",
                    ""path"": ""<Gamepad>/dpad"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Zoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""68d07df6-9b32-4647-9b06-159c40be422b"",
                    ""path"": ""<Gamepad>/rightStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MapNavigation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""19106b99-f0ed-4f82-a444-4406e71c071b"",
                    ""path"": ""<Keyboard>/tab"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Inventory"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1f323e97-b04d-4531-91ac-2b0568a26618"",
                    ""path"": ""<Gamepad>/dpad/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Inventory"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Player
        m_Player = asset.FindActionMap("Player", throwIfNotFound: true);
        m_Player_Move = m_Player.FindAction("Move", throwIfNotFound: true);
        m_Player_Jump = m_Player.FindAction("Jump", throwIfNotFound: true);
        m_Player_Interact = m_Player.FindAction("Interact", throwIfNotFound: true);
        m_Player_Shield = m_Player.FindAction("Shield", throwIfNotFound: true);
        m_Player_Melee = m_Player.FindAction("Melee", throwIfNotFound: true);
        m_Player_ThrowAim = m_Player.FindAction("ThrowAim", throwIfNotFound: true);
        m_Player_HeavyMelee = m_Player.FindAction("HeavyMelee", throwIfNotFound: true);
        m_Player_ShockwaveAttack = m_Player.FindAction("ShockwaveAttack", throwIfNotFound: true);
        m_Player_Dash = m_Player.FindAction("Dash", throwIfNotFound: true);
        m_Player_AdvanceText = m_Player.FindAction("AdvanceText", throwIfNotFound: true);
        m_Player_TargetPosition = m_Player.FindAction("TargetPosition", throwIfNotFound: true);
        // GameUI
        m_GameUI = asset.FindActionMap("GameUI", throwIfNotFound: true);
        m_GameUI_Pause = m_GameUI.FindAction("Pause", throwIfNotFound: true);
        m_GameUI_Map = m_GameUI.FindAction("Map", throwIfNotFound: true);
        m_GameUI_Navigation = m_GameUI.FindAction("Navigation", throwIfNotFound: true);
        m_GameUI_Zoom = m_GameUI.FindAction("Zoom", throwIfNotFound: true);
        m_GameUI_MapNavigation = m_GameUI.FindAction("MapNavigation", throwIfNotFound: true);
        m_GameUI_Inventory = m_GameUI.FindAction("Inventory", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // Player
    private readonly InputActionMap m_Player;
    private IPlayerActions m_PlayerActionsCallbackInterface;
    private readonly InputAction m_Player_Move;
    private readonly InputAction m_Player_Jump;
    private readonly InputAction m_Player_Interact;
    private readonly InputAction m_Player_Shield;
    private readonly InputAction m_Player_Melee;
    private readonly InputAction m_Player_ThrowAim;
    private readonly InputAction m_Player_HeavyMelee;
    private readonly InputAction m_Player_ShockwaveAttack;
    private readonly InputAction m_Player_Dash;
    private readonly InputAction m_Player_AdvanceText;
    private readonly InputAction m_Player_TargetPosition;
    public struct PlayerActions
    {
        private @InputActions m_Wrapper;
        public PlayerActions(@InputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Move => m_Wrapper.m_Player_Move;
        public InputAction @Jump => m_Wrapper.m_Player_Jump;
        public InputAction @Interact => m_Wrapper.m_Player_Interact;
        public InputAction @Shield => m_Wrapper.m_Player_Shield;
        public InputAction @Melee => m_Wrapper.m_Player_Melee;
        public InputAction @ThrowAim => m_Wrapper.m_Player_ThrowAim;
        public InputAction @HeavyMelee => m_Wrapper.m_Player_HeavyMelee;
        public InputAction @ShockwaveAttack => m_Wrapper.m_Player_ShockwaveAttack;
        public InputAction @Dash => m_Wrapper.m_Player_Dash;
        public InputAction @AdvanceText => m_Wrapper.m_Player_AdvanceText;
        public InputAction @TargetPosition => m_Wrapper.m_Player_TargetPosition;
        public InputActionMap Get() { return m_Wrapper.m_Player; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayerActions set) { return set.Get(); }
        public void SetCallbacks(IPlayerActions instance)
        {
            if (m_Wrapper.m_PlayerActionsCallbackInterface != null)
            {
                @Move.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
                @Move.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
                @Move.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
                @Jump.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJump;
                @Jump.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJump;
                @Jump.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJump;
                @Interact.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnInteract;
                @Interact.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnInteract;
                @Interact.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnInteract;
                @Shield.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnShield;
                @Shield.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnShield;
                @Shield.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnShield;
                @Melee.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMelee;
                @Melee.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMelee;
                @Melee.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMelee;
                @ThrowAim.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnThrowAim;
                @ThrowAim.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnThrowAim;
                @ThrowAim.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnThrowAim;
                @HeavyMelee.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnHeavyMelee;
                @HeavyMelee.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnHeavyMelee;
                @HeavyMelee.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnHeavyMelee;
                @ShockwaveAttack.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnShockwaveAttack;
                @ShockwaveAttack.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnShockwaveAttack;
                @ShockwaveAttack.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnShockwaveAttack;
                @Dash.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnDash;
                @Dash.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnDash;
                @Dash.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnDash;
                @AdvanceText.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAdvanceText;
                @AdvanceText.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAdvanceText;
                @AdvanceText.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAdvanceText;
                @TargetPosition.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnTargetPosition;
                @TargetPosition.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnTargetPosition;
                @TargetPosition.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnTargetPosition;
            }
            m_Wrapper.m_PlayerActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
                @Jump.started += instance.OnJump;
                @Jump.performed += instance.OnJump;
                @Jump.canceled += instance.OnJump;
                @Interact.started += instance.OnInteract;
                @Interact.performed += instance.OnInteract;
                @Interact.canceled += instance.OnInteract;
                @Shield.started += instance.OnShield;
                @Shield.performed += instance.OnShield;
                @Shield.canceled += instance.OnShield;
                @Melee.started += instance.OnMelee;
                @Melee.performed += instance.OnMelee;
                @Melee.canceled += instance.OnMelee;
                @ThrowAim.started += instance.OnThrowAim;
                @ThrowAim.performed += instance.OnThrowAim;
                @ThrowAim.canceled += instance.OnThrowAim;
                @HeavyMelee.started += instance.OnHeavyMelee;
                @HeavyMelee.performed += instance.OnHeavyMelee;
                @HeavyMelee.canceled += instance.OnHeavyMelee;
                @ShockwaveAttack.started += instance.OnShockwaveAttack;
                @ShockwaveAttack.performed += instance.OnShockwaveAttack;
                @ShockwaveAttack.canceled += instance.OnShockwaveAttack;
                @Dash.started += instance.OnDash;
                @Dash.performed += instance.OnDash;
                @Dash.canceled += instance.OnDash;
                @AdvanceText.started += instance.OnAdvanceText;
                @AdvanceText.performed += instance.OnAdvanceText;
                @AdvanceText.canceled += instance.OnAdvanceText;
                @TargetPosition.started += instance.OnTargetPosition;
                @TargetPosition.performed += instance.OnTargetPosition;
                @TargetPosition.canceled += instance.OnTargetPosition;
            }
        }
    }
    public PlayerActions @Player => new PlayerActions(this);

    // GameUI
    private readonly InputActionMap m_GameUI;
    private IGameUIActions m_GameUIActionsCallbackInterface;
    private readonly InputAction m_GameUI_Pause;
    private readonly InputAction m_GameUI_Map;
    private readonly InputAction m_GameUI_Navigation;
    private readonly InputAction m_GameUI_Zoom;
    private readonly InputAction m_GameUI_MapNavigation;
    private readonly InputAction m_GameUI_Inventory;
    public struct GameUIActions
    {
        private @InputActions m_Wrapper;
        public GameUIActions(@InputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Pause => m_Wrapper.m_GameUI_Pause;
        public InputAction @Map => m_Wrapper.m_GameUI_Map;
        public InputAction @Navigation => m_Wrapper.m_GameUI_Navigation;
        public InputAction @Zoom => m_Wrapper.m_GameUI_Zoom;
        public InputAction @MapNavigation => m_Wrapper.m_GameUI_MapNavigation;
        public InputAction @Inventory => m_Wrapper.m_GameUI_Inventory;
        public InputActionMap Get() { return m_Wrapper.m_GameUI; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(GameUIActions set) { return set.Get(); }
        public void SetCallbacks(IGameUIActions instance)
        {
            if (m_Wrapper.m_GameUIActionsCallbackInterface != null)
            {
                @Pause.started -= m_Wrapper.m_GameUIActionsCallbackInterface.OnPause;
                @Pause.performed -= m_Wrapper.m_GameUIActionsCallbackInterface.OnPause;
                @Pause.canceled -= m_Wrapper.m_GameUIActionsCallbackInterface.OnPause;
                @Map.started -= m_Wrapper.m_GameUIActionsCallbackInterface.OnMap;
                @Map.performed -= m_Wrapper.m_GameUIActionsCallbackInterface.OnMap;
                @Map.canceled -= m_Wrapper.m_GameUIActionsCallbackInterface.OnMap;
                @Navigation.started -= m_Wrapper.m_GameUIActionsCallbackInterface.OnNavigation;
                @Navigation.performed -= m_Wrapper.m_GameUIActionsCallbackInterface.OnNavigation;
                @Navigation.canceled -= m_Wrapper.m_GameUIActionsCallbackInterface.OnNavigation;
                @Zoom.started -= m_Wrapper.m_GameUIActionsCallbackInterface.OnZoom;
                @Zoom.performed -= m_Wrapper.m_GameUIActionsCallbackInterface.OnZoom;
                @Zoom.canceled -= m_Wrapper.m_GameUIActionsCallbackInterface.OnZoom;
                @MapNavigation.started -= m_Wrapper.m_GameUIActionsCallbackInterface.OnMapNavigation;
                @MapNavigation.performed -= m_Wrapper.m_GameUIActionsCallbackInterface.OnMapNavigation;
                @MapNavigation.canceled -= m_Wrapper.m_GameUIActionsCallbackInterface.OnMapNavigation;
                @Inventory.started -= m_Wrapper.m_GameUIActionsCallbackInterface.OnInventory;
                @Inventory.performed -= m_Wrapper.m_GameUIActionsCallbackInterface.OnInventory;
                @Inventory.canceled -= m_Wrapper.m_GameUIActionsCallbackInterface.OnInventory;
            }
            m_Wrapper.m_GameUIActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Pause.started += instance.OnPause;
                @Pause.performed += instance.OnPause;
                @Pause.canceled += instance.OnPause;
                @Map.started += instance.OnMap;
                @Map.performed += instance.OnMap;
                @Map.canceled += instance.OnMap;
                @Navigation.started += instance.OnNavigation;
                @Navigation.performed += instance.OnNavigation;
                @Navigation.canceled += instance.OnNavigation;
                @Zoom.started += instance.OnZoom;
                @Zoom.performed += instance.OnZoom;
                @Zoom.canceled += instance.OnZoom;
                @MapNavigation.started += instance.OnMapNavigation;
                @MapNavigation.performed += instance.OnMapNavigation;
                @MapNavigation.canceled += instance.OnMapNavigation;
                @Inventory.started += instance.OnInventory;
                @Inventory.performed += instance.OnInventory;
                @Inventory.canceled += instance.OnInventory;
            }
        }
    }
    public GameUIActions @GameUI => new GameUIActions(this);
    public interface IPlayerActions
    {
        void OnMove(InputAction.CallbackContext context);
        void OnJump(InputAction.CallbackContext context);
        void OnInteract(InputAction.CallbackContext context);
        void OnShield(InputAction.CallbackContext context);
        void OnMelee(InputAction.CallbackContext context);
        void OnThrowAim(InputAction.CallbackContext context);
        void OnHeavyMelee(InputAction.CallbackContext context);
        void OnShockwaveAttack(InputAction.CallbackContext context);
        void OnDash(InputAction.CallbackContext context);
        void OnAdvanceText(InputAction.CallbackContext context);
        void OnTargetPosition(InputAction.CallbackContext context);
    }
    public interface IGameUIActions
    {
        void OnPause(InputAction.CallbackContext context);
        void OnMap(InputAction.CallbackContext context);
        void OnNavigation(InputAction.CallbackContext context);
        void OnZoom(InputAction.CallbackContext context);
        void OnMapNavigation(InputAction.CallbackContext context);
        void OnInventory(InputAction.CallbackContext context);
    }
}
