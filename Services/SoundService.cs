using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ToDoListApp.Services
{
    public static class SoundService
    {
        [DllImport("winmm.dll", EntryPoint = "PlaySoundW", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool PlaySound(string pszSound, IntPtr hmod, uint fdwSound);

        private const uint SND_ASYNC = 0x0001;
        private const uint SND_FILENAME = 0x00020000;

        public static void PlayTaskComplete()
        {
            try
            {
                string soundPath = Path.Combine(AppContext.BaseDirectory, "Assets", "task_complete.wav");
                if (!File.Exists(soundPath))
                {
                    soundPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "task_complete.wav");
                }
                if (File.Exists(soundPath))
                {
                    PlaySound(soundPath, IntPtr.Zero, SND_ASYNC | SND_FILENAME);
                    return;
                }

                string winChimes = @"C:\Windows\Media\chimes.wav";
                if (File.Exists(winChimes))
                {
                    PlaySound(winChimes, IntPtr.Zero, SND_ASYNC | SND_FILENAME);
                }
            }
            catch { }
        }

        public static void PlayAllTasksComplete()
        {
            try
            {
                string soundPath = Path.Combine(AppContext.BaseDirectory, "Assets", "all_tasks_complete.wav");
                if (!File.Exists(soundPath))
                {
                    soundPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "all_tasks_complete.wav");
                }
                if (File.Exists(soundPath))
                {
                    PlaySound(soundPath, IntPtr.Zero, SND_ASYNC | SND_FILENAME);
                    return;
                }

                string winTada = @"C:\Windows\Media\Alarm03.wav";
                if (File.Exists(winTada))
                {
                    PlaySound(winTada, IntPtr.Zero, SND_ASYNC | SND_FILENAME);
                    return;
                }

                PlayTaskComplete();
            }
            catch { }
        }
    }
}
