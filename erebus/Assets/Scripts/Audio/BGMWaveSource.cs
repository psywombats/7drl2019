using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSource : MonoBehaviour {

    // How long does the full length of data represent in seconds?
    public float PlayRate = 0.2f;
    public AudioClip Source;
    public bool DrawWave = false;

    private float[] channelSamples;
    private float[] averageSamples;
    private float cyclesPlayed;

    public void FixedUpdate() {
        if (Source == null) {
            Source = Global.Instance().Audio.BGMClip();
        }
        if (Source == null) {
            return;
        }
        int sampleCount = (int)Math.Ceiling(Source.frequency * PlayRate);
        if (channelSamples == null) {
            channelSamples = new float[sampleCount * Source.channels];
            averageSamples = new float[sampleCount];
        } else if (Source == null) {
            return;
        }

        Source.GetData(channelSamples, (int)Math.Floor(cyclesPlayed));
        float accum = 0.0f;
        for (int i = 0; i < sampleCount * Source.channels; i += 1) {
            if ((i + 1) % Source.channels == 0) {
                // this is the last in a sequence
                averageSamples[(i - 1) / Source.channels] = accum / (float)Source.channels;
                accum = 0.0f;
            } else {
                accum += channelSamples[i];
            }
        }
        cyclesPlayed += Time.deltaTime * (float)Source.frequency;

        if (DrawWave) {
            for (int i = 1; i < sampleCount; i += 1) {
                Debug.DrawLine(new Vector3(-5.0f + 10.0f * ((float)(i - 1) / (float)sampleCount), averageSamples[i - 1], 0.0f),
                               new Vector3(-5.0f + 10.0f * ((float)(i - 0) / (float)sampleCount), averageSamples[i], 0.0f),
                               Color.white);
            }
        }
    }
}
