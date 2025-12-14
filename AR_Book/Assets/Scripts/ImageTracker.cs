// ImageTracker.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageTracker : MonoBehaviour
{
    public ARTrackedImageManager trackedImageManager;

    [System.Serializable]
    public class ImagePrefabAudio
    {
        public string imageName;      // 이미지 이름
        public GameObject prefab;     // 생성할 모델 프리팹
        public AudioClip audioClip;   // 재생할 오디오
    }

    public List<ImagePrefabAudio> imageData;

    private Dictionary<string, GameObject> spawned = new Dictionary<string, GameObject>();

    void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnChanged;
    }

    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnChanged;
    }

    void OnChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var img in args.added)
            SpawnOrUpdate(img);

        foreach (var img in args.updated)
            SpawnOrUpdate(img);

        foreach (var img in args.removed)
        {
            if (spawned.ContainsKey(img.referenceImage.name))
                spawned[img.referenceImage.name].SetActive(false);
        }
    }

    void SpawnOrUpdate(ARTrackedImage img)
    {
        string name = img.referenceImage.name;

        // 처음 인식됐을 때
        if (!spawned.ContainsKey(name))
        {
            foreach (var data in imageData)
            {
                if (data.imageName == name)
                {
                    // 모델 생성
                    GameObject obj = Instantiate(data.prefab, img.transform.position, img.transform.rotation);

                    // 오디오 소스 추가
                    AudioSource audio = obj.AddComponent<AudioSource>();
                    audio.clip = data.audioClip;
                    audio.playOnAwake = true;    // 생성되자마자 자동 재생
                    audio.spatialBlend = 1f;      // 3D 오디오

                    audio.Play(); // 오디오 재생

                    spawned.Add(name, obj);
                    break;
                }
            }
        }
        else
        {
            // 위치 업데이트
            spawned[name].transform.position = img.transform.position;
            spawned[name].transform.rotation = img.transform.rotation * Quaternion.Euler(0, -90, 0);
        }

        GameObject spawnedObj = spawned[name];

        if (img.trackingState == TrackingState.Tracking)
        {
            spawnedObj.SetActive(true);

            // 오디오가 재생 중이 아니면 재생
            AudioSource audio = spawnedObj.GetComponent<AudioSource>();
            if (audio != null && !audio.isPlaying)
            {
                audio.Play();
            }
        }
        else
        {
            // 이미지 안 보임 → 모델 + 오디오 숨김
            spawnedObj.SetActive(false);
        }
    }
}
