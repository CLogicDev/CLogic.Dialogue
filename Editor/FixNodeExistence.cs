using UnityEditor;
namespace CLogic.Dialogue.Editor
{
    [InitializeOnLoad]
    public static class FixNodeExistence
    {
        static FixNodeExistence()
        {
            DialogueSettings.UpdateDefines(DialogueSettings.GetOrCreateSettings().features);
        }
    }
}
