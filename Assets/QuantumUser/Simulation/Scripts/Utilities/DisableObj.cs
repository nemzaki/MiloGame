using System;
using System.Collections;
using UnityEngine;

namespace Quantum
{
    public class DisableObj : MonoBehaviour
    {
        public float disableTime = 1;

        private void OnEnable()
        {
            StartCoroutine(DisableFX());
        }

        IEnumerator DisableFX()
        {
            yield return new WaitForSeconds(disableTime);
            gameObject.SetActive(false);
        }
    }
}
