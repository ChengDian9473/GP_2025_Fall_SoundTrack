using UnityEngine;
using System.Collections;

namespace SoundTrack{
    public class CameraMove : MonoBehaviour
    {
        public Vector3 offset;
        public float moveDuration;
        
        private void Awake(){
            offset = new Vector3(4.0f, 0.0f, -10.0f);
            moveDuration = (60f / GameManager.Instance.bpm) * 0.75f;
            transform.position = offset + new Vector3(0.0f, 0.5f, 0.0f);

            GameManager.Instance.GameStart();
            //Debug.Log(moveDuration);
        }

        public void Follow(Vector3 targetPos)
        {
            StartCoroutine(MoveCoroutine(targetPos + offset));
        }
        IEnumerator MoveCoroutine(Vector3 endPos)
        {
            Vector3 start = transform.position;
            float elapsed = 0f;
            while (elapsed < moveDuration)
            {
                transform.position = Vector3.Lerp(start, endPos, elapsed / moveDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            transform.position = endPos;
        }
    }
}