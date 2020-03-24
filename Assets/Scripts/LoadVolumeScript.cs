using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[ExecuteInEditMode]
//Based on tutorial >> http://graphicsrunner.blogspot.com/2009/01/volume-rendering-101.html
public class LoadVolumeScript : MonoBehaviour
{
    public enum VolumeDataSrc { CT, RAW, ABSTRACT }
    public bool loadFlag = true;
    public VolumeDataSrc volumeDataSource;
    Texture3D mVolume;
    int mWidth = 512, mHeight = 512, mDepth = 113, maxDimension;
    float[] mScalars;
    Color[] mColors;

    public Texture3D GetVolume()
    {
        return mVolume;
    }

    void Start()
    {
        //always trigger load from start
        loadFlag = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (loadFlag)
        {
            loadFlag = false;

            //set volume dimensions to match file to load
            if (volumeDataSource == VolumeDataSrc.CT)
            {
                mWidth = mHeight = 256;
                mDepth = 109;
            }
            else if (volumeDataSource == VolumeDataSrc.RAW)
            {
                mWidth = mHeight = 256;
                //mDepth = 178;
                mDepth = 198;
            }
            else
            {
                mWidth = mHeight = mDepth = 256;
            }

            //get the max value to make volume equal on all sides (width x height x depth)
            maxDimension = Mathf.Max(mWidth, mHeight, mDepth);

            //create array to store data
            mScalars = new float[mWidth * mHeight * mDepth];

            //load data from either CT or RAW file
            switch (volumeDataSource)
            {
                case VolumeDataSrc.CT:
                    //create array of colors to fill 3d texture pixels
                    mColors = new Color[maxDimension * maxDimension * maxDimension];

                    //load one file per slice
                    for (int i = 0; i < mDepth; ++i)
                    {
                        //loadRAWFile16(File.Open("bunny-ctscan.tar/bunny/" + (i + 1).ToString(), FileMode.Open), i);
                        appendRAWFile16(File.Open("MRbrain.tar/MRbrain/MRBrain." + (i + 1).ToString(), FileMode.Open), i);
                        //loadRAWFile16(File.Open("CThead.tar/CThead/CThead." + (i + 1).ToString(), FileMode.Open), i);
                    }

                    //create a 3d texture and apply the data
                    mVolume = new Texture3D(maxDimension, maxDimension, maxDimension, TextureFormat.RGBA32, true);
                    mVolume.SetPixels(mColors);
                    mVolume.Apply();
                    break;
                case VolumeDataSrc.RAW:
                    //create a 3d texture to be filled with data
                    mVolume = new Texture3D(maxDimension, maxDimension, maxDimension, TextureFormat.RGBA32, true);

                    //load pixel values from file
                    loadRAWFile(Application.streamingAssetsPath + "/Models/output.raw");
                    break;
                case VolumeDataSrc.ABSTRACT:
                    mVolume = CreateTexture3D(64);// new Texture3D(mWidth, mHeight, mDepth, TextureFormat.R8, true);
                    break;
            }

        }
    }

    //create a spherical volume
    Texture3D CreateTexture3D(int size)
    {
        Color[] colorArray = new Color[size * size * size];
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    //get vector from pixel to center
                    Vector3 center = new Vector3(size, size, size) / 2;
                    Vector3 pos = new Vector3(x, y, z);
                    Vector3 toCenter = center - pos;

                    //use distance over range to attenuate color
                    float radius = size / 2;
                    float percentage = toCenter.magnitude / radius;
                    Color c = new Color(1 - percentage, 0, 0, 1);

                    //set pixel in color array
                    colorArray[x + (y * size) + (z * size * size)] = c;
                }
            }
        }

        //create texture and fill pixels
        Texture3D texture = new Texture3D(size, size, size, TextureFormat.RGBA32, true);
        texture.SetPixels(colorArray);
        texture.Apply();

        return texture;
    }

    private void loadRAWFile(string filename)
    {
        FileStream file = new FileStream(filename, FileMode.Open);
        long length = file.Length;

        if (length > mWidth * mHeight * mDepth)
        {
            loadRAWFile16(file);
        }
        else
        {
            loadRAWFile8(file);
        }

        file.Close();
    }

    /// <summary>
    /// Loads an 8-bit RAW file.
    /// </summary>
    /// <param name="file"></param>
    private void loadRAWFile8(FileStream file)
    {
        BinaryReader reader = new BinaryReader(file);

        byte[] buffer = new byte[mWidth * mHeight * mDepth];
        int size = sizeof(byte);

        reader.Read(buffer, 0, size * buffer.Length);

        reader.Close();

        mScalars = new float[buffer.Length];
        mColors = new Color[maxDimension * maxDimension * maxDimension];
        for (int i = 0; i < buffer.Length; i++)
        {
            //scale the scalar values to [0, 1]
            mScalars[i] = (float)buffer[i] / byte.MaxValue;
            mColors[i] = new Color(mScalars[i], 0, 0, 1);
        }

        mVolume.SetPixels(mColors);
        mVolume.Apply();
    }

    /// <summary>
    /// Loads a 16-bit RAW file.
    /// </summary>
    /// <param name="file"></param>
    private void loadRAWFile16(FileStream file)
    {
        BinaryReader reader = new BinaryReader(file);

        ushort[] buffer = new ushort[mWidth * mHeight * mDepth];

        for (int i = 0; i < buffer.Length; i++)
            buffer[i] = reader.ReadUInt16();

        reader.Close();

        mScalars = new float[buffer.Length];
        mColors = new Color[buffer.Length];
        for (int i = 0; i < buffer.Length; i++)
        {
            //scale the scalar values to [0, 1]
            mScalars[i] = (float)buffer[i] / ushort.MaxValue;
            mColors[i] = new Color(mScalars[i], 0, 0, 1);
        }

        mVolume.SetPixels(mColors);
        mVolume.Apply();
    }

    /// <summary>
    /// Loads a 16-bit RAW file and appends it to the array of data.
    /// </summary>
    /// <param name="file"></param>
    private void appendRAWFile16(FileStream file, int slice)
    {
        BinaryReader reader = new BinaryReader(file);

        short[] buffer = new short[mWidth * mHeight];

        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = reader.ReadInt16();
            var bytes = BitConverter.GetBytes(buffer[i]);
            Array.Reverse(bytes);
            buffer[i] = BitConverter.ToInt16(bytes, 0);
        }

        reader.Close();

        int offset = slice * mWidth * mHeight;
        for (int i = 0; i < buffer.Length; i++)
        {
            //scale the scalar values to [0, 1]
            mScalars[i + offset] = (float)buffer[i] / short.MaxValue;
            mColors[i + offset] = new Color(mScalars[i + offset], 0, 0, 1);
        }
    }
}