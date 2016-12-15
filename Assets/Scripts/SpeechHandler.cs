using HoloToolkit.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class SpeechHandler : MonoBehaviour {

    KeywordRecognizer keywordRecognizer = null;
    Dictionary<string, Action> keywords = new Dictionary<string, Action>();

    void Start () {
        keywords.Add("Visual Mesh Off", () =>
        {
            GameObject.Find("SpatialMapping").GetComponent<SpatialMappingManager>().DrawVisualMeshes = false;
        });

        keywords.Add("Visual Mesh On", () =>
        {
            GameObject.Find("SpatialMapping").GetComponent<SpatialMappingManager>().DrawVisualMeshes = true;
        });

        keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());

        keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        keywordRecognizer.Start();
    }

    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        Action keywordAction;
        if (keywords.TryGetValue(args.text, out keywordAction))
        {
            keywordAction.Invoke();
        }
    }
}
