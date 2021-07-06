using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Gameplay;
using Unity.FPS.Game;

namespace Unity.FPS.Gameplay
{
    public class LightMirror: MonoBehaviour, IInteractable
    {
        [Header("Mirroring Settings")]
        public bool Activated = false;
        public float MaxDistance = 0;
        public LayerMask layerMask;
        public GameObject TargetObject;
        public Transform FirePoint;
        [Header("Light Settings")]
        public GameObject LightBeam = null;
        public GameObject PointLight = null;
        public Vector3 BeamSizeOffset;
        public Transform HitPoint = null;

        [Header("Rotate settings")]
        public int RotateValue = 15;
        public float ClickableRange = 20;
        public GameObject Pillar;
        public InteractableType InteractType { get { return InteractableType.LightPuzzle; } }
        //Privates
        private bool rotating = false;
        private LightMirror targetMirrior;
        private LightReceiver lightReceiver;
        private GameObject HitMarker;

        private void Start ()
        {
            //Set Standard values
            if (HitMarker == null)
            {
                HitMarker = new GameObject();
            }

            if (HitPoint == null)
            {
                HitPoint = TargetObject.transform;
            }

            //Set beam position and rotation
            SetMirrorRotation();

            SetLightActivation(Activated);

            //Randomize the y axis
            if (RotateValue > 0)
            {
                Vector3 randomRot = new Vector3(0, Random.Range(0, (int) 360 / RotateValue) * RotateValue, 0);
                Pillar.transform.rotation = Quaternion.Euler(randomRot) * Pillar.transform.rotation;

            }

            targetMirrior = TargetObject.GetComponent<LightMirror>();
            if (targetMirrior == null)
            {
                lightReceiver = TargetObject.GetComponent<LightReceiver>();
            }

            if (Activated)
            {
                bool CanSeeMirror = CheckForMirror();

                if (CanSeeMirror == true)
                {
                    SetTargetMirriorState(true);
                }
                else
                {
                    SetTargetMirriorState(false);
                }
            }
        }

        public void SetMirrorRotation ()
        {
            if (RotateValue > 0)
            {
                Vector3 mirrorDirection = (HitPoint.transform.position - transform.position).normalized;
                Quaternion lookrotation = Quaternion.LookRotation(mirrorDirection);
                Pillar.transform.rotation = lookrotation;
                Pillar.transform.rotation = new Quaternion(0, Pillar.transform.rotation.y, 0f, Pillar.transform.rotation.w);
                
                transform.rotation = lookrotation;
                SetBeamPosition();
            }
            else
            {
                SetBeamPosition();
            }
        }
        public void SetBeamPosition ()
        {
            Vector3 pos;
            pos = (FirePoint.transform.position + HitPoint.transform.position) / 2;
            LightBeam.transform.position = pos;
            LightBeam.transform.localScale = BeamSizeOffset + new Vector3(.125f, .125f, Vector3.Distance(FirePoint.transform.position, HitPoint.transform.position));
            if (RotateValue <= 0)
            {
                LightBeam.transform.rotation = Quaternion.LookRotation((HitPoint.position - FirePoint.position));
            }
        }
        /// <summary>
        /// Sets the bool Activated to the value given. If true the script will start searching for the target mirror.
        /// </summary>
        /// <param name="value"></param>
        public void SetMirrorState (bool value)
        {
            Activated = value;

            SetLightActivation(value);

            if (Activated)
            {
                bool CanSeeMirror = CheckForMirror();
              
                if (CanSeeMirror == true)
                {
                    SetTargetMirriorState(true);
                }
                else
                {
                    SetTargetMirriorState(false);
                }
            }
            else
            {
                SetTargetMirriorState(false);
            }
        }
        private void Update ()
        {
            if (Activated && RotateValue > 0)
            {
                RaycastHit hit;
                if (Physics.Raycast(FirePoint.position, FirePoint.forward, out hit, MaxDistance))
                {
                    HitMarker.transform.position = hit.point;
                    HitPoint = HitMarker.transform;
                    SetBeamPosition();
                }
            }
           
        }

        public void SetLightActivation (bool active)
        {
            Activated = active;
            LightBeam.SetActive(active);
            PointLight.SetActive(active);
        }
        /// <summary>
        /// Checks if there is another mirror in front of this mirror
        /// </summary>
        /// <returns></returns>
        private bool CheckForMirror ()
        {
            RaycastHit hit;
            if (RotateValue <= 0)
            {
                if (Physics.Raycast(FirePoint.position, (HitPoint.transform.position - FirePoint.transform.position).normalized, out hit, MaxDistance, layerMask))
                {
                    if (hit.transform.gameObject == TargetObject)
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                if (Physics.Raycast(FirePoint.position, transform.forward, out hit, MaxDistance, layerMask))
                {
                    if (hit.transform.gameObject == TargetObject)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private void SetTargetMirriorState(bool state)
        {
            if (targetMirrior == null)
            {
                if (lightReceiver.Activated != state)
                {
                    lightReceiver.SetMirrorState(state);
                }
            }
            else
            {
                if (targetMirrior.Activated != state)
                {
                    targetMirrior.SetMirrorState(state);
                }
            }
        }

        private bool GetTargetMirriorState()
        {
            if (targetMirrior == null)
            {
                return lightReceiver.Activated;                
            }
            else
            {
                return targetMirrior.Activated;                
            }
        }

        IEnumerator RotateOverTime (Quaternion originalRotation, Quaternion finalRotation, float duration,Transform transformToRotate)
        {
            if (GetTargetMirriorState())
            {
                SetTargetMirriorState(false);
            }

            if (duration > 0f)
            {
                float startTime = Time.time;
                float endTime = startTime + duration;
                transformToRotate.rotation = originalRotation;
                yield return null;
                while (Time.time < endTime)
                {
                    float progress = (Time.time - startTime) / duration;
                    // progress will equal 0 at startTime, 1 at endTime.
                    transformToRotate.rotation = Quaternion.Slerp(originalRotation, finalRotation, progress);
                    yield return null;
                }
            }
            transformToRotate.rotation = finalRotation;
            rotating = false;

            if (Activated)
            {
                bool CanSeeMirror = CheckForMirror();

                if (CanSeeMirror == true)
                {
                    SetTargetMirriorState(true);
                }
                else
                {
                    SetTargetMirriorState(false);
                }
            }
        }
        public void Interact (GameObject user, MouseClick mouseClick)
        {
            if (!rotating)
            {
                if (mouseClick == MouseClick.Left)
                {
                    Rotate(-RotateValue);
                }

                if (mouseClick == MouseClick.Right)
                {
                    Rotate(RotateValue);
                }
            }
        }

        private void RotateGameObject (Vector3 DesiredIncrease, Transform transformToRotate)
        {
            Quaternion currentRotation = transformToRotate.rotation;
            Quaternion desiredRotation = currentRotation * Quaternion.Euler(DesiredIncrease);
            rotating = true;
            StartCoroutine(RotateOverTime(currentRotation, desiredRotation, .5f, transformToRotate));
            //PLAY ROTATESOUND
        }

        public void Rotate (float rotatevalue)
        {
            if (rotating) return;
            rotating = true;
            if (Pillar)
            {
                RotateGameObject(new Vector3(0, rotatevalue, 0), Pillar.transform);
            }
        }

        public void OnWaterRisen()
        {
            if (Activated)
            {
                bool CanSeeMirror = CheckForMirror();

                if (CanSeeMirror == true)
                {
                    SetTargetMirriorState(true);
                }
                else
                {
                    SetTargetMirriorState(false);
                }
            }
        }
    }
}