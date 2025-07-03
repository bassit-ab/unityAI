using Unity.Cinemachine;
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public sealed partial class VirtualCameraCase
    {
        [SerializeField] CameraType cameraType;
        public CameraType CameraType => cameraType;

        [SerializeField] CinemachineCamera virtualCamera;
        public CinemachineCamera VirtualCamera => virtualCamera;

        private TweenCase shakeTweenCase;

        public void Initialise()
        {

        }

        public void SetFollowOffset(Vector3 followOffset)
        {
            CinemachineFollow transposer = VirtualCamera.GetComponent<CinemachineFollow>();

            if (transposer != null)
            {
                transposer.FollowOffset = followOffset;
            }
        }

        public void SetTrackedObjectOffset(Vector3 trackedObjectOffset)
        {
            CinemachineRotationComposer composer = VirtualCamera.GetComponent<CinemachineRotationComposer>();

            if(composer != null)
            {
                composer.TargetOffset = trackedObjectOffset;
            }
        }

        public void SetFov(float fov)
        {
            VirtualCamera.Lens.FieldOfView = fov;
        }

        public void Shake(float fadeInTime, float fadeOutTime, float duration, float gain)
        {
            if (shakeTweenCase != null && !shakeTweenCase.IsCompleted)
                shakeTweenCase.Kill();

            CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = virtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();

            shakeTweenCase = Tween.DoFloat(0.0f, gain, fadeInTime, (float fadeInValue) =>
            {
                cinemachineBasicMultiChannelPerlin.AmplitudeGain = fadeInValue;
            }).OnComplete(delegate
            {
                shakeTweenCase = Tween.DelayedCall(duration, delegate
                {
                    shakeTweenCase = Tween.DoFloat(gain, 0.0f, fadeOutTime, (float fadeOutValue) =>
                    {
                        cinemachineBasicMultiChannelPerlin.AmplitudeGain = fadeOutValue;
                    });
                });
            });
        }
    }
}