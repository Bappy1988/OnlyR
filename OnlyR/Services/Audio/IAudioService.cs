﻿using System;
using System.Collections.Generic;
using OnlyR.Core.EventArgs;
using OnlyR.Model;
using OnlyR.Services.Options;

// ReSharper disable UnusedMember.Global
// ReSharper disable EventNeverSubscribedTo.Global

namespace OnlyR.Services.Audio
{
    public interface IAudioService
    {
        event EventHandler StartedEvent;
        event EventHandler StoppedEvent;
        event EventHandler StopRequested;
        event EventHandler<RecordingProgressEventArgs> RecordingProgressEvent;

        IEnumerable<RecordingDeviceItem> GetRecordingDeviceList();

        void StartRecording(RecordingCandidate candidateFile, IOptionsService optionsService);
        void StopRecording(bool fadeOut);
    }
}
