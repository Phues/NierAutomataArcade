using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace TinyGiantStudio.ModularToDoLists
{
    public class ColorSchemeEditor : EditorWindow
    {
        static Texture faviconIcon;

        [SerializeField]
        private VisualTreeAsset visualTreeAsset;
        [SerializeField]
        private ColorScheme selectedColorScheme;

        ObjectField colorSchemeField;
        Label taskLabel;
        ColorField completedTaskColorField;
        ColorField inProgressTaskColorField;
        ColorField failedTaskColorField;
        ColorField ignoredTaskColorField;



        private void Awake()
        {
            faviconIcon = EditorGUIUtility.Load("Assets/Plugins/Tiny Giant Studio/Modular To Do List/Artworks/Icon Correct.png") as Texture2D;
        }


        [MenuItem("Tools/Tiny Giant Studio/Modular To-do Lists/Color Scheme Editor", false, 1000)]
        public static void ShowEditor()
        {
            ColorSchemeEditor wnd = GetWindow<ColorSchemeEditor>();
            wnd.titleContent = new GUIContent("Color Scheme Editor", faviconIcon);
            wnd.minSize = new Vector2(300, 310);
        }

        void CreateGUI()
        {
            rootVisualElement.Add(visualTreeAsset.CloneTree());
            colorSchemeField = rootVisualElement.Q<ObjectField>("colorScheme");
            colorSchemeField.value = selectedColorScheme;

            var createNewSchemeButton = rootVisualElement.Q<Button>("createNewSchemeButton");
            createNewSchemeButton.clicked += CreateNewScheme;

            taskLabel = rootVisualElement.Q<Label>("taskLabel");

            completedTaskColorField = rootVisualElement.Q<ColorField>(nameof(selectedColorScheme.completedTask));
            inProgressTaskColorField = rootVisualElement.Q<ColorField>(nameof(selectedColorScheme.inprogressTask));
            failedTaskColorField = rootVisualElement.Q<ColorField>(nameof(selectedColorScheme.failedTask));
            ignoredTaskColorField = rootVisualElement.Q<ColorField>(nameof(selectedColorScheme.ignoredTask));


            colorSchemeField.RegisterValueChangedCallback(ev =>
            {
                UpdateUIBasedOnIfColorSchemeHaveBeenSelected();
            });
            UpdateUIBasedOnIfColorSchemeHaveBeenSelected();
        }

        void CreateNewScheme()
        {
            ColorScheme asset = ScriptableObject.CreateInstance<ColorScheme>();
            string name = AssetDatabase.GenerateUniqueAssetPath("Assets/Plugins/Tiny Giant Studio/Modular To Do List/Color Scheme.asset");
            AssetDatabase.CreateAsset(asset, name);
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;

            selectedColorScheme = asset;
            colorSchemeField.value = asset;

            UpdateUIBasedOnIfColorSchemeHaveBeenSelected();
        }


        void UpdateUIBasedOnIfColorSchemeHaveBeenSelected()
        {
            if (colorSchemeField.value)
                ColorSchemeSelectedToBeEdited();
            else
                ColorSchemeHaventBeenSelected();
        }

        void ColorSchemeHaventBeenSelected()
        {
            taskLabel.style.display = DisplayStyle.None;

            completedTaskColorField.Unbind();
            completedTaskColorField.style.display = DisplayStyle.None;

            inProgressTaskColorField.Unbind();
            inProgressTaskColorField.style.display = DisplayStyle.None;

            failedTaskColorField.Unbind();
            failedTaskColorField.style.display = DisplayStyle.None;

            ignoredTaskColorField.Unbind();
            ignoredTaskColorField.style.display = DisplayStyle.None;
        }

        void ColorSchemeSelectedToBeEdited()
        {
            taskLabel.style.display = DisplayStyle.Flex;

            selectedColorScheme = (ColorScheme)colorSchemeField.value;
            var colorSchemeSerialized = new SerializedObject(selectedColorScheme);

            var completedTaskProperty = colorSchemeSerialized.FindProperty(nameof(selectedColorScheme.completedTask));
            completedTaskColorField.BindProperty(completedTaskProperty);
            completedTaskColorField.style.display = DisplayStyle.Flex;


            var inProgressTaskProperty = colorSchemeSerialized.FindProperty(nameof(selectedColorScheme.inprogressTask));
            inProgressTaskColorField.BindProperty(inProgressTaskProperty);
            inProgressTaskColorField.style.display = DisplayStyle.Flex;

            var failedTaskProperty = colorSchemeSerialized.FindProperty(nameof(selectedColorScheme.failedTask));
            failedTaskColorField.BindProperty(failedTaskProperty);
            failedTaskColorField.style.display = DisplayStyle.Flex;

            var ignoredTaskProperty = colorSchemeSerialized.FindProperty(nameof(selectedColorScheme.ignoredTask));
            ignoredTaskColorField.BindProperty(ignoredTaskProperty);
            ignoredTaskColorField.style.display = DisplayStyle.Flex;
        }
    }
}