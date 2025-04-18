using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SampleCarController : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] Transform CenterOfMass;
    //Rigidbody.centerOfMassを上書きして、車の重心を決めるためのTransform
    [SerializeField] WheelCollider[] Wheels;
    //Wheel Colliderがアタッチされている各タイヤ
    [SerializeField] Transform[] Obj;
    //タイヤの見た目(AwakeでWheelsから取得するため割当不要)

    [SerializeField] string XAxisName = "Horizontal";
    [SerializeField] string YAxisName = "Vertical";
    [SerializeField] KeyCode BrakeKey = KeyCode.Space;
    //前後移動はVertical、ハンドル操作はHorizontalで行う。(InputManager参照)

    [SerializeField] Vector2 InputVector;
    //キー操作を受け取る
    [SerializeField] float BrakeInput = 0;
    //ブレーキのキー入力を受け取る

    [SerializeField] float AccelPower = 1000f;
    //車のパワー
    [SerializeField] float HandleAngle = 45f;
    //最大でハンドルが切れるタイヤの角度
    [SerializeField] float BrakePower = 1000f;
    //ブレーキの力

    [SerializeField] float[] DriveWheels = new float[] { 0f, 0f, 1.0f, 1.0f };
    //駆動させるタイヤ。0なら駆動しない、1なら駆動する。(パワーでタイヤを回すかどうか。初期値は後輪駆動)
    [SerializeField] float[] SteerWheels = new float[] { 1.0f, 1.0f, 0f, 0f };
    //ハンドル操作で曲がるタイヤ。

    Vector2 touchStartPos;
    Vector2 touchEndPos;
    bool isTouching = false;
    bool adjustingRotation = false;


    // Start is called before the first frame update
    void Awake()
    {
        Wheels = GetComponentsInChildren<WheelCollider>();
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = CenterOfMass.localPosition;

        Obj = new Transform[Wheels.Length];
        for (int i = 0; i < Wheels.Length; i++)
        {
            Obj[i] = Wheels[i].transform.GetChild(0);
        }
    }

    void Start()
    {
        // 摩擦
        for(int i =0; i < Wheels.Length; i++)
        {
            // Wheels[i].ConfigureVehicleSubsteps(5, 12, 15);
            WheelFrictionCurve forwardFriction = Wheels[i].forwardFriction;
            // _friction.extremumSlip = 0.4f; // 最大摩擦力が発生するスリップ角度
            // _friction.extremumValue = 1.0f; // 最大摩擦力
            // _friction.asymptoteSlip = 0.8f; // 摩擦力が減少し始めるスリップ角度
            // _friction.asymptoteValue = 0.5f; // 摩擦力が減少する値
            forwardFriction.stiffness = 1.0f; // 摩擦力の強さ
            Wheels[i].forwardFriction = forwardFriction; // 前輪の摩擦を設定
            
            WheelFrictionCurve sidewaysFriction = Wheels[i].sidewaysFriction;
            sidewaysFriction.stiffness = 1.0f; // 摩擦力の強さ
            Wheels[1].sidewaysFriction = sidewaysFriction; // 前輪の摩擦を設定
        }
    }

    // Update is called once per frame
    void Update()
    {
        ControllInput();

        CarControll();
    }
    private void ControllInput()
    {
        // WASDキー、spaceキーで操作する場合
        // InputVector = new Vector2(Input.GetAxis(XAxisName), Input.GetAxis(YAxisName));
        // BrakeInput = Input.GetKey(BrakeKey) ? BrakePower : 0f;
        // スマホの傾きで操作する場合
        // InputVector = new Vector2(Input.acceleration.x, Input.acceleration.y);
        // BrakeInput = Input.GetKey(BrakeKey) ? BrakePower : 0f;
        
        // 画面タッチ・ドラッグで操作する場合
#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchStartPos = touch.position;
                isTouching = true;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                touchEndPos = touch.position;
                Vector2 delta = touchEndPos - touchStartPos;
                InputVector.x = delta.x / Screen.width * 2; // スクリーン幅に対する割合でハンドル操作
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isTouching = false;
                InputVector.x = 0;
            }

            if (isTouching)
            {
                if (InputVector.y != -1)
                {
                    InputVector.y = 1; // タッチしている間はアクセル
                    // ガクガクにならないように
                    if (rb.velocity.magnitude > ValueController.Instance.MaxSpeed)
                    {
                        InputVector.y = 0;
                    }
                
                    // Debug.Log("InputVector.x:" + InputVector.x);
                    // ハンドル操作が一定以上ない場合、ハンドル無効に
                    if (Mathf.Abs(InputVector.x) < ValueController.Instance.SwipeDetectThreshold)
                    {
                        InputVector.x = 0;
                    }
                    if (Mathf.Abs(InputVector.x) > ValueController.Instance.SteeringThreshold)
                    {
                        // Debug.Log("rb.Velocity mg:" + rb.velocity.magnitude);
                        if (rb.velocity.magnitude > 3)
                        {
                            BrakeInput = ValueController.Instance.HandleBrakePower;
                            // Debug.Log("HandleBrakePower:" + BrakeInput);
                            return;
                        }
                        BrakeInput = 0;
                        return;
                    }
                }
            }
            else
            {
                InputVector.y = 0;
            }
        }
        else
        {
            InputVector = Vector2.zero;   

        }
#endif
#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
            if (Input.GetMouseButtonDown(0))
            {
                touchStartPos = Input.mousePosition;
                isTouching = true;
            }
            else if (Input.GetMouseButton(0))
            {
                touchEndPos = Input.mousePosition;
                Vector2 delta = (Vector2)touchEndPos - touchStartPos;
                InputVector.x = delta.x / Screen.width * 2; // スクリーン幅に対する割合でハンドル操作
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isTouching = false;
                InputVector.x = 0;
            }

            if (isTouching)
            {
                if (InputVector.y != -1)
                {
                    InputVector.y = 1; // クリックしている間はアクセル
                    // ガクガクにならないように
                    if (rb.velocity.magnitude > ValueController.Instance.MaxSpeed)
                    {
                        InputVector.y = 0;
                    }

                    // Debug.Log("InputVector.x:" + InputVector.x);
                    // ハンドル操作が一定以上ない場合、ハンドル無効に
                    if (Mathf.Abs(InputVector.x) < ValueController.Instance.SwipeDetectThreshold)
                    {
                        InputVector.x = 0;
                    }

                    if (Mathf.Abs(InputVector.x) > ValueController.Instance.SteeringThreshold)
                    {
                        // Debug.Log("rb.Velocity mg:" + rb.velocity.magnitude);
                        if (rb.velocity.magnitude > 3)
                        {
                            BrakeInput = ValueController.Instance.HandleBrakePower;
                            // Debug.Log("HandleBrakePower:" + BrakeInput);
                            return;
                        }
                        BrakeInput = 0;
                        return;
                    }
                }
            }
            else
            {
                InputVector.y = 0;
            }
        }
        else
        {
            InputVector = Vector2.zero;
        }
#endif
        // Spaceキーを押している間はInputVector.yを-1にする
        // if (Input.GetKey(KeyCode.Space))
        // {
        //     InputVector.y = -1;
        // }

        // 自動ブレーキ？
        // if (!isTouching && rb.velocity.magnitude > 1)
        if (Input.GetMouseButton(0) == false)
        {
            BrakeInput = ValueController.Instance.BrakePower;
        }
        else
        {
            BrakeInput = 0;
        }
    }

    private void CarControll()
    {
        // if (adjustingRotation) return;
        // 最高速度を超えないように速度を制限
        if (rb.velocity.magnitude > ValueController.Instance.MaxSpeed)
        {
            rb.velocity = rb.velocity.normalized * ValueController.Instance.MaxSpeed;
        }

        for (int i = 0; i < Wheels.Length; i++)
        {
            Wheels[i].motorTorque = InputVector.y * DriveWheels[i] * ValueController.Instance.AccelPower;
            Wheels[i].steerAngle = InputVector.x * SteerWheels[i] * ValueController.Instance.HandleAngle;
            Wheels[i].brakeTorque = BrakeInput;
        }

        Vector3 _pos;
        Quaternion _dir;
        Wheels[0].GetWorldPose(out _pos, out _dir);
        Obj[0].position = _pos;
        Obj[0].rotation = _dir;
    }
    // private void CarControll()
    // {
    //     for (int i = 0; i < Wheels.Length; i++)
    //     {
    //         Wheels[i].motorTorque = InputVector.y * DriveWheels[i] * AccelPower;
    //         Wheels[i].steerAngle = InputVector.x * SteerWheels[i] * HandleAngle;
    //         Wheels[i].brakeTorque = BrakeInput;

    //         Vector3 _pos;
    //         Quaternion _dir;
    //         Wheels[i].GetWorldPose(out _pos, out _dir);
    //         Obj[i].position = _pos;
    //         Obj[i].rotation = _dir;
    //     }
    // }
}
