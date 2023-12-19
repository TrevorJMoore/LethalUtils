using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

namespace LethalUtils
{
    internal class LethalGUI : MonoBehaviour
    {
        // Constants
        // Minimum and maximum values in percentage for the gui size
        private const int MINWIDTH = 25;
        private const int MINHEIGHT = 25;
        private const int MAXWIDTH = 100;
        private const int MAXHEIGHT = 100;


        private KeyCode menuKeybind = KeyCode.Home;
        
        // GUI properties
        private float menuWidth = ServerChangeables.menuWidth.Value;
        private float menuHeight = ServerChangeables.menuHeight.Value;
        private int menuPosX = 0;
        private int menuPosY = 0;

        // GUI elements
        Rect menuBackground = new Rect();

        
        void OnGUI()
        {
            
            if (LethalUtils.guiEnabled == true)
            {
                // Create GUI Container
                // Check if the width and height are set correctly
                if ((menuWidth >= MINWIDTH && menuWidth <= MAXWIDTH) && (menuHeight >= MINWIDTH && menuHeight <= MAXWIDTH))
                {
                    // Position the GUI in the center of the screen
                    // We subtract the GUI's size by the screen's size to get the distance
                    // Then divide by 2 to center the GUI between the distance
                    menuPosX = (int)(Screen.width - (Screen.width*menuWidth / 100)) / 2;
                    menuPosY = (int)(Screen.height - (Screen.height*menuHeight / 100)) / 2;
                    menuBackground.Set(menuPosX, menuPosY, Screen.width * (menuWidth/100), Screen.height * (menuHeight/100));
                }
                // If the width or height are set wrong, set to default size
                else
                {
                    menuBackground.Set(menuPosX, menuPosY, (float)(Screen.width), (float)(Screen.height));
                }
                GUI.Box(menuBackground, "Lethal Utilities");

                // Create GUI Elements
                Rect exitButton = new Rect(Screen.width - menuPosX, menuPosY, 50, 50);
                
                if (GUI.Button(exitButton, "X"))
                {
                    LethalUtils.guiEnabled = false;
                    GUI.enabled = false;
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    LethalUtils.guiEnabled = false;
                    LethalUtils.mls.LogInfo("Closed.");
                    
                }
            }
            
            
        }

        // Check each frame if the menu key is pressed
        void Update()
        {
            // If the menu key is pressed and the menu is NOT open
            // Use of UnityInput so I can ignore modifier keys, unlike BepInEx.Configuration.KeyboardShortcut
            // I want players able to access the menu while holding CTRL, SHIFT, and/or ALT.
            if (UnityInput.Current.GetKeyDown(menuKeybind) && LethalUtils.guiEnabled == false)
            {
                // Open the menu
                LethalUtils.guiEnabled = true;
                // Make the cursor visible
                Cursor.visible = true;
                // Lock the cursor to the game window
                Cursor.lockState = CursorLockMode.Confined;
            }
            // Else if the menu key is pressed and the menu is open
            else if (UnityInput.Current.GetKeyDown(menuKeybind) && LethalUtils.guiEnabled == true)
            {
                // Close the menu
                LethalUtils.guiEnabled = false;
                // Make the cursor no longer visible
                Cursor.visible = false;
                // Return the cursor to its previous state
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

    }
}
