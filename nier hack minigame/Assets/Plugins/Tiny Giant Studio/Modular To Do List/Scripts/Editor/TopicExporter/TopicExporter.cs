using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace TinyGiantStudio.ModularToDoLists
{
    public class TopicExporter : EditorWindow
    {
        [SerializeField] VisualTreeAsset visualTreeAsset;
        
        static Texture faviconIcon;


        public static Topic topic;
        
        ObjectField topicField;
        Label topicLabel;

        Button exportButton;

        Toggle exporterVersion;
        Toggle topicName;
        Toggle topicDescription;
        Toggle tags;
        Toggle colors;
        Toggle topicIcon;

        Toggle includeLists;
        IntegerField emptyColumnBeforeLists;
        Toggle listName;
        Toggle listDescription;
        Toggle listCreationTime;

        Toggle includeTasks;
        IntegerField emptyColumnBeforeTasks;
        Toggle taskName;
        Toggle taskDescription;
        Toggle taskStatus;
        Toggle taskTimes;
        Toggle taskReferences;





        void Awake()
        {
            faviconIcon = EditorGUIUtility.Load("Assets/Plugins/Tiny Giant Studio/Modular To Do List/Artworks/Icon Correct.png") as Texture2D;
        }




        [MenuItem("Tools/Tiny Giant Studio/Modular To-do Lists/Export Topic", false, 10000)]
        public static void ShowEditor()
        {
            TopicExporter window = GetWindow<TopicExporter>("Export Topic");
            window.titleContent = new GUIContent("TopicExporter", faviconIcon);
            window.minSize = new Vector2(400, 800);
        }

        public void CreateGUI()
        {
            visualTreeAsset.CloneTree(rootVisualElement);

            topicLabel = rootVisualElement.Q<Label>("topicName");

            exportButton = rootVisualElement.Q<Button>("ExportButton");
            exportButton.clicked += ExportButtonClicked;

            topicField = rootVisualElement.Q<ObjectField>("SelectedTopic");
            topicField.value = topic;
            UpdateLabel();
            topicField.RegisterValueChangedCallback(ev =>
            {
                topic = ev.newValue as Topic;
                UpdateLabel();
            });


            exporterVersion = rootVisualElement.Q<Toggle>("exporterVersionFilter");
            topicName = rootVisualElement.Q<Toggle>("topicName");
            topicDescription = rootVisualElement.Q<Toggle>("topicDescription");
            tags = rootVisualElement.Q<Toggle>("tags");
            colors = rootVisualElement.Q<Toggle>("colors");
            topicIcon = rootVisualElement.Q<Toggle>("topicIcon");


            includeLists = rootVisualElement.Q<Toggle>("todoLists");
            if (includeLists.value) rootVisualElement.Q<GroupBox>("toDoListsFoldoutGroupBox").style.opacity = 1;
            else rootVisualElement.Q<GroupBox>("toDoListsFoldoutGroupBox").style.opacity = 0.7f;
            includeLists.RegisterValueChangedCallback(ev =>
            {
                if (ev.newValue) rootVisualElement.Q<GroupBox>("toDoListsFoldoutGroupBox").style.opacity = 1;
                else rootVisualElement.Q<GroupBox>("toDoListsFoldoutGroupBox").style.opacity = 0.7f;
            });

            listName = rootVisualElement.Q<Toggle>("listName");
            emptyColumnBeforeLists = rootVisualElement.Q<IntegerField>("emptyColumnBeforeLists");
            listDescription = rootVisualElement.Q<Toggle>("listDescription");
            listCreationTime = rootVisualElement.Q<Toggle>("listCreationTime");


            includeTasks = rootVisualElement.Q<Toggle>("tasks");
            if (includeTasks.value) rootVisualElement.Q<GroupBox>("tasksfiltersFoldoutGroupBox").style.opacity = 1;
            else rootVisualElement.Q<GroupBox>("tasksfiltersFoldoutGroupBox").style.opacity = 0.7f;
            includeTasks.RegisterValueChangedCallback(ev =>
            {
                if (ev.newValue) rootVisualElement.Q<GroupBox>("tasksfiltersFoldoutGroupBox").style.opacity = 1;
                else rootVisualElement.Q<GroupBox>("tasksfiltersFoldoutGroupBox").style.opacity = 0.7f;
            });

            emptyColumnBeforeTasks = rootVisualElement.Q<IntegerField>("emptyColumnBeforeTasks");
            taskName = rootVisualElement.Q<Toggle>("taskName");
            taskDescription = rootVisualElement.Q<Toggle>("taskDescription");
            taskStatus = rootVisualElement.Q<Toggle>("taskStatus");
            taskTimes = rootVisualElement.Q<Toggle>("taskTimes");
            taskReferences = rootVisualElement.Q<Toggle>("taskReferences");
        }



        void UpdateLabel()
        {
            if (topic)
            {
                topicLabel.text = topic.myName;
                exportButton.style.opacity = 1;
            }
            else
            {
                topicLabel.text = "";
                exportButton.style.opacity = 0.7f;
            }
        }

        void ExportButtonClicked()
        {
            if (topic == null)
            {
                Debug.Log("Please select a topic to export");
                return;
            }
            if (TopicUtility.WriteTopicToCsv
                (
                topic,
                exporterVersion.value,
                topicName.value,
                topicDescription.value,
                tags.value,
                colors.value,
                topicIcon.value,

                includeLists.value,
                emptyColumnBeforeLists.value,
                listName.value,
                listDescription.value,
                listCreationTime.value,

                includeTasks.value,
                emptyColumnBeforeTasks.value,
                taskName.value,
                taskDescription.value,
                taskStatus.value,
                taskTimes.value,
                taskReferences.value
                )
                )
            {
                Close();
            }
        }
    }
}