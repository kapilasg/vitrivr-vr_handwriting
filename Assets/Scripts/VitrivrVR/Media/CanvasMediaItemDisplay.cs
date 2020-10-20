﻿using System;
using System.Collections;
using System.Threading.Tasks;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Model.Data;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;

namespace VitrivrVR.Media
{
  /// <summary>
  /// Canvas based <see cref="MediaItemDisplay"/>.
  /// </summary>
  public class CanvasMediaItemDisplay : MediaItemDisplay
  {
    public Texture2D errorTexture;
    public Texture2D loadingTexture;
    public RawImage previewImage;
    public TextMeshProUGUI segmentDataText;

    private SegmentData _segment;
    private bool _videoInitialized;
    private VideoPlayer _videoPlayer;

    /// <summary>
    /// Tiny class for the sole purpose of enabling click events on <see cref="CanvasMediaItemDisplay"/> instances.
    /// </summary>
    private class ClickHandler : MonoBehaviour, IPointerClickHandler
    {
      public Action onClick;

      public void OnPointerClick(PointerEventData eventData)
      {
        onClick();
      }
    }

    private void Awake()
    {
      GetComponent<Canvas>().worldCamera = Camera.main;
      var clickHandler = previewImage.gameObject.AddComponent<ClickHandler>();
      clickHandler.onClick = OnClick;
    }

    /// <summary>
    /// Initializes this display with the given segment data.
    /// </summary>
    /// <param name="segment">Segment to display</param>
    public override async Task Initialize(SegmentData segment)
    {
      _segment = segment;
      var config = CineastConfigManager.Instance.Config;
      var objectId = await segment.GetObjectId();
      var thumbnailPath = PathResolver.ResolvePath(config.thumbnailPath, objectId, segment.Id);
      var thumbnailUrl = $"{config.mediaHost}{thumbnailPath}{config.thumbnailExtension}";
      StartCoroutine(DownloadThumbnailTexture(thumbnailUrl));
    }

    private void OnClick()
    {
      if (_videoInitialized)
      {
        if (_videoPlayer.isPlaying)
        {
          _videoPlayer.Pause();
        }
        else
        {
          _videoPlayer.Play();
        }
      }
      else
      {
        InitializeVideo();
      }
    }

    /// <summary>
    /// Initializes the <see cref="VideoPlayer"/> component of this display.
    /// </summary>
    private async void InitializeVideo()
    {
      // Set flag here to ensure video is only initialized once
      _videoInitialized = true;

      // Change texture to loading texture and reset scale
      previewImage.texture = loadingTexture;
      previewImage.transform.localScale = Vector3.one;

      // Resolve media URL
      // TODO: Retrieve and / or apply all required media information, potentially from within PathResolver
      var config = CineastConfigManager.Instance.Config;
      var objectId = await _segment.GetObjectId();
      var mediaPath = PathResolver.ResolvePath(config.mediaPath, objectId);
      var mediaUrl = $"{config.mediaHost}{mediaPath}";

      _videoPlayer = gameObject.AddComponent<VideoPlayer>();
      var audioSource = gameObject.AddComponent<AudioSource>();
      audioSource.spatialize = true;
      audioSource.spatialBlend = 1;

      _videoPlayer.isLooping = true;
      _videoPlayer.renderMode = VideoRenderMode.RenderTexture;

      _videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
      _videoPlayer.SetTargetAudioSource(0, audioSource);
      _videoPlayer.prepareCompleted += PrepareCompleted;
      _videoPlayer.errorReceived += ErrorEncountered;

      _videoPlayer.playOnAwake = true;

      _videoPlayer.url = mediaUrl;
      _videoPlayer.frame = await _segment.GetStart();

      var start = await _segment.GetAbsoluteStart();
      var end = await _segment.GetAbsoluteEnd();
      segmentDataText.text = $"Segment {_segment.Id} (Object {objectId}): {start:F}s - {end:F}s";
    }

    /// <summary>
    /// Method to download and apply the thumbnail texture from the given URL. Start as <see cref="Coroutine"/>.
    /// </summary>
    /// <param name="url">The URL to the thumbnail file</param>
    private IEnumerator DownloadThumbnailTexture(string url)
    {
      var www = UnityWebRequestTexture.GetTexture(url);
      yield return www.SendWebRequest();

      if (www.isNetworkError || www.isHttpError)
      {
        Debug.LogError(www.error);
        previewImage.texture = errorTexture;
      }
      else
      {
        var loadedTexture = ((DownloadHandlerTexture) www.downloadHandler).texture;
        previewImage.texture = loadedTexture;
        float factor = Mathf.Max(loadedTexture.width, loadedTexture.height);
        previewImage.rectTransform.sizeDelta = new Vector2(1000 * loadedTexture.width / factor, 1000 * loadedTexture.height / factor);
      }
    }

    private void PrepareCompleted(VideoPlayer videoPlayer)
    {
      var factor = Mathf.Max(videoPlayer.width, videoPlayer.height);
      var renderTex = new RenderTexture((int) videoPlayer.width, (int) videoPlayer.height, 24);
      videoPlayer.targetTexture = renderTex;
      previewImage.texture = renderTex;
      previewImage.rectTransform.sizeDelta = new Vector2(1000 * videoPlayer.width / factor, 1000 * videoPlayer.height / factor);

      videoPlayer.Pause();
    }

    private void ErrorEncountered(VideoPlayer videoPlayer, string error)
    {
      Debug.LogError(error);
      previewImage.texture = errorTexture;
    }
  }
}