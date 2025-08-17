using System;
using System.Runtime.InteropServices;

struct TrackingData
{
    public float m_headPositionX; // Not used yet
    public float m_headPositionY; // Not used yet
    public float m_headPositionZ; // Not used yet
    public float m_headRotationX;
    public float m_headRotationY;
    public float m_headRotationZ;
    public float m_headRotationW;
    public float m_gazeX; // Range - [0;1], 0.5 - center
    public float m_gazeY; // Range - [0;1], 0.5 - center
    public float m_blink; // Range - [0;1], 1.0 - closed
    public float m_mouthOpen; // Range - [0;1]
    public float m_mouthShape; // Range - [-1;1], -1 - wide, 1 - narrow
    public float m_brows; // Range - [-1;1], -1 - up, 1 - down; not used yet

    public static byte[] ToBytes(TrackingData p_faceData)
    {
        int l_size = Marshal.SizeOf(p_faceData);
        byte[] l_arr = new byte[l_size];

        IntPtr ptr = Marshal.AllocHGlobal(l_size);
        Marshal.StructureToPtr(p_faceData, ptr, true);
        Marshal.Copy(ptr, l_arr, 0, l_size);
        Marshal.FreeHGlobal(ptr);
        return l_arr;
    }

    public static TrackingData ToObject(byte[] p_buffer)
    {
        TrackingData l_faceData = new TrackingData();

        int l_size = Marshal.SizeOf(l_faceData);
        IntPtr l_ptr = Marshal.AllocHGlobal(l_size);

        Marshal.Copy(p_buffer, 0, l_ptr, l_size);

        l_faceData = (TrackingData)Marshal.PtrToStructure(l_ptr, l_faceData.GetType());
        Marshal.FreeHGlobal(l_ptr);

        return l_faceData;
    }
}
