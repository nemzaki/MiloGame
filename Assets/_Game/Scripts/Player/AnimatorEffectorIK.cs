using UnityEngine;
using RootMotion.FinalIK;
using System.Collections.Generic;

public class AnimatorEffectorIK : MonoBehaviour
{
    public Transform currentBodyPartTarget;

    public enum HitDetectionMode
    {
        Raycast,
        Distance
    }

    [System.Serializable]
    public class EffectorData
    {
        public FullBodyBipedEffector effector;
        public Transform target;
        public float stopDistance = 0.1f;
        public bool enableRecoil = true;
        public Vector3 recoilOffset = new Vector3(0, 0, -0.05f);

        public bool hasHit = false;
        public float weight = 0f;
        public bool fadingOut = false;
    }

    [Header("IK & Hit Settings")]
    public FullBodyBipedIK ik;
    public List<EffectorData> effectors = new List<EffectorData>();
    public LayerMask hitLayers;
    public HitDetectionMode hitDetectionMode = HitDetectionMode.Raycast;

    public Transform GetBodyPartToHit(string bodyPartName)
    {
        return bodyPartName switch
        {
            "Head" => ik.references.head,
            "Spine" => ik.references.spine[0],
            "RightHand" => ik.references.rightHand,
            "LeftHand" => ik.references.leftHand,
            _ => null
        };
    }

    public void BodyPartAffectedByHit(string bodyPartName)
    {
        foreach (var effector in effectors)
        {
            var matched = false;

            if (bodyPartName == "RightHand" && effector.effector == FullBodyBipedEffector.RightHand) matched = true;
            if (bodyPartName == "LeftHand" && effector.effector == FullBodyBipedEffector.LeftHand) matched = true;
            if (bodyPartName == "RightLeg" && effector.effector == FullBodyBipedEffector.RightFoot) matched = true;
            if (bodyPartName == "LeftLeg" && effector.effector == FullBodyBipedEffector.LeftFoot) matched = true;

            if (matched)
            {
                effector.target = currentBodyPartTarget;
                effector.hasHit = false;
                effector.fadingOut = false;
                effector.weight = 0f;
            }
        }
    }

    private void LateUpdate()
    {
        if (ik == null) return;

        foreach (var effectorData in effectors)
        {
            var solver = ik.solver.GetEffector(effectorData.effector);

            if (effectorData.target == null)
            {
                solver.positionWeight = 0f;
                effectorData.weight = 0f;
                continue;
            }

            var currentPos = solver.bone.position;
            var targetPos = effectorData.target.position;
            var dir = (targetPos - currentPos).normalized;
            bool hitDetected = false;
            Vector3 hitPoint = targetPos;

            if (!effectorData.hasHit)
            {
                if (hitDetectionMode == HitDetectionMode.Raycast)
                {
                    if (Physics.Raycast(currentPos, dir, out RaycastHit hit, effectorData.stopDistance, hitLayers))
                    {
                        hitDetected = true;
                        hitPoint = hit.point;
                    }
                }
                else if (hitDetectionMode == HitDetectionMode.Distance)
                {
                    float dist = Vector3.Distance(currentPos, targetPos);
                    if (dist <= effectorData.stopDistance)
                    {
                        hitDetected = true;
                    }
                }

                if (hitDetected)
                {
                    effectorData.hasHit = true;
                    effectorData.fadingOut = false;
                    effectorData.weight = 1f;

                    solver.position = effectorData.enableRecoil
                        ? hitPoint + Quaternion.LookRotation(dir) * effectorData.recoilOffset
                        : hitPoint;
                }
                else
                {
                    effectorData.weight = Mathf.MoveTowards(effectorData.weight, 1f, Time.deltaTime * 5f);
                    solver.position = targetPos;
                }
            }
            else
            {
                // After hit, fade out
                effectorData.fadingOut = true;
                effectorData.weight = Mathf.MoveTowards(effectorData.weight, 0f, Time.deltaTime * 4f);

                if (effectorData.weight <= 0f)
                {
                    effectorData.hasHit = false;
                    effectorData.fadingOut = false;
                    effectorData.target = null;
                    effectorData.weight = 0f;
                }
            }

            solver.positionWeight = effectorData.weight;
        }
    }

    public void ResetHits()
    {
        foreach (var effector in effectors)
        {
            var solver = ik.solver.GetEffector(effector.effector);
            solver.positionWeight = 0f;
            effector.hasHit = false;
            effector.fadingOut = false;
            effector.target = null;
            effector.weight = 0f;
        }
    }
}