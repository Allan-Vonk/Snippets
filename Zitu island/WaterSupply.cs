using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class WaterSupply : MonoBehaviour
    {
        public Vector3 LowTideOffset;
        private Vector3 LowTide;
        private Vector3 HighTide;
        private bool Filled = true;
        public float drainspeed;
        public GameObject StoneBarrier;
        public WaterEndPoint WaterReceiver;
        private void Start ()
        {
            HighTide = transform.position;
            LowTide = HighTide - LowTideOffset;
        }
        private void Update ()
        {
            if (StoneBarrier.activeSelf) return;
            if (Filled == true)
            {
                Filled = false;
                StartCoroutine(MoveOverTime(transform, LowTide, 100 / drainspeed));
                WaterReceiver.AddWater(1);
            }
        }

        IEnumerator MoveOverTime (Transform OriginalPos, Vector3 FinalPos, float duration)
        {
            if (duration > 0f)
            {
                float startTime = Time.time;
                float endTime = startTime + duration;
                transform.position = OriginalPos.position;
                yield return null;
                while (startTime < endTime)
                {
                    float progress = (Time.time - startTime) / duration;
                    transform.position = Vector3.Slerp(OriginalPos.position, FinalPos, progress);
                    yield return null;
                }
            }
            transform.position = FinalPos;
        }
    }

}