using UnityEngine;

public static class InstantiateLocalized
{
    static readonly bool enableFeature = true;

    public static GameObject InstantiateLocalize(GameObject original)
    {
        var newPrefab = Object.Instantiate(original);

        if (Data.dataSet == null) return newPrefab;

        if (enableFeature)
        {
            var textCollector = newPrefab.GetComponent<TextCollector>();
            if (textCollector != null)
            {
                var curFont = FontManager.Instance.GetLanguageFont(Data.Instance.CurrentLanguageCode);
                foreach (var text in textCollector.AllTextsInPrefab)
                    if (text.font != curFont)
                        text.font = curFont;
            }
        }

        return newPrefab;
    }

    public static GameObject InstantiateLocalize(GameObject original, Transform parent)
    {
        var newPrefab = Object.Instantiate(original, parent);

        if (Data.dataSet == null) return newPrefab;

        if (enableFeature)
        {
            var textCollector = newPrefab.GetComponent<TextCollector>();
            if (textCollector != null)
            {
                var curFont = FontManager.Instance.GetLanguageFont(Data.Instance.CurrentLanguageCode);
                foreach (var text in textCollector.AllTextsInPrefab)
                    if (text.font != curFont)
                        text.font = curFont;
            }
        }

        return newPrefab;
    }

    public static GameObject InstantiateLocalize(Object original, Transform parent, bool instantiateInWorldSpace)
    {
        var newPrefab = (GameObject) Object.Instantiate(original, parent, instantiateInWorldSpace);

        if (Data.dataSet == null) return newPrefab;

        if (enableFeature)
        {
            var textCollector = newPrefab.GetComponent<TextCollector>();
            if (textCollector != null)
            {
                var curFont = FontManager.Instance.GetLanguageFont(Data.Instance.CurrentLanguageCode);
                foreach (var text in textCollector.AllTextsInPrefab)
                    if (text.font != curFont)
                        text.font = curFont;
            }
        }

        return newPrefab;
    }

    public static Component InstantiateLocalize(Component original)
    {
        var newPrefab = Object.Instantiate(original);

        if (Data.dataSet == null) return newPrefab;

        if (enableFeature)
        {
            var textCollector = newPrefab.GetComponent<TextCollector>();
            if (textCollector != null)
            {
                var curFont = FontManager.Instance.GetLanguageFont(Data.Instance.CurrentLanguageCode);
                foreach (var text in textCollector.AllTextsInPrefab)
                    if (text.font != curFont)
                        text.font = curFont;
            }
        }

        return newPrefab;
    }

    public static Component InstantiateLocalize(Component original, Transform parent)
    {
        var newPrefab = Object.Instantiate(original, parent);

        if (Data.dataSet == null) return newPrefab;

        if (enableFeature)
        {
            var textCollector = newPrefab.GetComponent<TextCollector>();
            if (textCollector != null)
            {
                var curFont = FontManager.Instance.GetLanguageFont(Data.Instance.CurrentLanguageCode);
                foreach (var text in textCollector.AllTextsInPrefab)
                    if (text.font != curFont)
                        text.font = curFont;
            }
        }

        return newPrefab;
    }


    public static Component InstantiateLocalize(Component original, Transform parent, bool instantiateInWorldSpace)
    {
        var newPrefab = Object.Instantiate(original, parent, instantiateInWorldSpace);

        if (Data.dataSet == null) return newPrefab;

        if (enableFeature)
        {
            var textCollector = newPrefab.GetComponent<TextCollector>();
            if (textCollector != null)
            {
                var curFont = FontManager.Instance.GetLanguageFont(Data.Instance.CurrentLanguageCode);
                foreach (var text in textCollector.AllTextsInPrefab)
                    if (text.font != curFont)
                        text.font = curFont;
            }
        }

        return newPrefab;
    }
}