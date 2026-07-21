using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSystems.Models;

namespace HonestMarkSystem.Interfaces
{
    public interface IEdoDocumentsView
    {
        void SaveParameters(Models.ConsignorOrganization myOrganization, params object[] parameters);

        bool SaveXmlDocument(string edoId, string fileName, params object[] parameters);

        void ChangeMyOrganization();

        void UpdateIdGood();
    }
}
