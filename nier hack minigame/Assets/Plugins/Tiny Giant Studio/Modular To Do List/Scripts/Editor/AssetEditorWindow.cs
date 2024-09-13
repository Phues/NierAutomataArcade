using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditorInternal;
using UnityEngine;


namespace TinyGiantStudio.ModularToDoLists
{
    public class AssetEditorWindow : EditorWindow
    {
        #region variables

        #region Mostly Readonly Stuff
        readonly string versionNumber = "1.4.3";

        readonly string proAssetLink = "https://u3d.as/38d4?aid=1011ljxWe";
        string currentAssetLink = "https://u3d.as/37Uh?aid=1011ljxWe"; //this isn't read only because if the current version is the pro version, this link will be changed to proasset link. Uses the proEditor variable check
        readonly string getMoreAssetsLink = "https://assetstore.unity.com/publishers/45848?aid=1011ljxWe";

        Type proEditor;
        #endregion Mostly Readonly Stuff


        Topic[] topics;
        List<Topic> starredTopics = new List<Topic>();
        List<Topic> unStarredTopics = new List<Topic>();
        bool requiresRefreshingTopicList = false;


        Page currentPage;

        Topic selectedTopic;
        ToDoList selectedTodoList;
        Task selectedTask;


        Vector2 scrollPos;
        float screenWidth;


        #region Spacing variables
        int indentLeft = 15;
        int indentRight = 15;

        int indentForTaskSubOptions = 30;
        int topicIconSize = 55;
        float fontSize = 1;
        #endregion Spacing


        #region Editor settings
        string editorPrefsPrefix = "ModularToDoLists";
        float _pageTransitionSpeed;
        float PageTransitionSpeed
        {
            get { return _pageTransitionSpeed; }
            set
            {
                if (_pageTransitionSpeed != value)
                {
                    _pageTransitionSpeed = value;

                    page_topicListAnimBool.speed = value;
                    page_cardListAnimBool.speed = value;
                    page_settingsAnimBool.speed = value;
                    page_taskAnimBool.speed = value;
                }
            }
        }
        bool showPercentageInProgressBar = false;
        int taskConsideredDueSoonIfLessThanDaysLeft;
        int newTaskIsDueInCurrentTimePlusHours;
        int maximumReferenceInOneLine = 1;
        bool clickingOnTaskEditsIt = false; //otherwise, it opens page with details
        bool taskAndToDoDeleteConfirmation = false;
        #endregion Editor settings


        SerializedObject selectedTopicSO;
        List<ReorderableList> reorderableListsOfTask = new List<ReorderableList>();
        List<SerializedProperty> serializedistsOfTask = new List<SerializedProperty>();
        bool requiresReSerialization = false;
        #endregion variables



        const string key = "T";



        #region Unity Stuff
        void Awake()
        {
            proEditor = Type.GetType("TinyGiantStudio.ModularToDoLists.ModularToDoListProEditor");
            PrepEverything(); //todo: seems unnecessary. Since on enable calls it
        }

        void OnEnable()
        {
            PrepEverything();
        }


        [MenuItem("Tools/Tiny Giant Studio/Modular To-do Lists/Modular To-do Lists %#" + key, false, 1)]
        public static void ModularToDoListShowWindow()
        {
            EditorWindow editorWindow = GetWindow<AssetEditorWindow>("Modular To Do List");
            editorWindow.titleContent = new GUIContent(" Modular To Do List", correctIcon, "");
            editorWindow.minSize = new Vector2(400, 400);
        }

        public void OnGUI()
        {
            requiresReSerialization = false;

            if (currentPage == Page.todoList)
                if (selectedTopicSO != null)
                    selectedTopicSO.Update();

            GenerateStyle();
            EditorGUI.BeginChangeCheck();


            if (topics.Length == 0)
            {
                NoTopicPage();
            }
            else if (currentPage == Page.topicList)
            {
                if (CardListFadedOut() && SettingsListFadedOut())
                {
                    if (!page_topicListAnimBool.target)
                        page_topicListAnimBool.target = true;

                    if (page_topicListAnimBool.faded > 0)
                    {
                        Color c = GUI.color;
                        c.a = page_topicListAnimBool.faded;
                        GUI.color = c;
                        Page_ListOfTopics();
                    }
                }
            }
            else if (currentPage == Page.todoList)
            {
                if (TopicListFadedOut())
                {
                    if (!page_cardListAnimBool.target)
                        page_cardListAnimBool.target = true;

                    if (page_cardListAnimBool.faded > 0)
                    {

                        Color c = GUI.color;
                        c.a = page_cardListAnimBool.faded;
                        GUI.color = c;
                        if (selectedTopic)
                            Page_SelectedTopic();
                    }
                }
            }
            else if (currentPage == Page.settings)
            {
                if (TopicListFadedOut())
                {
                    if (!page_settingsAnimBool.target)
                        page_settingsAnimBool.target = true;

                    if (page_settingsAnimBool.faded > 0)
                    {

                        Color c = GUI.color;
                        c.a = page_settingsAnimBool.faded;
                        GUI.color = c;
                        Page_Settings();
                    }
                }
            }
            else if (currentPage == Page.task)
            {
                if (CardListFadedOut())
                {
                    if (!page_taskAnimBool.target)
                        page_taskAnimBool.target = true;

                    if (page_taskAnimBool.faded > 0)
                    {

                        Color c = GUI.color;
                        c.a = page_taskAnimBool.faded;
                        GUI.color = c;
                        Page_Task();
                    }
                }
            }

            if (EditorGUI.EndChangeCheck())
            {

            }

            if (requiresRefreshingTopicList)
            {
                RefreshBoardsList();
                requiresRefreshingTopicList = false;
                Repaint();
            }

            if (requiresReSerialization)
                UpdateSerializedContent();


            if (currentPage == Page.todoList && selectedTopic == null)
            {
                currentPage = Page.topicList;
            }

            if (currentPage == Page.todoList)
            {
                if (selectedTopicSO != null)
                    selectedTopicSO.ApplyModifiedProperties();
                else
                    UpdateSerializedContent();
            }
        }

        #endregion Unity stuff

        void NoTopicPage()
        {
            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.Space(indentLeft);
            {
                GUILayout.BeginVertical();
                {
                    EditorGUILayout.HelpBox("No topics found. Please create one.", MessageType.Info);

                    GUILayout.Space(10);

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    CreateNewTopicButton();
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            GUILayout.Space(indentRight);
            GUILayout.EndHorizontal();

        }

        /// <summary>
        /// The first page shown when opening the window.
        /// </summary>
        void Page_ListOfTopics()
        {
            Color originalBackgroundColor = GUI.backgroundColor;
            screenWidth = Screen.width;

            int maxItemInOneLine = 1;
            if (screenWidth > 1200)
                maxItemInOneLine = 2;
            if (screenWidth > 1800)
                maxItemInOneLine = 3;
            if (screenWidth > 2500)
                maxItemInOneLine = 4;

            TopMenu_ListOfTopics();

            GUI.backgroundColor = originalBackgroundColor;
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);

            if (starredTopics.Count > 0)
                StarredBoardsList(originalBackgroundColor, maxItemInOneLine);

            if (starredTopics.Count > 0 && unStarredTopics.Count > 0)
                GUILayout.Space(30); //space between starred and unstarred list

            if (unStarredTopics.Count > 0)
                UnStarredBoardsList(originalBackgroundColor, maxItemInOneLine);

            EditorGUILayout.EndScrollView();

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            CreateNewTopicButton();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUI.backgroundColor = originalBackgroundColor;
            BottomInformation();
        }
        /// <summary>
        /// Page containing details about a topic.
        /// Topic needs to be selected before calling this
        /// </summary>
        void Page_SelectedTopic()
        {
            if (!selectedTopic)
                return;
            selectedTopic.GetStats();
            proEditor?.GetMethod("NavigationTodoList").Invoke(null, new object[] { this, selectedTopic.myName, myStyleToolbarButton, myStyleToolbarLabel });

            GUI.backgroundColor = selectedTopic.mainColor;
            GUILayout.BeginVertical(myStyleVerticalBox); //wrapper for the page
            TopMenu_SelectedTopic();

            if (!selectedTopic) //if the topic has been deleted from the topmenu
            {
                GUILayout.EndVertical();
                return;
            }



            GUILayout.BeginVertical();
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            SelectedTopic_FiltersRightSide();

            float size = EditorGUIUtility.currentViewWidth - 300;
            int maxInALine = 1;
            bool needsToBeClosed = false;
            if (size > 2500)
                maxInALine = 4;
            else if (size > 2000)
                maxInALine = 3;
            else if (size > 1200)
                maxInALine = 3;
            else if (size > 800)
                maxInALine = 2;
            int itemInCurrentLine = 1;

            GUILayout.BeginVertical();
            GUI.backgroundColor = Color.white;
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
            GUI.backgroundColor = selectedTopic.mainColor;
            if (selectedTopic != null)
            {
                proEditor?.GetMethod("TopicBar").Invoke(null, new object[] { selectedTopic, myStyleToolbarButton, myStyleToolbarLabel, myStyleHorizontallBox, myStyleToolbarSearchField });

                for (int i = 0; i < selectedTopic.toDoLists.Count; i++)
                {
                    if (itemInCurrentLine == 1)
                    {
                        GUILayout.BeginHorizontal();
                        needsToBeClosed = true;
                    }
                    itemInCurrentLine++;
                    DrawToDoList(i);
                    GUILayout.Space(20);
                    if (itemInCurrentLine > maxInALine)
                    {
                        itemInCurrentLine = 1;
                        GUILayout.EndHorizontal();
                        needsToBeClosed = false;
                        GUILayout.Space(20);
                    }
                }
            }
            if (needsToBeClosed)
                GUILayout.EndHorizontal();
            GUILayout.Space(10);


            GUILayout.FlexibleSpace();


            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add new to do list", myStyleNormalButton, GUILayout.Height(40)))
            {
                Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                var myList = new ToDoList();
                myList.creationTime = new TGSTime(DateTime.Now);
                selectedTopic.toDoLists.Add(myList);
                EditorUtility.SetDirty(selectedTopic);
            }
            GUILayout.Space(20);
            GUILayout.EndHorizontal();


            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.EndVertical(); //wrapper for the page
        }


        bool page_home_topicBorder = true;
        bool page_home_icon = true;
        bool page_home_iconBorder = true;
        bool page_home_progressbarBorder = true;

        int spaceBetweenSettings = 5;
        /// <summary>
        /// The first page shown when opening the window.
        /// </summary>
        void Page_Settings()
        {
            TopMenu_Settings();

            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(indentLeft);

                    GUILayout.BeginVertical(); //main content goes here
                    {
                        GUILayout.Space(20);
                        GUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField("Page Transition Speed", myStyleLabel, GUILayout.Width(220));
                            float newAnimationSpeed = EditorGUILayout.FloatField("", PageTransitionSpeed, myStyleInputField, GUILayout.MaxWidth(50));
                            if (newAnimationSpeed != PageTransitionSpeed)
                            {
                                if (newAnimationSpeed > 0)
                                {
                                    EditorPrefs.SetFloat(editorPrefsPrefix + "PageTransitionSpeed", newAnimationSpeed);
                                    Undo.RecordObject(this, "Modular To-do lists Page Transition Speed update");
                                    PageTransitionSpeed = newAnimationSpeed;
                                    EditorUtility.SetDirty(this);
                                }
                            }

                            GUILayout.FlexibleSpace();
                        }
                        GUILayout.EndHorizontal();





                        GUILayout.Space(spaceBetweenSettings);


                        GUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField("Task is considered due soon if less than", myStyleToggle, GUILayout.Width(240));
                            int newTaskIsDueInDays = EditorGUILayout.IntField("", taskConsideredDueSoonIfLessThanDaysLeft, myStyleInputField, GUILayout.MaxWidth(50));
                            GUILayout.Space(5);
                            EditorGUILayout.LabelField(" days is left.", myStyleToggle, GUILayout.Width(220));
                            if (newTaskIsDueInDays != taskConsideredDueSoonIfLessThanDaysLeft)
                            {
                                if (newTaskIsDueInDays > 0)
                                {
                                    EditorPrefs.SetInt(editorPrefsPrefix + "taskIsDueInDays", newTaskIsDueInDays);
                                    Undo.RecordObject(this, "Modular To-do lists Task Is considerer Due In Days update");
                                    taskConsideredDueSoonIfLessThanDaysLeft = newTaskIsDueInDays;
                                    EditorUtility.SetDirty(this);
                                }
                            }

                            GUILayout.FlexibleSpace();
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.Space(spaceBetweenSettings);
                        GUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField(new GUIContent("Default task due date when created is, current time + "), myStyleToggle, GUILayout.Width(300));
                            int newTaskIsDueInHours = EditorGUILayout.IntField("", newTaskIsDueInCurrentTimePlusHours, myStyleInputField, GUILayout.MaxWidth(50));
                            GUILayout.Space(5);
                            EditorGUILayout.LabelField("Hours is left", myStyleToggle, GUILayout.Width(220));
                            if (newTaskIsDueInHours != newTaskIsDueInCurrentTimePlusHours)
                            {
                                if (newTaskIsDueInHours >= 0)
                                {
                                    EditorPrefs.SetInt(editorPrefsPrefix + "newTaskIsDueInCurrentTimePlusHours", newTaskIsDueInHours);
                                    Undo.RecordObject(this, "Modular To-do lists newTaskIsDueInCurrentTimePlusHours update");
                                    newTaskIsDueInCurrentTimePlusHours = newTaskIsDueInHours;
                                    EditorUtility.SetDirty(this);
                                }
                            }

                            GUILayout.FlexibleSpace();
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.Space(spaceBetweenSettings);



                        GUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField("Maximum task references in one line", myStyleToggle, GUILayout.Width(220));
                            int referencesInOneLine = EditorGUILayout.IntField("", maximumReferenceInOneLine, myStyleInputField, GUILayout.MaxWidth(50));
                            if (referencesInOneLine != maximumReferenceInOneLine)
                            {
                                if (referencesInOneLine > 0 && referencesInOneLine < 29)
                                {
                                    EditorPrefs.SetInt(editorPrefsPrefix + "maximumReferenceInOneLine", referencesInOneLine);
                                    Undo.RecordObject(this, "Modular To-do lists maximumReferenceInOneLine update");
                                    maximumReferenceInOneLine = referencesInOneLine;
                                    EditorUtility.SetDirty(this);
                                }
                            }

                            GUILayout.FlexibleSpace();
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.Space(spaceBetweenSettings);

                        GUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField("Font size", myStyleToggle, GUILayout.Width(220));
                            float newFontSize = EditorGUILayout.Slider(fontSize, 0.5f, 2);
                            if (newFontSize != fontSize)
                            {
                                if (newFontSize >= 0.5f && newFontSize <= 3)
                                {
                                    EditorPrefs.SetFloat(editorPrefsPrefix + "fontSize", newFontSize);
                                    Undo.RecordObject(this, "Modular To-do lists maximumReferenceInOneLine update");
                                    fontSize = newFontSize;
                                    recreateStyles = true;
                                    EditorUtility.SetDirty(this);
                                }
                            }

                            GUILayout.FlexibleSpace();
                        }
                        GUILayout.EndHorizontal();



                        GUILayout.Space(spaceBetweenSettings);

                        GUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField("Clicking on task", myStyleToolbarLabel, GUILayout.Width(100));
                            Color originalColor = GUI.color;
                            if (clickingOnTaskEditsIt)
                                GUI.color = new Color(1, 1, 1, 0.5f * originalColor.a);
                            if (GUILayout.Button("Opens the task", myStyleNormalButton))
                            {
                                clickingOnTaskEditsIt = false;
                                EditorPrefs.SetBool(editorPrefsPrefix + "clickingOnTaskEditsIt", clickingOnTaskEditsIt);
                            }

                            if (clickingOnTaskEditsIt)
                                GUI.color = new Color(1, 1, 1, 1f * originalColor.a);
                            else
                                GUI.color = new Color(1, 1, 1, 0.5f * originalColor.a);

                            if (GUILayout.Button("Edits it", myStyleNormalButton))
                            {
                                clickingOnTaskEditsIt = true;
                                EditorPrefs.SetBool(editorPrefsPrefix + "clickingOnTaskEditsIt", clickingOnTaskEditsIt);
                            }
                            GUI.color = originalColor;
                            GUILayout.FlexibleSpace();
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.Space(spaceBetweenSettings);

                        GUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField("Ask to confirm when deleting tasks and to do lists", myStyleToolbarLabel, GUILayout.Width(300));
                            bool newtaskAndToDoDeleteConfirmation = EditorGUILayout.Toggle(taskAndToDoDeleteConfirmation);
                            if (newtaskAndToDoDeleteConfirmation != taskAndToDoDeleteConfirmation)
                            {
                                EditorPrefs.SetBool(editorPrefsPrefix + "taskAndToDoDeleteConfirmation", newtaskAndToDoDeleteConfirmation);
                                Undo.RecordObject(this, "Modular To-do lists taskAndToDoDeleteConfirmation update");
                                taskAndToDoDeleteConfirmation = newtaskAndToDoDeleteConfirmation;
                                EditorUtility.SetDirty(this);
                            }
                            GUILayout.FlexibleSpace();
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.Space(spaceBetweenSettings);

                        GUILayout.Label("Homepage topic card settings", myStyleLabelFaded);

                        GUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField("Topic Border", myStyleToggle, GUILayout.Width(220));
                            bool border = EditorGUILayout.Toggle(page_home_topicBorder);
                            if (border != page_home_topicBorder)
                            {
                                EditorPrefs.SetBool(editorPrefsPrefix + "page_home_topicBorder", border);
                                Undo.RecordObject(this, "Modular To-do lists page_home_topicBorder update");
                                page_home_topicBorder = border;
                                EditorUtility.SetDirty(this);
                            }

                            GUILayout.FlexibleSpace();
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField("Icon", myStyleToggle, GUILayout.Width(220));
                            bool border = EditorGUILayout.Toggle(page_home_icon);
                            if (border != page_home_icon)
                            {
                                EditorPrefs.SetBool(editorPrefsPrefix + "page_home_icon", border);
                                Undo.RecordObject(this, "Modular To-do lists page_home_icon update");
                                page_home_icon = border;
                                EditorUtility.SetDirty(this);
                            }

                            GUILayout.FlexibleSpace();
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField("Icon Border", myStyleToggle, GUILayout.Width(220));
                            bool border = EditorGUILayout.Toggle(page_home_iconBorder);
                            if (border != page_home_iconBorder)
                            {
                                EditorPrefs.SetBool(editorPrefsPrefix + "page_home_iconBorder", border);
                                Undo.RecordObject(this, "Modular To-do lists page_home_iconBorder update");
                                page_home_iconBorder = border;
                                EditorUtility.SetDirty(this);
                            }

                            GUILayout.FlexibleSpace();
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField("Progressbar Border", myStyleToggle, GUILayout.Width(220));
                            bool border = EditorGUILayout.Toggle(page_home_progressbarBorder);
                            if (border != page_home_progressbarBorder)
                            {
                                EditorPrefs.SetBool(editorPrefsPrefix + "page_home_progressbarBorder", border);
                                Undo.RecordObject(this, "Modular To-do lists page_home_progressbarBorder update");
                                page_home_progressbarBorder = border;
                                EditorUtility.SetDirty(this);
                            }

                            GUILayout.FlexibleSpace();
                        }
                        GUILayout.EndHorizontal();


                        GUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField("Show percentage in Progressbars", myStyleToggle, GUILayout.Width(220));
                            bool newshowPercentageInProgressBar = EditorGUILayout.ToggleLeft("", showPercentageInProgressBar, myStyleToggle, GUILayout.Width(20));
                            if (newshowPercentageInProgressBar != showPercentageInProgressBar)
                            {
                                EditorPrefs.SetBool(editorPrefsPrefix + "showPercentageInProgressBar", newshowPercentageInProgressBar);
                                Undo.RecordObject(this, "Modular To-do lists showPercentageInProgressBar bool update");
                                showPercentageInProgressBar = newshowPercentageInProgressBar;
                                EditorUtility.SetDirty(this);
                            }
                            GUILayout.FlexibleSpace();
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();
                    GUILayout.Space(indentRight);
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(10);
                if (topics.Length > 0)
                    TopicSelectorButton(topics[0]);


                GUILayout.FlexibleSpace();

                BottomInformation();
            }
            GUILayout.EndVertical();


        }

        void Page_Task()
        {
            TopMenu_Task();

            GUILayout.BeginVertical();
            {
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(indentLeft);

                    GUILayout.BeginVertical(); //main content goes here
                    {
                        GUILayout.Space(10);
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Task:", myStyleLabelFaded);
                        GUILayout.FlexibleSpace();
                        if (selectedTask.editing)
                        {
                            if (GUILayout.Button("Done Editing", myStyleNormalButton, GUILayout.MinWidth(130)))
                            {
                                Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                                selectedTopic.Clicked();
                                selectedTask.editing = false;
                                EditorUtility.SetDirty(selectedTopic);
                                GUI.FocusControl(null);
                            }
                        }
                        else
                        {
                            if (GUILayout.Button("Edit", myStyleNormalButton, GUILayout.MinWidth(130)))
                            {
                                Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                                selectedTopic.Clicked();
                                selectedTask.editing = true;
                                EditorUtility.SetDirty(selectedTopic);
                                GUI.FocusControl(null);
                            }
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        if (!selectedTask.editing)
                            GUILayout.Label(selectedTask.myName, myStyleLabelLarge);
                        else
                        {
                            string newName = EditorGUILayout.TextField(selectedTask.myName, myStyleInputFieldLarge);
                            if (newName != selectedTask.myName)
                            {
                                Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                                selectedTask.myName = newName;
                                EditorUtility.SetDirty(selectedTopic);
                            }
                        }
                        GUILayout.EndHorizontal();


                        GUILayout.BeginHorizontal();
                        if ((selectedTask.addedDescription && !string.IsNullOrEmpty(selectedTask.myDescription)))
                            GUILayout.Label("Description:", myStyleLabelFaded);
                        GUILayout.FlexibleSpace();
                        if (selectedTask.editing)
                        {
                            string label;
                            if (!selectedTask.addedDescription)
                                label = "Add description";
                            else
                                label = "Remove description";
                            if (GUILayout.Button(label, myStyleNormalButton, GUILayout.MinWidth(105)))
                            {
                                Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                                selectedTask.addedDescription = !selectedTask.addedDescription;
                                EditorUtility.SetDirty(selectedTopic);
                            }
                        }
                        GUILayout.EndHorizontal();

                        if (selectedTask.addedDescription)
                        {
                            if (!selectedTask.editing)
                                GUILayout.Label(selectedTask.myDescription, myStyleLabel);
                            else
                            {
                                string newDescription = EditorGUILayout.TextField(selectedTask.myDescription, myStyleInputField);
                                if (newDescription != selectedTask.myName)
                                {
                                    Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                                    selectedTask.myDescription = newDescription;
                                    EditorUtility.SetDirty(selectedTopic);
                                }
                            }
                        }


                        GUILayout.Space(25);
                        GUILayout.BeginHorizontal(EditorStyles.toolbar);
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("Created on " + selectedTask.creationTime.GetFullTime(), myStyleToolbarLabel);
                        GUILayout.Space(5);
                        GUILayout.EndHorizontal();
                        if (selectedTask.completed)
                        {
                            GUILayout.BeginHorizontal(EditorStyles.toolbar);
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("Completed on " + selectedTask.completionTime.GetFullTime(), myStyleToolbarLabel);
                            GUILayout.Space(5);
                            GUILayout.EndVertical();
                        }
                        else if (selectedTask.failed)
                        {
                            GUILayout.BeginHorizontal(EditorStyles.toolbar);
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("Failed on " + selectedTask.failedTime.GetFullTime(), myStyleToolbarLabel);
                            GUILayout.Space(5);
                            GUILayout.EndVertical();
                        }
                        else
                        {
                            DrawTaskDueDateEdit(selectedTask, true);
                        }

                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        TaskButtons(selectedTodoList, selectedTask, true, true, true, false);
                        GUILayout.EndHorizontal();

                    }
                    GUILayout.Space(20);
                    GUILayout.BeginHorizontal(EditorStyles.toolbar);
                    GUILayout.Space(5);
                    GUILayout.Label("References:", myStyleToolbarLabel);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(addIcon, myStyleToolbarButton, GUILayout.Width(25)))
                    {
                        Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                        selectedTask.addedReference = true;
                        selectedTask.references.Add(null);
                        EditorUtility.SetDirty(selectedTopic);
                    }
                    GUILayout.EndHorizontal();
                    TaskReferences(1, selectedTask, false, false);
                    GUILayout.EndVertical();
                    GUILayout.Space(indentRight);
                }
                GUILayout.EndHorizontal();

                GUILayout.FlexibleSpace();
            }
            GUILayout.EndVertical();
        }



        #region Page transitions
        bool TopicListFadedOut()
        {
            if (page_topicListAnimBool.target)
                page_topicListAnimBool.target = false;

            if (page_topicListAnimBool.faded > 0)
            {
                Color c = GUI.color;
                c.a = page_topicListAnimBool.faded;
                GUI.color = c;
                Page_ListOfTopics();
                return false;
            }
            return true;
        }
        bool CardListFadedOut()
        {
            if (page_cardListAnimBool.target)
                page_cardListAnimBool.target = false;

            if (page_cardListAnimBool.faded > 0)
            {
                Color c = GUI.color;
                c.a = page_cardListAnimBool.faded;
                GUI.color = c;
                if (selectedTopic)
                    Page_SelectedTopic();
                return false;
            }
            return true;
        }

        bool SettingsListFadedOut()
        {
            if (page_settingsAnimBool.target)
                page_settingsAnimBool.target = false;

            if (page_settingsAnimBool.faded > 0)
            {
                Color c = GUI.color;
                c.a = page_settingsAnimBool.faded;
                GUI.color = c;
                Page_Settings();
                return false;
            }
            return true;
        }
        #endregion Page transitions




        void CreateNewTopicButton()
        {
            if (GUILayout.Button("Create Topic", myStyleNormalButton, GUILayout.MinWidth(60), GUILayout.MaxWidth(200), GUILayout.MinHeight(30)))
            {
                CreateNewTopic();
                RefreshBoardsList();
            }
        }

        #region Stuff in List of Topics page
        void BottomInformation()
        {
            if (Screen.width < 400)
                return;
            GUI.backgroundColor = breadCrumbBackgroundColor;
            GUILayout.BeginVertical(myStyleWhiteBox);

            //GUILayout.BeginHorizontal(GUILayout.Height(EditorGUIUtility.singleLineHeight)); //hide button
            //GUILayout.FlexibleSpace();

            //if (bottomInfoAnimBool.target == true)
            //{
            //    if (GUILayout.Button(ignoreIcon, myStyleToolbarButton, GUILayout.Width(20), GUILayout.Height(15)))
            //    {
            //        bottomInfoAnimBool.target = false;
            //    }
            //}
            //else
            //{
            //    if (GUILayout.Button(upIcon, myStyleToolbarButton, GUILayout.Width(20), GUILayout.Height(15)))
            //    {
            //        bottomInfoAnimBool.target = true;
            //    }
            //}
            //GUILayout.EndHorizontal(); //end of hide button

            GUILayout.Space(5); //space at the top
            if (EditorGUILayout.BeginFadeGroup(bottomInfoAnimBool.faded))
            {
                GUILayout.BeginHorizontal();
                {
                    if (Screen.width > 900)
                        CompanyRow();

                    GUILayout.Space(10);

                    if (Screen.width > 400)
                        ProductRow();

                    GUILayout.Space(5);

                    if (Screen.width > 600 && proEditor == null)
                    {
                        float size = 150;
                        if (GUILayout.Button(getTheProVersionTexture, EditorStyles.label, GUILayout.Width(size), GUILayout.Height(size * 0.25f)))
                        {
                            Application.OpenURL(proAssetLink);
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.Space(5); //space at the bottom
            EditorGUILayout.EndFadeGroup();
            GUILayout.EndVertical(); //end of helpbox the full wrapper
        }

        void ProductRow()
        {
            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(assetIcon, myStyleCompanyName, GUILayout.Width(40), GUILayout.Height(40)))
                    {
                        Application.OpenURL(currentAssetLink);
                    }
                    GUILayout.Space(5);
                    GUILayout.BeginVertical();
                    {
                        GUILayout.Space(8);
                        GUILayout.Label("Modular To Do List", myStyleCompanyName);

                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("Version : " + versionNumber, myStyleLabel, GUILayout.Width(75));

                            if (GUILayout.Button("  (Check Update)", myStyleLinkLabel))
                            {
                                Application.OpenURL(currentAssetLink);
                            }
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        void CompanyRow()
        {
            GUILayout.BeginVertical(GUILayout.Height(30));
            {
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(companyIcon, myStyleCompanyName, GUILayout.Width(40), GUILayout.Height(40)))
                    {
                        Application.OpenURL("https://linktr.ee/tinygiantstudio"); //link to all
                    }

                    GUILayout.BeginVertical();
                    {
                        GUILayout.Space(8);
                        if (GUILayout.Button("Tiny Giant Studio", myStyleCompanyName))
                        {
                            Application.OpenURL("https://linktr.ee/tinygiantstudio"); //link to all
                        }
                        if (GUILayout.Button("Get more amazing assets!", myStyleLinkLabel))
                        {
                            Application.OpenURL(getMoreAssetsLink);
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        void TopMenu_ListOfTopics()
        {

            Color bgColor = GUI.backgroundColor;
            breadCrumbBackgroundColor.a = bgColor.a;
            GUI.backgroundColor = breadCrumbBackgroundColor;


            GUILayout.BeginVertical(myStyleWhiteBox);
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(indentLeft);
                if (GUILayout.Button(refreshIcon, myStyleLinkLabel, GUILayout.Width(25)))
                {
                    recreateStyles = true;
                    RefreshBoardsList();
                    page_topicListAnimBool.value = false;
                    GetGraphicsReferences();

                    ShowNotification(new GUIContent("Topics list refreshed successfully"));
                }
                GUILayout.Label("Home", myStyleBreadCrumbLabel);

                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Settings", myStyleBreadCrumbLink))
                {
                    currentPage = Page.settings;
                }
                //if (GUILayout.Button(" Import", myStyleBreadCrumbLink))
                //{
                //    TopicUtility.ReadTopicFromCsv();
                //    RefreshBoardsList();
                //}
                GUILayout.Space(indentRight);
            }
            GUILayout.EndHorizontal();

            GUI.backgroundColor = bgColor;

            proEditor?.GetMethod("NavigationTopicsList").Invoke(null, new object[] { indentLeft, indentRight, myStyleWhiteBox, editorPrefsPrefix });

            GUILayout.Space(10);
            GUILayout.EndVertical();

        }

        void StarredBoardsList(Color originalBackgroundColor, int maxItemInOneLine)
        {
            int itemInCurrentLine = 0;

            GUILayout.BeginVertical();

            bool needToCloseHorizontal = false;
            foreach (Topic toDoList in starredTopics)
            {
                if (toDoList == null) //adding break here causes the entire editor to break/return causing incomplete layouts = error. pretty weird ngl.
                    requiresRefreshingTopicList = true;
                else
                {
                    if (itemInCurrentLine == 0)
                    {
                        EditorGUILayout.BeginHorizontal();
                        needToCloseHorizontal = true;
                    }

                    itemInCurrentLine++;

                    TopicSelectorButton(toDoList);

                    if (itemInCurrentLine >= maxItemInOneLine)
                    {
                        EditorGUILayout.EndHorizontal();
                        needToCloseHorizontal = false;
                        GUILayout.Space(10);

                        itemInCurrentLine = 0;
                    }
                }
            }


            if (needToCloseHorizontal)
            {
                if (unStarredTopics.Count == 0)
                {
                    GUILayout.BeginHorizontal();
                    //CreateNewBoardButton();
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                if (unStarredTopics.Count == 0)
                {
                    GUILayout.BeginHorizontal();
                    //CreateNewBoardButton();
                    EditorGUILayout.EndHorizontal();
                }
                GUI.backgroundColor = originalBackgroundColor;
            }
            GUILayout.EndVertical();
        }
        void UnStarredBoardsList(Color originalBackgroundColor, int maxItemInOneLine)
        {
            int itemInCurrentLine = 0;
            bool needToCloseHorizontal = false;

            foreach (Topic toDoList in unStarredTopics)
            {
                if (toDoList == null) //adding break here causes the entire editor to break/return causing incomplete layouts = error. pretty weird ngl.
                    requiresRefreshingTopicList = true;
                else
                {
                    if (itemInCurrentLine == 0)
                    {
                        EditorGUILayout.BeginHorizontal();
                        needToCloseHorizontal = true;
                    }

                    itemInCurrentLine++;

                    TopicSelectorButton(toDoList);

                    if (itemInCurrentLine >= maxItemInOneLine)
                    {
                        EditorGUILayout.EndHorizontal();
                        needToCloseHorizontal = false;

                        itemInCurrentLine = 0;
                    }
                }
            }


            if (needToCloseHorizontal)
            {
                EditorGUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// For all topics list in homepage
        /// </summary>
        /// <param name="topic"></param>
        void TopicSelectorButton(Topic topic)
        {
            if (proEditor != null)
            {
                string searchingFor = EditorPrefs.GetString(editorPrefsPrefix + "searchingFor");

                if (!string.IsNullOrEmpty(searchingFor))
                {
                    topic.Search(searchingFor);

                    if (!topic.keywordFound)
                        return;
                    GUILayout.BeginVertical();
                    {
                        EditorGUILayout.LabelField("Keyword found in: ", myStyleLabelFaded);
                        if (topic.searchKeywordInTopicName)
                            EditorGUILayout.LabelField("Topic name", myStyleLabel);
                        if (topic.searchKeywordInTopicDescription)
                            EditorGUILayout.LabelField("Topic description", myStyleLabel);
                        if (topic.searchKeywordInTodoListName > 0)
                            EditorGUILayout.LabelField(topic.searchKeywordInTodoListName + " to-do list name", myStyleLabel);
                        if (topic.searchKeywordInTodoListDescription > 0)
                            EditorGUILayout.LabelField(topic.searchKeywordInTodoListDescription + " to-do list description", myStyleLabel);
                        if (topic.searchKeywordInTaskNames > 0)
                            EditorGUILayout.LabelField(topic.searchKeywordInTaskNames + " task name", myStyleLabel);
                        if (topic.searchKeywordInTaskDescription > 0)
                            EditorGUILayout.LabelField(topic.searchKeywordInTaskDescription + " task description", myStyleLabel);
                    }
                    GUILayout.EndVertical();
                }
            }


            Color originalBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = topic.mainColor;

            if (page_home_topicBorder)
                GUILayout.BeginVertical(EditorStyles.helpBox);
            else
                GUILayout.BeginVertical();
            {
                GUILayout.BeginVertical(myStyleVerticalBox, GUILayout.Height(80));
                {
                    GUILayout.BeginHorizontal(); //start of main
                    {
                        GUILayout.Space(10);
                        if (page_home_icon)
                        {
                            GUILayout.BeginVertical(GUILayout.Width(68)); //start of icon
                            GUILayout.Space(10);

                            DrawBoardIcon(topic, originalBackgroundColor);

                            GUILayout.FlexibleSpace();
                            GUILayout.EndVertical(); //end of icon
                        }
                        GUILayout.BeginVertical(); //start of information
                        {
                            GUILayout.Space(10);
                            GUILayout.BeginHorizontal();
                            {
                                if (topic.starred)
                                {
                                    GUILayout.Label(starIcon, myStyleIcon, GUILayout.Width(15), GUILayout.Height(15));
                                    GUILayout.Space(5);
                                }

                                if (GUILayout.Button(topic.myName, myStyleLinkLabelBigger))
                                {
                                    OpenTopic(topic);
                                }

                                //GUILayout.Label(board.myName, myStyleAssetName);
                                GUILayout.FlexibleSpace();
                                GUI.backgroundColor = new Color(1, 1, 1, originalBackgroundColor.a);
                                if (GUILayout.Button("Open", myStyleNormalButton))
                                {
                                    OpenTopic(topic);
                                }
                                //if (GUILayout.Button("Export", myStyleNormalButton))
                                //{
                                //    TopicExporter.topic = topic;
                                //    TopicExporter.ShowEditor();
                                //    //var exporter = TopicExporter.Open();
                                //}

                                GUILayout.Space(indentRight);
                            }
                            GUILayout.EndHorizontal();

                            GUILayout.Space(5);

                            if (!string.IsNullOrEmpty(topic.myDescription))
                            {
                                GUILayout.Label(topic.myDescription, myStyleLabel, GUILayout.MinWidth(100));
                                GUILayout.Space(12);
                            }

                            TopicDueDates(topic);
                        }
                        GUILayout.EndVertical(); //end of information
                    }
                    GUILayout.EndHorizontal(); //end of main

                    GUILayout.Space(5);
                    BoardProgressBar(topic);
                    GUILayout.Space(5);

                }
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();
            GUI.backgroundColor = originalBackgroundColor;
        }

        void TopicDueDates(Topic board)
        {
            Color originalContentColor = GUI.contentColor;
            GUILayout.BeginHorizontal();
            {
                if (board.dueSoonTasksCount > 0)
                {
                    GUI.contentColor = new Color(1f, 0.75f, 0.75f, originalContentColor.a);
                    GUILayout.Label(new GUIContent("Tasks due soon : " + board.dueSoonTasksCount, "Task is considered due if less than " + taskConsideredDueSoonIfLessThanDaysLeft + " days is left. This amount can be modified from settings page."), myStyleLabel, GUILayout.MinWidth(100));
                }
                if (board.backlogTasksCount > 0)
                {
                    GUI.contentColor = new Color(1f, 0.5f, 0.5f, originalContentColor.a);
                    GUILayout.Label(new GUIContent("Backlogs : " + board.backlogTasksCount, "Tasks that has passed it's due date"), myStyleLabel, GUILayout.MinWidth(100));
                }
            }
            GUILayout.EndHorizontal();
            GUI.contentColor = originalContentColor;
        }
        #endregion Stuff in List of Boards page




        #region Stuff in Selected Topic page

        #region Top menu for Selected Topic page
        void TopMenu_SelectedTopic()
        {
            if (!selectedTopic)
                return;

            TopMenu_SelectedTopic_BreadCrumb();

            Color originalBackgroundColor = GUI.backgroundColor;

            //GUILayout.BeginVertical(myStyleVerticalBox, GUILayout.Height(80));
            GUILayout.BeginVertical(GUILayout.Height(80));



            GUILayout.BeginHorizontal(); //start of main


            //GUILayout.BeginVertical();
            //GUILayout.FlexibleSpace();
            //BackToTopicsListButton();
            //GUILayout.FlexibleSpace();
            //GUILayout.EndVertical();


            GUILayout.Space(10);

            if (page_home_icon)
            {
                GUILayout.BeginVertical(GUILayout.Width(68)); //start of icon
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                GUILayout.Space(5);
                DrawBoardIcon(selectedTopic, originalBackgroundColor);
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.EndVertical(); //end of icon
            }

            GUILayout.BeginVertical(); //start of information
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            BoardNameEditButton();

            BookmarkBoardButton();

            if (!selectedTopic.editing)
            {
                GUILayout.Space(2);
                if (GUILayout.Button(selectedTopic.myName, myStyleLinkLabelBigger))
                {
                    EditorGUIUtility.PingObject(selectedTopic);
                }
            }
            else
            {
                string newName = EditorGUILayout.TextField(selectedTopic.myName, myStyleLinkLabelBiggerInputField, GUILayout.MaxWidth(1000));
                if (newName != selectedTopic.myName)
                {
                    Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                    selectedTopic.myName = newName;
                    EditorUtility.SetDirty(selectedTopic);
                }
            }



            GUILayout.FlexibleSpace();
            bool removedBoard = false;
            if (selectedTopic.editing)
            {
                GUILayout.Space(5);
                var newScheme = (ColorScheme)EditorGUILayout.ObjectField(selectedTopic.colorScheme, typeof(ColorScheme), false);
                if (newScheme != selectedTopic.colorScheme)
                {
                    Undo.RecordObject(selectedTopic, selectedTopic.name + " color scheme update");
                    selectedTopic.colorScheme = newScheme;
                    EditorUtility.SetDirty(selectedTopic);
                }

                BoardColorButton();
                DeleteBoardButton(out removedBoard);
            }
            GUILayout.Space(indentRight);
            GUILayout.EndHorizontal();

            GUILayout.Space(5); //space after title
            if (!removedBoard)
            {
                if (selectedTopic.editing)
                {
                    GUILayout.BeginHorizontal();
                    string newDescription = EditorGUILayout.TextArea(selectedTopic.myDescription, myStyleInputField, GUILayout.MaxWidth(1000));
                    GUILayout.Space(indentRight);
                    GUILayout.EndHorizontal();
                    if (newDescription != selectedTopic.myDescription)
                    {
                        Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                        selectedTopic.myDescription = newDescription;
                        EditorUtility.SetDirty(selectedTopic);
                    }
                }
                else if (!string.IsNullOrEmpty(selectedTopic.myDescription))
                {
                    GUILayout.Space(4);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(6);
                    GUILayout.Label(selectedTopic.myDescription, myStyleLabel, GUILayout.MinWidth(100));
                    GUILayout.Space(indentRight);
                    GUILayout.EndHorizontal();
                    GUILayout.Space(5);
                }


                GUILayout.BeginHorizontal();
                GUILayout.Space(5);
                TopicDueDates(selectedTopic);
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }

            GUILayout.EndVertical(); //end of information
            GUILayout.EndHorizontal(); //end of main

            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            BoardProgressBar(selectedTopic);
            GUILayout.Space(7);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUI.backgroundColor = originalBackgroundColor;
        }

        bool editLocked = false;
        void TopMenu_SelectedTopic_BreadCrumb()
        {
            Color bgColor = GUI.backgroundColor;
            breadCrumbBackgroundColor.a = bgColor.a;
            GUI.backgroundColor = breadCrumbBackgroundColor;


            GUILayout.BeginVertical(myStyleWhiteBox);
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(indentLeft);

                if (GUILayout.Button("Home", myStyleBreadCrumbLink))
                {
                    OpenTopicsList();
                }

                GUILayout.Label(" / ", myStyleBreadCrumbLabel);

                GUILayout.Label(selectedTopic.myName, myStyleBreadCrumbLabel);

                GUILayout.FlexibleSpace();
                GUIContent lockIcon;
                if (editLocked)
                {
                    GUILayout.Label("Edit locked", myStyleBreadCrumbLabelFaded, GUILayout.Width(80));
                    lockIcon = EditorGUIUtility.IconContent("LockIcon-On");
                }
                else
                {
                    lockIcon = EditorGUIUtility.IconContent("LockIcon");
                }
                if (GUILayout.Button(lockIcon, myStyleBreadCrumbLink))
                {
                    Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                    editLocked = !editLocked;
                    EditorUtility.SetDirty(selectedTopic);
                }
                GUILayout.Space(indentRight);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.EndVertical();

            GUI.backgroundColor = bgColor;
        }

        void DeleteBoardButton(out bool removed)
        {
            removed = false;
            GUIContent removeButtonContent = new GUIContent(trashIcon);
            removeButtonContent.tooltip = "Delete board";
            if (GUILayout.Button(removeButtonContent, myStyleIcon))
            {
                if (EditorUtility.DisplayDialog("Delete " + selectedTopic.myName, //dialog title
                        "Are you sure you want to delete this board? \n\nThis doesn't support undo. The board will be moved to the trash of your OS.", //dialog description
                        "Delete", "Do Not Delete"))
                {
                    removed = true;
                    var path = AssetDatabase.GetAssetPath(selectedTopic);
                    AssetDatabase.MoveAssetToTrash(path);
                    //Undo.DestroyObjectImmediate(selectedBoard);
                    selectedTopic = null;
                    selectedTopicSO = null;
                    reorderableListsOfTask.Clear();
                    serializedistsOfTask.Clear();
                    currentPage = Page.topicList;
                    RefreshBoardsList();
                }
            }
        }

        void BoardColorButton()
        {
            if (page_cardListAnimBool.faded > 0.2f)
            {
                GUILayout.Space(5);
                Color boardColor = EditorGUILayout.ColorField(GUIContent.none, selectedTopic.mainColor, false, false, false, GUILayout.Height(15), GUILayout.Width(15 * page_cardListAnimBool.faded));
                if (boardColor != selectedTopic.mainColor)
                {
                    Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                    selectedTopic.mainColor = boardColor;
                    //selectedBoard.Clicked();
                    EditorUtility.SetDirty(selectedTopic);
                }
                GUILayout.Space(5);
            }
        }

        void BoardNameEditButton()
        {
            if (editLocked)
                return;

            Color original = GUI.contentColor;
            if (selectedTopic.editing)
                GUI.contentColor = new Color(1, 1, 0, original.a);
            if (GUILayout.Button(editIcon, myStyleIcon, GUILayout.Height(20), GUILayout.Width(20)))
            {
                Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                selectedTopic.editing = !selectedTopic.editing;
                EditorUtility.SetDirty(selectedTopic);
                UpdateStarredUnstarredBoardsList();
            }
            GUI.contentColor = original;

        }

        void BookmarkBoardButton()
        {
            if (!selectedTopic)
                return;


            if (!selectedTopic.starred)
            {
                if (GUILayout.Button(new GUIContent(unStarIcon, "Boorkmarked board will appear on top on boards list."), myStyleIcon, GUILayout.Height(20), GUILayout.Width(20)))
                {
                    Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                    selectedTopic.starred = true;
                    selectedTopic.Clicked();
                    EditorUtility.SetDirty(selectedTopic);
                    UpdateStarredUnstarredBoardsList();
                }
            }
            else
            {
                if (GUILayout.Button(new GUIContent(starIcon, "Boorkmarked board will appear on top on boards list."), myStyleIcon, GUILayout.Height(20), GUILayout.Width(20)))
                {
                    Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                    selectedTopic.starred = false;
                    selectedTopic.Clicked();
                    EditorUtility.SetDirty(selectedTopic);
                    UpdateStarredUnstarredBoardsList();
                }
            }
        }

        //void BackToTopicsListButton()
        //{
        //    GUIContent backIconContent = new GUIContent(backIcon, "Back to boards");
        //    GUILayout.BeginVertical();
        //    GUILayout.FlexibleSpace();
        //    if (GUILayout.Button(backIconContent, EditorStyles.whiteLabel, GUILayout.Height(30), GUILayout.Width(30)))
        //    {
        //        OpenTopicsList();
        //    }
        //    GUILayout.FlexibleSpace();
        //    GUILayout.EndVertical();
        //}

        public void OpenTopicsList()
        {
            currentPage = Page.topicList;

            if (selectedTopic)
                selectedTopic.Clicked();

            UpdateTopicsStats();
        }

        void UpdateTopicsStats()
        {
            for (int i = 0; i < topics.Length; i++)
            {
                if (topics[i] != null)
                    topics[i].GetStats();
            }
        }
        #endregion Top menu for Selected Topic page



        #region SelectedTopic_FiltersRightSide
        void SelectedTopic_FiltersRightSide()
        {
            if (selectedTopic == null)
                return;

            Color c = GUI.color;
            float originalAlpha = c.a;
            Color originalContentColor = GUI.contentColor;
            Color originalBackgroundColor = GUI.backgroundColor;

            if (filterListAnimBool.faded != 0)
            {
                if (filterListAnimBool.faded < originalAlpha)
                    c.a = filterListAnimBool.faded;
                GUI.color = c;
                SelectedTopic_FiltersList(originalContentColor, originalBackgroundColor);

            }
            else if (filterListAnimBool.faded == 0)
            {
                SelectedTopic_OpenFiltersButton();
            }

            GUI.backgroundColor = originalBackgroundColor;
            c.a = originalAlpha;
            GUI.color = c;
        }

        void SelectedTopic_FiltersList(Color originalContentColor, Color originalBackgroundColor)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(150 * filterListAnimBool.faded));

            TaskStateFilters(originalContentColor);

            GUI.backgroundColor = originalBackgroundColor;
            proEditor?.GetMethod("TodoTags").Invoke(null, new object[] { selectedTopic, filterListAnimBool.faded, addIcon, editIcon, trashIcon, myStyleToggle });

            GUILayout.Space(25);

            GUI.backgroundColor = originalBackgroundColor;
            VisualSettings();

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();

            GUI.backgroundColor = originalBackgroundColor;
        }

        void TaskStateFilters(Color originalContentColor)
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Space(5);
            GUILayout.Label("Filters", myStyleToolbarLabel, GUILayout.Width(140 * filterListAnimBool.faded));

            if (GUILayout.Button(backIcon, myStyleToolbarButton, GUILayout.Width(20 * filterListAnimBool.faded)))
            {
                filterListAnimBool.target = false;
                EditorPrefs.SetBool("filterListAnimBool", filterListAnimBool.target);
            }
            GUI.contentColor = originalContentColor;
            GUILayout.EndHorizontal();

            GUI.backgroundColor = Color.white;
            bool showPendingTasks = EditorGUILayout.ToggleLeft("Pending Tasks", selectedTopic.showPendingItems, myStyleToggle, GUILayout.Width(130 * filterListAnimBool.faded));
            if (showPendingTasks != selectedTopic.showPendingItems)
            {
                Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                selectedTopic.showPendingItems = showPendingTasks;
                EditorUtility.SetDirty(selectedTopic);
            }

            bool showCompletedItems = EditorGUILayout.ToggleLeft("Completed Tasks", selectedTopic.showCompletedItems, myStyleToggle, GUILayout.Width(130 * filterListAnimBool.faded));
            if (showCompletedItems != selectedTopic.showCompletedItems)
            {
                Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                selectedTopic.showCompletedItems = showCompletedItems;
                EditorUtility.SetDirty(selectedTopic);
            }
            bool showIgnoredItems = EditorGUILayout.ToggleLeft("Ignored Tasks", selectedTopic.showIgnoredItems, myStyleToggle, GUILayout.Width(130 * filterListAnimBool.faded));
            if (showIgnoredItems != selectedTopic.showIgnoredItems)
            {
                Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                selectedTopic.showIgnoredItems = showIgnoredItems;
                EditorUtility.SetDirty(selectedTopic);
            }
            bool showFailedItems = EditorGUILayout.ToggleLeft("Failed Tasks", selectedTopic.showFailedTasks, myStyleToggle, GUILayout.Width(130 * filterListAnimBool.faded));
            if (showFailedItems != selectedTopic.showFailedTasks)
            {
                Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                selectedTopic.showFailedTasks = showFailedItems;
                EditorUtility.SetDirty(selectedTopic);
            }
        }

        void VisualSettings()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Space(5);
            GUILayout.Label("Display", myStyleToolbarLabel, GUILayout.Width(160 * filterListAnimBool.faded));
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            GUI.backgroundColor = Color.white;
            bool showTaskDetails = EditorGUILayout.ToggleLeft(new GUIContent("Task Details", ""), selectedTopic.showTaskDetails, myStyleToggle, GUILayout.Width(130 * filterListAnimBool.faded));
            if (showTaskDetails != selectedTopic.showTaskDetails)
            {
                Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                selectedTopic.showTaskDetails = showTaskDetails;
                EditorUtility.SetDirty(selectedTopic);
            }
            bool hideFolderIcons = EditorGUILayout.ToggleLeft(new GUIContent("Hide Folder Icons", ""), selectedTopic.hideFolderIcons, myStyleToggle, GUILayout.Width(130 * filterListAnimBool.faded));
            if (hideFolderIcons != selectedTopic.hideFolderIcons)
            {
                Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                selectedTopic.hideFolderIcons = hideFolderIcons;
                EditorUtility.SetDirty(selectedTopic);
            }


            bool alwaysShowIconsOnly = EditorGUILayout.ToggleLeft(new GUIContent("Task Icons Only", "If set to false, text will be shown when the window size is big enough."), selectedTopic.alwaysShowIconsOnly, myStyleToggle, GUILayout.Width(130 * filterListAnimBool.faded));
            if (alwaysShowIconsOnly != selectedTopic.alwaysShowIconsOnly)
            {
                Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                selectedTopic.alwaysShowIconsOnly = alwaysShowIconsOnly;
                EditorUtility.SetDirty(selectedTopic);
            }

            bool showRealIndex = EditorGUILayout.ToggleLeft(new GUIContent("Show Real Index", "If set to true, the index in to do list will be shown, otherwise, it shown in a \"neat and clean\" form starting from index 1."), selectedTopic.showRealIndex, myStyleToggle, GUILayout.Width(130 * filterListAnimBool.faded));
            if (showRealIndex != selectedTopic.showRealIndex)
            {
                Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                selectedTopic.showRealIndex = showRealIndex;
                EditorUtility.SetDirty(selectedTopic);
            }
        }

        void SelectedTopic_OpenFiltersButton()
        {
            GUILayout.BeginVertical();
            GUILayout.Space(5);
            if (GUILayout.Button(nextIcon, myStyleIcon, GUILayout.Height(20), GUILayout.Width(20)))
            {
                filterListAnimBool.target = true;
                EditorPrefs.SetBool("filterListAnimBool", filterListAnimBool.target);
            }
            GUILayout.EndVertical();
        }

        #endregion SelectedTopic_FiltersRightSide




        void DrawToDoList(int i)
        {
            ToDoList selectedToDoList = selectedTopic.toDoLists[i];

            float originalAlpha = GUI.color.a;
            Color todoListColor = selectedToDoList.mainColor;
            todoListColor.a = originalAlpha;
            GUI.color = todoListColor;
            todoListColor.a = originalAlpha;
            GUI.backgroundColor = todoListColor;
            Color originalContentColor = GUI.contentColor;


            //GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MaxWidth(1000)); //main container
            GUILayout.BeginVertical(myStyleVerticalBox); //main container
            {
                //ToDoTimes(i);
                if (selectedToDoList != null)
                    ToDoListTitleToolbar(i, selectedToDoList, originalContentColor);

                if (selectedToDoList != null) //this check is incase it was deleted
                {
                    if (selectedToDoList.opened)
                    {
                        ToDoListDescription(selectedToDoList);
                        GUILayout.Space(5);
                        ListOfTasks(i, originalAlpha);

                        GUI.color = new Color(1, 1, 1, originalAlpha);
                    }
                }
            }
            GUILayout.EndVertical(); //main container
            //GUILayout.EndVertical(); //main container
        }

        void ToDoListTitleToolbar(int todoListIndex, ToDoList selectedToDoList, Color originalContentColor)
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                ReorderToDoListButtons(selectedTopic, todoListIndex, selectedToDoList);

                if (selectedToDoList != null)
                {
                    ToDoListFolderIcon(selectedToDoList);

                    if (UnityEngine.Object.ReferenceEquals(selectedTodoList, null))
                    {
                        selectedTodoList = selectedTopic.toDoLists[todoListIndex];
                    }
                    if (!editLocked)
                    {
                        //if (selectedTodoList.editing)
                        {
                            if (GUILayout.Button(editIcon, myStyleToolbarButton, GUILayout.Width(20)))
                            {
                                Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                                bool shouldbe = !selectedToDoList.editing;
                                selectedTopic.Clicked();
                                selectedToDoList.editing = shouldbe;
                                EditorUtility.SetDirty(selectedTopic);
                                GUI.FocusControl(null);
                            }
                        }
                    }

                    if (selectedToDoList.editing)
                    {
                        string listName = EditorGUILayout.TextField(string.Empty, selectedToDoList.myName, GUILayout.MinWidth(20));
                        if (listName != selectedToDoList.myName)
                        {
                            Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                            selectedToDoList.myName = listName;
                            EditorUtility.SetDirty(selectedTopic);
                        }
                    }
                    else
                    {
                        GUILayout.BeginVertical();
                        GUILayout.FlexibleSpace();
                        GUILayout.BeginHorizontal();
                        if (selectedToDoList != null)
                        {
                            GUILayout.Label(selectedToDoList.myName + " ", myStyleTodoLabel);
                            GUILayout.Label("(" + selectedToDoList.GetActiveTaskCount() + ")", EditorStyles.miniLabel);
                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                        GUILayout.FlexibleSpace();
                        GUILayout.EndVertical();
                    }

                    if (page_cardListAnimBool.faded > 0.2f && selectedToDoList.editing)
                    {
                        GUILayout.BeginVertical(GUILayout.MaxWidth(10));
                        GUILayout.FlexibleSpace();
                        Color listColor = EditorGUILayout.ColorField(GUIContent.none, selectedToDoList.mainColor, false, false, false, GUILayout.Height(15), GUILayout.Width(15 * page_cardListAnimBool.faded));
                        if (listColor != selectedToDoList.mainColor)
                        {
                            Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                            selectedToDoList.mainColor = listColor;
                            EditorUtility.SetDirty(selectedTopic);
                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.EndVertical();
                    }
                    GUILayout.Space(5);

                    //var proEditor = Type.GetType("TinyGiantStudio.ModularToDoList.ModularToDoListProEditor");
                    proEditor?.GetMethod("ToDoListBar").Invoke(null, new object[] { selectedTopic, selectedToDoList, myStyleToolbarButton, myStyleToolbarLabel, myStyleHorizontallBox, myStyleToolbarSearchField });


                    TodoOptions(selectedToDoList, todoListIndex);

                    if (!EditorGUIUtility.isProSkin)
                        GUI.contentColor = Color.gray;
                    if (GUILayout.Button(addIcon, myStyleToolbarButton, GUILayout.MaxWidth(30))) //create new task
                    {
                        Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                        Task newTask = new Task();
                        DateTime currentTime = DateTime.Now;

                        newTask.creationTime = new TGSTime(currentTime);
                        newTask.creationTime.UpdateHourInTweleveHourFormat();


                        currentTime = currentTime.AddHours(newTaskIsDueInCurrentTimePlusHours);
                        newTask.dueDate = new TGSTime(currentTime);
                        newTask.dueDate.UpdateHourInTweleveHourFormat();



                        selectedTopic.toDoLists[todoListIndex].tasks.Add(newTask);
                        selectedTopic.Clicked();
                        newTask.editing = true;
                        EditorUtility.SetDirty(selectedTopic);
                    }
                    GUI.contentColor = originalContentColor;
                    //if (GUILayout.Button(deleteIcon, toolbarButtonStyle, GUILayout.MaxWidth(25)))
                    //{
                    //    Undo.RecordObject(selectedBoard, selectedBoard.name + " update");
                    //    selectedBoard.toDoLists.RemoveAt(i);
                    //    EditorUtility.SetDirty(selectedBoard);
                    //    modifiedList = true;
                    //    GUI.FocusControl(null);
                    //}
                }
            }
            GUILayout.EndHorizontal();
        }
        void ReorderToDoListButtons(Topic selectedBoard, int todoListIndex, ToDoList selectedToDoList)
        {
            if (!selectedToDoList.editing)
                return;

            if (todoListIndex != 0)
            {
                if (GUILayout.Button(upIcon, myStyleToolbarButton, GUILayout.Width(20)))
                {
                    Undo.RecordObject(selectedBoard, selectedBoard.name + " update");

                    selectedBoard.toDoLists.RemoveAt(todoListIndex);
                    selectedBoard.toDoLists.Insert(todoListIndex - 1, selectedToDoList);

                    EditorUtility.SetDirty(selectedBoard);

                    selectedToDoList = null;
                    requiresReSerialization = true;
                }
            }
            if (todoListIndex != selectedBoard.toDoLists.Count - 1)
            {
                if (GUILayout.Button(downIcon, myStyleToolbarButton, GUILayout.Width(20)))
                {
                    Undo.RecordObject(selectedBoard, selectedBoard.name + " update");

                    selectedBoard.toDoLists.RemoveAt(todoListIndex);
                    selectedBoard.toDoLists.Insert(todoListIndex + 1, selectedToDoList);

                    EditorUtility.SetDirty(selectedBoard);

                    selectedToDoList = null;
                    requiresReSerialization = true;
                }
            }
        }

        void ToDoListFolderIcon(ToDoList selectedToDoList)
        {
            if (!selectedTopic.hideFolderIcons)
            {
                Color originalContentColor = GUI.contentColor;
                if (!EditorGUIUtility.isProSkin)
                    GUI.contentColor = Color.gray;
                if (selectedToDoList.opened)
                {
                    if (GUILayout.Button(folderOpenedIcon, myStyleToolbarButton, GUILayout.Width(25)))
                    {
                        Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                        selectedToDoList.opened = false;
                        EditorUtility.SetDirty(selectedTopic);
                        GUI.FocusControl(null);
                    }
                }
                else
                {
                    if (GUILayout.Button(folderIcon, myStyleToolbarButton, GUILayout.Width(25)))
                    {
                        Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                        selectedToDoList.opened = true;
                        EditorUtility.SetDirty(selectedTopic);
                        GUI.FocusControl(null);
                    }
                }
                GUI.contentColor = originalContentColor;
            }
            else
            {
                GUILayout.Space(5);
            }
        }

        int currentList = 0;
        void ListOfTasks(int listIndex, float originalAlpha)
        {
            if (listIndex >= selectedTopic.toDoLists.Count)
                return;

            selectedTodoList = selectedTopic.toDoLists[listIndex];

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.Width(maxTaskIndexWidth));
            currentList = listIndex;
            if (reorderableListsOfTask.Count > listIndex)
                if (reorderableListsOfTask[listIndex] != null)
                    reorderableListsOfTask[listIndex].DoLayoutList();
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.MaxWidth(screenWidth));

            int visibilityIndex = 0;

            GUILayout.Space(5);
            for (int taskIndex = 0; taskIndex < selectedTodoList.tasks.Count; taskIndex++)
            {
                Task selectedTask = selectedTodoList.tasks[taskIndex];

                if (!BeingSearchedForInTopic(selectedTask)) continue;
                if (!BeingSearchedForInToDoList(selectedTodoList, selectedTask)) continue;
                if (FilteredOut(selectedTask)) continue;
                if (TagFilteredOut(selectedTask)) continue;

                visibilityIndex++;
                selectedTodoList.tasks[taskIndex].visibilityIndex = visibilityIndex;

                DrawTask(listIndex, originalAlpha, taskIndex, selectedTask);
            } //end of cards list for loop
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }


        void DrawTasksGUI(Rect rect, int taskIndex, bool isActive, bool isFocused)
        {
            if (selectedTopic.toDoLists.Count <= currentList)
                return;
            if (selectedTopic.toDoLists[currentList].tasks.Count <= taskIndex)
                return;
            if (serializedistsOfTask[currentList].arraySize <= taskIndex)
                return;


            Task currentTask = selectedTopic.toDoLists[currentList].tasks[taskIndex];

            if (!BeingSearchedForInTopic(currentTask)) return;
            if (!BeingSearchedForInToDoList(selectedTopic.toDoLists[currentList], currentTask)) return;
            if (FilteredOut(currentTask)) return;
            if (TagFilteredOut(currentTask)) return;





            //SerializedProperty element = serializedistsOfTask[currentList].GetArrayElementAtIndex(taskIndex);

            GUIContent numberContent;
            if (selectedTopic.showRealIndex)
                numberContent = new GUIContent(taskIndex.ToString());
            else
                numberContent = new GUIContent(currentTask.visibilityIndex.ToString());

            float requiredWidth = myStyleTaskLabel.CalcSize(numberContent).x + 5;
            if (requiredWidth > maxTaskIndexWidth) maxTaskIndexWidth = requiredWidth;

            EditorGUI.LabelField(new Rect(rect.x - 20, rect.y, maxTaskIndexWidth, EditorGUIUtility.singleLineHeight), numberContent, myStyleTaskNumbering);
        }

        bool BeingSearchedForInTopic(Task selectedTask)
        {
            if (!string.IsNullOrEmpty(selectedTopic.searchingFor))
                if (!selectedTask.myName.ToUpper().Contains(selectedTopic.searchingFor.ToUpper()) && !(selectedTask.addedDescription && selectedTask.myDescription.ToUpper().Contains(selectedTopic.searchingFor.ToUpper())))
                    return false;

            return true;
        }
        bool BeingSearchedForInToDoList(ToDoList selectedToDoList, Task selectedTask)
        {
            if (!string.IsNullOrEmpty(selectedToDoList.searchingFor))
                if (!selectedTask.myName.ToUpper().Contains(selectedToDoList.searchingFor.ToUpper()) && !(selectedTask.addedDescription && selectedTask.myDescription.ToUpper().Contains(selectedToDoList.searchingFor.ToUpper())))
                    return false;

            return true;
        }
        bool FilteredOut(Task selectedTask)
        {
            if (
                (!selectedTopic.showCompletedItems && selectedTask.completed) ||
                (!selectedTopic.showIgnoredItems && selectedTask.ignored) ||
                (!selectedTopic.showFailedTasks && selectedTask.failed) ||
                (!selectedTopic.showPendingItems && !selectedTask.completed && !selectedTask.failed && !selectedTask.ignored))
                return true;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="selectedTask"></param>
        /// <returns>True = ignored</returns>
        bool TagFilteredOut(Task selectedTask)
        {
            if (selectedTopic.tags.Count == 0) return false;

            if (selectedTopic.tagFilterType == Topic.TagFilterType.hideInactiveTags)
            {
                if (selectedTask.tags.Count == 0) return false;

                for (int i = 0; i < selectedTask.tags.Count; i++)
                {
                    if (!selectedTask.tags[i].enabled)
                        return true;
                }
                return false;
            }
            else //Only show active tags
            {
                if (selectedTask.tags.Count == 0) return true;

                for (int i = 0; i < selectedTask.tags.Count; i++)
                {
                    if (selectedTask.tags[i].enabled)
                        return false;
                }
                return true;
            }
        }


        //int collapseDueDateEditingOn = 500; //pixel
        float maxTaskIndexWidth = 0;
        void DrawTask(int listIndex, float originalAlpha, int taskIndex, Task task)
        {
            Color taskColor;

            if (task.completed)
            {
                if (selectedTopic.colorScheme)
                    taskColor = selectedTopic.colorScheme.completedTask;
                else
                    taskColor = new Color(0.9f, 1f, 0.9f, 1);
            }
            else if (task.failed)
            {
                if (selectedTopic.colorScheme)
                    taskColor = selectedTopic.colorScheme.failedTask;
                else
                    taskColor = new Color(1f, 0.8f, 0.8f, 1);
            }
            else if (task.ignored)
            {
                if (selectedTopic.colorScheme)
                    taskColor = selectedTopic.colorScheme.ignoredTask;
                else
                    taskColor = new Color(1f, 1, 1, 0.7f);
            }
            else
            {
                if (selectedTopic.colorScheme)
                    taskColor = selectedTopic.colorScheme.inprogressTask;
                else
                    taskColor = new Color(1f, 1f, 1f, 1);
            }


            GUI.color = new Color(taskColor.r, taskColor.g, taskColor.b, taskColor.a * originalAlpha);


            if (taskIndex % 2 == 0)
                GUILayout.BeginHorizontal(myStyleTaskOne);
            else
                GUILayout.BeginHorizontal(myStyleTaskTwo);


            GUILayout.BeginVertical();


            GUILayout.BeginHorizontal();

            //if (EditorGUIUtility.currentViewWidth > 450)
            //{
            //    GUIContent numberContent = new GUIContent(taskIndex.ToString());
            //    float requiredWidth = myStyleTaskLabel.CalcSize(numberContent).x + 5;
            //    if (requiredWidth > maxTaskIndexWidth) maxTaskIndexWidth = requiredWidth;
            //    GUILayout.BeginHorizontal(myStyleTaskNumbering, GUILayout.Width(maxTaskIndexWidth));
            //    GUILayout.Label(taskIndex.ToString(), myStyleTaskLabel, GUILayout.Width(maxTaskIndexWidth));
            //    GUILayout.EndHorizontal();
            //}
            //GUILayout.Label(taskIndex.ToString(), myStyleTaskNumbering, GUILayout.Width(20));
            if (!selectedTopic.hideFolderIcons)
                TaskFolderControl(task);

            TaskStateIcon(task);



            TaskName(selectedTopic.toDoLists[listIndex], task);

            if (!task.editing)
                TaskDueDateLabel(task);


            bool removed = TaskButtons(selectedTopic.toDoLists[listIndex], task); //was the task removed by the remove task button
            GUILayout.EndHorizontal();

            if (!removed)
            {
                if (task.editing)
                    DrawTaskDueDateEdit(task);

                if (selectedTopic.showTaskDetails && !task.hideMyDetails)
                {
                    if (task.addedDescription)
                        TaskDescription(task);

                    if (task.addedReference)
                        TaskReferences(originalAlpha, task);
                }
            }



            GUILayout.EndVertical(); //end of task box
            GUILayout.EndHorizontal(); //end of task + numbering

            float newHeight = GUILayoutUtility.GetLastRect().height;
            newHeight -= 2;
            if (task.heightTakenInEditor != newHeight && newHeight > 5)
            {
                task.heightTakenInEditor = newHeight;
                EditorUtility.SetDirty(selectedTopic);
                requiresReSerialization = true;
            }
        }

        void DrawTaskDueDateEdit(Task task, bool flexibleIndent = false)
        {
            if (task.dueDate != null && !task.completed && !task.failed)
            {
                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                if (!flexibleIndent)
                {
                    if (EditorGUIUtility.currentViewWidth > 500)
                        GUILayout.Space(indentForTaskSubOptions);
                    else if (EditorGUIUtility.currentViewWidth > 200)
                        GUILayout.Space(15);
                }
                else
                {
                    GUILayout.FlexibleSpace();
                }




                Color bg = GUI.backgroundColor;
                Color c = Color.white;
                c.a = bg.a * 1.5f;
                GUI.backgroundColor = c;








                bool hasTargetTime = EditorGUILayout.Toggle(task.hasDueDate, GUILayout.Width(15));
                GUI.backgroundColor = bg;
                if (hasTargetTime != task.hasDueDate)
                {
                    Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                    task.hasDueDate = hasTargetTime;
                    EditorUtility.SetDirty(selectedTopic);
                    GUI.FocusControl(null);
                }
                GUILayout.Label(new GUIContent("Due date ", "Day/Month/Year"), myStyleToolbarLabel, GUILayout.Width(55));
                TaskDueDateEditing(task);
                GUILayout.Space(4);
                GUILayout.Label(new GUIContent("at ", "Day/Month/Year"), myStyleToolbarLabel, GUILayout.Width(15));
                TaskDueTime(task);
                GUI.enabled = true;

                if (!flexibleIndent)
                    GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();
            }
        }

        void TaskDoneButton(Task task)
        {
            if (GUILayout.Button("Done", myStyleToolbarButton, GUILayout.Width(50)))
            {
                Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                selectedTopic.Clicked();
                task.editing = false;
                EditorUtility.SetDirty(selectedTopic);
                GUI.FocusControl(null);
            }
        }

        Color dueALotLater = new Color(1, 1, 1, 0.75f);
        Color dueNotYet = new Color(1, 1, 1, 0.85f);
        Color dueDatePassedColor = new Color(1, 0.5f, 0.5f, 1f);
        void TaskDueDateLabel(Task task)
        {
            if (!task.hasDueDate || task.completed || task.ignored || task.failed)
                return;


            Color originalContentColor = GUI.contentColor;
            Color newColor = dueNotYet;
            if (task.dueDate.TimeIsToday(DateTime.Now))
            {
                if (task.dueDate.NotTimeYet())
                    newColor = myColorYellow;
                else
                    newColor = dueDatePassedColor;
            }
            else
            {
                int daysTil = task.dueDate.DaysFromCurrentTime();
                if (daysTil < 0)
                    newColor = dueDatePassedColor;
                else if (daysTil > 15)
                    newColor = dueALotLater;
                else if (daysTil > 5)
                    newColor = dueNotYet;
            }
            GUI.contentColor = newColor;

            string tooltip = "Due on " + task.dueDate.GetFullTime() + "\n" + "Created on " + task.creationTime.GetFullTime();

            float width = EditorGUIUtility.currentViewWidth;
            GUILayout.FlexibleSpace();
            if ((width > 700 && filterListAnimBool.target) || (width > 600 && !filterListAnimBool.target))
            {
                GUIContent targetTime = new GUIContent(task.dueDate.GetDueTime(), tooltip);
                EditorGUILayout.LabelField(targetTime, myStyleDueDateLabel, GUILayout.MaxWidth(200));
            }
            else if (width > 400)
            {
                GUIContent targetTime = new GUIContent(task.dueDate.GetShortDueTime(), tooltip);
                EditorGUILayout.LabelField(targetTime, myStyleDueDateLabel, GUILayout.MaxWidth(myStyleDueDateLabel.CalcSize(new GUIContent(targetTime)).x + 5));
            }
            GUILayout.Space(10);
            GUI.contentColor = originalContentColor;
        }

        void TaskDueDateEditing(Task task)
        {
            GUILayout.BeginHorizontal();

            if (!task.hasDueDate)
                GUI.enabled = false;

            int day = EditorGUILayout.IntField("", task.dueDate.day, myStyleToolbarInputField, GUILayout.MaxWidth(20));
            if (day != task.dueDate.day)
            {
                Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                task.dueDate.day = day;
                task.dueDate.VerifyValues();
                task.dueDate.UpdateDayOfTheWeek();
                EditorUtility.SetDirty(selectedTopic);
            }

            GUILayout.Label("/", myStyleToolbarLabel, GUILayout.Width(7));

            int month = EditorGUILayout.IntField("", task.dueDate.month, myStyleToolbarInputField, GUILayout.MaxWidth(20));
            if (month != task.dueDate.month)
            {
                Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                task.dueDate.month = month;
                task.dueDate.VerifyValues();
                task.dueDate.UpdateDayOfTheWeek();
                EditorUtility.SetDirty(selectedTopic);
            }


            GUILayout.Label("/", myStyleToolbarLabel, GUILayout.Width(7));

            int year = EditorGUILayout.IntField("", task.dueDate.year, myStyleToolbarInputField, GUILayout.MaxWidth(37));
            int currentYear = DateTime.Now.Year;
            if (year < currentYear) year = currentYear;
            if (year != task.dueDate.year)
            {
                Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                task.dueDate.year = year;
                task.dueDate.VerifyValues();
                task.dueDate.UpdateDayOfTheWeek();
                EditorUtility.SetDirty(selectedTopic);
            }

            GUILayout.EndHorizontal();
        }
        void TaskDueTime(Task task)
        {
            int hour = EditorGUILayout.IntField("", task.dueDate.HourInTweleveHourFormat.hour, myStyleToolbarInputField, GUILayout.MaxWidth(25));
            if (hour != task.dueDate.HourInTweleveHourFormat.hour)
            {
                Undo.RecordObject(selectedTopic, selectedTopic.name + " update");

                if (hour < 0)
                    hour = 0;
                else if (hour > 12)
                    hour = 12;

                task.dueDate.HourInTweleveHourFormat.hour = hour;

                if (task.dueDate.HourInTweleveHourFormat.format == TGSTime.TimeFormat.AM)
                {
                    if (task.dueDate.HourInTweleveHourFormat.hour == 12)
                        task.dueDate.hour = 0;
                    else task.dueDate.hour = task.dueDate.HourInTweleveHourFormat.hour;
                }
                else if (task.dueDate.HourInTweleveHourFormat.format == TGSTime.TimeFormat.PM)
                {
                    if (task.dueDate.HourInTweleveHourFormat.hour == 12)
                        task.dueDate.hour = 12;
                    else task.dueDate.hour = hour + 12;
                }

                EditorUtility.SetDirty(selectedTopic);
            }

            GUILayout.Label(new GUIContent(":", ""), myStyleToolbarLabel, GUILayout.Width(7));

            int minute = EditorGUILayout.IntField("", task.dueDate.minute, myStyleToolbarInputField, GUILayout.MaxWidth(25));
            if (minute < 0) minute = 0;
            if (minute > 59) minute = 59;
            if (minute != task.dueDate.minute)
            {
                Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                task.dueDate.minute = minute;
                EditorUtility.SetDirty(selectedTopic);
            }

            TGSTime.TimeFormat timeFormat = (TGSTime.TimeFormat)EditorGUILayout.EnumPopup(task.dueDate.HourInTweleveHourFormat.format, EditorStyles.toolbarDropDown, GUILayout.MaxWidth(37));
            if (timeFormat != task.dueDate.HourInTweleveHourFormat.format)
            {
                Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                task.dueDate.HourInTweleveHourFormat.format = timeFormat;

                if (task.dueDate.HourInTweleveHourFormat.format == TGSTime.TimeFormat.AM)
                {
                    if (task.dueDate.HourInTweleveHourFormat.hour == 12)
                        task.dueDate.hour = 0;
                    else task.dueDate.hour = task.dueDate.HourInTweleveHourFormat.hour;
                }
                else if (task.dueDate.HourInTweleveHourFormat.format == TGSTime.TimeFormat.PM)
                {
                    if (task.dueDate.HourInTweleveHourFormat.hour == 12)
                        task.dueDate.hour = 12;
                    else task.dueDate.hour = hour + 12;
                }

                EditorUtility.SetDirty(selectedTopic);
            }
        }



        void TaskReferences(float originalAlpha, Task task, bool indent = true, bool drawLabel = true)
        {
            GUILayout.BeginHorizontal();
            if (indent)
            {
                GUILayout.Space(indentForTaskSubOptions);
            }
            GUI.color = new Color(1, 1, 1, originalAlpha * 0.85f);




            bool needsToBeClosed = false;
            int indexInRow = 0;
            GUILayout.BeginVertical();

            if (task.editing && drawLabel)
            {
                GUILayout.Space(3);
                GUILayout.BeginHorizontal();
                GUILayout.Label("References", myStyleLabel, GUILayout.Width(90));
                if (GUILayout.Button(addIcon, myStyleIcon, GUILayout.Width(25)))
                {
                    Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                    task.references.Add(null);
                    EditorUtility.SetDirty(selectedTopic);
                }
                GUILayout.EndHorizontal();
            }

            for (int k = 0; k < task.references.Count; k++)
            {
                if (indexInRow == 0)
                {
                    GUILayout.BeginHorizontal();
                    needsToBeClosed = true;
                }
                DrawReference(task, k);
                indexInRow++;
                if (indexInRow >= maximumReferenceInOneLine)
                {
                    GUILayout.EndHorizontal();
                    needsToBeClosed = false;
                    indexInRow = 0;
                }
            }



            if (needsToBeClosed)
            {
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if (task.references.Count > 0)
                GUILayout.Space(10);
            GUI.color = new Color(1, 1, 1, originalAlpha);
        }

        void TaskDescription(Task task, int indent = 500)
        {
            if (indent == 500)
                indent = indentForTaskSubOptions;
            GUILayout.Space(4);
            if (task.editing)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(indent); //indent
                string description = EditorGUILayout.TextArea(task.myDescription, myStyleInputField, GUILayout.Height(50)); //todo
                GUILayout.EndHorizontal();
                if (description != task.myDescription)
                {
                    Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                    task.myDescription = description;
                    EditorUtility.SetDirty(selectedTopic);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(task.myDescription))
                {
                    Color original = GUI.color;
                    float originalAlpha = original.a;
                    original.a *= 0.75f;
                    GUI.color = original;
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(indent); //indent
                    EditorGUILayout.LabelField(task.myDescription, myStyleLabel);
                    if (indent >= 0)
                        GUILayout.Space(indentRight);
                    GUILayout.EndHorizontal();

                    original.a = originalAlpha;
                    GUI.color = original;
                }
            }
            GUILayout.Space(6);
        }

        bool TaskButtons(ToDoList todoList, Task task, bool forceTextLabels = false, bool hideIgnoreButton = false, bool hideOptions = false, bool showDoneButton = true)
        {

            Color originalBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1, 1, 1, GUI.backgroundColor.a);
            Color guiColor = GUI.color;
            GUI.color = Color.white;

            GUILayout.BeginHorizontal(EditorStyles.toolbar); //this forces one line

            if (task.editing && showDoneButton)
                TaskDoneButton(task);

            if (!hideOptions)
                TaskOptions(todoList, task);
            GUI.backgroundColor = originalBackgroundColor;

            CompleteTaskButton(task, forceTextLabels);

            FailedTaskButton(task, forceTextLabels);

            if (!hideIgnoreButton)
                IgnoreTaskButton(task);

            bool removed = false;
            Color contentColor = GUI.contentColor;
            GUIContent removeButtonContent;
            int removeButtonContentMaxWidth;
            if (!forceTextLabels && ((EditorGUIUtility.currentViewWidth <= 600 && filterListAnimBool.faded == 0) || (EditorGUIUtility.currentViewWidth <= 800 && filterListAnimBool.faded != 0) || selectedTopic.alwaysShowIconsOnly))
            {
                removeButtonContent = new GUIContent(trashIcon);
                removeButtonContentMaxWidth = 25;
            }
            else
            {
                removeButtonContent = new GUIContent("Remove");
                removeButtonContentMaxWidth = 75;
            }
            removeButtonContent.tooltip = "Remove this task permanently.";
            GUI.contentColor = new Color(1, 1, 1, contentColor.a);
            GUI.color = new Color(1, 1, 1, guiColor.a);
            if (GUILayout.Button(removeButtonContent, myStyleToolbarButton, GUILayout.MaxWidth(removeButtonContentMaxWidth)))
            {
                if (!taskAndToDoDeleteConfirmation)
                {
                    removed = DeleteTask(todoList, task);
                }
                else
                {
                    if (EditorUtility.DisplayDialog("Delete " + task.myName + "?", //dialog title
                        "Are you sure you want to delete this task?", //dialog description
                        "Delete", "Do Not Delete"))
                    {
                        removed = DeleteTask(todoList, task);
                    }
                }
            }
            GUI.contentColor = contentColor;
            GUI.color = guiColor;
            GUILayout.EndHorizontal();

            return removed;
        }

        private bool DeleteTask(ToDoList todoList, Task task)
        {
            bool removed = true;
            Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
            todoList.tasks.Remove(task);
            EditorUtility.SetDirty(selectedTopic);
            GUI.FocusControl(null);

            requiresReSerialization = true;
            return removed;
        }

        void TaskFolderControl(Task task)
        {
            if (selectedTopic.showTaskDetails)
            {
                if ((task.addedDescription && !string.IsNullOrEmpty(task.myDescription)) || (task.addedReference && task.references.Count > 0))
                {
                    if (!task.hideMyDetails)
                    {
                        if (GUILayout.Button(folderOpenedIcon, myStyleIcon, GUILayout.Width(18), GUILayout.Height(18)))
                        {
                            Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                            task.hideMyDetails = true;
                            EditorUtility.SetDirty(selectedTopic);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button(folderIcon, myStyleIcon, GUILayout.Width(18), GUILayout.Height(18)))
                        {
                            Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                            task.hideMyDetails = false;
                            EditorUtility.SetDirty(selectedTopic);
                        }
                    }
                }
                else
                {
                    if ((EditorGUIUtility.currentViewWidth > 500 && filterListAnimBool.faded == 0) || (EditorGUIUtility.currentViewWidth > 700 && filterListAnimBool.faded != 0))
                        GUILayout.Space(21);
                }
            }
        }

        void TaskStateIcon(Task task)
        {
            Color originalContentColor = GUI.contentColor;

            if (task.completed)
            {
                GUIContent correctContent = new GUIContent(correctIcon, "Completed on:\n" + task.completionTime.GetFullTime() + "\n\nCreated on:\n" + task.creationTime.GetFullTime());
                GUILayout.Label(correctContent, myStyleIcon, GUILayout.Height(20), GUILayout.Width(20));
            }
            else if (task.failed)
            {
                GUIContent failTime = new GUIContent(incorrectIcon);
                failTime.tooltip = "Failed on:\n" + task.failedTime.GetFullTime() + "\n\nCreated on:\n" + task.creationTime.GetFullTime();
                GUILayout.Label(failTime, myStyleIcon, GUILayout.Height(20), GUILayout.Width(20));
            }
            else if (task.ignored)
            {
                if (!EditorGUIUtility.isProSkin)
                    GUI.contentColor = Color.gray;

                GUIContent ignoreContent = new GUIContent(ignoreIcon);
                ignoreContent.tooltip = "Ignored on:\n" + task.ignoredTime.GetFullTime() + "\n\nCreated on:\n" + task.creationTime.GetFullTime();
                GUILayout.Label(ignoreContent, myStyleIcon, GUILayout.Height(20), GUILayout.Width(20));
            }
            GUI.contentColor = originalContentColor;
        }

        void TaskName(ToDoList toDoList, Task task, bool showEditButton = true)
        {
            if (task.editing)
            {
                if (showEditButton)
                {
                    GUIContent editButtonContent;
                    if (clickingOnTaskEditsIt)
                        editButtonContent = EditorGUIUtility.IconContent("d_UnityEditor.ConsoleWindow");
                    else
                        editButtonContent = new GUIContent(editIcon);

                    if (GUILayout.Button(editButtonContent, myStyleIcon, GUILayout.Height(15), GUILayout.Width(15)))
                    {
                        if (!clickingOnTaskEditsIt)
                            StopEditingTask(task);
                        else
                            OpenTaskPage(toDoList, task);
                    }
                }

                //var proEditor = Type.GetType("TinyGiantStudio.ModularToDoList.ModularToDoListProEditor");
                proEditor?.GetMethod("DrawTags").Invoke(null, new object[] { selectedTopic, task, myStyleTags });

                //string cardName = EditorGUILayout.TextField(string.Empty, task.myName, myStyleInputField, GUILayout.MinWidth(50));
                string taskName = EditorGUILayout.TextArea(task.myName, myStyleInputField, GUILayout.MinWidth(50));
                if (taskName != task.myName)
                {
                    Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                    task.myName = taskName;
                    EditorUtility.SetDirty(selectedTopic);
                }
            }
            else
            {
                Color original = GUI.color;
                GUI.color = new Color(1, 1, 0f, original.a * 0.6f);
                if (showEditButton && !editLocked)
                {
                    GUIContent editButtonContent;
                    if (clickingOnTaskEditsIt)
                        editButtonContent = EditorGUIUtility.IconContent("d_UnityEditor.ConsoleWindow");
                    else
                        editButtonContent = new GUIContent(editIcon);

                    if (GUILayout.Button(editButtonContent, myStyleIcon, GUILayout.Height(15), GUILayout.Width(15)))
                    {
                        if (!clickingOnTaskEditsIt)
                            EditTask(task);
                        else
                            OpenTaskPage(toDoList, task);
                    }
                }
                GUI.color = original;

                //var proEditor = Type.GetType("TinyGiantStudio.ModularToDoList.ModularToDoListProEditor");
                proEditor?.GetMethod("DrawTags").Invoke(null, new object[] { selectedTopic, task, myStyleTags });

                string tooltip = "";
                if (task.completed)
                    tooltip += "Completed on " + task.completionTime.GetFullTime() + "\n";
                else if (task.failed)
                    tooltip += "Failed on " + task.failedTime.GetFullTime() + "\n";
                else if (task.ignored)
                    tooltip += "Ignored on " + task.ignoredTime.GetFullTime() + "\n";
                else if (task.hasDueDate)
                    tooltip += "Due on " + task.dueDate.GetFullTime() + "\n";

                tooltip += "Created on " + task.creationTime.GetFullTime();

                if (GUILayout.Button(new GUIContent(task.myName, tooltip), myStyleTaskLabel))
                {
                    if (clickingOnTaskEditsIt)
                        EditTask(task);
                    else
                        OpenTaskPage(toDoList, task);
                }
                //EditorGUILayout.LabelField(new GUIContent(task.myName, tooltip), myStyleTaskLabel);
            }
        }

        private void StopEditingTask(Task task)
        {
            Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
            selectedTopic.Clicked();
            task.editing = false;
            EditorUtility.SetDirty(selectedTopic);
            GUI.FocusControl(null);

            requiresReSerialization = true;
        }

        private void OpenTaskPage(ToDoList toDoList, Task task)
        {
            selectedTask = task;
            selectedTodoList = toDoList;
            page_taskAnimBool.value = false;
            currentPage = Page.task;
        }

        private void EditTask(Task task)
        {
            Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
            selectedTopic.Clicked();
            task.editing = true;
            if (!task.hasDueDate)
                task.dueDate = new TGSTime(DateTime.Now);
            EditorUtility.SetDirty(selectedTopic);
            GUI.FocusControl(null);

            requiresReSerialization = true;
        }

        void DrawReference(Task task, int k)
        {
            bool removed = false;

            GUILayout.BeginVertical();
            //GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.BeginHorizontal();
            var newobject = EditorGUILayout.ObjectField(task.references[k], typeof(UnityEngine.Object), false, GUILayout.MaxWidth(screenWidth));
            if (newobject != task.references[k])
            {
                Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                task.references[k] = (UnityEngine.Object)newobject;
                EditorUtility.SetDirty(selectedTopic);
            }
            if (task.editing)
            {
                if (GUILayout.Button(new GUIContent(trashIcon), myStyleToolbarButton, GUILayout.Width(25)))
                {
                    Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                    task.references.Remove(task.references[k]);
                    EditorUtility.SetDirty(selectedTopic);
                    removed = true;
                }
            }
            GUILayout.EndHorizontal();


            if (!removed)
            {
                if (task.references[k])
                {
                    var preview = AssetPreview.GetAssetPreview(task.references[k]);
                    if (preview != null)
                    {
                        int targetPreviewWidth = 100;
                        if (preview.width < 100)
                            targetPreviewWidth = preview.width;
                        int targetPreviewHeight = 100;
                        if (preview.height < 100)
                            targetPreviewHeight = preview.height;
                        GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(targetPreviewWidth), GUILayout.Height(targetPreviewHeight));
                        GUILayout.Box(preview, GUIStyle.none, GUILayout.MaxWidth(targetPreviewWidth), GUILayout.MaxHeight(targetPreviewHeight));
                        GUILayout.EndVertical();
                    }
                }
            }

            GUILayout.EndVertical();
        }

        void ToDoListDescription(ToDoList selectedToDoList)
        {
            if (selectedToDoList.addedDescription)
            {
                GUILayout.BeginHorizontal("Box");
                GUILayout.Space(indentLeft + 20);
                if (selectedToDoList.editing)
                {
                    //string description = EditorGUILayout.TextArea(selectedToDoList.myDescription, GUILayout.Height(50), GUILayout.MaxWidth(1500));
                    string description = GUILayout.TextArea(selectedToDoList.myDescription, myStyleInputField);
                    if (description != selectedToDoList.myDescription)
                    {
                        Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                        selectedToDoList.myDescription = description;
                        EditorUtility.SetDirty(selectedTopic);
                    }
                }
                else if (selectedToDoList.myDescription.Length > 0)
                {
                    EditorGUILayout.LabelField(selectedToDoList.myDescription, myStyleLabel);
                }
                GUILayout.EndHorizontal();
            }
        }



        readonly string todoListOptionsTextDelete = "Remove this list";
        readonly string todoListOptionsTextDuplicate = "Duplicate list";
        void TodoOptions(ToDoList toDoList, int i)
        {
            List<string> optionsList = new List<string>();
            optionsList.Add("Cancel");
            if (!toDoList.addedDescription)
                optionsList.Add("Add description");
            else
                optionsList.Add("Remove description");

            optionsList.Add(todoListOptionsTextDelete);
            optionsList.Add(todoListOptionsTextDuplicate);

            if (optionsList.Count > 1)
            {
                Color guiBackground = GUI.backgroundColor;
                GUI.backgroundColor = Color.white;
                int index = EditorGUILayout.Popup(0, optionsList.ToArray(), EditorStyles.toolbarDropDown, GUILayout.MaxWidth(18));
                GUI.backgroundColor = guiBackground;

                if (index != 0)
                {
                    if (optionsList[index] == "Add description")
                    {
                        Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                        selectedTopic.showTaskDetails = true;
                        toDoList.addedDescription = true;
                        toDoList.editing = true;
                        EditorUtility.SetDirty(selectedTopic);
                    }
                    else if (optionsList[index] == "Remove description")
                    {
                        Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                        toDoList.addedDescription = false;
                        toDoList.editing = false;
                        EditorUtility.SetDirty(selectedTopic);
                    }
                    else if (optionsList[index] == todoListOptionsTextDelete)
                    {
                        if (!taskAndToDoDeleteConfirmation)
                            RemoveList(i);
                        else
                             if (EditorUtility.DisplayDialog("Delete " + toDoList.myName, //dialog title
                        "Are you sure you want to delete this list?", //dialog description
                        "Delete", "Do Not Delete"))
                        {
                            RemoveList(i);
                        }
                    }
                    else if (optionsList[index] == todoListOptionsTextDuplicate)
                    {
                        Undo.RecordObject(selectedTopic, selectedTopic.name + " update");

                        ToDoList newList = new ToDoList(toDoList);
                        selectedTopic.toDoLists.Add(newList);

                        EditorUtility.SetDirty(selectedTopic);
                        GUI.FocusControl(null);

                        //this is used because "cloning" the todolist is a reference to the original, so, any changes made to this one also changes original
                        //this makes the asset lose reference
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(selectedTopic));
                    }
                }
            }
        }

        private void RemoveList(int i)
        {
            Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
            selectedTopic.toDoLists.RemoveAt(i);
            EditorUtility.SetDirty(selectedTopic);
            GUI.FocusControl(null);
        }

        string referenceAddText = "Add reference";
        /// <summary>
        /// Move to board is calculated by decreasing index by the number of non move related options before it.
        /// Which is currently 4: cancel, description, reference.
        /// </summary>
        /// <param name="toDoList"></param>
        /// <param name="task"></param>
        void TaskOptions(ToDoList toDoList, Task task)
        {
            List<string> optionsList = new List<string>();
            optionsList.Add("Cancel");

            if (!task.addedDescription)
                optionsList.Add("Add description");
            else
                optionsList.Add("Remove description");

            if (!task.addedReference)
                optionsList.Add(referenceAddText);
            else
                optionsList.Add("Remove reference");

            optionsList.Add("/");

            for (int i = 0; i < selectedTopic.toDoLists.Count; i++)
            {
                if (toDoList != selectedTopic.toDoLists[i])
                    optionsList.Add("Move to " + selectedTopic.toDoLists[i].myName);
            }

            optionsList.Add("/");

            //proEditor?.GetMethod("TaskTagOptionsCreate").Invoke(null, new object[] { selectedTopic, task, myStyleTags });

            for (int i = task.tags.Count - 1; i >= 0; i--)
            {
                if (task.tags[i] == null)
                    task.tags.RemoveAt(i);
            }

            for (int i = 0; i < task.tags.Count; i++)
            {
                optionsList.Add("Tag/Remove/" + task.tags[i].myName);
            }
            for (int i = 0; i < selectedTopic.tags.Count; i++)
            {
                if (!task.tags.Contains(selectedTopic.tags[i])) //check if already contains
                    optionsList.Add("Tag/Add/" + selectedTopic.tags[i].myName);
            }

            if (optionsList.Count > 1)
            {
                GUI.backgroundColor = Color.white;
                int index = EditorGUILayout.Popup(0, optionsList.ToArray(), EditorStyles.toolbarDropDown, GUILayout.MaxWidth(18));
                if (index != 0)
                {
                    if (optionsList[index] == "Add description")
                    {
                        Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                        selectedTopic.showTaskDetails = true;
                        task.addedDescription = true;
                        task.hideMyDetails = false;
                        selectedTopic.Clicked();
                        task.editing = true;
                        EditorUtility.SetDirty(selectedTopic);
                    }
                    else if (optionsList[index] == "Remove description")
                    {
                        Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                        task.addedDescription = false;
                        selectedTopic.Clicked();
                        EditorUtility.SetDirty(selectedTopic);
                    }
                    else if (optionsList[index] == referenceAddText) //these bools need to be set when importing references too
                    {
                        Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                        selectedTopic.showTaskDetails = true;
                        task.addedReference = true;
                        task.hideMyDetails = false;
                        selectedTopic.Clicked();
                        task.editing = true;
                        EditorUtility.SetDirty(selectedTopic);
                    }
                    else if (optionsList[index] == "Remove reference")
                    {
                        Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                        task.addedReference = false;
                        selectedTopic.Clicked();
                        EditorUtility.SetDirty(selectedTopic);
                    }
                    else if (optionsList[index].Contains("Move to"))
                    {
                        int targetIndex = index - 4; //4 is the number of items that isn't move to
                        bool increaseIt = false;
                        for (int i = 0; i <= targetIndex; i++)
                        {
                            if (toDoList == selectedTopic.toDoLists[i])
                                increaseIt = true;
                        }
                        if (increaseIt) targetIndex++;

                        if (targetIndex >= 0 && selectedTopic.toDoLists.Count > targetIndex)
                        {
                            Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                            selectedTopic.toDoLists[targetIndex].tasks.Add(task);
                            toDoList.tasks.Remove(task);
                            selectedTopic.Clicked();
                            EditorUtility.SetDirty(selectedTopic);
                        }
                    }
                    else if (optionsList[index].Contains("Tag/Remove/"))
                    {
                        //4 is the number of items that isn't move to
                        int targetIndex = index - 4 - selectedTopic.toDoLists.Count;

                        //Debug.Log("index:" + index + " targetIndex:" + targetIndex);

                        if (targetIndex >= 0 && task.tags.Count > targetIndex)
                        {
                            Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                            task.tags.RemoveAt(targetIndex);
                            selectedTopic.Clicked();
                            EditorUtility.SetDirty(selectedTopic);
                        }
                    }
                    else if (optionsList[index].Contains("Tag/Add/"))
                    {
                        //4 is the number of items that isn't move to
                        int targetIndex = index - 4 - selectedTopic.toDoLists.Count - task.tags.Count;

                        bool increaseIt = false;
                        for (int i = 0; i <= targetIndex; i++)
                        {
                            if (task.tags.Contains(selectedTopic.tags[i])) //if already contains tag
                                increaseIt = true;
                        }
                        if (increaseIt) targetIndex++;

                        //Debug.Log("index:" + index + " targetIndex:" + targetIndex);
                        if (targetIndex >= 0 && selectedTopic.tags.Count > targetIndex)
                        {
                            Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                            task.tags.Add(selectedTopic.tags[targetIndex]);
                            //selectedTopic.tas[targetIndex].tasks.Add(task);
                            selectedTopic.Clicked();
                            EditorUtility.SetDirty(selectedTopic);
                        }
                    }
                    //index = 0;
                }
            }
        }

        float minimumViewSizeForIconsWithFilterClosed = 600;
        void IgnoreTaskButton(Task task)
        {
            if (!task.completed && !task.failed)
            {
                Color contentColor = GUI.contentColor;
                Color guiColor = GUI.color;
                if (!task.ignored)
                {
                    GUIContent ignoredContent;
                    int ignoredContentMaxWidth;
                    if ((EditorGUIUtility.currentViewWidth <= minimumViewSizeForIconsWithFilterClosed && filterListAnimBool.faded == 0) || (EditorGUIUtility.currentViewWidth <= 800 && filterListAnimBool.faded != 0) || selectedTopic.alwaysShowIconsOnly)
                    {
                        ignoredContent = new GUIContent(ignoreIcon);
                        ignoredContentMaxWidth = 25;
                    }
                    else
                    {
                        ignoredContent = new GUIContent("Ignore");
                        ignoredContentMaxWidth = 50;
                    }
                    ignoredContent.tooltip = "Ignore this task.\n" +
                        "Ignored tasks can be hidden from task list temporarily.";

                    if (EditorGUIUtility.isProSkin)
                        GUI.contentColor = new Color(1, 1, 1, contentColor.a * 0.85f);
                    else
                        GUI.contentColor = new Color(0, 0, 0, contentColor.a * 0.55f);
                    GUI.color = new Color(1, 1, 1, guiColor.a * 0.85f);

                    if (GUILayout.Button(ignoredContent, myStyleToolbarButton, GUILayout.MaxWidth(ignoredContentMaxWidth)))
                    {
                        IgnoreTaskButtonEvent(task);
                    }
                }
                else
                {
                    GUIContent ignoredContent;
                    int ignoredContentMaxWidth;
                    if ((EditorGUIUtility.currentViewWidth <= minimumViewSizeForIconsWithFilterClosed && filterListAnimBool.faded == 0) || (EditorGUIUtility.currentViewWidth <= 800 && filterListAnimBool.faded != 0 || selectedTopic.alwaysShowIconsOnly))
                    {
                        ignoredContent = new GUIContent(clockIcon);
                        ignoredContentMaxWidth = 25;
                    }
                    else
                    {
                        ignoredContent = new GUIContent("Don't ignore");
                        ignoredContentMaxWidth = 90;
                    }
                    ignoredContent.tooltip = "No longer ignore this task.";

                    if (EditorGUIUtility.isProSkin)
                        GUI.contentColor = new Color(0.75f, 1, 0.75f, contentColor.a);
                    else
                        GUI.contentColor = new Color(0, 0, 0, contentColor.a * 0.55f);
                    GUI.color = new Color(0.9f, 1, 0.9f, guiColor.a);

                    if (GUILayout.Button(ignoredContent, myStyleToolbarButton, GUILayout.MaxWidth(ignoredContentMaxWidth)))
                    {
                        Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                        task.ignored = false;
                        selectedTopic.Clicked();
                        EditorUtility.SetDirty(selectedTopic);
                        GUI.FocusControl(null);
                    }
                }
                GUI.contentColor = contentColor;
                GUI.color = guiColor;
            }
        }

        void IgnoreTaskButtonEvent(Task task)
        {
            Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
            task.ignored = true;
            task.ignoredTime = new TGSTime(DateTime.Now);
            selectedTopic.Clicked();
            EditorUtility.SetDirty(selectedTopic);
            GUI.FocusControl(null);
        }

        void FailedTaskButton(Task task, bool forceTextLabels)
        {
            if (!task.completed && !task.ignored)
            {
                Color contentColor = GUI.contentColor;
                Color guiColor = GUI.color;

                if (!task.failed)
                {
                    GUIContent failedContent;
                    int failedContentMaxWidth;
                    if (!forceTextLabels && ((EditorGUIUtility.currentViewWidth <= minimumViewSizeForIconsWithFilterClosed && filterListAnimBool.faded == 0) || (EditorGUIUtility.currentViewWidth <= 800 && filterListAnimBool.faded != 0) || selectedTopic.alwaysShowIconsOnly))
                    {
                        failedContent = new GUIContent(incorrectIcon);
                        failedContentMaxWidth = 25;
                    }
                    else
                    {
                        failedContent = new GUIContent("Fail");
                        failedContentMaxWidth = 40;
                    }
                    failedContent.tooltip = "This task has failed.";

                    GUI.contentColor = new Color(1, 0.75f, 0.75f, contentColor.a);
                    GUI.color = new Color(1, 0.95f, 0.95f, guiColor.a);

                    if (GUILayout.Button(failedContent, myStyleToolbarButton, GUILayout.MaxWidth(failedContentMaxWidth)))
                    {
                        Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                        task.completed = false;
                        task.failed = true;
                        task.failedTime = new TGSTime(DateTime.Now);
                        selectedTopic.Clicked();
                        EditorUtility.SetDirty(selectedTopic);
                        GUI.FocusControl(null);
                    }
                }
                else
                {
                    GUIContent failedContent;
                    int failedContentMaxWidth;
                    if (!forceTextLabels && ((EditorGUIUtility.currentViewWidth <= minimumViewSizeForIconsWithFilterClosed && filterListAnimBool.faded == 0) || (EditorGUIUtility.currentViewWidth <= 800 && filterListAnimBool.faded != 0) || selectedTopic.alwaysShowIconsOnly))
                    {
                        failedContent = new GUIContent(clockIcon);
                        failedContentMaxWidth = 25;
                    }
                    else
                    {
                        failedContent = new GUIContent("Incomplete");
                        failedContentMaxWidth = 75;
                    }
                    failedContent.tooltip = "Mark this task as incomplete.";
                    GUI.color = new Color(1, 1, 1, guiColor.a);
                    if (!EditorGUIUtility.isProSkin)
                        GUI.contentColor = new Color(0, 0, 0, contentColor.a * 0.55f);
                    if (GUILayout.Button(failedContent, myStyleToolbarButton, GUILayout.MaxWidth(failedContentMaxWidth)))
                    {
                        Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                        task.completed = false;
                        task.failed = false;
                        selectedTopic.Clicked();
                        EditorUtility.SetDirty(selectedTopic);
                        GUI.FocusControl(null);
                    }
                }
                GUI.contentColor = contentColor;
                GUI.color = guiColor;
            }
        }
        void CompleteTaskButton(Task task, bool forceTextLabels)
        {
            if (!task.failed && !task.ignored)
            {
                Color contentColor = GUI.contentColor;
                Color guiColor = GUI.color;

                if (!task.completed)
                {
                    GUIContent completeContent;
                    int completeContentMaxWidth;
                    if (!forceTextLabels && ((EditorGUIUtility.currentViewWidth <= minimumViewSizeForIconsWithFilterClosed && filterListAnimBool.faded == 0) || (EditorGUIUtility.currentViewWidth <= 800 && filterListAnimBool.faded != 0) || selectedTopic.alwaysShowIconsOnly))
                    {
                        completeContent = new GUIContent(correctIcon);
                        completeContentMaxWidth = 25;
                    }
                    else
                    {
                        completeContent = new GUIContent("Complete");
                        completeContentMaxWidth = 65;
                    }
                    completeContent.tooltip = "This task has been completed";

                    GUI.contentColor = new Color(0.75f, 1, 0.75f, contentColor.a);
                    GUI.color = new Color(0.9f, 1, 0.9f, guiColor.a);
                    if (GUILayout.Button(completeContent, myStyleToolbarButton, GUILayout.MaxWidth(completeContentMaxWidth)))
                    {
                        Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                        task.completed = true;
                        task.failed = false;
                        task.completionTime = new TGSTime(DateTime.Now);
                        task.editing = false;
                        selectedTopic.Clicked();
                        EditorUtility.SetDirty(selectedTopic);
                        GUI.FocusControl(null);
                    }
                }
                else
                {
                    GUIContent completeContent;
                    int completeContentMaxWidth;
                    if (!forceTextLabels && ((EditorGUIUtility.currentViewWidth <= minimumViewSizeForIconsWithFilterClosed && filterListAnimBool.faded == 0) || (EditorGUIUtility.currentViewWidth <= 800 && filterListAnimBool.faded != 0) || selectedTopic.alwaysShowIconsOnly))
                    {
                        completeContent = new GUIContent(clockIcon);
                        completeContentMaxWidth = 25;
                    }
                    else
                    {
                        completeContent = new GUIContent("Incomplete");
                        completeContentMaxWidth = 75;
                    }
                    completeContent.tooltip = "This task has yet to be completed";
                    if (EditorGUIUtility.isProSkin)
                        GUI.contentColor = new Color(1f, 1, 1f, contentColor.a);
                    else
                        GUI.contentColor = new Color(0, 0, 0, contentColor.a * 0.55f);
                    GUI.color = new Color(1f, 1, 1f, guiColor.a);
                    if (GUILayout.Button(completeContent, myStyleToolbarButton, GUILayout.MaxWidth(completeContentMaxWidth)))
                    {
                        Undo.RecordObject(selectedTopic, selectedTopic.name + " update");
                        task.completed = false;
                        task.failed = false;
                        selectedTopic.Clicked();
                        EditorUtility.SetDirty(selectedTopic);
                        GUI.FocusControl(null);
                    }
                }

                GUI.contentColor = contentColor;
                GUI.color = guiColor;
            }
        }

        //void ToDoCardTime(Task toDoCard)
        //{
        //    GUILayout.BeginHorizontal();
        //    GUILayout.Space(25);

        //    if (toDoCard.completed)
        //    {
        //        GUILayout.Label(correctIcon, GUILayout.Width(20), GUILayout.Height(20));
        //        string completionTimeLabel = toDoCard.completionTime.GetFullTime();
        //        string completionTimeToolTip = "Completed on: " + toDoCard.completionTime.GetFullTime();
        //        GUILayout.Label(new GUIContent(completionTimeLabel, completionTimeToolTip), myStleDateTimeLabelStyle, GUILayout.MaxWidth(150));
        //    }
        //    else if (toDoCard.failed)
        //    {
        //        GUILayout.Label(new GUIContent(incorrectIcon), GUILayout.Width(20), GUILayout.Height(20));
        //        string failedTimeLabel = toDoCard.failedTime.GetFullTime();
        //        string failedTimeToolTip = "Failed on: " + toDoCard.failedTime.GetFullTime();
        //        GUILayout.Label(new GUIContent(failedTimeLabel, failedTimeToolTip), myStleDateTimeLabelStyle, GUILayout.MaxWidth(150));
        //    }
        //    else if (toDoCard.ignored)
        //    {
        //        GUILayout.Label(new GUIContent(ignoreIcon), GUILayout.Width(20), GUILayout.Height(20));
        //        string ignoredTimeLabel = toDoCard.ignoredTime.GetFullTime();
        //        string ignoredTimeToolTip = "Failed on: " + toDoCard.failedTime.GetFullTime();
        //        GUILayout.Label(new GUIContent(ignoredTimeLabel, ignoredTimeToolTip), myStleDateTimeLabelStyle, GUILayout.MaxWidth(150));
        //    }
        //    else
        //    {
        //        GUILayout.Label("", GUILayout.Width(20));
        //        GUILayout.Label("", GUILayout.MaxWidth(150));
        //    }

        //    GUILayout.Label(clockIcon, GUILayout.Width(20), GUILayout.Height(20));
        //    string creationTimeLabel = toDoCard.creationTime.GetFullTime();
        //    string creationTimeToolTip = "Created on: " + toDoCard.creationTime.GetFullTime();
        //    GUILayout.Label(new GUIContent(creationTimeLabel, creationTimeToolTip), myStleDateTimeLabelStyle, GUILayout.MaxWidth(150));

        //    GUILayout.EndHorizontal();
        //    GuiLine(Color.gray);
        //}

        //void ToDoTimes(int i)
        //{
        //    string listCreationTimeLabel = selectedTopic.toDoLists[i].creationTime.GetFullTime();
        //    string listCreationTimeToolTip = "Created on: " + selectedTopic.toDoLists[i].creationTime.GetFullTime();


        //    GUILayout.BeginHorizontal();

        //    GUILayout.Label(stopWatchIcon, GUILayout.Width(20), GUILayout.Height(20));
        //    GUILayout.Label(new GUIContent(listCreationTimeLabel, listCreationTimeToolTip), myStleDateTimeLabelStyle, GUILayout.MaxWidth(150));

        //    GUILayout.Label(stopWatchIcon, GUILayout.Width(20), GUILayout.Height(20));
        //    GUILayout.Label(new GUIContent(listCreationTimeLabel, listCreationTimeToolTip), myStleDateTimeLabelStyle, GUILayout.MaxWidth(150));

        //    GUILayout.EndHorizontal();
        //}
        #endregion Stuff in Selected Topic page







        #region Stuff in settings page
        void TopMenu_Settings()
        {
            Color bgColor = GUI.backgroundColor;
            breadCrumbBackgroundColor.a = bgColor.a;
            GUI.backgroundColor = breadCrumbBackgroundColor;


            GUILayout.BeginVertical(myStyleWhiteBox);
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(indentLeft);

                if (GUILayout.Button("Home", myStyleBreadCrumbLink))
                {
                    OpenTopicsList();
                }

                GUILayout.Label(" / ", myStyleBreadCrumbLabel);

                GUILayout.Label("Settings", myStyleBreadCrumbLabel);

                GUILayout.FlexibleSpace();

                GUILayout.Space(indentRight);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.EndVertical();

            GUI.backgroundColor = bgColor;
        }

        #endregion Stuff in settings page
        void TopMenu_Task()
        {
            screenWidth = Screen.width;
            //GUILayout.Label(screenWidth.ToString());
            int maxCharacter = 15;
            if (screenWidth < 590)
                maxCharacter = 10;
            else if (screenWidth < 650)
                maxCharacter = 11;
            else if (screenWidth < 700)
                maxCharacter = 13;
            else if (screenWidth < 750)
                maxCharacter = 15;
            else if (screenWidth < 950)
                maxCharacter = 22;
            else if (screenWidth > 1200)
                maxCharacter = 50;
            else
                maxCharacter = 200;
            Color bgColor = GUI.backgroundColor;
            breadCrumbBackgroundColor.a = bgColor.a;
            GUI.backgroundColor = breadCrumbBackgroundColor;


            GUILayout.BeginVertical(myStyleWhiteBox);
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(indentLeft);

                if (GUILayout.Button("Home", myStyleBreadCrumbLink))
                {
                    OpenTopicsList();
                }

                GUILayout.Label(" / ", myStyleBreadCrumbLabel);

                string topicLabel = selectedTopic.myName;
                if (topicLabel.Length > maxCharacter)
                {
                    topicLabel = topicLabel.Substring(0, maxCharacter);
                    topicLabel += "...";
                }
                GUIContent topicLabellContent = new GUIContent(topicLabel, selectedTopic.myName);
                if (GUILayout.Button(topicLabellContent, myStyleBreadCrumbLink, GUILayout.Width(myStyleBreadCrumbLink.CalcSize(topicLabellContent).x + 2)))
                {
                    selectedTopic.Clicked();
                    currentPage = Page.todoList;
                }
                GUILayout.Label(" / ", myStyleBreadCrumbLabel);


                //string todoListLabel = selectedTodoList.myName;
                //if (todoListLabel.Length > maxCharacter)
                //{
                //    todoListLabel = todoListLabel.Substring(0, maxCharacter);
                //    todoListLabel += "...";
                //}
                //GUIContent todoLabelContent = new GUIContent(todoListLabel, selectedTodoList.myName);
                //if (GUILayout.Button(todoLabelContent, myStyleBreadCrumbLink, GUILayout.Width(myStyleBreadCrumbLink.CalcSize(todoLabelContent).x + 2)))
                //{
                //    selectedTopic.Clicked();
                //    currentPage = Page.todoList;
                //}

                //GUILayout.Label(" / ", myStyleBreadCrumbLabel);

                string taskLabel = selectedTask.myName;
                if (taskLabel.Length > maxCharacter)
                {
                    taskLabel = taskLabel.Substring(0, maxCharacter);
                    taskLabel += "...";
                }
                GUIContent taskLabelContent = new GUIContent(taskLabel, selectedTask.myName);
                GUILayout.Label(taskLabelContent, myStyleBreadCrumbLabel, GUILayout.Width(myStyleBreadCrumbLabel.CalcSize(taskLabelContent).x + 2));

                GUILayout.FlexibleSpace();

                GUILayout.Space(indentRight);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.EndVertical();

            GUI.backgroundColor = bgColor;
        }










        string remainingTextProgressbar = "Remaining ";
        string remainingLabelProgressbar = "Remaining ";
        string completedTextlProgressbar = "Completed ";
        string completedLabelProgressbar = "Completed ";
        string failedTextProgressbar = "Failed ";
        string failedLabelProgressbar = "Failed ";
        void BoardProgressBar(Topic topic)
        {
            if (topic == null)
                return;

            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            int totalTaskCount = topic.GetTotalTasks();

            int incompleteTaskCount = topic.GetIncompleteTasks();
            int completedTasksCount = topic.GetCompletedTaskCount();
            int failedTasksCount = topic.GetFailedTaskCount();

            if (Screen.width < 650)
            {
                remainingLabelProgressbar = "";
                completedLabelProgressbar = "";
                failedLabelProgressbar = "";
            }
            else
            {
                remainingLabelProgressbar = remainingTextProgressbar;
                completedLabelProgressbar = completedTextlProgressbar;
                failedLabelProgressbar = failedTextProgressbar;
            }

            if (incompleteTaskCount != 0)
                ProgressBar(incompleteTaskCount, totalTaskCount, remainingLabelProgressbar, remainingTextProgressbar, progressbarTextureRemaining);

            GUILayout.Space(7);

            if (completedTasksCount != 0)
                ProgressBar(completedTasksCount, totalTaskCount, completedLabelProgressbar, completedTextlProgressbar, progressbarTextureCompleted);

            GUILayout.Space(7);

            if (failedTasksCount != 0)
                ProgressBar(failedTasksCount, totalTaskCount, failedLabelProgressbar, failedTextProgressbar, progressbarTextureFailed);

            GUILayout.Space(indentRight + 2);
            GUILayout.EndHorizontal();
        }

        void ProgressBar(int value, int total, string progressbarType, string tooltip = "", Texture2D previewTexture = null)
        {
            if (previewTexture == null)
                previewTexture = progressbarTexture;

            string fulllabel;
            if (!showPercentageInProgressBar)
            {
                fulllabel = progressbarType + value;
                tooltip += (((float)value / total) * 100).ToString("00.00") + "%";
            }
            else
            {
                fulllabel = progressbarType + (((float)value / total) * 100).ToString("00.00") + "%";
                tooltip += progressbarType + value;
            }

            GUIContent content = new GUIContent(fulllabel, tooltip);

            if (page_home_progressbarBorder)
                GUILayout.BeginHorizontal(EditorStyles.helpBox);
            else
                GUILayout.BeginHorizontal();

            Rect r = EditorGUILayout.BeginVertical();
            float percentage;
            if (value != 0 && total != 0)
                percentage = (float)value / total;
            else
                percentage = 0;

            Color guiColor = GUI.color;
            EditorGUI.DrawPreviewTexture(new Rect(r.x, r.y, r.width * guiColor.a, r.height), progressbarBGTexture);
            EditorGUI.DrawPreviewTexture(new Rect(r.x, r.y, r.width * percentage * guiColor.a * guiColor.a, r.height), previewTexture);
            EditorGUI.LabelField(new Rect(r.x, r.y, r.width, r.height), content, myStyleProgressbarLabel);

            GUILayout.Space(18);
            EditorGUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        int controlID;
        void DrawBoardIcon(Topic board, Color originalBackgroundColor)
        {
            GUI.backgroundColor = board.mainColor;
            if (page_home_iconBorder)
                GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(topicIconSize), GUILayout.Height(topicIconSize));
            else
                GUILayout.BeginVertical(GUILayout.Width(topicIconSize), GUILayout.Height(topicIconSize));

            controlID = EditorGUIUtility.GetControlID(FocusType.Passive);

            if (board.iconTexture)
            {
                if (GUILayout.Button(board.iconTexture, GUIStyle.none, GUILayout.Width(topicIconSize), GUILayout.Height(topicIconSize)))
                {
                    if (currentPage == Page.topicList)
                        OpenTopic(board);
                    else
                    {
                        EditorGUIUtility.ShowObjectPicker<Texture>(null, false, "", controlID);
                    }
                }
            }
            else
            {
                if (GUILayout.Button(assetIcon, GUIStyle.none, GUILayout.Width(topicIconSize), GUILayout.Height(topicIconSize)))
                {
                    if (currentPage == Page.topicList)
                        OpenTopic(board);
                    else
                    {
                        EditorGUIUtility.ShowObjectPicker<Texture>(null, false, "", controlID);
                    }
                }


            }

            if (currentPage != Page.topicList)
            {
                string commandName = Event.current.commandName;
                if (commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID() == controlID)
                {
                    Texture2D selectedTexture = EditorGUIUtility.GetObjectPickerObject() as Texture2D;
                    board.iconTexture = selectedTexture;
                    EditorUtility.SetDirty(board);
                    EditorGUIUtility.ExitGUI();
                }
            }

            GUILayout.EndVertical();
            GUI.backgroundColor = originalBackgroundColor;
        }

        void OpenTopic(Topic board)
        {
            maxTaskIndexWidth = 0;
            selectedTopic = board;

            UpdateSerializedContent();

            selectedTopic.Clicked();
            currentPage = Page.todoList;
        }

        private void UpdateSerializedContent()
        {
            if (selectedTopic == null)
                return;

            selectedTopicSO = new SerializedObject(selectedTopic);
            reorderableListsOfTask.Clear();
            serializedistsOfTask.Clear();

            for (int i = 0; i < selectedTopic.toDoLists.Count; i++)
            {
                ReorderableList reorderableList = new ReorderableList(selectedTopic.toDoLists[i].tasks, typeof(Task), true, false, false, false);
                reorderableList.drawElementCallback = DrawTasksGUI;
                reorderableList.drawElementBackgroundCallback = (rect, index, active, focused) =>
                {
                    ////if (active)
                    //{
                    //    if (index % 2 == 0)
                    //        GUI.DrawTexture(rect, myTextureAlternateTask, ScaleMode.StretchToFill);
                    //    else
                    //        GUI.DrawTexture(rect, Texture2D.grayTexture, ScaleMode.StretchToFill);
                    //}
                    ////else
                    ////{
                    ////    if (index % 2 == 0)
                    ////        GUI.DrawTexture(rect, myTextureAlternateTask);
                    ////}
                };
                reorderableList.elementHeightCallback = (index) =>
                {
                    Repaint();
                    float height = EditorGUIUtility.singleLineHeight;

                    Task currentTask = (Task)reorderableList.list[index];

                    if (proEditor != null)
                    {
                        if (!BeingSearchedForInTopic(currentTask)) return 0;
                        if (!BeingSearchedForInToDoList(selectedTodoList, currentTask)) return 0;
                        if (FilteredOut(currentTask)) return 0;
                        if (TagFilteredOut(currentTask)) return 0;
                    }


                    if (currentTask.heightTakenInEditor > 5)
                        return currentTask.heightTakenInEditor;
                    else
                        return height;
                };



                reorderableListsOfTask.Add(reorderableList);

                SerializedProperty property = selectedTopicSO.FindProperty("toDoLists").GetArrayElementAtIndex(i).FindPropertyRelative("tasks");
                serializedistsOfTask.Add(property);
            }
        }










        void CreateNewTopic()
        {
            Topic asset = ScriptableObject.CreateInstance<Topic>();
            asset.toDoLists.Add(new ToDoList("To do"));
            asset.toDoLists.Add(new ToDoList("Doing"));
            asset.toDoLists.Add(new ToDoList("In Review"));
            asset.toDoLists.Add(new ToDoList("Done"));

            string name = AssetDatabase.GenerateUniqueAssetPath("Assets/Plugins/Tiny Giant Studio/Modular To Do List/Board.asset");
            AssetDatabase.CreateAsset(asset, name);
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;

            Debug.Log("New Board created", asset);
        }
        void RefreshBoardsList()
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(Topic).Name);  //FindAssets uses tags check documentation for more info
            topics = new Topic[guids.Length];
            for (int i = 0; i < guids.Length; i++)         //probably could get optimized 
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                topics[i] = AssetDatabase.LoadAssetAtPath<Topic>(path);
            }
            UpdateStarredUnstarredBoardsList();
            UpdateTopicsStats();
        }

        void UpdateStarredUnstarredBoardsList()
        {
            starredTopics.Clear();
            unStarredTopics.Clear();
            for (int i = 0; i < topics.Length; i++)
            {
                if (topics[i])
                {
                    if (topics[i].starred)
                        starredTopics.Add(topics[i]);
                    else
                        unStarredTopics.Add(topics[i]);
                }
                else
                {
                    requiresRefreshingTopicList = true;
                }
            }
        }












        void PrepEverything()
        {
            RefreshBoardsList();

            SetupAnimBools();
            GetGraphicsReferences();

            PageTransitionSpeed = EditorPrefs.GetFloat(editorPrefsPrefix + "PageTransitionSpeed");
            if (PageTransitionSpeed == 0)
                PageTransitionSpeed = 5;

            if (!EditorPrefs.GetBool(editorPrefsPrefix + "OpenedBefore"))
            {
                EditorPrefs.SetBool(editorPrefsPrefix + "OpenedBefore", true);

                EditorPrefs.SetFloat(editorPrefsPrefix + "PageTransitionSpeed", 2);
                EditorPrefs.SetInt(editorPrefsPrefix + "taskIsDueInDays", 7);
                EditorPrefs.SetInt(editorPrefsPrefix + "maximumReferenceInOneLine", 2);
                EditorPrefs.SetFloat(editorPrefsPrefix + "fontSize", 1);

                EditorPrefs.SetBool(editorPrefsPrefix + "page_home_topicBorder", true);
                EditorPrefs.SetBool(editorPrefsPrefix + "page_home_icon", true);
                EditorPrefs.SetBool(editorPrefsPrefix + "page_home_iconBorder", true);
                EditorPrefs.SetBool(editorPrefsPrefix + "page_home_progressbarBorder", true);
            }

            showPercentageInProgressBar = EditorPrefs.GetBool(editorPrefsPrefix + "showPercentageInProgressBar");
            taskConsideredDueSoonIfLessThanDaysLeft = EditorPrefs.GetInt(editorPrefsPrefix + "taskIsDueInDays");
            newTaskIsDueInCurrentTimePlusHours = EditorPrefs.GetInt(editorPrefsPrefix + "newTaskIsDueInCurrentTimePlusHours");
            maximumReferenceInOneLine = EditorPrefs.GetInt(editorPrefsPrefix + "maximumReferenceInOneLine") + 1;
            fontSize = EditorPrefs.GetFloat(editorPrefsPrefix + "fontSize");
            clickingOnTaskEditsIt = EditorPrefs.GetBool(editorPrefsPrefix + "clickingOnTaskEditsIt");
            taskAndToDoDeleteConfirmation = EditorPrefs.GetBool(editorPrefsPrefix + "taskAndToDoDeleteConfirmation");

            page_home_topicBorder = EditorPrefs.GetBool(editorPrefsPrefix + "page_home_topicBorder");
            page_home_icon = EditorPrefs.GetBool(editorPrefsPrefix + "page_home_icon");
            page_home_iconBorder = EditorPrefs.GetBool(editorPrefsPrefix + "page_home_iconBorder");
            page_home_progressbarBorder = EditorPrefs.GetBool(editorPrefsPrefix + "page_home_progressbarBorder");


            if (proEditor == null)
                proEditor = Type.GetType("TinyGiantStudio.ModularToDoLists.ModularToDoListProEditor");

            if (proEditor != null)
                currentAssetLink = proAssetLink;
        }




        /// <summary>
        /// If new animbools are added, don't forget to add them to PageTransitionSpeed's setter
        /// </summary>
        AnimBool page_topicListAnimBool;
        AnimBool page_cardListAnimBool;
        AnimBool page_settingsAnimBool;
        AnimBool page_taskAnimBool;


        AnimBool filterListAnimBool;
        AnimBool bottomInfoAnimBool;

        void SetupAnimBools()
        {
            page_topicListAnimBool = new AnimBool(false);
            page_topicListAnimBool.valueChanged.AddListener(Repaint);

            page_cardListAnimBool = new AnimBool(false);
            page_cardListAnimBool.valueChanged.AddListener(Repaint);

            page_settingsAnimBool = new AnimBool(false);
            page_settingsAnimBool.valueChanged.AddListener(Repaint);

            page_taskAnimBool = new AnimBool(false);
            page_taskAnimBool.valueChanged.AddListener(Repaint);



            filterListAnimBool = new AnimBool(EditorPrefs.GetBool("filterListAnimBool"));
            filterListAnimBool.valueChanged.AddListener(Repaint);

            bottomInfoAnimBool = new AnimBool(true);
            bottomInfoAnimBool.valueChanged.AddListener(Repaint);
        }





        Texture2D getTheProVersionTexture;
        Texture2D progressbarTexture;
        Texture2D progressbarTextureRemaining;
        Texture2D progressbarTextureCompleted;
        Texture2D progressbarTextureFailed;
        Texture2D progressbarBGTexture;
        Texture2D backgroundTexture;
        //Texture2D myTextureAlternateTask;

        Texture assetIcon;
        Texture companyIcon;
        Texture editIcon;
        Texture starIcon;
        Texture unStarIcon;
        Texture clockIcon;

        static Texture correctIcon;
        Texture incorrectIcon;
        Texture ignoreIcon;
        Texture trashIcon;
        Texture nextIcon;
        Texture backIcon;
        Texture upIcon;
        Texture downIcon;

        GUIContent refreshIcon;
        string refreshIconName = "d_Refresh";
        GUIContent addIcon;
        string addIconName = "d_Toolbar Plus";
        //string emptyFolderIconName = "d_Folder Icon";
        GUIContent folderIcon;
        string folderIconName = "d_Folder Icon";
        GUIContent folderOpenedIcon;
        string folderOpenedIconName = "d_FolderOpened Icon";

        Font boldFont;
        Font semiboldFont;
        Font regularFont;

        void GetGraphicsReferences()
        {
            boldFont = EditorGUIUtility.Load("Assets/Plugins/Tiny Giant Studio/Modular To Do List/Fonts/Montserrat-Bold.ttf") as Font;
            semiboldFont = EditorGUIUtility.Load("Assets/Plugins/Tiny Giant Studio/Modular To Do List/Fonts/Montserrat-SemiBold.ttf") as Font;
            regularFont = EditorGUIUtility.Load("Assets/Plugins/Tiny Giant Studio/Modular To Do List/Fonts/Montserrat-Regular.ttf") as Font;

            getTheProVersionTexture = EditorGUIUtility.Load("Assets/Plugins/Tiny Giant Studio/Modular To Do List/Artworks/GetTheProVersion.png") as Texture2D;
            progressbarBGTexture = EditorGUIUtility.Load("Assets/Plugins/Tiny Giant Studio/Modular To Do List/Artworks/progressbarBG.png") as Texture2D;
            progressbarTexture = EditorGUIUtility.Load("Assets/Plugins/Tiny Giant Studio/Modular To Do List/Artworks/progressbar.png") as Texture2D;
            progressbarTextureRemaining = EditorGUIUtility.Load("Assets/Plugins/Tiny Giant Studio/Modular To Do List/Artworks/progressbarRemaining.png") as Texture2D;
            progressbarTextureCompleted = EditorGUIUtility.Load("Assets/Plugins/Tiny Giant Studio/Modular To Do List/Artworks/progressbarCompleted.png") as Texture2D;
            progressbarTextureFailed = EditorGUIUtility.Load("Assets/Plugins/Tiny Giant Studio/Modular To Do List/Artworks/progressbarFailed.png") as Texture2D;
            //myTextureAlternateTask = EditorGUIUtility.Load("Assets/Plugins/Tiny Giant Studio/Modular To Do List/Artworks/Task Alternate BG.png") as Texture2D;

            backgroundTexture = EditorGUIUtility.Load("Assets/Plugins/Tiny Giant Studio/Modular To Do List/Artworks/BG.png") as Texture2D;
            backgroundTexture.wrapMode = TextureWrapMode.Repeat;

            editIcon = EditorGUIUtility.Load("Assets/Plugins/Tiny Giant Studio/Modular To Do List/Artworks/Pencil.png") as Texture2D;
            assetIcon = EditorGUIUtility.Load("Assets/Plugins/Tiny Giant Studio/Modular To Do List/Artworks/AssetIcon.png") as Texture2D;
            companyIcon = EditorGUIUtility.Load("Assets/Plugins/Tiny Giant Studio/Modular To Do List/Artworks/TGSIcon.png") as Texture2D;
            starIcon = EditorGUIUtility.Load("Assets/Plugins/Tiny Giant Studio/Modular To Do List/Artworks/Star.png") as Texture2D;
            unStarIcon = EditorGUIUtility.Load("Assets/Plugins/Tiny Giant Studio/Modular To Do List/Artworks/unStar.png") as Texture2D;
            clockIcon = EditorGUIUtility.Load("Assets/Plugins/Tiny Giant Studio/Modular To Do List/Artworks/Icon Clock.png") as Texture2D;

            correctIcon = EditorGUIUtility.Load("Assets/Plugins/Tiny Giant Studio/Modular To Do List/Artworks/Icon Correct.png") as Texture2D;
            incorrectIcon = EditorGUIUtility.Load("Assets/Plugins/Tiny Giant Studio/Modular To Do List/Artworks/Icon Fail.png") as Texture2D;
            ignoreIcon = EditorGUIUtility.Load("Assets/Plugins/Tiny Giant Studio/Modular To Do List/Artworks/Icon Ignore.png") as Texture2D;
            trashIcon = EditorGUIUtility.Load("Assets/Plugins/Tiny Giant Studio/Modular To Do List/Artworks/Icon Trash.png") as Texture2D;
            nextIcon = EditorGUIUtility.Load("Assets/Plugins/Tiny Giant Studio/Modular To Do List/Artworks/Icon Next.png") as Texture2D;
            backIcon = EditorGUIUtility.Load("Assets/Plugins/Tiny Giant Studio/Modular To Do List/Artworks/Icon Back.png") as Texture2D;
            upIcon = EditorGUIUtility.Load("Assets/Plugins/Tiny Giant Studio/Modular To Do List/Artworks/Icon Up.png") as Texture2D;
            downIcon = EditorGUIUtility.Load("Assets/Plugins/Tiny Giant Studio/Modular To Do List/Artworks/Icon Down.png") as Texture2D;
            //incorrectIcon = EditorGUIUtility.IconContent(incorrectIconName, "|");
            refreshIcon = EditorGUIUtility.IconContent(refreshIconName, "|Refresh");
            addIcon = EditorGUIUtility.IconContent(addIconName, "|Add");

            folderIcon = EditorGUIUtility.IconContent(folderIconName, "|closed");
            folderOpenedIcon = EditorGUIUtility.IconContent(folderOpenedIconName, "|Opened");
        }




        //generic styles
        GUIStyle myStyleIcon;
        GUIStyle myStyleInputField;
        GUIStyle myStyleInputFieldLarge;
        GUIStyle myStyleTaskNameInputField;
        GUIStyle myStyleToolbarSearchField;
        GUIStyle myStyleLinkLabelBiggerInputField;

        GUIStyle myStyleLabel;
        GUIStyle myStyleLabelFaded;
        GUIStyle myStyleLabelLarge;

        GUIStyle myStyleToggle;
        GUIStyle myStyleLinkLabel;
        GUIStyle myStyleLinkLabelBigger;
        GUIStyle myStyleNormalButton;

        GUIStyle myStyleToolbarLabel;
        GUIStyle myStyleToolbarButton;
        GUIStyle myStyleToolbarInputField;

        //GUIStyle myStyleWhiteBackgroundColor;


        //specific styles
        GUIStyle myStyleTodoLabel;
        GUIStyle myStyleTaskLabel;
        GUIStyle myStyleDueDateLabel;
        GUIStyle myStyleTaskNumbering;
        GUIStyle myStleDateTimeLabelStyle;

        GUIStyle myStyleProgressbarLabel;
        GUIStyle myStyleVerticalBox;
        GUIStyle myStyleHorizontallBox;
        GUIStyle myStyleWhiteBox;
        GUIStyle myStyleTaskOne;
        GUIStyle myStyleTaskTwo;

        GUIStyle myStyleCompanyName;

        GUIStyle myStyleTags;

        GUIStyle myStyleBreadCrumbLabel;
        GUIStyle myStyleBreadCrumbLabelFaded;
        GUIStyle myStyleBreadCrumbLink;


        Color breadCrumbBackgroundColor = new Color(0.13f, 0.13f, 0.15f, 1);
        readonly Color myColorYellow = new Color(1f, 0.8f, 0.5f, 1);
        readonly Color myColorYellowDark = new Color(0.42f, 0.18f, 0.1f, 1);
        readonly Color myColorRed = new Color(1f, 0.4f, 0.25f, 1);
        readonly Color myColorRedDark = new Color(0.25f, 0.1f, 0.05f, 1);


        bool recreateStyles = false;
        void GenerateStyle()
        {
            if (myStyleBreadCrumbLabel == null || recreateStyles)
            {
                myStyleBreadCrumbLabel = new GUIStyle(EditorStyles.wordWrappedLabel);
                myStyleBreadCrumbLabel.font = regularFont;
                myStyleBreadCrumbLabel.fontSize = Mathf.RoundToInt(13 * fontSize);

                myStyleBreadCrumbLabel.padding = new RectOffset(0, 0, 0, 0);
                myStyleBreadCrumbLabel.margin = new RectOffset(0, 0, 0, 0);
                myStyleBreadCrumbLabel.contentOffset = new Vector2(0, 0);
            }
            if (myStyleBreadCrumbLabelFaded == null || recreateStyles)
            {
                myStyleBreadCrumbLabelFaded = new GUIStyle(myStyleBreadCrumbLabel);
                myStyleBreadCrumbLabelFaded.normal.textColor = new Color(1, 1, 1, 0.25f);
            }
            if (myStyleBreadCrumbLink == null || recreateStyles)
            {
                myStyleBreadCrumbLink = new GUIStyle(myStyleBreadCrumbLabel);
                myStyleBreadCrumbLink.font = boldFont;

                if (EditorGUIUtility.isProSkin)
                {
                    myStyleBreadCrumbLink.normal.textColor = myColorYellow;
                    myStyleBreadCrumbLink.hover.textColor = myColorRed;
                }
                else
                {
                    myStyleBreadCrumbLink.normal.textColor = myColorYellowDark;
                    myStyleBreadCrumbLink.hover.textColor = myColorRedDark;
                }
            }





            //if (myStyleTaskOne == null)
            {
                myStyleTaskOne = new GUIStyle();
                myStyleTaskOne.normal.background = progressbarBGTexture;
            }
            //if (myStyleTaskTwo == null)
            {
                myStyleTaskTwo = new GUIStyle();
                myStyleTaskTwo.normal.background = backgroundTexture;
            }

            if (myStyleVerticalBox == null)
            {
                myStyleVerticalBox = new GUIStyle(EditorStyles.helpBox);
                myStyleVerticalBox.normal.background = backgroundTexture;
                //should be two lines
                myStyleVerticalBox.margin = new RectOffset(0, 0, 0, 0);
                myStyleVerticalBox.padding = new RectOffset(5, 5, 5, 5);

            }
            if (myStyleHorizontallBox == null)
            {
                myStyleHorizontallBox = new GUIStyle(EditorStyles.helpBox);
                myStyleHorizontallBox.normal.background = progressbarTexture;
                myStyleHorizontallBox.active.background = progressbarTexture;
            }

            if (myStyleWhiteBox == null || recreateStyles)
            {
                myStyleWhiteBox = new GUIStyle(EditorStyles.helpBox);
                myStyleWhiteBox.normal.background = Texture2D.whiteTexture;
                myStyleWhiteBox.font = regularFont;
                myStyleWhiteBox.fontSize = Mathf.RoundToInt(12 * fontSize);
            }

            //generic styles start
            if (myStyleIcon == null)
            {
                myStyleIcon = new GUIStyle(EditorStyles.iconButton);
                myStyleIcon.padding = new RectOffset(0, 0, 0, 0);
                myStyleIcon.margin = new RectOffset(0, 0, 0, 0);

            }
            if (myStyleLabel == null || recreateStyles)
            {
                myStyleLabel = new GUIStyle(EditorStyles.wordWrappedLabel);
                myStyleLabel.font = regularFont;
                myStyleLabel.fontSize = Mathf.RoundToInt(11 * fontSize);
                myStyleLabel.padding = new RectOffset(0, 0, 0, 0);
                myStyleLabel.margin = new RectOffset(0, 0, 0, 0);
                myStyleLabel.contentOffset = new Vector2(0, 0);
            }
            if (myStyleLabelFaded == null)
            {
                myStyleLabelFaded = new GUIStyle(myStyleLabel);
                myStyleLabelFaded.normal.textColor = new Color(1, 1, 1, 0.5f);
                myStyleLabelFaded.fontSize = 10;
                myStyleLabelFaded.padding = new RectOffset(0, 0, 8, 0);
            }
            if (myStyleLabelLarge == null || recreateStyles)
            {
                myStyleLabelLarge = new GUIStyle(myStyleLabel);
                myStyleLabelLarge.fontSize = Mathf.RoundToInt(14 * fontSize);
                myStyleLabelLarge.normal.textColor = myColorYellow;
            }
            if (myStyleInputField == null || recreateStyles)
            {
                myStyleInputField = new GUIStyle(EditorStyles.textArea);
                myStyleInputField.font = regularFont;
                myStyleInputField.fontSize = Mathf.RoundToInt(11 * fontSize);
                myStyleInputField.wordWrap = true;
            }
            if (myStyleTaskNameInputField == null || recreateStyles)
            {
                myStyleTaskNameInputField = new GUIStyle(EditorStyles.textArea);
                myStyleTaskNameInputField.font = semiboldFont;
                myStyleTaskNameInputField.fontSize = Mathf.RoundToInt(11 * fontSize);
                myStyleTaskNameInputField.wordWrap = true;
                myStyleTaskNameInputField.contentOffset = new Vector2(2, 1);
            }
            if (myStyleInputFieldLarge == null || recreateStyles)
            {
                myStyleInputFieldLarge = new GUIStyle(myStyleInputField);
                myStyleInputFieldLarge.fontSize = Mathf.RoundToInt(11 * fontSize);
            }
            if (myStyleTaskLabel == null || recreateStyles)
            {
                myStyleTaskLabel = new GUIStyle(EditorStyles.wordWrappedLabel);
                myStyleTaskLabel.font = semiboldFont;
                myStyleTaskLabel.normal.textColor = Color.white;
                myStyleTaskLabel.fontSize = Mathf.RoundToInt(11 * fontSize);
            }
            if (myStyleToggle == null || recreateStyles)
            {
                myStyleToggle = new GUIStyle(EditorStyles.wordWrappedLabel);
                myStyleToggle.font = regularFont;
                myStyleToggle.fontSize = Mathf.RoundToInt(11 * fontSize);
                myStyleToggle.padding = new RectOffset(0, 0, 3, 0);
                myStyleToggle.margin = new RectOffset(0, 0, 0, 0);
            }
            if (myStyleToolbarSearchField == null || recreateStyles)
            {
                myStyleToolbarSearchField = new GUIStyle(EditorStyles.toolbarSearchField);
                myStyleToolbarSearchField.font = regularFont;
                myStyleToolbarSearchField.fontSize = Mathf.RoundToInt(11 * fontSize);
            }
            if (myStyleLinkLabelBiggerInputField == null || recreateStyles)
            {
                myStyleLinkLabelBiggerInputField = new GUIStyle(myStyleInputField);
                myStyleLinkLabelBiggerInputField.font = boldFont;
                myStyleLinkLabelBiggerInputField.fontSize = Mathf.RoundToInt(14 * fontSize);
                myStyleLinkLabelBiggerInputField.padding = new RectOffset(2, 0, 0, 0);
                myStyleLinkLabelBiggerInputField.margin = new RectOffset(0, 0, 0, 0);
            }
            if (myStyleNormalButton == null)
            {
                myStyleNormalButton = new GUIStyle("Button")
                {
                    font = boldFont,
                    fontSize = 12
                };
                if (EditorGUIUtility.isProSkin)
                {
                    myStyleNormalButton.normal.textColor = myColorYellow;
                    myStyleNormalButton.hover.textColor = Color.yellow;
                    myStyleNormalButton.active.textColor = Color.green;
                }
                else
                {
                    myStyleNormalButton.normal.textColor = Color.black;
                    myStyleNormalButton.hover.textColor = Color.red;
                    myStyleNormalButton.active.textColor = Color.black;
                }
            }


            if (myStyleToolbarButton == null || recreateStyles)
            {
                myStyleToolbarButton = new GUIStyle(EditorStyles.toolbarButton);
                myStyleToolbarButton.fontSize = Mathf.RoundToInt(11 * fontSize);
                myStyleToolbarButton.onHover.textColor = Color.yellow;
                myStyleToolbarButton.active.textColor = Color.green;
            }
            if (myStyleToolbarInputField == null || recreateStyles)
            {
                myStyleToolbarInputField = new GUIStyle(EditorStyles.toolbarTextField);
                myStyleToolbarInputField.fontSize = Mathf.RoundToInt(11 * fontSize);
                myStyleToolbarInputField.onHover.textColor = Color.yellow;
                myStyleToolbarInputField.active.textColor = Color.green;
            }
            if (myStyleToolbarLabel == null)
            {
                myStyleToolbarLabel = new GUIStyle(myStyleLabel);
                myStyleToolbarLabel.contentOffset = new Vector2(0, 3);
            }


            if (myStyleLinkLabel == null || recreateStyles)
            {
                myStyleLinkLabel = new GUIStyle(EditorStyles.label);
                myStyleLinkLabel.font = regularFont;
                myStyleLinkLabel.fontSize = Mathf.RoundToInt(11 * fontSize);
                myStyleLinkLabel.padding = new RectOffset(0, 0, 0, 0);
                myStyleLinkLabel.margin = new RectOffset(0, 0, 0, 0);

                if (EditorGUIUtility.isProSkin)
                {
                    myStyleLinkLabel.normal.textColor = myColorYellow;
                    myStyleLinkLabel.hover.textColor = myColorRed;
                }
                else
                {
                    myStyleLinkLabel.normal.textColor = myColorYellowDark;
                    myStyleLinkLabel.hover.textColor = myColorRedDark;
                }
            }
            if (myStyleLinkLabelBigger == null || recreateStyles)
            {
                myStyleLinkLabelBigger = new GUIStyle(myStyleLinkLabel);
                myStyleLinkLabelBigger.fontSize = Mathf.RoundToInt(14 * fontSize);
                myStyleLinkLabelBigger.font = boldFont;
            }
            if (myStyleProgressbarLabel == null)
            {
                myStyleProgressbarLabel = new GUIStyle(myStyleLabel);
                myStyleProgressbarLabel.normal.textColor = Color.white;
                myStyleProgressbarLabel.contentOffset = new Vector2(10, 3);
            }
            //generic styles - end






            //specific component styles - start
            if (myStyleDueDateLabel == null)
            {
                myStyleDueDateLabel = new GUIStyle(myStyleLabel);
                myStyleDueDateLabel.contentOffset = new Vector2(0, 5);
                myStyleDueDateLabel.alignment = TextAnchor.MiddleRight;
            }

            if (myStyleTodoLabel == null || recreateStyles)
            {
                myStyleTodoLabel = new GUIStyle(EditorStyles.toolbar);
                myStyleTodoLabel.fontSize = Mathf.RoundToInt(13 * fontSize);
                myStyleTodoLabel.font = boldFont;

                if (EditorGUIUtility.isProSkin)
                    myStyleTodoLabel.normal.textColor = myColorYellow;
                else
                    myStyleTodoLabel.normal.textColor = myColorYellowDark;

                myStyleTodoLabel.alignment = TextAnchor.MiddleRight;
            }



            if (myStyleTaskNumbering == null)
            {
                myStyleTaskNumbering = new GUIStyle();
                myStyleTaskNumbering.normal.textColor = Color.white;
                myStyleTaskNumbering.font = semiboldFont;
                myStyleTaskNumbering.fontSize = Mathf.RoundToInt(11 * fontSize);


                myStyleTaskNumbering.normal.background = Texture2D.grayTexture;
                myStyleTaskNumbering.padding = new RectOffset(5, 8, 0, 0);
                myStyleTaskNumbering.margin = new RectOffset(0, 0, 0, 0);
                myStyleTaskNumbering.contentOffset = new Vector2(0, 0);
            }

            if (myStleDateTimeLabelStyle == null || recreateStyles)
            {
                myStleDateTimeLabelStyle = new GUIStyle(EditorStyles.wordWrappedLabel);
                myStleDateTimeLabelStyle.fontSize = Mathf.RoundToInt(10 * fontSize);
            }
            if (myStyleCompanyName == null || recreateStyles)
            {
                myStyleCompanyName = new GUIStyle(myStyleLabel);
                myStyleCompanyName.fontSize = Mathf.RoundToInt(14 * fontSize);
            }

            if (myStyleTags == null || recreateStyles)
            {
                myStyleTags = new GUIStyle(EditorStyles.wordWrappedLabel);
                myStyleTags.font = regularFont;
                myStyleTags.normal.background = Texture2D.whiteTexture;
                myStyleTags.fontSize = Mathf.RoundToInt(12 * fontSize);
            }

            //specific component styles - end

            recreateStyles = false;
        }

        enum Page
        {
            topicList,
            todoList,
            task,
            settings
        }
    }
}