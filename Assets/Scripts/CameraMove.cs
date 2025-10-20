using UnityEngine;
using System.Collections;

namespace SoundTrack{
    public class CameraMove : MonoBehaviour
    {
        public Vector3 offset = new Vector3(0, 0, -10);
        public float moveDuration;
        
        private void Awake(){
            moveDuration = (60f / GameManager.Instance.bpm) * 0.75f;
            Debug.Log(moveDuration);    
        }

        public void Follow(Vector3Int targetPos)
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