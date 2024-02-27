using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace RTG
{
    public class VRControllerInputDevice : InputDeviceBase
    {
        private Vector3 _frameDelta;
        private Vector3 _vrPosInLastFrame;
        private XRRayInteractor _rayInteractor;
        private InputDevice _device;

        public override InputDeviceType DeviceType { get { return InputDeviceType.VRController; } }

        public VRControllerInputDevice(InputDevice device, XRRayInteractor rayInteractor)
        {
            _frameDelta = Vector3.zero;
            _rayInteractor = rayInteractor;
            rayInteractor.TryGetHitInfo(out Vector3 reticlePosition, out _, out _, out _);
            _vrPosInLastFrame = reticlePosition;
            _device = device;
        }

        public override Vector3 GetFrameDelta()
        {
            return _frameDelta;
        }

        public override Ray GetRay(Camera camera)
        {
            _rayInteractor.TryGetHitInfo(out Vector3 reticlePosition, out _, out _, out _);
            return camera.ScreenPointToRay(reticlePosition);
        }

        public override Vector3 GetPositionYAxisUp()
        {
            _rayInteractor.TryGetHitInfo(out Vector3 reticlePosition, out _, out _, out _);
            return reticlePosition;
        }

        public override bool HasPointer()
        {
            return _rayInteractor != null;
        }

        public override bool IsButtonPressed(int buttonIndex)
        {
            return _device.TryGetFeatureValue(CommonUsages.triggerButton, out _);
        }

        public override bool WasButtonPressedInCurrentFrame(int buttonIndex)
        {
            return RTInput.WasMouseButtonPressedThisFrame(buttonIndex);
        }

        public override bool WasButtonReleasedInCurrentFrame(int buttonIndex)
        {
            return RTInput.WasMouseButtonReleasedThisFrame(buttonIndex);
        }

        public override bool WasMoved()
        {
            _rayInteractor.TryGetHitInfo(out Vector3 reticlePosition, out _, out _, out _);
            return reticlePosition != _vrPosInLastFrame;
        }

        protected override void UpateFrameDeltas()
        {
            _rayInteractor.TryGetHitInfo(out Vector3 reticlePosition, out _, out _, out _);
            _frameDelta = reticlePosition - _vrPosInLastFrame;
            _vrPosInLastFrame = reticlePosition;
        }
    }
}

