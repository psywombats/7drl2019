using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AudioImporter : AssetPostprocessor {

	public void OnPreprocessAudio() {
        UnityEditor.AudioImporter importer = (UnityEditor.AudioImporter)assetImporter;
        AudioImporterSampleSettings settings = new AudioImporterSampleSettings();

        if (assetPath.ToLower().Contains("bgm")) {
            importer.preloadAudioData = false;
            importer.loadInBackground = false;
            settings.compressionFormat = AudioCompressionFormat.MP3;
            settings.loadType = AudioClipLoadType.Streaming;
        } else if (assetPath.ToLower().Contains("sfx")) {
            importer.preloadAudioData = true;
            importer.loadInBackground = true;
            settings.compressionFormat = AudioCompressionFormat.PCM;
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
        }

        importer.defaultSampleSettings = settings;
    }
}
