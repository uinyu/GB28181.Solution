﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Linq;
using GLib.Extension;
using System.IO;

namespace SS.ClientBase.Codec {
   public partial class X264Native {
        private static BinaryWriter w = null;
        public static void Test() {
            int width = 320, height = 240;
            X264Native x264 = new X264Native(new X264Params(width, height, 10, 320));

            //x264.SetIKeyIntMax(10);
            x264.Init();

            var ls = MediaServer.Media.ReadFile.GetBuffByFile1(@"D:\video_monitor\v2v_super_exchange\branch\branch_V3.16.0.0(dongtaifuyong)\bin\Debug\1237.yuv");
            AVCodecCfg cf = AVCodecCfg.CreateVideo(width, height, (int)SS.Media.AVCode.CODEC_ID_H264, 100000);
            FFImp ffimp = new FFImp(cf, true);
            //FFScale ffscale = new FFScale(width, height, 26, 12, width, height, 12, 12);
            FFScale ffscale = new FFScale(width, height, 0, 12,width,height, 3, 24);
            foreach (var item1 in ls)
            {
                var item = ffscale.FormatS(item1);
                var in_buf = FunctionEx.BytesToIntPtr(item);
                var out_buf = Marshal.AllocHGlobal(item.Length);
                //var bKeyFrame = false;
                //var nOutLen = 0;
                var nInLen = item.Length;
                //  var size = X264Encode(x264.obj, in_buf, ref nInLen, out_buf, ref nOutLen, ref bKeyFrame);
                // var buf = FunctionEx.IntPtrToBytes(out_buf, 0, size);
                var buf = x264.Encode(item);
                Console.WriteLine(buf.To16Strs(0, 16));
                var size = buf.Length;

                if (w == null)  //OK
                {
                    w = new BinaryWriter(new FileStream("4567.es", FileMode.Create));
                }
                w.Write(buf);

                ////Media.MediaFrame mf = new Media.MediaFrame();
                ////mf.nIsKeyFrame = (byte)(x264.IsKeyFrame() ? 1 : 0);
                ////mf.nWidth = width;
                ////mf.nHeight = height;
                ////mf.nEncoder = Media.MediaFrame.H264Encoder;
                ////mf.nTimetick = 0;
                ////mf.nSize = size;
                ////mf.Data = buf;
                ////buf = mf.GetBytes();
                ////fs.Write(BitConverter.GetBytes(buf.Length), 0, 4);
                //fs.Write(buf, 0, buf.Length);
                //fs.Flush();
                //IntPtr intt = IntPtr.Zero;
                //var sssss = ffimp.VideoDec(buf, ref intt);
                ////Console.WriteLine(buf.Take(32).ToArray().To16Strs());
                //// var size = Encode1(ii, in_buf, ref nInLen, out_buf);
            }

            //fs.Close();

        }

    }
   public partial class X264Native {
       public static X264Native _DLastX264Native = null;
       private IntPtr obj;
       private bool _isReleased = false;
       private byte[] _outBuf = null;
       private object _sync = new object();
       public int Bitrate { get; private set; }
       public X264Native(X264Params p) {
           Bitrate = p.bitrate;
           _outBuf = new byte[p.width * p.height * 4];
           byte[] b = FunctionEx.StructToBytes(p);
           obj = X264Native_New(b);
           _DLastX264Native = this;
         
       }

       public bool Init() {
           bool r = X264Native_Init(obj);
           return r;
       }

       public void ForceIDRFrame() {
           X264Native_ForceIDRFrame(obj);
       }

       public void SetBitrate(int bitrate) {
           Bitrate = bitrate;
           X264Native_SetBitrate(obj, bitrate);
       }


       public void UpgradeBitrateLevel() {
           X264Native_UpgradeBitrateLevel(obj);
       }

       public void DeclineBitrateLevel() {
           X264Native_DeclineBitrateLevel(obj);
       }

       public void SetLeastBitrateLevel() {
           X264Native_SetLeastBitrateLevel(obj);
       }

       public bool IsKeyFrame() {
           return _lastEncodeIsIFrame;
           //return X264Native_IsKeyFrame(obj);
       }
       private Boolean _lastEncodeIsIFrame = false;
       public byte[] Encode(byte[] inBuf) {
           lock (_sync) {
               if (_isReleased) {
                   return null;
               }
               try {
                   int size = X264Native_Encode(obj, inBuf, inBuf.Length, _outBuf);
                   byte[] outBuf = new byte[size];
                   Array.Copy(_outBuf, outBuf, size);
                   _lastEncodeIsIFrame = outBuf[4] == 0x67;
                   return outBuf;
               } catch (Exception e) {
                   if (_isReleased)
                       return null;
                   throw e;
               }
               return null;
           }
       }

       public void Release() {
           lock (_sync) {
               if (_isReleased)
                   return;

               _isReleased = true;
               X264Native_Release(obj);
           }
       }
       const string DLLFile = @"x264.dll";

       [DllImport(DLLFile, CallingConvention = CallingConvention.Cdecl)]
       private extern static IntPtr X264Native_New(byte[] x264params);
       [DllImport(DLLFile, CallingConvention = CallingConvention.Cdecl)]
       private extern static bool X264Native_Init(IntPtr obj);
       [DllImport(DLLFile, CallingConvention = CallingConvention.Cdecl)]
       private extern static bool X264Native_Release(IntPtr obj);

       [DllImport(DLLFile, CallingConvention = CallingConvention.Cdecl)]
       private extern static void X264Native_ForceIDRFrame(IntPtr obj);
       [DllImport(DLLFile, CallingConvention = CallingConvention.Cdecl)]
       private extern static void X264Native_SetBitrate(IntPtr obj, int bitrate);
       [DllImport(DLLFile, CallingConvention = CallingConvention.Cdecl)]
       private extern static void X264Native_UpgradeBitrateLevel(IntPtr obj);
       [DllImport(DLLFile, CallingConvention = CallingConvention.Cdecl)]
       private extern static void X264Native_DeclineBitrateLevel(IntPtr obj);
       [DllImport(DLLFile, CallingConvention = CallingConvention.Cdecl)]
       private extern static void X264Native_SetLeastBitrateLevel(IntPtr obj);
       [DllImport(DLLFile, CallingConvention = CallingConvention.Cdecl)]
       private extern static bool X264Native_IsKeyFrame(IntPtr obj);
       [DllImport(DLLFile, CallingConvention = CallingConvention.Cdecl)]
       private extern static int X264Native_Encode(IntPtr obj, byte[] inData, int len, byte[] outData);


   }

    public struct X264Params {
        public int width;
        public int height;
        public int fps;
        public int bitrate;
        public int pix;//源图片格式
        /*
        #define X264_RC_CQP                  0
        #define X264_RC_CRF                  1
        #define X264_RC_ABR                  2
        */
        public int method;//

        public int key_frame_max;//最大关键帧间隔
        public int key_frame_min;//最小关键帧间隔
        public int threads;//编码线程
        public int level_idc;//编码复杂度
        public int profile;//对应{ "baseline", "main", "high", "high10", "high422", "high444", 0 };
        public int zerolatency;//0为低延时

        public int rf_constant;//rf_constant是实际质量，越大图像越花，越小越清晰。 25; 
        public int rf_constant_max;//rf_constant_max ，图像质量的最大值 = 45;

        public X264Params(int width, int height, int fps, int bitrate) {
            this.width = width;
            this.height = height;
            this.fps = fps;
            this.bitrate = bitrate;

            pix = 0;
            method = 2;
            key_frame_max = fps;
            key_frame_min = fps;
            threads = 1;
            level_idc = 30;
            profile = 2;
            zerolatency = 0;
            rf_constant = 25;
            rf_constant_max = 45;



        }
    };
}
