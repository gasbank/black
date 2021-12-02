﻿using UnityEngine;

public static class CanvasGroupExtension
{
    public static void Show(this CanvasGroup cg)
    {
        cg.alpha = 1.0f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    public static void Hide(this CanvasGroup cg)
    {
        cg.alpha = 0.0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }
}