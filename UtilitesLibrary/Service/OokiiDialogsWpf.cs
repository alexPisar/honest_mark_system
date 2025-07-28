using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ookii.Dialogs.Wpf;

namespace UtilitesLibrary.Service
{
    public class OokiiDialogsWpf : Interfaces.IOpenDialogsWpf
    {
        public override string ChangePathDialog(string description = "Выберите папку")
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Выберите папку";
            dialog.UseDescriptionForTitle = true;

            if (dialog.ShowDialog() == true)
            {
                return dialog.SelectedPath;
            }

            return null;
        }
    }
}
