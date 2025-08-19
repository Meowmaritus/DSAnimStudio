using DSAnimStudio.TaeEditor;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public partial class DSAProj
    {
        public abstract class TaeContainerInfo
        {
            public enum ContainerTypes : int
            {
                Anibnd = 0,
                AnibndInBinder = 1,
            }



            public readonly ContainerTypes ContainerType;

            protected TaeContainerInfo(ContainerTypes containerType)
            {
                ContainerType = containerType;
            }

            public bool IsSameFileAs(TaeContainerInfo other)
            {
                return GetMainBinderName().ToLower() == other.GetMainBinderName().ToLower();
            }

            public abstract string GetMainBinderName();
            public string GetDSAProjFileName()
            {
                return GetMainBinderName() + DSAProj.EXT;
            }

            public string GetDSAProjFileDirectory()
            {
                return System.IO.Path.GetDirectoryName(GetDSAProjFileName());
            }

            protected abstract void InnerBinaryRead(BinaryReaderEx br, string relativeToDir);
            protected abstract void InnerBinaryWrite(BinaryWriterEx bw, string relativeToDir);
            public abstract bool CheckValidity(out string errorMsg, out IBinder mainBinder);
            public abstract string GetRecentFileListDispString();

            public void WriteToBinary(BinaryWriterEx bw, string relativeToDir)
            {
                bw.WriteInt32((int)ContainerType);
                InnerBinaryWrite(bw, relativeToDir);
            }

            public static TaeContainerInfo ReadFromBinary(BinaryReaderEx br, string relativeToDir)
            {
                var containerType = (ContainerTypes)br.ReadInt32();
                if (containerType == ContainerTypes.Anibnd)
                {
                    var containerAnibnd = new ContainerAnibnd();
                    containerAnibnd.InnerBinaryRead(br, relativeToDir);
                    return containerAnibnd;
                }
                else if (containerType == ContainerTypes.AnibndInBinder)
                {
                    var containerAnibndInBinder = new ContainerAnibndInBinder();
                    containerAnibndInBinder.InnerBinaryRead(br, relativeToDir);
                    return containerAnibndInBinder;
                }
                else
                {
                    var allEnumValues = (ContainerTypes[])Enum.GetValues(typeof(ContainerTypes));
                    if (allEnumValues.Contains(containerType))
                        throw new NotImplementedException($"The read function for container type '{containerType}' is not implemented. You should never see this error.");
                    else
                        throw new System.IO.InvalidDataException($"Read container type value of {(int)containerType}, which does not exist (at least as far as this build of the app is aware).");
                }
            }

            public class ContainerAnibnd : TaeContainerInfo
            {
                public ContainerAnibnd()
                    : base(ContainerTypes.Anibnd)
                {

                }

                public ContainerAnibnd(string anibndPath, string chrbndPath)
                    : base(ContainerTypes.Anibnd)
                {
                    AnibndPath = anibndPath;
                    ChrbndPath = chrbndPath;
                }

                public override string GetMainBinderName()
                {
                    return AnibndPath;
                }

                public string AnibndPath;
                public string ChrbndPath;

                public ContainerAnibnd GetClone()
                {
                    var clone = new ContainerAnibnd();
                    clone.AnibndPath = AnibndPath;
                    clone.ChrbndPath = ChrbndPath;
                    return clone;
                }

                protected override void InnerBinaryRead(BinaryReaderEx br, string relativeToDir)
                {
                    AnibndPath = br.ReadUTF16();
                    ChrbndPath = br.ReadNullPrefixUTF16();

                    if (relativeToDir != null)
                    {
                        AnibndPath = Path.GetFullPath(AnibndPath, relativeToDir);
                        if (ChrbndPath != null)
                            ChrbndPath = Path.GetFullPath(ChrbndPath, relativeToDir);
                    }
                }

                protected override void InnerBinaryWrite(BinaryWriterEx bw, string relativeToDir)
                {
                    string anibnd = AnibndPath;
                    string chrbnd = ChrbndPath;

                    if (relativeToDir != null)
                    {
                        anibnd = Path.GetRelativePath(relativeToDir, anibnd);
                        if (chrbnd != null)
                            chrbnd = Path.GetRelativePath(relativeToDir, chrbnd);
                    }

                    bw.WriteUTF16(anibnd, terminate: true);
                    bw.WriteNullPrefixUTF16(chrbnd);
                }

                public override bool CheckValidity(out string errorMsg, out IBinder mainBinder)
                {
                    mainBinder = null;
                    errorMsg = null;

                    if (File.Exists(AnibndPath))
                    {
                        if (Utils.IsReadBinder(AnibndPath, out IBinder binder))
                        {
                            mainBinder = binder;
                            return true;
                        }
                        else
                        {
                            errorMsg = $"ANIBND file '{AnibndPath}' exists but is corrupted/invalid.";
                            return false;
                        }
                        
                    }
                    else
                    {
                        errorMsg = $"ANIBND file '{AnibndPath}' does not exist.";
                        return false;
                    }
                }

                public override string GetRecentFileListDispString()
                {
                    return $"{AnibndPath}";
                }
            }

            public class ContainerAnibndInBinder : TaeContainerInfo
            {
                public ContainerAnibndInBinder()
                    : base(ContainerTypes.AnibndInBinder)
                {

                }

                public ContainerAnibndInBinder(string binderPath, int bindID)
                    : base(ContainerTypes.AnibndInBinder)
                {
                    BinderPath = binderPath;
                    BindID = bindID;
                }

                public ContainerAnibndInBinder GetClone()
                {
                    var clone = new ContainerAnibndInBinder();
                    clone.BinderPath = BinderPath;
                    clone.BindID = BindID;
                    return clone;
                }

                public override string GetMainBinderName()
                {
                    return BinderPath;
                }

                public string BinderPath;
                public int BindID;

                protected override void InnerBinaryRead(BinaryReaderEx br, string relativeToDir)
                {
                    BinderPath = br.ReadUTF16();
                    BindID = br.ReadInt32();

                    if (relativeToDir != null)
                    {
                        BinderPath = Path.GetFullPath(BinderPath, relativeToDir);
                    }
                }

                protected override void InnerBinaryWrite(BinaryWriterEx bw, string relativeToDir)
                {
                    string bnd = BinderPath;

                    if (relativeToDir != null)
                    {
                        bnd = Path.GetRelativePath(relativeToDir, bnd);
                    }

                    bw.WriteUTF16(bnd, true);
                    bw.WriteInt32(BindID);
                }

                public override bool CheckValidity(out string errorMsg, out IBinder mainBinder)
                {
                    mainBinder = null;
                    errorMsg = null;

                    if (File.Exists(BinderPath))
                    {
                        if (Utils.IsReadBinder(BinderPath, out IBinder binder))
                        {
                            if (binder.Files.Any(f => f.ID == BindID))
                            {
                                mainBinder = binder;
                                return true;
                            }
                            else
                            {
                                errorMsg = $"Binder '{BinderPath}' contains no file binded to ID {BindID}";
                                return false;
                            }
                        }
                        else
                        {
                            errorMsg = $"Binder file '{BinderPath}' exists but is corrupted/invalid.";
                            return false;
                        }
                    }
                    else
                    {
                        errorMsg = $"Binder file '{BinderPath}' does not exist.";
                        return false;
                    }
                    


                }

                public override string GetRecentFileListDispString()
                {
                    return $"{BinderPath}:{BindID}";
                }
            }
        }




    }
}
