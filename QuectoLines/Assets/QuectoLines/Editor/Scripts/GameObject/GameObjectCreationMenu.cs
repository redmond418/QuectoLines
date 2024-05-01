using UnityEditor;
using UnityEngine;

internal static class GameObjectCreationMenu
{
    private static void CreatePrefab(MenuCommand menuCommand, string prefabName)
    {
        var prefab = Resources.Load<GameObject>(prefabName);
        Debug.Log(prefab);
        if (prefab == null) return;
        GameObject obj = GameObject.Instantiate(prefab);
        obj.name = prefab.name;
        GameObjectUtility.SetParentAndAlign(obj, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(obj, "Create " + obj.name);
        Selection.activeObject = obj;
    }

    [MenuItem("GameObject/Quecto Lines/Spline/Circle")]
    private static void CreateSplineCircle(MenuCommand menuCommand)
        => CreatePrefab(menuCommand, "Prefabs/Splines/Spline Circle");

    [MenuItem("GameObject/Quecto Lines/Spline/Triangle")]
    private static void CreateSplineTriangle(MenuCommand menuCommand)
        => CreatePrefab(menuCommand, "Prefabs/Splines/Spline Triangle");

    [MenuItem("GameObject/Quecto Lines/Spline/Square")]
    private static void CreateSplineSquare(MenuCommand menuCommand)
        => CreatePrefab(menuCommand, "Prefabs/Splines/Spline Square");

    [MenuItem("GameObject/Quecto Lines/Spline/Hexagon")]
    private static void CreateSplineHexagon(MenuCommand menuCommand)
        => CreatePrefab(menuCommand, "Prefabs/Splines/Spline Hexagon");

    [MenuItem("GameObject/Quecto Lines/Line Generator/Default")]
    private static void CreateLineDefault(MenuCommand menuCommand)
        => CreatePrefab(menuCommand, "Prefabs/Lines/Line Default");

    [MenuItem("GameObject/Quecto Lines/Line Generator/Two Points")]
    private static void CreateLineWithTwoPoints(MenuCommand menuCommand)
        => CreatePrefab(menuCommand, "Prefabs/Lines/Line Two Points");

    [MenuItem("GameObject/Quecto Lines/Line Generator/Spline")]
    private static void CreateLineWithSpline(MenuCommand menuCommand)
        => CreatePrefab(menuCommand, "Prefabs/Lines/Line Spline");
}
