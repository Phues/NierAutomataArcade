using UnityEngine;

namespace TinyGiantStudio.ModularToDoLists
{
    [CreateAssetMenu(fileName = "Untitled Color Scheme", menuName = "Tiny Giant Studio/Modular To Do List/New Color Scheme", order = 1)]
    [System.Serializable]
    public class ColorScheme : ScriptableObject
    {
        public Color completedTask = new Color(0.9f, 1f, 0.9f, 1);
        public Color inprogressTask = new Color(0.9f, 1f, 0.9f, 1);
        public Color failedTask = new Color(0.9f, 1f, 0.9f, 1);
        public Color ignoredTask = new Color(0.9f, 1f, 0.9f, 1);
    }
}