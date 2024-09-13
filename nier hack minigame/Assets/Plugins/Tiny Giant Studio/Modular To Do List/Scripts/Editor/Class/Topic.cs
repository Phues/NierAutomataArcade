using System;
using System.Collections.Generic;
using UnityEngine;

namespace TinyGiantStudio.ModularToDoLists
{
    [CreateAssetMenu(fileName = "Untitled Topic", menuName = "Tiny Giant Studio/Modular To Do List/New Topic", order = 1)]
    public class Topic : ScriptableObject
    {
        public string myName = "Untitled Topic"; //Import,export supported
        public string myDescription = ""; //Import,export supported
        public Texture iconTexture;
        public Color mainColor = new Color(0.7f, 0.7f, 0.7f, 1); //Import,export supported
        public ColorScheme colorScheme;
        public bool starred;
        public List<ToDoList> toDoLists = new List<ToDoList>(); //Import,export supported




        //filters
        public bool showPendingItems = true;
        public bool showCompletedItems = true;
        public bool showIgnoredItems = true;
        public bool showFailedTasks = true;

        public bool alwaysShowIconsOnly = false;
        public bool showTaskDetails = true;
        public bool hideFolderIcons = false;
        public bool showRealIndex = false;

        public List<Tag> tags = new List<Tag>();
        public TagFilterType tagFilterType = TagFilterType.hideInactiveTags;

        //editor stuff
        public bool editing;
        public string searchingFor; //Pro feature

        //stats
        public int dueSoonTasksCount = 0;
        public int backlogTasksCount = 0;

        public bool keywordFound = false;
        public bool searchKeywordInTopicName;
        public bool searchKeywordInTopicDescription;
        public int searchKeywordInTodoListName;
        public int searchKeywordInTodoListDescription;
        public int searchKeywordInTaskNames;
        public int searchKeywordInTaskDescription;

        public void Search(string keyword)
        {
            keywordFound = false;
            searchKeywordInTopicName = false;
            searchKeywordInTopicDescription = false;
            searchKeywordInTodoListName = 0;
            searchKeywordInTodoListDescription = 0;
            searchKeywordInTaskNames = 0;
            searchKeywordInTaskDescription = 0;

            keyword = keyword.ToUpper();

            if (myName.ToUpper().Contains(keyword))
                searchKeywordInTopicName = true;

            if (myDescription.ToUpper().Contains(keyword))
                searchKeywordInTopicDescription = true;

            for (int i = 0; i < toDoLists.Count; i++)
            {
                if (toDoLists[i].myName.ToUpper().Contains(keyword))
                    searchKeywordInTodoListName++;

                if (toDoLists[i].myDescription.ToUpper().Contains(keyword))
                    searchKeywordInTodoListDescription++;

                for (int j = 0; j < toDoLists[i].tasks.Count; j++)
                {
                    if (toDoLists[i].tasks[j].myName.ToUpper().Contains(keyword))
                        searchKeywordInTaskNames++;
                    if (toDoLists[i].tasks[j].myDescription.ToUpper().Contains(keyword))
                        searchKeywordInTaskDescription++;
                }
            }

            if (searchKeywordInTopicName || searchKeywordInTopicDescription || searchKeywordInTodoListName > 0 || searchKeywordInTodoListDescription > 0 || searchKeywordInTaskNames > 0 || searchKeywordInTaskDescription > 0)
                keywordFound = true;
        }

        /// <summary>
        /// Resets everything
        /// </summary>
        public void Clicked()
        {
            editing = false;
            searchingFor = "";

            for (int i = 0; i < toDoLists.Count; i++)
            {
                toDoLists[i].editing = false;
                toDoLists[i].searchingFor = "";
                for (int j = 0; j < toDoLists[i].tasks.Count; j++)
                {
                    toDoLists[i].tasks[j].editing = false;
                }
            }

            for (int i = 0; i < tags.Count; i++)
            {
                tags[i].editing = false;
            }
            GetStats();
            GUI.FocusControl(null);
        }


        public void GetStats()
        {
            dueSoonTasksCount = 0;
            backlogTasksCount = 0;

            for (int i = 0; i < toDoLists.Count; i++)
            {
                ToDoList list = toDoLists[i];

                for (int j = 0; j < list.tasks.Count; j++)
                {
                    Task task = list.tasks[j];
                    if (!task.completed && !task.failed && !task.ignored)
                    {
                        if (task.hasDueDate)
                        {
                            if (task.dueDate.dateTime < DateTime.Now)
                                backlogTasksCount++;

                            else if (task.dueDate.DaysFromCurrentTime() < 7)
                                dueSoonTasksCount++;
                        }
                    }
                }
            }
        }

        public int GetIncompleteTasks()
        {
            int total = 0;
            for (int i = 0; i < toDoLists.Count; i++)
            {
                total += toDoLists[i].GetActiveTaskCount();
            }
            return total;
        }
        public int GetCompletedTaskCount()
        {
            int total = 0;
            for (int i = 0; i < toDoLists.Count; i++)
            {
                total += toDoLists[i].GetCompletedTaskCount();
            }
            return total;
        }
        public int GetFailedTaskCount()
        {
            int total = 0;
            for (int i = 0; i < toDoLists.Count; i++)
            {
                total += toDoLists[i].GetFailedTaskCount();
            }
            return total;
        }
        public int GetTotalTasks()
        {
            int total = 0;
            for (int i = 0; i < toDoLists.Count; i++)
            {
                total += toDoLists[i].GetTotalTaskCount();
            }
            return total;
        }

        [System.Serializable]
        public class Tag
        {
            public bool enabled = true;
            public bool editing = false;
            public Color bgColor = new Color(0.25f, 0.25f, 0.25f, 1); //Import,export supported
            public Color textColor = Color.white; //Import,export supported
            public string myName = "Unnamed tag";  //Import,export supported
        }

        [System.Serializable]
        public enum TagFilterType
        {
            hideInactiveTags,
            onlyShowActiveTags
        }
    }
}