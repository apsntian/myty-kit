using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine.U2D.Animation;

public class Sprite1DRangeConEditorWindow : EditorWindow
{
    public VisualTreeAsset UITemplate;


    private SerializedObject _conSO;

    [MenuItem("MYTY Kit/Controller/Sprite 1D Range Controller", false,20)]
    public static void ShowController()
    {
        var wnd = GetWindow<Sprite1DRangeConEditorWindow>();
        wnd.titleContent = new GUIContent("Sprite 1D Range Controller");
    }

    private void CreateGUI()
    {
        UITemplate.CloneTree(rootVisualElement);
        var selectedGOs = Selection.GetFiltered<Sprite1DRangeController>(SelectionMode.Editable);
        var conVE = rootVisualElement.Q<ObjectField>("OBJController");
        var listView = rootVisualElement.Q<ListView>("LSTSpriteGO");
        var addBtn = rootVisualElement.Q<Button>("BTNAdd");
        var removeBtn = rootVisualElement.Q<Button>("BTNRemove");
        var removeAllBtn = rootVisualElement.Q<Button>("BTNRemoveAll");
        var valueSlider = rootVisualElement.Q<Slider>("SLDValue");


        addBtn.clicked += AddSelections;
        removeBtn.clicked += Remove;
        removeAllBtn.clicked += RemoveAll;

        conVE.objectType = typeof(Sprite1DRangeController);
        listView.makeItem = () =>
        {
            var objItem = new ObjectField();

            return new ObjectField();
        };

        listView.bindItem = (e, i) =>
        {
            (e as ObjectField).value = listView.itemsSource[i] as GameObject;
            (e as ObjectField).AddToClassList("noEditableObjField");
        };

        conVE.RegisterValueChangedCallback((ChangeEvent<Object> e) =>
        {
            InitWithController(e.newValue as Sprite1DRangeController);
        });

        valueSlider.RegisterValueChangedCallback((ChangeEvent<float> e) =>
        {
            var con = _conSO.targetObject as Sprite1DRangeController;
            con.UpdateLabel();
        });

        rootVisualElement.Q<Button>("BTNAutoSetup").clicked += AutoSetup;

        if (selectedGOs.Length ==0) return;
        
        InitWithController(selectedGOs[0]);
    }

    void InitWithController(Sprite1DRangeController controller)
    {
        _conSO = new SerializedObject(controller);

        var conVE = rootVisualElement.Q<ObjectField>("OBJController");

        var intervalField = rootVisualElement.Q<PropertyField>("PRPIntervals");
        var minVE = rootVisualElement.Q<FloatField>("FLTMin");
        var maxVE = rootVisualElement.Q<FloatField>("FLTMax");
        var currLabelTxt = rootVisualElement.Q<TextField>("TXTCurrent");
        var listView = rootVisualElement.Q<ListView>("LSTSpriteGO");
        var listSource = new List<GameObject>();
        var valueSlider = rootVisualElement.Q<Slider>("SLDValue");

        var spritesProps = _conSO.FindProperty("spriteObjects");
        for (int i = 0; i < spritesProps.arraySize; i++)
        {
            if(spritesProps.GetArrayElementAtIndex(i).objectReferenceValue == null){
                listSource.Add(null);
            }else listSource.Add((spritesProps.GetArrayElementAtIndex(i).objectReferenceValue as SpriteResolver).gameObject);
        }

        listView.itemsSource = listSource;
        listView.Rebuild();

        conVE.value = controller;
        

        intervalField.BindProperty(_conSO.FindProperty("intervals"));
        minVE.BindProperty(_conSO.FindProperty("min"));
        maxVE.BindProperty(_conSO.FindProperty("max"));
        valueSlider.BindProperty(_conSO.FindProperty("value"));
        currLabelTxt.BindProperty(_conSO.FindProperty("currentLabel"));

       
    }

    void AddSelections()
    {
        var sprites = Selection.GetFiltered<SpriteResolver>(SelectionMode.Editable);
        var spritesProps = _conSO.FindProperty("spriteObjects");
        var newSource = new List<GameObject>();

        spritesProps.arraySize = sprites.Length;
        for (int i = 0; i < sprites.Length; i++)
        {
            spritesProps.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
            newSource.Add(sprites[i].gameObject);
        }

        _conSO.ApplyModifiedProperties();

        rootVisualElement.Q<ListView>("LSTSpriteGO").itemsSource = newSource;
        rootVisualElement.Q<ListView>("LSTSpriteGO").Rebuild();
    }

    void Remove()
    {
        var spritesProps = _conSO.FindProperty("spriteObjects");
        var listView = rootVisualElement.Q<ListView>("LSTSpriteGO");
        var willRemove = listView.selectedIndices.ToList();
        var newList = new List<SpriteResolver>();
        var newSource = new List<GameObject>();
        if (willRemove.Count == 0) return;

        for (int i = 0; i < spritesProps.arraySize; i++)
        {
            if (willRemove.Contains(i)) continue;
            newList.Add(spritesProps.GetArrayElementAtIndex(i).objectReferenceValue as SpriteResolver);
        }

        spritesProps.arraySize = newList.Count();
        for (int i = 0; i < spritesProps.arraySize; i++)
        {
            spritesProps.GetArrayElementAtIndex(i).objectReferenceValue = newList[i];
            newSource.Add(newList[i].gameObject);
        }

        listView.itemsSource = newSource;
        listView.Rebuild();
        _conSO.ApplyModifiedProperties();
    }

    void RemoveAll()
    {
        var spritesProps = _conSO.FindProperty("spriteObjects");
        var listView = rootVisualElement.Q<ListView>("LSTSpriteGO");
        spritesProps.arraySize = 0;
        listView.itemsSource = new List<GameObject>();
        listView.Rebuild();
        _conSO.ApplyModifiedProperties();
    }

    void AutoSetup()
    {
        var spritesProp = _conSO.FindProperty("spriteObjects");
        var intervalsProp = _conSO.FindProperty("intervals");
        SortedSet<string> history = null;
        
        for(int i = 0;i < spritesProp.arraySize; i++)
        {
            var spriteResolver = spritesProp.GetArrayElementAtIndex(i).objectReferenceValue as SpriteResolver;
            var cat = spriteResolver.GetCategory();
            var labelIter = spriteResolver.spriteLibrary.spriteLibraryAsset.GetCategoryLabelNames(cat);
            var labelSet = new SortedSet<string>();
            foreach(var label in labelIter)
            {
                labelSet.Add(label);
            }
            if (history == null) history = labelSet;
            else
            {
                if(!CompareSet(history, labelSet))
                {
                    EditorUtility.DisplayDialog("MYTY Kit", "Labels are not match : "+ spriteResolver.gameObject.name, "Ok");
                    return;
                }
                
            }
        }

        intervalsProp.arraySize = history.Count;

        var idx = 0;

        foreach(var label in history)
        {
            intervalsProp.GetArrayElementAtIndex(idx).FindPropertyRelative("label").stringValue = label;
            intervalsProp.GetArrayElementAtIndex(idx).FindPropertyRelative("min").floatValue = 0;
            intervalsProp.GetArrayElementAtIndex(idx).FindPropertyRelative("max").floatValue = 0;
            idx++;
        }
        _conSO.ApplyModifiedProperties();
    }

    private bool CompareSet(SortedSet<string> a, SortedSet<string> b)
    {
        if (a.Count != b.Count) return false;
        var aList = new List<string>();
        var bList = new List<string>();

        foreach(var elem in a)
        {
            aList.Add(elem);
        }

        foreach(var elem in b)
        {
            bList.Add(elem);
        }

        for(int i = 0; i < aList.Count; i++)
        {
            if (aList[i] != bList[i])
            {
                return false;
            }
        }

        return true;
    }
}
