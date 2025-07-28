using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitesLibrary.Interfaces
{
    public abstract class IDataMatrixGenerator
    {
        public abstract System.Drawing.Image GenerateDataMatrix(string text, int width = 200, int height = 200);
    }
}
