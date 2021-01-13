﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

namespace VitrivrVR.Input.Controller
{
  // Script structure from official documentation: https://docs.unity3d.com/Manual/xr_input.html

  /// <summary>
  /// Object for the management of XR controller button events.
  /// <para>Place in a scene and connect actions to the respective button events.</para>
  /// </summary>
  public class XRButtonObserver : MonoBehaviour
  {
    [Serializable]
    public class XRButtonEvent : UnityEvent<bool>
    {
    }

    [Serializable]
    public class XRAxisEvent : UnityEvent<Vector2>
    {
    }

    public XRButtonEvent primaryButtonEvent;
    public XRButtonEvent secondaryButtonEvent;
    public XRAxisEvent leftHandAxisEvent;
    public XRAxisEvent rightHandAxisEvent;
    
    public readonly List<InputDevice> leftHandDevices = new List<InputDevice>();
    public readonly List<InputDevice> rightHandDevices = new List<InputDevice>();

    private bool _lastPrimaryButtonState;
    private bool _lastSecondaryButtonState;
    private readonly List<InputDevice> _devicesWithPrimaryButton = new List<InputDevice>();
    private readonly List<InputDevice> _devicesWithSecondaryButton = new List<InputDevice>();

    private void OnEnable()
    {
      // Get all already connected XR input devices
      var allDevices = new List<InputDevice>();
      InputDevices.GetDevices(allDevices);
      foreach (var device in allDevices)
        DeviceConnected(device);

      // Register this observer to be notified when new XR input devices are connected or existing ones disconnected
      InputDevices.deviceConnected += DeviceConnected;
      InputDevices.deviceDisconnected += DeviceDisconnected;
    }

    private void OnDisable()
    {
      // Deregister this observer from XR input device changes
      InputDevices.deviceConnected -= DeviceConnected;
      InputDevices.deviceDisconnected -= DeviceDisconnected;
      // Clear all device lists
      _devicesWithPrimaryButton.Clear();
      _devicesWithSecondaryButton.Clear();
      leftHandDevices.Clear();
      rightHandDevices.Clear();
    }

    /// <summary>
    /// Adds a newly connected XR input device to all relevant device lists.
    /// </summary>
    /// <param name="device">New XR input device to add to lists</param>
    private void DeviceConnected(InputDevice device)
    {
      if (device.TryGetFeatureValue(CommonUsages.primaryButton, out _))
        _devicesWithPrimaryButton.Add(device);
      if (device.TryGetFeatureValue(CommonUsages.secondaryButton, out _))
        _devicesWithSecondaryButton.Add(device);
      if ((device.characteristics & InputDeviceCharacteristics.Left) != 0)
        leftHandDevices.Add(device);
      if ((device.characteristics & InputDeviceCharacteristics.Right) != 0)
        rightHandDevices.Add(device);
    }

    /// <summary>
    /// Removes a disconnecting XR input device from all device lists.
    /// </summary>
    /// <param name="device">XR input device to remove from lists</param>
    private void DeviceDisconnected(InputDevice device)
    {
      if (_devicesWithPrimaryButton.Contains(device))
        _devicesWithPrimaryButton.Remove(device);
      if (_devicesWithSecondaryButton.Contains(device))
        _devicesWithSecondaryButton.Remove(device);
      if (leftHandDevices.Contains(device))
        leftHandDevices.Remove(device);
      if (rightHandDevices.Contains(device))
        rightHandDevices.Remove(device);
    }

    private void Update()
    {
      // Get button states
      var primaryButtonState = _devicesWithPrimaryButton.Aggregate(false, (current, device) =>
        device.TryGetFeatureValue(CommonUsages.primaryButton, out var tempState) && tempState || current);

      var secondaryButtonState = _devicesWithSecondaryButton.Aggregate(false, (current, device) =>
        device.TryGetFeatureValue(CommonUsages.secondaryButton, out var tempState) && tempState || current);

      // Invoke events for which state has changed
      if (primaryButtonState != _lastPrimaryButtonState)
      {
        primaryButtonEvent.Invoke(primaryButtonState);
        _lastPrimaryButtonState = primaryButtonState;
      }

      if (secondaryButtonState != _lastSecondaryButtonState)
      {
        secondaryButtonEvent.Invoke(secondaryButtonState);
        _lastSecondaryButtonState = secondaryButtonState;
      }
      
      // Axis events
      var leftHandPrimaryAxisState = leftHandDevices.Aggregate(Vector2.negativeInfinity,
        (current, device) =>
          device.TryGetFeatureValue(CommonUsages.primary2DAxis, out var tempState) ? tempState : current);
      leftHandAxisEvent.Invoke(leftHandPrimaryAxisState);
      
      var rightHandAxisState = rightHandDevices.Aggregate(Vector2.negativeInfinity,
        (current, device) =>
          device.TryGetFeatureValue(CommonUsages.primary2DAxis, out var tempState) ? tempState : current);
      rightHandAxisEvent.Invoke(rightHandAxisState);
    }
  }
}