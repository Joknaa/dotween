﻿// Author: Daniele Giardini - http://www.demigiant.com
// Created: 2014/06/29 20:37
// 
// License Copyright (c) Daniele Giardini.
// This work is subject to the terms at http://dotween.demigiant.com/license.php

using System.Text;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEditor;
using UnityEngine;

namespace DG.DOTweenEditor.UI
{
    [CustomEditor(typeof(DOTweenComponent))]
    public class DOTweenComponentInspector : Editor
    {
        DOTweenSettings _settings;
        string _title;
        readonly StringBuilder _strb = new StringBuilder();
        bool _isRuntime;
        Texture2D _headerImg;
        string _playingTweensHex;
        string _pausedTweensHex;

        #region Unity + GUI

        void OnEnable()
        {
            _isRuntime = EditorApplication.isPlaying;
            ConnectToSource(true);

            _strb.Length = 0;
            _strb.Append("DOTween v").Append(DOTween.Version);
            if (TweenManager.isDebugBuild) _strb.Append(" [Debug build]");
            else _strb.Append(" [Release build]");

            if (EditorUtils.hasPro) _strb.Append("\nDOTweenPro v").Append(EditorUtils.proVersion);
            else _strb.Append("\nDOTweenPro not installed");
            if (EditorUtils.hasDOTweenTimeline) _strb.Append("\nDOTweenTimeline v").Append(EditorUtils.dotweenTimelineVersion);
            else _strb.Append("\nDOTweenTimeline not installed");
            _title = _strb.ToString();

            _playingTweensHex = EditorGUIUtility.isProSkin ? "<color=#00c514>" : "<color=#005408>";
            _pausedTweensHex = EditorGUIUtility.isProSkin ? "<color=#ff832a>" : "<color=#873600>";
        }

        override public void OnInspectorGUI()
        {
            _isRuntime = EditorApplication.isPlaying;
            ConnectToSource();

            EditorGUIUtils.SetGUIStyles();

            // Header img
            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            Rect headeR = GUILayoutUtility.GetRect(0, 93, 18, 18);
            GUI.DrawTexture(headeR, _headerImg, ScaleMode.ScaleToFit, true);
            GUILayout.Label(_isRuntime ? "RUNTIME MODE" : "EDITOR MODE");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Label(_title, TweenManager.isDebugBuild ? EditorGUIUtils.redLabelStyle : EditorGUIUtils.boldLabelStyle);

            if (!_isRuntime) {
                GUI.backgroundColor = new Color(0f, 0.31f, 0.48f);
                GUI.contentColor = Color.white;
                GUILayout.Label(
                    "This component is <b>added automatically</b> by DOTween at runtime." +
                    "\nAdding it yourself is <b>not recommended</b> unless you really know what you're doing:" +
                    " you'll have to be sure it's <b>never destroyed</b> and that it's present <b>in every scene</b>.",
                    EditorGUIUtils.infoboxStyle
                );
                GUI.backgroundColor = GUI.contentColor = GUI.contentColor = Color.white;
            }

            GUILayout.Space(6);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Documentation")) Application.OpenURL("http://dotween.demigiant.com/documentation.php");
            if (GUILayout.Button("Check Updates")) Application.OpenURL("http://dotween.demigiant.com/download.php?v=" + DOTween.Version);
            GUILayout.EndHorizontal();

            if (_isRuntime) {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(_settings.showPlayingTweens ? "Hide Playing Tweens" : "Show Playing Tweens")) {
                    _settings.showPlayingTweens = !_settings.showPlayingTweens;
                    EditorUtility.SetDirty(_settings);
                }
                if (GUILayout.Button(_settings.showPausedTweens ? "Hide Paused Tweens" : "Show Paused Tweens")) {
                    _settings.showPausedTweens = !_settings.showPausedTweens;
                    EditorUtility.SetDirty(_settings);
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Play all")) DOTween.PlayAll();
                if (GUILayout.Button("Pause all")) DOTween.PauseAll();
                if (GUILayout.Button("Kill all")) DOTween.KillAll();
                GUILayout.EndHorizontal();

                int totActiveTweens = TweenManager.totActiveTweens;
                int totPlayingTweens = TweenManager.TotalPlayingTweens();
                int totPausedTweens = totActiveTweens - totPlayingTweens;
                int totActiveDefaultTweens = TweenManager.totActiveDefaultTweens;
                int totActiveLateTweens = TweenManager.totActiveLateTweens;
                int totActiveFixedTweens = TweenManager.totActiveFixedTweens;
                int totActiveManualTweens = TweenManager.totActiveManualTweens;

                _strb.Length = 0;
                _strb.Append("Active tweens: ").Append(totActiveTweens)
                    .Append(" (").Append(TweenManager.totActiveTweeners).Append(" TW, ")
                    .Append(TweenManager.totActiveSequences).Append(" SE)")
                    .Append("\nDefault/Late/Fixed/Manual tweens: ").Append(totActiveDefaultTweens)
                    .Append("/").Append(totActiveLateTweens)
                    .Append("/").Append(totActiveFixedTweens)
                    .Append("/").Append(totActiveManualTweens);
                GUILayout.Label(_strb.ToString(), EditorGUIUtils.wordWrapRichTextLabelStyle);

                if (_settings.showPlayingTweens || _settings.showPausedTweens) {
                    GUILayout.Space(8);
                    GUILayout.Label("<b>Legend: </b> TW = Tweener, SE = Sequence", EditorGUIUtils.wordWrapRichTextLabelStyle);
                    // DrawSimpleTweensList();
                    DrawTweensButtons(totPlayingTweens, totPausedTweens);
                }

                GUILayout.Space(2);
                _strb.Length = 0;
                _strb.Append("Pooled tweens: ").Append(TweenManager.TotalPooledTweens())
                    .Append(" (").Append(TweenManager.totPooledTweeners).Append(" TW, ")
                    .Append(TweenManager.totPooledSequences).Append(" SE)");
                GUILayout.Label(_strb.ToString(), EditorGUIUtils.wordWrapRichTextLabelStyle);

                GUILayout.Space(2);
                _strb.Remove(0, _strb.Length);
                _strb.Append("Tweens Capacity: ").Append(TweenManager.maxTweeners).Append(" TW, ").Append(TweenManager.maxSequences).Append(" SE")
                    .Append("\nMax Simultaneous Active Tweens: ").Append(DOTween.maxActiveTweenersReached).Append(" TW, ")
                    .Append(DOTween.maxActiveSequencesReached).Append(" SE");
                GUILayout.Label(_strb.ToString(), EditorGUIUtils.wordWrapRichTextLabelStyle);
            }

            GUILayout.Space(8);
            _strb.Remove(0, _strb.Length);
            _strb.Append("<b>SETTINGS ▼</b>");
            _strb.Append("\nSafe Mode: ").Append((_isRuntime ? DOTween.useSafeMode : _settings.useSafeMode) ? "ON" : "OFF");
            _strb.Append("\nLog Behaviour: ").Append(_isRuntime ? DOTween.logBehaviour : _settings.logBehaviour);
            _strb.Append("\nShow Unity Editor Report: ").Append(_isRuntime ? DOTween.showUnityEditorReport : _settings.showUnityEditorReport);
            _strb.Append("\nTimeScale (Unity/DOTween): ").Append(Time.timeScale).Append("/").Append(_isRuntime ? DOTween.timeScale : _settings.timeScale);
            GUILayout.Label(_strb.ToString(), EditorGUIUtils.wordWrapRichTextLabelStyle);
            GUILayout.Label(
                "NOTE: DOTween's TimeScale is not the same as Unity's Time.timeScale: it is actually multiplied by it except for tweens that are set to update independently",
                EditorGUIUtils.wordWrapRichTextLabelStyle
            );

            GUILayout.Space(8);
            _strb.Remove(0, _strb.Length);
            _strb.Append("<b>DEFAULTS ▼</b>");
            _strb.Append("\ndefaultRecyclable: ").Append(_isRuntime ? DOTween.defaultRecyclable : _settings.defaultRecyclable);
            _strb.Append("\ndefaultUpdateType: ").Append(_isRuntime ? DOTween.defaultUpdateType : _settings.defaultUpdateType);
            _strb.Append("\ndefaultTSIndependent: ").Append(_isRuntime ? DOTween.defaultTimeScaleIndependent : _settings.defaultTimeScaleIndependent);
            _strb.Append("\ndefaultAutoKill: ").Append(_isRuntime ? DOTween.defaultAutoKill : _settings.defaultAutoKill);
            _strb.Append("\ndefaultAutoPlay: ").Append(_isRuntime ? DOTween.defaultAutoPlay : _settings.defaultAutoPlay);
            _strb.Append("\ndefaultEaseType: ").Append(_isRuntime ? DOTween.defaultEaseType : _settings.defaultEaseType);
            _strb.Append("\ndefaultLoopType: ").Append(_isRuntime ? DOTween.defaultLoopType : _settings.defaultLoopType);
            GUILayout.Label(_strb.ToString(), EditorGUIUtils.wordWrapRichTextLabelStyle);

            GUILayout.Space(10);
        }

        #endregion

        #region Methods

        void ConnectToSource(bool forceReconnection = false)
        {
            _headerImg = AssetDatabase.LoadAssetAtPath("Assets/" + EditorUtils.editorADBDir + "Imgs/DOTweenIcon.png", typeof(Texture2D)) as Texture2D;

            if (_settings == null || forceReconnection) {
                _settings = _isRuntime
                    ? Resources.Load(DOTweenSettings.AssetName) as DOTweenSettings
                    : DOTweenUtilityWindow.GetDOTweenSettings();
            }
        }

        void DrawTweensButtons(int totPlayingTweens, int totPausedTweens)
        {
            if (_settings.showPlayingTweens) {
                _strb.Length = 0;
                _strb.Append(_playingTweensHex).Append("Playing tweens: ").Append(totPlayingTweens).Append("</color>");
                GUILayout.Label(_strb.ToString(), EditorGUIUtils.wordWrapRichTextLabelStyle);
                foreach (Tween t in TweenManager._activeTweens) {
                    if (t == null || !t.isPlaying) continue;
                    DrawTweenButton(t, true);
                }
            }
            if (_settings.showPausedTweens) {
                _strb.Length = 0;
                _strb.Append(_pausedTweensHex).Append("Paused tweens: ").Append(totPausedTweens).Append("</color>");
                GUILayout.Label(_strb.ToString(), EditorGUIUtils.wordWrapRichTextLabelStyle);
                foreach (Tween t in TweenManager._activeTweens) {
                    if (t == null || t.isPlaying) continue;
                    DrawTweenButton(t, false);
                }
            }
        }

        void DrawTweenButton(Tween tween, bool isPlaying, bool isSequenced = false, int sequencedDepth = 0)
        {
            _strb.Length = 0;
            if (!isSequenced) {
                _strb.Append(isPlaying ? _playingTweensHex : _pausedTweensHex);
                _strb.Append(tween.isPlaying ? "► </color>" : "❚❚ </color>");
            }
            else {
                int spaces = sequencedDepth;
                while (spaces > 0) {
                    spaces--;
                    _strb.Append("     ");
                }
                _strb.Append("└ ");
            }
            _strb.Append("[").Append(tween.tweenType == TweenType.Sequence ? "SE" : "TW");
            AppendTweenIdLabel(_strb, tween);
            _strb.Append("]");
            AppendDebugTargetIdLabel(_strb, tween);
            AppendTargetTypeLabel(_strb, tween.target);
            switch (tween.tweenType) {
            case TweenType.Tweener:
                if (GUILayout.Button(_strb.ToString(), isSequenced ? EditorGUIUtils.btSequencedStyle : EditorGUIUtils.btTweenStyle)) {
                    Object tweenTarget = tween.target as Object;
                    if (tweenTarget != null)  EditorGUIUtility.PingObject(tweenTarget);
                }
                break;
            case TweenType.Sequence:
                GUILayout.Button(_strb.ToString(), isSequenced ? EditorGUIUtils.btSequencedStyle : EditorGUIUtils.btSequenceStyle);
                Sequence s = (Sequence)tween;
                sequencedDepth++;
                foreach (Tween t in s.sequencedTweens) {
                    DrawTweenButton(t, isPlaying, true, sequencedDepth);
                }
                break;
            }
        }

        // Old method now replaced with DrawTweensButtons
        // void DrawSimpleTweensList()
        // {
        //     int totActiveTweens = TweenManager.totActiveTweens;
        //     int totPlayingTweens = TweenManager.TotalPlayingTweens();
        //     int totPausedTweens = totActiveTweens - totPlayingTweens;
        //     int totActiveDefaultTweens = TweenManager.totActiveDefaultTweens;
        //     int totActiveLateTweens = TweenManager.totActiveLateTweens;
        //     int totActiveFixedTweens = TweenManager.totActiveFixedTweens;
        //     int totActiveManualTweens = TweenManager.totActiveManualTweens;
        //     _strb.Length = 0;
        //     _strb.Append("Active tweens: ").Append(totActiveTweens)
        //         .Append(" (").Append(TweenManager.totActiveTweeners).Append(" TW, ")
        //         .Append(TweenManager.totActiveSequences).Append(" SE)")
        //         .Append("\nDefault/Late/Fixed/Manual tweens: ").Append(totActiveDefaultTweens)
        //         .Append("/").Append(totActiveLateTweens)
        //         .Append("/").Append(totActiveFixedTweens)
        //         .Append("/").Append(totActiveManualTweens)
        //         .Append(_playingTweensHex).Append("\nPlaying tweens: ").Append(totPlayingTweens);
        //     if (_settings.showPlayingTweens) {
        //         foreach (Tween t in TweenManager._activeTweens) {
        //             if (t == null || !t.isPlaying) continue;
        //             _strb.Append("\n   - [").Append(t.tweenType == TweenType.Tweener ? "TW" : "SE");
        //             AppendTweenIdLabel(_strb, t);
        //             _strb.Append("]");
        //             AppendDebugTargetIdLabel(_strb, t);
        //             AppendTargetTypeLabel(_strb, t.target);
        //         }
        //     }
        //     _strb.Append("</color>");
        //     _strb.Append(_pausedTweensHex).Append("\nPaused tweens: ").Append(totPausedTweens);
        //     if (_settings.showPausedTweens) {
        //         foreach (Tween t in TweenManager._activeTweens) {
        //             if (t == null || t.isPlaying) continue;
        //             _strb.Append("\n   - [").Append(t.tweenType == TweenType.Tweener ? "TW" : "SE");
        //             AppendTweenIdLabel(_strb, t);
        //             _strb.Append("]");
        //             AppendDebugTargetIdLabel(_strb, t);
        //             AppendTargetTypeLabel(_strb, t.target);
        //         }
        //     }
        //     _strb.Append("</color>");
        //     _strb.Append("\nPooled tweens: ").Append(TweenManager.TotalPooledTweens())
        //         .Append(" (").Append(TweenManager.totPooledTweeners).Append(" TW, ")
        //         .Append(TweenManager.totPooledSequences).Append(" SE)");
        //     GUILayout.Label(_strb.ToString(), EditorGUIUtils.wordWrapRichTextLabelStyle);
        //
        //     GUILayout.Space(8);
        //     _strb.Remove(0, _strb.Length);
        //     _strb.Append("Tweens Capacity: ").Append(TweenManager.maxTweeners).Append(" TW, ").Append(TweenManager.maxSequences).Append(" SE")
        //         .Append("\nMax Simultaneous Active Tweens: ").Append(DOTween.maxActiveTweenersReached).Append(" TW, ")
        //         .Append(DOTween.maxActiveSequencesReached).Append(" SE");
        //     GUILayout.Label(_strb.ToString(), EditorGUIUtils.wordWrapRichTextLabelStyle);
        // }

        #endregion

        #region Helpers

        void AppendTweenIdLabel(StringBuilder strb, Tween t)
        {
            if (!string.IsNullOrEmpty(t.stringId)) strb.Append(":<b>").Append(t.stringId).Append("</b>");
            else if (t.intId != -999) strb.Append(":<b>").Append(t.intId).Append("</b>");
            else if (t.id != null) strb.Append(":<b>").Append(t.id).Append("</b>");
        }

        void AppendDebugTargetIdLabel(StringBuilder strb, Tween t)
        {
            if (string.IsNullOrEmpty(t.debugTargetId)) return;
            strb.Append(" \"<b>").Append(t.debugTargetId).Append("</b>\"");
        }

        void AppendTargetTypeLabel(StringBuilder strb, object tweenTarget)
        {
            if (tweenTarget == null) return;
            strb.Append(' ');
            string s = tweenTarget.ToString();
            if (s == "null") {
                _strb.Append("<b><color=#ff0000>×</color></b>");
            } else {
                strb.Append("<i>(");
                int dotIndex = s.LastIndexOf('.');
                if (dotIndex == -1) {
                    strb.Append(s).Append(')');
                } else {
                    strb.Append(s.Substring(dotIndex + 1));
                }
                strb.Append("</i>");
            }
        }

        #endregion
    }
}