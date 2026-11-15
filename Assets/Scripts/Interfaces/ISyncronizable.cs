using UnityEngine;

public interface ISyncronizable
{
    public void Tick(short relativeTick);
    void ROLLBACKsave();
    void ROLLBACKrestore();
}
