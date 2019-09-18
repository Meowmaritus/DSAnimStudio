using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.TaeEditor
{
    public static class TaeSoundManager
    {
        public enum SoundType
        {
            a_Environment = 0,
            c_Character = 1,
            f_MenuSE = 2,
            o_Object = 3,
            p_CutsceneSE = 4,
            s_SFX = 5,
            m_BGM = 6,
            v_Voice = 7,
            x_FloorMatDetermined = 8,
            b_ArmorMatDetermined = 9,
            g_Ghost = 10,
        }

        static Dictionary<string, List<SoundEffect>> Sounds = new Dictionary<string, List<SoundEffect>>();

        public static void LoadSoundsFromDir(string dir)
        {
            if (!Directory.Exists(dir))
                return;

            var sounds = Directory.GetFiles(dir, "*.wav", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(dir, "*.mp3", SearchOption.AllDirectories))
                .Concat(Directory.GetFiles(dir, "*.ogg", SearchOption.AllDirectories))
                ;

            foreach (var s in sounds)
            {
                using (var soundStream = File.OpenRead(s))
                {
                    var sound = SoundEffect.FromStream(soundStream);
                    var soundInfo = new FileInfo(s);
                    var shortSoundName = soundInfo.Name.Substring(0, soundInfo.Name.Length - soundInfo.Extension.Length);

                    if (char.IsLetter(shortSoundName.Last()))
                    {
                        shortSoundName = shortSoundName.Substring(0, shortSoundName.Length - 1);
                    }

                    if (!Sounds.ContainsKey(shortSoundName))
                        Sounds.Add(shortSoundName, new List<SoundEffect>());

                    Sounds[shortSoundName].Add(sound);
                }
                
            }
        }

        static Random Rand = new Random();

        public static void DisposeAll()
        {
            foreach (var kvp in Sounds)
            {
                foreach (var s in kvp.Value)
                    s.Dispose();
            }
        }

        public static bool Play(SoundType type, int id, float volume)
        {
            string soundKey = type.ToString().Substring(0, 1) + id.ToString("D9");
            if (Sounds.ContainsKey(soundKey))
            {
                int index = Rand.Next(Sounds[soundKey].Count);
                Sounds[soundKey][index].Play(volume, 0, 0);
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
