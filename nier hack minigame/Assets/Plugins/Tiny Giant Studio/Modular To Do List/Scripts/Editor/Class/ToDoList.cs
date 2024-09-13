using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace TinyGiantStudio.ModularToDoLists
{
    [System.Serializable]
    public class ToDoList
    {
        public string myName = "New To Do List"; //Import,export supported

        //editor stuff
        public bool editing;

        public Color mainColor = Color.white;  //Import,export supported 
        public bool addedDescription = false;   //Import,export supported 
                                                //myDescription is added to export if this is enabled and ignored if disabled.
                                                //So, when importing, if description is null/empty, this is set to false
        public string myDescription = "Description"; //Import,export supported
        public List<Task> tasks = new List<Task>(); //Import,export supported

        public TGSTime creationTime; //Import,export supported
        //public TGSTime completionTime;
        //public TGSTime targetTime;

        //editor only
        public bool opened = true;
        public string searchingFor = "";






        #region Constructors
        public ToDoList()
        {

        }
        public ToDoList(string listName)
        {
            myName = listName;
        }
        /// <summary>
        /// This makes a copy.
        /// But it keeps a reference(?) to the older version, so, anychanges made to this, also changes the older one
        /// </summary>
        /// <returns></returns>
        public ToDoList(ToDoList other)
        {
            foreach (FieldInfo field in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                field.SetValue(this, field.GetValue(other));
            }
        }
        #endregion











        public int GetActiveTaskCount()
        {
            int count = 0;
            for (int i = 0; i < tasks.Count; i++)
            {
                if (!tasks[i].completed && !tasks[i].failed && !tasks[i].ignored)
                    count++;
            }
            return count;
        }
        public int GetCompletedTaskCount()
        {
            int count = 0;
            for (int i = 0; i < tasks.Count; i++)
            {
                if (tasks[i].completed)
                    count++;
            }
            return count;
        }
        public int GetFailedTaskCount()
        {
            int count = 0;
            for (int i = 0; i < tasks.Count; i++)
            {
                if (tasks[i].failed)
                    count++;
            }
            return count;
        }
        public int GetTotalTaskCount()
        {
            int count = 0;
            for (int i = 0; i < tasks.Count; i++)
            {
                count++;
            }
            return count;
        }
    }
}