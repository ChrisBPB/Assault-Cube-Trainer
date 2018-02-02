using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Assault_Cube_Trainer
{
    public class Memory
    {

        #region cpp imports
        [DllImport("kernel32.dll")]
        public static extern Int32 CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        public static extern Int32 ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, UInt32 size, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(UInt32 dwDesiredAccess, Int32 bInheritHandle, UInt32 dwProcessId);
        #endregion

        #region constants
        public const uint PROCESS_VM_READ = (0x0010);
        public const uint PROCESS_VM_WRITE = (0x0020);
        public const uint PROCESS_VM_OPERATION = (0x0008);
        public const uint PAGE_READWRITE = 0x0004;
        public const uint PAGE_READ_ONLY = 0x0002;
        public const int WM_SYSCOMMAND = 0x0112;
        public const int WM_ACTIVATE = 0x6;
        public const int WM_HOTKEY = 0x0312;
        #endregion

        public Memory()
        {

        }

        private IntPtr m_hProcess = IntPtr.Zero;
        public Process ReadProcess
        {
            get
            {
                return m_ReadProcess;
            }
            set
            {
                m_ReadProcess = value;
            }
        }
        private Process m_ReadProcess = null;

        public void OpenProcess()
        {
            m_hProcess = OpenProcess(PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION, 1, (uint)m_ReadProcess.Id);

        }


        public byte[] ReadMatrix(int MemoryAddress)
        {
            uint bytesToRead = 64;
            byte[] buffer = new byte[bytesToRead];
            IntPtr procHandle = OpenProcess(PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION, 1, (uint)m_ReadProcess.Id);
            IntPtr ptrBytesReaded;
            ReadProcessMemory(procHandle, (IntPtr)MemoryAddress, buffer, bytesToRead, out ptrBytesReaded);
            CloseHandle(procHandle);

            return buffer;
        }

        public float ReadFloat(int MemoryAddress)
        {
            byte[] buffer;
            int read = ReadMem(MemoryAddress, 4, out buffer);
            if (read == 0)
                return 0;
            else
                return BitConverter.ToSingle(buffer, 0);
        }

        public int ReadInt(int MemoryAddress)
        {
            byte[] buffer;
            int read = ReadMem(MemoryAddress, 4, out buffer);
            if (read == 0)
                return 0;
            else
                return BitConverter.ToInt32(buffer, 0);
        }

        public int ReadMultiLevelPointer(int MemoryAddress, uint bytesToRead, Int32[] offsetList)
        {
            IntPtr procHandle = OpenProcess(PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION, 1, (uint)m_ReadProcess.Id);

            IntPtr pointer = (IntPtr)0x0;
            if (procHandle == IntPtr.Zero)
            {
                return 0;
            }

            byte[] btBuffer = new byte[bytesToRead];
            IntPtr lpOutStorage = IntPtr.Zero;



            int pointerAddy = MemoryAddress;

            for (int i = 0; i < (offsetList.Length); i++)
            {

                if (i == 0)
                {
                    ReadProcessMemory(
                          procHandle,
                         (IntPtr)(pointerAddy),
                         btBuffer,
                         (uint)btBuffer.Length,
                         out lpOutStorage);
                }
                pointerAddy = (BitConverter.ToInt32(btBuffer, 0) + offsetList[i]);
                ReadProcessMemory(
                     procHandle,
                     (IntPtr)(pointerAddy),
                     btBuffer,
                     (uint)btBuffer.Length,
                     out lpOutStorage);
            }
            return pointerAddy;
        }

        public int ReadMem(int MemoryAddress, uint bytesToRead, out byte[] buffer)
        {
            IntPtr procHandle = OpenProcess(PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION, 1, (uint)m_ReadProcess.Id);
            if (procHandle == IntPtr.Zero)
            {
                buffer = new byte[0];
                return 0;
            }

            buffer = new byte[bytesToRead];
            IntPtr ptrBytesReaded;
            ReadProcessMemory(procHandle, (IntPtr)MemoryAddress, buffer, bytesToRead, out ptrBytesReaded);
            CloseHandle(procHandle);
            return ptrBytesReaded.ToInt32();
        }

    }
}
