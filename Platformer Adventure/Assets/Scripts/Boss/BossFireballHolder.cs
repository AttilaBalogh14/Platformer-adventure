using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFireballHolder : MonoBehaviour
{
    [SerializeField] private Transform boss;

    void Update()
    {
        transform.localScale = boss.localScale;
    }
}
