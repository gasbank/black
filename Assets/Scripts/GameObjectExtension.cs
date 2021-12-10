using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class GameObjectExtension
{
    public static bool IsInteractable(this GameObject gameObject)
    {
        if (gameObject == null)
            return false;
 
        Selectable selectable = gameObject.GetComponent<Selectable>();
        if (selectable != null && !selectable.interactable)
            return false;
 
        List<CanvasGroup> m_CanvasGroupCache = new List<CanvasGroup>();
        bool interactibleCheck = true;
 
        Transform cg_transform = gameObject.transform;
        while (cg_transform != null)
        {
            cg_transform.GetComponents(m_CanvasGroupCache);
            bool ignoreParentGroups = false;
 
            for (int i = 0, count = m_CanvasGroupCache.Count; i < count; i++)
            {
                var canvasGroup = m_CanvasGroupCache[i];
 
                interactibleCheck &= canvasGroup.interactable;
                ignoreParentGroups |= canvasGroup.ignoreParentGroups || !canvasGroup.interactable;
            }
 
            if (ignoreParentGroups)
            {
                break;
            }
 
            cg_transform = cg_transform.parent;
        }
 
        return interactibleCheck;
    }
    
    public static bool IsBlocksRaycasts(this GameObject gameObject)
    {
        if (gameObject == null)
            return false;
 
        Selectable selectable = gameObject.GetComponent<Selectable>();
        if (selectable != null && !selectable.interactable)
            return false;
 
        List<CanvasGroup> m_CanvasGroupCache = new List<CanvasGroup>();
        bool interactibleCheck = true;
 
        Transform cg_transform = gameObject.transform;
        while (cg_transform != null)
        {
            cg_transform.GetComponents(m_CanvasGroupCache);
            bool ignoreParentGroups = false;
 
            for (int i = 0, count = m_CanvasGroupCache.Count; i < count; i++)
            {
                var canvasGroup = m_CanvasGroupCache[i];
 
                interactibleCheck &= canvasGroup.blocksRaycasts;
                ignoreParentGroups |= canvasGroup.ignoreParentGroups || !canvasGroup.blocksRaycasts;
            }
 
            if (ignoreParentGroups)
            {
                break;
            }
 
            cg_transform = cg_transform.parent;
        }
 
        return interactibleCheck;
    }
}
