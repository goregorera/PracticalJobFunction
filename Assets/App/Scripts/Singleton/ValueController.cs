using UnityEngine;
using System.Collections.Generic;

public class ValueController :SingletonMonoBehaviour<ValueController>
{
    public float SwipeDetectThreshold = 0.1f;
    public float MaxSpeed = 5f;
    public float AccelPower = 1000f;
    public float HandleAngle = 45f;
    public float ForwardFrictionStifness = 1f;
    public float SidewaysFrictionStifness = 1f;
    public float BrakePower = 40f;

    public float SteeringThreshold = 0.1f;
    public float ReducedAccelPower = 0.2f;
    public float HandleBrakePower = 40f;
    
    // public static ValueController Instance = null;

    void Awake()
    {
        // シングルトンだが、データリセットでシーンをタイトルに戻すときに
        // 破壊しない方がいいかもしれないので一旦コメントアウト
        // if (Instance != null) 
        // {
        //     Destroy(this.gameObject); //重複しないように破壊
        //     return;
        // }
        DontDestroyOnLoad(this.gameObject); 
        // Instance = this;   
    }
}
