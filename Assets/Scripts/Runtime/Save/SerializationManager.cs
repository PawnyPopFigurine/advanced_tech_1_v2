using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using JZK.Utility;

namespace JZK.Serialization
{
    public static class SerializationManager
    {
        public enum EResultType
        {
            Success,
            FileNotFound,
            ExceptionOccurred
        }

        public struct SerializationResult
        {
            public EResultType ResultType;
            public object Object;
        }

        public static readonly string SAVE_DATA_RELATIVE_PATH = "/Saves";

        public static string SERIALIZATION_MANAGER_TAG = "SerializationManager - ";

        //@note: This should be the only reference to Application.persistentDataPath
        public static string GetPersistentDataPath()
        {
            //Do other stuff here for different platforms
            return Application.persistentDataPath;
        }

        public static string GetPersistentSaveDataPath()
        {
            return GetPersistentDataPath() + SAVE_DATA_RELATIVE_PATH;
        }

        //@Note: Returns true if the specified directory exists in the persistent data path
        public static bool GetPersistentDataDirectoryExists(string relativePath)
        {

            //check if its a valid relative path
            if (!IsValidRelativePath(relativePath))
            {
                Debug.LogWarning(SERIALIZATION_MANAGER_TAG + "Verification failed: relative path is not valid");
                return false;
            }


            string pathToDirectory = GetPersistentDataPath() + relativePath;

            //Other stuff here for different platforms

            return Directory.Exists(pathToDirectory);
        }

        //Should be only reference to File.Exists()
        public static bool GetFileExists(string fileNameAndPath)
        {
            //Do other stuff for different platforms
            return File.Exists(fileNameAndPath);
        }

        public static void CreateSaveDataDirectory()
        {
            CreatePersistentDataDirectory(SAVE_DATA_RELATIVE_PATH);
        }

        //Only reference to Directory.CreateDirectory()
        //@Note: Creates the specified directory in the persistent data path if it doesn't already exist
        public static void CreatePersistentDataDirectory(string relativePath)
        {
            if (GetPersistentDataDirectoryExists(relativePath))
            {
                return;
            }

            string pathToDirectory = GetPersistentDataPath() + relativePath;
            //other stuff for switch here
            Directory.CreateDirectory(pathToDirectory);
        }

        private static bool IsValidRelativePath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath) ||
                !relativePath[0].Equals('/') ||
                relativePath.Length < 2 ||
                relativePath.EndsWith("'/"))
            {
                Debug.LogWarning(SERIALIZATION_MANAGER_TAG + "Relative path must begin with forward slash, must not end with a forward slash, and be at least 2 characters long.");
                return false;
            }

            return true;
        }

        public static async Task ProcessWriteAsync(object data, string path)
        {
            Debug.Log(SERIALIZATION_MANAGER_TAG + " ProcessWriteAsync: " + path);

            byte[] encodedData = await ObjectToByteArray(data);

            //other stuff for switch/xbox

            int lastBackslashIndex = path.LastIndexOf("\\");
            int lastSlashIndex = path.LastIndexOf("/");
            string directory = path.Substring(0, Mathf.Max(lastSlashIndex, lastBackslashIndex));
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (FileStream sourceStream = new FileStream(path,
                FileMode.Create, FileAccess.Write, FileShare.None,
                bufferSize: 4096 * 100, useAsync: false))
            {
                try
                {
                    await sourceStream.WriteAsync(encodedData, 0, encodedData.Length);
                }
                catch (IOException ex)
                {
                    Debug.LogError("IOException - Could not save game file " + ex.Message + " - " + Time.time);
                }
            }
        }

        public static async Task<byte[]> ObjectToByteArray(object obj)
        {
            BinaryFormatter bf = new();
            SurrogateSelector selector = new();

            bf.SurrogateSelector = selector;
            using (var ms = new MemoryStream())
            {
                try
                {
                    await Task.Run(() => bf.Serialize(ms, obj));
                    return ms.ToArray();
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(ex.Message);
                    return null;
                }
            }
        }

        public static async Task<SerializationResult> ProcessReadAsync(string path)
        {
            SerializationResult result = new();

            Debug.Log(SERIALIZATION_MANAGER_TAG + "ProcessReadAsync: " + path);

            //other stuff for xbox and switch

            if (File.Exists(path) == false)
            {
                Debug.LogWarning("FileNotFound: " + path);
                result.ResultType = EResultType.FileNotFound;
            }
            else
            {
                try
                {
                    result = await ReadTextAsync(path);
                }
                catch (IOException ex)
                {
                    Debug.LogError(ex.Message);
                    result.ResultType = EResultType.ExceptionOccurred;
                }
            }

            return result;
        }

        private static async Task<SerializationResult> ReadTextAsync(string filePath)
        {
            string cachedFilePath = System.String.Copy(filePath);

            using (FileStream sourceStream = new FileStream(cachedFilePath,
                FileMode.Open, FileAccess.Read, FileShare.None, 4096 * 100, false))
            {
                byte[] buffer = new byte[0];
                byte[] tempBuffer = new byte[4096 * 100];
                int numRead;
                while ((numRead = await sourceStream.ReadAsync(tempBuffer, 0, tempBuffer.Length)) != 0)
                {
                    byte[] resizedArray = (byte[])tempBuffer.Clone();
                    System.Array.Resize(ref resizedArray, numRead);
                    buffer = buffer.ArrayAdd(resizedArray);
                }

                SerializationResult result = await ByteArrayToObject(buffer);
                return result;
            }
        }

        public static async Task<SerializationResult> ByteArrayToObject(byte[] arrayBytes)
        {
            SerializationResult result = new SerializationResult();

            Debug.Log(SERIALIZATION_MANAGER_TAG + " ByteArrayToObject");
            using (var memStream = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                SurrogateSelector selector = new SurrogateSelector();

                ApplySurrogates(selector);

                bf.SurrogateSelector = selector;
                memStream.Write(arrayBytes, 0, arrayBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);

                try
                {
                    object obj = await Task.Run(() => bf.Deserialize(memStream));

                    result.ResultType = EResultType.Success;
                    result.Object = obj;
                    return result;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(SERIALIZATION_MANAGER_TAG + "ByteArrayToObject Exception " + ex.Message);
                    Debug.LogError(ex.Message);

                    result.ResultType = EResultType.ExceptionOccurred;
                }
            }

            return result;
        }

        static void ApplySurrogates(SurrogateSelector selector)
        {


        }

        //public static bool GetFileExists(string)

        public static void DeleteFile(string filePath)
        {
            bool fileExists = GetFileExists(filePath);

            if (fileExists)
            {
                File.Delete(filePath);
                Debug.Log("Serialization Manager - Successfully deleted file at path " + filePath);
            }
        }
    }
}