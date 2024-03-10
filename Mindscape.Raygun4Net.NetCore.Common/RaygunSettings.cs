﻿using System;

namespace Mindscape.Raygun4Net
{
  public abstract class RaygunSettingsBase
  {
    private const string DefaultApiEndPoint = "https://api.raygun.com/entries";
    private const string RaygunMessageQueueMaxVariable = "RAYGUN_MESSAGE_QUEUE_MAX";

    public RaygunSettingsBase()
    {
      ApiEndpoint = new Uri(DefaultApiEndPoint);
      
      // See if there's an overload defined in an environment variable, and set it accordingly
      var messageQueueMaxValue = Environment.GetEnvironmentVariable(RaygunMessageQueueMaxVariable);
      if (!string.IsNullOrEmpty(messageQueueMaxValue) && int.TryParse(messageQueueMaxValue, out var maxQueueSize))
      {
        BackgroundMessageQueueMax = maxQueueSize;
      }
    }

    /// <summary>
    /// Raygun Application API Key, can be found in the Raygun application dashboard by clicking the "Application settings" button
    /// </summary>
    public string ApiKey { get; set; }
 
    public Uri ApiEndpoint { get; set; }

    public bool ThrowOnError { get; set; }

    public string ApplicationVersion { get; set; }

    /// <summary>
    /// If set to true will automatically setup handlers to catch Unhandled Exceptions
    /// </summary>
    /// <remarks>
    /// Currently defaults to false. This may be change in future releases.
    /// </remarks>
    public bool CatchUnhandledExceptions { get; set; } = false;

    /// <summary>
    /// The maximum queue size for background exceptions
    /// </summary>
    public int BackgroundMessageQueueMax { get; } = ushort.MaxValue;

    /// <summary>
    /// Controls the number of background threads used to process the raygun message queue
    /// </summary>
    /// <remarks>
    /// Defaults to Environment.ProcessorCount * 2 &gt;= 8 ? 8 : Environment.ProcessorCount * 2
    /// </remarks>
    public int BackgroundMessageWorkerCount { get; set; } = Environment.ProcessorCount * 2 >= 8 ? 8 : Environment.ProcessorCount * 2;
  }
}