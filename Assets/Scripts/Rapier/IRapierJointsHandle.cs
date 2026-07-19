using System.Collections.Generic;
using UnityEngine;

public interface IRapierJointsHandle
{
    public interface IRapierBody
    {
        List<ulong> JointHandles { get; }
    }
}
