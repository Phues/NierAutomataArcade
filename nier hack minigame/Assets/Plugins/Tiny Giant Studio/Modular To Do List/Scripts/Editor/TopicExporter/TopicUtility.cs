using System.Text.RegularExpressions;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace TinyGiantStudio.ModularToDoLists
{
    public class TopicUtility
    {
        readonly static string exporterVersion = "1.0.0"; //This is used to identify which logic will be used to export/import the file.

        /// <summary>
        /// DO NOT CHANGE THE VALUES OF THESE VARIABLES 
        /// Changing these values will break compability with importing files from older versions that had the older names
        /// These MUST be UNIQUE
        /// </summary>
        readonly static string versionLabel = "Version";

        readonly static string topicNameLabel = "Topic Name";
        readonly static string topicDescriptionLabel = "Topic Description";
        readonly static string topicMainColorLabel = "Topic Main Color";
        readonly static string topicIconTextureLabel = "Topic Icon";

        readonly static string tagNameLabel = "Tag Name";
        readonly static string tagTextColorLabel = "Tag Text Color";
        readonly static string tagBackgroundColorLabel = "Tag Background Color";

        readonly static string listNameLabel = "List Name";
        readonly static string listDescriptionLabel = "List Description";
        readonly static string listCreationTimeLabel = "List Creation Time";
        readonly static string listMainColorLabel = "List Main Color";

        readonly static string taskNameLabel = "Task Name";
        readonly static string taskDescriptionLabel = "Task Description";
        readonly static string taskCreationTimeLabel = "Task Creation Time";

        readonly static string taskCompletedLabel = "Task Completed";
        readonly static string taskCompletionTimeLabel = "Task Completion Time";
        readonly static string taskFailedLabel = "Task Failed";
        readonly static string taskFailedTimeLabel = "Task Failed Time";
        readonly static string taskIgnoredLabel = "Task Ignored";
        readonly static string taskIgnoredTimeLabel = "Task Ignored Time";
        readonly static string taskDueLabel = "Task Due";
        readonly static string taskDueTimeLabel = "Task Due Time";


        readonly static string referenceLabel = "Reference";

        readonly static string referencesStart = "#Reference"; //unused when importing (hardcoded)
        readonly static string referencesEnd = "#EndReference"; //unused when importing (hardcoded)



        public static bool WriteTopicToCsv
            (
            Topic topic,
            bool includeVersionNumber,
            bool includeTopicName,
            bool includeTopicDescription,
            bool tags,
            bool colors,
            bool topicIcon,

            bool includeLists,
            int emptyColumnBeforeLists,
            bool listName,
            bool listDescription,
            bool listCreationTime,

            bool includeTasks,
            int emptyColumnBeforeTasks,
            bool taskName,
            bool taskDescription,
            bool taskStatus,
            bool taskTimes,
            bool taskReferences
            )
        {
            ///Get the path
            string pathToSave = EditorUtility.SaveFilePanel("Export Topic", Application.dataPath, topic.myName, "csv");

            if (string.IsNullOrEmpty(pathToSave)) return false;

            var csvBuilder = new StringBuilder();

            if (includeVersionNumber) csvBuilder.Append(versionLabel + "," + exporterVersion);

            csvBuilder.AppendLine();

            if (includeTopicName) csvBuilder.AppendLine(topicNameLabel + "," + topic.myName);
            if (includeTopicDescription) csvBuilder.AppendLine(topicDescriptionLabel + "," + topic.myDescription);
            if (colors) csvBuilder.AppendLine(topicMainColorLabel + "," + ColorUtility.ToHtmlStringRGBA(topic.mainColor));
            if (topicIcon)
            {
                if (topic.iconTexture != null)
                {
                    csvBuilder.AppendLine(topicIconTextureLabel + "," + AssetDatabase.GetAssetPath(topic.iconTexture));
                }
            }
            if (includeLists)
            {
                foreach (var toDoList in topic.toDoLists)
                {
                    int i = topic.toDoLists.IndexOf(toDoList);
                    csvBuilder.AppendLine($"#ToDoList[{i}]"); //This is used by the importer to detect the start of a list

                    if (listName) csvBuilder.AppendLine(Spacing(emptyColumnBeforeLists) + listNameLabel + "," + toDoList.myName);
                    if (listDescription && toDoList.addedDescription) csvBuilder.AppendLine(Spacing(emptyColumnBeforeLists) + listDescriptionLabel + "," + toDoList.myDescription);
                    if (listCreationTime) csvBuilder.AppendLine(Spacing(emptyColumnBeforeLists) + listCreationTimeLabel + "," + toDoList.creationTime);
                    if (colors) csvBuilder.AppendLine(Spacing(emptyColumnBeforeLists) + listMainColorLabel + "," + ColorUtility.ToHtmlStringRGBA(toDoList.mainColor));

                    if (!includeTasks)
                        continue;

                    foreach (var task in toDoList.tasks)
                    {
                        int j = toDoList.tasks.IndexOf(task);
                        csvBuilder.AppendLine(Spacing(emptyColumnBeforeTasks) + "#Task[" + j + "]"); //This is used by the importer to detect the start of a task

                        if (taskName) csvBuilder.AppendLine(Spacing(emptyColumnBeforeTasks) + taskNameLabel + "," + task.myName);
                        if (taskDescription && task.addedDescription) csvBuilder.AppendLine(Spacing(emptyColumnBeforeTasks) + taskDescriptionLabel + "," + task.myDescription);

                        if (taskStatus)
                        {
                            csvBuilder.AppendLine(Spacing(emptyColumnBeforeTasks) + taskDueLabel + "," + task.hasDueDate);
                            csvBuilder.AppendLine(Spacing(emptyColumnBeforeTasks) + taskCompletedLabel + "," + task.completed);
                            csvBuilder.AppendLine(Spacing(emptyColumnBeforeTasks) + taskFailedLabel + "," + task.failed);
                            csvBuilder.AppendLine(Spacing(emptyColumnBeforeTasks) + taskIgnoredLabel + "," + task.ignored);
                        }

                        if (taskTimes)
                        {
                            csvBuilder.AppendLine(Spacing(emptyColumnBeforeTasks) + taskCreationTimeLabel + "," + task.creationTime);

                            if (task.hasDueDate) csvBuilder.AppendLine(Spacing(emptyColumnBeforeTasks) + taskDueTimeLabel + "," + task.dueDate);
                            if (task.completed) csvBuilder.AppendLine(Spacing(emptyColumnBeforeTasks) + taskCompletionTimeLabel + "," + task.completionTime);
                            if (task.failed) csvBuilder.AppendLine(Spacing(emptyColumnBeforeTasks) + taskFailedLabel + "," + task.failedTime);
                            if (task.ignored) csvBuilder.AppendLine(Spacing(emptyColumnBeforeTasks) + taskIgnoredTimeLabel + "," + task.ignoredTime);
                        }



                        if (tags)
                        {
                            foreach (var tag in task.tags) //todo: turn it into for loop
                            {
                                int k = task.tags.IndexOf(tag);
                                csvBuilder.AppendLine(Spacing(emptyColumnBeforeTasks) + "#Tag[" + k + "]");
                                csvBuilder.AppendLine(Spacing(emptyColumnBeforeTasks) + tagNameLabel + "," + tag.myName);
                                if (colors) csvBuilder.AppendLine(Spacing(emptyColumnBeforeTasks) + tagTextColorLabel + "," + ColorUtility.ToHtmlStringRGBA(tag.textColor));
                                if (colors) csvBuilder.AppendLine(Spacing(emptyColumnBeforeTasks) + tagBackgroundColorLabel + "," + ColorUtility.ToHtmlStringRGBA(tag.bgColor));
                                csvBuilder.AppendLine(Spacing(emptyColumnBeforeTasks) + "#EndTag");
                            }
                        }

                        if (taskReferences)
                        {
                            for (int k = 0; k < task.references.Count; k++)
                            {
                                csvBuilder.AppendLine(Spacing(emptyColumnBeforeTasks) + referencesStart + "[" + k + "]");
                                csvBuilder.AppendLine(Spacing(emptyColumnBeforeTasks) + referenceLabel + "," + AssetDatabase.GetAssetPath(task.references[k]));
                                csvBuilder.AppendLine(Spacing(emptyColumnBeforeTasks) + referencesEnd);
                            }
                        }


                        csvBuilder.AppendLine(Spacing(emptyColumnBeforeTasks) + "#EndTask"); //This is used by the importer to detect the end of a Task
                    }
                    csvBuilder.AppendLine(Spacing(emptyColumnBeforeLists) + "#EndToDoList"); //This is used by the importer to detect the end of a List
                }
            }


            File.WriteAllText(pathToSave, csvBuilder.ToString());
            AssetDatabase.Refresh();
            EditorUtility.RevealInFinder(pathToSave);
            Debug.Log("File exported to " + pathToSave);

            return true;
        }

        static string Spacing(int spacingAmount)
        {
            string spacing = string.Empty;

            for (int i = 0; i < spacingAmount; i++)
            {
                spacing += ",";
            }

            return spacing;
        }

        [MenuItem("Tools/Tiny Giant Studio/Modular To-do Lists/Import Topic", false, 10001)]
        public static void ReadTopicFromCsv()
        {
            string pathToRead = EditorUtility.OpenFilePanel("Import Topic", Application.dataPath, "csv");
            if (string.IsNullOrEmpty(pathToRead)) return;

            string csvContent = File.ReadAllText(pathToRead);
            Topic topic = ScriptableObject.CreateInstance<Topic>();
            //Topic topic = new Topic();

            string todolistPattern = @"#ToDoList\[\d+\][\s\S]*?#EndToDoList";
            var todolistsMatches = Regex.Matches(csvContent, todolistPattern);

            string version = GetValueFromCsv(csvContent, versionLabel);
            Debug.Log("Importer version number : " + version);

            string topicName = GetValueFromCsv(csvContent, topicNameLabel);
            if (topicName != null) topic.myName = topicName;
            string topicDescription = GetValueFromCsv(csvContent, topicDescriptionLabel);
            if (topicDescription != null) topic.myDescription = topicDescription;

            string topicMainColor = GetValueFromCsv(csvContent, topicMainColorLabel);
            if (topicMainColor != null)
            {
                if (ColorUtility.TryParseHtmlString("#" + topicMainColor, out Color topicColor))
                    topic.mainColor = topicColor;
            }


            string topicIconString = GetValueFromCsv(csvContent, topicIconTextureLabel);
            if (topicIconString != null)
            {
                var icon = AssetDatabase.LoadAssetAtPath(topicIconString, typeof(Texture)) as Texture;
                if (icon != null)
                {
                    topic.iconTexture = icon;
                }
            }


            foreach (Match toDoListMatch in todolistsMatches.Cast<Match>())
            {
                ToDoList toDoList = new();
                string toDoListContent = toDoListMatch.Value;

                // Extract properties from ToDoList
                string listName = GetValueFromCsv(toDoListContent, listNameLabel);
                if (listName != null) toDoList.myName = listName;
                string listDescription = GetValueFromCsv(toDoListContent, listDescriptionLabel);
                if (listDescription != null)
                {
                    toDoList.addedDescription = true;
                    toDoList.myDescription = listDescription;
                }
                toDoList.creationTime = GetTime(toDoListContent, listCreationTimeLabel);

                string listMainColor = GetValueFromCsv(csvContent, listMainColorLabel);
                if (listMainColor != null)
                {
                    if (ColorUtility.TryParseHtmlString("#" + listMainColor, out Color listColor))
                        toDoList.mainColor = listColor;
                }
                //toDoList.creationTime = TGSTime.Parse(GetValueFromCsv(toDoListContent, "Creation Time"));
                //toDoList.completionTime = TGSTime.Parse(GetValueFromCsv(toDoListContent, "Completation Time"));
                //toDoList.targetTime = TGSTime.Parse(GetValueFromCsv(toDoListContent, "Target Time"));

                string taskPattern = @"#Task\[\d+\][\s\S]*?#EndTask";
                var tasksMatches = Regex.Matches(toDoListContent, taskPattern);
                foreach (Match taskMatch in tasksMatches)
                {
                    Task task = new Task();
                    string taskContent = taskMatch.Value;

                    // Extract properties from Task
                    string taskName = GetValueFromCsv(taskContent, taskNameLabel);
                    if (taskName != null) task.myName = taskName;
                    string taskDescription = GetValueFromCsv(taskContent, taskDescriptionLabel);
                    if (taskDescription != null)
                    {
                        task.addedDescription = true;
                        task.myDescription = taskDescription;
                    }

                    //TODO: These require furthur testing
                    bool.TryParse(GetValueFromCsv(taskContent, taskDueLabel), out task.hasDueDate);
                    bool.TryParse(GetValueFromCsv(taskContent, taskCompletedLabel), out task.completed);
                    bool.TryParse(GetValueFromCsv(taskContent, taskFailedLabel), out task.failed);
                    bool.TryParse(GetValueFromCsv(taskContent, taskIgnoredLabel), out task.ignored);

                    task.creationTime = GetTime(taskContent, taskCreationTimeLabel);
                    if (task.hasDueDate) task.dueDate = GetTime(taskContent, taskDueTimeLabel);
                    if (task.completed) task.completionTime = GetTime(taskContent, taskCompletionTimeLabel);
                    if (task.failed) task.failedTime = GetTime(taskContent, taskFailedTimeLabel);
                    if (task.ignored) task.ignoredTime = GetTime(taskContent, taskIgnoredTimeLabel);
                    // if (task.hasDueDate) task.dueDate = TGSTime.Parse(GetValueFromCsv(taskContent, taskDueTimeLabel));
                    //if (task.completed) task.completionTime = TGSTime.Parse(GetValueFromCsv(taskContent, taskCompletionTimeLabel));
                    //if (task.failed) task.failedTime = TGSTime.Parse(GetValueFromCsv(taskContent, taskFailedTimeLabel));
                    //if (task.ignored) task.ignoredTime = TGSTime.Parse(GetValueFromCsv(taskContent, taskIgnoredTimeLabel));

                    string tagPattern = @"#Tag\[\d+\][\s\S]*?#EndTag";
                    var tagsMatches = Regex.Matches(taskContent, tagPattern);
                    foreach (Match tagMatch in tagsMatches)
                    {
                        Topic.Tag tag = new Topic.Tag();
                        string tagContent = tagMatch.Value;

                        // Extract properties from Tag
                        string tagName = GetValueFromCsv(tagContent, tagNameLabel);
                        if (tagName != null) tag.myName = tagName;

                        string tagTextColorString = GetValueFromCsv(tagContent, tagTextColorLabel);
                        if (tagTextColorString != null)
                        {
                            if (ColorUtility.TryParseHtmlString("#" + tagTextColorString, out Color tagTextColor))
                                tag.textColor = tagTextColor;
                        }

                        string tagBGColorString = GetValueFromCsv(tagContent, tagBackgroundColorLabel);
                        if (tagBGColorString != null)
                        {
                            if (ColorUtility.TryParseHtmlString("#" + tagBGColorString, out Color tagBGColor))
                                tag.textColor = tagBGColor;
                        }

                        // Add the tag to the task
                        task.tags.Add(tag);
                    }


                    string referencePattern = @"#Reference\[\d+\][\s\S]*?#EndReference";
                    var referenceMatches = Regex.Matches(taskContent, referencePattern);
                    foreach (Match referenceMatch in referenceMatches)
                    {
                        string referenceContent = referenceMatch.Value;
                        string referenceLocation = GetValueFromCsv(referenceContent, referenceLabel);

                        if (referenceLocation != null)
                        {
                            var refernceObject = AssetDatabase.LoadAssetAtPath(referenceLocation, typeof(Object));
                            //var refernceObject = Resources.Load(referenceLocation);
                            if (refernceObject != null)
                            {
                                //these bools are also set when adding reference
                                task.addedReference = true;
                                task.hideMyDetails = false;
                                topic.showTaskDetails = true;

                                task.references.Add(refernceObject);
                            }
                            Debug.Log(refernceObject);
                        }
                    }


                    // Add the task to the ToDoList
                    toDoList.tasks.Add(task);
                }

                // Add the ToDoList to the Topic
                topic.toDoLists.Add(toDoList);
            }

            topic.tags = GetTagsFromTasks(topic);

            string pathToSave = EditorUtility.SaveFilePanelInProject("Save Topic", topic.myName, "asset", "");
            AssetDatabase.CreateAsset(topic, pathToSave);
            AssetDatabase.Refresh();
            Debug.Log("Topic imported to " + pathToSave);
        }




        static List<Topic.Tag> GetTagsFromTasks(Topic topic)
        {
            List<Topic.Tag> tags = new List<Topic.Tag>();

            for (int i = 0; i < topic.toDoLists.Count; i++)
            {
                for (int j = 0; j < topic.toDoLists[i].tasks.Count; j++)
                {
                    for (int k = 0; k < topic.toDoLists[i].tasks[j].tags.Count; k++)
                    {
                        bool includes = false;
                        for (int tagNumber = 0; tagNumber < tags.Count; tagNumber++)
                        {
                            if (tags[tagNumber].myName == topic.toDoLists[i].tasks[j].tags[k].myName)
                            {
                                includes = true;
                                break;
                            }
                        }

                        if (!includes) tags.Add(topic.toDoLists[i].tasks[j].tags[k]);
                    }
                }
            }

            return tags;
        }










        static TGSTime GetTime(string content, string label)
        {
            string timeString = GetValueFromCsv(content, label);
            if (timeString != null) return TGSTime.Parse(timeString);
            return null;
        }

        static string GetValueFromCsv(string csvContent, string propertyName)
        {
            string pattern = $@"{propertyName},(.+?)(?:\n|$)";
            Match match = Regex.Match(csvContent, pattern);
            return match.Success ? match.Groups[1].Value.Trim() : null;
        }
    }
}