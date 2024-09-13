using System.Collections.Generic;
using UnityEngine;

namespace TinyGiantStudio.ModularToDoLists
{
    [System.Serializable]
    public class Task
    {
        public string myName = "New Task Name"; //Import,export supported
        public bool hideMyDetails = true;
        public bool addedDescription = false;//Import,export supported 
                                             //myDescription is added to export if this is enabled and ignored if disabled.
                                             //So, when importing, if description is null/empty, this is set to false
        public string myDescription = "Description"; //Import,export supported

        public bool addedReference = false;
        public List<UnityEngine.Object> references = new List<UnityEngine.Object>();

        public bool completed = false;
        public bool failed = false;
        public bool ignored = false;

        public TGSTime creationTime;  //Import,export supported
        public bool hasDueDate = false; //Import,export supported
        public TGSTime dueDate; //Import,export supported
        public TGSTime completionTime; //Import,export supported
        public TGSTime failedTime; //Import,export supported
        public TGSTime ignoredTime; //Import,export supported

        public List<Topic.Tag> tags = new List<Topic.Tag>();  //Import,export supported

        //editor only
        public bool editing;

        [Tooltip("The index if filtered out tasks are ignored.")]
        public int visibilityIndex;
        public float heightTakenInEditor;
    }
}