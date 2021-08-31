using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

namespace ParkingLotSnapping
{
    internal static class ModKeyBindingFunctions // Helper Class for everyhing related to keybinds
    {
        private static readonly string kKeyBindingTemplate = "KeyBindingTemplate"; // Collosal's Keybinding UI template consisting of label + button
        private static bool currentlyBinding = false; // Defines if we are currently in a "Editing Binding" mode

        // Function that adds keybinding template to UI menu. Label and default (initial) binding as parameter
        internal static UIButton AddKeyMappingUI(UIHelper uIHelper, string name, SavedInputKey initialBinding)
        {
            UIComponent componentToUse = (UIComponent)uIHelper.self; //Gets UI component from UIhelper
            UIPanel keyBindingPanel = componentToUse.AttachUIComponent(UITemplateManager.GetAsGameObject(kKeyBindingTemplate)) as UIPanel; // Ad keybinding panel (label+button) to UI component
            UILabel uILabel = keyBindingPanel.Find<UILabel>("Name"); // "Label" element from panel
            UIButton uIButton = keyBindingPanel.Find<UIButton>("Binding"); // "Button" element from label

            uILabel.text = name; // Write name of binding in label
            uIButton.text = initialBinding.ToLocalizedString("KEYNAME"); // Write name of key in button
            uIButton.objectUserData = initialBinding; // Save binding in button user data (to access later to modify)
            uIButton.buttonsMask = UIMouseButton.Left; // Button will only respond to left click (when user tries to change binding)

            uIButton.eventMouseDown += OnBindingMouseDown; // What to do when button is pressed
            uIButton.eventKeyDown += OnBindingKeyDown; // What to do when key is pressed

            return uIButton; // For editing text later
        }

        //Function that does something when mouse is pressed on button (or when mouse is pressed again during button focused)
        private static void OnBindingMouseDown(UIComponent component, UIMouseEventParameter p) // event parameter p has all the info we need
        {
            p.Use();
            UIMouseButton pressedMouseButton = p.buttons; // Get mouse button that was pressed
            UIButton uIButtonToModify = (UIButton)p.source; // Get UI keybind button that was chosen by user
            if (currentlyBinding == false) // If i'm not in "new binding" mode, we need to go into that mode (user requested keybind change)
            {
                uIButtonToModify.buttonsMask = UIMouseButton.Left | UIMouseButton.Right | UIMouseButton.Middle | UIMouseButton.Special0 | UIMouseButton.Special1 | UIMouseButton.Special2 | UIMouseButton.Special3; // User may choose some of these mouse keys for the keybind
                uIButtonToModify.text = "Press any key"; // Change button text
                currentlyBinding = true; // We enter "currently binding" mode
                uIButtonToModify.Focus(); // Button will be on focus until keybinding is done
                UIView.PushModal(uIButtonToModify); // Start modal mode in button. Means the rest of the program is frozen until keybinding is done. Button will be the sole focus of our aplication for now
            }
            else if(IsBindableMouseButton(pressedMouseButton)) //If we are in "new binding" mode, we check first if it's a bindable button (no left or right click)
            {
                UIView.PopModal(); // End current modal mode
                InputKey newBinding = SavedInputKey.Encode(MouseButtonToKeycode(pressedMouseButton), IsControlDown(), IsShiftDown(), IsAltDown()); // Transform mouse button to "key" (modifier+key)
                SavedInputKey bindingToEdit = (SavedInputKey)uIButtonToModify.objectUserData; // Get current (old) binding
                bindingToEdit.value = newBinding; //Replace previous binding to new one
                uIButtonToModify.text = bindingToEdit.ToLocalizedString("KEYNAME"); // Update button text
                uIButtonToModify.buttonsMask = UIMouseButton.Left; // UI Button will again only respond to left click
                currentlyBinding = false; // Exit "new binding" mode
            }
        }

        //Function that does something when key is pressed is pressed on button
        private static void OnBindingKeyDown(UIComponent component, UIKeyEventParameter p)
        {
            p.Use();
            KeyCode pressedKey = p.keycode; // Get key that was pressed
            bool validKeyPressed = IsValidKeyPressed(pressedKey); // Invalid keys pressed are modifier keys. For example, we can't bind to a single Ctrl; otherwise would be impossible to bind Ctrl+Z
            UIButton uIButtonToModify = (UIButton)p.source; // Get UI keybind button that was chosen by user
            if (currentlyBinding && validKeyPressed) // If it's a valid key and we are in binding mode...
            {
                UIView.PopModal();
                InputKey newBinding;
                SavedInputKey bindingToEdit = (SavedInputKey)uIButtonToModify.objectUserData; // Get current (old) binding
                if (pressedKey == KeyCode.Escape) newBinding = bindingToEdit.value; // If escape is pressed, means old binding will be preserved
                else if (pressedKey == KeyCode.Delete) newBinding = SavedInputKey.Empty; // If delete or backspace pressed, binding will be deleted (none)
                else if (pressedKey == KeyCode.Backspace) newBinding = SavedInputKey.Empty;
                else newBinding = SavedInputKey.Encode(pressedKey, p.control, p.shift, p.alt); // Otherwise is a valid character, I encode it along with modifier key info

                bindingToEdit.value = newBinding; // Update binding with new key
                uIButtonToModify.text = bindingToEdit.ToLocalizedString("KEYNAME"); // Update button text
                uIButtonToModify.buttonsMask = UIMouseButton.Left; // UI Button will again only respond to left click
                currentlyBinding = false; // Exit "new binding" mode
            }
        }

        //Check if button code is bindable (left and right click are of course not bindable to anything)
        private static bool IsBindableMouseButton(UIMouseButton button)
        {
            switch (button)
            {
                case UIMouseButton.Left: return false;
                case UIMouseButton.Right: return false;
                default: return true;
            }
        }
        
        //Function to transform a button press into a key (for binding)
        private static KeyCode MouseButtonToKeycode(UIMouseButton button)
        {
            switch (button)
            {
                case UIMouseButton.Left: return KeyCode.Mouse0;
                case UIMouseButton.Right: return KeyCode.Mouse1;
                case UIMouseButton.Middle: return KeyCode.Mouse2;
                case UIMouseButton.Special0: return KeyCode.Mouse3;
                case UIMouseButton.Special1: return KeyCode.Mouse4;
                case UIMouseButton.Special2: return KeyCode.Mouse5;
                case UIMouseButton.Special3: return KeyCode.Mouse6;
                default: return KeyCode.None;
            }
        }

        //Functions to check if modifier keys are currently being pressed
        private static bool IsControlDown()
        {
            if (Input.GetKey(KeyCode.LeftControl)) return true;
            else if (Input.GetKey(KeyCode.RightControl)) return true;
            else return false;
        }
        private static bool IsAltDown()
        {
            if (Input.GetKey(KeyCode.LeftAlt)) return true;
            else if (Input.GetKey(KeyCode.RightAlt)) return true;
            else return false;
        }
        private static bool IsShiftDown()
        {
            if (Input.GetKey(KeyCode.LeftShift)) return true;
            else if (Input.GetKey(KeyCode.RightShift)) return true;
            else return false;
        }
    
        //Function to check if a pressed key is a valid binding (a single modifier key, like ctrl is not valid as a binding)
        private static bool IsValidKeyPressed(KeyCode key)
        {
            switch (key) // Modifier keys are not valid bindable keys (needs to be another key + modifier)
            {
                case KeyCode.LeftControl: return false;
                case KeyCode.RightControl: return false;
                case KeyCode.LeftShift: return false;
                case KeyCode.RightShift: return false;
                case KeyCode.LeftAlt: return false;
                case KeyCode.RightAlt: return false;
                default: return true;
            }
        }
    }
}