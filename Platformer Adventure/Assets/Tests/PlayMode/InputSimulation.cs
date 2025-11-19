using UnityEngine;

public static class InputSimulation
{
    public static float horizontal = 0f;

    public static float GetAxisRaw(string axis)
    {
        if (axis == "Horizontal")
            return horizontal;

        return 0f;
    }
}
