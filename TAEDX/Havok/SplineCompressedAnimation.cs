using Microsoft.Xna.Framework;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAEDX.Havok
{
    public class SplineCompressedAnimation
    {
        [Flags]
        public enum FlagOffset : byte
        {
            StaticX = 0b00000001,
            StaticY = 0b00000010,
            StaticZ = 0b00000100,
            StaticW = 0b00001000,
            SplineX = 0b00010000,
            SplineY = 0b00100000,
            SplineZ = 0b01000000,
            SplineW = 0b10000000
        };

        public enum ScalarQuantizationType
        {
            BITS8 = 0,
            BITS16 = 1,
        };

        public enum RotationQuantizationType
        {
            POLAR32 = 0, //4 bytes long
            THREECOMP40 = 1, //5 bytes long
            THREECOMP48 = 2, //6 bytes long
            THREECOMP24 = 3, //3 bytes long
            STRAIGHT16 = 4, //2 bytes long
            UNCOMPRESSED = 5, //16 bytes long
        }

        static int GetRotationAlign(RotationQuantizationType qt)
        {
            switch (qt)
            {
                case RotationQuantizationType.POLAR32: return 4;
                case RotationQuantizationType.THREECOMP40: return 1;
                case RotationQuantizationType.THREECOMP48: return 2;
                case RotationQuantizationType.THREECOMP24: return 1;
                case RotationQuantizationType.STRAIGHT16: return 2;
                case RotationQuantizationType.UNCOMPRESSED: return 4;
                default: throw new NotImplementedException();
            }
        }

        static int GetRotationByteCount(RotationQuantizationType qt)
        {
            switch (qt)
            {
                case RotationQuantizationType.POLAR32: return 4;
                case RotationQuantizationType.THREECOMP40: return 5;
                case RotationQuantizationType.THREECOMP48: return 6;
                case RotationQuantizationType.THREECOMP24: return 3;
                case RotationQuantizationType.STRAIGHT16: return 2;
                case RotationQuantizationType.UNCOMPRESSED: return 16;
                default: throw new NotImplementedException();
            }
        }

        static float ReadQuantizedFloat(BinaryReaderEx bin, float min, float max, ScalarQuantizationType type)
        {
            float ratio = -1;
            switch (type)
            {
                case ScalarQuantizationType.BITS8: ratio = bin.ReadByte() / 255.0f; break;
                case ScalarQuantizationType.BITS16: ratio = bin.ReadUInt16() / 65535.0f; break;
                default: throw new NotImplementedException();
            }
            return min + ((max - min) * ratio);
        }

        static Quaternion ReadQuatPOLAR32(BinaryReaderEx br)
        {
            const ulong rMask = (1 << 10) - 1;
            const float rFrac = 1.0f / rMask;
            const float fPI = 3.14159265f;
            const float fPI2 = 0.5f * fPI;
            const float fPI4 = 0.5f * fPI2;
            const float phiFrac = fPI2 / 511.0f;

            uint cVal = br.ReadUInt32();

            /*

            float R = static_cast<float>((cVal >> 18) & rMask) * rFrac;
            R = 1.0f - (R * R);

            float phiTheta = static_cast<float>((cVal & 0x3FFFF));

            float phi = floorf(sqrtf(phiTheta));
            float theta = 0;

            if (phi > 0.0f)
            {
                theta = fPI4 * (phiTheta - (phi * phi)) / phi;
                phi = phiFrac * phi;
            }

            float magnitude = sqrtf(1.0f - R * R);

            Vector4 retVal;
            retVal.X = sinf(phi) * cosf(theta) * magnitude;
            retVal.Y = sinf(phi) * sinf(theta) * magnitude;
            retVal.Z = cosf(phi) * magnitude;
            retVal.W = R;

            if (cVal & 0x10000000)
                retVal.X *= -1;

            if (cVal & 0x20000000)
                retVal.Y *= -1;

            if (cVal & 0x40000000)
                retVal.Z *= -1;

            if (cVal & 0x80000000)
                retVal.W *= -1;

            return retVal;

            */

            throw new NotImplementedException();
        }

        static Quaternion ReadQuatTHREECOMP48(BinaryReaderEx br)
        {
            const ulong mask = (1 << 15) - 1;
            const float fractal = 0.000043161f;

            //SVector cVal = *reinterpret_cast<SVector *>(buffer);

            //What the fuck is an SVector

            /*
	            char resultShift = ((cVal.Y >> 14) & 2) | ((cVal.X >> 15) & 1);
	            bool rSign = (cVal.Z >> 15) != 0;

	            cVal &= mask;
	            cVal -= mask >> 1;

	            Vector tempValF = cVal.Convert<float>() * fractal;

	            Vector4 retval;

	            for (int i = 0; i < 4; i++)
	            {
		            if (i < resultShift)
			            retval[i] = tempValF[i];
		            else if (i > resultShift)
			            retval[i] = tempValF[i - 1];
	            }

	            retval[resultShift] = 1.0f - tempValF.X * tempValF.X - tempValF.Y * tempValF.Y - tempValF.Z * tempValF.Z;

	            if (retval[resultShift] <= 0.0f)
		            retval[resultShift] = 0.0f;
	            else
		            retval[resultShift] = sqrtf(retval[resultShift]);

	            if (rSign)
		            retval[resultShift] *= -1;

	            buffer += 6;
	            return retval;
            */

            throw new NotImplementedException();
        }

        static Quaternion ReadQuatTHREECOMP40(BinaryReaderEx br)
        {
            const ulong mask = (1 << 12) - 1;
            const ulong positiveMask = mask >> 1;
            const float fractal = 0.000345436f;
            ulong cVal = br.ReadUInt64();

            /*
	            IVector tempVal;
	            tempVal.X = cVal & mask;
	            tempVal.Y = (cVal >> 12) & mask;
	            tempVal.Z = (cVal >> 24) & mask;

	            int resultShift = (cVal >> 36) & 3;

	            tempVal -= positiveMask;

	            Vector tempValF = tempVal.Convert<float>() * fractal;

	            Vector4 retval;

	            for (int i = 0; i < 4; i++)
	            {
		            if (i < resultShift)
			            retval[i] = tempValF[i];
		            else if (i > resultShift)
			            retval[i] = tempValF[i - 1];
	            }

	            retval[resultShift] = 1.0f - tempValF.X * tempValF.X - tempValF.Y * tempValF.Y - tempValF.Z * tempValF.Z;

	            if (retval[resultShift] <= 0.0f)
		            retval[resultShift] = 0.0f;
	            else
		            retval[resultShift] = sqrtf(retval[resultShift]);

	            if ((cVal >> 38) & 1)
		            retval[resultShift] *= -1;
	            return retval;
            */

            throw new NotImplementedException();
        }

        static Quaternion ReadQuantizedQuaternion(BinaryReaderEx br, RotationQuantizationType type)
        {
            switch (type)
            {
                case RotationQuantizationType.POLAR32:
                    return ReadQuatPOLAR32(br);
                case RotationQuantizationType.THREECOMP40:
                    return ReadQuatTHREECOMP40(br);
                case RotationQuantizationType.THREECOMP48:
                    return ReadQuatTHREECOMP48(br);
                case RotationQuantizationType.THREECOMP24:
                case RotationQuantizationType.STRAIGHT16:
                    throw new NotImplementedException();
                case RotationQuantizationType.UNCOMPRESSED:
                    return new Quaternion(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                default:
                    return Quaternion.Identity;
            }
        }

        public class SplineChannel<T>
        {
            public bool IsDynamic = true;
            public List<T> Values = new List<T>();
        }

        public class SplineTrackQuaternion
        {
            public SplineChannel<Quaternion> Channel;
            public List<byte> Knots = new List<byte>();
            public byte Degree;

            internal SplineTrackQuaternion(BinaryReaderEx br, RotationQuantizationType quantizationType)
            {
                long debug_StartOfThisSplineTrack = br.Position;

                short numItems = br.ReadInt16();
                Degree = br.ReadByte();
                int knotCount = numItems + Degree + 2;
                for (int i = 0; i < knotCount; i++)
                {
                    Knots.Add(br.ReadByte());
                }

                br.Pad(GetRotationAlign(quantizationType));

                Channel = new SplineChannel<Quaternion>();

                for (int i = 0; i <= numItems; i++)
                {
                    Channel.Values.Add(ReadQuantizedQuaternion(br, quantizationType));
                }
            }
        }

        public class SplineTrackVector3
        {
            public SplineChannel<float> ChannelX;
            public SplineChannel<float> ChannelY;
            public SplineChannel<float> ChannelZ;
            public List<byte> Knots = new List<byte>();
            public byte Degree;

            internal SplineTrackVector3(BinaryReaderEx br, List<FlagOffset> channelTypes, ScalarQuantizationType quantizationType, bool isPosition)
            {
                long debug_StartOfThisSplineTrack = br.Position;

                short numItems = br.ReadInt16();
                Degree = br.ReadByte();
                int knotCount = numItems + Degree + 2;
                for (int i = 0; i < knotCount; i++)
                {
                    Knots.Add(br.ReadByte());
                }

                br.Pad(4);

                float BoundsXMin = 0;
                float BoundsXMax = 0;
                float BoundsYMin = 0;
                float BoundsYMax = 0;
                float BoundsZMin = 0;
                float BoundsZMax = 0;

                ChannelX = new SplineChannel<float>();
                ChannelY = new SplineChannel<float>();
                ChannelZ = new SplineChannel<float>();

                if (channelTypes.Contains(FlagOffset.SplineX))
                {
                    BoundsXMin = br.ReadSingle();
                    BoundsXMax = br.ReadSingle();
                }
                else if (channelTypes.Contains(FlagOffset.StaticX))
                {
                    ChannelX.Values = new List<float> { br.ReadSingle() };
                    ChannelX.IsDynamic = false;
                }
                else
                {
                    ChannelX = null;
                }

                if (channelTypes.Contains(FlagOffset.SplineY))
                {
                    BoundsYMin = br.ReadSingle();
                    BoundsYMax = br.ReadSingle();
                }
                else if (channelTypes.Contains(FlagOffset.StaticY))
                {
                    ChannelY.Values = new List<float> { br.ReadSingle() };
                    ChannelY.IsDynamic = false;
                }
                else
                {
                    ChannelY = null;
                }

                if (channelTypes.Contains(FlagOffset.SplineZ))
                {
                    BoundsZMin = br.ReadSingle();
                    BoundsZMax = br.ReadSingle();
                }
                else if (channelTypes.Contains(FlagOffset.StaticZ))
                {
                    ChannelZ.Values = new List<float> { br.ReadSingle() };
                    ChannelZ.IsDynamic = false;
                }
                else
                {
                    ChannelZ = null;
                }

                for (int i = 0; i <= numItems; i++)
                {
                    if (channelTypes.Contains(FlagOffset.SplineX))
                    {
                        ChannelX.Values.Add(ReadQuantizedFloat(br, BoundsXMin, BoundsXMax, quantizationType));
                    }

                    if (channelTypes.Contains(FlagOffset.SplineY))
                    {
                        ChannelY.Values.Add(ReadQuantizedFloat(br, BoundsYMin, BoundsYMax, quantizationType));
                    }

                    if (channelTypes.Contains(FlagOffset.SplineZ))
                    {
                        ChannelZ.Values.Add(ReadQuantizedFloat(br, BoundsZMin, BoundsZMax, quantizationType));
                    }
                }
            }
        }

        public class TransformMask
        {
            public ScalarQuantizationType PositionQuantizationType;
            public RotationQuantizationType RotationQuantizationType;
            public ScalarQuantizationType ScaleQuantizationType;
            public List<FlagOffset> PositionTypes;
            public List<FlagOffset> RotationTypes;
            public List<FlagOffset> ScaleTypes;

            internal TransformMask(BinaryReaderEx br)
            {
                PositionTypes = new List<FlagOffset>();
                RotationTypes = new List<FlagOffset>();
                ScaleTypes = new List<FlagOffset>();

                var byteQuantizationTypes = br.ReadByte();
                var bytePositionTypes = (FlagOffset)br.ReadByte();
                var byteRotationTypes = (FlagOffset)br.ReadByte();
                var byteScaleTypes = (FlagOffset)br.ReadByte();

                PositionQuantizationType = (ScalarQuantizationType)(byteQuantizationTypes & 3);
                RotationQuantizationType = (RotationQuantizationType)((byteQuantizationTypes >> 2) & 0xF);
                ScaleQuantizationType = (ScalarQuantizationType)((byteQuantizationTypes >> 6) & 3);

                foreach (var flagOffset in (FlagOffset[])Enum.GetValues(typeof(FlagOffset)))
                {
                    if (bytePositionTypes.HasFlag(flagOffset))
                        PositionTypes.Add(flagOffset);

                    if (byteRotationTypes.HasFlag(flagOffset))
                        RotationTypes.Add(flagOffset);

                    if (byteScaleTypes.HasFlag(flagOffset))
                        ScaleTypes.Add(flagOffset);
                }
            }
        }

        public class TransformTrack
        {
            public int Unk1;
            public int Unk2;
            public int Unk3;
            public int Unk4;

            public TransformMask Mask;

            public bool HasSplinePosition;
            public bool HasSplineRotation;
            public bool HasSplineScale;

            public bool HasStaticRotation;

            public Vector3 StaticPosition = Vector3.Zero;

            //public Vector4 StaticRotation;
            //TEMP; DONT FEEL LIKE DOING QUATERNION READ
            public byte[] StaticRotation;
            public Vector3 StaticScale = Vector3.One;
            public SplineTrackVector3 SplinePosition = null;
            public SplineTrackQuaternion SplineRotation = null;
            public SplineTrackVector3 SplineScale = null;
        }

        public static TransformTrack[] ReadSplineCompressedAnimByteBlock(
            bool isBigEndian, byte[] animationData, int numTransformTracks, int numBlocks)
        {
            var TransformTracks = new TransformTrack[numTransformTracks];

            for (int i = 0; i < numTransformTracks; i++)
            {
                TransformTracks[i] = new TransformTrack();
            }

            var br = new BinaryReaderEx(isBigEndian, animationData);

            for (int blockIndex = 0; blockIndex < numBlocks; blockIndex++)
            {
                for (int i = 0; i < numTransformTracks; i++)
                {
                    TransformTracks[i].Mask = new TransformMask(br);
                }

                br.Pad(4);

                for (int i = 0; i < numTransformTracks; i++)
                {
                    var m = TransformTracks[i].Mask;
                    var track = TransformTracks[i];

                    track.HasSplinePosition = m.PositionTypes.Contains(FlagOffset.SplineX)
                        || m.PositionTypes.Contains(FlagOffset.SplineY)
                        || m.PositionTypes.Contains(FlagOffset.SplineZ);

                    track.HasSplineRotation = m.RotationTypes.Contains(FlagOffset.SplineX)
                        || m.RotationTypes.Contains(FlagOffset.SplineY)
                        || m.RotationTypes.Contains(FlagOffset.SplineZ)
                        || m.RotationTypes.Contains(FlagOffset.SplineW);

                    track.HasStaticRotation = m.RotationTypes.Contains(FlagOffset.StaticX)
                        || m.RotationTypes.Contains(FlagOffset.StaticY)
                        || m.RotationTypes.Contains(FlagOffset.StaticZ)
                        || m.RotationTypes.Contains(FlagOffset.StaticW);

                    track.HasSplineScale = m.ScaleTypes.Contains(FlagOffset.SplineX)
                        || m.ScaleTypes.Contains(FlagOffset.SplineY)
                        || m.ScaleTypes.Contains(FlagOffset.SplineZ);

                    if (track.HasSplinePosition)
                    {
                        track.SplinePosition = new SplineTrackVector3(br, m.PositionTypes, m.PositionQuantizationType, isPosition: true);
                    }
                    else
                    {
                        if (m.PositionTypes.Contains(FlagOffset.StaticX))
                        {
                            track.StaticPosition.X = br.ReadSingle();
                        }

                        if (m.PositionTypes.Contains(FlagOffset.StaticY))
                        {
                            track.StaticPosition.Y = br.ReadSingle();
                        }

                        if (m.PositionTypes.Contains(FlagOffset.StaticZ))
                        {
                            track.StaticPosition.Z = br.ReadSingle();
                        }
                    }

                    br.Pad(4);



                    if (track.HasSplineRotation)
                    {
                        track.SplineRotation = new SplineTrackQuaternion(br, m.RotationQuantizationType);
                    }
                    else
                    {
                        if (track.HasStaticRotation)
                        {
                            br.Pad(GetRotationAlign(m.RotationQuantizationType));
                            track.StaticRotation = br.ReadBytes(GetRotationByteCount(m.RotationQuantizationType));
                        }
                    }

                    br.Pad(4);

                    if (track.HasSplineScale)
                    {
                        track.SplineScale = new SplineTrackVector3(br, m.ScaleTypes, m.ScaleQuantizationType, isPosition: false);
                    }
                    else
                    {


                        if (m.ScaleTypes.Contains(FlagOffset.StaticX))
                        {
                            track.StaticScale.X = br.ReadSingle();
                        }

                        if (m.ScaleTypes.Contains(FlagOffset.StaticY))
                        {
                            track.StaticScale.Y = br.ReadSingle();
                        }

                        if (m.ScaleTypes.Contains(FlagOffset.StaticZ))
                        {
                            track.StaticScale.Z = br.ReadSingle();
                        }
                    }



                    br.Pad(4);
                }

                br.Pad(16);
            }

            return TransformTracks;
        }
    }
}
