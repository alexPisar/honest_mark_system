using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitesLibrary.Implementations
{
    public class EpsSaveMarkedCodes : ImageSaveMarkedCodes
    {
        private string _inkscapePath;

        public string InkscapeLnkPath { get; set; }

        System.Diagnostics.Process _process;

        public EpsSaveMarkedCodes() : base()
        {

        }

        public override async Task SaveMarkedCodes(string pathFolder, string savedFileName, List<string> fullMarkedCodes)
        {
            if (string.IsNullOrEmpty(InkscapeLnkPath))
                throw new Exception("Не задана папка для исполняемого файла приложения inkscape.");

            _inkscapePath = WindowsShortcutFactory.WindowsShortcut.Load(InkscapeLnkPath)?.Path;
            _process = new System.Diagnostics.Process();
            _process.StartInfo.FileName = _inkscapePath;
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.RedirectStandardOutput = true;
            _process.StartInfo.CreateNoWindow = true;

            await base.SaveMarkedCodes(pathFolder, savedFileName, fullMarkedCodes);
        }

        protected override void ImageProcessSaveInNeedFormat(string savedFilePath, string savingFilePathFolder, string savingFileName, string indxStr)
        {
            if (!string.IsNullOrEmpty(_inkscapePath))
            {
                string epsFilePath = $"{savingFilePathFolder}\\{savingFileName}_{indxStr}.eps";
                string arguments = $"-z \"{savedFilePath}\" --export-type=eps -o \"{epsFilePath}\"";

                _process.StartInfo.Arguments = arguments;
                _process.Start();
                string output = _process.StandardOutput.ReadToEnd();
                _process.WaitForExit();

                if (_process.ExitCode != 0)
                    throw new Exception($"Ошибка при конвертации. Код выхода: {_process.ExitCode}");

                System.IO.File.Delete(savedFilePath);
            }
        }
    }
}
