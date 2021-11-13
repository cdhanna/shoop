using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameBoardGeneratorObject))]
public class GameBoardGeneratorObjectEditor : Editor
{
    public Vector2 _boardScroll;

    public List<GameBoardObjectOpen> _foldouts = new List<GameBoardObjectOpen>();

    [Serializable]
    public class GameBoardObjectOpen
    {
        public GameBoardObject board;
        public bool Open;
    }
    
    public override void OnInspectorGUI()
    {
        var gen = this.target as GameBoardGeneratorObject;
        if (gen == null) return;

        var path = AssetDatabase.GetAssetPath(gen);
        
        base.OnInspectorGUI();
        
        
        var shouldGenerate = GUILayout.Button("Generate!");

        var shouldEraseAll = GUILayout.Button("Clear all");

        if (shouldGenerate)
        {
            var board = gen.Generate();
            board.name = "Sample";
            AssetDatabase.AddObjectToAsset(board, gen);
            EditorUtility.SetDirty(gen);
            AssetDatabase.ImportAsset(path);

        }

        var boards = AssetDatabase.LoadAllAssetsAtPath(path).Where(a => a is GameBoardObject).Cast<GameBoardObject>().ToList();

        _boardScroll = EditorGUILayout.BeginScrollView(_boardScroll);
        EditorGUILayout.BeginVertical();
        foreach (var board in boards)
        {
            EditorGUILayout.BeginHorizontal();
            var foldout = _foldouts.FirstOrDefault(b => b.board == board);

            if (foldout == null)
            {
                foldout = new GameBoardObjectOpen {board = board, Open = false};
                _foldouts.Add(foldout);
            }
            foldout.Open = EditorGUILayout.Foldout(foldout.Open, $"{board.name}", true);

            EditorGUI.BeginChangeCheck();
            var nextName = EditorGUILayout.DelayedTextField(board.name);
            if (EditorGUI.EndChangeCheck())
            {
                board.name = nextName;
                EditorUtility.SetDirty(board);
                EditorUtility.SetDirty(gen);
                AssetDatabase.ImportAsset(path);
            }
            if (GUILayout.Button("delete"))
            {
                AssetDatabase.RemoveObjectFromAsset(board);
                EditorUtility.SetDirty(gen);
                AssetDatabase.ImportAsset(path);
            }
            EditorGUILayout.EndHorizontal();

            
            if (!foldout.Open) continue;

            EditorGUI.indentLevel++;
            var shouldRegen = GUILayout.Button("Regenerate!");
            if (shouldRegen)
            {
                var nextBoard = gen.Generate(board.Seed);
                board.Requirements = nextBoard.Requirements;
                board.Slots = nextBoard.Slots;
                board.Seed = nextBoard.Seed;
                board.PerfectMoveCount = nextBoard.PerfectMoveCount;
                EditorUtility.SetDirty(board);
                EditorUtility.SetDirty(gen);
                AssetDatabase.ImportAsset(path);
            }

            if (GUILayout.Button("Recompute Win Count"))
            {
                board.PerfectMoveCount = GameBoardAI.CountMovesToWinInstant(board);
                EditorUtility.SetDirty(board);
                EditorUtility.SetDirty(gen);
                AssetDatabase.ImportAsset(path);
            }
            
            EditorGUILayout.ObjectField(board, typeof(GameBoardObject), gen);
            var subEditor = Editor.CreateEditor(board);
            subEditor.OnInspectorGUI();
            EditorGUI.indentLevel--;


        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
        
        if (shouldEraseAll)
        {
            boards = AssetDatabase.LoadAllAssetsAtPath(path).Where(a => a is GameBoardObject).Cast<GameBoardObject>().ToList();
            foreach (var board in boards)
            {
                AssetDatabase.RemoveObjectFromAsset(board);
            }
            EditorUtility.SetDirty(gen);
            AssetDatabase.ImportAsset(path);
        }
        

    }
}
