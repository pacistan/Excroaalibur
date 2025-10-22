using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GPainterWindow :  EditorWindow
{
     private GPrefabBrushLibrary brushLibrary;
    
    // Currently selected brush
    private int selectedBrushIndex = -1;
    private GBrushData currentBrush;
    
    // Grid display settings
    private int columns = 5;
    private float iconSize = 64f;
    private float spacing = 8f;
    private Vector2 scrollPosition;
    
    // Painting settings
    private bool isPainting = false;
    private float brushSize = 1f;
    private LayerMask paintLayerMask;
    
    // UI State
    private bool showBrushSettings = true;
    private bool showPaintSettings = true;
    
    [MenuItem("Window/Prefab Painter")]
    public static void ShowWindow()
    {
        GPainterWindow window = GetWindow<GPainterWindow>("Prefab Painter");
        window.minSize = new Vector2(400, 500);
        window.Show();
    }

    private void OnEnable()
    {
        // Subscribe to scene view events for painting
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        // Unsubscribe from scene view events
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(5);
        
          DrawLibrarySection();
        
        if (brushLibrary != null && brushLibrary.brushes.Count > 0)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            DrawBrushGrid();
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            DrawBrushSettings();
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            DrawPaintSettings();
        }
        else
        {
            EditorGUILayout.Space(20);
            EditorGUILayout.HelpBox("Please assign a Prefab Brush Library to start painting!", MessageType.Info);
        }
    }

    private void DrawLibrarySection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Prefab Brush Library", EditorStyles.boldLabel);
        
        GPrefabBrushLibrary newLibrary = (GPrefabBrushLibrary)EditorGUILayout.ObjectField(
            "Library Asset", 
            brushLibrary, 
            typeof(GPrefabBrushLibrary), 
            false
        );
        
        if (newLibrary != brushLibrary)
        {
            brushLibrary = newLibrary;
            selectedBrushIndex = -1;
            currentBrush = null;
        }
        
        if (brushLibrary != null)
        {
            EditorGUILayout.LabelField($"Available Brushes: {brushLibrary.brushes.Count}");
        }
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button("Create New Library"))
        {
            CreateNewLibrary();
        }
        
        EditorGUILayout.EndVertical();
    }

    private void DrawBrushGrid()
    {
        if (brushLibrary == null || brushLibrary.brushes.Count == 0)
            return;
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Brush Selection", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        columns = EditorGUILayout.IntSlider("Columns", columns, 3, 10);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MinHeight(200));
        
        int rows = Mathf.CeilToInt((float)brushLibrary.brushes.Count / columns);
        float totalWidth = columns * iconSize + (columns - 1) * spacing;
        float totalHeight = rows * iconSize + (rows - 1) * spacing;
        
        Rect gridRect = GUILayoutUtility.GetRect(totalWidth, totalHeight);
        
        for (int i = 0; i < brushLibrary.brushes.Count; i++)
        {
            int row = i / columns;
            int col = i % columns;
            
            float xPos = gridRect.x + col * (iconSize + spacing);
            float yPos = gridRect.y + row * (iconSize + spacing);
            
            Rect cellRect = new Rect(xPos, yPos, iconSize, iconSize);
            
            DrawBrushCell(cellRect, i, brushLibrary.brushes[i]);
        }
        
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawBrushCell(Rect rect, int index, GBrushData brush)
    {
        bool isSelected = (index == selectedBrushIndex);
        
        // Draw background
        Color oldColor = GUI.backgroundColor;
        GUI.backgroundColor = isSelected ? new Color(0.3f, 0.6f, 1f) : new Color(0.3f, 0.3f, 0.3f);
        GUI.Box(rect, "");
        GUI.backgroundColor = oldColor;
        
        // Draw border for selected brush
        if (isSelected)
        {
            Rect borderRect = new Rect(rect.x - 2, rect.y - 2, rect.width + 4, rect.height + 4);
            EditorGUI.DrawRect(borderRect, new Color(0.3f, 0.6f, 1f));
            EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f));
        }
        
        // Draw icon or preview
        Rect iconRect = new Rect(rect.x + 4, rect.y + 4, rect.width - 8, rect.height - 28);
        
        if (brush.icon != null)
        {
            // Use custom icon if provided
            GUI.DrawTexture(iconRect, brush.icon, ScaleMode.ScaleToFit);
        }
        else if (brush.prefab != null)
        {
            // Try to get prefab preview first (3D preview)
            Texture2D preview = AssetPreview.GetAssetPreview(brush.prefab);
            if (preview != null)
            {
                GUI.DrawTexture(iconRect, preview, ScaleMode.ScaleToFit);
            }
            else
            {
                // If no preview available yet, get the prefab's mini thumbnail (project icon)
                Texture2D prefabIcon = AssetPreview.GetMiniThumbnail(brush.prefab);
                if (prefabIcon != null)
                {
                    GUI.DrawTexture(iconRect, prefabIcon, ScaleMode.ScaleToFit);
                }
                else
                {
                    // Fallback to generic prefab icon
                    GUIContent iconContent = EditorGUIUtility.IconContent("Prefab Icon");
                    if (iconContent != null && iconContent.image != null)
                    {
                        GUI.DrawTexture(iconRect, iconContent.image, ScaleMode.ScaleToFit);
                    }
                    else
                    {
                        // Ultimate fallback - just show the GameObject icon
                        GUI.Label(iconRect, EditorGUIUtility.IconContent("GameObject Icon"), 
                            new GUIStyle() { alignment = TextAnchor.MiddleCenter });
                    }
                }
            }
        }
        else
        {
            GUI.Label(iconRect, "No Prefab", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });
        }
        
        // Draw label
        Rect labelRect = new Rect(rect.x + 2, rect.y + rect.height - 22, rect.width - 4, 20);
        GUI.Label(labelRect, brush.brushName, new GUIStyle(GUI.skin.label) 
        { 
            alignment = TextAnchor.MiddleCenter, 
            fontSize = 9,
            normal = new GUIStyleState() { textColor = Color.white }
        });
        
        // Handle click
        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
        {
            selectedBrushIndex = index;
            currentBrush = brush;
            Event.current.Use();
            Repaint();
        }
    }

    private void DrawBrushSettings()
    {
        if (currentBrush == null)
        {
            EditorGUILayout.HelpBox("Select a brush to see its settings", MessageType.Info);
            return;
        }
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        showBrushSettings = EditorGUILayout.Foldout(showBrushSettings, "Current Brush Settings", true, EditorStyles.foldoutHeader);
        
        if (showBrushSettings)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.LabelField("Brush Name:", currentBrush.brushName, EditorStyles.boldLabel);
            EditorGUILayout.ObjectField("Prefab:", currentBrush.prefab, typeof(GameObject), false);
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.LabelField("Placement Options:", EditorStyles.miniBoldLabel);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.Toggle("Random Rotation", currentBrush.randomRotation);
            EditorGUILayout.FloatField("Rotation Offset", currentBrush.rotationOffset);
            EditorGUI.EndDisabledGroup();
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.EndVertical();
    }

    private void DrawPaintSettings()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        showPaintSettings = EditorGUILayout.Foldout(showPaintSettings, "Paint Settings", true, EditorStyles.foldoutHeader);
        
        if (showPaintSettings)
        {
            EditorGUI.indentLevel++;
            
            isPainting = EditorGUILayout.Toggle("Paint Mode Active", isPainting);
            
            if (isPainting)
            {
                EditorGUILayout.HelpBox("Paint Mode is ACTIVE. Click in the Scene view to place prefabs!", MessageType.Warning);
            }
            
            EditorGUILayout.Space(5);
            
            brushSize = EditorGUILayout.Slider("Brush Size", brushSize, 0.1f, 10f);
            EditorGUILayout.Space(5);
            

            EditorGUILayout.Space(5);
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.EndVertical();
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!isPainting || currentBrush == null)
            return;
        
        Event e = Event.current;
        paintLayerMask = LayerMask.GetMask("Cell");
        // Draw brush preview
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, paintLayerMask))
        {
            // Draw brush indicator
            Handles.color = new Color(0.3f, 0.6f, 1f, 0.3f);
            Handles.DrawSolidDisc(hit.point, hit.normal, brushSize);
            Handles.color = new Color(0.3f, 0.6f, 1f, 1f);
            Handles.DrawWireDisc(hit.point, hit.normal, brushSize);
            
            // Paint on left click
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                PaintPrefab(hit);
                e.Use();
            }
            
            sceneView.Repaint();
        }
    }

    private void PaintPrefab(RaycastHit hit)
    {
        if (currentBrush == null)
            return;

        // Calculate rotation
        float rotation = 0;
        
        if (currentBrush.randomRotation)
        {
            rotation = currentBrush.isIncrementalRotation ? Random.Range(0, 6) * 60f : Random.Range(0f, 360f);
        }
        else
        {
            rotation += currentBrush.rotationOffset;
        }
        
        var hits = Physics.OverlapSphere(hit.point, brushSize, paintLayerMask);
        
        foreach (var hitCell in hits)
        {
            var position = hitCell.transform.position;
            var coordinate = GHexCoordinate.FromPosition(position);
            GGridManager gridManager = GameObject.FindFirstObjectByType<GGridManager>();
            GCell cell = gridManager.GetCell(coordinate);
            if (cell == null)
            {
                Debug.LogError($"No Cell Found at {coordinate} with position {position}");
                continue;
            }
            if (currentBrush.prefab != null)
            {
                // Instantiate prefab

                GCellVisualPresetData presetData = new GCellVisualPresetData(currentBrush.prefab, rotation);
                
                GameObject instance = cell.cellVisualsController.OnCreateVisualPreset(presetData);
                // Register undo
                Undo.RegisterCreatedObjectUndo(instance, "Paint Prefab");
                Selection.activeGameObject = instance;
            }
            else
            {
                cell.cellVisualsController.ClearVisualsPresets();
            }
        }
    }

    private void CreateNewLibrary()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Create New Prefab Brush Library",
            "NewPrefabBrushLibrary",
            "asset",
            "Create a new Prefab Brush Library asset"
        );
        
        if (!string.IsNullOrEmpty(path))
        {
            GPrefabBrushLibrary newLibrary = CreateInstance<GPrefabBrushLibrary>();
            AssetDatabase.CreateAsset(newLibrary, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            brushLibrary = newLibrary;
            EditorGUIUtility.PingObject(newLibrary);
        }
    }
}