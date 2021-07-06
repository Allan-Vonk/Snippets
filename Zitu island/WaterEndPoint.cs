using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Gameplay
{
    public class WaterEndPoint : MonoBehaviour
    {
        public UnityEvent OnWaterRisen = new UnityEvent();
        public Vector3 FilledOffset;
        private Transform LowTide;
        private Vector3 HighTide;
        public float FillSpeed;
        public int MaxWater = 2;
        public int FillAmount = 0;
        private Coroutine routine;


        private void Start ()
        {
            LowTide = transform;
            HighTide = LowTide.position + FilledOffset;
        }

        public void AddWater (int AmountOfWater)
        {
            if (FillAmount + AmountOfWater <= MaxWater)
            {
                FillAmount += AmountOfWater;
            }
            Vector3 WaterPercentageFilled = LowTide.position + ((HighTide - LowTide.position) / MaxWater * FillAmount);

            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(MoveOverTime(LowTide, WaterPercentageFilled, 100 / FillSpeed));
        }
        IEnumerator MoveOverTime (Transform OriginalPos, Vector3 FinalPos, float duration)
        {
            if (duration > 0f)
            {
                float progress = 0;
                transform.position = OriginalPos.position;
                yield return null;
                while (Vector3.Distance(OriginalPos.position, FinalPos) > 0.01f)
                {
                    transform.position = Vector3.Lerp(OriginalPos.position, FinalPos, progress / duration);
                    progress += Time.deltaTime;
                    yield return null;
                }
            }
            transform.position = FinalPos;
            routine = null;
            OnWaterRisen.Invoke();
        }
    }
}
