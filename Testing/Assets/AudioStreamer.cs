using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Network;
using Network.Logger;
using UnityEngine;

public class AudioStreamer : MonoBehaviour
{
    public const int FREQUENCY = 44100 / 4;
    
    public AudioPlayer Player;
    private AudioClip Mic;
    private int ConnectionId;
    private AudioSource Source;
    private int LastSample = 0;
    private Queue<float[]> SampleQueue = new Queue<float[]>();
    private object SampleLock = new object();

    void Start()
    {
        NetworkManager.PacketReceived += OnPacketReceived;
        NetworkManager.TCPConnect("127.0.0.1", 6969, (success, id) =>
        {
            ConnectionId = id;
            if (success)
            {
                Debug.Log(string.Format("Connected! {0}", ConnectionId));
            }
            else
            {
                Debug.Log(string.Format("Connection failed {0}", ConnectionId));
            }
        });

        Source = GetComponent<AudioSource>();
        string mic_name = Microphone.devices[0];
        Debug.Log(mic_name);
        Mic = Microphone.Start(mic_name, true, 1, FREQUENCY);
        while(Microphone.GetPosition(mic_name) < 0) {}
        Source.Play();
    }

    private void OnDestroy()
    {
        NetworkManager.ShutDownConnection(ConnectionId);
    }

    private void OnPacketReceived(Packet packet)
    {
        Debug.Log(string.Format("Got packet type {0} of size {1}", packet.Type, packet.Payload?.Length));
        if (packet.Type == 1)
        {
            float[] samples = ToArray(packet.Payload);
            Debug.Log(string.Format("Got samples {0}", samples.Length));
            lock (SampleLock)
            {
                SampleQueue.Enqueue(samples);
            }
        }
    }

    private void FixedUpdate()
    {
        int pos = Microphone.GetPosition(null);
        int diff = pos - LastSample;
        if (diff > 0)
        {
            float[] samples = new float[diff * Mic.channels];
            Mic.GetData(samples, LastSample);

            int index = -1;
            int trim_to = -1;
            for (int i = 0; i < samples.Length; i++)
            {
                if (samples[i] > 0.001f)
                {
                    if (index == -1) index = i;
                    trim_to = i;
                }
            }

            if (index != -1)
            {
                float[] trimmed = new float[trim_to - index];
                Array.Copy(samples, index, trimmed, 0, trimmed.Length);
                byte[] bytes = ToArray(trimmed);
                if (bytes?.Length > 0 && NetworkManager.SendPacket(ConnectionId, new Packet(1, bytes)))
                {
                    Log.Info("Sent samples {0} ({1} bytes)", trimmed.Length, bytes.Length);
                }
            }
        }
        LastSample = pos;

        lock (SampleLock)
        {
            if (SampleQueue.Count > 0)
            {
                float[] samples = SampleQueue.Dequeue();
                Log.Info("Playing {0} samples", samples.Length);
                Player.Play(samples, Mic.channels);
            }
        }
    }

    private byte[] ToArray(float[] array)
    {
        byte[] bytes = new byte[array.Length * 4];
        for (int i = 0; i < array.Length; i++)
        {
            byte[] data = BitConverter.GetBytes(array[i]);
            Array.Copy(data, 0, bytes, i * 4, data.Length);
        }

        return bytes;
    }

    private float[] ToArray(byte[] array)
    {
        float[] floats = new float[array.Length / 4];
        for (int i = 0; i < array.Length; i += 4)
        {
            floats[i / 4] = BitConverter.ToSingle(array, i);
        }

        return floats;
    }
}
