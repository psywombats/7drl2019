using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSource : MonoBehaviour {

    // How long does the full length of data represent in seconds?
    public float PlayRate = 0.15f;
    public int Oversample = 25;
    public AudioClip Source;
    public bool DrawWave = false;

    private float[] channelSamples;
    private float[] averageSamples;
    private float elapsedTime;

    public void FixedUpdate() {
        if (Source == null) {
            Source = Global.Instance().Audio.BGMClip();
        }
        if (Source == null) {
            return;
        }
        int sampleInCount = (int)Math.Ceiling(Source.channels * Source.frequency * PlayRate);
        int sampleCountPerChannel = sampleInCount / Source.channels;
        int outputSampleCount = sampleCountPerChannel / Oversample;
        int inSamplesPerOut = sampleInCount / outputSampleCount;
        if (channelSamples == null) {
            channelSamples = new float[sampleInCount];
            averageSamples = new float[outputSampleCount];
        }

        Source.GetData(channelSamples, (int)(elapsedTime * (float)Source.frequency));
        for (int outSample = 0; outSample < outputSampleCount; outSample += 1) {
            float accum = 0.0f;
            for (int i = 0; i < inSamplesPerOut; i += 1) {
                accum += channelSamples[outSample * inSamplesPerOut + i];
            }
            averageSamples[outSample] = accum / (float)inSamplesPerOut;
        }
        elapsedTime += Time.deltaTime;

        if (DrawWave) {
            for (int i = 1; i < outputSampleCount; i += 1) {
                Debug.DrawLine(new Vector3(-5.0f + 10.0f * ((float)(i - 1) / (float)outputSampleCount), averageSamples[i - 1], 0.0f),
                               new Vector3(-5.0f + 10.0f * ((float)(i - 0) / (float)outputSampleCount), averageSamples[i], 0.0f),
                               Color.white);
            }
        }
    }

    public float[] GetSamples() {
        return averageSamples;
    }

    public int GetSampleCount() {
        int sampleInCount = (int)Math.Ceiling(Source.channels * Source.frequency * PlayRate);
        int sampleCountPerChannel = sampleInCount / Source.channels;
        return sampleCountPerChannel / Oversample;
    }
}
