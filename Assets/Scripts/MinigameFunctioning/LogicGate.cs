using UnityEngine;

public enum GateType
{
    AND,
    OR,
    NOT,
    INVENTORY
}

public class LogicGate : MonoBehaviour
{
    public GateType type;

    public bool Evaluate(bool inputA, bool inputB = false)
    {
        switch (type)
        {
            case GateType.AND:
                return inputA && inputB;

            case GateType.OR:
                return inputA || inputB;

            case GateType.NOT:
                return !inputA;

            default:
                return false;
        }
    }
}