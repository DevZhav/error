using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using TMPro;
using Rewired;

namespace Settings
{
    public class InputSettings : MonoBehaviour
    {
        public InputSettingsSave Save;

        [Header("Listening Panel")]
        public GameObject ListeningPanel;
        public TextMeshProUGUI ListeningText;
        public Button ListeningButton;

        [Header("Inputs")]
        public GameObject InputButtonTemplate;
        public Transform KeyboardControlContent;
        public Transform MouseControlContent;

        private InputMapper InputMapper = new InputMapper();
        private List<Template.InputButtonTemplate> Buttons = new List<Template.InputButtonTemplate>();

        private Rewired.Player Player
        {
            get
            {
                return ReInput.players.GetPlayer(0);
            }
        }
        private ControllerMap ControllerMapKeyboard
        {
            get
            {
                if (Player.controllers.maps.GetMap(0) == null)
                    return Player.controllers.maps.GetMap(2);
                else
                    return Player.controllers.maps.GetMap(0);
            }
        }
        private ControllerMap ControllerMapMouse
        {
            get
            {
                if (Player.controllers.maps.GetMap(0) == null)
                    return Player.controllers.maps.GetMap(5);
                else
                    return Player.controllers.maps.GetMap(1);
            }
        }

        private void RedrawUI(ControllerMap map)
        {
            // Update each button label with the currently mapped element identifier
            for (int i = 0; i < Buttons.Count; i++)
            {
                Template.InputButtonTemplate template = Buttons[i];
                InputAction action = Buttons[i].InputAction;

                string name = string.Empty;
                int actionElementMapId = -1;

                // Find the first ActionElementMap that maps to this action and is compatible with this field type
                foreach (var actionElementMap in map.ElementMapsWithAction(action.id))
                {
                    Debug.Log(actionElementMap.actionDescriptiveName + " | " + actionElementMap.controllerMap.controllerType.ToString());
                    if (actionElementMap.controllerMap.controllerType == ControllerType.Keyboard)
                        template.IsKeyboard = true;

                    if (actionElementMap.ShowInField(template.AxisRange))
                    {
                        name = actionElementMap.elementIdentifierName;
                        actionElementMapId = actionElementMap.id;
                        break;
                    }
                }

                if (map.controllerType == ControllerType.Keyboard && template.IsKeyboard)
                {
                    // Set the label for the button
                    template.RemapButtonText.SetText(name);

                    // Set the field button callback
                    template.RemapButton.onClick.RemoveAllListeners(); // clear all events
                    int index = i; // copy the variable for closer
                    template.RemapButton.onClick.AddListener(() => OnInputFieldClicked(index, actionElementMapId, map));
                }
                else if (map.controllerType == ControllerType.Mouse && !template.IsKeyboard)
                {
                    template.transform.SetParent(MouseControlContent);

                    // Set the label for the button
                    template.RemapButtonText.SetText(name);

                    // Set the field button callback
                    template.RemapButton.onClick.RemoveAllListeners(); // clear all events
                    int index = i; // copy the variable for closer
                    template.RemapButton.onClick.AddListener(() => OnInputFieldClicked(index, actionElementMapId, map));
                }
            }
        }

        private void ClearUI()
        {
            // Clear button labels
            for (int i = 0; i < Buttons.Count; i++)
            {
                Buttons[i].RemapButtonText.SetText(string.Empty);
            }
        }

        private void InitializeUI()
        {
            // Clear any content
            foreach (Transform t in KeyboardControlContent)
            {
                Destroy(t.gameObject);
            }
            // Clear any content
            foreach (Transform t in MouseControlContent)
            {
                Destroy(t.gameObject);
            }
            Buttons.Clear();

            // Create action fields and input field buttons
            foreach (var action in ReInput.mapping.UserAssignableActions)
            {
                // If this is the mouse cateogry, ignore
                if (action.categoryId == 1)
                    continue;

                // If it's an axis, let's create a button for positive and negative
                if (action.type == InputActionType.Axis)
                {
                    CreateButton(action, AxisRange.Positive, action.positiveDescriptiveName);
                    CreateButton(action, AxisRange.Negative, action.negativeDescriptiveName);
                }
                // If it's just a normal input
                else if (action.type == InputActionType.Button)
                {
                    CreateButton(action, AxisRange.Positive, action.positiveDescriptiveName);
                }
            }

            RedrawUI(ControllerMapKeyboard);
            RedrawUI(ControllerMapMouse);
        }

        private void CreateButton(InputAction action, AxisRange axisRange, string label)
        {
            GameObject obj = Instantiate(InputButtonTemplate, KeyboardControlContent);
            obj.GetComponent<Settings.Template.InputButtonTemplate>().SetupInputButton(action, axisRange, label);

            Buttons.Add(obj.GetComponent<Settings.Template.InputButtonTemplate>());
        }

        private void OnInputFieldClicked(int index, int actionElementMapToReplaceId, ControllerMap map)
        {
            if (index < 0 || index >= Buttons.Count) return; // index out of range

            // Begin listening for input
            InputMapper.Start(
                new InputMapper.Context()
                {
                    actionId = Buttons[index].InputAction.id,
                    controllerMap = map,
                    actionRange = Buttons[index].AxisRange,
                    actionElementMapToReplace = map.GetElementMap(actionElementMapToReplaceId)
                });

            // SHOW A UI TO TELL WE'RE LISTENING
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                InputMapper.Stop();
            }

            if (InputMapper.status == InputMapper.Status.Listening)
            {
                ListeningPanel.SetActive(true);
                ListeningText.SetText(string.Format("Press a key you want to map to this action.\nMapping will automatically cancel after {0} seconds.\n\nNote: When setting to the Space Key, you have to wait until it's automatically cancelled.", Mathf.Round(InputMapper.timeRemaining)));
            }
            else
            {
                ListeningPanel.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (!ReInput.isReady) return; // Don't run if Rewired hasn't been initialized

            // Timeout after 5 seconds of listening
            InputMapper.options.timeout = 5f;

            // Ignore Mouse X and Y axes
            InputMapper.options.ignoreMouseXAxis = true;
            InputMapper.options.ignoreMouseYAxis = true;
            InputMapper.options.checkForConflicts = false;
            InputMapper.options.allowKeyboardKeysWithModifiers = false;
            InputMapper.options.allowKeyboardModifierKeyAsPrimary = true;

            // Subscribe to events
            InputMapper.InputMappedEvent += OnInputMapped;
            InputMapper.StoppedEvent += OnStopped;

            // Setup the listening panel button
            ListeningButton.onClick.AddListener(()=> {
                InputMapper.Stop();
            });

            // Create UI elements
            InitializeUI();
        }

        private void OnDisable()
        {
            // Make sure the input mapper is stopped first
            InputMapper.Stop();

            // Unsubscribe from events
            InputMapper.RemoveAllEventListeners();
        }

        private void OnInputMapped(InputMapper.InputMappedEventData data)
        {
            RedrawUI(ControllerMapKeyboard);
            RedrawUI(ControllerMapMouse);
            Debug.Log("Input Remapped");
        }

        private void OnStopped(InputMapper.StoppedEventData data)
        {
            // DISAPPEAR THE UI THAT TELLS WE'RE LISTENING
        }

        public void Setup(InputSettingsSave save)
        {
            Save = save;
        }

        public void Apply()
        {
            ReInput.userDataStore.Save();
        }

        public void LoadOptions()
        {
            //ReInput.userDataStore.Load();
            //Debug.Log("Loaded input data...");
        }

        public void SaveOptions()
        {
            ReInput.userDataStore.Save();

            Debug.Log("Saved input data...");
        }
    }
}