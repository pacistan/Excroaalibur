using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Reflection;
using UnityEditor.UIElements;

[InitializeOnLoad]
public class ConsoleToolbarExtension
{
    private static bool isAdded = false;

    static ConsoleToolbarExtension()
    {
        EditorApplication.update += OnUpdate;
    }

    private static void OnUpdate()
    {
        if (isAdded) return;

        var consoleWindowType = Type.GetType("UnityEditor.ConsoleWindow,UnityEditor");
        if (consoleWindowType == null) return;

        var consoleWindows = Resources.FindObjectsOfTypeAll(consoleWindowType);
        
        foreach (var consoleWindow in consoleWindows)
        {
            var editorWindow = consoleWindow as EditorWindow;
            if (editorWindow == null) continue;

            var rootVisualElement = editorWindow.rootVisualElement;
            if (rootVisualElement == null) continue;

            // Try to find toolbar - it might be nested
            var toolbar = FindToolbar(rootVisualElement);
            if (toolbar == null) continue;

            // Check if already added
            if (toolbar.Q("CustomConsoleDropdown") != null) continue;

            // Create dropdown
            var dropdownButton = new ToolbarMenu 
            { 
                text = "My Menu", 
                name = "CustomConsoleDropdown",
                style = { marginLeft = 5 }
            };
            
            dropdownButton.menu.AppendAction("Option 1", (a) => Debug.Log("Option 1"));
            dropdownButton.menu.AppendAction("Option 2", (a) => Debug.Log("Option 2"));
            dropdownButton.menu.AppendSeparator();
            dropdownButton.menu.AppendAction("Test Action", (a) => Debug.Log("Test!"));

            toolbar.Add(dropdownButton);
            Debug.Log("Console dropdown added successfully!");
            isAdded = true;
        }
    }

    private static Toolbar FindToolbar(VisualElement root)
    {
        // Search recursively for toolbar
        var toolbar = root.Q<Toolbar>();
        if (toolbar != null) return toolbar;

        foreach (var child in root.Children())
        {
            toolbar = FindToolbar(child);
            if (toolbar != null) return toolbar;
        }

        return null;
    }
}