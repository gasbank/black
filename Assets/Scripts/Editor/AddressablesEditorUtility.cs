using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

public class AddressablesEditorUtility
{
    public static void SetAddressableGroupAutomatically(Object obj)
    {
        var assetPath = AssetDatabase.GetAssetPath(obj);

        var parentPath = assetPath;

        while (string.IsNullOrEmpty(parentPath) == false)
        {
            var newParentPath = Path.GetDirectoryName(parentPath);
            if (newParentPath == parentPath)
                break;

            parentPath = newParentPath;

            var parentPathGuid = AssetDatabase.GUIDFromAssetPath(parentPath);
            if (AssetDatabase.GetLabels(parentPathGuid).Contains("AddressablesGroup"))
            {
                break;
            }
        }

        if (parentPath == assetPath)
        {
            Debug.LogWarning($"{assetPath}: Addressables Group cannot be determined from asset path.");
        }
        else
        {
            var parentDirName = Path.GetFileNameWithoutExtension(parentPath);
            SetAddressableGroup(obj, parentDirName);
        }
    }

    static void SetAddressableGroup(Object obj, string groupName = null)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;

        if (!settings)
        {
            Debug.LogWarning("AddressableAssetSettingsDefaultObject.Settings is null!");
            return;
        }

        var group = string.IsNullOrEmpty(groupName) ? settings.DefaultGroup : settings.FindGroup(groupName);

        if (!group)
        {
            group = settings.CreateGroup(groupName, false, false, true,
                null, typeof(ContentUpdateGroupSchema),
                typeof(BundledAssetGroupSchema));
        }

        var assetPath = AssetDatabase.GetAssetPath(obj);
        var guid = AssetDatabase.AssetPathToGUID(assetPath);

        var e = settings.CreateOrMoveEntry(guid, group, false, false);
        var normalizedAssetName = Path.GetFileNameWithoutExtension(e.AssetPath)?.Normalize();
        e.SetAddress(normalizedAssetName, false);

        var entriesAdded = new List<AddressableAssetEntry> {e};

        group.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, false, true);
        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, true, false);
    }
}