using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic.SpellCheck
{
    public class LinuxHunspell: Hunspell
    {
        [DllImport ("libhunspell")]
        private static extern IntPtr Hunspell_create(string affpath, string dpath);

        [DllImport ("libhunspell")]
        private static extern IntPtr Hunspell_destroy(IntPtr hunspellHandle);

        [DllImport ("libhunspell")]
        private static extern int Hunspell_spell(IntPtr hunspellHandle, string word);

        [DllImport ("libhunspell")]
        private static extern int Hunspell_suggest(IntPtr hunspellHandle, IntPtr slst, string word);

        [DllImport ("libhunspell")]
        private static extern void Hunspell_free_list(IntPtr hunspellHandle, IntPtr slst, int n);

        private IntPtr _hunspellHandle = IntPtr.Zero;

        public LinuxHunspell(string affDirectory, string dicDictory)
        {
            //Also search - /usr/share/hunspell
            try
            {
                _hunspellHandle = Hunspell_create(affDirectory, dicDictory);
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Unable to start hunspell spell checker - make sure hunspell is installed!");
                throw;
            }
        }

        public override bool Spell(string word)
        {
            return Hunspell_spell(_hunspellHandle, word) != 0;
        }

        public override List<string> Suggest(string word)
        {
            IntPtr pointerToAddressStringArray = Marshal.AllocHGlobal(IntPtr.Size);
            int resultCount = Hunspell_suggest(_hunspellHandle, pointerToAddressStringArray, word);
            IntPtr addressStringArray = Marshal.ReadIntPtr(pointerToAddressStringArray);
            List<string> results = new List<string>();
            for (int i = 0; i < resultCount; i++)
            {
                IntPtr addressCharArray = Marshal.ReadIntPtr(addressStringArray, i * IntPtr.Size);
                string suggestion = Marshal.PtrToStringAuto(addressCharArray);
                if (!string.IsNullOrEmpty(suggestion))
                    results.Add(suggestion);
            }
            Hunspell_free_list(_hunspellHandle, pointerToAddressStringArray, resultCount);
            Marshal.FreeHGlobal(pointerToAddressStringArray);

            return results;
        }

        ~ LinuxHunspell()
        {
            if (_hunspellHandle != IntPtr.Zero)
                Hunspell_destroy(_hunspellHandle);
        }
    }
}