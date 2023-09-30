using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DragynGames
{
    static class SystemInfoLogger
    {
        public static string LogSystemInfo()
        {
            StringBuilder stringBuilder = new StringBuilder(1024);
            stringBuilder.Append("Rig: ").AppendSysInfoIfPresent(SystemInfo.deviceModel)
                .AppendSysInfoIfPresent(SystemInfo.processorType)
                .AppendSysInfoIfPresent(SystemInfo.systemMemorySize, "MB RAM").Append(SystemInfo.processorCount)
                .Append(" cores\n");
            stringBuilder.Append("OS: ").Append(SystemInfo.operatingSystem).Append("\n");
            stringBuilder.Append("GPU: ").Append(SystemInfo.graphicsDeviceName).Append(" ")
                .Append(SystemInfo.graphicsMemorySize)
                .Append("MB ").Append(SystemInfo.graphicsDeviceVersion)
                .Append(SystemInfo.graphicsMultiThreaded ? " multi-threaded\n" : "\n");
            stringBuilder.Append("Data Path: ").Append(Application.dataPath).Append("\n");
            stringBuilder.Append("Persistent Data Path: ").Append(Application.persistentDataPath).Append("\n");
            stringBuilder.Append("StreamingAssets Path: ").Append(Application.streamingAssetsPath).Append("\n");
            stringBuilder.Append("Temporary Cache Path: ").Append(Application.temporaryCachePath).Append("\n");
            stringBuilder.Append("Device ID: ").Append(SystemInfo.deviceUniqueIdentifier).Append("\n");
            stringBuilder.Append("Max Texture Size: ").Append(SystemInfo.maxTextureSize).Append("\n");
#if UNITY_5_6_OR_NEWER
            stringBuilder.Append("Max Cubemap Size: ").Append(SystemInfo.maxCubemapSize).Append("\n");
#endif
            stringBuilder.Append("Accelerometer: ")
                .Append(SystemInfo.supportsAccelerometer ? "supported\n" : "not supported\n");
            stringBuilder.Append("Gyro: ").Append(SystemInfo.supportsGyroscope ? "supported\n" : "not supported\n");
            stringBuilder.Append("Location Service: ")
                .Append(SystemInfo.supportsLocationService ? "supported\n" : "not supported\n");
#if !UNITY_2019_1_OR_NEWER
			stringBuilder.Append( "Image Effects: " ).Append( SystemInfo.supportsImageEffects ? "supported\n" : "not supported\n" );
			stringBuilder.Append( "RenderToCubemap: " ).Append( SystemInfo.supportsRenderToCubemap ? "supported\n" : "not supported\n" );
#endif
            stringBuilder.Append("Compute Shaders: ")
                .Append(SystemInfo.supportsComputeShaders ? "supported\n" : "not supported\n");
            stringBuilder.Append("Shadows: ").Append(SystemInfo.supportsShadows ? "supported\n" : "not supported\n");
            stringBuilder.Append("Instancing: ")
                .Append(SystemInfo.supportsInstancing ? "supported\n" : "not supported\n");
            stringBuilder.Append("Motion Vectors: ")
                .Append(SystemInfo.supportsMotionVectors ? "supported\n" : "not supported\n");
            stringBuilder.Append("3D Textures: ")
                .Append(SystemInfo.supports3DTextures ? "supported\n" : "not supported\n");
#if UNITY_5_6_OR_NEWER
            stringBuilder.Append("3D Render Textures: ")
                .Append(SystemInfo.supports3DRenderTextures ? "supported\n" : "not supported\n");
#endif
            stringBuilder.Append("2D Array Textures: ")
                .Append(SystemInfo.supports2DArrayTextures ? "supported\n" : "not supported\n");
            stringBuilder.Append("Cubemap Array Textures: ")
                .Append(SystemInfo.supportsCubemapArrayTextures ? "supported" : "not supported");

            return stringBuilder.ToString();
            

        }
        private static StringBuilder AppendSysInfoIfPresent(this StringBuilder sb, int info, string postfix = null)
        {
            if (info > 0)
            {
                sb.Append(info);

                if (postfix != null)
                    sb.Append(postfix);

                sb.Append(" ");
            }

            return sb;
        }
        private static StringBuilder AppendSysInfoIfPresent(this StringBuilder sb, string info, string postfix = null)
        {
            if (info != SystemInfo.unsupportedIdentifier)
            {
                sb.Append(info);

                if (postfix != null)
                    sb.Append(postfix);

                sb.Append(" ");
            }

            return sb;
        }

    }
}
