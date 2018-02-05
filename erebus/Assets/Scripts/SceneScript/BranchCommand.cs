using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Assertions;

public class BranchCommand : SceneCommand {

    private enum ComparisonType {
        LessThan,
        LessThanOrEqual,
        Equal,
        GreaterThanOrEqual,
        GreaterThan
    };

    public string TrueSceneName { get; set; }
    public string FalseSceneName { get; set; }

    private string variableName;
    private ComparisonType sign;
    private int threshold;

    public BranchCommand(string variableName, string sign, string threshold) {
        this.variableName = variableName;
        this.threshold = Int32.Parse(threshold);
        switch (sign) {
            case "<":       this.sign = ComparisonType.LessThan;            break;
            case "<=":      this.sign = ComparisonType.LessThanOrEqual;     break;
            case "==":      this.sign = ComparisonType.Equal;               break;
            case ">=":      this.sign = ComparisonType.GreaterThanOrEqual;  break;
            case ">":       this.sign = ComparisonType.GreaterThan;         break;
            default:
                Assert.IsTrue(false, "Unknown sign: " + sign);
                break;
        }
    }

    public override IEnumerator PerformAction(ScenePlayer player) {
        int variableValue = Global.Instance().memory.GetVariable(variableName);
        bool result = false;
        switch (sign) {
            case ComparisonType.LessThan:           result = variableValue < threshold;     break;
            case ComparisonType.LessThanOrEqual:    result = variableValue <= threshold;    break;
            case ComparisonType.Equal:              result = variableValue == threshold;    break;
            case ComparisonType.GreaterThanOrEqual: result = variableValue >= threshold;    break;
            case ComparisonType.GreaterThan:        result = variableValue > threshold;     break;
        }

        string sceneToPlay = result ? TrueSceneName : FalseSceneName;
        yield return player.StartCoroutine(player.PlayScriptForScene(sceneToPlay));
    }
}
