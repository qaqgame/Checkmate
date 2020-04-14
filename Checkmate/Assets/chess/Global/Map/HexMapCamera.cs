using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Checkmate.Game
{

    public class HexMapCamera : MonoBehaviour
    {
        Transform swivel, stick;//前者控制旋转，后者控制位移
        public float stickMinZoom = -250, stickMaxZoom = -45;//等同于调整stick的z轴
        public float swivelMinZoon = 90, swivelMaxZoom = 45;//旋转的限制

        public float moveSpeedMinZoom = 400, moveSpeedMaxZoom = 100;//移动速度(视距最远和最近时)
        public float rotationSpeed = 180;//旋转速度
        public HexGrid grid;//所操作的地图
        float zoom = 1f;//缩放
        float rotationAngle;//旋转角度
        private void Awake()
        {
            swivel = transform.GetChild(0);
            stick = swivel.GetChild(0);
        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
            if (zoomDelta != 0f)
            {
                AdjustZoom(zoomDelta);
            }

            float rotationDelta = Input.GetAxis("Rotation");
            if (rotationDelta != 0f)
            {
                AdjustRotation(rotationDelta);
            }

            float xDelta = Input.GetAxis("Horizontal");
            float zDelta = Input.GetAxis("Vertical");
            if (xDelta != 0f || zDelta != 0f)
            {
                AdjustPosition(xDelta, zDelta);
            }
        }

        //调整缩放
        private void AdjustZoom(float delta)
        {
            zoom = Mathf.Clamp01(zoom + delta);

            float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
            stick.localPosition = new Vector3(0f, 0f, distance);

            float angle = Mathf.Lerp(swivelMinZoon, swivelMaxZoom, zoom);
            swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
        }

        //调整位置
        private void AdjustPosition(float xDelta, float zDelta)
        {
            Vector3 direction = transform.localRotation * new Vector3(xDelta, 0f, zDelta).normalized;
            float dampling = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));//阻尼
            float distance = Mathf.Lerp(moveSpeedMinZoom, moveSpeedMaxZoom, zoom) * dampling * Time.deltaTime;

            Vector3 position = transform.localPosition;
            position += direction * distance;
            transform.localPosition = ClampPosition(position);
        }
        //约束位置在地图范围内
        private Vector3 ClampPosition(Vector3 position)
        {
            float xMax =
                (grid.chunkCountX * HexMetrics.chunkSizeX - 1) *
                (1.5f * HexMetrics.outerRadius);
            position.x = Mathf.Clamp(position.x, 0f, xMax);

            float zMax =
                (grid.chunkCountZ * HexMetrics.chunkSizeZ - 0.5f) *
                (2f * HexMetrics.innerRadius);
            position.z = Mathf.Clamp(position.z, 0f, zMax);

            return position;
        }

        //调整旋转
        private void AdjustRotation(float delta)
        {
            rotationAngle += delta * rotationSpeed * Time.deltaTime;
            if (rotationAngle < 0f)
            {
                rotationAngle += 360f;
            }
            else if (rotationAngle >= 360f)
            {
                rotationAngle -= 360f;
            }
            transform.localRotation = Quaternion.Euler(0f, rotationAngle, 0f);
        }
    }
}