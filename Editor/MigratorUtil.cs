using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace CLogic.Dialogue.Editor
{
    public static class MigratorUtil
    {
        static readonly (Regex pattern, string replacement)[] k_RegexRules =
        {
            // 1. Generic type references (CRITICAL — must run first)
            (
                new Regex(
                    @"CLogic\.Systems\.DialogueSystem\.Editor\.([A-Za-z0-9_]+),\s*CLogic\.Systems\.DialogueGraph\.Editor",
                    RegexOptions.Compiled),
                "CLogic.Dialogue.Editor.$1, CLogic.Dialogue.Editor"
            ),

            // 2. Namespace inside type blocks
            (
                new Regex(
                    @"ns:\s*CLogic\.Systems\.DialogueSystem\.Editor",
                    RegexOptions.Compiled),
                "ns: CLogic.Dialogue.Editor"
            ),

            // 3. Assembly inside type blocks
            (
                new Regex(
                    @"asm:\s*CLogic\.Systems\.DialogueGraph\.Editor",
                    RegexOptions.Compiled),
                "asm: CLogic.Dialogue.Editor"
            ),

            // 4. Double-colon references
            (
                new Regex(
                    @"CLogic\.Systems\.DialogueSystem\.Editor::",
                    RegexOptions.Compiled),
                "CLogic.Dialogue.Editor::"
            ),

            // 5. GraphToolkit assembly migration
            (
                new Regex(
                    @"asm:\s*Unity\.GraphToolkit\.(Editor|Internal\.Editor)}",
                    RegexOptions.Compiled),
                "asm: UnityEditor.GraphToolkitModule}"
            ),

            // 6. GraphToolkit type references
            (
                new Regex(
                    @"Unity\.GraphToolkit\.Editor::",
                    RegexOptions.Compiled),
                "UnityEditor.GraphToolkitModule::"
            ),

            // 7. Script reference fix
            (
                new Regex(
                    @"\{fileID:\s*11500000,\s*guid:\s*790b4d75d92f4b0984310a268dbd952f,\s*type:\s*3\}",
                    RegexOptions.Compiled),
                "{fileID: 12501, guid: 0000000000000000e000000000000000, type: 0}"
            )
        };

        static void MigrateFile(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            if (!filePath.EndsWith(".cdg"))
                return;

            var original = File.ReadAllText(filePath);

            // pre-check to avoid unnecessary processing
            if (!original.Contains("CLogic.Systems.DialogueSystem.Editor") &&
                !original.Contains("CLogic.Systems.DialogueGraph.Editor") &&
                !original.Contains("Unity.GraphToolkit"))
                return;

            var migrated = original;

            foreach (var (pattern, replacement) in k_RegexRules)
            {
                migrated = pattern.Replace(migrated, replacement);
            }

            if (migrated != original)
            {
                File.WriteAllText(filePath, migrated);
                Debug.Log($"[Migration] Updated: {filePath}");
            }
        }

        [MenuItem("Tools/CLogic/Migrate Graph Assets")]
        internal static void MigrateGraphAssets()
        {
            var paths = AssetDatabase.GetAllAssetPaths();

            int processed = 0;
            int modified = 0;

            foreach (var path in paths)
            {
                if (!path.StartsWith("Assets") && !path.StartsWith("Packages/dev.clogic.dialogue"))
                    continue;

                processed++;

                var fullPath = Path.GetFullPath(path);
                var before = File.Exists(fullPath) ? File.ReadAllText(fullPath) : null;

                MigrateFile(fullPath);

                if (before != null)
                {
                    var after = File.ReadAllText(fullPath);
                    if (before != after)
                        modified++;
                }
            }

            Debug.Log($"[Migration Complete] Processed: {processed}, Modified: {modified}");

            AssetDatabase.Refresh();
        }
    }
}