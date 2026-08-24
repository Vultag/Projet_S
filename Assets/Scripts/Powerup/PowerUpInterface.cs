using UnityEngine;

public interface PowerUpInterface
{


    void RestorePowerUpState(bool OnOrOff);
    void SavePowerUpState();

    void PowerUpSelect();
    void PowerUpDeselect();
    void PowerUpEnable();
    void PowerUpDisable();

    void PowerUpAim(Vector2 dir);
    void PowerUpAction1(Vector2 delta);
    void PowerUpAction2(Vector2 delta);




}
