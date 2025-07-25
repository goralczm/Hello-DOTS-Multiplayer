using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

public struct FloatAnimatorParameterRpcCommand : IRpcCommand
{
    public FixedString64Bytes ParameterName;
    public float ParameterValue;
    public int NetworkId;
}
